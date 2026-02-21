using SerializeReferenceEditor;
using UnityEngine;

/// <summary>
/// Card Skill คือความสามารถพิเศษของการ์ดแต่ละใบ
/// ประกอบด้วย Condition (เงื่อนไข) และ Effect (ผลลัพธ์)
/// ใช้ SerializeReference เพื่อให้สร้างได้หลายแบบใน Inspector
/// </summary>
[System.Serializable]
public class CardSkill {

    [SerializeReference, SR]
    public SlotCondition Condition;

    [SerializeReference, SR]
    public CardEffect Effect;

    /// <summary>
    /// คืนคำอธิบายของ skill นี้เป็นภาษาที่อ่านง่าย
    /// </summary>
    public string GetDescription() {
        if (Condition == null || Effect == null)
            return "Invalid skill";

        return $"{Condition.GetDescription()}: {Effect.GetDescription()}";
    }
}
