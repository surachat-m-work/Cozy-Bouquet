using System.Collections.Generic;
using SerializeReferenceEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Card/Card Data")]
public class CardData : ScriptableObject
{
    public string CardName;
    public int scoreValue;
    public Sprite cardSprite;
    public FlowerColor Color;
    public FlowerType type;

    [SerializeReference, SR] public List<Effect> effects = new List<Effect>();

    [Header("Special Ability")]
    public AbilityType ability; // ประเภทสกิล
    public SlotType preferredSlot; // ช่องที่ชอบ (Fertile/Edge/Shade)
    public float bonusMultiplier = 1.0f; // ตัวคูณโบนัส
    public int bonusChips = 0; // แต้มบวกเพิ่ม
}

public enum AbilityType { None, ScoreBoostInSlot, MultBoostInSlot, ShadeResilience }