using System.Collections.Generic;
using UnityEngine;

// 규칙 기반 강화 전략: 돈이 떨어질 때까지 살 수 있는 것 중 랜덤으로 강화.
// 특성(진입료 방식)과 능력치(개별 비용)를 섞어서 선택. 도박은 제외.
public class RandomUpgradeStrategy : IUpgradeStrategy
{
    public void UpgradeUnit(UnitAttack unit, UpgradeManager manager)
    {
        if (unit == null || manager == null) return;

        int safety = 100;   // 무한루프 방지
        while (safety-- > 0)
        {
            int gold = manager.GetUnitGold(unit);
            if (gold <= 0) break;

            // 이번 턴에 가능한 선택지 구성
            bool canTrait = manager.CanEnterTrait(unit);
            List<UpgradeData> affordableStats = manager.GetAffordableBaseStats(unit);

            // "trait" 1표 + 능력치 각 1표를 후보로 (둘 다 없으면 종료)
            List<string> options = new List<string>();
            if (canTrait) options.Add("trait");
            foreach (var _ in affordableStats) options.Add("stat");

            if (options.Count == 0) break;

            string pick = options[Random.Range(0, options.Count)];
            bool didSomething = false;

            if (pick == "trait")
            {
                List<UpgradeData> choices = manager.EnterTraitUpgrade(unit);
                if (choices != null && choices.Count > 0)
                {
                    UpgradeData chosen = choices[Random.Range(0, choices.Count)];
                    manager.SelectTrait(unit, chosen);
                    didSomething = true;
                }
            }
            else // stat
            {
                UpgradeData d = affordableStats[Random.Range(0, affordableStats.Count)];
                if (manager.TryPurchaseBaseStat(unit, d))
                    didSomething = true;
            }

            if (!didSomething) break;
        }

        Debug.Log($"[AI:Random] {unit.name} 강화 종료 (남은 {manager.GetUnitGold(unit)}원)");
    }
}