using System;
using UnityEngine;

namespace CyberDefense.Economy
{
    /// <summary>
    /// 게임 내 재화("사이버 크레딧")를 관리하는 싱글톤입니다.
    /// 타워 건설/업그레이드 비용 지불, 적 처치 보상 지급에 사용됩니다.
    /// </summary>
    public class CurrencyManager : MonoBehaviour
    {
        public static CurrencyManager Instance { get; private set; }

        [SerializeField] private int startingCurrency = 100;

        public int CurrentCurrency { get; private set; }

        public event Action<int> OnCurrencyChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            CurrentCurrency = startingCurrency;
        }

        public bool CanAfford(int amount) => CurrentCurrency >= amount;

        /// <summary>
        /// 비용을 지불합니다. 잔액이 부족하면 false를 반환하고 아무 변화도 없습니다.
        /// </summary>
        public bool TrySpend(int amount)
        {
            if (!CanAfford(amount)) return false;

            CurrentCurrency -= amount;
            OnCurrencyChanged?.Invoke(CurrentCurrency);
            return true;
        }

        public void AddCurrency(int amount)
        {
            if (amount <= 0) return;
            CurrentCurrency += amount;
            OnCurrencyChanged?.Invoke(CurrentCurrency);
        }

        /// <summary>
        /// 난이도 등 게임 시작 시점의 보정치를 시작 크레딧에 반영합니다.
        /// AddCurrency와 달리 음수(차감)도 허용합니다.
        /// </summary>
        public void ApplyStartingBonus(int amount)
        {
            if (amount == 0) return;

            CurrentCurrency = Mathf.Max(0, CurrentCurrency + amount);
            OnCurrencyChanged?.Invoke(CurrentCurrency);
        }
    }
}
