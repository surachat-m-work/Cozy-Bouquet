using System;
using UnityEngine;

[Serializable]
public abstract class Effect {
    // ฟังก์ชันหลักที่ต้องมี: เปลี่ยนข้อมูลใน Inspector ให้กลายเป็น Action
    public abstract GameAction GetGameAction(CardView card);
}