using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ComboSystem : Singleton<ComboSystem> {
    // โครงสร้างสำหรับเก็บผลลัพธ์ Combo
    public struct ComboResult {
        public string comboName;
        public int baseChips;
        public float baseMult;
    }

    // public List<Slot> allSlots;

    private void OnEnable() {
        ActionSystem.AttachPerformer<CheckComboGA>(CheckComboPerformer);
    }

    private void OnDisable() {
        ActionSystem.DetachPerformer<CheckComboGA>();
    }

    private IEnumerator CheckComboPerformer(CheckComboGA action) {
        // 1. หาการ์ดทั้งหมดที่มีอยู่บน Grid ในขณะนั้น
        List<CardView> allCardsOnGrid = GetAllCardsOnGrid();

        // 2. แยกกลุ่มการ์ด (Clusters)
        List<List<CardView>> clusters = FindClusters(allCardsOnGrid);

        // 3. วิเคราะห์แต่ละกลุ่มเพื่อหาคอมโบ
        foreach (var cluster in clusters) {
            if (cluster.Count >= 2) // คอมโบต้องมีการ์ด 2 ใบขึ้นไป (หรือตามกฎของคุณ)
            {
                EvaluateClusterCombo(cluster);
            }
        }

        yield return null;
    }

    private List<CardView> GetAllCardsOnGrid() {
        HashSet<CardView> cards = new HashSet<CardView>();
        foreach (var slot in GridSystem.Instance.slotMap.Values) {
            if (slot.OccupiedCard != null)
                cards.Add(slot.OccupiedCard);
        }
        return cards.ToList();
    }

    // --- Algorithm: หา Cluster (เชื่อมโยงกัน) ---
    private List<List<CardView>> FindClusters(List<CardView> allCards) {
        List<List<CardView>> clusters = new List<List<CardView>>();
        HashSet<CardView> visited = new HashSet<CardView>();

        foreach (var card in allCards) {
            if (!visited.Contains(card)) {
                List<CardView> newCluster = new List<CardView>();
                FloodFill(card, visited, newCluster);
                clusters.Add(newCluster);
            }
        }
        return clusters;
    }

    private void FloodFill(CardView card, HashSet<CardView> visited, List<CardView> cluster) {
        visited.Add(card);
        cluster.Add(card);

        // หาเพื่อนบ้าน (ต้องเช็คจาก Slot รอบๆ)
        foreach (var neighbor in GetNeighborCards(card)) {
            if (!visited.Contains(neighbor)) {
                FloodFill(neighbor, visited, cluster);
            }
        }
    }

    private void EvaluateClusterCombo(List<CardView> cluster) {
        int cardCount = cluster.Count;

        // เช็คเงื่อนไขตามลำดับความยาก (แบบเดียวกับที่เราออกแบบไว้)
        if (IsColorFlush(cluster)) {
            ActionSystem.Instance.AddReaction(new AddChipsGA(80));
            ActionSystem.Instance.AddReaction(new AddMultiplierGA(4.0f));
            Debug.Log("Combo: Color Flush!");
        }
    }

    // ตัวอย่าง Helper Method สำหรับเช็คสีเดียวกันหมด
    private bool IsColorFlush(List<CardView> cluster) {
        FlowerColor firstColor = cluster[0].CardData.Color;
        return cluster.All(c => c.CardData.Color == firstColor);
    }

    // --- ฟังก์ชันหลัก: คำนวณคะแนนรวมทุกกลุ่มก้อน ---
    public int CalculateTotalBoardScore() {
        List<CardView> allCards = GetCardsFromGrid();
        List<List<CardView>> clusters = FindClusters(allCards);

        int totalScore = 0;

        foreach (var cluster in clusters) {
            totalScore += CalculateClusterScore(cluster);
        }

        return totalScore;
    }

    // public int CalculateFinalScore(List<CardView> cluster) {
    //     int chips = 0;
    //     float mult = 1.0f;

    //     // ขั้นตอนที่ 1: Chips (บวกให้จบ)
    //     foreach (var card in cluster) {
    //         chips += card.CardData.scoreValue;
    //         chips += GetAbilityChips(card); // เช็ค Sunflower, Fern, etc.
    //     }

    //     // ขั้นตอนที่ 2: Hand Rank (บวก Chips/Mult พื้นฐานของชุด)
    //     ComboResult hand = GetHandRank(cluster);
    //     chips += hand.baseChips;
    //     mult += (hand.baseMult - 1);

    //     // ขั้นตอนที่ 3: Multiply (คูณให้แหลก)
    //     foreach (var card in cluster) {
    //         mult *= GetSlotMultipliers(card); // เช็ค Ivy, Fertile, etc.
    //     }

    //     return Mathf.RoundToInt(chips * mult);
    // }

    private int CalculateClusterScore(List<CardView> cluster) {
        int totalChips = 0;
        float totalMult = 1.0f; // เริ่มต้นที่ x1

        // --- STEP 1: คิดคะแนนรายใบ (Chips Phase) ---
        foreach (var card in cluster) {
            // 1.1 บวกแต้มหน้าการ์ด
            totalChips += card.CardData.scoreValue;

            // 1.2 เช็ค Ability และพื้นที่ (Anatomy) ที่ให้แต้มบวก (Chips)
            totalChips += GetChipsBonus(card);
        }

        // --- STEP 2: คิดคะแนนจากชุด Combo (Hand Phase) ---
        ComboResult combo = GetFinalCombo(cluster);
        totalChips += combo.baseChips;
        totalMult += (combo.baseMult - 1); // บวกตัวคูณเพิ่มจากฐาน เช่น x2 คือบวกเพิ่ม 1

        // --- STEP 3: คิดตัวคูณจากพื้นที่และสกิล (Multiplication Phase) ---
        foreach (var card in cluster) {
            totalMult *= GetMultBonus(card, combo);
        }

        // --- STEP 4: รวมยอด (The Final Calc) ---
        // สูตร: Total = Chips * Mult
        int finalScore = Mathf.RoundToInt(totalChips * totalMult);

        Debug.Log($"[Cluster {combo.comboName}] Chips: {totalChips} x Mult: {totalMult:F1} = {finalScore}");
        return finalScore;
    }

    private int GetChipsBonus(CardView card) {
        int bonus = 0;
        List<Slot> occupied = card.GetOccupiedSlots();

        // เช็คเงื่อนไขพื้นฐาน (Fertile, Shade, Edge) ตามที่เคยเขียน
        // ... (Code เดิม) ...

        // เพิ่มเงื่อนไขพิเศษ: Red Rose (Thorned Love)
        if (card.CardData.cardName == "Red Rose") {
            var neighbors = GetNeighborCards(card);
            if (neighbors.Any(n => n.CardData.Color == FlowerColor.Red))
                bonus += 30;
        }

        return bonus;
    }

    private float GetMultBonus(CardView card, ComboResult currentCombo) {
        float mult = 1.0f;
        // ... (เช็คพื้นที่ Fertile/Edge/Shade ตามเดิม) ...

        // เพิ่มเงื่อนไขพิเศษ: Blue Orchid (Exotic Power)
        if (card.CardData.cardName == "Blue Orchid") {
            if (currentCombo.comboName == "Color Flush")
                mult *= 2.0f;
        }

        return mult;
    }


    // --- Helper: หาเพื่อนบ้าน (เช็คจาก Slot) ---
    public List<CardView> GetNeighborCards(CardView card) {
        List<CardView> neighbors = new List<CardView>();
        List<Slot> slots = card.GetOccupiedSlots();

        if (slots == null || slots.Count == 0) {
            Debug.LogWarning($"Card {card.name} has no occupied slots recorded!");
            return neighbors;
        }

        foreach (Slot s in slots) {
            int x = s.Coordinate.x;
            int y = s.Coordinate.y;

            // เช็ค 4 ทิศทางรอบ Slot นั้นๆ
            Vector2Int[] directions = {
            new Vector2Int(x + 1, y),
            new Vector2Int(x - 1, y),
            new Vector2Int(x, y + 1),
            new Vector2Int(x, y - 1)
        };

            foreach (var dir in directions) {
                Slot neighborSlot = GridSystem.Instance.GetSlotAt(dir.x, dir.y);
                if (neighborSlot != null && neighborSlot.isOccupied) {
                    CardView neighborCard = neighborSlot.OccupiedCard;

                    // ต้องไม่ใช่ตัวมันเอง และต้องมีการ์ดอยู่จริงๆ
                    if (neighborCard != null && neighborCard != card) {
                        if (!neighbors.Contains(neighborCard)) {
                            neighbors.Add(neighborCard);
                            // Debug.Log($"Found neighbor: {neighborCard.name} at ({dir.x},{dir.y})");
                        }
                    }
                }
            }
        }
        return neighbors;
    }

    // --- การ์ดทั้งหมดบนบอร์ด ---
    private List<CardView> GetCardsFromGrid() {
        List<CardView> cards = new List<CardView>();
        // สมมติว่า allSlots อยู่ใน GridManager
        foreach (Slot s in GameManager.Instance.AllSlots) {
            if (s.isOccupied && s.OccupiedCard != null && !cards.Contains(s.OccupiedCard))
                cards.Add(s.OccupiedCard);
        }
        return cards;
    }

    private ComboResult GetFinalCombo(List<CardView> cluster) {
        int count = cluster.Count;

        // 1. Grand Floral (5 ใบขึ้นไป และชนิดเดียวกันหมด) - ระดับสูงสุด
        if (count >= 5 && HasTypeSet(cluster, count))
            return new ComboResult { comboName = "Grand Floral", baseChips = 200, baseMult = 8f };

        // 2. Rainbow Bouquet (4 ใบขึ้นไป และสีไม่ซ้ำกันเลย)
        if (count >= 4 && IsRainbow(cluster))
            return new ComboResult { comboName = "Rainbow Bouquet", baseChips = 150, baseMult = 5f };

        // 3. Color Flush (3 ใบขึ้นไป และสีเดียวกันหมด)
        if (count >= 3 && HasColorSet(cluster, count))
            return new ComboResult { comboName = "Color Flush", baseChips = 80, baseMult = 4f };

        // 4. Triple Set (3 ใบ และชนิดเดียวกันหมด)
        if (count >= 3 && HasTypeSet(cluster, count))
            return new ComboResult { comboName = "Triple Set", baseChips = 60, baseMult = 3f };

        // 5. Duo Pair (2 ใบ ชนิดเดียวกัน)
        if (count >= 2 && HasTypeSet(cluster, count))
            return new ComboResult { comboName = "Duo Pair", baseChips = 30, baseMult = 2f };

        // 6. Simple Cluster (กลุ่มที่ต่อกันเฉยๆ แต่ไม่เข้าพวก)
        if (count >= 2)
            return new ComboResult { comboName = "Simple Cluster", baseChips = 10, baseMult = 1.2f };

        return new ComboResult { comboName = "Single", baseChips = 0, baseMult = 1f };
    }

    // Helper เพิ่มเติมสำหรับเช็คสีไม่ซ้ำ (Rainbow)
    private bool IsRainbow(List<CardView> cards) {
        return cards.Select(c => c.CardData.Color).Distinct().Count() == cards.Count;
    }

    private bool HasTypeSet(List<CardView> cards, int count) {
        return cards.GroupBy(c => c.CardData.type).Any(g => g.Count() >= count);
    }

    // เช็คจำนวนสี (Color)
    private bool HasColorSet(List<CardView> cards, int count) {
        return cards.GroupBy(c => c.CardData.Color).Any(g => g.Count() >= count);
    }

    // เช็ค Full House (3 ชนิดหนึ่ง + 2 อีกชนิดหนึ่ง)
    private bool IsFullHouse(List<CardView> cards) {
        var groups = cards.GroupBy(c => c.CardData.type).OrderByDescending(g => g.Count()).ToList();
        return groups.Count >= 2 && groups[0].Count() >= 3 && groups[1].Count() >= 2;
    }

    // เช็ค Two Pairs สี
    private bool HasTwoPairsColor(List<CardView> cards) {
        var groups = cards.GroupBy(c => c.CardData.Color).Where(g => g.Count() >= 2).ToList();
        return groups.Count >= 2;
    }

    // เช็ค Straight (คะแนนเรียงกัน)
    private bool IsStraight(List<CardView> cards, int count) {
        if (cards.Count < count) return false;
        // ดึงคะแนนมาเรียงลำดับและเอาค่าที่ซ้ำออก
        var scores = cards.Select(c => c.CardData.scoreValue).Distinct().OrderBy(s => s).ToList();

        int consecutive = 1;
        for (int i = 0; i < scores.Count - 1; i++) {
            // เช็คว่าค่าถัดไปคือค่าปัจจุบัน + interval (สมมติ interval ละ 10 เช่น 10, 20, 30)
            if (scores[i + 1] == scores[i] + 10) {
                consecutive++;
                if (consecutive >= count) return true;
            } else consecutive = 1;
        }
        return false;
    }
}
