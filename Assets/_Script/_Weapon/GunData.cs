using UnityEngine;

[CreateAssetMenu(
    fileName = "New Gun Data",
    menuName = "Weapons/Gun Data"
)]
public class GunData : ScriptableObject
{
    [Header("Weapon")]
    public string weaponName;

    [Header("Damage")]
    public float damage = 25f;
    public float range = 100f;

    [Header("Firing")]
    public float fireRate = 0.15f;
    public bool isAutomatic;

    [Header("Spread")]
    public float hipFireSpread = 0.03f;

    [Header("Ammo")]
    public int magazineSize = 12;
    public float reloadTime = 1.5f;
}