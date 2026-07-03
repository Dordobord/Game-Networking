using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    public Camera cam;
    private float xRot = 0f;

    [SerializeField]private float xSens = 2f;
    [SerializeField]private float ySens = 2f;

    public void ProcessLook(Vector2 input)
    {
        float mouseX = input.x * xSens;
        float mouseY = input.y * ySens;

        xRot -= mouseY;
        xRot = Mathf.Clamp(xRot, -80f, 80f);

        cam.transform.localRotation = Quaternion.Euler(xRot, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
}