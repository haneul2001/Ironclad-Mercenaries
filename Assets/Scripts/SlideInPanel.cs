using UnityEngine;
using System.Collections;

public class SlideInPanel : MonoBehaviour
{
    public float slideDuration = 0.5f;
    public float startY = 800f;    // 화면 위 (안 보임)
    public float endY = 0f;        // 제자리 (중앙)

    private RectTransform rect;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    // 위에서 아래로 내려오기
    public void SlideDown()
    {
        StartCoroutine(SlideRoutine(startY, endY));
    }

    // 아래에서 위로 올라가기 (사라짐)
    public void SlideUp()
    {
        StartCoroutine(SlideRoutine(endY, startY));
    }

    private IEnumerator SlideRoutine(float fromY, float toY)
    {
        Vector2 startPos = new Vector2(rect.anchoredPosition.x, fromY);
        Vector2 endPos = new Vector2(rect.anchoredPosition.x, toY);
        rect.anchoredPosition = startPos;

        float elapsed = 0f;
        while (elapsed < slideDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / slideDuration;
            t = 1f - (1f - t) * (1f - t);   // ease out
            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }
        rect.anchoredPosition = endPos;
    }
}