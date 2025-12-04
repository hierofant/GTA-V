using UnityEngine;

namespace Sandbox.Missions
{
    /// <summary>
    /// Attach to a trigger collider to start a mission when the player enters.
    /// </summary>
    public class MissionTrigger : MonoBehaviour
    {
        [SerializeField] private string missionId;
        [SerializeField] private MissionManager missionManager;

        public void Initialize(MissionManager manager, string id)
        {
            missionManager = manager;
            missionId = id;

            SphereCollider collider = gameObject.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = 1.5f;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                missionManager.ActivateMission(missionId);
            }
        }
    }
}
