using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using CyberDefense.Data;

namespace CyberDefense.UI
{
    /// <summary>
    /// 상점/인벤토리 아이템 슬롯 하나의 표시를 담당합니다.
    /// ShopController/InventoryPanelController가 슬롯 프리팹을 Instantiate한 뒤 Setup 함수를 호출해서 내용을 채웁니다.
    /// </summary>
    public class ShopItemSlot : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text priceText;
        [SerializeField] private Button actionButton;
        [SerializeField] private TMP_Text actionButtonText;

        /// <summary>
        /// 상점용: 구매 가능한 아이템으로 슬롯을 채웁니다. 이미 보유 중이면 "보유중"으로 표시하고 버튼을 비활성화합니다.
        /// </summary>
        public void SetupForShop(ShopItemData item, bool alreadyOwned, Action onBuyClicked)
        {
            if (iconImage != null) iconImage.sprite = item.icon;
            if (nameText != null) nameText.text = item.displayName;
            if (priceText != null) priceText.text = alreadyOwned ? string.Empty : $"{item.price} 코인";
            if (actionButtonText != null) actionButtonText.text = alreadyOwned ? "보유중" : "구매";

            if (actionButton != null)
            {
                actionButton.interactable = !alreadyOwned;
                actionButton.onClick.RemoveAllListeners();
                if (!alreadyOwned && onBuyClicked != null)
                    actionButton.onClick.AddListener(() => onBuyClicked());
            }
        }

        /// <summary>
        /// 인벤토리용: 보유 중인 아이템을 "보유중" 표시로만 채웁니다(구매 버튼 없음).
        /// </summary>
        public void SetupForInventory(ShopItemData item)
        {
            if (iconImage != null) iconImage.sprite = item.icon;
            if (nameText != null) nameText.text = item.displayName;
            if (priceText != null) priceText.text = string.Empty;
            if (actionButtonText != null) actionButtonText.text = "보유중";

            if (actionButton != null)
            {
                actionButton.interactable = false;
                actionButton.onClick.RemoveAllListeners();
            }
        }
    }
}
