// 강화 판단 "전략"의 계약(인터페이스).
// 매니저는 이 인터페이스만 알고, 실제 판단이 규칙 기반인지 LLM인지는 신경 쓰지 않음.
// 지금은 RandomUpgradeStrategy(규칙 기반)가 구현.
// 나중에 LLMUpgradeStrategy(제미나이 등)를 만들면 매니저 코드 변경 없이 교체 가능.

public interface IUpgradeStrategy
{
    // 주어진 유닛을 (예산 안에서) 강화하라.
    // manager를 통해 실제 강화 실행 함수(특성 진입/선택, 능력치 구매)를 호출한다.
    void UpgradeUnit(UnitAttack unit, UpgradeManager manager);
}