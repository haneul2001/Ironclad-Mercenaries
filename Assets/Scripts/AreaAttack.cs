using UnityEngine;
using System.Collections.Generic;

public class AreaAttack : UnitAttack
{
    [Header("스킬별 폭발 반경")]
    public float radius01 = 1.5f;   // 스킬1 기본
    public float radius02 = 2.5f;   // 스킬2 큰 장판
    public float radius03 = 3.5f;   // 스킬3 광역기

    [Header("스킬별 데미지")]
    public int damage01 = 10;
    public int damage02 = 18;
    public int damage03 = 25;

    [Header("이펙트")]
    public GameObject explosionEffect;    // 스킬1
    public GameObject explosionEffect02;  // 스킬2
    public GameObject explosionEffect03;  // 스킬3
    public float effectBaseRadius = 1f;   // 이펙트 원본 Scale 1일 때 반경

    [Header("트리거별 클립 이름")]
    public string clip01Name = "Attack01_Magician";
    public string clip02Name = "Attack02_Magician";
    public string clip03aName = "Attack03_Charging_Magician";  // 스킬3 기모으기
    public string clip03bName = "Attack03_Fire_Magician";    // 스킬3 발사

    private Animator anim;
    private EnemyHealth currentTarget;
    private int currentLevel = 1;
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
        currentLevel = level;

        string trigger;
        float clipLen;   // 이번 공격의 애니메이션 총 길이

        switch (level)
        {
            case 3:
                trigger = "Attack03";
                // 스킬3은 기모으기 + 발사 두 클립 길이 합
                float chargeLen = clipLengths.ContainsKey(clip03aName) ? clipLengths[clip03aName] : 0f;
                float fireLen = clipLengths.ContainsKey(clip03bName) ? clipLengths[clip03bName] : 0f;
                clipLen = chargeLen + fireLen;
                break;
            case 2:
                trigger = "Attack02";
                clipLen = clipLengths.ContainsKey(clip02Name) ? clipLengths[clip02Name] : 0f;
                break;
            default:
                trigger = "Attack01";
                clipLen = clipLengths.ContainsKey(clip01Name) ? clipLengths[clip01Name] : 0f;
                break;
        }

        if (anim != null)
        {
            // 총 길이가 attackInterval 안에 끝나도록 speed 조절
            if (clipLen > 0f && attackInterval > 0f)
            {
                anim.speed = clipLen / attackInterval;
            }
            else
            {
                anim.speed = 1f;
            }
            anim.SetTrigger(trigger);
        }
    }

    // 폭발 실행 (현재 스킬 단계 기준 반경·데미지·이펙트)
    private void DoExplosion()
    {
        float radius;
        int damage;
        GameObject effect;
        switch (currentLevel)
        {
            case 3: radius = radius03; damage = damage03; effect = explosionEffect03; break;
            case 2: radius = radius02; damage = damage02; effect = explosionEffect02; break;
            default: radius = radius01; damage = damage01; effect = explosionEffect; break;
        }

        // 타겟 죽었으면 재탐색
        if (currentTarget == null)
        {
            currentTarget = FindNearestEnemy();
        }
        if (currentTarget == null) return;   // 적 없으면 폭발 안 함

        Vector3 explosionCenter = currentTarget.transform.position;

        // 이펙트 생성 (반경에 맞춰 크기)
        if (effect != null)
        {
            GameObject fx = Instantiate(effect, explosionCenter, Quaternion.identity);
            float scaleFactor = radius / effectBaseRadius;
            fx.transform.localScale = Vector3.one * scaleFactor;
            Destroy(fx, 2f);
        }

        // 반경 안 적에게 데미지
        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
        foreach (EnemyHealth enemy in enemies)
        {
            float distance = Vector3.Distance(explosionCenter, enemy.transform.position);
            if (distance <= radius)
            {
                enemy.TakeDamage(damage);
            }
        }
    }

    // 애니메이션 이벤트가 호출 → 폭발 (타이밍은 클립에서 지정)
    public void Explode()
    {
        DoExplosion();
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius01);
    }
}