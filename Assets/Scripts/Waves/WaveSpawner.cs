using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CyberDefense.Core;
using CyberDefense.Data;
using CyberDefense.Enemies;

namespace CyberDefense.Waves
{
    [Serializable]
    public class EnemySpawnEntry
    {
        public EnemyData enemyData;
        [Tooltip("이번 웨이브에서 이 종류의 적을 몇 마리 스폰할지")]
        public int count = 5;
        [Tooltip("같은 종류 적들 사이의 스폰 간격(초)")]
        public float spawnInterval = 0.8f;
    }

    [Serializable]
    public class WaveDefinition
    {
        public string waveName = "Wave 1";
        public List<EnemySpawnEntry> enemies = new List<EnemySpawnEntry>();
        [Tooltip("이 웨이브 시작 전 대기 시간(초). 플레이어가 타워를 배치할 준비 시간입니다.")]
        public float delayBeforeWave = 5f;
    }

    /// <summary>
    /// 정의된 웨이브 목록에 따라 순서대로 적을 스폰합니다.
    /// 모든 웨이브가 끝나고 적이 전부 처치되면 GameManager에 승리를 알립니다.
    /// </summary>
    public class WaveSpawner : MonoBehaviour
    {
        public static WaveSpawner Instance { get; private set; }

        [SerializeField] private List<WaveDefinition> waves = new List<WaveDefinition>();
        [SerializeField] private List<Transform> pathWaypoints = new List<Transform>();
        [SerializeField] private Transform spawnPoint;

        public int CurrentWaveIndex { get; private set; } = -1;
        public int TotalWaves => waves.Count;
        public bool IsSpawning { get; private set; }

        private int aliveEnemyCount;
        private List<Vector3> cachedPath;

        public event Action<int, int> OnWaveStarted; // (현재 웨이브 인덱스(0부터), 전체 웨이브 수)
        public event Action<int, int, float> OnPrepareCountdown; // (다음 웨이브 인덱스, 전체 웨이브 수, 시작까지 남은 시간(초))
        public event Action OnAllWavesCleared;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            CachePath();
            StartCoroutine(RunWaves());
        }

        private void CachePath()
        {
            cachedPath = new List<Vector3>();
            if (spawnPoint != null) cachedPath.Add(spawnPoint.position);
            foreach (var wp in pathWaypoints)
            {
                if (wp != null) cachedPath.Add(wp.position);
            }
        }

        private IEnumerator RunWaves()
        {
            for (int i = 0; i < waves.Count; i++)
            {
                CurrentWaveIndex = i;
                var wave = waves[i];

                if (GameManager.Instance != null)
                    GameManager.Instance.SetState(GameManager.GameState.Prepare);

                yield return StartCoroutine(CountdownToWave(i, wave.delayBeforeWave));

                if (GameManager.Instance != null)
                    GameManager.Instance.SetState(GameManager.GameState.Playing);

                OnWaveStarted?.Invoke(i, waves.Count);
                yield return StartCoroutine(SpawnWave(wave));

                // 해당 웨이브의 적이 모두 처치되거나 서버에 도달할 때까지 대기
                yield return new WaitUntil(() => aliveEnemyCount <= 0);
            }

            OnAllWavesCleared?.Invoke();
            if (GameManager.Instance != null)
                GameManager.Instance.NotifyAllWavesCleared();
        }

        /// <summary>
        /// 다음 웨이브 시작까지 남은 시간을 매 프레임 OnPrepareCountdown으로 알립니다.
        /// </summary>
        private IEnumerator CountdownToWave(int waveIndex, float delay)
        {
            float remaining = delay;
            while (remaining > 0f)
            {
                OnPrepareCountdown?.Invoke(waveIndex, waves.Count, remaining);
                yield return null;
                remaining -= Time.deltaTime;
            }
            OnPrepareCountdown?.Invoke(waveIndex, waves.Count, 0f);
        }

        private IEnumerator SpawnWave(WaveDefinition wave)
        {
            IsSpawning = true;

            foreach (var entry in wave.enemies)
            {
                for (int i = 0; i < entry.count; i++)
                {
                    SpawnEnemy(entry.enemyData);
                    yield return new WaitForSeconds(entry.spawnInterval);
                }
            }

            IsSpawning = false;
        }

        private void SpawnEnemy(EnemyData data)
        {
            if (data == null || data.enemyPrefab == null || cachedPath == null || cachedPath.Count == 0)
                return;

            GameObject obj = Instantiate(data.enemyPrefab, cachedPath[0], Quaternion.identity);
            var controller = obj.GetComponent<EnemyController>();
            if (controller == null) controller = obj.AddComponent<EnemyController>();

            // 맵 선택 화면을 거쳐 들어온 경우에만 난이도 배율이 설정되어 있고, 아니면 1.0으로 취급합니다.
            var difficulty = GameSessionSettings.SelectedDifficulty;
            float healthMultiplier = difficulty != null ? difficulty.enemyHealthMultiplier : 1f;
            float speedMultiplier = difficulty != null ? difficulty.enemySpeedMultiplier : 1f;

            controller.Initialize(data, cachedPath, healthMultiplier, speedMultiplier);
            aliveEnemyCount++;

            controller.OnDeath += HandleEnemyRemoved;
            controller.OnReachedServer += HandleEnemyRemoved;
        }

        private void HandleEnemyRemoved(EnemyController enemy)
        {
            aliveEnemyCount = Mathf.Max(0, aliveEnemyCount - 1);
        }
    }
}
