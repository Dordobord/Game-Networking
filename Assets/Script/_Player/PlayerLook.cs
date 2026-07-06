using UnityEngine;
using Unity.Netcode;

public class PlayerLook : NetworkBehaviour
{
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

    public override void OnNetworkSpawn()
    {
        Camera playerCam = GetComponentInChildren<Camera>();
        AudioListener playerListener = GetComponentInChildren<AudioListener>();

        if (IsOwner)
        {
            if (playerCam != null) playerCam.enabled = true;

            if (playerListener != null) playerListener.enabled = true;
        }
        else
        {
            if (playerCam != null)playerCam.enabled = false;

            if (playerListener != null)playerListener.enabled = false;
        }
    }

    public void ProcessLook(Vector2 input)
    {
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
}