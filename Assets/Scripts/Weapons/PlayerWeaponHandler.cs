using UnityEngine;
using Sandbox.World;

namespace Sandbox.Weapons
{
    /// <summary>
    /// Handles weapon switching, firing logic and recoil for the player.
    /// </summary>
    public class PlayerWeaponHandler : MonoBehaviour
    {
        [SerializeField] private Transform fireOrigin;
        [SerializeField] private WeaponDefinition startingPistol;
        [SerializeField] private WeaponDefinition startingRifle;
        [SerializeField] private float aimSpreadMultiplier = 0.5f;
        [SerializeField] private LayerMask hitMask;
        [SerializeField] private Camera playerCamera;

        private WeaponInstance currentWeapon;
        private WeaponInstance pistolInstance;
        private WeaponInstance rifleInstance;
        private float nextFireTime;
        private int currentIndex;

        public void Initialize(Camera cam, Transform origin, WeaponDefinition pistolDef, WeaponDefinition rifleDef, LayerMask mask)
        {
            playerCamera = cam;
            fireOrigin = origin;
            startingPistol = pistolDef;
            startingRifle = rifleDef;
            hitMask = mask;
            EnsureInstances();
        }

        private void Start()
        {
            EnsureInstances();
            Equip(0);
        }

        private void Update()
        {
            if (playerCamera == null)
            {
                playerCamera = Camera.main;
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Equip(0);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Equip(1);
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                currentWeapon.Reload();
            }

            bool isAiming = Input.GetMouseButton(1);
            if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
            {
                TryFire(isAiming);
            }
        }

        private void Equip(int index)
        {
            currentIndex = index;
            currentWeapon = index == 0 ? pistolInstance : rifleInstance;
        }

        private void EnsureInstances()
        {
            if (startingPistol == null)
            {
                startingPistol = ScriptableObject.CreateInstance<WeaponDefinition>();
            }

            if (startingRifle == null)
            {
                startingRifle = ScriptableObject.CreateInstance<WeaponDefinition>();
                startingRifle.damage = 10f;
                startingRifle.fireRate = 0.12f;
                startingRifle.range = 80f;
                startingRifle.recoil = 1.5f;
                startingRifle.magazineSize = 24;
            }

            if (pistolInstance == null && startingPistol != null)
            {
                pistolInstance = new WeaponInstance(startingPistol);
            }

            if (rifleInstance == null && startingRifle != null)
            {
                rifleInstance = new WeaponInstance(startingRifle);
            }
        }

        private void TryFire(bool isAiming)
        {
            if (currentWeapon == null)
            {
                return;
            }

            if (!currentWeapon.CanFire())
            {
                return;
            }

            nextFireTime = Time.time + currentWeapon.Definition.fireRate;
            currentWeapon.ConsumeAmmo();

            Vector3 direction = playerCamera.transform.forward;
            float spread = isAiming ? aimSpreadMultiplier : 1f;
            direction = Quaternion.Euler(Random.Range(-currentWeapon.Definition.recoil * spread, currentWeapon.Definition.recoil * spread),
                Random.Range(-currentWeapon.Definition.recoil * spread, currentWeapon.Definition.recoil * spread), 0f) * direction;

            if (Physics.Raycast(fireOrigin.position, direction, out RaycastHit hit, currentWeapon.Definition.range, hitMask))
            {
                Health targetHealth = hit.collider.GetComponentInParent<Health>();
                if (targetHealth != null)
                {
                    targetHealth.ApplyDamage(currentWeapon.Definition.damage);
                }

                if (currentWeapon.Definition.tracerPrefab != null)
                {
                    SpawnTracer(hit.point);
                }
            }

            if (playerCamera != null)
            {
                playerCamera.transform.localRotation *= Quaternion.Euler(-currentWeapon.Definition.recoil, 0f, 0f);
            }
        }

        private void SpawnTracer(Vector3 target)
        {
            GameObject tracer = Instantiate(currentWeapon.Definition.tracerPrefab, fireOrigin.position, Quaternion.identity);
            tracer.transform.LookAt(target);
        }
    }

    /// <summary>
    /// Runtime weapon wrapper.
    /// </summary>
    public class WeaponInstance
    {
        public WeaponDefinition Definition { get; }
        public int CurrentAmmo { get; private set; }
        private bool isReloading;

        public WeaponInstance(WeaponDefinition definition)
        {
            Definition = definition;
            CurrentAmmo = definition != null ? definition.magazineSize : 0;
        }

        public bool CanFire()
        {
            return !isReloading && CurrentAmmo > 0;
        }

        public void ConsumeAmmo()
        {
            CurrentAmmo = Mathf.Max(0, CurrentAmmo - 1);
            if (CurrentAmmo <= 0)
            {
                Reload();
            }
        }

        public async void Reload()
        {
            if (isReloading || Definition == null)
            {
                return;
            }

            isReloading = true;
            float endTime = Time.time + Definition.reloadTime;
            while (Time.time < endTime)
            {
                await System.Threading.Tasks.Task.Yield();
            }

            CurrentAmmo = Definition.magazineSize;
            isReloading = false;
        }
    }
}
