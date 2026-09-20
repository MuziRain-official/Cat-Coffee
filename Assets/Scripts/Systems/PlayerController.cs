using UnityEngine;

namespace CatCafe
{
    /// <summary>
    /// 主角移动：WASD/方向键，Rigidbody2D 驱动。
    /// 撞静态 Collider2D（墙/桌子/设备）会停住，实现"家具不可穿过"。
    /// 猫/顾客无 Collider，主角可穿过。
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;

        private Rigidbody2D _rb;

        private void Awake() => _rb = GetComponent<Rigidbody2D>();

        private void Update()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            var dir = new Vector2(h, v);
            if (dir.sqrMagnitude > 1f) dir.Normalize();
            _rb.velocity = dir * moveSpeed;
        }
    }
}
