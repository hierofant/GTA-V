using System.Collections.Generic;
using UnityEngine;

namespace Sandbox.World
{
    /// <summary>
    /// Simple object pooling system to reuse effects like tracers and impacts.
    /// </summary>
    public class ObjectPool : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private int initialSize = 10;

        private readonly Queue<GameObject> pool = new();

        private void Awake()
        {
            for (int i = 0; i < initialSize; i++)
            {
                GameObject obj = Instantiate(prefab, transform);
                obj.SetActive(false);
                pool.Enqueue(obj);
            }
        }

        public GameObject Spawn(Vector3 position, Quaternion rotation)
        {
            GameObject obj = pool.Count > 0 ? pool.Dequeue() : Instantiate(prefab, transform);
            obj.transform.SetPositionAndRotation(position, rotation);
            obj.SetActive(true);
            return obj;
        }

        public void Despawn(GameObject obj)
        {
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }
}
