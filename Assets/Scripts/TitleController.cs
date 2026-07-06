using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleController : MonoBehaviour
{
    [Header("튜토리얼")]
    public GameObject tutorialPanel;   // 튜토리얼 페이지 패널

    // 게임 시작 버튼 OnClick
    public void OnStartClicked()
    {
        if (FadeController.Instance != null)
            FadeController.Instance.FadeToScene("GameScene");
        else
            SceneManager.LoadScene("GameScene");
    }

    // 튜토리얼 버튼 OnClick → 페이지 열기
    public void OnTutorialClicked()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(true);
    }

    // 튜토리얼 닫기 버튼 OnClick → 페이지 닫기
    public void OnTutorialClose()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);
    }

    // 나가기 버튼 OnClick
    public void OnQuitClicked()
    {
        Application.Quit();
    }
}