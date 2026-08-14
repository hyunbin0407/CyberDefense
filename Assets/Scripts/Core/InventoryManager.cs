using System;
using System.Collections.Generic;
using UnityEngine;

namespace CyberDefense.Core
{
    /// <summary>
    /// 플레이어가 상점에서 구매한 아이템 ID 목록을 관리하는 싱글톤입니다.
    /// 씬이 바뀌어도 유지되고(DontDestroyOnLoad), PlayerPrefs에 JSON으로 저장되어 앱을 껐다 켜도 유지됩니다.
    /// </summary>
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }

        private const string InventoryPrefKey = "CyberDefense.Inventory";

        private List<string> ownedItemIds = new List<string>();

        public event Action OnInventoryChanged;

        /// <summary>
        /// JsonUtility는 최상위 배열/리스트를 직접 직렬화하지 못하므로 감싸는 래퍼입니다.
        /// </summary>
        [Serializable]
        private class InventorySaveData
        {
            public List<string> itemIds = new List<string>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            Load();
        }

        /// <summary>
        /// 이미 보유 중이면 아무 일도 하지 않습니다(중복 방지).
        /// </summary>
        public void AddItem(string itemId)
        {
            if (string.IsNullOrEmpty(itemId) || ownedItemIds.Contains(itemId)) return;

            ownedItemIds.Add(itemId);
            Save();
            OnInventoryChanged?.Invoke();
        }

        public bool HasItem(string itemId)
        {
            return !string.IsNullOrEmpty(itemId) && ownedItemIds.Contains(itemId);
        }

        private void Save()
        {
            var data = new InventorySaveData { itemIds = ownedItemIds };
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(InventoryPrefKey, json);
            PlayerPrefs.Save();
        }

        private void Load()
        {
            string json = PlayerPrefs.GetString(InventoryPrefKey, string.Empty);
            if (string.IsNullOrEmpty(json))
            {
                ownedItemIds = new List<string>();
                return;
            }

            var data = JsonUtility.FromJson<InventorySaveData>(json);
            ownedItemIds = data != null && data.itemIds != null ? data.itemIds : new List<string>();
        }
    }
}
