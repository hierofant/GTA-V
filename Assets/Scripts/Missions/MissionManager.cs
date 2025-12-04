using System.Collections.Generic;
using UnityEngine;

namespace Sandbox.Missions
{
    /// <summary>
    /// Tracks mission activation, rewards and persistence.
    /// </summary>
    public class MissionManager : MonoBehaviour
    {
        [SerializeField] private MissionDefinition[] missions;
        [SerializeField] private MissionMarker markerPrefab;

        private readonly HashSet<string> completed = new();
        private MissionDefinition activeMission;
        private MissionMarker activeMarker;
        private int cash;

        private void Start()
        {
            LoadState();
        }

        public void ActivateMission(string missionId)
        {
            MissionDefinition mission = System.Array.Find(missions, m => m.missionId == missionId);
            if (mission == null || completed.Contains(mission.missionId))
            {
                return;
            }

            activeMission = mission;
            SpawnMarker(mission);
        }

        public void CompleteMission(MissionDefinition mission)
        {
            if (mission == null)
            {
                return;
            }

            completed.Add(mission.missionId);
            cash += mission.rewardCash;
            SaveState();
            ClearMarker();
            activeMission = null;
        }

        private void SpawnMarker(MissionDefinition mission)
        {
            ClearMarker();
            if (markerPrefab != null)
            {
                activeMarker = Instantiate(markerPrefab);
                activeMarker.Initialize(this, mission);
            }
        }

        private void ClearMarker()
        {
            if (activeMarker != null)
            {
                Destroy(activeMarker.gameObject);
                activeMarker = null;
            }
        }

        private void SaveState()
        {
            PlayerPrefs.SetInt("cash", cash);
            PlayerPrefs.SetString("missions", string.Join(",", completed));
        }

        private void LoadState()
        {
            cash = PlayerPrefs.GetInt("cash", 0);
            string data = PlayerPrefs.GetString("missions", string.Empty);
            if (!string.IsNullOrEmpty(data))
            {
                string[] ids = data.Split(',');
                foreach (string id in ids)
                {
                    if (!string.IsNullOrEmpty(id))
                    {
                        completed.Add(id);
                    }
                }
            }
        }

        public bool IsCompleted(string missionId) => completed.Contains(missionId);
    }
}
