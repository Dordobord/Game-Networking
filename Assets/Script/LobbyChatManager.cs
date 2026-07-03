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
public class LobbyMenuChat : NetworkBehaviour
{
    [Header("Lobby UI")]
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private TMP_Text joinCodeText;
    [SerializeField] private TMP_InputField joinCodeInput;
    [SerializeField] private TMP_Text statusText;
    [Header("Chat UI")]
    [SerializeField] private TMP_Text chatDisplayText;
    [SerializeField] private TMP_InputField chatInputField;
    [Header("Relay Settings")]
    [SerializeField] private int maxConnections = 4;
    private const string WebGLConnectionType = "wss";
    
    private async void Start()
    {
        await InitializeUnityServices();
        SetStatus("Ready.");
        if (joinCodeText != null)
        {
            joinCodeText.text = "Join Code:";
        }
        if (chatDisplayText != null)
        {
            chatDisplayText.text = "Chat:";
        }
    }

    private async System.Threading.Tasks.Task InitializeUnityServices()
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
        }

        catch (Exception exception)
        {
            SetStatus("Unity Services failed to initialize.");
            Debug.LogError(exception);
        }
    }

    public async void StartHost()
    {
        try
        {
            SetStatus("Creating host...");
            await InitializeUnityServices();
            Allocation allocation = await
            RelayService.Instance.CreateAllocationAsync(maxConnections);
            string joinCode = await
            RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            UnityTransport transport =
            NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.UseWebSockets = true;
            transport.SetRelayServerData(
            AllocationUtils.ToRelayServerData(allocation, WebGLConnectionType)
            );
            bool started = NetworkManager.Singleton.StartHost();
            if (started)
            {
            ShowJoinCode(joinCode);
            SetStatus("Host started. Share the join code.");
            AddLocalChatMessage("System: Host started.");
            HideLobbyPanel();
            HideCursor();
            }
            else
            {
            SetStatus("Failed to start Host.");
            }
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
            SetStatus("Joining...");
            await InitializeUnityServices();
            if (joinCodeInput == null)
            {
                SetStatus("Join Code Input is missing.");
                return;
            }

            string joinCode = joinCodeInput.text.Trim().ToUpper();
            if (string.IsNullOrEmpty(joinCode))
            {
                SetStatus("Please enter a join code.");
                return;
            }

            JoinAllocation joinAllocation = await
            RelayService.Instance.JoinAllocationAsync(joinCode);
            UnityTransport transport =
            NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.UseWebSockets = true;
            transport.SetRelayServerData(
            AllocationUtils.ToRelayServerData(joinAllocation,
            WebGLConnectionType)
            );

            bool started = NetworkManager.Singleton.StartClient();

            if (started)
            {
                SetStatus("Client started.");
                AddLocalChatMessage("System: Client joined.");
                HideLobbyPanel();
                HideCursor();
            }
            else
            {
            SetStatus("Failed to start Client.");
            }
        }
        catch (Exception exception)
        {
            SetStatus("Client failed. Check join code and Console.");
            Debug.LogError(exception);
        }
    }

    public void SendChatMessage()
    {
        if (chatInputField == null)
        {
            
        }

        string message = chatInputField.text.Trim();
        if (string.IsNullOrEmpty(message))
        {
            
        }

        chatInputField.text = "";
        string senderName = "Player " + NetworkManager.Singleton.LocalClientId;
        FixedString128Bytes fixedMessage = senderName + ": " + message;
        SendChatMessageRpc(fixedMessage);
    }

    [Rpc(SendTo.Server)]
    private void SendChatMessageRpc(FixedString128Bytes message)
    {
        BroadcastChatMessageRpc(message);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void BroadcastChatMessageRpc(FixedString128Bytes message)
    {
        AddLocalChatMessage(message.ToString());
    }

    private void ShowJoinCode(string joinCode)
    {
        if (joinCodeText != null)
        {
            joinCodeText.text = "Join Code: " + joinCode;
        }
    }

    private void HideLobbyPanel()
    {
        if (lobbyPanel != null)
        {
            lobbyPanel.SetActive(false);
        }
    }

    private void HideCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log(message);
    }

    private void AddLocalChatMessage(string message)
    {
        if (chatDisplayText == null)
        {
        return;
        }
        chatDisplayText.text += "\n" + message;
    }
}