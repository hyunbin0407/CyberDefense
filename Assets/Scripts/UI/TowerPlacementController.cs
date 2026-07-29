using UnityEngine;
using CyberDefense.Core;
using CyberDefense.Data;
using CyberDefense.Economy;
using CyberDefense.Towers;

namespace CyberDefense.UI
{
    /// <summary>
    /// 모바일 터치(및 에디터 테스트용 마우스 클릭)로 타워를 배치하는 컨트롤러입니다.
    /// 1) SelectTowerToBuild(TowerData)로 배치할 타워를 고르고
    /// 2) 화면을 터치하면 해당 위치의 그리드 셀에 타워를 건설합니다.
    /// </summary>
    public class TowerPlacementController : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private GameObject placementPreviewPrefab;

        private TowerData selectedTowerData;
        private GameObject previewInstance;

        private void Awake()
        {
            if (mainCamera == null) mainCamera = Camera.main;
        }

        private void Update()
        {
            if (selectedTowerData == null) return;

            Vector3 worldPos = GetPointerWorldPosition();
            if (worldPos == Vector3.negativeInfinity) return;

            UpdatePreview(worldPos);

            if (WasPointerReleasedThisFrame())
            {
                TryPlaceTower(worldPos);
            }
        }

        /// <summary>
        /// UI의 타워 선택 버튼에서 호출합니다. 이후 화면 터치 시 이 타워가 건설됩니다.
        /// </summary>
        public void SelectTowerToBuild(TowerData data)
        {
            selectedTowerData = data;

            if (previewInstance != null) Destroy(previewInstance);
            if (placementPreviewPrefab != null)
            {
                previewInstance = Instantiate(placementPreviewPrefab);
            }
        }

        public void CancelPlacement()
        {
            selectedTowerData = null;
            if (previewInstance != null) Destroy(previewInstance);
        }

        private void UpdatePreview(Vector3 worldPos)
        {
            if (previewInstance == null || GridManager.Instance == null) return;

            var cell = GridManager.Instance.WorldToCell(worldPos);
            previewInstance.transform.position = GridManager.Instance.CellToWorld(cell);

            bool canPlace = GridManager.Instance.CanPlaceTower(cell)
                && CurrencyManager.Instance != null
                && CurrencyManager.Instance.CanAfford(selectedTowerData.buildCost);

            var sr = previewInstance.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.color = canPlace ? new Color(0, 1, 0, 0.5f) : new Color(1, 0, 0, 0.5f);
        }

        private void TryPlaceTower(Vector3 worldPos)
        {
            if (GridManager.Instance == null || selectedTowerData == null) return;

            var cell = GridManager.Instance.WorldToCell(worldPos);
            if (!GridManager.Instance.CanPlaceTower(cell)) return;

            if (CurrencyManager.Instance == null ||
                !CurrencyManager.Instance.TrySpend(selectedTowerData.buildCost))
            {
                return; // 재화 부족
            }

            Vector3 spawnPos = GridManager.Instance.CellToWorld(cell);
            GameObject towerObj = Instantiate(selectedTowerData.towerPrefab, spawnPos, Quaternion.identity);
            var tower = towerObj.GetComponent<TowerController>();
            if (tower == null) tower = towerObj.AddComponent<TowerController>();
            tower.Initialize(selectedTowerData, cell);

            GridManager.Instance.OccupyCell(cell);

            CancelPlacement();
        }

        /// <summary>
        /// 터치(모바일) 또는 마우스(에디터) 입력의 월드 좌표를 반환합니다.
        /// 입력이 없으면 Vector3.negativeInfinity를 반환합니다.
        /// </summary>
        private Vector3 GetPointerWorldPosition()
        {
            Vector3 screenPos;

            if (Input.touchCount > 0)
            {
                screenPos = Input.GetTouch(0).position;
            }
            else if (Input.mousePresent)
            {
                screenPos = Input.mousePosition;
            }
            else
            {
                return Vector3.negativeInfinity;
            }

            screenPos.z = -mainCamera.transform.position.z;
            return mainCamera.ScreenToWorldPoint(screenPos);
        }

        private bool WasPointerReleasedThisFrame()
        {
            if (Input.touchCount > 0)
            {
                return Input.GetTouch(0).phase == TouchPhase.Ended;
            }
            return Input.GetMouseButtonUp(0);
        }
    }
}
