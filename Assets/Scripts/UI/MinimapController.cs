using UnityEngine;

namespace Sandbox.UI
{
    /// <summary>
    /// Controls the minimap camera and icon alignment.
    /// </summary>
    public class MinimapController : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float height = 30f;
        [SerializeField] private Camera minimapCamera;

        private void LateUpdate()
        {
            if (target == null || minimapCamera == null)
            {
                return;
            }

            Vector3 position = target.position;
            position.y += height;
            minimapCamera.transform.position = position;
            minimapCamera.transform.rotation = Quaternion.Euler(90f, target.eulerAngles.y, 0f);
        }
    }
}
