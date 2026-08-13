using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CyberDefense.Core;
using CyberDefense.Data;
using CyberDefense.Enemies;

namespace CyberDefense.Towers
{
    /// <summary>
    /// 타워 하나의 동작(사거리 내 적 탐색, 타겟팅, 공격, 레벨업)을 담당합니다.
    /// </summary>
    public class TowerController : MonoBehaviour
    {
        [SerializeField] private TowerData data;
        [SerializeField] private Transform firePoint;
        [SerializeField] private LineRenderer attackLine;

        private const float AttackLineFlashDuration = 0.12f;
        private const int AreaEffectSegmentCount = 24;

        public int CurrentLevel { get; private set; } = 1;
        public Vector2Int GridCell { get; private set; }

        /// <summary>이 타워의 원본 데이터입니다. UI에서 이름/최대 레벨 등을 표시할 때 사용합니다.</summary>
        public TowerData Data => data;

        /// <summary>업그레이드가 반영된 현재 공격력입니다. UI에서 표시할 때 사용합니다.</summary>
        public float CurrentDamage => currentDamage;

        /// <summary>업그레이드에 성공했을 때 발생합니다. UI가 표시를 갱신할 타이밍을 알 수 있게 합니다.</summary>
        public event Action OnUpgraded;

        private float currentDamage;
        private float attackTimer;
        private EnemyController currentTarget;
        private Coroutine attackLineRoutine;

        private readonly List<EnemyController> enemiesInRange = new List<EnemyController>();
        private CircleCollider2D rangeCollider;
        private readonly Collider2D[] overlapBuffer = new Collider2D[32];

        private void Awake()
        {
            rangeCollider = GetComponent<CircleCollider2D>();
            if (rangeCollider == null)
                rangeCollider = gameObject.AddComponent<CircleCollider2D>();
            rangeCollider.isTrigger = true;

            if (data != null)
            {
                currentDamage = data.damage;
                rangeCollider.radius = data.attackRange;
            }
        }

        public void Initialize(TowerData towerData, Vector2Int cell)
        {
            data = towerData;
            GridCell = cell;
            currentDamage = data.damage;
            CurrentLevel = 1;
            rangeCollider.radius = data.attackRange;

            if (attackLine != null) attackLine.enabled = false;
        }

        private void Update()
        {
            if (data == null) return;

            attackTimer -= Time.deltaTime;

            // 죽거나 null이 된 타겟 갱신
            if (currentTarget == null || currentTarget.IsDead)
                currentTarget = FindBestTarget();

            if (currentTarget != null && attackTimer <= 0f)
            {
                Attack(currentTarget);
                attackTimer = data.attackCooldown;
            }
        }

        // 적이 사거리 안에 들어오면 리스트에 추가
        private void OnTriggerEnter2D(Collider2D other)
        {
            var enemy = other.GetComponent<EnemyController>();
            if (enemy != null && !enemiesInRange.Contains(enemy))
                enemiesInRange.Add(enemy);
        }

        // 적이 사거리 밖으로 나가면 리스트에서 제거
        private void OnTriggerExit2D(Collider2D other)
        {
            var enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
                enemiesInRange.Remove(enemy);
        }

        /// <summary>
        /// 사거리 내 적 중 경로를 가장 많이 진행한(서버에 가까운) 적을 우선 타겟으로 반환합니다.
        /// </summary>
        private EnemyController FindBestTarget()
        {
            // 이미 사망하거나 파괴된 적 정리
            enemiesInRange.RemoveAll(e => e == null || e.IsDead);

            EnemyController best = null;
            float bestScore = float.NegativeInfinity;

            foreach (var enemy in enemiesInRange)
            {
                if (enemy.IsStealth && !data.canDetectStealth) continue;

                float score = enemy.WaypointIndex;
                if (score > bestScore)
                {
                    bestScore = score;
                    best = enemy;
                }
            }

            return best;
        }

