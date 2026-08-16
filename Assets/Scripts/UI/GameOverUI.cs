using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using CyberDefense.Core;

namespace CyberDefense.UI
{
    /// <summary>
    /// 게임이 승리(Victory) 또는 패배(Defeat) 상태가 되면 결과 팝업을 띄우는 스크립트입니다.
    /// GameOverPanel(초기에는 비활성화된 UI 패널)을 만들고 이 스크립트를 붙이세요.
    /// </summary>
    public class GameOverUI : MonoBehaviour
    {
        [Header("연결할 UI 요소")]
        [SerializeField] private GameObject panelRoot; // 평소엔 꺼져있다가 게임 끝나면 켜짐
        [SerializeField] private TMP_Text resultText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button backButton;

        [Header("뒤로가기 버튼이 이동할 씬 이름")]
        [SerializeField] private string backSceneName = "MapSelect";

        [Header("게임 종료 시 비활성화할 타워 짓기 버튼들")]
        [SerializeField] private Button[] buildButtons;
        [SerializeField] private TowerPlacementController placementController;

        private void Start()
        {
            if (panelRoot != null)
                panelRoot.SetActive(false);

            if (GameManager.Instance != null)
                GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;

            if (restartButton != null)
                restartButton.onClick.AddListener(HandleRestartClicked);

            if (backButton != null)
                backButton.onClick.AddListener(HandleBackClicked);
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;

            if (restartButton != null)
                restartButton.onClick.RemoveListener(HandleRestartClicked);

            if (backButton != null)
                backButton.onClick.RemoveListener(HandleBackClicked);
        }

        private void HandleGameStateChanged(GameManager.GameState newState)
        {
            if (newState == GameManager.GameState.Victory)
            {
                ShowResult(true);
            }
            else if (newState == GameManager.GameState.Defeat)
            {
                ShowResult(false);
            }
        }

        private void ShowResult(bool isVictory)
        {
            if (panelRoot != null)
                panelRoot.SetActive(true);

            if (resultText != null)
                resultText.text = isVictory ? "방어 성공!\n서버를 지켜냈습니다." : "방어 실패\n서버가 침해당했습니다.";

            // 게임이 끝나면 다시하기 버튼만 누를 수 있도록 타워 짓기 버튼은 모두 비활성화(회색 처리)
            if (placementController != null)
                placementController.CancelPlacement();

            if (buildButtons != null)
            {
                foreach (var button in buildButtons)
                {
                    if (button != null) button.interactable = false;
                }
            }
        }

        private void HandleRestartClicked()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.RestartLevel();
        }

        /// <summary>
        /// 맵 선택 화면으로 돌아갑니다. Victory/Defeat 상태는 이미 Time.timeScale이 1이지만,
        /// 혹시 모를 경우를 대비해 명시적으로 복원한 뒤 씬을 로드합니다.
        /// </summary>
        private void HandleBackClicked()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(backSceneName);
        }
    }
}
