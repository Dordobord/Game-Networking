using UnityEngine;

public class CollectObject : Interactable
{
    public GameObject particle;

    protected override void Interact()
    {
        Destroy(gameObject);
        Instantiate(particle, transform.position, Quaternion.identity);
    }
}
