using System.Collections.Generic;
using UnityEngine;

namespace CyberDefense.Core
{
    /// <summary>
    /// 타워 배치를 위한 격자(그리드)를 관리합니다.
    /// 월드 좌표 <-> 셀 좌표 변환, 셀 점유 여부(타워 배치 가능 여부)를 담당합니다.
    /// </summary>
    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance { get; private set; }

        [Header("그리드 설정")]
        [SerializeField] private int width = 12;
        [SerializeField] private int height = 8;
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private Vector3 originPosition = Vector3.zero;

        [Header("경로(적 이동 경로) 셀은 타워 배치 불가")]
        [SerializeField] private List<Vector2Int> pathCells = new List<Vector2Int>();

        private readonly HashSet<Vector2Int> occupiedCells = new HashSet<Vector2Int>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public Vector2Int WorldToCell(Vector3 worldPosition)
        {
            Vector3 local = worldPosition - originPosition;
            int x = Mathf.FloorToInt(local.x / cellSize);
            int y = Mathf.FloorToInt(local.y / cellSize);
            return new Vector2Int(x, y);
        }

        public Vector3 CellToWorld(Vector2Int cell)
        {
            float x = cell.x * cellSize + cellSize * 0.5f;
            float y = cell.y * cellSize + cellSize * 0.5f;
            return originPosition + new Vector3(x, y, 0f);
        }

        public bool IsInsideGrid(Vector2Int cell)
        {
            return cell.x >= 0 && cell.x < width && cell.y >= 0 && cell.y < height;
        }

        public bool IsPathCell(Vector2Int cell) => pathCells.Contains(cell);

        public bool IsCellOccupied(Vector2Int cell) => occupiedCells.Contains(cell);

        /// <summary>
        /// 해당 셀에 타워를 배치할 수 있는지 종합 판단합니다.
        /// </summary>
        public bool CanPlaceTower(Vector2Int cell)
        {
            return IsInsideGrid(cell) && !IsPathCell(cell) && !IsCellOccupied(cell);
        }

        public void OccupyCell(Vector2Int cell) => occupiedCells.Add(cell);

        public void FreeCell(Vector2Int cell) => occupiedCells.Remove(cell);

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector3 center = CellToWorld(new Vector2Int(x, y));
                    Gizmos.DrawWireCube(center, new Vector3(cellSize, cellSize, 0f) * 0.95f);
                }
            }

            Gizmos.color = Color.yellow;
            foreach (var p in pathCells)
            {
                Gizmos.DrawCube(CellToWorld(p), new Vector3(cellSize, cellSize, 0f) * 0.8f);
            }
        }
    }
}
