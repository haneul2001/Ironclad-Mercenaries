using UnityEngine;
using System.Collections.Generic;

public class RangedAttack : UnitAttack
{
    [Header("원거리 전용")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    [Header("트리거별 클립 이름")]
    public string clip01Name = "Attack01_BowAndArrow";
    public string clip02Name = "Attack02_BowAndArrow";
    public string clip03Name = "Attack03_BowAndArrow";

    private Animator anim;
    private EnemyHealth currentTarget;
    private Dictionary<string, float> clipLengths = new Dictionary<string, float>();

    void Start()
    {
        anim = GetComponentInChildren<Animator>();

        // 클립 길이 자동 저장
        if (anim != null && anim.runtimeAnimatorController != null)
        {
            foreach (AnimationClip clip in anim.runtimeAnimatorController.animationClips)
            {
                clipLengths[clip.name] = clip.length;
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
            // 이 클립이 attackInterval 안에 끝나도록 speed 조절
            if (clipLengths.ContainsKey(clipName) && attackInterval > 0f)
            {
                anim.speed = clipLengths[clipName] / attackInterval;
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
        if (proj != null) proj.Setup(dir, attackDamage);
    }

    protected override void OnAttackStatsChanged() { }
}