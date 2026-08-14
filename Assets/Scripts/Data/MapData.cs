using UnityEngine;

namespace CyberDefense.Data
{
    /// <summary>
    /// 선택 가능한 맵(스테이지) 하나의 정보를 정의하는 데이터 에셋입니다.
    /// Project 창에서 우클릭 -> Create -> CyberDefense -> Map Data 로 생성합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "NewMapData", menuName = "CyberDefense/Map Data")]
    public class MapData : ScriptableObject
    {
        [Header("기본 정보")]
        public string mapId;
        public string displayName = "맵 1";
        [TextArea] public string description;
        public Sprite thumbnail;

        [Header("씬 연결")]
        [Tooltip("이 맵을 선택했을 때 로드할 씬 이름. File > Build Settings에 등록된 씬 이름과 정확히 같아야 합니다.")]
        public string sceneName;

        [Header("잠금/난이도")]
        public bool isUnlocked = true;
        [Tooltip("이 맵 자체의 기본 난이도 배율(맵마다 기본적으로 더 어려울 수 있음). 난이도 선택과는 별개입니다.")]
        public float difficultyMultiplierBase = 1f;
    }
}
