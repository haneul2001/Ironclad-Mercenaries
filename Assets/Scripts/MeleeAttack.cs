using UnityEngine;
using System.Collections.Generic;

// UnitAttack을 상속받음
public class MeleeAttack : UnitAttack
{
    [Header("근접 광역 (사각 범위)")]
    public float rangeX = 2f;                   // 좌우 폭
    public float rangeZ = 3f;                   // 앞뒤 길이

    [Header("스킬별 데미지 배수 (attackDamage 기준)")]
    public float skill1Mult = 1.0f;   // 어택1 = attackDamage × 1.0
    public float skill2Mult = 1.0f;   // 어택2 = attackDamage × 1.0 (2연타라 실질 2.0)
    public float skill3Mult = 2.5f;   // 어택3(검기) = attackDamage × 2.5

    [Header("이펙트")]
    public GameObject slashEffect;              // 스킬1 검기 이펙트
    public GameObject slashEffect02;            // 스킬2 휘릭 이펙트 (짧게)
    [Header("이펙트 배속")]
    public float effectSpeedMultiplier = 1f;   // 이펙트를 이만큼 더 빠르게
    [Header("검기 발사체 (스킬3)")]
    public GameObject slashProjectilePrefab;    // 검기 발사체
    public Transform firePoint;                 // 검기 발사 위치

    [Header("트리거별 클립 이름")]
    public string clip01Name = "Attack01_SwordAndShiled";
    public string clip02Name = "Attack04_SwordAndShiled";
    public string clip03Name = "Combo05_InPlaceWithRMHeight_SwordAndShield";

    private Animator anim;
    private int currentLevel = 1;   // 현재 공격 단계 (이벤트에서 참조)

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

    // 스킬 단계별 실제 데미지 = 기준 공격력 × 배수
    private int GetSkillDamage(float mult)
    {
        return Mathf.RoundToInt(attackDamage * mult);
    }

    // 부모의 원형 탐지를 사각형 탐지로 재정의
    protected override EnemyHealth FindNearestEnemy()
    {
        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
        EnemyHealth nearest = null;
        float minDistance = float.MaxValue;

        foreach (EnemyHealth enemy in enemies)
        {
            Vector3 diff = enemy.transform.position - transform.position;
            if (Mathf.Abs(diff.x) <= rangeX / 2f && Mathf.Abs(diff.z) <= rangeZ / 2f)
            {
                float distance = diff.magnitude;
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = enemy;
                }
            }
        }
        return nearest;
    }

    protected override void PerformAttack(EnemyHealth target)
    {
        int level = RollAttackLevel();
        currentLevel = level;

        string trigger, clipName;
        switch (level)
        {
            case 3: trigger = "Attack03";  clipName = clip03Name; break;
            case 2: trigger = "Attack02";  clipName = clip02Name; break;
            default: trigger = "Attack01"; clipName = clip01Name; break;
        }

        if (anim != null)
        {
            // speed를 매번 명확히 설정 (이전 값이 남지 않게)
            float newSpeed = 1f;
            if (clipLengths.ContainsKey(clipName) && attackInterval > 0f)
            {
                float clipLen = clipLengths[clipName];
                if (clipLen > attackInterval)   // 긴 클립만 빠르게
                {
                    newSpeed = clipLen / (attackInterval * 0.9f); // 0.9배로 살짝 여유
                }
            }
            anim.speed = newSpeed;

            anim.SetTrigger(trigger);
        }

        // 스킬1(기본)은 즉시 사각 데미지
        if (level == 1)
        {
            DoBoxDamage(GetSkillDamage(skill1Mult));
            SpawnEffect(slashEffect, 2f);
        }
    }

    // 사각 범위 데미지 (공통 로직, 데미지는 인자로)
    private void DoBoxDamage(int damage)
    {
        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
        foreach (EnemyHealth enemy in enemies)
        {
            Vector3 diff = enemy.transform.position - transform.position;
            if (Mathf.Abs(diff.x) <= rangeX / 2f && Mathf.Abs(diff.z) <= rangeZ / 2f)
            {
                enemy.TakeDamage(damage);
            }
        }
    }

    // 이펙트 생성 + 파티클 속도를 애니메이션 속도에 연동
    private void SpawnEffect(GameObject effectPrefab, float baseLifeTime)
    {
        if (effectPrefab == null) return;

        GameObject fx = Instantiate(effectPrefab, transform.position, Quaternion.Euler(0, 180, 0));

        float spd = (anim != null && anim.speed > 0f) ? anim.speed : 1f;
        spd *= effectSpeedMultiplier;   // ★ 추가 배속

        // 파티클 재생 속도
        ParticleSystem[] systems = fx.GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in systems)
        {
            var main = ps.main;
            main.simulationSpeed = spd;
        }

        // 빠른 만큼 빨리 사라지게
        Destroy(fx, baseLifeTime / spd);
    }

    // ───────── 애니메이션 이벤트 함수 ─────────

    // 스킬2: 두 번 휘릭할 때마다 호출 (애니메이션에 이벤트 2개 → 총 2배)
    public void SlashDamage()
    {
        DoBoxDamage(GetSkillDamage(skill2Mult));
        SpawnEffect(slashEffect02, 1f);   // 짧게 (속도 연동됨)
    }

    // 스킬3: 검기 발사 (애니메이션 이벤트로 타이밍 지정)
    public void FireSlash()
    {
        EnemyHealth target = FindNearestEnemy();
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;

        Vector3 dir;
        if (target != null)
        {
            dir = target.transform.position - spawnPos;
            dir.y = 0;
            dir = dir.normalized;
        }
        else
        {
            dir = transform.forward;   // 적 없으면 정면
            dir.y = 0;
            dir = dir.normalized;
        }

        if (slashProjectilePrefab != null)
        {
            GameObject slash = Instantiate(slashProjectilePrefab, spawnPos, Quaternion.identity);
            Projectile proj = slash.GetComponent<Projectile>();
            // 전사 검기는 디버프 없음 → 2개짜리 Setup 사용 (디버프 0)
            if (proj != null) proj.Setup(dir, GetSkillDamage(skill3Mult));
        }
    }

    // 전사 전용 강화 처리 (스킬 데미지 배수 / 범위)
    protected override bool ApplyJobSpecificUpgrade(UpgradeType type, float value)
    {
        switch (type)
        {
            case UpgradeType.Skill2Damage:
                skill2Mult += value;   // 배수를 올림 (예: 1.0 → 1.2)
                return true;
            case UpgradeType.Skill3Damage:
                skill3Mult += value;   // 예: 2.5 → 2.7
                return true;
            case UpgradeType.Skill1Range:
            case UpgradeType.Skill2Range:
                rangeX *= (1f + value);   // 예: 0.1 = +10%
                rangeZ *= (1f + value);
                return true;
            default:
                return false;
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(rangeX, 1f, rangeZ));
    }
    // 전사: attackDamage × 배수 (스킬2는 2연타라 ×2)
    public override int GetDisplayDamage(int skillLevel)
    {
        switch (skillLevel)
        {
            case 3: return GetSkillDamage(skill3Mult);
            case 2: return GetSkillDamage(skill2Mult) * 2;
            default: return GetSkillDamage(skill1Mult);
        }
    }
}