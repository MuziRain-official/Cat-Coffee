using UnityEngine;

namespace CatCafe
{
    /// <summary>
    /// 猫的空间实体（表现层）。可喂食/铲屎/互动(E/Q/R)，可抱/放(F)。
    /// 被抱时跟随主角；放下时定位到对应区域的猫垫。
    /// </summary>
    public class CatView : MonoBehaviour, IInteractable
    {
        public float interactRadius = 1.8f;

        private GameFlow Flow => GameManager.Instance != null ? GameManager.Instance.Flow : null;
        private Transform _player;

        public bool IsInteractable => Flow != null;

        public bool IsPlayerNear(Vector3 playerPos) =>
            Vector2.Distance(transform.position, playerPos) <= interactRadius;

        // —— 喂食/铲屎/互动 ——
        public void OnInteractPrimary() => Flow?.FeedCat();
        public void OnInteractSecondary() => Flow?.CleanCat();
        public void OnInteractTertiary() => Flow?.PetCat();

        private void Update()
        {
            if (Flow == null) return;

            if (Flow.Cat.IsCarried)
            {
                if (_player == null) _player = GameObject.Find("Player")?.transform;
                if (_player != null)
                    transform.position = _player.position + new Vector3(0.4f, 0.4f, 0f);
            }
            else
            {
                // 定位到当前区域对应的猫垫
                var pos = WorldBuilder.PadPosition(Flow.Cat.Zone);
                transform.position = Vector3.Lerp(transform.position, pos, Time.deltaTime * 8f);
            }
        }
    }
}
