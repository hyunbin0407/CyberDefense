using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using CyberDefense.Core;

namespace CyberDefense.UI
{
    /// <summary>
    /// 메인 메뉴 화면의 버튼(시작하기/상점/인벤토리/종료)을 처리합니다.
    /// 상점/인벤토리는 씬 전환 없이 같은 씬 안의 패널을 켜고 끄는 방식(오버레이)으로 처리합니다.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("맵 선택 씬 이름")]
        [SerializeField] private string mapSelectSceneName = "MapSelect";

        [Header("오버레이 패널")]
        [SerializeField] private GameObject shopPanelRoot;
        [SerializeField] private GameObject inventoryPanelRoot;

        [Header("코인 표시 (선택사항)")]
        [SerializeField] private TMP_Text coinsText;

        private void Start()
        {
            if (MetaCurrencyManager.Instance != null)
            {
                UpdateCoinsText(MetaCurrencyManager.Instance.CurrentCoins);
                MetaCurrencyManager.Instance.OnCoinsChanged += UpdateCoinsText;
            }
        }

        private void OnDestroy()
        {
            if (MetaCurrencyManager.Instance != null)
                MetaCurrencyManager.Instance.OnCoinsChanged -= UpdateCoinsText;
        }

        private void UpdateCoinsText(int amount)
        {
            if (coinsText != null) coinsText.text = $"코인: {amount}";
        }

        /// <summary>"시작하기" 버튼에 연결합니다.</summary>
        public void GoToMapSelect()
        {
            SceneManager.LoadScene(mapSelectSceneName);
        }

        /// <summary>"상점" 버튼에 연결합니다.</summary>
        public void OpenShopPanel()
        {
            if (inventoryPanelRoot != null) inventoryPanelRoot.SetActive(false);
            if (shopPanelRoot != null) shopPanelRoot.SetActive(true);
        }

        /// <summary>"인벤토리" 버튼에 연결합니다.</summary>
        public void OpenInventoryPanel()
        {
            if (shopPanelRoot != null) shopPanelRoot.SetActive(false);
            if (inventoryPanelRoot != null) inventoryPanelRoot.SetActive(true);
        }

        /// <summary>상점/인벤토리 패널 안의 "닫기" 버튼에 연결합니다.</summary>
        public void CloseOverlayPanels()
        {
            if (shopPanelRoot != null) shopPanelRoot.SetActive(false);
            if (inventoryPanelRoot != null) inventoryPanelRoot.SetActive(false);
        }

        /// <summary>"종료" 버튼에 연결합니다.</summary>
        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
