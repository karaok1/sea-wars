using System;
using Unity.Netcode;
using UnityEngine;

public class MultiplayerController : MonoBehaviour
{
    private void Start()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted += OnServerStarted;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }

        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-mode" && i + 1 < args.Length && args[i + 1] == "server")
            {
                StartHeadlessServer();
                return;
            }
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    private void OnServerStarted()
    {
        Debug.Log("Server Started");
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client Connected: {clientId}");
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.LogError($"Client Disconnected: {clientId}. Possible Connection Timeout or Server Shutdown.");
    }

    private void StartHeadlessServer()
    {
        Debug.Log("Starting Headless Server via CLI...");
        bool started = NetworkManager.Singleton.StartServer();
        if (started)
        {
            Debug.Log("Headless Server Started. Listening for connections...");
        }
        else
        {
            Debug.LogError("Failed to start Headless Server.");
        }
    }
}
