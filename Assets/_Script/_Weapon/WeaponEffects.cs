using UnityEngine;

public class WeaponEffects : MonoBehaviour
{
    [Header("Muzzle Flash")]
    [SerializeField] private GameObject muzzleFlashRoot;

    private ParticleSystem[] muzzleParticles;

    private void Awake()
    {
        if (muzzleFlashRoot == null)
        {
            Debug.LogWarning($"{name}: Muzzle flash root is not assigned.");
            return;
        }

        muzzleParticles =
            muzzleFlashRoot.GetComponentsInChildren<ParticleSystem>(true);
    }

    public void PlayShoot()
    {
        if (muzzleParticles == null)
            return;

        foreach (ParticleSystem particle in muzzleParticles)
        {
            particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particle.Play(true);
        }
    }
}