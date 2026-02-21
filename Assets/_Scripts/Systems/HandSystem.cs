using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandSystem : Singleton<HandSystem> {
    // [SerializeField] private HandCurveData _handCurve;
    // [SerializeField] private GameObject _cardPrefab;
    // [SerializeField] private Transform _handContainer;
    // private List<CardView> _cardsInHand = new();

    // void OnEnable() {
    //     ActionSystem.AttachPerformer<DrawCardGA>(DrawCardPerformer);
    // }

    // void OnDisable() {
    //     ActionSystem.DetachPerformer<DrawCardGA>();
    // }

    // private IEnumerator DrawCardPerformer(DrawCardGA action) {
    //     for (int i = 0; i < action.Amount; i++) {
    //         CardData data = DeckSystem.Instance.DrawCard();
    //         if (data == null) break;

    //         GameObject obj = Instantiate(_cardPrefab, _handContainer);
    //         CardView newCard = obj.GetComponent<CardView>();
    //         newCard.Setup(data);
    //         _cardsInHand.Add(newCard);

    //         // รอให้ Animation จั่วเสร็จ
    //         yield return newCard.AnimateToHand(_handCurve, _cardsInHand.Count, action.Amount);
    //     }
    // }
}
