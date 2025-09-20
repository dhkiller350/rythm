using UnityEngine;
using ProceduralDungeonShooter.Core;
using ProceduralDungeonShooter.Enemies;

namespace ProceduralDungeonShooter.Player
{
    /// <summary>
    /// Handles player shooting mechanics and weapon management
    /// </summary>
    public class PlayerShooting : MonoBehaviour
    {
        [Header("Weapon Settings")]
        public float fireRate = 10f;
        public float damage = 25f;
        public float range = 100f;
        public float weaponAccuracy = 0.95f;
        
        [Header("Ammo Settings")]
        public int maxAmmo = 30;
        public int currentAmmo;
        public float reloadTime = 2f;
        public bool infiniteAmmo = false;
        
        [Header("Visual Effects")]
        public LineRenderer laserLine;
        public float laserDisplayTime = 0.1f;
        public ParticleSystem muzzleFlash;
        public ParticleSystem impactEffect;
        
        [Header("Audio")]
        public AudioSource audioSource;
        public AudioClip shootSound;
        public AudioClip reloadSound;
        public AudioClip emptySound;
        
        [Header("Camera")]
        public Camera playerCamera;
        
        // Private variables
        private float nextTimeToFire = 0f;
        private bool isReloading = false;
        private bool canShoot = true;
        
        // Events
        public System.Action<int, int> OnAmmoChanged;
        public System.Action OnReloadStart;
        public System.Action OnReloadComplete;
        
        void Awake()
        {
            // Get camera if not assigned
            if (playerCamera == null)
            {
                playerCamera = GetComponentInChildren<Camera>();
            }
            
            // Get audio source if not assigned
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }
            
            // Set up laser line if available
            if (laserLine != null)
            {
                laserLine.enabled = false;
            }
        }
        
        void Start()
        {
            currentAmmo = maxAmmo;
            OnAmmoChanged?.Invoke(currentAmmo, maxAmmo);
        }
        
        void Update()
        {
            if (canShoot && !GameManager.Instance.gameOver && !isReloading)
            {
                HandleShootingInput();
            }
            
            HandleReloadInput();
        }
        
        void HandleShootingInput()
        {
            // Check for shooting input
            if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire)
            {
                if (currentAmmo > 0 || infiniteAmmo)
                {
                    Shoot();
                    nextTimeToFire = Time.time + 1f / fireRate;
                }
                else
                {
                    // Play empty sound
                    PlaySound(emptySound);
                }
            }
        }
        
        void HandleReloadInput()
        {
            // Manual reload input
            if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmo < maxAmmo)
            {
                StartReload();
            }
        }
        
        void Shoot()
        {
            // Consume ammo
            if (!infiniteAmmo)
            {
                currentAmmo--;
                OnAmmoChanged?.Invoke(currentAmmo, maxAmmo);
            }
            
            // Play sound
            PlaySound(shootSound);
            
            // Show muzzle flash
            if (muzzleFlash != null)
            {
                muzzleFlash.Play();
            }
            
            // Perform raycast
            Vector3 rayOrigin = playerCamera.transform.position;
            Vector3 rayDirection = GetShootDirection();
            
            RaycastHit hit;
            if (Physics.Raycast(rayOrigin, rayDirection, out hit, range))
            {
                // Hit something
                ProcessHit(hit);
                
                // Show laser to hit point
                if (laserLine != null)
                {
                    StartCoroutine(ShowLaser(rayOrigin, hit.point));
                }
            }
            else
            {
                // No hit, show laser to max range
                if (laserLine != null)
                {
                    Vector3 endPoint = rayOrigin + rayDirection * range;
                    StartCoroutine(ShowLaser(rayOrigin, endPoint));
                }
            }
            
            // Auto-reload if empty
            if (currentAmmo <= 0 && !infiniteAmmo)
            {
                StartReload();
            }
        }
        
        Vector3 GetShootDirection()
        {
            Vector3 direction = playerCamera.transform.forward;
            
            // Add some inaccuracy based on weapon accuracy
            if (weaponAccuracy < 1f)
            {
                float inaccuracy = 1f - weaponAccuracy;
                direction += new Vector3(
                    Random.Range(-inaccuracy, inaccuracy),
                    Random.Range(-inaccuracy, inaccuracy),
                    Random.Range(-inaccuracy, inaccuracy)
                );
                direction.Normalize();
            }
            
            return direction;
        }
        
        void ProcessHit(RaycastHit hit)
        {
            // Show impact effect
            if (impactEffect != null)
            {
                Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            }
            
            // Check if we hit an enemy
            EnemyController enemy = hit.collider.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log($"Hit enemy for {damage} damage");
            }
            
            // Check for destructible objects
            Destructible destructible = hit.collider.GetComponent<Destructible>();
            if (destructible != null)
            {
                destructible.TakeDamage(damage);
            }
        }
        
        void StartReload()
        {
            if (isReloading || currentAmmo >= maxAmmo)
                return;
            
            isReloading = true;
            OnReloadStart?.Invoke();
            
            // Play reload sound
            PlaySound(reloadSound);
            
            Debug.Log("Reloading...");
            
            // Start reload coroutine
            StartCoroutine(ReloadCoroutine());
        }
        
        System.Collections.IEnumerator ReloadCoroutine()
        {
            yield return new WaitForSeconds(reloadTime);
            
            // Complete reload
            currentAmmo = maxAmmo;
            isReloading = false;
            
            OnAmmoChanged?.Invoke(currentAmmo, maxAmmo);
            OnReloadComplete?.Invoke();
            
            Debug.Log("Reload complete");
        }
        
        System.Collections.IEnumerator ShowLaser(Vector3 start, Vector3 end)
        {
            if (laserLine != null)
            {
                laserLine.enabled = true;
                laserLine.SetPosition(0, start);
                laserLine.SetPosition(1, end);
                
                yield return new WaitForSeconds(laserDisplayTime);
                
                laserLine.enabled = false;
            }
        }
        
        void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
        
        public void SetCanShoot(bool value)
        {
            canShoot = value;
        }
        
        public bool IsReloading()
        {
            return isReloading;
        }
        
        public float GetAmmoPercentage()
        {
            return maxAmmo > 0 ? (float)currentAmmo / maxAmmo : 0f;
        }
        
        public void AddAmmo(int amount)
        {
            currentAmmo = Mathf.Min(maxAmmo, currentAmmo + amount);
            OnAmmoChanged?.Invoke(currentAmmo, maxAmmo);
        }
        
        public void UpgradeWeapon(float newDamage, float newFireRate, float newAccuracy)
        {
            damage = newDamage;
            fireRate = newFireRate;
            weaponAccuracy = Mathf.Clamp01(newAccuracy);
            
            Debug.Log($"Weapon upgraded! Damage: {damage}, Fire Rate: {fireRate}, Accuracy: {weaponAccuracy}");
        }
    }
    
    /// <summary>
    /// Simple destructible object interface
    /// </summary>
    public class Destructible : MonoBehaviour
    {
        [Header("Destructible Settings")]
        public float health = 50f;
        public GameObject destroyEffect;
        
        public void TakeDamage(float damage)
        {
            health -= damage;
            if (health <= 0f)
            {
                if (destroyEffect != null)
                {
                    Instantiate(destroyEffect, transform.position, transform.rotation);
                }
                Destroy(gameObject);
            }
        }
    }
}