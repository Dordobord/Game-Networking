using Unity.Netcode;
using UnityEngine;

public class PlayerInteraction : NetworkBehaviour
{
    [Header("Interaction")]
    [SerializeField] private float distance = 3f;
    [SerializeField] private LayerMask mask;

    private Camera cam;
    private UIPlayer playerUI;
    private PlayerInputController inputController;
    private string currentPrompt = string.Empty;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        PlayerLook playerLook = GetComponent<PlayerLook>();

        if (playerLook != null) cam = playerLook._cam;

        inputController = GetComponent<PlayerInputController>();
        playerUI = FindFirstObjectByType<UIPlayer>();

        if (cam == null)
        {
            Debug.LogError("PlayerInteraction: Camera was not found.");
        }

        if (inputController == null)
        {
            Debug.LogError("PlayerInteraction: PlayerInputController was not found.");
        }

        if (playerUI == null)
        {
            Debug.LogError("PlayerInteraction: UIPlayer was not found.");
        }
    }

    private void Update()
    {
        if (!IsOwner || cam == null || inputController == null || playerUI == null)
        {
            return;
        }

        string newPrompt = string.Empty;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, distance, mask))
        {
            Interactable interactable = hitInfo.collider.GetComponentInParent<Interactable>();

            if (interactable != null)
            {
                newPrompt = interactable.PromptMessage;
                if (inputController.OnFoot.Interact.WasPressedThisFrame())
                {
                    interactable.BaseInteract(gameObject);
                }
            }
        }

        if (currentPrompt == newPrompt) return;

        currentPrompt = newPrompt;
        playerUI.UpdateText(currentPrompt);
    }
}