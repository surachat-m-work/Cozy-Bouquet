using UnityEngine;
using UnityEngine.InputSystem;

public static class MouseUtil {
    private static Camera camera = Camera.main;
    private static Mouse mouse = Mouse.current;

    public static Vector3 GetMousePositionInWorldSpace(float zValue = 0f) {
        Plane dragPlan = new(camera.transform.forward, new Vector3(0, 0, zValue));
        Vector2 screenPos = Mouse.current.position.ReadValue();
        Ray ray = camera.ScreenPointToRay(screenPos);

        if (dragPlan.Raycast(ray, out float distance)) {
            return ray.GetPoint(distance);
        }

        return Vector3.zero;
    }

    // Helper: สำหรับหาตำแหน่งในโลก 3D (ใช้ตอน Drag)
    public static Vector3 GetMouseWorldPosition(float zDistance) {
        Vector3 mouseScreenPos = mouse.position.ReadValue();
        mouseScreenPos.z = zDistance;
        return camera.ScreenToWorldPoint(mouseScreenPos);
    }

    public static Ray GetMouseWorldPosition() {
        Vector3 mouseScreenPos = mouse.position.ReadValue();
        return camera.ScreenPointToRay(mouseScreenPos);
    }
}
