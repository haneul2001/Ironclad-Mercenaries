using UnityEngine;

// 애니메이션이 있는 모델(MC11)에 붙이는 중계 스크립트
public class AnimationRelay : MonoBehaviour
{
    private RangedAttack rangedAttack;

    void Start()
    {
        // 부모 쪽으로 올라가며 RangedAttack 찾기
        rangedAttack = GetComponentInParent<RangedAttack>();
    }

    // 애니메이션 이벤트가 호출 → 부모의 FireArrow 실행
    public void FireArrow()
    {
        if (rangedAttack != null)
        {
            rangedAttack.FireArrow();
        }
    }
}