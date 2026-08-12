using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace CyberDefense.Core
{
    /// <summary>
    /// 타워 배치를 위한 격자(그리드)를 관리합니다.
    /// 월드 좌표 <-> 셀 좌표 변환, 셀 점유 여부(타워 배치 가능 여부)를 담당합니다.
    /// 적 이동 경로(pathCells)는 에디터에서 수동으로 입력하지 않고,
    /// spawnPoint/pathWaypoints를 기반으로 런타임에 자동 계산합니다.
    /// </summary>
    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance { get; private set; }

        [Header("그리드 설정")]
        [SerializeField] private int width = 12;
        [SerializeField] private int height = 8;
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private Vector3 originPosition = Vector3.zero;

        [Header("적 이동 경로 (자동 계산용, WaveSpawner와 같은 지점을 연결하세요)")]
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private List<Transform> pathWaypoints = new List<Transform>();
        [Tooltip("경로 폭(칸 수). 1이면 이동 경로가 지나가는 칸만, 2 이상이면 경로 양옆으로 셀을 넓혀서 여유 있게 막습니다.")]
        [SerializeField] private int pathWidthInCells = 1;

        [Header("런타임 시각화 (Game 뷰/실제 빌드에서 보이는 그리드)")]
        [Tooltip("경로 셀 색상 (알파값으로 반투명하게)")]
        [SerializeField] private Color pathCellColor = new Color(0.1f, 0.12f, 0.25f, 0.6f);
        [Tooltip("설치 가능한 빈 셀 색상 (알파값으로 은은하게)")]
        [SerializeField] private Color buildableCellColor = new Color(0.3f, 1f, 0.3f, 0.15f);
        [Tooltip("타워/적 스프라이트보다 뒤에 그려지도록 낮은 값을 사용합니다.")]
        [SerializeField] private int visualizationSortingOrder = -10;

        // 경로 셀 조회가 빈번하므로(매 프레임 CanPlaceTower 호출) HashSet으로 O(1) 조회합니다.
        private readonly HashSet<Vector2Int> pathCells = new HashSet<Vector2Int>();
        private readonly HashSet<Vector2Int> occupiedCells = new HashSet<Vector2Int>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            ComputePathCells();
            BuildGridVisualization();
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

        // ==================== 경로 셀 자동 계산 ====================

        /// <summary>
        /// spawnPoint부터 pathWaypoints를 순서대로 이어가며, 각 구간이 지나가는 모든 그리드 셀을
        /// Bresenham 라인 알고리즘으로 계산해서 pathCells를 채웁니다.
        /// </summary>
        private void ComputePathCells()
        {
            pathCells.Clear();

            var points = new List<Vector3>();
            if (spawnPoint != null) points.Add(spawnPoint.position);
            foreach (var waypoint in pathWaypoints)
            {
                if (waypoint != null) points.Add(waypoint.position);
            }

            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector2Int startCell = WorldToCell(points[i]);
                Vector2Int endCell = WorldToCell(points[i + 1]);

                foreach (var cell in GetLineCells(startCell, endCell))
                {
                    AddCellWithWidth(cell);
                }
            }
        }

        /// <summary>
        /// Bresenham 라인 알고리즘으로 start와 end 셀 사이를 잇는 모든 정수 셀 좌표를 순서대로 반환합니다.
        /// </summary>
        private static IEnumerable<Vector2Int> GetLineCells(Vector2Int start, Vector2Int end)
        {
            int x0 = start.x, y0 = start.y;
            int x1 = end.x, y1 = end.y;

            int dx = Mathf.Abs(x1 - x0);
            int dy = Mathf.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            int x = x0, y = y0;
            while (true)
            {
                yield return new Vector2Int(x, y);
                if (x == x1 && y == y1) yield break;

                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y += sy;
                }
            }
        }

        /// <summary>
        /// pathWidthInCells가 1이면 해당 셀만 추가합니다. 2 이상이면, 정확한 수직 확장 대신
        /// 셀 주변 (pathWidthInCells - 1)칸 범위의 정사각형 이웃까지 함께 추가하는 근사 방식으로
        /// 경로 폭을 넓힙니다(경로 전체에 대해 반복되므로 결과적으로 넉넉한 통로 모양이 됩니다).
        /// </summary>
        private void AddCellWithWidth(Vector2Int center)
        {
            pathCells.Add(center);

            int extra = Mathf.Max(0, pathWidthInCells - 1);
            if (extra <= 0) return;

            for (int dx = -extra; dx <= extra; dx++)
            {
                for (int dy = -extra; dy <= extra; dy++)
                {
                    pathCells.Add(new Vector2Int(center.x + dx, center.y + dy));
                }
            }
        }

        // ==================== 런타임 시각화 (메시 생성) ====================

        /// <summary>
        /// 셀마다 개별 오브젝트를 만드는 대신, 그리드 전체를 하나의 메시로 만들어
        /// MeshRenderer 하나로 그립니다(수백~수천 셀이어도 가볍게 처리하기 위함).
        /// Start/Awake에서 한 번만 생성하고 이후 다시 만들지 않습니다.
        /// </summary>
        private void BuildGridVisualization()
        {
            var visualGO = new GameObject("GridVisualization");
            visualGO.transform.SetParent(transform);
            // originPosition 기준의 절대 월드 좌표로 정점을 계산할 것이므로,
            // 부모(GridManager)의 위치와 무관하게 이 오브젝트의 월드 트랜스폼을 원점/기본값으로 고정합니다.
            visualGO.transform.position = Vector3.zero;
            visualGO.transform.rotation = Quaternion.identity;
            visualGO.transform.localScale = Vector3.one;

            var meshFilter = visualGO.AddComponent<MeshFilter>();
            var meshRenderer = visualGO.AddComponent<MeshRenderer>();

            // 정점 컬러가 화면에 보이려면 버텍스 컬러를 지원하는 셰이더가 필요합니다.
            // 별도 Material 에셋을 만들지 않고 코드에서 바로 생성합니다.
            meshRenderer.material = new Material(Shader.Find("Sprites/Default"));
            meshRenderer.sortingOrder = visualizationSortingOrder;
            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;

            meshFilter.mesh = BuildGridMesh();
        }

        private Mesh BuildGridMesh()
        {
            int cellCount = width * height;
            var vertices = new Vector3[cellCount * 4];
            var colors = new Color[cellCount * 4];
            var triangles = new int[cellCount * 6];

            float half = cellSize * 0.5f;
            int vi = 0;
            int ti = 0;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var cell = new Vector2Int(x, y);
                    Vector3 center = CellToWorld(cell);
                    Color color = IsPathCell(cell) ? pathCellColor : buildableCellColor;

                    // 셀 하나를 사각형(quad) = 삼각형 2개로 만듭니다.
                    vertices[vi + 0] = center + new Vector3(-half, -half, 0f);
                    vertices[vi + 1] = center + new Vector3(-half, half, 0f);
                    vertices[vi + 2] = center + new Vector3(half, half, 0f);
                    vertices[vi + 3] = center + new Vector3(half, -half, 0f);

                    colors[vi + 0] = color;
                    colors[vi + 1] = color;
                    colors[vi + 2] = color;
                    colors[vi + 3] = color;

                    triangles[ti + 0] = vi + 0;
                    triangles[ti + 1] = vi + 1;
                    triangles[ti + 2] = vi + 2;
                    triangles[ti + 3] = vi + 0;
                    triangles[ti + 4] = vi + 2;
                    triangles[ti + 5] = vi + 3;

                    vi += 4;
                    ti += 6;
                }
            }

            var mesh = new Mesh();
            // 정점 수가 65535를 넘을 수 있는 큰 그리드도 안전하게 처리하기 위해 32비트 인덱스를 사용합니다.
            mesh.indexFormat = IndexFormat.UInt32;
            mesh.vertices = vertices;
            mesh.colors = colors;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();

            return mesh;
        }

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
