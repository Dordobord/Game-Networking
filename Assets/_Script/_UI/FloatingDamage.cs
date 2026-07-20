using TMPro;
using UnityEngine;

public class FloatingDamage : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text damageText;

    [Header("Animation")]
    [SerializeField] private float floatHeight = 1.5f;
    [SerializeField] private float lifeTime = 1f;
    [SerializeField] private float fadeStart = 0.5f;

    private Vector3 startPosition;
    private Vector3 targetPosition;

    private float timer;
    private Color originalColor;
    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;

        if (damageText != null)
            originalColor = damageText.color;
    }

    public void Setup(int damage)
    {
        damageText.text = damage.ToString();

        startPosition = transform.position;
        targetPosition = startPosition + Vector3.up * floatHeight;

        timer = 0f;
        damageText.color = originalColor;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        float t = Mathf.Clamp01(timer / lifeTime);

        // Smooth floating animation
        transform.position = Vector3.Lerp(
            startPosition,
            targetPosition,
            Mathf.SmoothStep(0f, 1f, t)
        );

        // Fade out during the second half
        if (t >= fadeStart)
        {
            float fade = Mathf.InverseLerp(1f, fadeStart, t);

            Color c = damageText.color;
            c.a = fade;
            damageText.color = c;
        }

        if (timer >= lifeTime)
        {
            Destroy(gameObject);
        }
    }

    private void LateUpdate()
    {
        if (cam == null || !cam.isActiveAndEnabled)
            cam = Camera.main;

        if (cam != null)
            transform.rotation = cam.transform.rotation;
    }
}