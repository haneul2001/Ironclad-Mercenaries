using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneController : MonoBehaviour
{
    public void RestartGame()
    {
        Time.timeScale = 1f; // 멈췄던 시간 되돌리기
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    // "타이틀로" 버튼이 호출
    public void OnTitleClicked()
    {
        Time.timeScale = 1f;   // 혹시 멈춰있으면 풀기

        if (FadeController.Instance != null)
            FadeController.Instance.FadeToScene("Title");
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("Title");
    }
}
