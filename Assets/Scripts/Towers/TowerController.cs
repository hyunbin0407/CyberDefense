using System.Collections.Generic;
using UnityEngine;
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
        [SerializeField] private LineRenderer attackLine; // 간단한 레이저형 공격 연출용 (선택)

        public int CurrentLevel { get; private set; } = 1;
        public Vector2Int GridCell { get; private set; }

        private float currentDamage;
        private float attackTimer;
        private EnemyController currentTarget;

        private static readonly Collider2D[] overlapBuffer = new Collider2D[32];

        public void Initialize(TowerData towerData, Vector2Int cell)
        {
            data = towerData;
            GridCell = cell;
            currentDamage = data.damage;
            CurrentLevel = 1;

            if (attackLine != null) attackLine.enabled = false;
        }

        private void Update()
        {
            if (data == null) return;

            attackTimer -= Time.deltaTime;

            if (currentTarget == null || !IsTargetValid(currentTarget))
            {
                currentTarget = FindTarget();
            }

            if (currentTarget != null && attackTimer <= 0f)
            {
                Attack(currentTarget);
                attackTimer = data.attackCooldown;
            }

            if (attackLine != null) attackLine.enabled = currentTarget != null;
            if (attackLine != null && currentTarget != null)
            {
                attackLine.SetPosition(0, firePoint != null ? firePoint.position : transform.position);
                attackLine.SetPosition(1, currentTarget.transform.position);
            }
        }

        private bool IsTargetValid(EnemyController target)
        {
            if (target == null || target.IsDead) return false;
            float dist = Vector2.Distance(transform.position, target.transform.position);
            return dist <= data.attackRange;
        }

        /// <summary>
        /// 사거리 내에서 가장 진행도가 높은(서버에 가까운) 적을 우선 타겟으로 삼습니다.
        /// canDetectStealth가 false인 타워는 은신 적(제로데이)을 무시합니다.
        /// </summary>
        private EnemyController FindTarget()
        {
            int count = Physics2D.OverlapCircleNonAlloc(
                transform.position, data.attackRange, overlapBuffer);

            EnemyController best = null;
            float bestScore = float.NegativeInfinity;

            for (int i = 0; i < count; i++)
            {
                var enemy = overlapBuffer[i].GetComponent<EnemyController>();
                if (enemy == null || enemy.IsDead) continue;
                if (enemy.IsStealth && !data.canDetectStealth) continue;

                // 서버에 가까울수록(경로를 많이 지났을수록) 우선순위 높음 - 근사치로 x축 등 활용 가능
                float score = -Vector2.Distance(enemy.transform.position, transform.position);
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
            if (data.isAreaAttack)
            {
                AttackArea(target.transform.position);
            }
            else
            {
                target.TakeDamage(currentDamage);
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
                enemy.TakeDamage(currentDamage);
            }
        }

        /// <summary>
        /// 재화가 충분할 때 UI 등에서 호출하여 타워를 업그레이드합니다.
        /// </summary>
        public bool TryUpgrade()
        {
            if (CurrentLevel >= data.maxLevel) return false;

            CurrentLevel++;
            currentDamage *= data.upgradeDamageMultiplier;
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
