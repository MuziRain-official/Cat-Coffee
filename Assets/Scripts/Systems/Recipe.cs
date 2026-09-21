namespace CatCafe
{
    /// <summary>菜品类型。</summary>
    public enum RecipeType
    {
        Latte,       // 拿铁：萃取时机条 → 读条 → 取料 → 装杯
        Cappuccino,  // 卡布奇诺：打奶泡节奏连击 → 取料 → 装杯
        CatPaw,      // 猫爪咖啡：萃取时机条 → 读条 → 取料 → 装杯 → 拉花
    }

    /// <summary>
    /// 菜品配方（纯数据，静态只读）。
    /// 定义每个菜品的工序链与显示信息。价格/成本在 GameConfigSO。
    /// </summary>
    public static class Recipe
    {
        /// <summary>菜品中文名。</summary>
        public static string Name(RecipeType t) => t switch
        {
            RecipeType.Latte => "拿铁",
            RecipeType.Cappuccino => "卡布奇诺",
            RecipeType.CatPaw => "猫爪咖啡",
            _ => "未知",
        };

        /// <summary>是否含"萃取"工序（拿铁/猫爪有，卡布没有）。</summary>
        public static bool NeedsBrew(RecipeType t) => t != RecipeType.Cappuccino;

        /// <summary>是否含"打奶泡"工序（仅卡布）。</summary>
        public static bool NeedsFroth(RecipeType t) => t == RecipeType.Cappuccino;

        /// <summary>是否含"拉花"工序（仅猫爪）。</summary>
        public static bool NeedsLatteArt(RecipeType t) => t == RecipeType.CatPaw;

        /// <summary>全部菜品列表。</summary>
        public static readonly RecipeType[] All = { RecipeType.Latte, RecipeType.Cappuccino, RecipeType.CatPaw };
    }
}
