using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [SerializeField] private string promptMessage;

    protected GameObject CurrentInteractor { get; private set; }

    public string PromptMessage => promptMessage;

    public void BaseInteract(GameObject interactor)
    {
        CurrentInteractor = interactor;

        Interact();

        CurrentInteractor = null;
    }

    protected virtual void Interact()
    {
    }
}