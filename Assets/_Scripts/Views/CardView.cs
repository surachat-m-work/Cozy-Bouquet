using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CardView : MonoBehaviour {
    private Card _card;
    private Canvas _canvas;
    private CanvasGroup _canvasGroup;
    private RectTransform _rectTransform;
    private Transform _handContainer;
    private Transform _cardsContainer;

    public GameObject ghostPrefab; // ลาก Prefab Ghost มาใส่ใน Inspector
    public GameObject CurrentGhost { get; private set; }

    private void Awake() {
        _card = GetComponent<Card>();
        _canvas = GetComponentInParent<Canvas>();
        _canvasGroup = GetComponent<CanvasGroup>();
        _rectTransform = GetComponent<RectTransform>();
        _handContainer = GameObject.Find("HandView").transform;
        _cardsContainer = GameObject.Find("CardsContainer").transform;
    }

    void Update() {
        // ตรวจจับการกดปุ่ม R เฉพาะตอนที่กำลังลากการ์ดอยู่เท่านั้น
        if (_canvasGroup.blocksRaycasts == false && Keyboard.current.rKey.wasPressedThisFrame) {
            RotateCard();
        }
    }

    private void RotateCard() {
        _card.ToggleOrientation();
        float angle = _card.Orientation == CardOrientation.Horizontal ? -90f : 0f;
        _rectTransform.localRotation = Quaternion.Euler(0, 0, angle);

        if (_card.LastHoveredSlot != null) ShowGhost(_card.LastHoveredSlot);
    }

    public void ReturnToHand() {
        _card.SetOrientation(CardOrientation.Vertical);
        _rectTransform.localRotation = Quaternion.identity;
        transform.SetParent(_handContainer);
    }

    public void ShowGhost(Slot hoveredSlot) {
        _card.SetLastHoverSlot( hoveredSlot);

        if (currentTargetTopSlot == null) {
            DetermineInitialSlot(hoveredSlot);
        } else {
            // Hysteresis Logic: ถ้าเมาส์ยังวนเวียนในเขตการจองเดิม ไม่ต้องคำนวณใหม่
            Slot secondOccupied = GridSystem.Instance.GetSecondSlot(currentTargetTopSlot);
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

        if (_card.Orientation == CardOrientation.Horizontal) {
            // แนวนอน: ถ้ากางการ์ดอยู่ขวาของ Slot ให้ช่องนี้เป็นช่องซ้าย
            if (cardX > slotX) {
                _card.SetCurrentTargetStartSlot(hoveredSlot);
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
                currentGhost = Instantiate(ghostPrefab, _canvas.transform);
                // currentGhost.GetComponent<CanvasGroup>().blocksRaycasts = false; // กันกระพริบ
            }

            currentGhost.SetActive(true);
            currentGhost.transform.localRotation = _rectTransform.localRotation;

            // Snap ตรงกลาง
            Vector3 targetPos = (s2 != null) ? (s1.transform.position + s2.transform.position) / 2f : s1.transform.position;
            currentGhost.transform.position = targetPos;
        } else {
            HideGhost();
        }
    }



    public void HideGhost() {
        if (CurrentGhost != null) CurrentGhost.SetActive(false);
        // อย่าเพิ่งสั่ง currentTargetTopSlot = null ทันที เพื่อเก็บความจำ (Hysteresis)
    }




}
