using UnityEngine;

namespace CatCafe
{
    /// <summary>
    /// 咖啡馆世界搭建器（代码自建，零序列化依赖）。
    /// 运行时搭建：正俯视正交相机、地板、四面墙、吧台、3 桌 6 座、主角。
    /// 碰撞规则：墙/桌子/设备 = 静态 Collider2D（不可穿过）；
    ///           座位 = 地面标记（无碰撞），猫/顾客后续加入（可穿过）。
    /// </summary>
    public class WorldBuilder : MonoBehaviour
    {
        // 房间内区尺寸（世界单位）
        public const float RoomW = 16f;
        public const float RoomH = 12f;

        // 关键位置（供交互系统引用）
        public static Vector3 CoffeeMachinePos = new Vector3(-4.5f, -4.5f, 0f);
        public static Vector3 FrotherPos       = new Vector3(-1.5f, -4.5f, 0f); // 奶泡机
        public static Vector3 CounterPos       = new Vector3(1.5f, -4.5f, 0f);
        public static Vector3 WarmerPos        = new Vector3(4.5f, -4.5f, 0f);  // 保温台
        public static Vector3 CatNestPos       = new Vector3(-6f, -1f, 0f);    // 默认猫窝（Nest 区）
        public static Vector3 PlayerSpawnPos   = new Vector3(0f, -2f, 0f);     // 主角出生在吧台上方活动区

        // 三个猫垫位置（吧台/座位/门口）
        public static Vector3 CatPadBarPos  = new Vector3(-6f, -3.8f, 0f); // 吧台旁
        public static Vector3 CatPadSeatPos = new Vector3(0f, 1.2f, 0f);    // 座位区中间
        public static Vector3 CatPadDoorPos = new Vector3(6.5f, 3.5f, 0f);  // 门口旁

        /// <summary>根据区域返回猫垫位置（猫不在怀时定位用）。</summary>
        public static Vector3 PadPosition(CatZone zone) => zone switch
        {
            CatZone.Bar => CatPadBarPos,
            CatZone.Seat => CatPadSeatPos,
            CatZone.Door => CatPadDoorPos,
            _ => CatNestPos,
        };

        // 6 个座位位置（桌子下方，面向吧台，方便主角上菜）
        public static readonly Vector3[] Seats = new Vector3[]
        {
            new Vector3(-5.2f, 2.5f, 0f), new Vector3(-3.8f, 2.5f, 0f),
            new Vector3(-0.7f, 2.5f, 0f), new Vector3( 0.7f, 2.5f, 0f),
            new Vector3( 3.8f, 2.5f, 0f), new Vector3( 5.2f, 2.5f, 0f),
        };

        private void Start()
        {
            SetupCamera();
            BuildFloor();
            BuildWalls();
            BuildFurniture();
            BuildPlayer();
        }

        private void SetupCamera()
        {
            var camGo = GameObject.Find("Main Camera");
            if (camGo == null) camGo = new GameObject("Main Camera");
            var cam = camGo.GetComponent<Camera>();
            if (cam == null) cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = RoomH / 2f; // 垂直铺满房间
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.transform.rotation = Quaternion.identity;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.08f, 0.10f);
            camGo.tag = "MainCamera";
        }

        private void BuildFloor()
        {
            NewArtObject("Floor", "floor", new Vector3(0f, 0f, 0f), new Vector2(RoomW, RoomH), new Color(0.78f, 0.71f, 0.58f), -2);
        }

        private void BuildWalls()
        {
            float halfW = RoomW / 2f, halfH = RoomH / 2f, t = 0.6f;
            MakeWall(new Vector3(0f, halfH, 0f), new Vector2(RoomW, t));     // 上
            MakeWall(new Vector3(0f, -halfH, 0f), new Vector2(RoomW, t));    // 下
            MakeWall(new Vector3(-halfW, 0f, 0f), new Vector2(t, RoomH));    // 左
            MakeWall(new Vector3(halfW, 0f, 0f), new Vector2(t, RoomH));     // 右
        }

