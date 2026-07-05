using UnityEngine;

// 유닛마다 붙는 "지갑" 컴포넌트.
// 유닛별로 각자 예산을 가지므로(각 20원), 지갑을 유닛에 붙은 컴포넌트로 두는 게 자연스러움.
//
// 사용처:
//  - 정비소 진입 시 GrantBudget(20)으로 이번 날 예산 지급
//  - 강화 전에 CanAfford(cost)로 살 수 있는지 확인
//  - 강화 시 Spend(cost)로 차감
//  - AI 강화 루프는 "돈이 안 남을 때까지" = while(gold > 0) 형태로 이 값을 참조

public class UnitWallet : MonoBehaviour
{
    [Header("골드")]
    [SerializeField] private int gold = 0;   // 현재 보유 골드

    // 외부에서 읽기만 가능 (직접 조작 방지)
    public int Gold => gold;

    // 이번 정비 페이즈 예산 지급 (기존에 남은 골드에 더할지, 초기화할지 선택)
    public void GrantBudget(int amount, bool resetFirst = true)
    {
        if (resetFirst) gold = 0;
        gold += amount;
    }

    // 이 비용을 낼 수 있는가?
    public bool CanAfford(int cost)
    {
        return gold >= cost;
    }

    // 소비. 성공하면 true, 잔액 부족이면 false(차감 안 함).
    public bool Spend(int cost)
    {
        if (!CanAfford(cost)) return false;
        gold -= cost;
        return true;
    }

    // 도박 보상 등으로 골드를 얻을 때
    public void Add(int amount)
    {
        gold += amount;
    }
}