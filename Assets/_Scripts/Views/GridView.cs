using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GridView : Singleton<GridView>, IPointerEnterHandler {
    [Header("Slot Prefab")]
    [SerializeField] private GameObject _slotPrefab;

    [Header("Grid Layout")]
    [SerializeField] private RectTransform _gridContainer;
    [SerializeField] private RectTransform _cardContainer;

    [SerializeField] private float _slotSize = 100f;
    [SerializeField] private float _spacing = 10f;

    [Header("Visual")]
    [SerializeField] private bool _showDebugLabels = true;

    // ─── Storage ───────────────────────────────────────────────
    private SlotView[,] _slotViews = new SlotView[GridSystem.GridSize, GridSystem.GridSize];

    // ─── Properties ────────────────────────────────────────────
    public RectTransform CardContainer => _cardContainer;

    // ─── Initialization ────────────────────────────────────────
    protected override void Awake() {
        base.Awake();

        if (_gridContainer == null)
            _gridContainer = transform.GetChild(0) as RectTransform;

        if (_cardContainer == null) {
            // สร้าง card container ถ้ายังไม่มี
            GameObject cardContainerObj = new GameObject("Card Container");
            cardContainerObj.transform.SetParent(transform, false);
            _cardContainer = cardContainerObj.AddComponent<RectTransform>();
            _cardContainer.anchorMin = Vector2.zero;
            _cardContainer.anchorMax = Vector2.one;
            _cardContainer.sizeDelta = Vector2.zero;
        }
    }

    private void Start() {
        GenerateGrid();
    }

    public void OnPointerEnter(PointerEventData eventData) {
        CardView _draggingCard = eventData.pointerDrag?.GetComponent<CardView>();
        if (_draggingCard != null) {
            _draggingCard.HideGhost();
        }
    }

    // ─── Grid Generation ───────────────────────────────────────
    /// <summary>
    /// สร้าง grid slots ทั้งหมด
    /// </summary>
    [ContextMenu("Generate Grid")]
    public void GenerateGrid() {
        ClearGrid();

        if (_slotPrefab == null) {
            Debug.LogError("Slot prefab not assigned!");
            return;
        }

        // คำนวณ offset เพื่อให้ grid อยู่กลาง
        float totalWidth = (GridSystem.GridSize - 1) * (_slotSize + _spacing);
        float totalHeight = (GridSystem.GridSize - 1) * (_slotSize + _spacing);
        Vector2 startPos = new Vector2(-totalWidth * 0.5f, totalHeight * 0.5f);

        // สร้าง slot แต่ละช่อง
        for (int y = 0; y < GridSystem.GridSize; y++) {
            for (int x = 0; x < GridSystem.GridSize; x++) {
                Slot slotData = GridSystem.Instance.GetSlot(x, y);

                // ถ้า slot นี้ไม่มีอยู่จริง (ถูกตัดออก) ข้ามไป
                if (slotData == null) continue;

                // สร้าง slot view
                GameObject slotObj = Instantiate(_slotPrefab, _gridContainer);
                slotObj.name = $"Slot_{x}_{y}";

                RectTransform slotRect = slotObj.GetComponent<RectTransform>();
                slotRect.sizeDelta = new Vector2(_slotSize, _slotSize);

                // คำนวณตำแหน่ง
                Vector2 position = startPos + new Vector2(
                    x * (_slotSize + _spacing),
                    -y * (_slotSize + _spacing)
                );
                slotRect.anchoredPosition = position;

                // Initialize slot view
                SlotView slotView = slotObj.GetComponent<SlotView>();
                if (slotView == null) {
                    slotView = slotObj.AddComponent<SlotView>();
                }
                slotView.Initialize(new Vector2Int(x, y), slotData.SlotType);

                _slotViews[x, y] = slotView;

                // เพิ่ม debug label
                if (_showDebugLabels) {
                    CreateDebugLabel(slotObj, x, y, slotData.SlotType);
                }
            }
        }

        Debug.Log("Grid generated successfully!");
    }

    /// <summary>
    /// ลบ grid slots ทั้งหมด
    /// </summary>
    [ContextMenu("Clear Grid")]
    public void ClearGrid() {
        if (_gridContainer == null) return;

        // ลบ children ทั้งหมด
        foreach (Transform child in _gridContainer) {
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }

        _slotViews = new SlotView[GridSystem.GridSize, GridSystem.GridSize];
    }

    // ─── Debug Labels ──────────────────────────────────────────
    private void CreateDebugLabel(GameObject slotObj, int x, int y, SlotType type) {
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(slotObj.transform, false);

        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.sizeDelta = Vector2.zero;

        Text labelText = labelObj.AddComponent<Text>();
        labelText.text = $"{x},{y}\n{type}";
        labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        labelText.fontSize = 20;
        labelText.alignment = TextAnchor.MiddleCenter;
        labelText.color = Color.white;
        labelText.raycastTarget = false;
    }

    // ─── Public Utilities ──────────────────────────────────────
    /// <summary>
    /// ดึง GridSlotView จากตำแหน่ง
    /// </summary>
    public SlotView GetSlotView(int x, int y) {
        if (x < 0 || x >= GridSystem.GridSize || y < 0 || y >= GridSystem.GridSize)
            return null;
        return _slotViews[x, y];
    }

    public SlotView GetSlotView(Vector2Int position) => GetSlotView(position.x, position.y);

    public SlotView GetSecondSlotView(SlotView startSlot, CardOrientation orientation) {
        return orientation == CardOrientation.Horizontal
            ? GetSlotView(startSlot.GridPosition.x + 1, startSlot.GridPosition.y)
            : GetSlotView(startSlot.GridPosition.x, startSlot.GridPosition.y + 1);
    }

    // ─── Editor Helpers ────────────────────────────────────────
    private void OnValidate() {
        // Auto-generate grid ใน Editor mode
        if (!Application.isPlaying && _gridContainer != null && _slotPrefab != null) {
            // ไม่ auto-generate เพราะจะทำให้ช้า
            // ใช้ context menu "Generate Grid" แทน
        }
    }

    private void OnDrawGizmos() {
        if (!Application.isPlaying) return;

        // แสดง grid bounds
        Gizmos.color = Color.cyan;
        float totalWidth = (GridSystem.GridSize - 1) * (_slotSize + _spacing);
        float totalHeight = (GridSystem.GridSize - 1) * (_slotSize + _spacing);

        Vector3 center = transform.position;
        Vector3 size = new Vector3(totalWidth, totalHeight, 0f);

        Gizmos.DrawWireCube(center, size);
    }
}