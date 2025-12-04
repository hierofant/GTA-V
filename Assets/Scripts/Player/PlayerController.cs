using UnityEngine;
using Sandbox.Weapons;
using Sandbox.Vehicles;
using Sandbox.World;

namespace Sandbox.Player
{
    /// <summary>
    /// Handles third-person character movement, locomotion states and vehicle interaction.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Health))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float walkSpeed = 4f;
        [SerializeField] private float runSpeed = 7f;
        [SerializeField] private float crouchSpeed = 2f;
        [SerializeField] private float jumpHeight = 1.5f;
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private Transform cameraRoot;

        [Header("Crouch")]
        [SerializeField] private float crouchHeight = 1.2f;
        [SerializeField] private float standingHeight = 1.8f;

        [Header("Interaction")]
        [SerializeField] private float interactRange = 2.5f;
        [SerializeField] private LayerMask vehicleLayer;

        [Header("References")]
        [SerializeField] private PlayerWeaponHandler weaponHandler;

        private CharacterController controller;
        private Vector3 velocity;
        private bool isCrouching;
        private bool isGrounded;
        private VehicleController currentVehicle;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (currentVehicle != null)
            {
                if (Input.GetKeyDown(KeyCode.F))
                {
                    ExitVehicle();
                }
                return;
            }

            HandleMovement();
            HandleJump();
            HandleCrouch();
            HandleInteraction();
        }

        private void HandleMovement()
        {
            isGrounded = controller.isGrounded;
            if (isGrounded && velocity.y < 0f)
            {
                velocity.y = -2f;
            }

            float targetSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
            if (isCrouching)
            {
                targetSpeed = crouchSpeed;
            }

            Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
            input = Vector3.ClampMagnitude(input, 1f);

            Vector3 forward = cameraRoot.forward;
            forward.y = 0f;
            forward.Normalize();
            Vector3 right = cameraRoot.right;
            right.y = 0f;
            right.Normalize();

            Vector3 move = forward * input.z + right * input.x;
            controller.Move(move * targetSpeed * Time.deltaTime);

            if (move.magnitude > 0.1f)
            {
                Quaternion lookRot = Quaternion.LookRotation(move);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 10f);
            }

            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }

        private void HandleJump()
        {
            if (isGrounded && Input.GetButtonDown("Jump"))
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }

        private void HandleCrouch()
        {
            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                isCrouching = !isCrouching;
                controller.height = isCrouching ? crouchHeight : standingHeight;
            }
        }

        private void HandleInteraction()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                Ray ray = new Ray(cameraRoot.position, cameraRoot.forward);
                if (Physics.Raycast(ray, out RaycastHit hit, interactRange, vehicleLayer))
                {
                    VehicleController vehicle = hit.collider.GetComponentInParent<VehicleController>();
                    if (vehicle != null)
                    {
                        EnterVehicle(vehicle);
                    }
                }
            }
        }

        private void EnterVehicle(VehicleController vehicle)
        {
            if (!vehicle.TryEnter(this))
            {
                return;
            }

            currentVehicle = vehicle;
            controller.enabled = false;
            weaponHandler.enabled = false;
            gameObject.SetActive(false);
        }

        private void ExitVehicle()
        {
            if (currentVehicle == null)
            {
                return;
            }

            currentVehicle.ExitCurrentDriver();
            currentVehicle = null;
            gameObject.SetActive(true);
            transform.position += transform.right * 2f;
            controller.enabled = true;
            weaponHandler.enabled = true;
        }

        public void ForceSetVehicle(VehicleController vehicle)
        {
            currentVehicle = vehicle;
        }
    }
}
