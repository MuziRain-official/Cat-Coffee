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
            // 头
            c.FillCircle(8, 4, 3, PixelPalette.Skin);
            // 头发
            c.FillRect(5, 1, 11, 4, PixelPalette.HairBrown);
            c.FillRect(6, 1, 10, 2, PixelPalette.HairBrown);
            // 眼睛
            c.FillRect(6, 4, 7, 5, PixelPalette.DarkWood);
            c.FillRect(9, 4, 10, 5, PixelPalette.DarkWood);
            // 身体（围裙）
            c.FillRect(5, 7, 11, 13, PixelPalette.Blue);
            c.FillRect(6, 8, 10, 13, PixelPalette.Cream); // 围裙
            c.FillRect(5, 7, 11, 8, PixelPalette.Cream);  // 围裙带
            // 手臂
            c.FillRect(3, 8, 5, 11, PixelPalette.Blue);
            c.FillRect(11, 8, 13, 11, PixelPalette.Blue);
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

        /// <summary>按资产名获取程序化 sprite。</summary>
        public static Sprite Get(string assetName) => assetName switch
        {
            "coffee_machine" => CoffeeMachine(),
            "counter" => Counter(),
            "warmer" => Warmer(),
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
