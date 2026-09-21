using UnityEngine;

namespace CatCafe
{
    /// <summary>
    /// 程序化像素占位图生成器：用代码在像素网格上绘制奶油复古风的游戏资产。
    /// 比 AI 后处理更干净、统一、像素味浓。AI 贴图保留在 Assets/Art 备用。
    /// </summary>
    public static class PixelArtGenerator
    {
        private const int N = 16; // 16×16 网格

        public static Sprite CoffeeMachine()
        {
            var c = new PixelCanvas(N);
            // 机身
            c.FillRect(3, 4, 13, 14, PixelPalette.LightWood);
            c.FillRect(4, 3, 12, 4, PixelPalette.Wood);      // 顶部
            c.FillRect(3, 13, 13, 14, PixelPalette.DarkWood); // 底座
            // 冲煮头
            c.FillRect(6, 5, 10, 8, PixelPalette.CreamDark);
            c.FillRect(7, 6, 9, 7, PixelPalette.CoffeeBrown); // 咖啡出口
            // 压力表
            c.FillCircle(11, 9, 2, PixelPalette.White);
            c.FillCircle(11, 9, 1, PixelPalette.DarkWood);
            // 蒸汽棒
            c.FillRect(4, 5, 5, 9, PixelPalette.Cream);
            // 按钮
            c.FillRect(5, 10, 7, 11, PixelPalette.Green);
            c.FillRect(8, 10, 10, 11, PixelPalette.Red);
            return c.ToSprite();
        }

        public static Sprite Counter()
        {
            var c = new PixelCanvas(N);
            c.FillRect(2, 5, 14, 14, PixelPalette.Wood);
            c.FillRect(2, 3, 14, 5, PixelPalette.LightWood); // 台面
            c.FillRect(2, 13, 14, 14, PixelPalette.DarkWood); // 底部
            // 台面上摆件
            c.FillRect(4, 3, 5, 4, PixelPalette.Cream);
            c.FillRect(11, 3, 12, 4, PixelPalette.Cream);
            return c.ToSprite();
        }

        public static Sprite Warmer()
        {
            var c = new PixelCanvas(N);
            c.FillRect(2, 6, 14, 14, PixelPalette.DarkWood);
            c.FillRect(2, 3, 14, 6, PixelPalette.Wood); // 台面
            // 玻璃展示区
            c.FillRect(3, 7, 13, 13, PixelPalette.White);
            c.FillRect(4, 8, 6, 12, PixelPalette.MilkFoam); // 保温中的杯子
            c.FillRect(7, 8, 9, 12, PixelPalette.MilkFoam);
            c.FillRect(10, 8, 12, 12, PixelPalette.MilkFoam);
            return c.ToSprite();
        }

        public static Sprite Frother()
        {
            var c = new PixelCanvas(N);
            c.FillRect(3, 3, 13, 13, PixelPalette.CreamDark); // 机身
            c.FillRect(4, 4, 12, 12, PixelPalette.White);     // 奶罐
            // 奶泡
            c.FillRect(5, 5, 11, 8, PixelPalette.MilkFoam);
            // 蒸汽口
            c.FillRect(6, 2, 10, 4, PixelPalette.Wood);
            // 底座
            c.FillRect(3, 12, 13, 14, PixelPalette.DarkWood);
            return c.ToSprite();
        }

        public static Sprite CoffeeCup()
        {
            var c = new PixelCanvas(N);
            // 杯身（上宽下窄）
            c.FillRect(5, 6, 11, 7, PixelPalette.Cream);
            c.FillRect(5, 7, 11, 13, PixelPalette.CreamDark);
            c.FillRect(5, 12, 11, 13, PixelPalette.LightWood); // 杯底
            // 杯口奶泡
            c.FillRect(4, 5, 12, 7, PixelPalette.MilkFoam);
            c.FillRect(5, 6, 11, 7, PixelPalette.Cream);
            // 把手
            c.RectOutline(11, 8, 14, 11, PixelPalette.CreamDark);
            return c.ToSprite();
        }

