using UnityEngine;
using System.Collections.Generic;

public class RangedAttack : UnitAttack
{
    [Header("원거리 전용")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    [Header("트리거별 클립 이름")]
    public string clip01Name = "Attack01_Archer";
    public string clip02Name = "Attack02_Archer";
    public string clip03Name = "Attack03_Archer";

    private Animator anim;
    private EnemyHealth currentTarget;
    private Dictionary<string, float> clipLengths = new Dictionary<string, float>();
        // ── 궁수 디버프 값 (강화로 쌓임, 화살에 실려 나감) ──
    private float arrowVulnerable = 0f;   // 취약 부여량
    private float arrowSlow = 0f;         // 둔화 부여량

   void Start()
{
    anim = GetComponentInChildren<Animator>();

    if (anim != null && anim.runtimeAnimatorController != null)
    {
        foreach (AnimationClip clip in anim.runtimeAnimatorController.animationClips)
        {
            clipLengths[clip.name] = clip.length;
            // Debug.Log("저장된 클립 이름: [" + clip.name + "]");   // ← 임시
        }
    }
}

    protected override void PerformAttack(EnemyHealth target)
{
    currentTarget = target;
    int level = RollAttackLevel();

    string trigger, clipName;
    switch (level)
    {
        case 3: trigger = "Attack03"; clipName = clip03Name; break;
        case 2: trigger = "Attack02"; clipName = clip02Name; break;
        default: trigger = "Attack01"; clipName = clip01Name; break;
    }

    if (anim != null)
    {
        // 제한 없이 순수 비례 (interval에 맞춰 그대로 빨라짐)
        if (clipLengths.ContainsKey(clipName) && attackInterval > 0f)
        {
            anim.speed = clipLengths[clipName] / attackInterval;
        }
        else
        {
            anim.speed = 1f;
        }
        anim.SetTrigger(trigger);
    }
}

    // 애니메이션 이벤트가 호출 → 한 발 발사
    public void FireArrow()
    {
        // 타겟이 죽었을 때만 다시 탐색 (성능 최적화)
        if (currentTarget == null)
        {
            currentTarget = FindNearestEnemy();
        }
        if (currentTarget == null) return;   // 살아있는 적 없으면 발사 안 함

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        Vector3 dir = currentTarget.transform.position - spawnPos;
        dir.y = 0;
        dir = dir.normalized;

        GameObject arrow = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        Projectile proj = arrow.GetComponent<Projectile>();
         if (proj != null) proj.Setup(dir, attackDamage, arrowVulnerable, arrowSlow);
    }

    protected override void OnAttackStatsChanged() { }

    protected override bool ApplyJobSpecificUpgrade(UpgradeType type, float value)
    {
        switch (type)
        {
            case UpgradeType.ArrowVulnerable:
                arrowVulnerable += value;   // 예: 0.02 씩 (레벨별 값과 일치)
                return true;
            case UpgradeType.ArrowSlow:
                arrowSlow += value;
                return true;
            default:
                return false;   // 궁수는 스킬데미지·범위 강화 없음
        }
    }
     // 궁수: 스킬1=1발, 스킬2=3발, 스킬3=5발
    public override int GetDisplayDamage(int skillLevel)
    {
        int arrows = (skillLevel == 3) ? 5 : (skillLevel == 2) ? 3 : 1;
        return attackDamage * arrows;
    }
}