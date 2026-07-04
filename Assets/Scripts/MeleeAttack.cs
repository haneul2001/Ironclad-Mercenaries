using UnityEngine;
using System.Collections.Generic;

// UnitAttack을 상속받음
public class MeleeAttack : UnitAttack
{
    [Header("근접 광역 (사각 범위)")]
    public float rangeX = 2f;                   // 좌우 폭
    public float rangeZ = 3f;                   // 앞뒤 길이

    [Header("스킬별 데미지")]
    public int damage01 = 10;   // 스킬1 기본 사각 광역
    public int damage02 = 20;   // 스킬2 두 번 휘릭 (세게)
    public int damage03 = 15;   // 스킬3 검기 발사

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
                    newSpeed = clipLen / (attackInterval*0.9f); // 0.9배로 살짝 여유를 줘서 공격이 끝나기 전에 데미지 이벤트가 발생하도록
                }
            }
            anim.speed = newSpeed;

            anim.SetTrigger(trigger);
        }

        // 스킬1(기본)은 즉시 사각 데미지
        if (level == 1)
        {
            DoBoxDamage(damage01);
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

    // 스킬2: 두 번 휘릭할 때마다 호출 (애니메이션에 이벤트 2개)
    public void SlashDamage()
    {
        DoBoxDamage(damage02);
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
        if (proj != null) proj.Setup(dir, damage03);
    }
}

    protected override void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(rangeX, 1f, rangeZ));
    }
}