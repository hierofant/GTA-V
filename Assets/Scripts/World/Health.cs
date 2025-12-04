using UnityEngine;

namespace Sandbox.World
{
    /// <summary>
    /// Basic health component shared across players, NPCs and vehicles.
    /// </summary>
    public class Health : MonoBehaviour
    {
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private bool destroyOnDeath = true;

        public float CurrentHealth { get; private set; }
        public bool IsDead => CurrentHealth <= 0f;

        public System.Action<Health> OnDamaged;
        public System.Action<Health> OnDied;

        private void Awake()
        {
            CurrentHealth = maxHealth;
        }

        public void ApplyDamage(float amount)
        {
            if (IsDead)
            {
                return;
            }

            CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
            OnDamaged?.Invoke(this);

            if (IsDead)
            {
                OnDied?.Invoke(this);
                if (destroyOnDeath)
                {
                    Destroy(gameObject);
                }
            }
        }

        public void Heal(float amount)
        {
            if (IsDead)
            {
                return;
            }

            CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        }
    }
}
