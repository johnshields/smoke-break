using System.Collections;
using System.IO;
using _Scripts.Managers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Scripts.Player
{
    public class PistolProfiler : MonoBehaviour
    {
        #region Variables

        private static readonly int ShootHash = Animator.StringToHash("Shoot");

        [Header("Pistol Settings")] [SerializeField]
        private GameObject bulletPrefab;

        [SerializeField] private Transform firePoint;
        [SerializeField] private float bulletSpeed = 20f;
        [SerializeField] private float fireRate = 0.3f;

        [Header("Ammo Settings")] [SerializeField]
        private int maxClipSize = 10;

        [SerializeField] public int maxStoredAmmo = 99;
        [SerializeField] public int currentClip;
        [SerializeField] public int storedAmmo;
        [SerializeField] private bool unlimitedAmmo;

        [Header("Effects")] [SerializeField] private GameObject muzzleFlashPrefab;
        [SerializeField] private GameObject worldCrosshairPrefab;
        private Renderer _crosshairRenderer;
        [SerializeField] private Color defaultCrosshairColor = Color.white;

        [Header("Weapon Visibility")] [SerializeField]
        private GameObject pistol;

        private const float WeaponHideTime = 20f;
        private float _lastActionTime;

        [Header("Aiming Settings")] [SerializeField]
        private Camera playerCamera;

        [SerializeField] private float aimFOV = 40f;
        [SerializeField] private float normalFOV = 60f;
        [SerializeField] private float aimSpeed = 10f;
        [SerializeField] private float aimSnapSpeed = 10f;
        [SerializeField] private float crosshairHeightOffset = 0.2f;
        [SerializeField] private LayerMask aimableLayers;

        [Header("Audio Settings")] [SerializeField]
        private AudioSource audioSource;

        [SerializeField] private AudioClip gunshotSound;
        [SerializeField] private AudioClip reloadSound;
        [SerializeField] private AudioClip emptyGunSound;
        [SerializeField] private RandomAudio randomAudio;
        [SerializeField] private float gunshotVolume = 1.0f;


        private GameObject _worldCrosshair;
        private InputControls _actions;
        private InputAction _moveKeys;
        private PlayerProfiler _player;
        private Animator _animator;
        private bool _canShoot = true;
        private bool _isReloading;
        private bool _isAiming;
        private Vector3 _aimTarget;
        private bool _reloadTriggered;
        private string _playerId;
        private string _savePath;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            _playerId = SaveManager.GetOrCreatePlayerId();
            _savePath = Path.Combine(SaveManager.GetSaveDirectory(), $"savegame_{_playerId}.json");

            _actions = new InputControls();
            _player = GetComponent<PlayerProfiler>();
            _animator = GetComponent<Animator>();
            pistol.SetActive(false);

            _worldCrosshair = Instantiate(worldCrosshairPrefab);
            _worldCrosshair.SetActive(false);

            _crosshairRenderer = _worldCrosshair.GetComponent<Renderer>();
            _crosshairRenderer.material.color = defaultCrosshairColor;

            _worldCrosshair.transform.localScale *= 1.5f;
            _moveKeys = _actions.Profiler.Movement;

            LoadAmmo();
        }

        private void OnEnable()
        {
            _actions.Profiler.Aim.performed += StartAiming;
            _actions.Profiler.Aim.canceled += StopAiming;
            _actions.Profiler.Shoot.performed += ShootAction;
            _actions.Profiler.Reload.performed += ReloadAction;
            _actions.Profiler.Enable();
        }

        private void OnDisable()
        {
            _actions.Profiler.Aim.performed -= StartAiming;
            _actions.Profiler.Aim.canceled -= StopAiming;
            _actions.Profiler.Shoot.performed -= ShootAction;
            _actions.Profiler.Reload.performed -= ReloadAction;
            _actions.Profiler.Disable();
        }

        private void Update()
        {
            if (pistol.activeSelf && Time.time - _lastActionTime > WeaponHideTime)
            {
                pistol.SetActive(false);
            }

            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView,
                _isAiming ? aimFOV : normalFOV, Time.deltaTime * aimSpeed);

            if (_isAiming)
            {
                UpdateCrosshairPosition();
                SnapToTarget();
            }

            if (currentClip != 0 || storedAmmo <= 0 || _isReloading) return;
            if (_reloadTriggered) return;
            _reloadTriggered = true;
            StartCoroutine(ReloadDelay(1f));
        }

        #endregion

        #region Ammo

        private void LoadAmmo()
        {
            if (File.Exists(_savePath))
            {
                var json = File.ReadAllText(_savePath);
                var data = JsonUtility.FromJson<SaveData>(json);
                currentClip = data.clipAmmo;
                storedAmmo = data.storedAmmo;
            }
            else
            {
                currentClip = 9;
                storedAmmo = 27;
            }
        }

        public int GetCurrentClip() => currentClip;

        public int GetStoredAmmo() => storedAmmo;

        public void SetAmmo(int clip, int stored)
        {
            currentClip = clip;
            storedAmmo = stored;
            SaveAmmo();
        }

        private void SaveAmmo()
        {
            if (File.Exists(_savePath))
            {
                var json = File.ReadAllText(_savePath);
                var data = JsonUtility.FromJson<SaveData>(json);
                data.clipAmmo = currentClip;
                data.storedAmmo = storedAmmo;
                File.WriteAllText(_savePath, JsonUtility.ToJson(data, true));
            }
        }

        public void RefillAmmo(int amount)
        {
            storedAmmo = Mathf.Min(storedAmmo + amount, maxStoredAmmo);
            Debug.Log($"Ammo refilled! Stored Ammo: {storedAmmo}");
        }

        #endregion

        #region Aiming

        private void StartAiming(InputAction.CallbackContext context)
        {
            _isAiming = true;
            _worldCrosshair.SetActive(true);
        }

        private void StopAiming(InputAction.CallbackContext context)
        {
            _isAiming = false;
            _worldCrosshair.SetActive(false);
        }

        private void UpdateCrosshairPosition()
        {
            var enemiesInRange = Physics.OverlapSphere(transform.position, 35f, aimableLayers);

            if (enemiesInRange.Length > 0)
            {
                var closestEnemy = FindClosestEnemy(enemiesInRange);

                if (closestEnemy is not null)
                {
                    var enemyCenter = closestEnemy.position + Vector3.up * crosshairHeightOffset;
                    var directionToPlayer = (playerCamera.transform.position - enemyCenter).normalized;

                    _aimTarget = enemyCenter + directionToPlayer * 1.5f;

                    if (_worldCrosshair.transform.position != _aimTarget)
                    {
                        _worldCrosshair.transform.position = _aimTarget;
                    }

                    if (_worldCrosshair.transform.rotation != Quaternion.LookRotation(-directionToPlayer))
                    {
                        _worldCrosshair.transform.rotation = Quaternion.LookRotation(-directionToPlayer);
                    }

                    if (!_worldCrosshair.activeSelf)
                        _worldCrosshair.SetActive(true);
                }
            }
            else
            {
                _aimTarget = Vector3.zero;
                _worldCrosshair.SetActive(false);
            }
        }

        private Transform FindClosestEnemy(Collider[] enemies)
        {
            if (enemies.Length == 0) return null;

            Transform closest = null;
            var minDistance = Mathf.Infinity;

            foreach (var enemy in enemies)
            {
                var distance = Vector3.Distance(transform.position, enemy.transform.position);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = enemy.transform;
                }
            }

            return closest ?? transform;
        }

        private void SnapToTarget()
        {
            if (_aimTarget == Vector3.zero) return;
            if (_moveKeys.ReadValue<Vector2>().sqrMagnitude > 0.01f) return;

            var lookDirection = (_aimTarget - transform.position).normalized;

            if (lookDirection.sqrMagnitude < 0.01f) return;

            lookDirection.y = 0;
            var targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * aimSnapSpeed);
        }

        #endregion

        #region Shooting and Reloading

        private void ShootAction(InputAction.CallbackContext context)
        {
            if (currentClip <= 0 && storedAmmo <= 0)
            {
                Debug.Log("Ammo Empty!");
                audioSource.PlayOneShot(emptyGunSound);
                return;
            }

            if (!_canShoot || _isReloading || currentClip <= 0) return;

            _player.SetMovement(true);
            _lastActionTime = Time.time;
            pistol.SetActive(true);

            _canShoot = false;
            _animator.SetTrigger(ShootHash);
            StartCoroutine(ShootWithDelay());
        }

        private IEnumerator ShootWithDelay()
        {
            // Wait before firing the shot to simulate gun handling delay
            yield return new WaitForSeconds(0.5f);

            // Reduce ammo count in the current clip
            currentClip--;

            // Play a random gunshot sound from the "pistol" category at the specified volume
            randomAudio.PlayRandomSound("pistol", gunshotVolume);

            // Create a muzzle flash effect at the gun's fire point
            var muzzleFlash = Instantiate(muzzleFlashPrefab, firePoint.position, firePoint.rotation);
            Destroy(muzzleFlash, 0.1f); // Destroy the effect after 0.1 seconds to clean up memory

            // Instantiate a bullet at the fire point
            var bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            var rb = bullet.GetComponent<Rigidbody>(); // Get the bullet's Rigidbody for physics movement

            // Calculate the direction of the shot based on the aiming target
            var shotDirection = (_aimTarget - firePoint.position).normalized;

            // Apply velocity to the bullet to make it move in the calculated direction
            rb.velocity = shotDirection * bulletSpeed;

            // Wait for the weapon's fire rate cooldown before allowing another shot
            yield return new WaitForSeconds(fireRate);

            // Check if the gun is out of ammo
            if (currentClip <= 0)
            {
                // Automatically start reloading with a 1-second delay
                StartCoroutine(ReloadDelay(1f));
            }
            else
            {
                // Allow shooting again if ammo is still available
                _canShoot = true;
            }

            // Re-enable player movement after shooting
            _player.SetMovement(false);
        }

        private void ReloadAction(InputAction.CallbackContext context)
        {
            if (_isReloading) return;
            StartCoroutine(ReloadDelay(0));
        }

        private IEnumerator ReloadDelay(float reloadTime)
        {
            if (_isReloading || storedAmmo <= 0 || currentClip == maxClipSize)
                yield break;

            _isReloading = true;

            yield return new WaitForSeconds(reloadTime);

            audioSource.PlayOneShot(reloadSound);
            var ammoNeeded = maxClipSize - currentClip;
            var ammoToLoad = Mathf.Min(ammoNeeded, storedAmmo);

            currentClip += ammoToLoad;
            storedAmmo -= ammoToLoad;

            _isReloading = false;
            _reloadTriggered = false;
            _player.SetMovement(false);
            _canShoot = true;
        }

        #endregion
    }
}