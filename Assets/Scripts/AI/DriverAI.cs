using UnityEngine;
using Sandbox.Vehicles;

namespace Sandbox.AI
{
    /// <summary>
    /// Simple driver AI that follows a chain of road waypoints.
    /// </summary>
    [RequireComponent(typeof(VehicleController))]
    public class DriverAI : MonoBehaviour
    {
        [SerializeField] private Transform[] path;
        [SerializeField] private float waypointRadius = 3f;

        private VehicleController vehicle;
        private int currentIndex;

        public void Configure(Transform[] route)
        {
            path = route;
        }

        private void Awake()
        {
            vehicle = GetComponent<VehicleController>();
        }

        private void Update()
        {
            if (path == null || path.Length == 0)
            {
                return;
            }

            Transform target = path[currentIndex];
            Vector3 local = transform.InverseTransformPoint(target.position);
            float steer = Mathf.Clamp(local.x / waypointRadius, -1f, 1f);
            float forward = Mathf.Clamp(local.z, 0f, 1f);

            SimulateInput(forward, steer);

            if (Vector3.Distance(transform.position, target.position) < waypointRadius)
            {
                currentIndex = (currentIndex + 1) % path.Length;
            }
        }

        private void SimulateInput(float forward, float steer)
        {
            // Fake input by directly modifying physics force application.
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                return;
            }

            rb.AddForce(transform.forward * forward * Time.deltaTime * 600f, ForceMode.Acceleration);
            rb.AddRelativeTorque(Vector3.up * steer * rb.mass * Time.deltaTime * 20f, ForceMode.Force);
        }
    }
}
