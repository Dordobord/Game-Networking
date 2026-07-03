using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    private Camera cam;

    [SerializeField]private float distance = 3f;
    [SerializeField]private LayerMask mask;

    private PlayerUI playerUI;
    private InputManager inputManager;

    private string currentPrompt = "";

    void Start()
    {
        cam = GetComponent<PlayerLook>().cam;
        playerUI = GetComponent<PlayerUI>();
        inputManager = GetComponent<InputManager>();
    }

    void Update()
    {
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