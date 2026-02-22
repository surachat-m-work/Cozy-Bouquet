using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandSystem : Singleton<HandSystem> {
    [SerializeField] private HandCurveData _handCurve;
    [SerializeField] private GameObject _cardPrefab;
    [SerializeField] private Transform _handContainer;
    private List<CardView> _cardsInHand = new();

    void OnEnable() {
        // ActionSystem.AttachPerformer<DrawCardGA>(DrawCardPerformer);
    }

    void OnDisable() {
        // ActionSystem.DetachPerformer<DrawCardGA>();
    }

    // private IEnumerator DrawCardPerformer(DrawCardGA action) {
    //     // 1. ดึงข้อมูลจากการ์ดใน DeckSystem
    //     CardData data = DeckSystem.Instance.Draw();

    //     // 2. สร้าง Visual การ์ด (โค้ดเดิมของคุณใน HandSystem)
    //     CardView newCard = Instantiate(_cardPrefab, ...).GetComponent<CardView>();
    //     newCard.SetData(data);

    //     // 3. รอให้ Animation การจั่ว (Curve/DOTween) ทำงานจนเสร็จ
    //     // เพื่อให้ระบบ Action-Reaction รู้ว่า "จั่วเสร็จจริงๆ แล้วนะ"
    //     yield return new WaitForSeconds(0.5f);
    // }
}
