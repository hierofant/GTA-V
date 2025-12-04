using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Sandbox.AI;
using Sandbox.Missions;
using Sandbox.Player;
using Sandbox.UI;
using Sandbox.Vehicles;
using Sandbox.Weapons;

namespace Sandbox.World
{
    /// <summary>
    /// Builds a lightweight playable sandbox scene out of simple primitives at runtime.
    /// This lets the project boot straight into a testable open-world layout without manual scene setup.
    /// </summary>
    public class SandboxSceneBuilder : MonoBehaviour
    {
        private static bool hasBuilt;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureScene()
        {
            if (hasBuilt)
            {
                return;
            }

            GameObject host = new GameObject("SandboxSceneBuilder");
            host.AddComponent<SandboxSceneBuilder>();
        }

        private void Start()
        {
            if (hasBuilt)
            {
                Destroy(gameObject);
                return;
            }

            hasBuilt = true;
            BuildScene();
        }

        private void BuildScene()
        {
            CleanupDefaultCamera();
            Transform environmentRoot = new GameObject("Environment").transform;
            CreateGround(environmentRoot);
            Transform roadLoop = CreateRoads(environmentRoot);
            CreateBuildings(environmentRoot);

            PlayerController player = CreatePlayer(out Transform cameraRoot, out Camera mainCamera);
            ThirdPersonCamera followCamera = CreateCameraRig(player.transform, cameraRoot, mainCamera);
            SetupMinimap(player.transform);

            VehicleController playerVehicle = CreateVehicle(new Vector3(0f, 0.5f, -20f), "PlayerCar");
            VehicleController aiVehicle = CreateVehicle(new Vector3(25f, 0.5f, 10f), "TrafficCar");
            SetupDriverAI(aiVehicle, roadLoop);

            SetupWaypoints(out List<WaypointNode> sidewalkNodes);
            SpawnPedestrians(sidewalkNodes);

            SetupMissionSystem(player.transform);
            PositionPlayer(player, cameraRoot, followCamera, playerVehicle.transform.position + Vector3.back * 4f);
        }

        private void CleanupDefaultCamera()
        {
            if (Camera.main != null)
            {
                Destroy(Camera.main.gameObject);
            }
        }

        private void CreateGround(Transform parent)
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.SetParent(parent);
            ground.transform.localScale = new Vector3(8f, 1f, 8f);
            ground.GetComponent<Renderer>().material.color = new Color(0.25f, 0.35f, 0.25f);
        }

        private Transform CreateRoads(Transform parent)
        {
            Transform roadRoot = new GameObject("Roads").transform;
            roadRoot.SetParent(parent);

            CreateRoadStrip(roadRoot, new Vector3(0f, 0.01f, 0f), new Vector3(1f, 0.05f, 8f));
            CreateRoadStrip(roadRoot, new Vector3(-20f, 0.01f, 0f), new Vector3(1f, 0.05f, 8f));
            CreateRoadStrip(roadRoot, new Vector3(20f, 0.01f, 0f), new Vector3(1f, 0.05f, 8f));
            CreateRoadStrip(roadRoot, new Vector3(0f, 0.01f, -20f), new Vector3(4f, 0.05f, 1f));
            CreateRoadStrip(roadRoot, new Vector3(0f, 0.01f, 20f), new Vector3(4f, 0.05f, 1f));

            return roadRoot;
        }

        private void CreateRoadStrip(Transform parent, Vector3 position, Vector3 scale)
        {
            GameObject road = GameObject.CreatePrimitive(PrimitiveType.Cube);
            road.name = $"Road_{position}";
            road.transform.SetParent(parent);
            road.transform.position = position;
            road.transform.localScale = new Vector3(scale.x * 10f, scale.y, scale.z * 10f);
            road.GetComponent<Renderer>().material.color = new Color(0.1f, 0.1f, 0.1f);
            road.layer = LayerMask.NameToLayer("Default");
        }

        private void CreateBuildings(Transform parent)
        {
            Transform blockRoot = new GameObject("Buildings").transform;
            blockRoot.SetParent(parent);

            Vector3[] positions =
            {
                new Vector3(-15f, 0.5f, -15f),
                new Vector3(-15f, 0.5f, 15f),
                new Vector3(15f, 0.5f, -15f),
                new Vector3(15f, 0.5f, 15f),
                new Vector3(10f, 0.5f, 0f)
            };

            foreach (Vector3 pos in positions)
            {
                GameObject building = GameObject.CreatePrimitive(PrimitiveType.Cube);
                building.name = $"Building_{pos}";
                building.transform.SetParent(blockRoot);
                building.transform.position = pos;
                building.transform.localScale = new Vector3(8f, Random.Range(4f, 9f), 8f);
                building.GetComponent<Renderer>().material.color = new Color(Random.value, Random.value, Random.value * 0.5f + 0.2f);
                building.isStatic = true;
            }
        }

