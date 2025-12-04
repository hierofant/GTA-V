using UnityEngine;
using UnityEngine.AI;
using Sandbox.World;

namespace Sandbox.AI
{
    /// <summary>
    /// Simple pedestrian that patrols waypoint graph and flees when threatened.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Health))]
    public class PedestrianAI : MonoBehaviour
    {
        [SerializeField] private WaypointNode startNode;
        [SerializeField] private float fleeDistance = 10f;
        [SerializeField] private float fleeSpeedMultiplier = 1.5f;

        private NavMeshAgent agent;
        private Transform player;
        private WaypointNode currentNode;
        private float baseSpeed;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            baseSpeed = agent.speed;
        }

        private void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            currentNode = startNode;
            MoveToNextNode();
        }

        private void Update()
        {
            if (player != null)
            {
                float distance = Vector3.Distance(transform.position, player.position);
                if (distance < fleeDistance)
                {
                    FleeFromPlayer();
                    return;
                }
            }

            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                MoveToNextNode();
            }
        }

        private void MoveToNextNode()
        {
            if (currentNode == null)
            {
                return;
            }

            WaypointNode next = currentNode.GetRandomNeighbor();
            if (next != null)
            {
                currentNode = next;
                agent.speed = baseSpeed;
                agent.SetDestination(currentNode.transform.position);
            }
        }

        private void FleeFromPlayer()
        {
            Vector3 dir = (transform.position - player.position).normalized;
            Vector3 destination = transform.position + dir * fleeDistance;
            agent.speed = baseSpeed * fleeSpeedMultiplier;
            agent.SetDestination(destination);
        }
    }
}
