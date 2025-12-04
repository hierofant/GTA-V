using UnityEngine;
using UnityEngine.UI;

namespace Sandbox.UI
{
    /// <summary>
    /// Keeps UI icon facing up while matching world rotation.
    /// </summary>
    public class MinimapIcon : MonoBehaviour
    {
        [SerializeField] private RectTransform icon;
        [SerializeField] private Transform target;

        private void LateUpdate()
        {
            if (icon == null || target == null)
            {
                return;
            }

            icon.rotation = Quaternion.Euler(0f, 0f, -target.eulerAngles.y);
        }
    }
}
