// 강화의 "종류"를 정의하는 enum.
// 문자열 대신 enum을 쓰는 이유: 오타로 인한 버그를 막고, switch로 안전하게 분기하기 위함.
// 강화 종류를 늘리고 싶으면 여기에 항목을 추가하고, UnitAttack.ApplyUpgrade의 switch에만 처리를 더하면 됨.

public enum UpgradeType
{
    // ── 특성 강화 (공용: 전사·마법사) ──
    Skill2Chance,   // 어택2 발동 확률 증가
    Skill3Chance,   // 어택3 발동 확률 증가
    Skill2Damage,   // 어택2 데미지 증가 (+% 배수)
    Skill3Damage,   // 어택3 데미지 증가 (+% 배수)
    Skill1Range,    // 어택1 공격범위 증가 (+%)
    Skill2Range,    // 어택2 공격범위 증가 (+%)

    // ── 기본 능력치 강화 (공용) ──
    BaseAttack,     // 기본 공격력 증가 (+% 배수)
    AttackSpeed,    // 공격 쿨타임 감소 (초 단위)

    // ── 궁수 전용 특성 ──
    ArrowVulnerable,    // 화살 맞은 적이 받는 데미지 증가 (취약)
    ArrowSlow,          // 화살 맞은 적 이동속도 감소 (둔화)
    TraitAttackSpeed    // 특성 탭 공속 대폭 감소 (능력치 공속과 레벨 분리)
}

// 강화가 어느 "범주"에 속하는지. UI 패널을 나누거나, 도박 보상권을 구분할 때 사용.
public enum UpgradeCategory
{
    Trait,      // 특성 강화
    BaseStat    // 기본 능력치 강화
}