        private PlayerController CreatePlayer(out Transform cameraRoot, out Camera mainCamera)
        {
            GameObject player = new GameObject("Player");
            player.tag = "Player";
            player.transform.position = new Vector3(0f, 1.2f, -10f);

            CharacterController controller = player.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.35f;
            controller.center = new Vector3(0f, 0.9f, 0f);

            Health health = player.AddComponent<Health>();

            cameraRoot = new GameObject("CameraRoot").transform;
            cameraRoot.SetParent(player.transform);
            cameraRoot.localPosition = new Vector3(0f, 1.5f, 0f);

            Transform fireOrigin = new GameObject("FireOrigin").transform;
            fireOrigin.SetParent(cameraRoot);
            fireOrigin.localPosition = new Vector3(0.2f, -0.1f, 0.3f);

            PlayerWeaponHandler weaponHandler = player.AddComponent<PlayerWeaponHandler>();
            PlayerController playerController = player.AddComponent<PlayerController>();

            WeaponDefinition pistol = CreateWeaponDefinition("pistol", 20f, 0.3f, 60f, 2f, 12);
            WeaponDefinition rifle = CreateWeaponDefinition("rifle", 8f, 0.1f, 90f, 1.5f, 30);

            mainCamera = CreateMainCamera();
            weaponHandler.Initialize(mainCamera, fireOrigin, pistol, rifle, Physics.DefaultRaycastLayers);
            playerController.Initialize(cameraRoot, weaponHandler, Physics.DefaultRaycastLayers);

            return playerController;
        }

        private WeaponDefinition CreateWeaponDefinition(string id, float damage, float rate, float range, float recoil, int magazine)
        {
            WeaponDefinition definition = ScriptableObject.CreateInstance<WeaponDefinition>();
            definition.id = id;
            definition.damage = damage;
            definition.fireRate = rate;
            definition.range = range;
            definition.recoil = recoil;
            definition.magazineSize = magazine;
            definition.reloadTime = 1.4f;
            return definition;
        }

        private Camera CreateMainCamera()
        {
            GameObject camObject = new GameObject("MainCamera");
            camObject.tag = "MainCamera";
            Camera cam = camObject.AddComponent<Camera>();
            camObject.AddComponent<AudioListener>();
            cam.clearFlags = CameraClearFlags.Skybox;
            return cam;
        }

        private ThirdPersonCamera CreateCameraRig(Transform target, Transform cameraRoot, Camera mainCamera)
        {
            GameObject rig = new GameObject("CameraRig");
            rig.transform.position = target.position + new Vector3(0f, 2f, -4f);
            ThirdPersonCamera follow = rig.AddComponent<ThirdPersonCamera>();
            follow.SetTarget(target);

            mainCamera.transform.SetParent(rig.transform);
            mainCamera.transform.localPosition = Vector3.zero;
            mainCamera.transform.localRotation = Quaternion.identity;

            return follow;
        }

        private VehicleController CreateVehicle(Vector3 position, string name)
        {
            GameObject vehicle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            vehicle.name = name;
            vehicle.transform.position = position;
            vehicle.transform.localScale = new Vector3(2f, 1f, 4f);
            vehicle.GetComponent<Renderer>().material.color = new Color(0.8f, 0.1f, 0.1f);

            Rigidbody rb = vehicle.AddComponent<Rigidbody>();
            rb.mass = 1200f;

            BoxCollider collider = vehicle.GetComponent<BoxCollider>();
            collider.material = new PhysicMaterial { bounciness = 0.1f, frictionCombine = PhysicMaterialCombine.Multiply };

            Health health = vehicle.AddComponent<Health>();
            VehicleController controller = vehicle.AddComponent<VehicleController>();

            Transform seat = new GameObject("DriverSeat").transform;
            seat.SetParent(vehicle.transform);
            seat.localPosition = new Vector3(0f, 0.5f, 0f);

            Transform exit = new GameObject("ExitPoint").transform;
            exit.SetParent(vehicle.transform);
            exit.localPosition = new Vector3(-1.5f, 0.5f, 0f);

            controller.Configure(seat, exit);
            return controller;
        }

