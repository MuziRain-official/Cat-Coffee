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
            ItemType.QuickServe => "猫咪跑腿",
            ItemType.NoWait => "猫咪加速",
            ItemType.ExtraTables => "猫咪扩张",
            _ => "未知",
        };

        public static string Desc(ItemType t) => t switch
        {
            ItemType.QuickServe => "手上持菜品时按 Q，猫咪帮你送到最急的顾客手里",
            ItemType.NoWait => "猫咪帮你盯着，萃取/奶泡后不再需要读条",
            ItemType.ExtraTables => "猫咪帮你收拾出两张新桌子和四把椅子",
            _ => "",
        };

        public static int Price(ItemType t) => t switch
        {
            ItemType.QuickServe => 300,
            ItemType.NoWait => 500,
            ItemType.ExtraTables => 800,
            _ => 0,
        };

        public static readonly ItemType[] All = { ItemType.QuickServe, ItemType.NoWait, ItemType.ExtraTables };
    }
}
