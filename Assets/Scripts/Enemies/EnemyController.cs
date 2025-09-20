using UnityEngine;
using UnityEngine.AI;
using ProceduralDungeonShooter.Player;
using ProceduralDungeonShooter.Loot;
using ProceduralDungeonShooter.Core;

namespace ProceduralDungeonShooter.Enemies
{
    /// <summary>
    /// Base enemy controller with health, damage, and basic behavior
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyController : MonoBehaviour
    {
        [Header("Enemy Stats")]
        public float maxHealth = 50f;
        public float currentHealth;
        public float damage = 10f;
        public float attackRange = 2f;
        public float attackCooldown = 1.5f;
        
        [Header("Movement")]
        public float moveSpeed = 3.5f;
        public float rotationSpeed = 5f;
        
        [Header("Detection")]
        public float detectionRange = 10f;
        public float loseTargetRange = 15f;
        public LayerMask playerLayer = 1;
        
        [Header("Visual Effects")]
        public GameObject deathEffect;
        public GameObject hitEffect;
        public Renderer enemyRenderer;
        public Color normalColor = Color.white;
        public Color hitColor = Color.red;
        
        [Header("Audio")]
        public AudioSource audioSource;
        public AudioClip attackSound;
        public AudioClip deathSound;
        public AudioClip hitSound;
        
        // Private variables
        private NavMeshAgent navAgent;
        private Transform player;
        private EnemyAI enemyAI;
        private EnemyStats stats;
        private float lastAttackTime = 0f;
        private bool isDead = false;
        private bool hasTarget = false;
        
        // Events
        public System.Action<EnemyController> OnEnemyDeath;
        
        // Enemy states
        public enum EnemyState
        {
            Idle,
            Patrolling,
            Chasing,
            Attacking,
            Dead
        }
        
        public EnemyState currentState = EnemyState.Idle;
        
        void Awake()
        {
            navAgent = GetComponent<NavMeshAgent>();
            enemyAI = GetComponent<EnemyAI>();
            stats = GetComponent<EnemyStats>();
            
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }
            
            if (enemyRenderer == null)
            {
                enemyRenderer = GetComponentInChildren<Renderer>();
            }
        }
        
        void Start()
        {
            InitializeEnemy();
            FindPlayer();
        }
        
        void Update()
        {
            if (!isDead)
            {
                UpdateEnemyBehavior();
            }
        }
        
        void InitializeEnemy()
        {
            // Apply randomized stats if available
            if (stats != null)
            {
                stats.ApplyRandomStats();
                maxHealth = stats.health;
                damage = stats.damage;
                moveSpeed = stats.speed;
                detectionRange = stats.detectionRange;
            }
            
            // Set initial values
            currentHealth = maxHealth;
            isDead = false;
            hasTarget = false;
            currentState = EnemyState.Idle;
            
            // Configure NavMeshAgent
            if (navAgent != null)
            {
                navAgent.speed = moveSpeed;
                navAgent.stoppingDistance = attackRange;
            }
            
            // Set normal color
            if (enemyRenderer != null)
            {
                enemyRenderer.material.color = normalColor;
            }
            
            Debug.Log($"Enemy initialized - Health: {maxHealth}, Damage: {damage}, Speed: {moveSpeed}");
        }
        
        void FindPlayer()
        {
            // Find player in scene
            PlayerController playerController = FindObjectOfType<PlayerController>();
            if (playerController != null)
            {
                player = playerController.transform;
            }
        }
        
        void UpdateEnemyBehavior()
        {
            if (player == null) return;
            
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            
            switch (currentState)
            {
                case EnemyState.Idle:
                case EnemyState.Patrolling:
                    if (CanSeePlayer() && distanceToPlayer <= detectionRange)
                    {
                        SetState(EnemyState.Chasing);
                        hasTarget = true;
                    }
                    else if (enemyAI != null)
                    {
                        enemyAI.Patrol();
                        SetState(EnemyState.Patrolling);
                    }
                    break;
                    
                case EnemyState.Chasing:
                    if (distanceToPlayer <= attackRange)
                    {
                        SetState(EnemyState.Attacking);
                    }
                    else if (distanceToPlayer > loseTargetRange || !CanSeePlayer())
                    {
                        SetState(EnemyState.Idle);
                        hasTarget = false;
                    }
                    else
                    {
                        ChasePlayer();
                    }
                    break;
                    
                case EnemyState.Attacking:
                    if (distanceToPlayer > attackRange)
                    {
                        SetState(EnemyState.Chasing);
                    }
                    else
                    {
                        AttackPlayer();
                    }
                    break;
            }
        }
        
