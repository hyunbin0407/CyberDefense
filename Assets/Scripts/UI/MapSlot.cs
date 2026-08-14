using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using CyberDefense.Data;

namespace CyberDefense.UI
{
    /// <summary>
    /// 맵 선택 화면의 맵 슬롯 하나를 표시합니다. isUnlocked가 false면 잠금 표시를 하고 선택할 수 없게 합니다.
    /// </summary>
    public class MapSlot : MonoBehaviour
    {
        [SerializeField] private Image thumbnailImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private Button selectButton;

        public void Setup(MapData map, Action onSelected)
        {
            if (thumbnailImage != null && map.thumbnail != null) thumbnailImage.sprite = map.thumbnail;
            if (nameText != null) nameText.text = map.isUnlocked ? map.displayName : $"{map.displayName} (잠김)";

            if (selectButton != null)
            {
                selectButton.interactable = map.isUnlocked;
                selectButton.onClick.RemoveAllListeners();
                if (map.isUnlocked && onSelected != null)
                    selectButton.onClick.AddListener(() => onSelected());
            }
        }
    }
}
