using UnityEngine;
using UnityEngine.AI;
using ProceduralDungeonShooter.Player;

namespace ProceduralDungeonShooter.Enemies
{
    /// <summary>
    /// AI behavior for enemy navigation and combat tactics
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyAI : MonoBehaviour
    {
        [Header("Patrol Settings")]
        public float patrolRadius = 10f;
        public float patrolWaitTime = 2f;
        public int maxPatrolPoints = 5;
        
        [Header("Combat Settings")]
        public float retreatDistance = 3f;
        public float flankingDistance = 5f;
        public bool useAdvancedTactics = true;
        
        [Header("Pathfinding")]
        public float pathUpdateRate = 0.2f;
        public float stuckThreshold = 0.5f;
        public float stuckCheckTime = 2f;
        
        // Private variables
        private NavMeshAgent navAgent;
        private EnemyController enemyController;
        private Transform player;
        private Vector3 originalPosition;
        private Vector3 currentPatrolTarget;
        private Vector3 lastPosition;
        private float lastPathUpdate = 0f;
        private float stuckTimer = 0f;
        private float patrolWaitTimer = 0f;
        private bool isWaitingAtPatrol = false;
        
        // Patrol points
        private Vector3[] patrolPoints;
        private int currentPatrolIndex = 0;
        
        void Awake()
        {
            navAgent = GetComponent<NavMeshAgent>();
            enemyController = GetComponent<EnemyController>();
        }
        
        void Start()
        {
            originalPosition = transform.position;
            lastPosition = transform.position;
            
            // Find player
            PlayerController playerController = FindObjectOfType<PlayerController>();
            if (playerController != null)
            {
                player = playerController.transform;
            }
            
            // Generate patrol points
            GeneratePatrolPoints();
        }
        
        void Update()
        {
            CheckIfStuck();
            
            if (Time.time >= lastPathUpdate + pathUpdateRate)
            {
                lastPathUpdate = Time.time;
                UpdateAI();
            }
        }
        
        void UpdateAI()
        {
            if (enemyController == null || player == null) return;
            
            switch (enemyController.GetState())
            {
                case EnemyController.EnemyState.Chasing:
                    ChasePlayer();
                    break;
                    
                case EnemyController.EnemyState.Attacking:
                    CombatPositioning();
                    break;
            }
        }
        
        public void Patrol()
        {
            if (patrolPoints == null || patrolPoints.Length == 0) return;
            
            if (isWaitingAtPatrol)
            {
                patrolWaitTimer -= Time.deltaTime;
                if (patrolWaitTimer <= 0f)
                {
                    isWaitingAtPatrol = false;
                    MoveToNextPatrolPoint();
                }
                return;
            }
            
            // Check if reached current patrol point
            if (navAgent.hasPath && navAgent.remainingDistance < 1f)
            {
                isWaitingAtPatrol = true;
                patrolWaitTimer = patrolWaitTime;
            }
            else if (!navAgent.hasPath || navAgent.pathStatus == NavMeshPathStatus.PathInvalid)
            {
                MoveToNextPatrolPoint();
            }
        }
        
        void MoveToNextPatrolPoint()
        {
            if (patrolPoints == null || patrolPoints.Length == 0) return;
            
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            Vector3 targetPoint = patrolPoints[currentPatrolIndex];
            
            if (navAgent.isActiveAndEnabled)
            {
                navAgent.SetDestination(targetPoint);
            }
        }
        
        void GeneratePatrolPoints()
        {
            patrolPoints = new Vector3[maxPatrolPoints];
            
            for (int i = 0; i < maxPatrolPoints; i++)
            {
                Vector3 randomPoint = originalPosition + Random.insideUnitSphere * patrolRadius;
                randomPoint.y = originalPosition.y; // Keep same height
                
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomPoint, out hit, patrolRadius, NavMesh.AllAreas))
                {
                    patrolPoints[i] = hit.position;
                }
                else
                {
                    patrolPoints[i] = originalPosition; // Fallback to original position
                }
            }
        }
        
        void ChasePlayer()
        {
            if (player == null || !navAgent.isActiveAndEnabled) return;
            
            Vector3 targetPosition = player.position;
            
            if (useAdvancedTactics)
            {
                // Use advanced chase tactics
                targetPosition = GetAdvancedChasePosition();
            }
            
            navAgent.SetDestination(targetPosition);
        }
        
        Vector3 GetAdvancedChasePosition()
        {
            Vector3 playerPosition = player.position;
            Vector3 enemyPosition = transform.position;
            
            // Predict player movement
            Rigidbody playerRb = player.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                Vector3 playerVelocity = playerRb.velocity;
                float timeToReach = Vector3.Distance(enemyPosition, playerPosition) / navAgent.speed;
                playerPosition += playerVelocity * timeToReach;
            }
            
            // Add some randomness to avoid predictable movement
            Vector3 randomOffset = Random.insideUnitSphere * 2f;
            randomOffset.y = 0;
            playerPosition += randomOffset;
            
            return playerPosition;
        }
        
        void CombatPositioning()
        {
            if (player == null || !navAgent.isActiveAndEnabled) return;
            
            Vector3 playerPosition = player.position;
            Vector3 enemyPosition = transform.position;
            float distanceToPlayer = Vector3.Distance(enemyPosition, playerPosition);
            
            if (useAdvancedTactics)
            {
                // Use advanced combat positioning
                Vector3 tacticalPosition = GetTacticalPosition();
                navAgent.SetDestination(tacticalPosition);
            }
            else
            {
                // Simple approach: stop and attack
                navAgent.isStopped = true;
            }
        }
        
        Vector3 GetTacticalPosition()
        {
            Vector3 playerPosition = player.position;
            Vector3 enemyPosition = transform.position;
            Vector3 directionToPlayer = (playerPosition - enemyPosition).normalized;
            
            // Randomly choose tactic
            float tacticChoice = Random.value;
            
            if (tacticChoice < 0.3f) // Retreat
            {
                Vector3 retreatPosition = enemyPosition - directionToPlayer * retreatDistance;
                return GetValidNavMeshPosition(retreatPosition);
            }
            else if (tacticChoice < 0.6f) // Flank left
            {
                Vector3 perpendicular = Vector3.Cross(directionToPlayer, Vector3.up);
                Vector3 flankPosition = playerPosition + perpendicular * flankingDistance;
                return GetValidNavMeshPosition(flankPosition);
            }
            else if (tacticChoice < 0.9f) // Flank right
            {
                Vector3 perpendicular = Vector3.Cross(directionToPlayer, Vector3.up);
                Vector3 flankPosition = playerPosition - perpendicular * flankingDistance;
                return GetValidNavMeshPosition(flankPosition);
            }
            else // Direct approach
            {
                return playerPosition;
            }
        }
        
        Vector3 GetValidNavMeshPosition(Vector3 targetPosition)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPosition, out hit, 5f, NavMesh.AllAreas))
            {
                return hit.position;
            }
            
            // Fallback to current position if no valid position found
            return transform.position;
        }
        
        void CheckIfStuck()
        {
            float distanceMoved = Vector3.Distance(transform.position, lastPosition);
            
            if (distanceMoved < stuckThreshold && navAgent.hasPath)
            {
                stuckTimer += Time.deltaTime;
                
                if (stuckTimer >= stuckCheckTime)
                {
                    // Try to unstuck
                    UnstuckEnemy();
                    stuckTimer = 0f;
                }
            }
            else
            {
                stuckTimer = 0f;
            }
            
            lastPosition = transform.position;
        }
        
        void UnstuckEnemy()
        {
            // Try jumping to nearby valid position
            Vector3 randomDirection = Random.insideUnitSphere * 3f;
            randomDirection.y = 0;
            Vector3 unstuckPosition = transform.position + randomDirection;
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(unstuckPosition, out hit, 10f, NavMesh.AllAreas))
            {
                transform.position = hit.position;
                Debug.Log("Enemy unstuck");
            }
        }
        
        public void ResetAI()
        {
            currentPatrolIndex = 0;
            isWaitingAtPatrol = false;
            patrolWaitTimer = 0f;
            stuckTimer = 0f;
            
            if (navAgent.isActiveAndEnabled)
            {
                navAgent.ResetPath();
            }
        }
        
        public bool HasValidPath()
        {
            return navAgent.hasPath && navAgent.pathStatus == NavMeshPathStatus.PathComplete;
        }
        
        public float GetDistanceToDestination()
        {
            return navAgent.hasPath ? navAgent.remainingDistance : float.MaxValue;
        }
        
        void OnDrawGizmosSelected()
        {
            // Draw patrol radius
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(originalPosition, patrolRadius);
            
            // Draw patrol points
            if (patrolPoints != null)
            {
                Gizmos.color = Color.cyan;
                for (int i = 0; i < patrolPoints.Length; i++)
                {
                    Gizmos.DrawSphere(patrolPoints[i], 0.3f);
                    
                    // Draw connections
                    if (i < patrolPoints.Length - 1)
                    {
                        Gizmos.DrawLine(patrolPoints[i], patrolPoints[i + 1]);
                    }
                    else
                    {
                        Gizmos.DrawLine(patrolPoints[i], patrolPoints[0]);
                    }
                }
            }
            
            // Draw current path
            if (navAgent != null && navAgent.hasPath)
            {
                Gizmos.color = Color.red;
                Vector3[] pathPoints = navAgent.path.corners;
                for (int i = 0; i < pathPoints.Length - 1; i++)
                {
                    Gizmos.DrawLine(pathPoints[i], pathPoints[i + 1]);
                }
            }
        }
    }
}