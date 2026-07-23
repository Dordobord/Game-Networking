using UnityEngine;
using Unity.Netcode;

public class PlayerLook : NetworkBehaviour
{
    [Header("Mode")]
    [SerializeField] private bool offlineMode;

    [Header ("Mouse Sensitivty")]
    [SerializeField]private float xSens = 2f;
    [SerializeField]private float ySens = 2f;

    [Header("Recoil")]
    [SerializeField]private float recoilX = 2f;
    [SerializeField]private float recoilY = .5f;
    [SerializeField]private float recoilSpeed = 10f;
    [SerializeField]private float snap = 20f;

    private Vector2 currentRecoil;
    private Vector2 targetRecoil;
    public Camera _cam;
    private Vector2 recoilVelocity;
    private float xRot = 0f;

    private void Start()
    {
        if (offlineMode)
            SetLocalCameraState(true);
    }

    public override void OnNetworkSpawn()
    {
        SetLocalCameraState(IsOwner);
    }

    public void ProcessLook(Vector2 input)
    {
        if (_cam == null)
            return;

        float mouseX = input.x * xSens;
        float mouseY = input.y * ySens;

        xRot -= mouseY;
        xRot = Mathf.Clamp(xRot, -80f, 80f);

        targetRecoil = Vector2.MoveTowards(targetRecoil, Vector2.zero, recoilSpeed * Time.deltaTime);
        currentRecoil = Vector2.SmoothDamp(currentRecoil, targetRecoil, ref recoilVelocity, 1f/snap);

        _cam.transform.localRotation = Quaternion.Euler(xRot - currentRecoil.x, currentRecoil.y, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    public void Recoil()
    {
        targetRecoil += new Vector2(recoilX, Random.Range(-recoilY, recoilY));
    }

    private void SetLocalCameraState(bool enabled)
    {
        Camera playerCam = GetComponentInChildren<Camera>(true);
        AudioListener playerListener = GetComponentInChildren<AudioListener>(true);

        if (playerCam != null)
        {
            playerCam.enabled = enabled;
            _cam = playerCam;
        }

        if (playerListener != null)
            playerListener.enabled = enabled;
    }
}