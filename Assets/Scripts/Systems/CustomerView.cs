using UnityEngine;

namespace CatCafe
{
    /// <summary>
    /// 顾客的空间实体（表现层）。绑定一个逻辑层 Customer，
    /// 负责可视化状态（颜色表耐心/用餐），并作为"上菜"交互点。
    /// </summary>
    public class CustomerView : MonoBehaviour, IInteractable
    {
        public Customer Customer { get; private set; }
        public int SeatIndex { get; private set; }
        public float interactRadius = 1.6f;

        private SpriteRenderer _body;
        private GameFlow _flow;
        private float _maxPatience;

        private static readonly Color WaitingColor = new Color(0.55f, 0.75f, 0.95f);
        private static readonly Color EatingColor = new Color(1f, 0.75f, 0.3f);

        public bool IsInteractable => _flow != null && Customer != null;

        public bool IsPlayerNear(Vector3 playerPos) =>
            Vector2.Distance(transform.position, playerPos) <= interactRadius;

        /// <summary>创建顾客实体并绑定逻辑对象。</summary>
        public static CustomerView Create(Customer customer, int seatIndex, Vector3 seatPos,
            GameFlow flow, float maxPatience)
        {
            var go = new GameObject("Customer", typeof(SpriteRenderer), typeof(CustomerView));
            go.transform.position = seatPos;
            var sr = go.GetComponent<SpriteRenderer>();
            sr.sprite = SpriteUtil.White;
            sr.sortingOrder = 3;

            var view = go.GetComponent<CustomerView>();
            view.Customer = customer;
            view.SeatIndex = seatIndex;
            view._flow = flow;
            view._maxPatience = maxPatience;
            view._body = sr;
            return view;
        }

        private void Update()
        {
            if (Customer == null) return;

            switch (Customer.Phase)
            {
                case CustomerPhase.Waiting:
                    // 颜色随耐心从绿(充足)渐变到红(快走)
                    float ratio = Mathf.Clamp01(Customer.RemainingPatience / _maxPatience);
                    _body.color = Color.Lerp(new Color(1f, 0.3f, 0.3f), WaitingColor, ratio);
                    break;

                case CustomerPhase.Eating:
                    _body.color = EatingColor;
                    break;

                default:
                    _body.color = Color.gray;
                    break;
            }
        }

        // —— 交互：上菜 ——
        public void OnInteractPrimary()
        {
            if (Customer == null || Customer.Phase != CustomerPhase.Waiting) return;
            _flow?.ServeTo(Customer);
        }

        public void OnInteractSecondary() { }
        public void OnInteractTertiary() { }
    }
}
