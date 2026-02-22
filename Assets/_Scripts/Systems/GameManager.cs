using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : Singleton<GameManager> {
    private List<Slot> allSlots = new List<Slot>();

    public List<Slot> AllSlots => allSlots;

    void Start() {
        // หา Slot ทั้งหมดใน Grid
        allSlots.AddRange(FindObjectsByType<Slot>(FindObjectsSortMode.None));
    }

    // ฟังก์ชันสำหรับปุ่ม "Confirm" (Play Hand)
    public void OnConfirmButtonClick() {
        // 1. รวบรวมการ์ดทั้งหมดบน Grid (ไม่นับซ้ำ)
        List<CardView> cardsOnGrid = new List<CardView>();
        foreach (Slot slot in allSlots) {
            if (slot.isOccupied && slot.OccupiedCard != null) {
                if (!cardsOnGrid.Contains(slot.OccupiedCard)) {
                    cardsOnGrid.Add(slot.OccupiedCard);
                }
            }
        }

        // เรียกคำนวณคะแนนรวมแบบแยกกลุ่มก้อน
        int finalScore = ComboSystem.Instance.CalculateTotalBoardScore();

        Debug.Log($"คะแนนรวมทั้งหมดในรอบนี้: {finalScore}");
        ScoreSystem.Instance.AddScore(finalScore);

        // 5. เคลียร์กระดานและจั่วใหม่ (EndTurn)
        EndTurn(cardsOnGrid);
    }

    void EndTurn(List<CardView> cardsToDelete) {
        foreach (var card in cardsToDelete) {
            Destroy(card.gameObject);
        }

        // เคลียร์ Slot ทั้งหมด
        foreach (Slot slot in allSlots) slot.ClearSlot();

        // จั่วการ์ดใหม่ให้ครบ 8
        DeckSystem.Instance.RefillHand();
    }
}
