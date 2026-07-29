using System;
using UnityEngine;

namespace CyberDefense.Core
{
    /// <summary>
    /// 게임 전체 상태(진행중/승리/패배/일시정지)를 관리하는 싱글톤 매니저입니다.
    /// 씬에 빈 GameObject를 만들고 이 스크립트를 붙여서 사용하세요.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public enum GameState
        {
            Prepare,   // 웨이브 시작 전 준비 단계 (타워 배치 가능)
            Playing,   // 웨이브 진행 중
            Paused,    // 일시정지
            Victory,   // 모든 웨이브 클리어
            Defeat     // 서버 라이프(HP) 소진
        }

        [Header("게임 설정")]
        [Tooltip("서버(플레이어 기지)의 시작 체력, 0이 되면 패배 처리됩니다.")]
        [SerializeField] private int startingServerHealth = 20;

        public int ServerHealth { get; private set; }
        public GameState CurrentState { get; private set; } = GameState.Prepare;

        public event Action<int> OnServerHealthChanged;
        public event Action<GameState> OnGameStateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            ServerHealth = startingServerHealth;
        }

        /// <summary>
        /// 적(악성코드)이 서버에 도달했을 때 호출합니다. 데미지만큼 서버 체력을 깎습니다.
        /// </summary>
        public void DamageServer(int damage)
        {
            if (CurrentState != GameState.Playing) return;

            ServerHealth = Mathf.Max(0, ServerHealth - damage);
            OnServerHealthChanged?.Invoke(ServerHealth);

            if (ServerHealth <= 0)
            {
                SetState(GameState.Defeat);
            }
        }

        public void SetState(GameState newState)
        {
            if (CurrentState == newState) return;
            CurrentState = newState;
            Time.timeScale = newState == GameState.Paused ? 0f : 1f;
            OnGameStateChanged?.Invoke(newState);
        }

        /// <summary>
        /// WaveSpawner가 마지막 웨이브를 모두 처치했을 때 호출해서 승리 처리합니다.
        /// </summary>
        public void NotifyAllWavesCleared()
        {
            if (CurrentState == GameState.Defeat) return;
            SetState(GameState.Victory);
        }

        public void TogglePause()
        {
            if (CurrentState == GameState.Playing) SetState(GameState.Paused);
            else if (CurrentState == GameState.Paused) SetState(GameState.Playing);
        }

        public void RestartLevel()
        {
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
    }
}
