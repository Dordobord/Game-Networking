using UnityEngine;

public class Crate : Interactable
{
    [SerializeField]private GameObject crate;
    private bool crateOpen;
    private bool isAnimating;

    protected override void Interact()
    {
        if (isAnimating) return;

        isAnimating = true;
        crateOpen = !crateOpen;
        crate.GetComponent<Animator>().SetBool("IsOpen", crateOpen);
    }

    public void FinishAnimation()
    {
        isAnimating = false;
    }
}
