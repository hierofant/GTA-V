using UnityEngine;

namespace Sandbox.Player
{
    /// <summary>
    /// Smooth follow camera with aim zoom and vehicle support.
    /// </summary>
    public class ThirdPersonCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 2f, -4f);
        [SerializeField] private float smoothSpeed = 10f;
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private float minPitch = -20f;
        [SerializeField] private float maxPitch = 70f;
        [SerializeField] private float aimFov = 40f;
        [SerializeField] private float defaultFov = 60f;
        [SerializeField] private float vehicleDistance = -6f;

        private float yaw;
        private float pitch;
        private Camera cam;

        private void Awake()
        {
            cam = GetComponentInChildren<Camera>();
            if (cam != null)
            {
                cam.fieldOfView = defaultFov;
            }
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            float distance = offset.z;
            if (!target.gameObject.activeInHierarchy)
            {
                distance = vehicleDistance;
            }

            Vector3 desiredPosition = target.position + rotation * new Vector3(offset.x, offset.y, distance);
            transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * smoothSpeed);
            transform.rotation = rotation;

            if (cam != null)
            {
                bool isAiming = Input.GetMouseButton(1);
                float targetFov = isAiming ? aimFov : defaultFov;
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFov, Time.deltaTime * 8f);
            }
        }
    }
}
