using UnityEngine;
using System.Collections.Generic;

// 정비소 전체를 총괄하는 매니저.
// 역할: "강화를 실행하는 손" — 예산 지급, 특성 진입/선택, 능력치 구매, 조회.
// "무엇을 강화할지 판단하는 두뇌"는 IUpgradeStrategy(AI) 또는 플레이어 UI가 담당.
public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    [Header("유닛 (인스펙터에서 드래그)")]
    public UnitAttack archer;    // 궁수 (RangedAttack)
    public UnitAttack warrior;   // 전사 (MeleeAttack)
    public UnitAttack mage;      // 마법사 (AreaAttack)

    [Header("주인공 (플레이어가 강화하는 유닛)")]
    public UnitAttack hero;      // 시작 시 선택. 임시로 인스펙터 지정 가능.

    [Header("예산 / 비용")]
    public int budgetPerUnit = 20;   // 정비소 진입 시 유닛마다 지급
    public int traitEntryCost = 20;  // 특성 강화 진입료
     public int budgetAddPerDay = 5;   // Day마다 예산 추가 (Day1=기본, Day2=+5...)

    [Header("강화 정의 목록 (SO)")]
    public List<UpgradeData> commonTraits = new List<UpgradeData>();  // 전사·마법사 공용 특성
    public List<UpgradeData> archerTraits = new List<UpgradeData>();  // 궁수 전용 특성
    public List<UpgradeData> baseStats = new List<UpgradeData>();     // 기본 능력치 (공격력, 공속)

    // AI 강화 전략 (지금은 규칙 기반. 나중에 LLM 구현체로 교체 가능)
    private IUpgradeStrategy aiStrategy = new RandomUpgradeStrategy();

    void Awake()
    {
        Instance = this;
    }

    // ─────────────────────────────────────────────
    //  내부 헬퍼
    // ─────────────────────────────────────────────

    // 유닛에 붙은 지갑 가져오기 (없으면 자동 추가)
    private UnitWallet GetWallet(UnitAttack unit)
    {
        if (unit == null) return null;
        UnitWallet w = unit.GetComponent<UnitWallet>();
        if (w == null) w = unit.gameObject.AddComponent<UnitWallet>();
        return w;
    }

    // 이 유닛이 궁수인지 (특성 목록 구분용)
    private bool IsArcher(UnitAttack unit)
    {
        return unit is RangedAttack;
    }

    // 이 유닛이 받을 수 있는 특성 목록
    private List<UpgradeData> GetTraitPool(UnitAttack unit)
    {
        return IsArcher(unit) ? archerTraits : commonTraits;
    }

    // ─────────────────────────────────────────────
    //  1) 정비소 진입 시: 모든 유닛에 예산 지급
    // ─────────────────────────────────────────────
     public void GrantBudgets()
    {
        // Day에 따라 예산 증가 (Day1이면 기본, Day2면 +5, ...)
        int day = (GameManager.Instance != null) ? GameManager.Instance.currentDay : 1;
        int budget = budgetPerUnit + budgetAddPerDay * (day - 1);

        GetWallet(archer)?.GrantBudget(budget);
        GetWallet(warrior)?.GrantBudget(budget);
        GetWallet(mage)?.GrantBudget(budget);
        Debug.Log($"[정비소] Day {day} - 각 유닛에 {budget}원 지급");
    }

    // ─────────────────────────────────────────────
    //  2) 특성 강화 진입 (20원 차감 + 3종 랜덤 뽑기)
    //     성공하면 3종 목록 반환, 실패(돈 부족)하면 null
    // ─────────────────────────────────────────────
    public List<UpgradeData> EnterTraitUpgrade(UnitAttack unit, int count = 3)
    {
        if (unit == null) return null;
        UnitWallet wallet = GetWallet(unit);

        if (!wallet.CanAfford(traitEntryCost))
        {
            Debug.Log($"{unit.name}: 특성 진입료({traitEntryCost}원) 부족 (보유 {wallet.Gold}원)");
            return null;
        }

        wallet.Spend(traitEntryCost);   // 진입료 차감

        // 받을 수 있는 특성 중 만렙 아닌 것만 후보로
        List<UpgradeData> pool = new List<UpgradeData>();
        foreach (UpgradeData d in GetTraitPool(unit))
        {
            int lv = unit.GetUpgradeLevel(d.type);
            if (d.maxLevel > 0 && lv >= d.maxLevel) continue;
            pool.Add(d);
        }

        // 중복 없이 count개 뽑기
        List<UpgradeData> result = new List<UpgradeData>();
        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            int idx = Random.Range(0, pool.Count);
            result.Add(pool[idx]);
            pool.RemoveAt(idx);
        }

        Debug.Log($"{unit.name}: 특성 진입 (-{traitEntryCost}원), {result.Count}종 제시");
        return result;
    }

    // ─────────────────────────────────────────────
    //  3) 특성 선택 적용 (진입 후 3종 중 하나, 비용 0)
    // ─────────────────────────────────────────────
    public bool SelectTrait(UnitAttack unit, UpgradeData data)
    {
        if (unit == null || data == null) return false;
        bool applied = unit.ApplyUpgrade(data);   // 진입료는 이미 냈으니 무료 적용
        if (applied)
            Debug.Log($"{unit.name}: 특성 '{data.displayName}' 선택 적용");
        return applied;
    }

    // ─────────────────────────────────────────────
    //  4) 능력치 강화 구매 (개별 비용, 레벨마다 오름)
    // ─────────────────────────────────────────────
    public bool TryPurchaseBaseStat(UnitAttack unit, UpgradeData data)
    {
        if (unit == null || data == null) return false;

        UnitWallet wallet = GetWallet(unit);
        int currentLv = unit.GetUpgradeLevel(data.type);
        int cost = data.GetCost(currentLv);

        if (data.maxLevel > 0 && currentLv >= data.maxLevel)
        {
            Debug.Log($"{unit.name}: {data.displayName} 최대 레벨");
            return false;
        }
        if (!wallet.CanAfford(cost))
        {
            Debug.Log($"{unit.name}: {data.displayName} 골드 부족 ({wallet.Gold}/{cost})");
            return false;
        }

        bool applied = unit.ApplyUpgrade(data);
        if (!applied) return false;

        wallet.Spend(cost);
        Debug.Log($"{unit.name}: {data.displayName} 구매 (-{cost}원, 남은 {wallet.Gold}원)");
        return true;
    }

    // ─────────────────────────────────────────────
    //  5) AI 자동 강화 (주인공 제외). 판단은 전략에게 위임.
    // ─────────────────────────────────────────────
    public void RunAIUpgrades()
    {
        UnitAttack[] all = { archer, warrior, mage };
        foreach (UnitAttack unit in all)
        {
            if (unit == null || unit == hero) continue;
            aiStrategy.UpgradeUnit(unit, this);
        }
    }

    // ─────────────────────────────────────────────
    //  전략(AI)이 사용하는 조회용 함수들
    // ─────────────────────────────────────────────

    public int GetUnitGold(UnitAttack unit)
    {
        UnitWallet w = GetWallet(unit);
        return w != null ? w.Gold : 0;
    }

    public bool CanEnterTrait(UnitAttack unit)
    {
        UnitWallet w = GetWallet(unit);
        return w != null && w.CanAfford(traitEntryCost);
    }

    // 지금 살 수 있는 능력치 강화 목록 (만렙·잔액 필터)
    public List<UpgradeData> GetAffordableBaseStats(UnitAttack unit)
    {
        List<UpgradeData> result = new List<UpgradeData>();
        UnitWallet w = GetWallet(unit);
        if (w == null) return result;

        foreach (UpgradeData d in baseStats)
        {
            int lv = unit.GetUpgradeLevel(d.type);
            if (d.maxLevel > 0 && lv >= d.maxLevel) continue;
            if (w.CanAfford(d.GetCost(lv)))
                result.Add(d);
        }
        return result;
    }

    [Header("도박")]
    public int gambleCost = 5;

    public struct GambleResult
    {
        public bool success;
        public int dice;
        public int reward;
    }

    public GambleResult TryGamble()
    {
        GambleResult result = new GambleResult();

        UnitAttack h = hero;
        if (h == null) { result.success = false; return result; }

        UnitWallet wallet = GetWallet(h);

        if (!wallet.CanAfford(gambleCost))
        {
            result.success = false;
            Debug.Log($"도박 골드 부족 (보유 {wallet.Gold}/{gambleCost})");
            return result;
        }

        wallet.Spend(gambleCost);

        int dice = Random.Range(1, 7);   // 1~6
        int reward = 0;
        switch (dice)
        {
            case 4: reward = 10; break;
            case 5: reward = 15; break;
            case 6: reward = 20; break;
            default: reward = 0; break;
        }

        if (reward > 0) wallet.Add(reward);

        result.success = true;
        result.dice = dice;
        result.reward = reward;

        Debug.Log($"도박: 주사위 {dice} → {(reward > 0 ? reward + "원 획득" : "꽝")}");
        return result;
    }


      
}