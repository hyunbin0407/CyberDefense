using System;
using System.Collections.Generic;
using UnityEngine;
using CyberDefense.Core;
using CyberDefense.Data;
using CyberDefense.Economy;

namespace CyberDefense.Enemies
{
    /// <summary>
    /// 적(악성코드) 한 마리의 이동, 체력, 피격, 사망을 처리합니다.
    /// WaveSpawner가 스폰 시 Initialize()를 호출해서 경로와 데이터를 주입합니다.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class EnemyController : MonoBehaviour
    {
        public EnemyData Data { get; private set; }
        public float CurrentHealth { get; private set; }
        public bool IsStealth => Data != null && Data.isStealth;
        public bool IsDead { get; private set; }

        public event Action<EnemyController> OnDeath;
        public event Action<EnemyController> OnReachedServer;

        private List<Vector3> waypoints;
        private int currentWaypointIndex;
        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Initialize(EnemyData data, List<Vector3> path)
        {
            Data = data;
            CurrentHealth = data.maxHealth;
            waypoints = path;
            currentWaypointIndex = 0;
            IsDead = false;

            if (spriteRenderer != null && data.icon != null)
                spriteRenderer.sprite = data.icon;

            if (waypoints != null && waypoints.Count > 0)
                transform.position = waypoints[0];
        }

        private void Update()
        {
            if (IsDead || waypoints == null || waypoints.Count == 0) return;
            MoveAlongPath();
        }

        private void MoveAlongPath()
        {
            if (currentWaypointIndex >= waypoints.Count)
            {
                ReachServer();
                return;
            }

            Vector3 target = waypoints[currentWaypointIndex];
            transform.position = Vector3.MoveTowards(
                transform.position, target, Data.moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, target) < 0.05f)
            {
                currentWaypointIndex++;
            }
        }

        /// <summary>
        /// 타워로부터 데미지를 받습니다. 저항력이 적용된 후 체력이 깎입니다.
        /// </summary>
        public void TakeDamage(float rawDamage)
        {
            if (IsDead) return;

            float mitigated = rawDamage * (1f - Data.damageResistance);
            CurrentHealth -= mitigated;

            if (CurrentHealth <= 0f)
            {
                Die();
            }
        }

        private void Die()
        {
            if (IsDead) return;
            IsDead = true;

            if (CurrencyManager.Instance != null)
                CurrencyManager.Instance.AddCurrency(Data.rewardOnKill);

            OnDeath?.Invoke(this);
            Destroy(gameObject);
        }

        private void ReachServer()
        {
            if (IsDead) return;
            IsDead = true;

            if (GameManager.Instance != null)
                GameManager.Instance.DamageServer(Data.damageToServer);

            OnReachedServer?.Invoke(this);
            Destroy(gameObject);
        }
    }
}
