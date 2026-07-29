using UnityEngine;

namespace CyberDefense.Data
{
    public enum EnemyType
    {
        Worm,       // 웜 - 빠르지만 약함, 다수 등장
        Ransomware, // 랜섬웨어 - 체력 높고 느림, 고위협
        ZeroDay,    // 제로데이 - 은신(탐지 타워 필요), 중간 속도
        DDoSBot     // DDoS 봇 - 매우 빠르게 몰려옴, 낮은 개별 체력
    }

    /// <summary>
    /// 적(악성코드) 종류별 기본 스펙을 정의하는 데이터 에셋입니다.
    /// Project 창에서 우클릭 -> Create -> CyberDefense -> Enemy Data 로 생성합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "NewEnemyData", menuName = "CyberDefense/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [Header("기본 정보")]
        public EnemyType enemyType;
        public string displayName = "웜";
        public Sprite icon;
        public GameObject enemyPrefab;

        [Header("스탯")]
        public float maxHealth = 30f;
        public float moveSpeed = 2f;
        [Tooltip("서버(플레이어 기지)에 도달했을 때 깎는 서버 체력")]
        public int damageToServer = 1;
        [Tooltip("처치 시 지급되는 재화(사이버 크레딧)")]
        public int rewardOnKill = 5;

        [Header("특수 속성")]
        [Tooltip("true면 탐지 능력이 있는 타워만 이 적을 공격할 수 있습니다.")]
        public bool isStealth = false;
        [Tooltip("일반 공격에 대한 데미지 감소율 (0~1). 랜섬웨어 등 방어형에 사용")]
        [Range(0f, 0.9f)] public float damageResistance = 0f;
    }
}
