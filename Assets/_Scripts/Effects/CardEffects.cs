// using System;
// using UnityEngine;

// [Serializable]
// public class AddChipsEffect : Effect {
//     public int amount;
//     public SlotType preferredSlot;

//     public override GameAction GetGameAction(CardView card) {
//         bool conditionMet = false;
//         foreach (var slot in card.GetOccupiedSlots()) {
//             if (slot.type == preferredSlot) conditionMet = true;
//         }

//         if (conditionMet) {
//             // สร้าง Action กลับไปเฉยๆ ยังไม่สั่ง Perform ทันที
//             return new AddScoreGA(amount);
//         }

//         return null; // ถ้าเงื่อนไขไม่ครบก็ไม่ต้องทำอะไร
//     }
// }

// [Serializable]
// public class AddChipsOnSlotEffect : Effect {
//     [SerializeField] private SlotType _requiredSlot;
//     [SerializeField] private int _bonusAmount;

//     public override GameAction GetGameAction(CardView card) {
//         // เช็คว่ามีช่องที่ทับอยู่ตรงกับเงื่อนไขไหม
//         foreach (var slot in card.GetOccupiedSlots()) {
//             if (slot.Type == _requiredSlot)
//                 return new AddChipsGA(_bonusAmount);
//         }
//         return null;
//     }
// }

// [Serializable]
// public class MultiplyOnSlotEffect : Effect {
//     [SerializeField] private SlotType _requiredSlot;
//     [SerializeField] private float _multiplier;

//     public override GameAction GetGameAction(CardView card) {
//         foreach (var slot in card.GetOccupiedSlots()) {
//             if (slot.Type == _requiredSlot)
//                 return new AddMultiplierGA(_multiplier);
//         }
//         return null;
//     }
// }

// [Serializable]
// public class ShadeProtectionEffect : Effect {
//     [SerializeField] private int _bonusChips;

//     public override GameAction GetGameAction(CardView card) {
//         foreach (var slot in card.GetOccupiedSlots()) {
//             if (slot.Type == SlotType.Shade) {
//                 // ส่ง Action พิเศษหรือแค่บวกแต้มก็ได้ แต่ในที่นี้เราเน้นบวก Chips
//                 return new AddChipsGA(_bonusChips);
//             }
//         }
//         return null;
//     }
// }

// [Serializable]
// public class DirectionalBonusEffect : Effect {
//     [SerializeField] private bool _requireHorizontal;
//     [SerializeField] private int _bonusChips;

//     public override GameAction GetGameAction(CardView card) {
//         if (card.IsHorizontal == _requireHorizontal)
//             return new AddChipsGA(_bonusChips);

//         return null;
//     }
// }

// [Serializable]
// public class NeighborBuffEffect : Effect {
//     [SerializeField] private int _chipsPerNeighbor;

//     public override GameAction GetGameAction(CardView card) {
//         // สมมติว่า DraggableCard มี Method สำหรับหาเพื่อนบ้านใน Cluster
//         int neighborCount = card.GetNeighbors().Count;
//         if (neighborCount > 0)
//             return new AddChipsGA(_chipsPerNeighbor * neighborCount);

//         return null;
//     }
// }

// [Serializable]
// public class ColorSynergyEffect : Effect
// {
//     [SerializeField] private FlowerColor _targetColor;
//     [SerializeField] private float _multAmount;

//     public override GameAction GetGameAction(CardView card)
//     {
//         // เรียกใช้ GridManager เพื่อหาเพื่อนบ้าน
//         var neighbors = GridSystem.Instance.GetNeighborsOfCard(card);
        
//         // เช็คว่ามีเพื่อนบ้านสีที่ต้องการไหม
//         foreach (var neighbor in neighbors)
//         {
//             if (neighbor.CardData.Color == _targetColor)
//             {
//                 return new AddMultiplierGA(_multAmount);
//             }
//         }
        
//         return null;
//     }
// }