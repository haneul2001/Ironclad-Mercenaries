using UnityEngine;
using System.Collections.Generic;

// abstract = 이 클래스 자체로는 못 쓰고, 상속받아서만 사용
public abstract class UnitAttack : MonoBehaviour
{
    [Header("공통 공격 스탯")]
    public float attackRange = 3f;
    public int attackDamage = 10;
    public float attackInterval = 1f;

    private float attackTimer = 0f;

    [Header("빌드 스킬 (해금/확률)")]
    public bool skill02Unlocked = false;   // 2단계 스킬 해금 여부
    public bool skill03Unlocked = false;   // 3단계 스킬 해금 여부
    public float skill02Chance = 0.3f;     // 2단계 발동 확률 (30%)
    public float skill03Chance = 0.1f;     // 3단계 발동 확률 (10%)

    // ── 강화 레벨 저장 ──
    // "이 종류의 강화를 몇 번 했는가"를 종류(UpgradeType)별로 기억.
    // 다음 비용 계산(GetCost)과 최대 레벨 체크에 사용.
    private Dictionary<UpgradeType, int> upgradeLevels = new Dictionary<UpgradeType, int>();

    // 특정 종류를 현재 몇 레벨 올렸는지 조회 (없으면 0)
    public int GetUpgradeLevel(UpgradeType type)
    {
        return upgradeLevels.TryGetValue(type, out int lv) ? lv : 0;
    }

    void Update()
    {
        attackTimer += Time.deltaTime;

        if (attackTimer >= attackInterval)
        {
            EnemyHealth target = FindNearestEnemy();
            if (target != null)
            {
                PerformAttack(target);
                attackTimer = 0f;
            }
        }
    }

    // 사거리 안 가장 가까운 적 찾기 (공통 로직)
    protected virtual EnemyHealth FindNearestEnemy()
    {
        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
        EnemyHealth nearest = null;
        float minDistance = attackRange;

        foreach (EnemyHealth enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance <= minDistance)
            {
                minDistance = distance;
                nearest = enemy;
            }
        }
        return nearest;
    }

    // 자식이 반드시 구현해야 하는 "공격 방법"
    protected abstract void PerformAttack(EnemyHealth target);

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    // 공격 주기를 바꾸고, 관련 처리(애니메이션 속도 등)를 갱신
    public virtual void SetAttackInterval(float newInterval)
    {
        attackInterval = newInterval;
        OnAttackStatsChanged();
    }

    protected virtual void OnAttackStatsChanged() { }

    // 확률로 공격 단계 결정 (강한 스킬부터 독립 체크)
    protected int RollAttackLevel()
    {
        if (skill03Unlocked && Random.value < skill03Chance)
            return 3;
        if (skill02Unlocked && Random.value < skill02Chance)
            return 2;
        return 1;
    }

    // ─────────────────────────────────────────────
    //  강화 적용 (플레이어 UI든 AI든 이 함수 하나를 호출)
    // ─────────────────────────────────────────────

    // 강화를 실제로 적용. 성공하면 true.
    public bool ApplyUpgrade(UpgradeData data)
    {
        if (data == null) return false;

        int currentLv = GetUpgradeLevel(data.type);

        // 최대 레벨 체크 (maxLevel 0 이하면 무제한)
        if (data.maxLevel > 0 && currentLv >= data.maxLevel)
        {
            Debug.Log($"{name}: {data.type} 이미 최대 레벨({data.maxLevel})");
            return false;
        }

        float v = data.valuePerLevel;

        switch (data.type)
        {
            // ── 공통 (부모가 직접 처리) ──
            case UpgradeType.Skill2Chance:
                skill02Chance += v;
                break;
            case UpgradeType.Skill3Chance:
                skill03Chance += v;
                break;
            case UpgradeType.BaseAttack:
                // +2%p 씩: 현재값에 곱해 누적 (게임잼 기준 복리 허용)
                ApplyBaseAttackMultiplier(v);
                break;
            case UpgradeType.AttackSpeed:
            case UpgradeType.TraitAttackSpeed:
                // 공속 강화 = 공격 쿨타임(attackInterval)을 초 단위로 감소.
                // 두 타입 모두 interval을 깎음(값은 동일 처리). 타입이 달라 레벨만 따로 셈.
                float reduced = attackInterval - v;
                reduced = Mathf.Max(0.1f, reduced);      // 0.1초 밑으로는 안 내려감
                SetAttackInterval(reduced);
                break;

            // ── 직업별로 다른 부분 (자식이 override) ──
            case UpgradeType.Skill2Damage:
            case UpgradeType.Skill3Damage:
            case UpgradeType.Skill1Range:
            case UpgradeType.Skill2Range:
                bool handled = ApplyJobSpecificUpgrade(data.type, v);
                if (!handled)
                {
                    Debug.Log($"{name}: {data.type}는 이 직업에 해당 없음");
                    return false;
                }
                break;

            // ── 궁수 전용 디버프 (자식이 override) ──
            case UpgradeType.ArrowVulnerable:
            case UpgradeType.ArrowSlow:
                bool debuffHandled = ApplyJobSpecificUpgrade(data.type, v);
                if (!debuffHandled)
                {
                    Debug.Log($"{name}: {data.type}는 이 직업에 해당 없음");
                    return false;
                }
                break;

            default:
                Debug.LogWarning($"{name}: 처리되지 않은 강화 {data.type}");
                return false;
        }

        // 레벨 1 증가 기록 (타입별로 저장 → 같은 값 처리라도 타입 다르면 레벨 분리)
        upgradeLevels[data.type] = currentLv + 1;
        OnAttackStatsChanged();
        Debug.Log($"{name}: {data.type} 강화 → Lv.{currentLv + 1}");
        return true;
    }

    // 기본 공격력 배율 적용 (자식이 자기 데미지 변수에 반영하도록 override 가능)
    // 기본 구현: 부모의 attackDamage에 적용 (attackDamage를 데미지 기준으로 쓰는 경우)
    protected virtual void ApplyBaseAttackMultiplier(float percent)
    {
        attackDamage = Mathf.RoundToInt(attackDamage * (1f + percent));
    }

    // 직업별로 다른 강화(스킬 데미지/범위/디버프)를 자식이 처리.
    // 처리했으면 true, 해당 없으면 false 반환.
    protected virtual bool ApplyJobSpecificUpgrade(UpgradeType type, float value)
    {
        return false;   // 부모는 기본적으로 처리 안 함
    }
}