        /// <summary>按菜品返回成品杯图标（拿铁=奶白杯、卡布=高奶泡、猫爪=带爪印）。</summary>
        public static Sprite CupIcon(RecipeType recipe)
        {
            var c = new PixelCanvas(N);
            switch (recipe)
            {
                case RecipeType.Latte:
                    c.FillRect(5, 6, 11, 13, PixelPalette.CreamDark);
                    c.FillRect(4, 5, 12, 7, PixelPalette.MilkFoam);
                    c.RectOutline(11, 8, 14, 11, PixelPalette.CreamDark);
                    break;

                case RecipeType.Cappuccino:
                    c.FillRect(5, 6, 11, 13, PixelPalette.CreamDark);
                    // 高高奶泡
                    c.FillRect(4, 3, 12, 8, PixelPalette.MilkFoam);
                    c.FillRect(5, 4, 11, 8, PixelPalette.Cream);
                    c.RectOutline(11, 8, 14, 11, PixelPalette.CreamDark);
                    break;

                case RecipeType.CatPaw:
                    c.FillRect(5, 6, 11, 13, PixelPalette.CreamDark);
                    c.FillRect(4, 5, 12, 7, PixelPalette.MilkFoam);
                    // 猫爪印（三个小圆+一个大圆）
                    c.FillCircle(6, 8, 1, PixelPalette.CoffeeBrown);
                    c.FillCircle(8, 7, 1, PixelPalette.CoffeeBrown);
                    c.FillCircle(10, 8, 1, PixelPalette.CoffeeBrown);
                    c.FillCircle(8, 10, 2, PixelPalette.CoffeeBrown);
                    c.RectOutline(11, 8, 14, 11, PixelPalette.CreamDark);
                    break;
            }
            return c.ToSprite();
        }

        public static Sprite Cat()
        {
            var c = new PixelCanvas(N);
            // 身体
            c.FillCircle(8, 9, 5, PixelPalette.Orange);
            c.FillCircle(8, 10, 5, PixelPalette.Orange);
            // 头
            c.FillCircle(8, 5, 4, PixelPalette.LightOrange);
            // 耳朵
            c.FillRect(4, 1, 6, 4, PixelPalette.LightOrange);
            c.FillRect(10, 1, 12, 4, PixelPalette.LightOrange);
            c.FillRect(5, 2, 6, 4, PixelPalette.Orange);
            c.FillRect(10, 2, 11, 4, PixelPalette.Orange);
            // 眼睛
            c.FillRect(6, 5, 7, 6, PixelPalette.DarkWood);
            c.FillRect(9, 5, 10, 6, PixelPalette.DarkWood);
            // 鼻子
            c.Set(8, 6, PixelPalette.Red);
            // 尾巴
            c.FillRect(13, 8, 14, 12, PixelPalette.Orange);
            return c.ToSprite();
        }

        public static Sprite Player()
        {
            var c = new PixelCanvas(N);
            // 头发（更圆润的刘海 + 双马尾）
            c.FillCircle(8, 3, 4, PixelPalette.HairBrown);
            c.FillRect(5, 1, 11, 4, PixelPalette.HairBrown); // 头顶
            // 双马尾（两侧）
            c.FillRect(3, 3, 5, 8, PixelPalette.HairBrown);
            c.FillRect(11, 3, 13, 8, PixelPalette.HairBrown);
            // 脸
            c.FillCircle(8, 5, 3, PixelPalette.Skin);
            // 眼睛（大而圆）
            c.FillRect(6, 5, 7, 6, PixelPalette.DarkWood);
            c.FillRect(9, 5, 10, 6, PixelPalette.DarkWood);
            // 腮红
            c.Set(5, 7, PixelPalette.LightOrange);
            c.Set(10, 7, PixelPalette.LightOrange);
            // 身体（围裙，圆润）
            c.FillRect(5, 8, 11, 14, PixelPalette.Cream); // 围裙主体
            c.FillRect(4, 8, 12, 9, PixelPalette.CreamDark); // 围裙上沿
            // 围裙口袋
            c.FillRect(6, 10, 10, 12, PixelPalette.CreamDark);
            // 蝴蝶结
            c.FillRect(7, 8, 9, 9, PixelPalette.Red);
            // 手臂（更自然）
            c.FillRect(3, 9, 5, 12, PixelPalette.CreamDark);
            c.FillRect(11, 9, 13, 12, PixelPalette.CreamDark);
            return c.ToSprite();
        }

