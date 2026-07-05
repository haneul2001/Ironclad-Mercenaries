using UnityEngine;

// 강화 하나의 "정의"를 담는 ScriptableObject.
// 실제 게임 오브젝트가 아니라 에셋 파일(.asset)로 존재하는 "데이터 덩어리".
//
// 왜 SO로 빼는가:
//  - 밸런싱(수치·비용)을 코드 수정 없이 Unity 인스펙터에서 조정 가능
//  - 강화 종류를 늘릴 때 .asset 파일만 하나 더 만들면 됨
//  - UI가 이 데이터를 읽어 이름·설명·아이콘을 그대로 표시
//
// 만드는 법(Unity): Project 창에서 우클릭 → Create → Ironclad → Upgrade Data

[CreateAssetMenu(fileName = "NewUpgrade", menuName = "Ironclad/Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    [Header("정체")]
    public UpgradeType type;              // 어떤 강화인지 (enum)
    public UpgradeCategory category;      // 특성 / 기본능력치

    [Header("표시 정보 (UI용)")]
    public string displayName;            // 예: "정밀 사격"
    [TextArea] public string description; // 예: "어택2 발동 확률이 0.1%p 증가합니다."
    public Sprite icon;                   // 버튼에 표시할 아이콘 (없어도 됨)

    [Header("수치")]
    // 한 레벨당 증가량. 종류에 따라 의미가 다름:
    //  - 확률(Skill2/3Chance): 0.001 = +0.1%p
    //  - 데미지(Skill2/3Damage): 0.2 = +20%
    //  - 범위(Skill1/2Range): 0.1 = +10%
    //  - 공격력(BaseAttack): 0.02 = +2%p
    //  - 공속(AttackSpeed): 0.1 = +10%
    public float valuePerLevel = 0.1f;

    // 이 강화를 최대 몇 레벨까지 올릴 수 있는지 (0 이하면 무제한)
    public int maxLevel = 5;

    [Header("비용")]
    // 능력치 강화는 레벨마다 비용이 오름(5,10,15,20,25).
    // 특성 강화는 진입 시 20원 고정이라 여기 비용은 안 써도 됨.
    // baseCost + costPerLevel * (현재레벨) 형태로 계산.
    public int baseCost = 5;              // 1레벨 강화 비용
    public int costPerLevel = 5;          // 레벨이 오를수록 추가되는 비용

    // 현재 레벨 기준 다음 강화 비용을 계산 (currentLevel은 0부터 시작)
    public int GetCost(int currentLevel)
    {
        return baseCost + costPerLevel * currentLevel;
    }
}