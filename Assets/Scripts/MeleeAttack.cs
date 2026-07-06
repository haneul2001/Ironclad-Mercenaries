using UnityEngine;
using System.Collections.Generic;

// UnitAttack을 상속받음
public class MeleeAttack : UnitAttack
{
    public AudioSource audioSource;
    public AudioClip slashClip1;
    public AudioClip slashClip2;
    public AudioClip slashClip3;

    [Header("근접 광역 (사각 범위)")]
    public float rangeX = 2f;
    public float rangeZ = 3f;

    [Header("스킬별 데미지 배수 (attackDamage 기준)")]
    public float skill1Mult = 1.0f;
    public float skill2Mult = 1.0f;
    public float skill3Mult = 2.5f;

    [Header("이펙트")]
    public GameObject slashEffect;
    public GameObject slashEffect02;
    [Header("이펙트 배속")]
    public float effectSpeedMultiplier = 1f;
    [Header("이펙트 기준 크기")]
    public float effectBaseRange = 3f;   // 이펙트 Scale 1일 때 기준 범위(rangeZ 기준)

    [Header("검기 발사체 (스킬3)")]
    public GameObject slashProjectilePrefab;
    public Transform firePoint;

    [Header("트리거별 클립 이름")]
    public string clip01Name = "Attack01_SwordAndShiled";
    public string clip02Name = "Attack04_SwordAndShiled";
    public string clip03Name = "Combo05_InPlaceWithRMHeight_SwordAndShield";

    private Animator anim;
    private int currentLevel = 1;

    private Dictionary<string, float> clipLengths = new Dictionary<string, float>();

    void Start()
    {
        anim = GetComponentInChildren<Animator>();

        if (anim != null && anim.runtimeAnimatorController != null)
        {
            foreach (AnimationClip clip in anim.runtimeAnimatorController.animationClips)
            {
                clipLengths[clip.name] = clip.length;
            }
        }
    }

    private int GetSkillDamage(float mult)
    {
        return Mathf.RoundToInt(attackDamage * mult);
    }

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
            float newSpeed = 1f;
            if (clipLengths.ContainsKey(clipName) && attackInterval > 0f)
            {
                float clipLen = clipLengths[clipName];
                if (clipLen > attackInterval)
                {
                    newSpeed = clipLen / (attackInterval * 0.9f);
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
            if (audioSource != null && slashClip1 != null)
                audioSource.PlayOneShot(slashClip1);
        }
    }

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

    // 이펙트 생성 + 범위에 맞춰 크기 조절 + 파티클 속도 연동
    private void SpawnEffect(GameObject effectPrefab, float baseLifeTime)
    {
        if (effectPrefab == null) return;

        GameObject fx = Instantiate(effectPrefab, transform.position, Quaternion.Euler(0, 180, 0));

        float spd = (anim != null && anim.speed > 0f) ? anim.speed : 1f;
        spd *= effectSpeedMultiplier;

        // 범위에 맞춰 이펙트 크기 조절 (범위 강화하면 이펙트도 커짐)
        float scaleFactor = (effectBaseRange > 0f) ? rangeZ / effectBaseRange : 1f;
        fx.transform.localScale = Vector3.one * scaleFactor;

        ParticleSystem[] systems = fx.GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in systems)
        {
            var main = ps.main;
            main.simulationSpeed = spd;
        }

        Destroy(fx, baseLifeTime / spd);
    }

    // ───────── 애니메이션 이벤트 함수 ─────────

    public void SlashDamage()
    {
        DoBoxDamage(GetSkillDamage(skill2Mult));
        SpawnEffect(slashEffect02, 1f);
        if (audioSource != null && slashClip2 != null)
            audioSource.PlayOneShot(slashClip2);
    }

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
            dir = transform.forward;
            dir.y = 0;
            dir = dir.normalized;
        }

        if (slashProjectilePrefab != null)
        {
            GameObject slash = Instantiate(slashProjectilePrefab, spawnPos, Quaternion.identity);
            Projectile proj = slash.GetComponent<Projectile>();
            if (proj != null) proj.Setup(dir, GetSkillDamage(skill3Mult));
        }
        if (audioSource != null && slashClip3 != null)
            audioSource.PlayOneShot(slashClip3);
    }

    protected override bool ApplyJobSpecificUpgrade(UpgradeType type, float value)
    {
        switch (type)
        {
            case UpgradeType.Skill2Damage:
                skill2Mult += value;
                return true;
            case UpgradeType.Skill3Damage:
                skill3Mult += value;
                return true;
            case UpgradeType.Skill1Range:
            case UpgradeType.Skill2Range:
                rangeX *= (1f + value);
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