using System.Collections.Generic;
using UnityEngine;

namespace CatCafe
{
    /// <summary>
    /// 表现层桥接器：把 GameFlow 里的顾客列表同步到场景实体。
    /// 顾客新增→生成 CustomerView（分配座位）；顾客离开→销毁实体并释放座位。
    /// 在 LateUpdate 同步，保证在 GameManager.Update 的 Flow.Tick 之后。
    /// </summary>
    public class WorldController : MonoBehaviour
    {
        private readonly Dictionary<Customer, CustomerView> _views = new Dictionary<Customer, CustomerView>();
        private readonly bool[] _seatOccupied = new bool[WorldBuilder.Seats.Length];
        private GameFlow _lastFlow;

        private void LateUpdate()
        {
            var flow = GameManager.Instance != null ? GameManager.Instance.Flow : null;

            // Flow 被重置（重开一天）→ 清空所有实体
            if (flow != _lastFlow)
            {
                ClearAll();
                _lastFlow = flow;
            }

            if (flow == null) return;
            SyncCustomers(flow);
        }

        private void SyncCustomers(GameFlow flow)
        {
            var customers = flow.Customers;

            // 新增顾客 → 生成实体
            foreach (var c in customers)
            {
                if (_views.ContainsKey(c)) continue;

                int seat = FindFreeSeat();
                if (seat < 0) continue; // 座位满了（理论上 Flow 已限流）

                _seatOccupied[seat] = true;
                float maxPatience = GameManager.Instance.Config.customerPatienceSeconds;
                var view = CustomerView.Create(c, seat, WorldBuilder.Seats[seat], flow, maxPatience);
                _views[c] = view;
            }

            // 移除顾客 → 销毁实体
            var toRemove = new List<Customer>();
            foreach (var kv in _views)
            {
                if (!ContainsCustomer(customers, kv.Key)) toRemove.Add(kv.Key);
            }
            foreach (var c in toRemove)
            {
                var view = _views[c];
                if (view.SeatIndex >= 0 && view.SeatIndex < _seatOccupied.Length)
                    _seatOccupied[view.SeatIndex] = false;
                Destroy(view.gameObject);
                _views.Remove(c);
            }
        }

        private int FindFreeSeat()
        {
            for (int i = 0; i < _seatOccupied.Length; i++)
                if (!_seatOccupied[i]) return i;
            return -1;
        }

        private static bool ContainsCustomer(IReadOnlyList<Customer> list, Customer target)
        {
            for (int i = 0; i < list.Count; i++)
                if (ReferenceEquals(list[i], target)) return true;
            return false;
        }

        private void ClearAll()
        {
            foreach (var kv in _views)
                if (kv.Value != null) Destroy(kv.Value.gameObject);
            _views.Clear();
            for (int i = 0; i < _seatOccupied.Length; i++) _seatOccupied[i] = false;
        }
    }
}
