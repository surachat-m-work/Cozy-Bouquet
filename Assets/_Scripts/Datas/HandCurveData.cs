using UnityEngine;

[CreateAssetMenu(fileName = "HandCurveData", menuName = "Data/Curve Parameters")]
public class HandCurveData : ScriptableObject {
    public AnimationCurve positioning;
    public float positioningInfluence = .02f;
    public AnimationCurve rotation;
    public float rotationInfluence = 0f;
}
