using System.Collections;
using TMPro;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("Gun Settings")]
    [SerializeField] private GunData gunData;

    [Header("Bullet Visual")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform muzzlePoint;

    [Header("Floating Damage")]
    [SerializeField] private FloatingDamage floatingDamagePrefab;

    private PlayerWeaponManager weaponManager;
    private PlayerInputController inputController;
    private PlayerLook playerLook;
    private WeaponAudio weaponAudio;
    private WeaponEffects weaponEffects;
    private WeaponVisualRecoil visualRecoil;
    private Camera cam;
    private TMP_Text ammoText;

    private int currentAmmo;
    private bool isReloading;
    private float nextFireTime;

    public GunData Data => gunData;
    public GameObject BulletPrefab => bulletPrefab;
    public FloatingDamage FloatingDamagePrefab => floatingDamagePrefab;

    public void Initialize(
        PlayerWeaponManager manager,
        TMP_Text playerAmmoText)
    {
        weaponManager = manager;
        ammoText = playerAmmoText;

        inputController = weaponManager.GetComponent<PlayerInputController>();

        playerLook = weaponManager.GetComponent<PlayerLook>();

        weaponAudio = GetComponent<WeaponAudio>();
        weaponEffects = GetComponent<WeaponEffects>();
        visualRecoil = GetComponentInChildren<WeaponVisualRecoil>();

        if (playerLook != null)
            cam = playerLook._cam;

        if (gunData != null)
            currentAmmo = gunData.magazineSize;

        UpdateAmmoUI();
    }

    private void Update()
    {
        if (!CanUseGun())
            return;

        HandleReload();
        HandleShooting();
    }

    private bool CanUseGun()
    {
        return weaponManager != null
            && weaponManager.IsOwner
            && gunData != null
            && inputController != null
            && playerLook != null
            && cam != null
            && muzzlePoint != null;
    }

    private void HandleReload()
    {
        if (isReloading)
            return;

        if (inputController.OnFoot.Reload.WasPressedThisFrame())
            TryReload();
    }

    private void HandleShooting()
    {
        if (isReloading)
            return;

        bool shootPressed = gunData.isAutomatic ? inputController.OnFoot.Shoot.IsPressed() : inputController.OnFoot.Shoot.WasPressedThisFrame();

        if (!shootPressed)
            return;

        if (Time.time < nextFireTime)
            return;

        if (currentAmmo <= 0)
        {
            PlayEmptyFeedback();
            nextFireTime = Time.time + gunData.fireRate;
            return;
        }

        nextFireTime = Time.time + gunData.fireRate;
        currentAmmo--;

        UpdateAmmoUI();

        Vector3 shootDirection = GetSpreadDirection();

        weaponManager.RequestShoot(cam.transform.position, shootDirection, muzzlePoint.position);

        visualRecoil?.Play();
        playerLook.Recoil();
        if (currentAmmo <= 0)
            TryReload();
    }

    private void TryReload()
    {
        if (isReloading || currentAmmo >= gunData.magazineSize)
            return;

        StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        isReloading = true;
        UpdateAmmoUI();

        weaponManager.RequestReload();

        yield return new WaitForSeconds(gunData.reloadTime);

        currentAmmo = gunData.magazineSize;
        isReloading = false;

        UpdateAmmoUI();
    }

    public void PlayShootFeedback()
    {
        weaponAudio?.PlayShoot();
        weaponEffects?.PlayShoot();
    }

    public void PlayReloadFeedback()
    {
        weaponAudio?.PlayReload();
    }

    private void PlayEmptyFeedback()
    {
        weaponAudio?.PlayEmpty();
    }

    private void UpdateAmmoUI()
    {
        if (ammoText == null || weaponManager == null || !weaponManager.IsOwner || gunData == null)
        {
            return;
        }

        ammoText.text = isReloading ? "Reloading..." : $"{currentAmmo} / {gunData.magazineSize}";
    }

    private Vector3 GetSpreadDirection()
    {
        Vector3 direction = cam.transform.forward;

        direction += cam.transform.right * Random.Range(-gunData.hipFireSpread,gunData.hipFireSpread);
        direction += cam.transform.up *
            Random.Range(-gunData.hipFireSpread, gunData.hipFireSpread);
        return direction.normalized;
    }
}