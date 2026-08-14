using System.Collections.Generic;
using UnityEngine;
using CyberDefense.Core;
using CyberDefense.Data;

namespace CyberDefense.UI
{
    /// <summary>
    /// 상점 패널: ShopItemData 목록을 슬롯으로 표시하고, 구매 버튼 클릭 시
    /// MetaCurrencyManager에서 코인을 차감하고 InventoryManager에 아이템을 추가합니다.
    /// 패널은 SetActive로 켜고 끄는 방식이므로(파괴되지 않으므로) OnEnable/OnDisable에서
    /// 열릴 때마다 최신 보유 상태로 다시 그리고, 닫히면 이벤트 구독을 해제합니다.
    /// </summary>
    public class ShopController : MonoBehaviour
    {
        [SerializeField] private List<ShopItemData> items = new List<ShopItemData>();
        [SerializeField] private GameObject itemSlotPrefab;
        [SerializeField] private Transform slotContainer;

        private readonly List<GameObject> spawnedSlots = new List<GameObject>();

        private void OnEnable()
        {
            RefreshShop();

            if (InventoryManager.Instance != null)
                InventoryManager.Instance.OnInventoryChanged += RefreshShop;
        }

        private void OnDisable()
        {
            if (InventoryManager.Instance != null)
                InventoryManager.Instance.OnInventoryChanged -= RefreshShop;
        }

        /// <summary>
        /// 아이템 목록을 다시 그립니다(구매 후 "보유중" 상태 갱신 포함).
        /// </summary>
        private void RefreshShop()
        {
            ClearSlots();

            if (itemSlotPrefab == null || slotContainer == null) return;

            foreach (var item in items)
            {
                if (item == null) continue;

                var slotGO = Instantiate(itemSlotPrefab, slotContainer);
                spawnedSlots.Add(slotGO);

                var slot = slotGO.GetComponent<ShopItemSlot>();
                if (slot == null) continue;

                bool owned = InventoryManager.Instance != null && InventoryManager.Instance.HasItem(item.itemId);
                slot.SetupForShop(item, owned, () => TryBuyItem(item));
            }
        }

        private void TryBuyItem(ShopItemData item)
        {
            if (item == null) return;
            if (MetaCurrencyManager.Instance == null || InventoryManager.Instance == null) return;

            // 코인이 부족하면 TrySpendCoins가 false를 반환하고 차감도 안 됨 - 아무 일도 안 일어남
            if (!MetaCurrencyManager.Instance.TrySpendCoins(item.price)) return;

            InventoryManager.Instance.AddItem(item.itemId);
        }

        private void ClearSlots()
        {
            foreach (var slot in spawnedSlots)
            {
                if (slot != null) Destroy(slot);
            }
            spawnedSlots.Clear();
        }
    }
}
