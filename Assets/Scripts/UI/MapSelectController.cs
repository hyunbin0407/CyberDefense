using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using CyberDefense.Core;
using CyberDefense.Data;

namespace CyberDefense.UI
{
    /// <summary>
    /// 맵 선택 화면: 맵 목록을 슬롯으로 표시하고, 맵을 고르면 난이도 선택 패널을 엽니다.
    /// 난이도까지 고르면 GameSessionSettings에 저장하고 실제 게임 씬을 로드합니다.
    /// </summary>
    public class MapSelectController : MonoBehaviour
    {
        [Header("맵 목록")]
        [SerializeField] private List<MapData> maps = new List<MapData>();
        [SerializeField] private GameObject mapSlotPrefab;
        [SerializeField] private Transform mapSlotContainer;

        [Header("난이도 선택 패널")]
        [SerializeField] private GameObject difficultyPanelRoot;
        [SerializeField] private List<DifficultyData> difficulties = new List<DifficultyData>();
        [SerializeField] private GameObject difficultyButtonPrefab;
        [SerializeField] private Transform difficultyButtonContainer;

        private bool difficultyButtonsBuilt;

        private void Start()
        {
            BuildMapSlots();

            if (difficultyPanelRoot != null)
                difficultyPanelRoot.SetActive(false);
        }

        private void BuildMapSlots()
        {
            if (mapSlotPrefab == null || mapSlotContainer == null) return;

            foreach (var map in maps)
            {
                if (map == null) continue;

                var slotGO = Instantiate(mapSlotPrefab, mapSlotContainer);
                var slot = slotGO.GetComponent<MapSlot>();
                if (slot != null) slot.Setup(map, () => SelectMap(map));
            }
        }

        private void SelectMap(MapData map)
        {
            GameSessionSettings.SelectedMap = map;

            if (!difficultyButtonsBuilt) BuildDifficultyButtons();
            if (difficultyPanelRoot != null) difficultyPanelRoot.SetActive(true);
        }

        /// <summary>난이도 선택 패널의 "취소" 버튼에 연결합니다.</summary>
        public void CloseDifficultyPanel()
        {
            if (difficultyPanelRoot != null) difficultyPanelRoot.SetActive(false);
        }

        /// <summary>"메인 메뉴로" 버튼에 연결합니다.</summary>
        public void GoBackToMainMenu()
        {
            SceneManager.LoadScene("MainMenu");
        }

        private void BuildDifficultyButtons()
        {
            difficultyButtonsBuilt = true;
            if (difficultyButtonPrefab == null || difficultyButtonContainer == null) return;

            foreach (var difficulty in difficulties)
            {
                if (difficulty == null) continue;

                var buttonGO = Instantiate(difficultyButtonPrefab, difficultyButtonContainer);

                var label = buttonGO.GetComponentInChildren<TMP_Text>();
                if (label != null) label.text = difficulty.displayName;

                var button = buttonGO.GetComponent<Button>();
                if (button != null)
                    button.onClick.AddListener(() => StartGame(difficulty));
            }
        }

        private void StartGame(DifficultyData difficulty)
        {
            if (GameSessionSettings.SelectedMap == null) return;

            GameSessionSettings.SelectedDifficulty = difficulty;
            SceneManager.LoadScene(GameSessionSettings.SelectedMap.sceneName);
        }
    }
}
