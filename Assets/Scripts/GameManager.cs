using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // 싱글톤 어디든지 참조 가능 ( 전역 )
    [Header("스테이지 설정")]
    public int targetWave = 5; // 목표 웨이브 수
    [Header("UI 패널")]
    public GameObject victoryPanel;
    public GameObject defeatPanel;


    //게임 상태
    public enum GameState{ Playing, Victory, Defeat}
    public GameState currentState = GameState.Playing;

    void Awake()
    {
        Instance = this;
    }
 
    public void OnCoreDestroyed()
    {
        if(currentState != GameState.Playing) return;
        currentState = GameState.Defeat;
        Debug.Log("게임 오버! 성벽이 파괴되었습니다.");
        if(defeatPanel != null) defeatPanel.SetActive(true);

        EndGame();
    }

    public void OnAllWavesCleared()
    {
        if(currentState!=GameState.Playing) return;
        currentState = GameState.Victory;
        Debug.Log("승리! 모든 웨이브를 클리어했습니다.");
        if(victoryPanel != null) victoryPanel.SetActive(true);
        EndGame();
    }

    void EndGame()
    {
        // 종료 처리
        Time.timeScale = 0f; // 게임 정지
    }
}

