using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneController : MonoBehaviour
{
    public void RestartGame()
    {
        Time.timeScale = 1f; // 멈췄던 시간 되돌리기
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
