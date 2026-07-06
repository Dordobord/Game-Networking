using UnityEngine;
using Unity.Netcode;

public class PlayerInteraction : NetworkBehaviour
{
    private Camera cam;

    [SerializeField] private float distance = 3f;
    [SerializeField] private LayerMask mask;

    private UIPlayer playerUI;
    private PlayerInputController inputManager;

    private string currentPrompt = "";

    void Start()
    {
        cam = GetComponent<PlayerLook>()._cam;
        playerUI = GetComponent<UIPlayer>();
        inputManager = GetComponent<PlayerInputController>();
    }

    void Update()
    {
        if (!IsOwner) return;
        string newPrompt = "";

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, distance, mask))
        {
            if (hitInfo.collider.TryGetComponent(out Interactable interactable))
            {
                newPrompt = interactable.promptMessage;

                if (inputManager.OnFoot.Interact.triggered)
                {
                    interactable.BaseInteract();
                }
            }
        }

        if (currentPrompt != newPrompt)
        {
            currentPrompt = newPrompt;
            playerUI.UpdateText(currentPrompt);
        }
    }
}