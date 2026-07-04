using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    public float floatSpeed = 2f;      // 위로 떠오르는 속도
    public float lifeTime = 1f;        // 사라지기까지 시간

    private TextMeshPro tmp;
    private float timer = 0f;

    void Awake()
    {
        tmp = GetComponent<TextMeshPro>();
    }

    // 데미지 값 설정
    public void SetDamage(int damage)
    {
        if (tmp != null)
        {
            tmp.text = damage.ToString();
        }
    }

    void Update()
{
    // 탑뷰: 화면 위쪽(월드 Z축)으로 이동
    transform.position += Vector3.forward * floatSpeed * Time.deltaTime;

    // 투명해지며 사라지기 (기존 그대로)
    timer += Time.deltaTime;
    if (tmp != null)
    {
        float alpha = 1f - (timer / lifeTime);
        tmp.color = new Color(tmp.color.r, tmp.color.g, tmp.color.b, alpha);
    }

    if (timer >= lifeTime)
    {
        Destroy(gameObject);
    }
}
}