using UnityEngine;

public class TopDownCameraFollow : MonoBehaviour
{
    [SerializeField] Vector3 offset = new Vector3(0f, 5f, -6f);
    [SerializeField] private float followSpeed;

    private Transform target;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPos = target.position + offset;
        transform.position = 
            Vector3.Lerp(transform.position, desiredPos, followSpeed * Time.deltaTime);
        
        transform.LookAt(target.position);
    }
}