        void SetState(EnemyState newState)
        {
            if (currentState != newState)
            {
                currentState = newState;
                OnStateChanged(newState);
            }
        }
        
        void OnStateChanged(EnemyState newState)
        {
            switch (newState)
            {
                case EnemyState.Chasing:
                    if (navAgent != null)
                    {
                        navAgent.isStopped = false;
                    }
                    break;
                    
                case EnemyState.Attacking:
                    if (navAgent != null)
                    {
                        navAgent.isStopped = true;
                    }
                    break;
            }
        }
        
        bool CanSeePlayer()
        {
            if (player == null) return false;
            
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            RaycastHit hit;
            
            if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer, out hit, detectionRange))
            {
                return hit.collider.CompareTag("Player");
            }
            
            return false;
        }
        
        void ChasePlayer()
        {
            if (navAgent != null && player != null)
            {
                navAgent.SetDestination(player.position);
                navAgent.isStopped = false;
            }
        }
        
        void AttackPlayer()
        {
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                lastAttackTime = Time.time;
                
                // Look at player
                Vector3 lookDirection = (player.position - transform.position).normalized;
                lookDirection.y = 0;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), rotationSpeed * Time.deltaTime);
                
                // Deal damage to player
                PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                    Debug.Log($"Enemy attacked player for {damage} damage");
                }
                
                // Play attack sound
                PlaySound(attackSound);
            }
        }
        
        public void TakeDamage(float damageAmount)
        {
            if (isDead) return;
            
            currentHealth -= damageAmount;
            currentHealth = Mathf.Max(0f, currentHealth);
            
            // Visual feedback
            StartCoroutine(HitEffect());
            
            // Play hit sound
            PlaySound(hitSound);
            
            // Force target player when hit
            if (!hasTarget && player != null)
            {
                hasTarget = true;
                SetState(EnemyState.Chasing);
            }
            
            Debug.Log($"Enemy took {damageAmount} damage. Health: {currentHealth}/{maxHealth}");
            
            // Check for death
            if (currentHealth <= 0f)
            {
                Die();
            }
        }
        
        void Die()
        {
            if (isDead) return;
            
            isDead = true;
            SetState(EnemyState.Dead);
            
            // Stop movement
            if (navAgent != null)
            {
                navAgent.isStopped = true;
            }
            
            // Play death sound
            PlaySound(deathSound);
            
            // Spawn death effect
            if (deathEffect != null)
            {
                Instantiate(deathEffect, transform.position, transform.rotation);
            }
            
            // Notify events
            OnEnemyDeath?.Invoke(this);
            
            // Spawn loot
            LootManager lootManager = FindObjectOfType<LootManager>();
            if (lootManager != null)
            {
                lootManager.SpawnLoot(transform.position);
            }
            
            Debug.Log("Enemy died");
            
            // Destroy after short delay
            Destroy(gameObject, 1f);
        }
        
        System.Collections.IEnumerator HitEffect()
        {
            if (enemyRenderer != null)
            {
                enemyRenderer.material.color = hitColor;
                yield return new WaitForSeconds(0.1f);
                enemyRenderer.material.color = normalColor;
            }
            
            // Spawn hit effect
            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position + Vector3.up, transform.rotation);
            }
        }
        
        void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
        
        public float GetDamage()
        {
            return damage;
        }
        
        public bool IsDead()
        {
            return isDead;
        }
        
        public bool HasTarget()
        {
            return hasTarget;
        }
        
        public EnemyState GetState()
        {
            return currentState;
        }
        
        public float GetHealthPercentage()
        {
            return maxHealth > 0 ? currentHealth / maxHealth : 0f;
        }
        
        void OnDrawGizmosSelected()
        {
            // Draw detection range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            
            // Draw attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
            
            // Draw lose target range
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, loseTargetRange);
        }
    }
}