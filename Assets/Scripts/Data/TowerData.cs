using UnityEngine;

namespace CyberDefense.Data
{
    public enum TowerType
    {
        Firewall,   // 방화벽 - 기본 단일 타겟 공격
        IDSIPS,     // 침입 탐지/차단 시스템 - 은신(제로데이) 탐지 특화
        Antivirus,  // 안티바이러스 - 광역 공격 (웜 확산 저지)
        Honeypot,   // 허니팟 - 적을 유인/지연시키는 디버프형
        WAF         // 웹 방화벽 - 랜섬웨어 등 고위협 대상 고데미지
    }

    /// <summary>
    /// 타워 종류별 기본 스펙을 정의하는 데이터 에셋입니다.
    /// Project 창에서 우클릭 -> Create -> CyberDefense -> Tower Data 로 생성합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "NewTowerData", menuName = "CyberDefense/Tower Data")]
    public class TowerData : ScriptableObject
    {
        [Header("기본 정보")]
        public TowerType towerType;
        public string displayName = "방화벽";
        [TextArea] public string description;
        public Sprite icon;
        public GameObject towerPrefab;

        [Header("전투 스탯")]
        public float damage = 10f;
        public float attackRange = 3f;
        public float attackCooldown = 1f;
        [Tooltip("true면 범위 안 모든 적을 동시에 공격 (안티바이러스 등)")]
        public bool isAreaAttack = false;
        public float areaRadius = 1.5f;
        [Tooltip("제로데이(은신) 적을 탐지할 수 있는지 여부")]
        public bool canDetectStealth = false;

        [Header("경제")]
        public int buildCost = 50;
        public int upgradeCost = 30;
        [Range(1, 5)] public int maxLevel = 3;
        [Tooltip("레벨업마다 곱해지는 데미지 배율")]
        public float upgradeDamageMultiplier = 1.3f;

        [Header("디버프 (허니팟 등)")]
        [Tooltip("true면 공격 시 데미지와 별개로 적 이동속도를 늦추는 슬로우 효과를 적용합니다.")]
        public bool appliesSlow = false;
        [Tooltip("슬로우 적용 시 남는 이동속도 비율 (0.4 = 원래 속도의 40%로 감소, 즉 60% 슬로우)")]
        [Range(0.1f, 1f)] public float slowMultiplier = 0.5f;
        [Tooltip("슬로우 효과 지속시간(초)")]
        public float slowDuration = 2f;

        [Header("이펙트")]
        [Tooltip("공격 시 레이저/범위 이펙트 색상 (타워 종류마다 다르게 지정)")]
        public Color effectColor = Color.cyan;
    }
}
