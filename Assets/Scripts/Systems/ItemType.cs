namespace CatCafe
{
    /// <summary>可购买的道具类型。</summary>
    public enum ItemType
    {
        QuickServe,   // 道具1：手上持菜品按Q自动送餐
        NoWait,       // 道具2：制作不再需要读条
        ExtraTables,  // 道具3：场景加两桌四椅
    }

    /// <summary>
    /// 道具定义（纯数据）。
    /// </summary>
    public static class ItemDef
    {
        public static string Name(ItemType t) => t switch
        {
            ItemType.QuickServe => "自动送餐",
            ItemType.NoWait => "免读条",
            ItemType.ExtraTables => "加两桌",
            _ => "未知",
        };

        public static string Desc(ItemType t) => t switch
        {
            ItemType.QuickServe => "手上持菜品时按 Q 直接送到最急的顾客手里",
            ItemType.NoWait => "萃取/奶泡游戏后不再需要读条",
            ItemType.ExtraTables => "场景增加两张桌子和四把椅子",
            _ => "",
        };

        public static int Price(ItemType t) => t switch
        {
            ItemType.QuickServe => 100,
            ItemType.NoWait => 200,
            ItemType.ExtraTables => 300,
            _ => 0,
        };

        public static readonly ItemType[] All = { ItemType.QuickServe, ItemType.NoWait, ItemType.ExtraTables };
    }
}
