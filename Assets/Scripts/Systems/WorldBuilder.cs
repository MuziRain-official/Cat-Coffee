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
        public static Vector3 CoffeeMachinePos = new Vector3(-4f, -3f, 0f);
        public static Vector3 CounterPos       = new Vector3(4f, -3f, 0f);
        public static Vector3 CatNestPos       = new Vector3(-5f, 4f, 0f);
        public static Vector3 PlayerSpawnPos   = new Vector3(0f, -1f, 0f);

        // 6 个座位位置（顾客会坐这里）
        public static readonly Vector3[] Seats = new Vector3[]
        {
            new Vector3(-3f, 3.8f, 0f), new Vector3(-3f, 2.2f, 0f),
            new Vector3( 0f, 3.8f, 0f), new Vector3( 0f, 2.2f, 0f),
            new Vector3( 3f, 3.8f, 0f), new Vector3( 3f, 2.2f, 0f),
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
            NewSprite("Floor", new Vector3(0f, 0f, 0f), new Vector2(RoomW, RoomH),
                new Color(0.78f, 0.71f, 0.58f), -2);
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
            // 吧台区设备（占位方块，带碰撞，挂 Station 交互）
            MakeStation("CoffeeMachine", CoffeeMachinePos, new Vector2(1.6f, 1.6f), new Color(0.4f, 0.3f, 0.25f), StationType.CoffeeMachine);
            MakeStation("Counter", CounterPos, new Vector2(1.6f, 1.6f), new Color(0.5f, 0.45f, 0.35f), StationType.Counter);

            // 3 张桌子（带碰撞，主角不可穿）
            MakeFurniture("Table_A", new Vector3(-3f, 3f, 0f), new Vector2(2f, 1f), new Color(0.45f, 0.35f, 0.25f));
            MakeFurniture("Table_B", new Vector3(0f, 3f, 0f), new Vector2(2f, 1f), new Color(0.45f, 0.35f, 0.25f));
            MakeFurniture("Table_C", new Vector3(3f, 3f, 0f), new Vector2(2f, 1f), new Color(0.45f, 0.35f, 0.25f));

            // 猫窝（挂 Station 交互，无碰撞）
            MakeStation("CatNest", CatNestPos, new Vector2(1.4f, 1.4f), new Color(0.85f, 0.6f, 0.5f), StationType.CatNest);

            // 6 个座位标记（无碰撞）
            for (int i = 0; i < Seats.Length; i++)
                NewSprite("Seat_" + i, Seats[i], new Vector2(0.9f, 0.9f), new Color(0.6f, 0.65f, 0.7f), -1);
        }

        private void BuildPlayer()
        {
            var player = NewSprite("Player", PlayerSpawnPos, new Vector2(0.9f, 0.9f), new Color(0.3f, 0.8f, 0.7f), 5);
            var rb = player.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            player.AddComponent<BoxCollider2D>(); // 默认 1x1，随 scale 拉伸
            player.AddComponent<PlayerController>();
            player.AddComponent<InteractionController>();
        }

        private void MakeWall(Vector3 pos, Vector2 size)
        {
            var go = NewSprite("Wall", pos, size, new Color(0.4f, 0.35f, 0.3f), 0);
            go.AddComponent<BoxCollider2D>();
        }

        private GameObject MakeFurniture(string name, Vector3 pos, Vector2 size, Color color)
        {
            var go = NewSprite(name, pos, size, color, 0);
            go.AddComponent<BoxCollider2D>();
            return go;
        }

        private GameObject MakeStation(string name, Vector3 pos, Vector2 size, Color color, StationType type)
        {
            var go = MakeFurniture(name, pos, size, color);
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
