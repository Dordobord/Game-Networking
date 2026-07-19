using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class SpawnZone : MonoBehaviour
{
    private BoxCollider zoneCollider;

    private void Awake()
    {
        zoneCollider = GetComponent<BoxCollider>();
    }

    public Vector3 GetRandomPoint()
    {
        if (zoneCollider == null)
            zoneCollider = GetComponent<BoxCollider>();

        Bounds bounds = zoneCollider.bounds;

        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }

    private void OnValidate()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();

        if (boxCollider != null)
            boxCollider.isTrigger = true;
    }
}