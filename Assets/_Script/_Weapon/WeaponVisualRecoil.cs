using UnityEngine;

public class WeaponVisualRecoil : MonoBehaviour
{
    [Header("Recoil Offset")]
    [SerializeField] private Vector3 kickPosition =
        new Vector3(0f, 0f, -0.08f);

    [SerializeField] private Vector3 kickRotation =
        new Vector3(-8f, 0f, 0f);

    [Header("Recoil Timing")]
    [SerializeField, Min(0f)] private float kickDuration = 0.05f;
    [SerializeField, Min(0f)] private float kickSpeed = 30f;
    [SerializeField, Min(0f)] private float returnSpeed = 12f;

    private Vector3 restingPosition;
    private Quaternion restingRotation;
    private float kickTimer;

    private void Awake()
    {
        restingPosition = transform.localPosition;
        restingRotation = transform.localRotation;
    }

    private void Update()
    {
        bool isKicking = kickTimer > 0f;

        Vector3 targetPosition = isKicking
            ? restingPosition + kickPosition
            : restingPosition;

        Quaternion targetRotation = isKicking
            ? restingRotation * Quaternion.Euler(kickRotation)
            : restingRotation;

        float movementSpeed = isKicking
            ? kickSpeed
            : returnSpeed;

        float interpolation = 1f - Mathf.Exp(
            -movementSpeed * Time.deltaTime
        );

        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            targetPosition,
            interpolation
        );

        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            targetRotation,
            interpolation
        );

        if (isKicking)
            kickTimer -= Time.deltaTime;
    }

    private void OnDisable()
    {
        transform.localPosition = restingPosition;
        transform.localRotation = restingRotation;
        kickTimer = 0f;
    }

    public void Play()
    {
        kickTimer = kickDuration;
    }
}