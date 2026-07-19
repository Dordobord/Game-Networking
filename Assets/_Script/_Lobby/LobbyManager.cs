using System;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class LobbyManager : NetworkBehaviour
{
    [Header("Lobby UI")]
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private GameObject hostButton;
    [SerializeField] private GameObject joinButton;
    [SerializeField] private GameObject joinCodeInputObject;

    [SerializeField] private TMP_Text joinCodeText;
    [SerializeField] private TMP_InputField joinCodeInput;
    [SerializeField] private TMP_Text statusText;

    [Header("Chat UI")]
    [SerializeField] private GameObject chatPanel;
    [SerializeField] private TMP_Text chatDisplayText;
    [SerializeField] private TMP_InputField chatInputField;

    [Header("Gameplay UI")]
    [SerializeField] private GameObject gameUI;

    [Header("Relay Settings")]
    [SerializeField] private int maxConnections = 4;

    private const string WebGLConnectionType = "wss";

    public static bool IsChatOpen { get; private set; }

    private bool isChatOpen;
    private bool servicesReady;

    private async void Start()
    {
        ResetUIToLobbyState();

        SetStatus("Initializing Unity Services...");

        servicesReady = await InitializeUnityServices();

        if (servicesReady)
            SetStatus("Ready.");
    }

    private void ResetUIToLobbyState()
    {
        isChatOpen = false;
        IsChatOpen = false;

        if (lobbyPanel != null)
            lobbyPanel.SetActive(true);

        if (hostButton != null)
            hostButton.SetActive(true);

        if (joinButton != null)
            joinButton.SetActive(true);

        if (joinCodeInputObject != null)
            joinCodeInputObject.SetActive(true);

        if (joinCodeText != null)
        {
            joinCodeText.text = "";
            joinCodeText.gameObject.SetActive(false);
        }

        if (chatDisplayText != null)
            chatDisplayText.text = "Chat:";

        if (chatInputField != null)
        {
            chatInputField.text = "";
            chatInputField.DeactivateInputField();
        }

        if (chatPanel != null)
            chatPanel.SetActive(false);

        if (gameUI != null)
            gameUI.SetActive(false);

        ShowCursor();
    }

    private async System.Threading.Tasks.Task<bool> InitializeUnityServices()
    {
        try
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                await UnityServices.InitializeAsync();
            }

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            return true;
        }
        catch (Exception exception)
        {
            SetStatus("Unity Services failed to initialize.");
            Debug.LogError(exception);
            return false;
        }
    }

    public async void StartHost()
    {
        try
        {
            SetStatus("Creating host...");

            if (!servicesReady)
            {
                servicesReady = await InitializeUnityServices();

                if (!servicesReady)
                    return;
            }

            if (NetworkManager.Singleton == null)
            {
                SetStatus("NetworkManager is missing.");
                return;
            }

            UnityTransport transport =
                NetworkManager.Singleton.GetComponent<UnityTransport>();

            if (transport == null)
            {
                SetStatus("Unity Transport is missing.");
                return;
            }

            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            transport.UseWebSockets = true; transport.SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, WebGLConnectionType));
            bool started = NetworkManager.Singleton.StartHost();
            if (!started)
            {
                SetStatus("Failed to start Host.");
                return;
            }

            DisableLobbyControls();
            HideLobbyPanel();
            ShowJoinCode(joinCode);
            ShowChatPanel();
            ShowGameUI();

            SetStatus("Host started. Share the join code.");
            AddLocalChatMessage("System: Host started.");

            HideCursor();
        }
        catch (Exception exception)
        {
            SetStatus("Host failed. Check Console.");
            Debug.LogError(exception);
        }
    }

    public async void StartClient()
    {
        try
        {
            if (joinCodeInput == null)
            {
                SetStatus("Join Code Input is missing.");
                return;
            }

            string joinCode =
                joinCodeInput.text.Trim().ToUpperInvariant();

            if (string.IsNullOrWhiteSpace(joinCode))
            {
                SetStatus("Please enter a join code.");
                return;
            }

            SetStatus("Joining...");

            if (!servicesReady)
            {
                servicesReady = await InitializeUnityServices();

                if (!servicesReady)
                    return;
            }

            if (NetworkManager.Singleton == null)
            {
                SetStatus("NetworkManager is missing.");
                return;
            }

            UnityTransport transport =
                NetworkManager.Singleton.GetComponent<UnityTransport>();

            if (transport == null)
            {
                SetStatus("Unity Transport is missing.");
                return;
            }

            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            transport.UseWebSockets = true;
            transport.SetRelayServerData(AllocationUtils.ToRelayServerData( joinAllocation, WebGLConnectionType));
            bool started = NetworkManager.Singleton.StartClient();
            if (!started)
            {
                SetStatus("Failed to start Client.");
                return;
            }

            HideJoinCode();
            HideLobbyPanel();
            ShowChatPanel();
            ShowGameUI();

            SetStatus("Client started.");
            AddLocalChatMessage("System: Client joined.");

            HideCursor();
        }
        catch (Exception exception)
        {
            SetStatus("Client failed. Check join code and Console.");
            Debug.LogError(exception);
        }
    }

    private void Update()
    {
        HandleChatToggle();
    }

    private void HandleChatToggle()
    {
        if (chatInputField == null || Keyboard.current == null)
            return;

        if (chatPanel == null || !chatPanel.activeInHierarchy)
            return;

        bool enterPressed = Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame;

        if (!enterPressed)
            return;

        if (!isChatOpen)
        {
            OpenChat();
        }
        else
        {
            SendChatMessage();
            CloseChat();
        }
    }

    private void OpenChat()
    {
        if (chatInputField == null)
            return;

        isChatOpen = true;
        IsChatOpen = true;

        ShowCursor();

        chatInputField.text = "";
        chatInputField.gameObject.SetActive(true);
        chatInputField.ActivateInputField();
        chatInputField.Select();
    }

    private void CloseChat()
    {
        isChatOpen = false;
        IsChatOpen = false;

        if (chatInputField != null)
            chatInputField.DeactivateInputField();

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        HideCursor();
    }

    public void SendChatMessage()
    {
        if (chatInputField == null)
            return;

        string message = chatInputField.text.Trim();

        if (string.IsNullOrWhiteSpace(message))
            return;

        chatInputField.text = "";

        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
        {
            SetStatus("Connect as Host or Client before sending chat.");
            return;
        }

        string senderName = "Player " + NetworkManager.Singleton.LocalClientId;
        FixedString128Bytes fixedMessage = senderName + ": " + message;
        SendChatMessageRpc(fixedMessage);
    }

    [Rpc(SendTo.Server)]
    private void SendChatMessageRpc(
        FixedString128Bytes message)
    {
        BroadcastChatMessageRpc(message);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void BroadcastChatMessageRpc(
        FixedString128Bytes message)
    {
        AddLocalChatMessage(message.ToString());
    }

    private void ShowJoinCode(string joinCode)
    {
        if (joinCodeText == null)
            return;

        joinCodeText.text = "Join Code: " + joinCode;
        joinCodeText.gameObject.SetActive(true);
    }

    private void HideJoinCode()
    {
        if (joinCodeText == null)
            return;

        joinCodeText.text = "";
        joinCodeText.gameObject.SetActive(false);
    }

    private void DisableLobbyControls()
    {
        if (hostButton != null)
            hostButton.SetActive(false);

        if (joinButton != null)
            joinButton.SetActive(false);

        if (joinCodeInputObject != null)
            joinCodeInputObject.SetActive(false);
    }

    private void HideLobbyPanel()
    {
        if (lobbyPanel != null)
            lobbyPanel.SetActive(false);
    }

    private void ShowChatPanel()
    {
        if (chatPanel != null)
            chatPanel.SetActive(true);
    }

    private void ShowGameUI()
    {
        if (gameUI != null)
            gameUI.SetActive(true);
    }

    private void HideCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void ShowCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;

        Debug.Log(message);
    }

    private void AddLocalChatMessage(string message)
    {
        if (chatDisplayText == null)
            return;

        if (string.IsNullOrEmpty(chatDisplayText.text))
            chatDisplayText.text = "Chat:";

        chatDisplayText.text += "\n" + message;
    }
}