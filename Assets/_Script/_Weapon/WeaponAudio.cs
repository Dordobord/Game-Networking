using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class WeaponAudio : MonoBehaviour
{
    [Header("Weapon Sounds")]
    [SerializeField] private AudioClip shootClip;
    [SerializeField] private AudioClip reloadClip;
    [SerializeField] private AudioClip emptyClip;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayShoot()
    {
        PlayOneShot(shootClip);
    }

    public void PlayReload()
    {
        PlayOneShot(reloadClip);
    }

    public void PlayEmpty()
    {
        PlayOneShot(emptyClip);
    }

    private void PlayOneShot(AudioClip clip)
    {
        if (clip != null) audioSource.PlayOneShot(clip);
    }
}