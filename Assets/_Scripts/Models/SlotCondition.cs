using UnityEngine;

/// <summary>
/// SlotCondition เป็นเงื่อนไขในการ trigger skill ของการ์ด
/// เช็คว่าการ์ดวางลง slot ประเภทไหน ครบเงื่อนไขหรือไม่
/// </summary>
[System.Serializable]
public abstract class SlotCondition {
    
    /// <summary>
    /// เช็คว่าเงื่อนไขนี้ถูกต้องหรือไม่
    /// โดยรับ slot type 2 ช่องที่การ์ดครอบอยู่
    /// </summary>
    public abstract bool IsMet(SlotType slotA, SlotType slotB);

    /// <summary>
    /// คืนคำอธิบายเงื่อนไขเป็นภาษาที่อ่านง่าย
    /// </summary>
    public abstract string GetDescription();
}