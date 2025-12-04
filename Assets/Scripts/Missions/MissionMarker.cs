using UnityEngine;

namespace Sandbox.Missions
{
    /// <summary>
    /// Spawns a simple world-space marker and listens for completion trigger.
    /// </summary>
    public class MissionMarker : MonoBehaviour
    {
        [SerializeField] private float markerHeight = 2f;
        [SerializeField] private GameObject markerVisual;

        private MissionManager manager;
        private MissionDefinition mission;

        public void Initialize(MissionManager mgr, MissionDefinition def)
        {
            manager = mgr;
            mission = def;
            transform.position = mission.markerPosition + Vector3.up * markerHeight;
            name = $"MissionMarker_{mission.missionId}";
        }

        private void OnTriggerEnter(Collider other)
        {
            if (mission == null || manager == null)
            {
                return;
            }

            if (other.CompareTag("Player"))
            {
                manager.CompleteMission(mission);
            }
        }
    }
}
