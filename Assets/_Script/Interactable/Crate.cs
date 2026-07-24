using UnityEngine;

public class Crate : Interactable
{
    [SerializeField] private NetworkCrate networkCrate;

    protected override void Interact()
    {
        if (networkCrate == null || networkCrate.IsOpen)
            return;

        networkCrate.RequestOpen();
    }
    
    public void FinishAnimation()
    {
        networkCrate?.FinishOpenAnimation();
    }
}