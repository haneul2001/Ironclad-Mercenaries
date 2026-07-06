using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("스테이지 설정")]
    public int targetWave = 3;

    public GameObject heroSelectPanel;

    [Header("Day")]
    public int currentDay = 1;
    public EnemySpawner spawner;
    public TMP_Text dayText;

    [Header("카운트다운")]
    public Image countdownImage;        // 카운트다운 표시할 Image
    public Sprite[] countdownSprites;   // [0]=3, [1]=2, [2]=1, [3]=시작!

    [Header("UI 패널")]
    public GameObject victoryPanel;
    public GameObject defeatPanel;

    [Header("정비 UI")]
    public GameObject upgradePanel;
    public UpgradeMenuController upgradeMenu;

    [Header("클릭 안내 텍스트")]
    public GameObject clickToContinueText;

    public enum GameState { Playing, Victory, Defeat }
    public GameState currentState = GameState.Playing;

    private bool waitingForClick = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        UpdateDayText();

        if (heroSelectPanel != null)
        {
            heroSelectPanel.SetActive(true);
            Time.timeScale = 0f;   // 선택 전 완전 정지
        }
        else
        {
            StartFirstDay();
        }
    }

    // 선택창에서 주인공 고른 뒤 호출 → 카운트다운 후 전투 시작
    public void StartFirstDay()
    {
        if (heroSelectPanel != null)
            heroSelectPanel.SetActive(false);

        StartCoroutine(CountdownThenStartDay());
    }

    // 카운트다운(3,2,1,시작!) 후 해당 날 전투 시작. 시작/다음날 공용.
    private IEnumerator CountdownThenStartDay()
    {
        Time.timeScale = 0f;   // 카운트다운 중엔 멈춤

        if (countdownImage != null)
            countdownImage.gameObject.SetActive(true);

        int[] order = { 0, 1, 2 };   // 스프라이트 인덱스 (3, 2, 1)
        foreach (int idx in order)
        {
            if (countdownImage != null && countdownSprites != null && idx < countdownSprites.Length)
                countdownImage.sprite = countdownSprites[idx];
            yield return new WaitForSecondsRealtime(1f);
        }

        // "시작!" 표시 (스프라이트 4개면)
        if (countdownImage != null && countdownSprites != null && countdownSprites.Length >= 4)
        {
            countdownImage.sprite = countdownSprites[3];
            yield return new WaitForSecondsRealtime(0.6f);
        }

        if (countdownImage != null)
            countdownImage.gameObject.SetActive(false);

        // 전투 시작
        Time.timeScale = 1f;
        if (spawner != null)
            spawner.StartDay(currentDay);
    }

    void Update()
    {
        if (waitingForClick && Input.GetMouseButtonDown(0))
        {
            waitingForClick = false;
            OnVictoryClicked();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)) Time.timeScale = 1f;
        if (Input.GetKeyDown(KeyCode.Alpha2)) Time.timeScale = 2f;
        if (Input.GetKeyDown(KeyCode.Alpha3)) Time.timeScale = 4f;
    }

    void UpdateDayText()
    {
        if (dayText != null)
            dayText.text = "DAY " + currentDay;
    }

    public void OnCoreDestroyed()
    {
        if (currentState != GameState.Playing) return;
        currentState = GameState.Defeat;
        Debug.Log("게임 오버! 성벽이 파괴되었습니다.");
        if (defeatPanel != null) defeatPanel.SetActive(true);
        EndGame();
    }

    public void OnAllWavesCleared()
    {
        if (currentState != GameState.Playing) return;
        currentState = GameState.Victory;
        Debug.Log("승리! 모든 웨이브를 클리어했습니다.");
        StartCoroutine(VictorySequence());
    }

    IEnumerator VictorySequence()
    {
        yield return new WaitForSeconds(3f);

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            SlideInPanel slide = victoryPanel.GetComponentInChildren<SlideInPanel>();
            if (slide != null) slide.SlideDown();
        }

        yield return new WaitForSeconds(2f);

        if (clickToContinueText != null)
            clickToContinueText.SetActive(true);

        waitingForClick = true;
    }

    void OnVictoryClicked()
    {
        if (clickToContinueText != null)
            clickToContinueText.SetActive(false);

        StartCoroutine(ToUpgradeSequence());
    }

    IEnumerator ToUpgradeSequence()
    {
        if (victoryPanel != null)
        {
            SlideInPanel slide = victoryPanel.GetComponentInChildren<SlideInPanel>();
            if (slide != null) slide.SlideUp();
        }

        yield return new WaitForSecondsRealtime(0.5f);

        if (victoryPanel != null) victoryPanel.SetActive(false);

        if (upgradePanel != null)
        {
            upgradePanel.SetActive(true);

            if (upgradeMenu != null && upgradeMenu.selectMenu != null)
                upgradeMenu.selectMenu.SlideDown();
        }

        if (UpgradeManager.Instance != null)
            UpgradeManager.Instance.GrantBudgets();

        Debug.Log("정비 UI 내려옴");
    }

    // "다음 날로" 버튼이 호출
    public void OnNextDayClicked()
    {
        // 1) AI 자동 강화
        if (UpgradeManager.Instance != null)
            UpgradeManager.Instance.RunAIUpgrades();

        // 2) 정비소 패널들 리셋
        if (upgradeMenu != null)
            upgradeMenu.ResetPanels();

        // 3) 정비 UI 닫기
        if (upgradePanel != null)
            upgradePanel.SetActive(false);

        if (clickToContinueText != null)
            clickToContinueText.SetActive(false);

        // 4) 다음 날로
        currentDay++;
        UpdateDayText();
        currentState = GameState.Playing;

        // 5) 카운트다운 후 새 날 시작
        StartCoroutine(CountdownThenStartDay());

        Debug.Log($"=== Day {currentDay} 시작 ===");
    }

    void EndGame()
    {
        Time.timeScale = 0f;
    }
}