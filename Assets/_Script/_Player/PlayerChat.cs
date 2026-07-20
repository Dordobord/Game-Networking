using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerChat : NetworkBehaviour
{
    [Header("Gameplay Chat UI")]
    [SerializeField] private GameObject chatPanel;
    [SerializeField] private TMP_Text chatDisplayText;
    [SerializeField] private TMP_InputField chatInputField;
    [SerializeField] private ScrollRect chatScrollRect;

    public static bool IsChatOpen { get; private set; }

    private static PlayerChat localPlayerChat;
    private PlayerInputController inputController;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            if (chatPanel != null)
                chatPanel.SetActive(false);

            return;
        }

        localPlayerChat = this;
        IsChatOpen = false;
        inputController = GetComponent<PlayerInputController>();

        if (chatPanel != null)
            chatPanel.SetActive(true);

        if (chatDisplayText != null)
            chatDisplayText.text = "";

        if (chatInputField != null)
        {
            chatInputField.text = "";
            chatInputField.gameObject.SetActive(false);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner)
            return;

        if (localPlayerChat == this)
            localPlayerChat = null;

        IsChatOpen = false;
    }

    private void Update()
    {
        if (!IsOwner || Keyboard.current == null)
            return;

        if (inputController != null && !inputController.GameplayInputAllowed)
            return;

        bool enterPressed =
            Keyboard.current.enterKey.wasPressedThisFrame ||
            Keyboard.current.numpadEnterKey.wasPressedThisFrame;

        if (!enterPressed)
            return;

        if (!IsChatOpen)
        {
            OpenChat();
            return;
        }

        SendCurrentMessage();
        CloseChat();
    }

    private void OpenChat()
    {
        if (chatInputField == null)
            return;

        IsChatOpen = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        chatInputField.gameObject.SetActive(true);
        chatInputField.text = "";
        chatInputField.ActivateInputField();
        chatInputField.Select();
    }

    private void CloseChat()
    {
        IsChatOpen = false;

        if (chatInputField != null)
        {
            chatInputField.text = "";
            chatInputField.DeactivateInputField();
            chatInputField.gameObject.SetActive(false);
        }

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void CloseForRespawn()
    {
        if (!IsOwner || !IsChatOpen)
            return;

        CloseChat();
    }

    private void SendCurrentMessage()
    {
        if (chatInputField == null)
            return;

        string message = chatInputField.text.Trim();

        if (string.IsNullOrWhiteSpace(message))
            return;

        FixedString128Bytes networkMessage = message;
        SendChatMessageRpc(networkMessage);
    }

    [Rpc(SendTo.Server)]
    private void SendChatMessageRpc(
        FixedString128Bytes message,
        RpcParams rpcParams = default)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;
        FixedString512Bytes formattedMessage =
            "Player " + senderId + ": " + message;

        BroadcastChatMessageRpc(formattedMessage);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void BroadcastChatMessageRpc(
        FixedString512Bytes message)
    {
        localPlayerChat?.AddMessage(message.ToString());
    }

    private void AddMessage(string message)
    {
        if (chatDisplayText == null)
            return;

        chatDisplayText.text += "\n" + message;

        if (chatScrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            chatScrollRect.verticalNormalizedPosition = 0f;
        }
    }
}