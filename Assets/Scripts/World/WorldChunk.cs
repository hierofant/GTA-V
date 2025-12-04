using UnityEngine;

namespace Sandbox.World
{
    /// <summary>
    /// Marks a chunk of the city that can be streamed in/out.
    /// </summary>
    public class WorldChunk : MonoBehaviour
    {
        [SerializeField] private LODGroup lodGroup;

        private void OnEnable()
        {
            if (lodGroup != null)
            {
                lodGroup.ForceLOD(0);
            }
        }

        private void OnDisable()
        {
            if (lodGroup != null)
            {
                lodGroup.ForceLOD(1);
            }
        }
    }
}
