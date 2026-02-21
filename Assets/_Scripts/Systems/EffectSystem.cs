public class EffectSystem : Singleton<EffectSystem> {
    private void OnEnable() {
        ActionSystem.AttachPerformer<PerformEffectGA>(PerformEffectPerformer);
    }

    private void OnDisable() {
        ActionSystem.DetachPerformer<PerformEffectGA>();
    }

    private System.Collections.IEnumerator PerformEffectPerformer(PerformEffectGA action) {
        // ดึงข้อมูล GameAction ที่แท้จริงออกมาจาก Effect (เช่น AddScoreAction)
        GameAction realAction = action.Effect.GetGameAction(action.Card);

        if (realAction != null) {
            // ส่งผลลัพธ์นั้นกลับเข้าไปในระบบเป็น Reaction อีกชั้นหนึ่ง
            ActionSystem.Instance.AddReaction(realAction);
        }

        // เราสามารถใส่ yield return new WaitForSeconds(0.1f) ตรงนี้ได้ 
        // ถ้าต้องการให้ Effect ค่อยๆ รันทีละอันสวยๆ เหมือนในคลิป
        yield return null;
    }
}