using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace _Scripts.Player
{
    public class PistolProfiler : MonoBehaviour
    {
        private static readonly int ShootHash = Animator.StringToHash("Shoot");

        [Header("Pistol Settings")] [SerializeField]
        private GameObject bulletPrefab;

        [SerializeField] private Transform firePoint;
        [SerializeField] private float bulletSpeed = 20f;
        [SerializeField] private float fireRate = 0.3f;

        [Header("Ammo Settings")] [SerializeField]
        private int maxClipSize = 10;

        [SerializeField] public int maxStoredAmmo = 99;
        [SerializeField] public int currentClipAmmo;
        [SerializeField] public int storedAmmo;
        [SerializeField] private bool unlimitedAmmo;

        [Header("Effects")] [SerializeField] private GameObject muzzleFlashPrefab;
        [SerializeField] private GameObject worldCrosshairPrefab;
        private Renderer _crosshairRenderer;
        [SerializeField] private Color defaultCrosshairColor = Color.white;

        [Header("Weapon Visibility")] [SerializeField]
        private GameObject pistol;

        private const float WeaponHideTime = 10f;
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

        [FormerlySerializedAs("randoAudio")] [SerializeField]
        private RandomAudio randomAudio; // ✅ Handles random gunshot sounds

        [SerializeField] private float gunshotVolume = 1.0f;


        private GameObject _worldCrosshair;
        private InputControls _actions;
        private PlayerProfiler _player;
        private Animator _animator;
        private bool _canShoot = true;
        private bool _isReloading;
        private bool _isAiming;
        private Vector3 _aimTarget;

        private void Awake()
        {
            _actions = new InputControls();
            _player = GetComponent<PlayerProfiler>();
            _animator = GetComponent<Animator>();
            pistol.SetActive(false);

            _worldCrosshair = Instantiate(worldCrosshairPrefab);
            _worldCrosshair.SetActive(false);

            _crosshairRenderer = _worldCrosshair.GetComponent<Renderer>();
            _crosshairRenderer.material.color = defaultCrosshairColor;

            _worldCrosshair.transform.localScale *= 1.5f;
        }

        private void OnEnable()
        {
            _actions.Profiler.Enable();
            _actions.Profiler.Shoot.performed += ShootAction;
            _actions.Profiler.Aim.performed += StartAiming;
            _actions.Profiler.Aim.canceled += StopAiming;
        }

        private void OnDisable()
        {
            _actions.Profiler.Shoot.performed -= ShootAction;
            _actions.Profiler.Aim.performed -= StartAiming;
            _actions.Profiler.Aim.canceled -= StopAiming;
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

            if (currentClipAmmo == 0 && storedAmmo > 0 && !_isReloading)
            {
                StartCoroutine(Reload());
            }
        }

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
            Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, 35f, aimableLayers);

            if (enemiesInRange.Length > 0)
            {
                Transform closestEnemy = FindClosestEnemy(enemiesInRange);
                if (closestEnemy is not null)
                {
                    Vector3 enemyCenter = closestEnemy.position + Vector3.up * crosshairHeightOffset;
                    Vector3 directionToPlayer = (playerCamera.transform.position - enemyCenter).normalized;

                    _aimTarget = enemyCenter + directionToPlayer * 1.5f;

                    _worldCrosshair.transform.position = _aimTarget;
                    _worldCrosshair.transform.rotation = Quaternion.LookRotation(-directionToPlayer);

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
            float minDistance = Mathf.Infinity;

            foreach (Collider enemy in enemies)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
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

            Vector3 lookDirection = (_aimTarget - transform.position).normalized;

            if (lookDirection.sqrMagnitude < 0.01f) return;

            lookDirection.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * aimSnapSpeed);
        }

        private void ShootAction(InputAction.CallbackContext context)
        {
            if (currentClipAmmo <= 0 && storedAmmo <= 0)
            {
                Debug.Log("Ammo Empty!");
                audioSource.PlayOneShot(emptyGunSound);
                return;
            }

            if (!_canShoot || _isReloading || currentClipAmmo <= 0) return;

            _player.disableMovement = true;
            _lastActionTime = Time.time;
            pistol.SetActive(true);

            _canShoot = false;
            _animator.SetTrigger(ShootHash);
            StartCoroutine(ShootWithDelay());
        }

        private IEnumerator ShootWithDelay()
        {
            yield return new WaitForSeconds(0.5f);

            currentClipAmmo--;
            PlayRandomGunshot();
            GameObject muzzleFlash = Instantiate(muzzleFlashPrefab, firePoint.position, firePoint.rotation);
            Destroy(muzzleFlash, 0.1f);

            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            Rigidbody rb = bullet.GetComponent<Rigidbody>();

            Vector3 shotDirection = (_aimTarget - firePoint.position).normalized;
            rb.velocity = shotDirection * bulletSpeed;

            yield return new WaitForSeconds(fireRate);

            if (currentClipAmmo <= 0)
            {
                StartCoroutine(Reload());
            }
            else
            {
                _canShoot = true;
            }

            _player.disableMovement = false;
        }

        private void PlayRandomGunshot()
        {
            audioSource.Stop();
            AudioClip randomClip = randomAudio.GetRandomClip("Sounds/Pistol/");
            if (randomClip != null)
            {
                audioSource.PlayOneShot(randomClip, gunshotVolume);
            }
        }

        private IEnumerator Reload()
        {
            if (_isReloading || storedAmmo <= 0 || currentClipAmmo == maxClipSize)
                yield break;

            _isReloading = true;

            yield return new WaitForSeconds(1f);

            audioSource.PlayOneShot(reloadSound);
            int ammoNeeded = maxClipSize - currentClipAmmo;
            int ammoToLoad = Mathf.Min(ammoNeeded, storedAmmo);

            currentClipAmmo += ammoToLoad;
            storedAmmo -= ammoToLoad;

            _isReloading = false;
            _canShoot = true;
        }

        public void RefillAmmo(int amount)
        {
            storedAmmo = Mathf.Min(storedAmmo + amount, maxStoredAmmo);
            Debug.Log($"Ammo refilled! Stored Ammo: {storedAmmo}");
        }
    }
}