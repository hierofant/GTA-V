using UnityEngine;

namespace Sandbox.Missions
{
    /// <summary>
    /// Scriptable mission description.
    /// </summary>
    [CreateAssetMenu(menuName = "Sandbox/Mission")]
    public class MissionDefinition : ScriptableObject
    {
        public string missionId = "mission_001";
        public string title = "Meet the Contact";
        [TextArea] public string description = "Reach the marker and speak to the contact.";
        public Vector3 markerPosition;
        public int rewardCash = 500;
    }
}
