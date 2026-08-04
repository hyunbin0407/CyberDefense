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
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyController : MonoBehaviour
    {
        public EnemyData Data { get; private set; }
        public float CurrentHealth { get; private set; }
        public bool IsStealth => Data != null && Data.isStealth;
        public bool IsDead { get; private set; }
        public int WaypointIndex => currentWaypointIndex;

        public event Action<EnemyController> OnDeath;
        public event Action<EnemyController> OnReachedServer;

        private List<Vector3> waypoints;
        private int currentWaypointIndex;
        private SpriteRenderer spriteRenderer;
        private Rigidbody2D rb;

        private float slowMultiplier = 1f;
        private float slowTimer;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            rb = GetComponent<Rigidbody2D>();
        }

        public void Initialize(EnemyData data, List<Vector3> path)
        {
            Data = data;
            CurrentHealth = data.maxHealth;
            waypoints = path;
            currentWaypointIndex = 0;
            IsDead = false;
            slowMultiplier = 1f;
            slowTimer = 0f;

            if (spriteRenderer != null && data.icon != null)
                spriteRenderer.sprite = data.icon;

            if (waypoints != null && waypoints.Count > 0)
                rb.position = waypoints[0];
        }

        private void FixedUpdate()
        {
            if (IsDead || waypoints == null || waypoints.Count == 0) return;

            if (slowTimer > 0f)
            {
                slowTimer -= Time.fixedDeltaTime;
                if (slowTimer <= 0f) slowMultiplier = 1f;
            }

            MoveAlongPath();
        }

        /// <summary>
        /// 허니팟 등 슬로우 타워가 호출합니다. 가장 강한 효과와 가장 긴 지속시간을 유지합니다.
        /// </summary>
        public void ApplySlow(float multiplier, float duration)
        {
            multiplier = Mathf.Clamp01(multiplier);
            if (multiplier < slowMultiplier) slowMultiplier = multiplier;
            if (duration > slowTimer) slowTimer = duration;
        }

        private void MoveAlongPath()
        {
            if (currentWaypointIndex >= waypoints.Count)
            {
                ReachServer();
                return;
            }

            Vector3 target = waypoints[currentWaypointIndex];
            float speed = Data.moveSpeed * slowMultiplier;
            Vector2 newPos = Vector2.MoveTowards(
                rb.position, target, speed * Time.fixedDeltaTime);
            rb.MovePosition(newPos);

            if (Vector2.Distance(newPos, (Vector2)target) < 0.05f)
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
