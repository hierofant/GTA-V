using UnityEngine;

namespace Sandbox.Weapons
{
    /// <summary>
    /// Scriptable object describing a firearm.
    /// </summary>
    [CreateAssetMenu(menuName = "Sandbox/Weapon Definition")]
    public class WeaponDefinition : ScriptableObject
    {
        public string id = "pistol";
        public float damage = 20f;
        public float fireRate = 0.2f;
        public float range = 60f;
        public float recoil = 2f;
        public int magazineSize = 12;
        public float reloadTime = 1.2f;
        public AudioClip fireSfx;
        public GameObject tracerPrefab;
    }
}
