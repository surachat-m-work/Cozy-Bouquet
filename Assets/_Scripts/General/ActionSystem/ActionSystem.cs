using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionSystem : Singleton<ActionSystem> {

    private List<GameAction> m_Reactions = null;
    public bool IsPerforming { get; private set; } = false;

    private static Dictionary<Type, List<Action<GameAction>>> m_PreSubs = new();
    private static Dictionary<Type, List<Action<GameAction>>> m_PostSubs = new();
    private static Dictionary<Type, Func<GameAction, IEnumerator>> m_Performers = new();

    public void Perform(GameAction action, System.Action OnPreformFinished = null) {
        if (IsPerforming) return;

        IsPerforming = true;
        StartCoroutine(Flow(action, () => {
            IsPerforming = false;
            OnPreformFinished?.Invoke();
        }));
    }

    public void AddReaction(GameAction gameAction) {
        m_Reactions?.Add(gameAction);
    }

    private IEnumerator Flow(GameAction action, Action OnFlowFinished = null) {
        m_Reactions = action.PreReactions;
        PerformSubscibers(action, m_PreSubs);
        yield return PerformReactions();

        m_Reactions = action.PerformReactions;
        yield return PerformPerformer(action);
        yield return PerformReactions();

        m_Reactions = action.PostReactions;
        PerformSubscibers(action, m_PostSubs);
        yield return PerformReactions();

        OnFlowFinished?.Invoke();
    }

    private IEnumerator PerformPerformer(GameAction action) {
        Type type = action.GetType();
        if (m_Performers.ContainsKey(type)) {
            yield return m_Performers[type](action);
        }
    }

    private IEnumerator PerformReactions() {
        foreach (var reaction in m_Reactions) {
            yield return Flow(reaction);
        }
    }

    private void PerformSubscibers(GameAction action, Dictionary<Type, List<Action<GameAction>>> subs) {
        Type type = action.GetType();
        if (subs.ContainsKey(type)) {
            foreach (var sub in subs[type]) {
                sub(action);
            }
        }
    }

    public static void AttachPerformer<T>(Func<T, IEnumerator> performer) where T : GameAction {
        Type type = typeof(T);
        IEnumerator wrappedPerformer(GameAction action) => performer((T)action);
        if (m_Performers.ContainsKey(type)) m_Performers[type] = wrappedPerformer;
        else m_Performers.Add(type, wrappedPerformer);
    }

    public static void DetachPerformer<T>() where T : GameAction {
        Type type = typeof(T);
        if (m_Performers.ContainsKey(type)) m_Performers.Remove(type);
    }

    public static void SubscribeReaction<T>(Action<T> reaction, ReactionTiming timing) where T : GameAction {
        Dictionary<Type, List<Action<GameAction>>> subs = timing == ReactionTiming.PRE ? m_PreSubs : m_PostSubs;
        void wrappedReaction(GameAction action) => reaction((T)action);
        if (subs.ContainsKey(typeof(T))) {
            subs[typeof(T)].Add(wrappedReaction);
        } else {
            subs.Add(typeof(T), new());
            subs[typeof(T)].Add(wrappedReaction);
        }
    }

    public static void UnsubscribeReaction<T>(Action<T> reaction, ReactionTiming timing) where T : GameAction {
        Dictionary<Type, List<Action<GameAction>>> subs = timing == ReactionTiming.PRE ? m_PreSubs : m_PostSubs;
        if (subs.ContainsKey(typeof(T))) {
            void wrappedReaction(GameAction action) => reaction((T)action);
            subs[typeof(T)].Remove(wrappedReaction);
        }
    }
}
