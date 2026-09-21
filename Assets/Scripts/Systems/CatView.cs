using UnityEngine;

namespace CatCafe
{
    /// <summary>
    /// 猫的空间实体（表现层）。只负责跟随/定位，不响应 E/Q/R。
    /// 抱猫/放猫由 InteractionController 的 F 键单独处理。
    /// </summary>
    public class CatView : MonoBehaviour
    {
        private GameFlow Flow => GameManager.Instance != null ? GameManager.Instance.Flow : null;
        private Transform _player;

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
                var pos = WorldBuilder.PadPosition(Flow.Cat.Zone);
                transform.position = Vector3.Lerp(transform.position, pos, Time.deltaTime * 8f);
            }
        }
    }
}
