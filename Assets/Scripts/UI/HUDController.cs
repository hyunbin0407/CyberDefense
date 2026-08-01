using TMPro;
using UnityEngine;
using CyberDefense.Core;
using CyberDefense.Economy;

namespace CyberDefense.UI
{
    /// <summary>
    /// 화면 상단 등에 재화, 서버 체력을 실시간으로 표시하는 HUD(Head-Up Display) 스크립트입니다.
    /// 빈 GameObject(예: "HUDController")를 만들고 이 스크립트를 붙인 뒤,
    /// Inspector에서 TextMeshPro 텍스트들을 연결하세요.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [Header("연결할 UI 텍스트")]
        [SerializeField] private TMP_Text currencyText;
        [SerializeField] private TMP_Text serverHealthText;
        [SerializeField] private TMP_Text waveText;

        private void Start()
        {
            // 최초 1회 값 표시
            if (CurrencyManager.Instance != null)
            {
                UpdateCurrencyText(CurrencyManager.Instance.CurrentCurrency);
                CurrencyManager.Instance.OnCurrencyChanged += UpdateCurrencyText;
            }

            if (GameManager.Instance != null)
            {
                UpdateServerHealthText(GameManager.Instance.ServerHealth);
                GameManager.Instance.OnServerHealthChanged += UpdateServerHealthText;
            }
        }

        private void OnDestroy()
        {
            // 메모리 누수를 막기 위해 구독 해제
            if (CurrencyManager.Instance != null)
                CurrencyManager.Instance.OnCurrencyChanged -= UpdateCurrencyText;

            if (GameManager.Instance != null)
                GameManager.Instance.OnServerHealthChanged -= UpdateServerHealthText;
        }

        private void UpdateCurrencyText(int amount)
        {
            if (currencyText != null)
                currencyText.text = $"크레딧: {amount}";
        }

        private void UpdateServerHealthText(int health)
        {
            if (serverHealthText != null)
                serverHealthText.text = $"서버 체력: {health}";
        }

        /// <summary>
        /// WaveSpawner의 OnWaveStarted 이벤트에 연결해서 사용하세요.
        /// </summary>
        public void UpdateWaveText(int currentWaveIndex, int totalWaves)
        {
            if (waveText != null)
                waveText.text = $"웨이브: {currentWaveIndex + 1} / {totalWaves}";
        }
    }
}
