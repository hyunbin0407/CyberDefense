using System.Collections.Generic;
using UnityEngine;
using CyberDefense.Core;
using CyberDefense.Data;

namespace CyberDefense.UI
{
    /// <summary>
    /// 인벤토리(상자) 패널: 전체 상점 아이템 목록 중 InventoryManager에 보유 중인 것만 필터링해서 표시합니다.
    /// 상점과 동일한 ShopItemSlot 프리팹을 재사용하되, 구매 버튼 대신 "보유중" 표시만 합니다.
    /// </summary>
    public class InventoryPanelController : MonoBehaviour
    {
        [SerializeField] private List<ShopItemData> allItems = new List<ShopItemData>();
        [SerializeField] private GameObject itemSlotPrefab;
        [SerializeField] private Transform slotContainer;

        private readonly List<GameObject> spawnedSlots = new List<GameObject>();

        private void OnEnable()
        {
            RefreshInventory();

            if (InventoryManager.Instance != null)
                InventoryManager.Instance.OnInventoryChanged += RefreshInventory;
        }

        private void OnDisable()
        {
            if (InventoryManager.Instance != null)
                InventoryManager.Instance.OnInventoryChanged -= RefreshInventory;
        }

        private void RefreshInventory()
        {
            ClearSlots();

            if (itemSlotPrefab == null || slotContainer == null || InventoryManager.Instance == null) return;

            foreach (var item in allItems)
            {
                if (item == null || !InventoryManager.Instance.HasItem(item.itemId)) continue;

                var slotGO = Instantiate(itemSlotPrefab, slotContainer);
                spawnedSlots.Add(slotGO);

                var slot = slotGO.GetComponent<ShopItemSlot>();
                if (slot != null) slot.SetupForInventory(item);
            }
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
