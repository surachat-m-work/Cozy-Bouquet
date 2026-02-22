using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CardView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    [Header("Card Settings")]
    public CardData CardData { get; private set; }
    public bool IsHorizontal { get; private set; }


    public void Setup(CardData data) {
        CardData = data;
        // Update UI: Sprite, Name, Color
    }

    // ฟังก์ชันเคลื่อนที่แบบ Balatro
    public IEnumerator AnimateToHand(HandCurveData curve, int index, int total) {
        float t = (total > 1) ? (float)(index - 1) / (total - 1) : 0.5f;

        // คำนวณตำแหน่งและองศาการหมุนจาก Curve
        Vector3 targetPos = new Vector3(t * 10f, curve.positioning.Evaluate(t) * curve.positioningInfluence, 0);
        float targetRot = curve.rotation.Evaluate(t) * curve.rotationInfluence;

        // ใช้ LeanTween หรือ Simple Lerp
        float duration = 0.4f;
        float elapsed = 0;
        Vector3 startPos = transform.localPosition;
        Quaternion startRot = transform.localRotation;

        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float p = elapsed / duration;
            transform.localPosition = Vector3.Lerp(startPos, targetPos, p);
            transform.localRotation = Quaternion.Lerp(startRot, Quaternion.Euler(0, 0, targetRot), p);
            yield return null;
        }
    }

    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Transform handContainer;
    private Transform cardsContainer;

    private Vector2 originalPosition;
    private Transform originalParent = null;

    private List<Slot> occupiedSlots = new List<Slot>();

    public GameObject ghostPrefab; // ลาก Prefab Ghost มาใส่ใน Inspector
    private GameObject currentGhost;

    public bool isHorizontal = false;
    private Slot lastHoveredSlot;
    private Slot currentTargetTopSlot; // ช่องบนสุดที่เราเล็งไว้ปัจจุบัน

    private void Awake() {
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        handContainer = GameObject.Find("HandView").transform;
        cardsContainer = GameObject.Find("CardsContainer").transform;
    }

    void Update() {
        // ตรวจจับการกดปุ่ม R เฉพาะตอนที่กำลังลากการ์ดอยู่เท่านั้น
        if (canvasGroup.blocksRaycasts == false && Keyboard.current.rKey.wasPressedThisFrame) {
            RotateCard();
        }
    }

    public void NotifyPointerExitSlot(Slot slot) {
        // ถ้าเมาส์ออกจาก Slot ที่เราเคยบันทึกไว้ล่าสุดจริงๆ
        if (lastHoveredSlot == slot) {
            lastHoveredSlot = null;
            currentTargetTopSlot = null; // ล้างเป้าหมาย Snap ด้วย
            HideGhost();
        }
    }

    private void RotateCard() {
        isHorizontal = !isHorizontal;
        float angle = isHorizontal ? -90f : 0f;
        rectTransform.localRotation = Quaternion.Euler(0, 0, angle);

        if (lastHoveredSlot != null) ShowGhost(lastHoveredSlot);
    }

    public void ReturnToHand() {
        isHorizontal = false;
        rectTransform.localRotation = Quaternion.identity;
        transform.SetParent(handContainer);
    }

    public void OnBeginDrag(PointerEventData eventData) {
        // --- เพิ่ม Logic คืนพื้นที่ตรงนี้ ---
        foreach (Slot s in occupiedSlots) {
            s.ClearSlot(); // สั่งให้ Slot ว่าง
        }
        occupiedSlots.Clear(); // ล้างรายชื่อ Slot ที่เคยทับ
                               // ----------------------------

        originalParent = transform.parent;
        transform.SetParent(canvas.transform); // ให้การ์ดลอยอยู่ชั้นบนสุด
        canvasGroup.blocksRaycasts = false;  // ให้เมาส์ทะลุไปโดน Slot ข้างหลัง

        lastHoveredSlot = null;
        currentTargetTopSlot = null;
    }

    public void OnDrag(PointerEventData eventData) {
        rectTransform.anchoredPosition += eventData.delta; // ลากตามเมาส์
                                                           // transform.position = Input.mousePosition; // ลากตามเมาส์

        if (lastHoveredSlot != null) {
            ShowGhost(lastHoveredSlot);
        }
    }

    public void OnEndDrag(PointerEventData eventData) {
        canvasGroup.blocksRaycasts = true;
        HideGhost();

        if (currentGhost != null) Destroy(currentGhost);

        if (transform.parent == canvas.transform) {
            ReturnToHand();
        }
    }

    public bool CanFit(Slot startSlot) {

        // หา Slot ที่อยู่ข้างล่าง (พิกัด Y + 1)
        // ตรงนี้คุณต้องมีระบบอ้างอิง Slot ตามพิกัด X,Y (เช่น Dictionary)
        Slot bottomSlot = GridSystem.Instance.GetSlotAt(startSlot.Coordinate.x, startSlot.Coordinate.y + 1);

        return bottomSlot != null && !bottomSlot.isOccupied;
    }

    // ฟังก์ชันให้ Slot เรียกเพื่อบันทึกการจอง
    public void SetCurrentSlots(params Slot[] slots) {
        occupiedSlots.Clear();
        occupiedSlots.AddRange(slots);
    }

    public void ShowGhost(Slot hoveredSlot) {
        lastHoveredSlot = hoveredSlot;

        if (currentTargetTopSlot == null) {
            DetermineInitialSlot(hoveredSlot);
        } else {
            // Hysteresis Logic: ถ้าเมาส์ยังวนเวียนในเขตการจองเดิม ไม่ต้องคำนวณใหม่
            Slot secondOccupied = GetSecondSlot(currentTargetTopSlot);
            if (hoveredSlot != currentTargetTopSlot && hoveredSlot != secondOccupied) {
                DetermineInitialSlot(hoveredSlot);
            }
        }
        UpdateGhostPosition();
    }

    private void DetermineInitialSlot(Slot hoveredSlot) {

        // ใช้ตำแหน่งกึ่งกลางการ์ดเทียบกับกึ่งกลาง Slot ตามที่คุณต้องการ
        float cardX = transform.position.x;
        float slotX = hoveredSlot.transform.position.x;
        float cardY = transform.position.y;
        float slotY = hoveredSlot.transform.position.y;

        if (isHorizontal) {
            // แนวนอน: ถ้ากางการ์ดอยู่ขวาของ Slot ให้ช่องนี้เป็นช่องซ้าย
            if (cardX > slotX) {
                currentTargetTopSlot = hoveredSlot;
            } else // ถ้าการ์ดอยู่ซ้าย ให้ถอยไปจองช่องทางซ้าย (เพื่อให้ช่องที่เมาส์ชี้เป็นช่องขวา)
              {
                Slot left = GridSystem.Instance.GetSlotAt(hoveredSlot.Coordinate.x - 1, hoveredSlot.Coordinate.y);
                currentTargetTopSlot = (left != null && !left.isOccupied) ? left : hoveredSlot;
            }
        } else {
            // แนวตั้ง: ถ้ากางการ์ดอยู่สูงกว่ากึ่งกลาง Slot ให้ถอยขึ้นไปจองช่องบน
            if (cardY > slotY) {
                Slot above = GridSystem.Instance.GetSlotAt(hoveredSlot.Coordinate.x, hoveredSlot.Coordinate.y - 1);
                currentTargetTopSlot = (above != null && !above.isOccupied) ? above : hoveredSlot;
            } else {
                currentTargetTopSlot = hoveredSlot;
            }
        }
    }

    private void UpdateGhostPosition() {
        if (currentTargetTopSlot == null) return;

        Slot s1 = currentTargetTopSlot;
        Slot s2 = GetSecondSlot(s1);

        bool canPlace = !s1.isOccupied && s2 != null && !s2.isOccupied;

        if (canPlace) {
            if (currentGhost == null) {
                currentGhost = Instantiate(ghostPrefab, canvas.transform);
                // currentGhost.GetComponent<CanvasGroup>().blocksRaycasts = false; // กันกระพริบ
            }

            currentGhost.SetActive(true);
            currentGhost.transform.localRotation = rectTransform.localRotation;

            // Snap ตรงกลาง
            Vector3 targetPos = (s2 != null) ? (s1.transform.position + s2.transform.position) / 2f : s1.transform.position;
            currentGhost.transform.position = targetPos;
        } else {
            HideGhost();
        }
    }

    private Slot GetSecondSlot(Slot topSlot) {
        if (isHorizontal)
            return GridSystem.Instance.GetSlotAt(topSlot.Coordinate.x + 1, topSlot.Coordinate.y);
        else
            return GridSystem.Instance.GetSlotAt(topSlot.Coordinate.x, topSlot.Coordinate.y + 1);
    }

    public void HideGhost() {
        if (currentGhost != null) currentGhost.SetActive(false);
        // อย่าเพิ่งสั่ง currentTargetTopSlot = null ทันที เพื่อเก็บความจำ (Hysteresis)
    }

    public Slot GetCurrentTargetTopSlot() {
        return currentTargetTopSlot;
    }

    public List<Slot> GetOccupiedSlots() {
        return occupiedSlots;
    }

    public void PlaceInSlots(List<Slot> slots) {
        occupiedSlots = new List<Slot>(slots);

        foreach (Slot s in occupiedSlots) {
            s.SetCard(this);
        }

        // จัดตำแหน่งการ์ดให้อยู่ตรงกลางกลุ่ม Slot
        Vector3 centerPos = Vector3.zero;
        foreach (Slot s in slots) centerPos += s.transform.position;
        transform.position = centerPos / slots.Count;
        // transform.SetParent(slots[0].transform); // เกาะไว้กับ Slot แรก

        transform.SetParent(cardsContainer); // เกาะไว้กับ Slot แรก
        canvasGroup.blocksRaycasts = true;
    }

    // ใน DraggableCard.cs
    public List<Slot> GetAllTargetSlots(Slot anchorSlot) {
        List<Slot> targetSlots = new List<Slot>();
        if (anchorSlot == null) return targetSlots;

        targetSlots.Add(anchorSlot); // ช่องแรก (ที่เมาส์ชี้ หรือช่อง Anchor)

        int nextX = anchorSlot.Coordinate.x;
        int nextY = anchorSlot.Coordinate.y;

        if (isHorizontal) nextX += 1; // ถ้าแนวนอน ช่องที่สองคือ X + 1
        else nextY += 1; // ถ้าแนวตั้ง ช่องที่สองคือ Y + 1

        Slot secondSlot = GridSystem.Instance.GetSlotAt(nextX, nextY);
        if (secondSlot != null) {
            targetSlots.Add(secondSlot);
        }
        return targetSlots;
    }
}
