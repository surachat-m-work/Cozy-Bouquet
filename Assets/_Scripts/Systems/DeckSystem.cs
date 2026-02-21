using System.Collections.Generic;
using UnityEngine;

public class DeckSystem : Singleton<DeckSystem> {
    public List<CardData> masterDeck;    // กองการ์ดทั้งหมดที่มี
    private List<CardData> drawPile;    // กองการ์ดที่เหลือให้จั่ว
    public List<CardView> cardsInHand = new List<CardView>(); // การ์ดที่อยู่บนมือ (Visual)

    public Transform handParent;        // UI Panel ที่เก็บการ์ดในมือ
    public GameObject cardPrefab;       // Prefab ของการ์ด

    public int handSize = 5;            // จำนวนการ์ดบนมือสูงสุด

    void Start() {
        // เริ่มเกม: ก็อปปี้การ์ดจาก Master ลงกองจั่วแล้วสลับ
        drawPile = new List<CardData>(masterDeck);
        Shuffle(drawPile);
        RefillHand();
    }

    public void RefillHand() {
        int cardsNeeded = handSize - cardsInHand.Count;
        for (int i = 0; i < cardsNeeded; i++) {
            if (drawPile.Count > 0) DrawCard();
        }
    }

    public void DrawCard() {
        CardData data = drawPile[0];
        drawPile.RemoveAt(0);

        // สร้าง Object การ์ดขึ้นมาใน UI
        GameObject newCardObj = Instantiate(cardPrefab, handParent);
        CardView cardScript = newCardObj.GetComponent<CardView>();
        cardScript.Setup(data); // อย่าลืมสร้างฟังก์ชัน Setup ใน DraggableCard เพื่อรับข้อมูล CardData

        cardsInHand.Add(cardScript);
    }

    private void Shuffle(List<CardData> list) {
        for (int i = 0; i < list.Count; i++) {
            CardData temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    // เรียกใช้ตอนกด Confirm
    public void RemoveUsedCards(List<CardView> usedCards) {
        foreach (var card in usedCards) {
            cardsInHand.Remove(card);
            Destroy(card.gameObject); // ลบออกจากเกมหลังจากใช้แล้ว
        }
        RefillHand(); // จั่วใหม่ให้เต็ม
    }

    // public CardData DrawCard() {
    //     return new CardData();
    // }
}
