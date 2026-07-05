using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

// 도박 패널. 버튼 누르면 주사위 이미지가 촤르르 바뀌다 멈추고, 1초 뒤 보상 표시.
public class GamblePanel : MonoBehaviour
{
    public Button rollButton;
    public Image diceImage;         // 주사위 그림 표시
    public Sprite[] diceSprites;    // 주사위 1~6 이미지 (인덱스 0=눈1 ... 5=눈6)
    public TMP_Text resultText;

    [Header("연출 설정")]
    public float rollDuration = 1f;
    public float rollInterval = 0.05f;
    public float resultDelay = 1f;

    private bool isRolling = false;

    void Start()
    {
        if (rollButton != null)
            rollButton.onClick.AddListener(OnRoll);
    }

    void OnEnable()
    {
        isRolling = false;
        if (resultText != null) resultText.text = "주사위를 굴려보세요! (5원)";
        if (rollButton != null) rollButton.interactable = true;
    }

    void OnRoll()
    {
        if (isRolling) return;
        if (UpgradeManager.Instance == null) return;

        UpgradeManager.GambleResult r = UpgradeManager.Instance.TryGamble();

        if (!r.success)
        {
            if (resultText != null) resultText.text = "골드가 부족합니다!";
            return;
        }

        StartCoroutine(RollRoutine(r));
    }

    private IEnumerator RollRoutine(UpgradeManager.GambleResult r)
    {
        isRolling = true;
        if (rollButton != null) rollButton.interactable = false;
        if (resultText != null) resultText.text = "";

        // 1) 이미지 촤르르 (가짜 랜덤)
        float elapsed = 0f;
        while (elapsed < rollDuration)
        {
            SetDice(Random.Range(1, 7));
            yield return new WaitForSecondsRealtime(rollInterval);
            elapsed += rollInterval;
        }

        // 2) 진짜 결과에서 멈춤
        SetDice(r.dice);

        // 3) 1초 대기 후 보상 표시
        yield return new WaitForSecondsRealtime(resultDelay);

        if (resultText != null)
            resultText.text = (r.reward > 0) ? $"{r.reward}원 획득!" : "꽝!";

        // 4) 다시 굴릴 수 있게
        isRolling = false;
        if (rollButton != null) rollButton.interactable = true;
    }

    // 주사위 눈(1~6)에 맞는 스프라이트 표시
    private void SetDice(int n)
    {
        if (diceImage == null || diceSprites == null) return;
        int idx = Mathf.Clamp(n - 1, 0, diceSprites.Length - 1);
        if (idx < diceSprites.Length && diceSprites[idx] != null)
            diceImage.sprite = diceSprites[idx];
    }
}