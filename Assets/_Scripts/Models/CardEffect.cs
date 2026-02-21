using UnityEngine;

/// <summary>
/// CardEffect คือผลลัพธ์ของ skill ที่ trigger
/// ส่วนใหญ่จะเป็นการเพิ่ม Chips หรือ Mult
/// </summary>
[System.Serializable]
public abstract class CardEffect {
    
    /// <summary>
    /// คืน bonus chips และ mult ที่ effect นี้ให้
    /// </summary>
    public abstract (int chipsBonus, int multBonus) GetBonus();

    /// <summary>
    /// คืนคำอธิบาย effect เป็นภาษาที่อ่านง่าย
    /// </summary>
    public abstract string GetDescription();
}