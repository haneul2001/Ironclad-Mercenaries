using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class FadeController : MonoBehaviour
{
    public static FadeController Instance;

    public Image fadeImage;
    public float fadeDuration = 0.5f;

    private string pendingScene = null;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        // 최초 실행 시 밝아지며 시작
        SetAlpha(1f);
        StartCoroutine(FadeIn());
    }

    // 외부(시작 버튼 등)에서 호출
    public void FadeToScene(string sceneName)
    {
        pendingScene = sceneName;
        StartCoroutine(FadeOutThenLoad());
    }

    private IEnumerator FadeOutThenLoad()
    {
        yield return StartCoroutine(FadeOut());
        SceneManager.LoadScene(pendingScene);
        // 씬 로드 완료 후 OnSceneLoaded에서 FadeIn 실행
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 새 씬 로드되면 자동으로 밝아짐
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeOut()
    {
        float t = 0f;
        SetAlpha(0f);
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            SetAlpha(Mathf.Clamp01(t / fadeDuration));
            yield return null;
        }
        SetAlpha(1f);
    }

    private IEnumerator FadeIn()
    {
        float t = 0f;
        SetAlpha(1f);
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            SetAlpha(1f - Mathf.Clamp01(t / fadeDuration));
            yield return null;
        }
        SetAlpha(0f);
    }

    private void SetAlpha(float a)
    {
        if (fadeImage == null) return;
        Color c = fadeImage.color;
        c.a = a;
        fadeImage.color = c;
        fadeImage.raycastTarget = (a > 0.01f);
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}