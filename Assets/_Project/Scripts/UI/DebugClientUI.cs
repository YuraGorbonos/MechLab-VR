using System.Collections;
using System.Collections.Generic;
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
        [Header("UI Elements")]
        [SerializeField] private GameObject _actionPanel; // TODO: assign in Inspector
        [SerializeField] private TMP_Text _statusText; // TODO: assign in Inspector
        [SerializeField] private TMP_Text _errorText; // TODO: assign in Inspector
        [SerializeField] private Button _connectButton; // TODO: assign in Inspector

        [SerializeField] private Button _btnScanDiagPort; // TODO: assign in Inspector
        [SerializeField] private Button _btnScanBrakes; // TODO: assign in Inspector
        [SerializeField] private Button _btnLoosenBoltLeft; // TODO: assign in Inspector
        [SerializeField] private Button _btnRemoveCaliperLeft; // TODO: assign in Inspector
        [SerializeField] private Button _btnRemoveBrakePadLeft; // TODO: assign in Inspector
        [SerializeField] private Button _btnInstallBrakePadLeft; // TODO: assign in Inspector
        [SerializeField] private Button _btnInstallCaliperLeft; // TODO: assign in Inspector
        [SerializeField] private Button _btnTightenBoltLeft; // TODO: assign in Inspector

        private IntentionSender _intentionSender;
        private int _playerId = -1;

        private void Start()
        {
            if (_connectButton != null)
                _connectButton.onClick.AddListener(OnConnectClicked);

            SetupActionButtons();

            if (_actionPanel != null)
                _actionPanel.SetActive(false);

            UpdateStatus("Not connected.");
            GameManager.OnErrorReceived += OnServerErrorReceived;
        }

        private void OnDestroy()
        {
            GameManager.OnErrorReceived -= OnServerErrorReceived;
        }

        private void OnServerErrorReceived(string message)
        {
            ShowError(message);
        }

        private void SetupActionButtons()
        {
            if (_btnScanDiagPort != null)
                _btnScanDiagPort.onClick.AddListener(() => SendIntention(new Intention
                {
                    playerId = _playerId,
                    intentType = "scan",
                    targetObject = "diag_port"
                }));

            if (_btnScanBrakes != null)
                _btnScanBrakes.onClick.AddListener(() => SendIntention(new Intention
                {
                    playerId = _playerId,
                    intentType = "scan",
                    targetObject = "brakes"
                }));

            if (_btnLoosenBoltLeft != null)
                _btnLoosenBoltLeft.onClick.AddListener(() => SendIntention(new Intention
                {
                    playerId = _playerId,
                    intentType = "use_tool",
                    targetObject = "bolt_caliper_front_left_01",
                    tool = "wrench",
                    action = "loosen"
                }));

            if (_btnRemoveCaliperLeft != null)
                _btnRemoveCaliperLeft.onClick.AddListener(() => SendIntention(new Intention
                {
                    playerId = _playerId,
                    intentType = "use_tool",
                    targetObject = "caliper_front_left",
                    action = "remove"
                }));

            if (_btnRemoveBrakePadLeft != null)
                _btnRemoveBrakePadLeft.onClick.AddListener(() => SendIntention(new Intention
                {
                    playerId = _playerId,
                    intentType = "use_tool",
                    targetObject = "brake_pad_front_left",
                    action = "remove"
                }));

            if (_btnInstallBrakePadLeft != null)
                _btnInstallBrakePadLeft.onClick.AddListener(() => SendIntention(new Intention
                {
                    playerId = _playerId,
                    intentType = "use_tool",
                    targetObject = "brake_pad_front_left",
                    action = "install"
                }));

            if (_btnInstallCaliperLeft != null)
                _btnInstallCaliperLeft.onClick.AddListener(() => SendIntention(new Intention
                {
                    playerId = _playerId,
                    intentType = "use_tool",
                    targetObject = "caliper_front_left",
                    action = "install"
                }));

            if (_btnTightenBoltLeft != null)
                _btnTightenBoltLeft.onClick.AddListener(() => SendIntention(new Intention
                {
                    playerId = _playerId,
                    intentType = "use_tool",
                    targetObject = "bolt_caliper_front_left_01",
                    tool = "wrench",
                    action = "tighten"
                }));
        }

        private void OnConnectClicked()
        {
            var networkManager = NetworkManager.singleton;
            if (networkManager == null)
            {
                ShowError("NetworkManager not found.");
                return;
            }

            if (!NetworkClient.isConnected)
            {
                Debug.Log($"[DebugClientUI] Starting client, connecting to {networkManager.networkAddress}:{networkManager.GetComponent<Transport>()?.GetType().Name}");
                networkManager.StartClient();
                UpdateStatus("Connecting...");
                StartCoroutine(CheckConnectionTimeout());
            }
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

        private void Update()
        {
            if (_playerId < 0 && NetworkClient.isConnected && NetworkClient.localPlayer != null)
            {
                _playerId = (int)NetworkClient.localPlayer.netId;
                _intentionSender = NetworkClient.localPlayer.GetComponent<IntentionSender>();
                SetPlayerId(_playerId);
            }
        }

        private void SendIntention(Intention intent)
        {
            if (_intentionSender == null)
            {
                ShowError("IntentionSender not found.");
                return;
            }

            if (!_intentionSender.isOwned)
            {
                ShowError("IntentionSender not owned by this client.");
                return;
            }

            _intentionSender.CmdSendIntention(intent);
        }

        public void UpdateStatus(string message)
        {
            if (_statusText != null)
                _statusText.text = message;
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

            if (_actionPanel != null)
                _actionPanel.SetActive(true);
        }
    }
}

