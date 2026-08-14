using UnityEngine;

namespace CyberDefense.Data
{
    public enum ShopItemType
    {
        Cosmetic,
        TowerSkin,
        StartingBonus
    }

    /// <summary>
    /// 상점에서 판매하는 아이템 하나의 정보를 정의하는 데이터 에셋입니다.
    /// Project 창에서 우클릭 -> Create -> CyberDefense -> Shop Item Data 로 생성합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "NewShopItemData", menuName = "CyberDefense/Shop Item Data")]
    public class ShopItemData : ScriptableObject
    {
        [Header("기본 정보")]
        public string itemId;
        public string displayName = "아이템";
        [TextArea] public string description;
        public Sprite icon;

        [Header("가격/종류")]
        [Tooltip("코인(메타 재화) 단위 가격")]
        public int price = 100;
        public ShopItemType itemType;
    }
}
