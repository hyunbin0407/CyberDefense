using UnityEngine;

namespace CyberDefense.Data
{
    /// <summary>
    /// 난이도 하나의 배율/보너스를 정의하는 데이터 에셋입니다.
    /// Project 창에서 우클릭 -> Create -> CyberDefense -> Difficulty Data 로 생성합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "NewDifficultyData", menuName = "CyberDefense/Difficulty Data")]
    public class DifficultyData : ScriptableObject
    {
        [Header("기본 정보")]
        public string difficultyId;
        public string displayName = "보통";
        [TextArea] public string description;

        [Header("배율")]
        [Tooltip("적 체력에 곱해지는 배율")]
        public float enemyHealthMultiplier = 1f;
        [Tooltip("적 이동속도에 곱해지는 배율")]
        public float enemySpeedMultiplier = 1f;
        [Tooltip("게임 시작 시 인게임 크레딧에 추가로 더해지는(음수면 차감되는) 보너스")]
        public int startingCurrencyBonus = 0;
    }
}