        private void Attack(EnemyController target)
        {
            AudioManager.Instance?.PlayTowerShootSFX();

            if (data.isAreaAttack)
            {
                AttackArea(target.transform.position);
            }
            else
            {
                ApplyHit(target);
                PlayLaserEffect(target.transform.position);
            }
        }

        private void AttackArea(Vector3 center)
        {
            int count = Physics2D.OverlapCircleNonAlloc(center, data.areaRadius, overlapBuffer);
            for (int i = 0; i < count; i++)
            {
                var enemy = overlapBuffer[i].GetComponent<EnemyController>();
                if (enemy == null || enemy.IsDead) continue;
                if (enemy.IsStealth && !data.canDetectStealth) continue;
                ApplyHit(enemy);
            }

            PlayAreaEffect(center);
        }

        /// <summary>
        /// 단일 타겟 공격 시 발사 순간에만 레이저가 반짝이는 연출입니다.
        /// </summary>
        private void PlayLaserEffect(Vector3 targetPosition)
        {
            if (attackLine == null) return;

            if (attackLineRoutine != null) StopCoroutine(attackLineRoutine);
            attackLineRoutine = StartCoroutine(LaserFlashRoutine(targetPosition));
        }

        private IEnumerator LaserFlashRoutine(Vector3 targetPosition)
        {
            attackLine.loop = false;
            attackLine.startColor = data.effectColor;
            attackLine.endColor = data.effectColor;
            attackLine.positionCount = 2;
            attackLine.SetPosition(0, firePoint != null ? firePoint.position : transform.position);
            attackLine.SetPosition(1, targetPosition);
            attackLine.enabled = true;

            yield return new WaitForSeconds(AttackLineFlashDuration);

            attackLine.enabled = false;
            attackLineRoutine = null;
        }

        /// <summary>
        /// 광역 공격 시 공격 지점(사거리 areaRadius)에 원형으로 퍼지는 이펙트를
        /// attackLine을 재사용해 그립니다. 새 컴포넌트를 추가하지 않기 위한 방식입니다.
        /// </summary>
        private void PlayAreaEffect(Vector3 center)
        {
            if (attackLine == null) return;

            if (attackLineRoutine != null) StopCoroutine(attackLineRoutine);
            attackLineRoutine = StartCoroutine(AreaEffectRoutine(center));
        }

        private IEnumerator AreaEffectRoutine(Vector3 center)
        {
            attackLine.loop = true;
            attackLine.startColor = data.effectColor;
            attackLine.endColor = data.effectColor;
            attackLine.positionCount = AreaEffectSegmentCount;
            for (int i = 0; i < AreaEffectSegmentCount; i++)
            {
                float angle = i * Mathf.PI * 2f / AreaEffectSegmentCount;
                Vector3 point = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * data.areaRadius;
                attackLine.SetPosition(i, point);
            }
            attackLine.enabled = true;

            yield return new WaitForSeconds(AttackLineFlashDuration);

            attackLine.enabled = false;
            attackLine.loop = false;
            attackLineRoutine = null;
        }

        /// <summary>
        /// 데미지와 슬로우(허니팟 등 디버프형 타워) 효과를 대상에게 함께 적용합니다.
        /// </summary>
        private void ApplyHit(EnemyController target)
        {
            if (data.appliesSlow)
                target.ApplySlow(data.slowMultiplier, data.slowDuration);

            if (currentDamage > 0f)
                target.TakeDamage(currentDamage);
        }

        public bool TryUpgrade()
        {
            if (CurrentLevel >= data.maxLevel) return false;
            CurrentLevel++;
            currentDamage *= data.upgradeDamageMultiplier;
            OnUpgraded?.Invoke();
            return true;
        }

        public int GetUpgradeCost() => data.upgradeCost * CurrentLevel;

        private void OnDrawGizmosSelected()
        {
            if (data == null) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, data.attackRange);
        }
    }
}