        public static Sprite Customer()
        {
            var c = new PixelCanvas(N);
            // 头
            c.FillCircle(8, 4, 3, PixelPalette.Skin);
            // 头发
            c.FillRect(5, 1, 11, 4, PixelPalette.CoffeeBrown);
            // 眼睛
            c.FillRect(6, 4, 7, 5, PixelPalette.DarkWood);
            c.FillRect(9, 4, 10, 5, PixelPalette.DarkWood);
            // 身体
            c.FillRect(5, 7, 11, 13, PixelPalette.Green);
            c.FillRect(6, 8, 10, 13, PixelPalette.Cream);
            return c.ToSprite();
        }

        public static Sprite Table()
        {
            var c = new PixelCanvas(N);
            c.FillCircle(8, 8, 7, PixelPalette.Wood);
            c.FillCircle(8, 8, 6, PixelPalette.LightWood);
            // 桌面装饰（奶油色桌布边缘）
            c.CircleOutline(8, 8, 5, PixelPalette.Cream);
            return c.ToSprite();
        }

        public static Sprite Chair()
        {
            var c = new PixelCanvas(N);
            // 坐垫
            c.FillRect(3, 4, 13, 8, PixelPalette.Cream);
            c.FillRect(3, 3, 13, 4, PixelPalette.CreamDark); // 靠背
            // 椅腿
            c.FillRect(4, 8, 5, 13, PixelPalette.DarkWood);
            c.FillRect(11, 8, 12, 13, PixelPalette.DarkWood);
            return c.ToSprite();
        }

        public static Sprite CatNest()
        {
            var c = new PixelCanvas(N);
            c.FillCircle(8, 8, 7, PixelPalette.CreamDark);
            c.FillCircle(8, 8, 5, PixelPalette.Cream);
            // 内垫
            c.FillCircle(8, 8, 3, PixelPalette.LightOrange);
            return c.ToSprite();
        }

        public static Sprite CatPad()
        {
            var c = new PixelCanvas(N);
            c.FillRect(2, 3, 14, 13, PixelPalette.Green);
            c.RectOutline(2, 3, 14, 13, PixelPalette.GreenDark);
            // 爪印
            c.FillCircle(8, 8, 2, PixelPalette.Cream);
            return c.ToSprite();
        }

        public static Sprite Floor()
        {
            var c = new PixelCanvas(N);
            // 棋盘格木地板
            for (int y = 0; y < N; y++)
                for (int x = 0; x < N; x++)
                {
                    bool light = ((x / 2) + (y / 2)) % 2 == 0;
                    c.Set(x, y, light ? PixelPalette.LightWood : PixelPalette.Wood);
                }
            return c.ToSprite();
        }

        public static Sprite Wall()
        {
            var c = new PixelCanvas(N);
            c.FillRect(0, 0, N, N, PixelPalette.CreamDark);
            // 墙砖缝
            for (int y = 4; y < N; y += 5)
                for (int x = 0; x < N; x++)
                    c.Set(x, y, PixelPalette.Cream);
            return c.ToSprite();
        }

