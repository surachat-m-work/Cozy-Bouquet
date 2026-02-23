using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridSystem : Singleton<GridSystem> {
    // เก็บ Slot ทั้งหมดในรูปแบบ Dictionary เพื่อให้ค้นหาตามพิกัดได้ง่าย
    // public Dictionary<Vector2Int, Slot> slotMap = new Dictionary<Vector2Int, Slot>();

    public const int GridSize = 6;

    private static readonly SlotType?[,] SlotLayout = new SlotType?[GridSize, GridSize] {
        { null,           null,           SlotType.Border, SlotType.Border, null,           null           },
        { null,           SlotType.Shade, SlotType.Open,   SlotType.Open,   SlotType.Shade, null           },
        { SlotType.Border, SlotType.Open, SlotType.Bloom,  SlotType.Bloom,  SlotType.Open,  SlotType.Border },
        { SlotType.Border, SlotType.Open, SlotType.Bloom,  SlotType.Bloom,  SlotType.Open,  SlotType.Border },
        { null,           SlotType.Shade, SlotType.Open,   SlotType.Open,   SlotType.Shade, null           },
        { null,           null,           SlotType.Border, SlotType.Border, null,           null           }
    };

    // [row, col] → Slot หรือ null ถ้าช่องนั้นตัดออก
    private Slot[,] _grid = new Slot[GridSize, GridSize];

    protected override void Awake() {
        base.Awake();
        InitializeGrid();
        // หา Slot ทั้งหมดในฉาก (ต้องแน่ใจว่า SlotSlot อยู่ใต้ GridPanel/SlotContainer)
        // Slot[] allSlots = FindObjectsByType<Slot>(FindObjectsSortMode.None);
        // foreach (Slot slot in allSlots) {
        //     slotMap[slot.Coordinate] = slot;
        // }
    }

    

    private void InitializeGrid() {
        for (int row = 0; row < GridSize; row++) {
            for (int col = 0; col < GridSize; col++) {
                SlotType? type = SlotLayout[row, col];
                if (type.HasValue) {
                    _grid[row, col] = new Slot(type.Value, new Vector2Int(row, col));
                }
            }
        }
    }

    /// <summary>
    /// ดึง SlotView จากตำแหน่ง row, col
    /// คืน null ถ้าอยู่นอก grid หรือเป็นช่องที่ตัดออก
    /// </summary>
    public Slot GetSlot(int row, int col) {
        if (row < 0 || row >= GridSize || col < 0 || col >= GridSize) return null;
        return _grid[row, col];
    }

    public Slot GetSlot(Vector2Int position) => GetSlot(position.x, position.y);

    // ฟังก์ชันให้ Slot เรียกใช้เพื่อหาเพื่อนบ้าน
    // public Slot GetSlotAt(int x, int y) {
    //     Vector2Int pos = new Vector2Int(x, y);
    //     return slotMap.ContainsKey(pos) ? slotMap[pos] : null;
    // }


    /// <summary>
    /// เช็คว่าสามารถวางการ์ด 1x2 ลงตำแหน่งนี้ได้ไหม
    /// </summary>
    public bool CanPlaceCard(Vector2Int origin, CardOrientation orientation) {
        Vector2Int secondSlot = GetSecondSlotPosition(origin, orientation);

        Slot slotA = GetSlot(origin);
        Slot slotB = GetSlot(secondSlot);

        if (slotA == null || slotB == null) return false;
        if (slotA.IsOccupied || slotB.IsOccupied) return false;

        return true;
    }

    /// <summary>
    /// วางการ์ดลง grid
    /// คืน true ถ้าวางสำเร็จ
    /// </summary>
    public bool PlaceCard(CardView card, Vector2Int origin, CardOrientation orientation) {
        if (!CanPlaceCard(origin, orientation)) return false;

        Vector2Int secondSlot = GetSecondSlotPosition(origin, orientation);
        _grid[origin.x, origin.y].PlaceCard(card);
        _grid[secondSlot.x, secondSlot.y].PlaceCard(card);

        card.SetPlacement(origin, orientation);
        return true;
    }

    /// <summary>
    /// เอาการ์ดออกจาก grid
    /// </summary>
    public void RemoveCard(CardView card) {
        Vector2Int origin = card.PlacedOrigin;
        Vector2Int secondSlot = GetSecondSlotPosition(origin, card.Orientation);

        GetSlot(origin)?.ClearCard();
        GetSlot(secondSlot)?.ClearCard();
        card.ClearPlacement();
    }

    /// <summary>
    /// หา clusters ทั้งหมดใน grid (กลุ่มการ์ดที่ติดกัน)
    /// ใช้ BFS flood fill
    /// </summary>
    public List<List<CardView>> GetClusters() {
        HashSet<CardView> visited = new();
        List<List<CardView>> clusters = new();

        for (int row = 0; row < GridSize; row++) {
            for (int col = 0; col < GridSize; col++) {
                Slot slot = _grid[row, col];
                if (slot == null || !slot.IsOccupied) continue;

                CardView card = slot.OccupyingCard;
                if (visited.Contains(card)) continue;

                List<CardView> cluster = new();
                BFS(card, visited, cluster);
                clusters.Add(cluster);
            }
        }

        return clusters;
    }

    private void BFS(CardView startCard, HashSet<CardView> visited, List<CardView> cluster) {
        Queue<CardView> queue = new();
        queue.Enqueue(startCard);
        visited.Add(startCard);

        while (queue.Count > 0) {
            CardView current = queue.Dequeue();
            cluster.Add(current);

            foreach (CardView neighbor in GetAdjacentCards(current)) {
                if (!visited.Contains(neighbor)) {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }
    }

    /// <summary>
    /// หาการ์ดที่อยู่ติดกับการ์ดนี้ (4 ทิศทาง)
    /// </summary>
    private List<CardView> GetAdjacentCards(CardView card) {
        List<CardView> adjacent = new();
        List<Vector2Int> occupiedPositions = GetOccupiedPositions(card);

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (Vector2Int pos in occupiedPositions) {
            foreach (Vector2Int dir in directions) {
                Slot neighbor = GetSlot(pos + dir);
                if (neighbor == null || !neighbor.IsOccupied) continue;
                if (neighbor.OccupyingCard == card) continue;
                if (!adjacent.Contains(neighbor.OccupyingCard)) {
                    adjacent.Add(neighbor.OccupyingCard);
                }
            }
        }

        return adjacent;
    }

    /// <summary>
    /// คืน slot positions ที่การ์ดนี้ครอบอยู่ (2 ช่อง)
    /// </summary>
    public List<Vector2Int> GetOccupiedPositions(CardView card) {
        Vector2Int origin = card.PlacedOrigin;
        Vector2Int second = GetSecondSlotPosition(origin, card.Orientation);
        return new List<Vector2Int> { origin, second };
    }

    /// <summary>
    /// คำนวณตำแหน่งช่องที่ 2 จาก origin และ orientation
    /// </summary>
    public Vector2Int GetSecondSlotPosition(Vector2Int origin, CardOrientation orientation) {
        return orientation == CardOrientation.Horizontal
            ? origin + Vector2Int.right
            : origin + Vector2Int.down;
    }

    // private Slot GetSecondSlot(SlotView topSlot, CardOrientation orientation) {
    //     if (orientation == CardOrientation.Horizontal)
    //         return GetSlot(topSlot.Coordinate.x + 1, topSlot.Coordinate.y);
    //     else
    //         return GetSlot(topSlot.Coordinate.x, topSlot.Coordinate.y + 1);
    // }

    /// <summary>
    /// คืน SlotType ทั้ง 2 ช่องที่การ์ดครอบอยู่
    /// </summary>
    public (SlotType slotA, SlotType slotB) GetCardSlotTypes(CardView card) {
        List<Vector2Int> positions = GetOccupiedPositions(card);
        SlotType a = GetSlot(positions[0]).SlotType;
        SlotType b = GetSlot(positions[1]).SlotType;
        return (a, b);
    }

    /// <summary>
    /// Clear การ์ดทั้งหมดออกจาก grid
    /// </summary>
    public void ClearGrid() {
        for (int row = 0; row < GridSize; row++) {
            for (int col = 0; col < GridSize; col++) {
                _grid[row, col]?.ClearCard();
            }
        }
    }

    // public List<CardView> GetNeighborsOfCard(CardView card) {
    //     HashSet<CardView> neighbors = new HashSet<CardView>();
    //     List<SlotView> occupiedSlots = card.GetOccupiedSlots();

    //     foreach (var slot in occupiedSlots) {
    //         Vector2Int pos = slot.Coordinate;

    //         // ตรวจสอบ 4 ทิศทางรอบ Slot นั้นๆ
    //         CheckDirection(pos.x + 1, pos.y, neighbors, card);
    //         CheckDirection(pos.x - 1, pos.y, neighbors, card);
    //         CheckDirection(pos.x, pos.y + 1, neighbors, card);
    //         CheckDirection(pos.x, pos.y - 1, neighbors, card);
    //     }

    //     return neighbors.ToList();
    // }

    // private void CheckDirection(int x, int y, HashSet<CardView> neighbors, CardView originalCard) {
    //     // เช็คว่าไม่หลุดขอบ Grid
    //     if (x >= 0 && x < 6 && y >= 0 && y < 6) {
    //         SlotView targetSlot = _allSlots[x, y];
    //         if (targetSlot != null && targetSlot.OccupiedCard != null) {
    //             // ถ้ามีการ์ดอยู่ และไม่ใช่การ์ดใบเดิมของเรา (ตัวมันเอง)
    //             if (targetSlot.OccupiedCard != originalCard) {
    //                 neighbors.Add(targetSlot.OccupiedCard);
    //             }
    //         }
    //     }
    // }
}
