using System.Collections;
using Mirror;
using SkillForge.Demo;
using SkillForge.Core;
using SkillForge.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SkillForge.UI
{
    public class DebugClientUI : MonoBehaviour
    {
        [Header("UI Elements"), SerializeField]
        private GameObject _actionPanel;

        [SerializeField]
        private TMP_Text _statusText;

        [SerializeField]
        private TMP_Text _errorText;

        [SerializeField]
        private Button _connectButton;

        [SerializeField] private DemoTaskSpace _taskSpace;
        [SerializeField] private Button _currentActionButton;

        [SerializeField]
        private Button _btnEndSession;

        private int _playerId = -1;
        private UpdateUIMessage _step;

private void Start()
{
    if (_connectButton != null)
    {
        _connectButton.onClick.AddListener(OnConnectClicked);
    }

    if (_currentActionButton != null)
    {
        _currentActionButton.onClick.AddListener(SendCurrentAction);
    }

    if (_btnEndSession != null)
    {
        _btnEndSession.onClick.AddListener(NetworkMessageSender.SendEndSession);
    }

    if (_actionPanel != null)
    {
        _actionPanel.SetActive(false);
    }

    UpdateStatus("Not connected.");
    _taskSpace.GameMessageReceived += OnGameNetworkMessage;
    _taskSpace.StepUpdated += OnUIUpdate;
}

private void OnUIUpdate(UpdateUIMessage msg)
{
    _step = msg;
    if (_currentActionButton != null)
    {
        _currentActionButton.interactable = !string.IsNullOrEmpty(msg.intention.action);
    }

    UpdateStatus($"Шаг {msg.stepIndex + 1}. {msg.instruction}");
}


        private void OnDestroy()
        {
            if (_taskSpace != null)
            {
                _taskSpace.GameMessageReceived -= OnGameNetworkMessage;
                _taskSpace.StepUpdated -= OnUIUpdate;
            }
        }

        private void Update()
        {
            if (_playerId < 0 && NetworkClient.isConnected && NetworkClient.localPlayer != null)
            {
                _playerId = (int)NetworkClient.localPlayer.netId;
                SetPlayerId(_playerId);
            }
        }

        public void UpdateStatus(string message)
        {
            if (_statusText != null)
            {
                _statusText.text = message;
            }
        }

        public void ShowError(string message)
        {
            if (_errorText != null)
            {
                _errorText.text = $"Error: {message}";
                _errorText.gameObject.SetActive(true);
            }

            Debug.LogWarning($"[DebugClientUI] {message}");
        }

        public void SetPlayerId(int id)
        {
            _playerId = id;
            UpdateStatus($"Connected as Player {id}");
        }

        private void OnSessionActiveChanged(bool active)
        {
            if (_actionPanel != null)
            {
                _actionPanel.SetActive(active);
            }

            if (_statusText != null)
            {
                _statusText.text = active ? "Session active" : "Waiting for session...";
            }
        }

        private void OnGameNetworkMessage(GameNetworkMessage msg)
        {
            switch (msg.type)
            {
                case "session_state":
                    OnSessionActiveChanged(msg.payload == "1");
                    break;
                case "error":
                    ShowError(msg.payload);
                    break;
                case "report":
                    UpdateStatus("Отчёт: " + msg.payload);
                    break;
            }
        }

private void OnConnectClicked()
{
    if (NetworkClient.isConnected || NetworkServer.active)
    {
        UpdateStatus(NetworkClient.isConnected ? "Connected." : "Host is running.");
        return;
    }

    var networkManager = NetworkManager.singleton;

    if (networkManager == null)
    {
        ShowError("NetworkManager not found.");
        return;
    }

    Debug.Log($"[DebugClientUI] Starting client, connecting to {networkManager.networkAddress}");
    networkManager.StartClient();
    UpdateStatus("Connecting...");
    StartCoroutine(CheckConnectionTimeout());
}

        private IEnumerator CheckConnectionTimeout()
        {
            yield return new WaitForSeconds(5f);

            if (!NetworkClient.isConnected)
            {
                ShowError($"Connection timed out. Server at {NetworkManager.singleton?.networkAddress ?? "unknown"} not reachable.");
                UpdateStatus("Connection failed.");
            }
        }

        private void SendCurrentAction()
        {
            if (NetworkClient.isConnected && !string.IsNullOrEmpty(_step.intention.action))
            {
                NetworkMessageSender.SendIntention(_step.intention);
            }
        }
    }
}
