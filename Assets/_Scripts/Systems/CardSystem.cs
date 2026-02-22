using System.Collections;
using System.Collections.Generic;

public class CardSystem : Singleton<CardSystem> {

    private void OnEnable() {
        ActionSystem.AttachPerformer<PlayCardGA>(PlayCardPerformer);
    }

    private void OnDisable() {
        ActionSystem.DetachPerformer<PlayCardGA>();
    }

    private IEnumerator PlayCardPerformer(PlayCardGA action) {
        // // action.Card.PlaceInSlots(action.Slots);

        // // 2. รัน Effect ของตัวมันเอง (Sunflower, Ivy, etc.)
        // foreach (var effect in action.Card.CardData.effects) {
        //     ActionSystem.Instance.AddReaction(new PerformEffectGA(effect, action.Card));
        // }

        // // 3. [สำคัญ] สั่งเช็คคอมโบทั้ง Grid หลังจากวางและรัน Effect เสร็จ
        // ActionSystem.Instance.AddReaction(new CheckComboGA());

        yield return null;
    }
}