        private void SetupDriverAI(VehicleController vehicle, Transform pathParent)
        {
            if (vehicle == null || pathParent == null)
            {
                return;
            }

            List<Transform> nodes = new();
            foreach (Transform child in pathParent)
            {
                nodes.Add(child);
            }

            DriverAI ai = vehicle.gameObject.AddComponent<DriverAI>();
            ai.Configure(nodes.ToArray());
        }

        private void SetupWaypoints(out List<WaypointNode> sidewalkNodes)
        {
            sidewalkNodes = new List<WaypointNode>();
            Vector3[] points =
            {
                new Vector3(-18f, 0f, -18f),
                new Vector3(-18f, 0f, 18f),
                new Vector3(18f, 0f, 18f),
                new Vector3(18f, 0f, -18f),
                new Vector3(0f, 0f, 0f)
            };

            for (int i = 0; i < points.Length; i++)
            {
                WaypointNode node = new GameObject($"Waypoint_{i}").AddComponent<WaypointNode>();
                node.transform.position = points[i];
                node.neighbors = new WaypointNode[2];
                sidewalkNodes.Add(node);
            }

            for (int i = 0; i < sidewalkNodes.Count; i++)
            {
                WaypointNode current = sidewalkNodes[i];
                current.neighbors[0] = sidewalkNodes[(i + 1) % sidewalkNodes.Count];
                current.neighbors[1] = sidewalkNodes[(i + sidewalkNodes.Count - 1) % sidewalkNodes.Count];
            }
        }

        private void SpawnPedestrians(List<WaypointNode> nodes)
        {
            if (nodes == null || nodes.Count == 0)
            {
                return;
            }

            for (int i = 0; i < nodes.Count; i++)
            {
                GameObject ped = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                ped.name = $"Pedestrian_{i}";
                ped.transform.position = nodes[i].transform.position + Vector3.up * 1f;
                ped.GetComponent<Renderer>().material.color = new Color(Random.value, Random.value, Random.value);

                NavMeshAgent agent = ped.AddComponent<NavMeshAgent>();
                agent.speed = 2.5f;
                agent.angularSpeed = 720f;
                agent.baseOffset = 0f;

                Health health = ped.AddComponent<Health>();
                PedestrianAI ai = ped.AddComponent<PedestrianAI>();
                ai.SetStartNode(nodes[i]);
            }
        }

        private void SetupMissionSystem(Transform player)
        {
            MissionDefinition starterMission = ScriptableObject.CreateInstance<MissionDefinition>();
            starterMission.missionId = "mission_drive";
            starterMission.title = "Drive to the Square";
            starterMission.description = "Reach the glowing marker at the plaza to complete the tutorial mission.";
            starterMission.markerPosition = new Vector3(0f, 0f, 18f);
            starterMission.rewardCash = 250;

            MissionMarker markerPrefab = CreateMissionMarkerPrefab();

            MissionManager manager = new GameObject("MissionManager").AddComponent<MissionManager>();
            manager.SetConfig(new[] { starterMission }, markerPrefab);

            MissionTrigger trigger = new GameObject("MissionTrigger").AddComponent<MissionTrigger>();
            trigger.transform.position = player.position + Vector3.forward * 2f;
            trigger.Initialize(manager, starterMission.missionId);
        }

        private MissionMarker CreateMissionMarkerPrefab()
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = "MissionMarkerPrefab";
            marker.transform.localScale = Vector3.one * 1.5f;
            marker.GetComponent<Renderer>().material.color = new Color(1f, 0.8f, 0.2f, 0.8f);
            SphereCollider collider = marker.GetComponent<SphereCollider>();
            collider.isTrigger = true;

            MissionMarker missionMarker = marker.AddComponent<MissionMarker>();
            missionMarker.SetVisual(marker);

            marker.SetActive(false);
            return missionMarker;
        }

        private void SetupMinimap(Transform target)
        {
            GameObject minimap = new GameObject("MinimapCamera");
            Camera mapCam = minimap.AddComponent<Camera>();
            mapCam.orthographic = true;
            mapCam.orthographicSize = 40f;
            mapCam.clearFlags = CameraClearFlags.SolidColor;
            mapCam.backgroundColor = new Color(0.1f, 0.1f, 0.12f, 1f);
            mapCam.cullingMask = ~0;

            MinimapController controller = minimap.AddComponent<MinimapController>();
            controller.SetTarget(target, mapCam);
        }

        private void PositionPlayer(PlayerController player, Transform cameraRoot, ThirdPersonCamera followCamera, Vector3 spawnPosition)
        {
            player.transform.position = spawnPosition;
            cameraRoot.position = spawnPosition + new Vector3(0f, 1.5f, 0f);
            followCamera.ResetAim(cameraRoot);
        }
    }
}
