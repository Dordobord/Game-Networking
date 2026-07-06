using UnityEngine;

public class BulletVisuals : MonoBehaviour
{
    [SerializeField] private float speed = 150f;

    private Vector3 targetPoint;

    public void SetTarget(Vector3 point)
    {
        targetPoint = point;

        Destroy(gameObject, 2f);
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPoint,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPoint) < 0.1f)
        {
            Destroy(gameObject);
        }
    }
}