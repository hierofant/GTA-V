using UnityEngine;

namespace Sandbox.AI
{
    /// <summary>
    /// Connects pedestrian routes together; place these along sidewalks.
    /// </summary>
    public class WaypointNode : MonoBehaviour
    {
        public WaypointNode[] neighbors;

        public WaypointNode GetRandomNeighbor()
        {
            if (neighbors == null || neighbors.Length == 0)
            {
                return null;
            }

            int index = Random.Range(0, neighbors.Length);
            return neighbors[index];
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(transform.position, 0.2f);
            if (neighbors == null)
            {
                return;
            }

            Gizmos.color = Color.cyan;
            foreach (WaypointNode neighbor in neighbors)
            {
                if (neighbor != null)
                {
                    Gizmos.DrawLine(transform.position, neighbor.transform.position);
                }
            }
        }
#endif
    }
}
