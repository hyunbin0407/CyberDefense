using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using CyberDefense.Towers;

namespace CyberDefense.UI
{
    /// <summary>
    /// 화면 터치/마우스 클릭으로 씬에 배치된 타워를 선택하는 컨트롤러입니다.
    /// 타워를 클릭하면 OnTowerSelected, 타워가 없는 빈 곳을 클릭하면 OnSelectionCleared를 발생시킵니다.
    /// TowerPlacementController가 타워 배치 모드일 때는 동작하지 않습니다.
    /// </summary>
    public class TowerSelectionController : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private TowerPlacementController placementController;

        public event Action<TowerController> OnTowerSelected;
        public event Action OnSelectionCleared;

        // Physics2D.OverlapPoint 결과를 담을 버퍼(매 클릭마다 새로 할당하지 않도록 재사용)
        private static readonly List<Collider2D> OverlapResults = new List<Collider2D>();

        private void Awake()
        {
            if (mainCamera == null) mainCamera = Camera.main;
        }

        private void Update()
        {
            // 타워를 새로 짓는 배치 모드 중에는 선택 로직이 끼어들면 안 됨
            if (placementController != null && placementController.IsPlacementModeActive) return;

            // 업그레이드 패널의 버튼 등 UI 위 클릭은 타워 선택/해제로 처리하지 않음
            if (IsPointerOverUI()) return;

            if (!WasPointerReleasedThisFrame()) return;

            Vector3 worldPos = GetPointerWorldPosition();
            if (worldPos == Vector3.negativeInfinity) return;

            TrySelectTower(worldPos);
        }

        /// <summary>
        /// 클릭 위치의 콜라이더를 검사해서 타워를 찾습니다.
        /// 타워의 사거리 감지용 트리거 콜라이더(CircleCollider2D, isTrigger)는 제외하고,
        /// 클릭 감지용으로 프리팹에 추가한 일반 콜라이더만 검사합니다.
        /// </summary>
        private void TrySelectTower(Vector3 worldPos)
        {
            var filter = new ContactFilter2D();
            filter.NoFilter();
            filter.useTriggers = false;

            int count = Physics2D.OverlapPoint(worldPos, filter, OverlapResults);

            TowerController tower = null;
            for (int i = 0; i < count; i++)
            {
                tower = OverlapResults[i].GetComponent<TowerController>();
                if (tower != null) break;
            }

            if (tower != null)
                OnTowerSelected?.Invoke(tower);
            else
                OnSelectionCleared?.Invoke();
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

        private bool IsPointerOverUI()
        {
            if (EventSystem.current == null) return false;

            if (Input.touchCount > 0)
                return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);

            return EventSystem.current.IsPointerOverGameObject();
        }
    }
}
