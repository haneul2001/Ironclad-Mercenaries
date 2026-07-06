using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleController : MonoBehaviour
{
    // 시작 버튼 OnClick에 연결 (이미 연결돼 있음)
    public void OnStartClicked()
    {
        if (FadeController.Instance != null)
            FadeController.Instance.FadeToScene("GameScene");
        else
            SceneManager.LoadScene("GameScene");   // 페이드 없을 때 폴백
    }

    // 종료 버튼 만들면 연결 (선택)
    public void OnQuitClicked()
    {
        Application.Quit();
    }
}