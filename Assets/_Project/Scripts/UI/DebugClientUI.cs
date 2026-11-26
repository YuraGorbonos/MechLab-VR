using System.Collections;
using Mirror;
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

        [SerializeField]
        private Button _btnScanDiagPort;

        [SerializeField]
        private Button _btnScanBrakes;

        [SerializeField]
        private Button _btnLoosenBoltLeft;

        [SerializeField]
        private Button _btnRemoveCaliperLeft;

        [SerializeField]
        private Button _btnRemoveBrakePadLeft;

        [SerializeField]
        private Button _btnInstallBrakePadLeft;

        [SerializeField]
        private Button _btnInstallCaliperLeft;

        [SerializeField]
        private Button _btnTightenBoltLeft;

        [SerializeField]
        private Button _btnLoosenBoltRight;

        [SerializeField]
        private Button _btnRemoveCaliperRight;

        [SerializeField]
        private Button _btnRemovePadRight;

        [SerializeField]
        private Button _btnInstallPadRight;

        [SerializeField]
        private Button _btnInstallCaliperRight;

        [SerializeField]
        private Button _btnTightenBoltRight;

        [SerializeField]
        private Button _btnFinalDiagnostic;

        [SerializeField]
        private Button _btnEndSession;

        private int _playerId = -1;

private void Start()
{
    if (_connectButton != null)
    {
        _connectButton.onClick.AddListener(OnConnectClicked);
    }

    SetupActionButtons();

    if (_actionPanel != null)
    {
        _actionPanel.SetActive(false);
    }

    UpdateStatus("Not connected.");
    GameManager.OnErrorReceived += OnServerErrorReceived;
    GameManager.OnActiveSessionChanged += OnSessionActiveChanged;
    NetworkClient.RegisterHandler<GameNetworkMessage>(OnGameNetworkMessage);
    NetworkClient.RegisterHandler<UpdateUIMessage>(OnUIUpdate);
}

private void OnUIUpdate(UpdateUIMessage msg)
{
    UpdateStatus($"Шаг {msg.stepIndex + 1}: {msg.instruction}");
}


        private void OnDestroy()
        {
            GameManager.OnErrorReceived -= OnServerErrorReceived;
            GameManager.OnActiveSessionChanged -= OnSessionActiveChanged;

            if (NetworkClient.isConnected)
            {
                NetworkClient.UnregisterHandler<GameNetworkMessage>();
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

        private void OnServerErrorReceived(string message)
        {
            ShowError(message);
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

        private void SetupActionButtons()
        {
            if (_btnScanDiagPort != null)
            {
                _btnScanDiagPort.onClick.AddListener(() => SendIntention(new Intention
                {
                    playerId = _playerId,
                    intentType = "scan",
                    targetObject = "diag_port"
                }));
            }

            if (_btnScanBrakes != null)
            {
                _btnScanBrakes.onClick.AddListener(() => SendIntention(new Intention
                {
                    playerId = _playerId,
                    intentType = "scan",
                    targetObject = "brakes"
                }));
            }

            if (_btnLoosenBoltLeft != null)
            {
                _btnLoosenBoltLeft.onClick.AddListener(() => SendIntention(new Intention
                {
                    playerId = _playerId,
                    intentType = "use_tool",
                    targetObject = "bolt_caliper_front_left_01",
                    tool = "wrench",
                    action = "loosen"
                }));
            }

            if (_btnRemoveCaliperLeft != null)
            {
                _btnRemoveCaliperLeft.onClick.AddListener(() => SendIntention(new Intention
                {
                    playerId = _playerId,
                    intentType = "use_tool",
                    targetObject = "caliper_front_left",
                    action = "remove"
                }));
            }

            if (_btnRemoveBrakePadLeft != null)
            {
                _btnRemoveBrakePadLeft.onClick.AddListener(() => SendIntention(new Intention
                {
                    playerId = _playerId,
                    intentType = "use_tool",
                    targetObject = "brake_pad_front_left",
                    action = "remove"
                }));
            }

            if (_btnInstallBrakePadLeft != null)
            {
                _btnInstallBrakePadLeft.onClick.AddListener(() => SendIntention(new Intention
                {
                    playerId = _playerId,
                    intentType = "use_tool",
                    targetObject = "brake_pad_front_left",
                    action = "install"
                }));
            }

            if (_btnInstallCaliperLeft != null)
            {
                _btnInstallCaliperLeft.onClick.AddListener(() => SendIntention(new Intention
                {
                    playerId = _playerId,
                    intentType = "use_tool",
                    targetObject = "caliper_front_left",
                    action = "install"
                }));
            }

            if (_btnTightenBoltLeft != null)
            {
                _btnTightenBoltLeft.onClick.AddListener(() => SendIntention(new Intention
                {
                    playerId = _playerId,
                    intentType = "use_tool",
                    targetObject = "bolt_caliper_front_left_01",
                    tool = "wrench",
                    action = "tighten"
                }));
            }

            if (_btnLoosenBoltRight != null)
            {
                _btnLoosenBoltRight.onClick.AddListener(() => SendIntention(new Intention
                {
                    playerId = _playerId,
                    intentType = "use_tool",
                    targetObject = "bolt_caliper_front_right_01",
                    tool = "wrench",
                    action = "loosen"
                }));
            }

            if (_btnRemoveCaliperRight != null)
            {
                _btnRemoveCaliperRight.onClick.AddListener(() => SendIntention(new Intention
                {
                    playerId = _playerId,
                    intentType = "use_tool",
                    targetObject = "caliper_front_right",
                    action = "remove"
                }));
            }

            if (_btnRemovePadRight != null)
            {
                _btnRemovePadRight.onClick.AddListener(() => SendIntention(new Intention
                {
                    playerId = _playerId,
                    intentType = "use_tool",
                    targetObject = "pad_front_right",
                    action = "remove"
                }));
            }

            if (_btnInstallPadRight != null)
            {
                _btnInstallPadRight.onClick.AddListener(() => SendIntention(new Intention
                {
                    playerId = _playerId,
                    intentType = "use_tool",
                    targetObject = "pad_front_right",
                    action = "install"
                }));
            }

            if (_btnInstallCaliperRight != null)
            {
                _btnInstallCaliperRight.onClick.AddListener(() => SendIntention(new Intention
                {
                    playerId = _playerId,
                    intentType = "use_tool",
                    targetObject = "caliper_front_right",
                    action = "install"
                }));
            }

            if (_btnTightenBoltRight != null)
            {
                _btnTightenBoltRight.onClick.AddListener(() => SendIntention(new Intention
                {
                    playerId = _playerId,
                    intentType = "use_tool",
                    targetObject = "bolt_caliper_front_right_01",
                    tool = "wrench",
                    action = "tighten"
                }));
            }

            if (_btnFinalDiagnostic != null)
            {
                _btnFinalDiagnostic.onClick.AddListener(() => SendIntention(new Intention
                {
                    playerId = _playerId,
                    intentType = "scan",
                    targetObject = "brakes"
                }));
            }

            if (_btnEndSession != null)
            {
                _btnEndSession.onClick.AddListener(NetworkMessageSender.SendEndSession);
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

        private void SendIntention(Intention intent)
        {
            Debug.Log($"[DebugClientUI] SendIntention called: type={intent.intentType}, target={intent.targetObject}");
            NetworkMessageSender.SendIntention(intent);
        }
    }
}
