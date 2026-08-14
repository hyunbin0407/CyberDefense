using System;
using UnityEngine;

namespace CyberDefense.Core
{
    /// <summary>
    /// 게임 플레이 중에만 쓰는 인게임 크레딧(CurrencyManager)과는 완전히 별개인,
    /// 메인 메뉴/상점에서 쓰는 영속 재화("코인")를 관리하는 싱글톤입니다.
    /// 씬이 바뀌어도 유지되고(DontDestroyOnLoad), PlayerPrefs에 저장되어 앱을 껐다 켜도 유지됩니다.
    /// </summary>
    public class MetaCurrencyManager : MonoBehaviour
    {
        public static MetaCurrencyManager Instance { get; private set; }

        private const string CoinsPrefKey = "CyberDefense.MetaCoins";

        [Tooltip("최초 실행 시(저장된 값이 아직 없을 때) 지급되는 시작 코인")]
        [SerializeField] private int startingCoins = 500;

        public int CurrentCoins { get; private set; }

        public event Action<int> OnCoinsChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            CurrentCoins = PlayerPrefs.GetInt(CoinsPrefKey, startingCoins);
        }

        public void AddCoins(int amount)
        {
            if (amount <= 0) return;

            CurrentCoins += amount;
            Save();
            OnCoinsChanged?.Invoke(CurrentCoins);
        }

        /// <summary>
        /// 코인을 소비합니다. 잔액이 부족하면 false를 반환하고 아무 변화도 없습니다.
        /// </summary>
        public bool TrySpendCoins(int amount)
        {
            if (amount <= 0 || CurrentCoins < amount) return false;

            CurrentCoins -= amount;
            Save();
            OnCoinsChanged?.Invoke(CurrentCoins);
            return true;
        }

        private void Save()
        {
            PlayerPrefs.SetInt(CoinsPrefKey, CurrentCoins);
            PlayerPrefs.Save();
        }
    }
}
