using UnityEngine;

// 마법사 모델에 붙이는 중계 스크립트
public class AreaAnimationRelay : MonoBehaviour
{
    private AreaAttack areaAttack;

    void Start()
    {
        // 부모 쪽으로 올라가며 AreaAttack 찾기
        areaAttack = GetComponentInParent<AreaAttack>();
    }

    // 애니메이션 이벤트가 호출 → 부모의 Explode 실행
    public void Explode()
    {
        if (areaAttack != null)
        {
            areaAttack.Explode();
        }
    }
}