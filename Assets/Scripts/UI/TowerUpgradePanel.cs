using TMPro;
using UnityEngine;
using UnityEngine.UI;
using CyberDefense.Economy;
using CyberDefense.Towers;

namespace CyberDefense.UI
{
    /// <summary>
    /// 선택된 타워의 정보(이름, 레벨, 공격력, 업그레이드 비용)를 보여주고
    /// 업그레이드 버튼 클릭을 처리하는 UI 패널입니다.
    /// TowerSelectionController의 선택/해제 이벤트를 구독해서 표시 여부를 결정합니다.
    /// </summary>
    public class TowerUpgradePanel : MonoBehaviour
    {
        [Header("연결할 컨트롤러")]
        [SerializeField] private TowerSelectionController selectionController;

        [Header("연결할 UI 요소")]
        [SerializeField] private GameObject panelRoot; // 평소엔 꺼져있다가 타워 선택 시 켜짐
        [SerializeField] private TMP_Text towerNameText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text damageText;
        [SerializeField] private TMP_Text upgradeCostText;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private TMP_Text maxLevelIndicatorText; // 선택사항: 최대 레벨 도달 시 안내 문구

        private TowerController selectedTower;

        private void Start()
        {
            if (panelRoot != null)
                panelRoot.SetActive(false);

            if (selectionController != null)
            {
                selectionController.OnTowerSelected += HandleTowerSelected;
                selectionController.OnSelectionCleared += HandleSelectionCleared;
            }

            if (upgradeButton != null)
                upgradeButton.onClick.AddListener(HandleUpgradeClicked);
        }

        private void OnDestroy()
        {
            if (selectionController != null)
            {
                selectionController.OnTowerSelected -= HandleTowerSelected;
                selectionController.OnSelectionCleared -= HandleSelectionCleared;
            }

            if (upgradeButton != null)
                upgradeButton.onClick.RemoveListener(HandleUpgradeClicked);

            UnsubscribeFromSelectedTower();
        }

        private void HandleTowerSelected(TowerController tower)
        {
            UnsubscribeFromSelectedTower();

            selectedTower = tower;
            if (selectedTower != null)
                selectedTower.OnUpgraded += RefreshPanel;

            if (panelRoot != null)
                panelRoot.SetActive(true);

            RefreshPanel();
        }

        private void HandleSelectionCleared()
        {
            UnsubscribeFromSelectedTower();
            selectedTower = null;

            if (panelRoot != null)
                panelRoot.SetActive(false);
        }

        private void UnsubscribeFromSelectedTower()
        {
            if (selectedTower != null)
                selectedTower.OnUpgraded -= RefreshPanel;
        }

        /// <summary>
        /// 선택된 타워의 현재 상태를 패널 텍스트에 반영합니다.
        /// 타워를 새로 선택했을 때, 그리고 업그레이드에 성공했을 때 호출됩니다.
        /// </summary>
        private void RefreshPanel()
        {
            if (selectedTower == null || selectedTower.Data == null) return;

            var data = selectedTower.Data;
            bool isMaxLevel = selectedTower.CurrentLevel >= data.maxLevel;

            if (towerNameText != null)
                towerNameText.text = $"{data.displayName} (Lv.{selectedTower.CurrentLevel})";

            if (levelText != null)
                levelText.text = $"레벨: {selectedTower.CurrentLevel} / {data.maxLevel}";

            if (damageText != null)
                damageText.text = $"공격력: {selectedTower.CurrentDamage:0.#}";

            if (upgradeCostText != null)
                upgradeCostText.text = isMaxLevel ? "최대 레벨" : $"업그레이드 비용: {selectedTower.GetUpgradeCost()}";

            if (maxLevelIndicatorText != null)
                maxLevelIndicatorText.text = isMaxLevel ? "최대 레벨입니다" : string.Empty;

            if (upgradeButton != null)
                upgradeButton.interactable = !isMaxLevel;
        }

        private void HandleUpgradeClicked()
        {
            if (selectedTower == null) return;
            if (CurrencyManager.Instance == null) return;

            int cost = selectedTower.GetUpgradeCost();

            // TrySpend가 false를 반환하면(재화 부족) 차감도 안 되고 여기서 끝남 - 아무 일도 안 일어남
            if (!CurrencyManager.Instance.TrySpend(cost)) return;

            selectedTower.TryUpgrade();
        }
    }
}