        /// <summary>原料图标（萃取液=深棕杯，奶泡=浅奶杯）。</summary>
        public static Sprite IngredientIcon(RecipeType recipe)
        {
            var c = new PixelCanvas(N);
            if (recipe == RecipeType.Cappuccino)
            {
                // 奶泡原料：浅奶色杯
                c.FillRect(5, 5, 11, 13, PixelPalette.CreamDark);
                c.FillRect(5, 4, 11, 6, PixelPalette.MilkFoam);
                c.RectOutline(11, 7, 14, 10, PixelPalette.CreamDark);
            }
            else
            {
                // 咖啡液原料：深棕杯
                c.FillRect(5, 5, 11, 13, PixelPalette.CreamDark);
                c.FillRect(5, 4, 11, 6, PixelPalette.CoffeeBrown);
                c.RectOutline(11, 7, 14, 10, PixelPalette.CreamDark);
            }
            return c.ToSprite();
        }

        /// <summary>就餐中图标（叉子/用餐）。</summary>
        public static Sprite EatingIcon()
        {
            var c = new PixelCanvas(N);
            c.FillCircle(8, 8, 7, PixelPalette.White);
            c.CircleOutline(8, 8, 7, PixelPalette.CreamDark);
            // 简单"餐盘"：一个盘子 + 食物
            c.FillCircle(8, 8, 4, PixelPalette.CreamDark);
            c.FillCircle(8, 8, 2, PixelPalette.Orange);
            c.FillCircle(8, 8, 1, PixelPalette.CoffeeBrown);
            return c.ToSprite();
        }

        /// <summary>气泡底（白色圆底）。</summary>
        public static Sprite BubbleBg()
        {
            var c = new PixelCanvas(N);
            c.FillCircle(8, 8, 7, PixelPalette.White);
            c.CircleOutline(8, 8, 7, PixelPalette.CreamDark);
            return c.ToSprite();
        }

        /// <summary>道具像素图标。</summary>
        public static Sprite ItemIcon(ItemType item)
        {
            var c = new PixelCanvas(N);
            switch (item)
            {
                case ItemType.QuickServe: // 猫咪跑腿：猫爪印
                    c.FillCircle(8, 8, 7, PixelPalette.Orange);
                    c.FillCircle(6, 6, 2, PixelPalette.LightOrange);
                    c.FillCircle(8, 4, 2, PixelPalette.LightOrange);
                    c.FillCircle(10, 6, 2, PixelPalette.LightOrange);
                    c.FillCircle(8, 9, 3, PixelPalette.LightOrange);
                    break;

                case ItemType.NoWait: // 猫咪加速：闪电
                    c.FillCircle(8, 8, 7, PixelPalette.LightOrange);
                    c.FillRect(9, 2, 12, 8, PixelPalette.Orange);
                    c.FillRect(6, 6, 9, 13, PixelPalette.Orange);
                    c.FillRect(8, 1, 10, 6, PixelPalette.Orange);
                    break;

                case ItemType.ExtraTables: // 猫咪扩张：桌子
                    c.FillCircle(8, 8, 7, PixelPalette.Wood);
                    c.FillCircle(8, 8, 5, PixelPalette.LightWood);
                    c.FillRect(7, 4, 9, 12, PixelPalette.DarkWood);
                    break;
            }
            return c.ToSprite();
        }

        /// <summary>按资产名获取程序化 sprite。</summary>
        public static Sprite Get(string assetName) => assetName switch
        {
            "coffee_machine" => CoffeeMachine(),
            "counter" => Counter(),
            "warmer" => Warmer(),
            "frother" => Frother(),
            "coffee_cup" => CoffeeCup(),
            "cat" => Cat(),
            "player" => Player(),
            "customer" => Customer(),
            "table" => Table(),
            "chair" => Chair(),
            "cat_nest" => CatNest(),
            "cat_pad" => CatPad(),
            "floor" => Floor(),
            "wall" => Wall(),
            _ => null,
        };
    }
}
