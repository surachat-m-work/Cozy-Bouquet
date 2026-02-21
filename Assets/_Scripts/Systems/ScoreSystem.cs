using TMPro;
using UnityEngine;

public class ScoreSystem : Singleton<ScoreSystem> {
    [SerializeField] private int _currentTotalScore;

    public TextMeshProUGUI scoreText;
    private int totalScore = 0;

    private void OnEnable() {
        ActionSystem.AttachPerformer<AddScoreGA>(AddScorePerformer);
    }

    private System.Collections.IEnumerator AddScorePerformer(AddScoreGA action) {
        _currentTotalScore += action.Amount;
        Debug.Log($"<color=yellow>Score Updated: {_currentTotalScore} (+{action.Amount})</color>");

        // ตรงนี้คือจุดที่คุณจะสั่งให้ UI เด้งตัวเลข (Tweening)
        yield return null;
    }
    
    public void AddScore(int amount) {
        totalScore += amount;
        UpdateUI();
    }

    public void ResetTurnScore() {
        // สำหรับเริ่มรอบใหม่
    }

    void UpdateUI() {
        scoreText.text = "Score: " + totalScore.ToString();
    }
}