        private void BuildFurniture()
        {
            // —— 吧台设备一字排开（做咖啡少跑路）——
            var coffee = MakeStation("CoffeeMachine", CoffeeMachinePos, new Vector2(1.6f, 1.6f), new Color(0.4f, 0.3f, 0.25f), StationType.CoffeeMachine, "coffee_machine");
            coffee.AddComponent<CoffeeMachineView>(); // 萃取读条
            coffee.AddComponent<CoffeeSelectView>(); // 选菜品面板
            var frother = MakeStation("Frother", FrotherPos, new Vector2(1.6f, 1.6f), new Color(0.55f, 0.5f, 0.6f), StationType.Frother, "frother");
            frother.AddComponent<FrothGameView>(); // 下落音符表现层
            var counter = MakeStation("Counter", CounterPos, new Vector2(1.6f, 1.6f), new Color(0.5f, 0.45f, 0.35f), StationType.Counter, "counter");
            counter.AddComponent<LatteArtGameView>(); // 拉花九宫格视图
            var warmer = MakeStation("Warmer", WarmerPos, new Vector2(1.6f, 1.6f), new Color(0.55f, 0.5f, 0.4f), StationType.Warmer, "warmer");
            warmer.AddComponent<WarmerView>(); // 每杯新鲜度条
            warmer.AddComponent<WarmerSelectView>(); // 选择栏

            // —— 3 张桌子（上方，围绕吧台，圆桌用方形scale保持圆形）——
            MakeFurniture("Table_A", new Vector3(-4.5f, 3.6f, 0f), new Vector2(1.6f, 1.6f), new Color(0.45f, 0.35f, 0.25f), "table");
            MakeFurniture("Table_B", new Vector3(0f, 3.6f, 0f), new Vector2(1.6f, 1.6f), new Color(0.45f, 0.35f, 0.25f), "table");
            MakeFurniture("Table_C", new Vector3(4.5f, 3.6f, 0f), new Vector2(1.6f, 1.6f), new Color(0.45f, 0.35f, 0.25f), "table");

            // —— 三个猫垫（无碰撞）+ 猫实体 ——
            BuildCatPadsAndCat();

            // 6 个座位标记（无碰撞）
            for (int i = 0; i < Seats.Length; i++)
                NewArtObject("Seat_" + i, "chair", Seats[i], new Vector2(0.9f, 0.9f), new Color(0.6f, 0.65f, 0.7f), -1);
        }

        private void BuildCatPadsAndCat()
        {
            // 默认猫窝（Nest，猫初始位置）
            NewArtObject("CatNest", "cat_nest", CatNestPos, new Vector2(1.2f, 1.2f), new Color(0.85f, 0.6f, 0.5f), -1);

            // 三个猫垫（带 CatPad 组件）
            MakeCatPad("CatPad_Bar", CatPadBarPos, new Color(0.4f, 0.7f, 0.5f), CatZone.Bar);
            MakeCatPad("CatPad_Seat", CatPadSeatPos, new Color(0.4f, 0.6f, 0.8f), CatZone.Seat);
            MakeCatPad("CatPad_Door", CatPadDoorPos, new Color(0.8f, 0.7f, 0.4f), CatZone.Door);

            // 猫实体（贴图）
            var cat = NewArtObject("Cat", "cat", CatNestPos, new Vector2(0.9f, 0.9f), new Color(0.95f, 0.6f, 0.25f), 4);
            cat.AddComponent<CatView>();
        }

        private void MakeCatPad(string name, Vector3 pos, Color color, CatZone zone)
        {
            var go = NewArtObject(name, "cat_pad", pos, new Vector2(1.1f, 1.1f), color, -1);
            var pad = go.AddComponent<CatPad>();
            pad.Zone = zone;
        }

        private void BuildPlayer()
        {
            var player = NewArtObject("Player", "player", PlayerSpawnPos, new Vector2(0.9f, 0.9f), new Color(0.3f, 0.8f, 0.7f), 5);
            var rb = player.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            player.AddComponent<BoxCollider2D>(); // 默认 1x1，随 scale 拉伸
            player.AddComponent<PlayerController>();
            player.AddComponent<InteractionController>();
            player.AddComponent<PlayerCarryView>(); // 手中物品可视化
        }

        private void MakeWall(Vector3 pos, Vector2 size)
        {
            var go = NewArtObject("Wall", "wall", pos, size, new Color(0.4f, 0.35f, 0.3f), 0);
            go.AddComponent<BoxCollider2D>();
        }

        private GameObject MakeFurniture(string name, Vector3 pos, Vector2 size, Color color, string artName = null)
        {
            var go = string.IsNullOrEmpty(artName)
                ? NewSprite(name, pos, size, color, 0)
                : NewArtObject(name, artName, pos, size, color, 0);
            go.AddComponent<BoxCollider2D>();
            return go;
        }

        private GameObject MakeStation(string name, Vector3 pos, Vector2 size, Color color, StationType type, string artName = null)
        {
            var go = MakeFurniture(name, pos, size, color, artName);
            var st = go.AddComponent<Station>();
            st.Type = type;
            return go;
        }

        private GameObject NewSprite(string name, Vector3 pos, Vector2 size, Color color, int sortingOrder)
        {
            var go = new GameObject(name, typeof(SpriteRenderer));
            go.transform.position = pos;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            var sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = WhiteSprite();
            sr.color = color;
            sr.sortingOrder = sortingOrder;
            return go;
        }

        /// <summary>创建带贴图的物体（加载失败回退到色块）。</summary>
        private GameObject NewArtObject(string name, string artName, Vector3 pos, Vector2 size, Color fallbackColor, int sortingOrder)
        {
            var go = NewSprite(name, pos, size, fallbackColor, sortingOrder);
            ArtLoader.Apply(go.GetComponent<SpriteRenderer>(), artName);
            return go;
        }

        private static Sprite _whiteSprite;
        private static Sprite WhiteSprite()
        {
            if (_whiteSprite != null) return _whiteSprite;
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            _whiteSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            return _whiteSprite;
        }
    }
}
