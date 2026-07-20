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

    // Called by an Animation Event at the end of CrateOpen.
    public void FinishAnimation()
    {
        networkCrate?.FinishOpenAnimation();
    }
}