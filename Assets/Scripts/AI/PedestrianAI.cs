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
        private Vector3 manualTarget;

        public void SetStartNode(WaypointNode node)
        {
            startNode = node;
            currentNode = node;
        }

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

            if (HasNavMeshAgent())
            {
                if (!agent.pathPending && agent.remainingDistance < 0.5f)
                {
                    MoveToNextNode();
                }
            }
            else if (Vector3.Distance(transform.position, manualTarget) < 0.5f)
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
                if (HasNavMeshAgent())
                {
                    agent.speed = baseSpeed;
                    agent.SetDestination(currentNode.transform.position);
                }
                else
                {
                    manualTarget = currentNode.transform.position;
                }
            }
        }

        private void FleeFromPlayer()
        {
            Vector3 dir = (transform.position - player.position).normalized;
            Vector3 destination = transform.position + dir * fleeDistance;
            if (HasNavMeshAgent())
            {
                agent.speed = baseSpeed * fleeSpeedMultiplier;
                agent.SetDestination(destination);
            }
            else
            {
                manualTarget = destination;
            }
        }

        private bool HasNavMeshAgent()
        {
            return agent != null && agent.enabled && agent.isOnNavMesh;
        }

        private void FixedUpdate()
        {
            if (HasNavMeshAgent())
            {
                return;
            }

            Vector3 direction = (manualTarget - transform.position);
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.01f)
            {
                transform.position += direction.normalized * baseSpeed * Time.fixedDeltaTime;
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(direction), Time.fixedDeltaTime * 4f);
            }
        }
    }
}
