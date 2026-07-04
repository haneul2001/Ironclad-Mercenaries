using UnityEngine;

// 전사 모델에 붙이는 중계 스크립트
public class MeleeAnimationRelay : MonoBehaviour
{
    private MeleeAttack meleeAttack;

    void Start()
    {
        // 부모 쪽으로 올라가며 MeleeAttack 찾기
        meleeAttack = GetComponentInParent<MeleeAttack>();
    }

    // 스킬2: 두 번 휘릭할 때마다 호출 → 부모의 SlashDamage
    public void SlashDamage()
    {
        if (meleeAttack != null)
        {
            meleeAttack.SlashDamage();
        }
    }

    // 스킬3: 검기 발사 → 부모의 FireSlash
    public void FireSlash()
    {
        if (meleeAttack != null)
        {
            meleeAttack.FireSlash();
        }
    }
}