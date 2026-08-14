using CyberDefense.Data;

namespace CyberDefense.Core
{
    /// <summary>
    /// 맵 선택 화면에서 고른 맵/난이도를 실제 게임 씬으로 전달하기 위한 정적 저장소입니다.
    /// MonoBehaviour가 아니라 정적(static) 클래스이므로, 씬이 바뀌어도 별도의
    /// DontDestroyOnLoad 없이 값이 그대로 유지됩니다.
    /// </summary>
    public static class GameSessionSettings
    {
        /// <summary>맵 선택 화면에서 고른 맵. 씬을 바로 실행한 경우(테스트 등) null일 수 있습니다.</summary>
        public static MapData SelectedMap { get; set; }

        /// <summary>맵 선택 화면에서 고른 난이도. 씬을 바로 실행한 경우(테스트 등) null일 수 있습니다.</summary>
        public static DifficultyData SelectedDifficulty { get; set; }
    }
}
