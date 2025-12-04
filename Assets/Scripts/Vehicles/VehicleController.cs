using UnityEngine;
using Sandbox.Player;
using Sandbox.World;

namespace Sandbox.Vehicles
{
    /// <summary>
    /// Arcade-style vehicle controller with entry/exit logic and damage handling.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Health))]
    public class VehicleController : MonoBehaviour
    {
        [SerializeField] private float acceleration = 800f;
        [SerializeField] private float brakeForce = 1200f;
        [SerializeField] private float turnTorque = 4f;
        [SerializeField] private Transform driverSeat;
        [SerializeField] private Transform exitPoint;
        [SerializeField] private float maxDamageImpact = 50f;

        private Rigidbody rb;
        private PlayerController driver;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.centerOfMass = Vector3.down;
        }

        private void FixedUpdate()
        {
            if (driver == null)
            {
                return;
            }

            float forward = Input.GetAxis("Vertical");
            float steer = Input.GetAxis("Horizontal");

            Vector3 force = transform.forward * forward * acceleration * Time.fixedDeltaTime;
            rb.AddForce(force, ForceMode.Acceleration);

            float torque = steer * turnTorque * rb.mass;
            rb.AddRelativeTorque(Vector3.up * torque, ForceMode.Force);

            if (Input.GetKey(KeyCode.Space))
            {
                rb.AddForce(-rb.velocity * brakeForce * Time.fixedDeltaTime, ForceMode.Acceleration);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            float impulse = collision.impulse.magnitude;
            if (impulse > 10f)
            {
                float damage = Mathf.InverseLerp(10f, 80f, impulse) * maxDamageImpact;
                GetComponent<Health>().ApplyDamage(damage);
            }
        }

        public bool TryEnter(PlayerController player)
        {
            if (driver != null)
            {
                return false;
            }

            driver = player;
            player.transform.SetParent(driverSeat);
            player.transform.localPosition = Vector3.zero;
            player.transform.localRotation = Quaternion.identity;
            player.gameObject.SetActive(false);
            return true;
        }

        public void ExitCurrentDriver()
        {
            if (driver == null)
            {
                return;
            }

            driver.transform.SetParent(null);
            if (exitPoint != null)
            {
                driver.transform.position = exitPoint.position;
            }
            driver.gameObject.SetActive(true);
            driver.ForceSetVehicle(null);
            driver = null;
        }
    }
}
