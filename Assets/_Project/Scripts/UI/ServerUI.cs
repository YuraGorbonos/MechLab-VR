using System;
using System.Collections.Generic;
using kcp2k;
using Mirror;
using SkillForge.Core;
using SkillForge.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace SkillForge.UI
{
    public class ServerUI : MonoBehaviour
    {
        [Inject] private CustomNetworkManager _networkManager;

        [Header("Server Controls")]
        [SerializeField] private Button _startServerButton; // TODO: assign in Inspector
        [SerializeField] private Button _stopServerButton; // TODO: assign in Inspector

        [Header("Session Controls")]
        [SerializeField] private Button _startSessionButton; // TODO: assign in Inspector
        [SerializeField] private TMP_Dropdown _scenarioDropdown; // TODO: assign in Inspector
        [SerializeField] private TMP_Dropdown _modeDropdown; // TODO: assign in Inspector
        [SerializeField] private TMP_Dropdown _playerCountDropdown; // TODO: assign in Inspector

        [Header("Log")]
        [SerializeField] private TMP_Text _logText; // TODO: assign in Inspector

        private readonly List<string> _scenarioIds = new List<string>
        {
            "brake_pad_replacement",
            "caliper_service",
            "brake_disc_replacement",
            "brake_fluid_change"
        };

        private void Start()
        {

            if (_startServerButton != null)
                _startServerButton.onClick.AddListener(OnStartServerClicked);

            if (_stopServerButton != null)
                _stopServerButton.onClick.AddListener(OnStopServerClicked);

            if (_startSessionButton != null)
                _startSessionButton.onClick.AddListener(OnStartSessionClicked);

            PopulateScenarioDropdown();
            PopulateModeDropdown();
            PopulatePlayerCountDropdown();

            if (_networkManager != null)
            {
                _networkManager.OnPlayerJoined += OnPlayerJoined;
                _networkManager.OnPlayerLeft += OnPlayerLeft;
            }

            UpdateSessionState(false);
            AddLog("Server UI initialized.");
        }

        private void OnDestroy()
        {
            if (_networkManager != null)
            {
                _networkManager.OnPlayerJoined -= OnPlayerJoined;
                _networkManager.OnPlayerLeft -= OnPlayerLeft;
            }
        }

        private void PopulateScenarioDropdown()
        {
            if (_scenarioDropdown == null) return;

            _scenarioDropdown.ClearOptions();
            _scenarioDropdown.AddOptions(_scenarioIds);
            _scenarioDropdown.value = 0;
        }

        private void PopulatePlayerCountDropdown()
        {
            if (_playerCountDropdown == null) return;

            _playerCountDropdown.ClearOptions();
            _playerCountDropdown.AddOptions(new List<string> { "1", "2" });
            _playerCountDropdown.value = 0;
        }

        private void PopulateModeDropdown()
        {
            if (_modeDropdown == null) return;

            _modeDropdown.ClearOptions();
            _modeDropdown.AddOptions(new List<string> { "Training", "Exam" });
            _modeDropdown.value = 0;
        }

        public void OnStartServerClicked()
        {
            if (_networkManager == null)
            {
                AddLog("Error: NetworkManager not found.");
                return;
            }

            _networkManager.StartServer();

            if (NetworkServer.active)
            {
                AddLog("Server started.");
                if (Transport.active is KcpTransport kcp)
                    AddLog($"KCP transport on port {kcp.Port}");
            }
            else
            {
                AddLog("Error: Server failed to start.");
            }
        }

        public void OnStopServerClicked()
        {
            if (_networkManager == null)
            {
                AddLog("Error: NetworkManager not found.");
                return;
            }

            _networkManager.StopServer();
            AddLog("Server stopped.");
            UpdateSessionState(false);
        }

        public void OnStartSessionClicked()
        {
            if (_networkManager == null)
            {
                AddLog("Error: NetworkManager not found.");
                return;
            }

            if (!NetworkServer.active)
            {
                AddLog("Error: Server is not running.");
                return;
            }

            var config = new SessionConfig
            {
                scenarioId = _scenarioIds[_scenarioDropdown.value],
                mode = GetSelectedMode(),
                playerCount = int.Parse(_playerCountDropdown.options[_playerCountDropdown.value].text)
            };

            _networkManager.StartSession(config);
        }

        private SessionMode GetSelectedMode()
        {
            if (_modeDropdown != null && _modeDropdown.value == 1)
                return SessionMode.Exam;

            return SessionMode.Training;
        }

        public void AddLog(string message)
        {
            if (_logText == null) return;

            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            _logText.text += $"[{timestamp}] {message}\n";
        }

        public void UpdateSessionState(bool isActive)
        {
            if (_startSessionButton != null)
                _startSessionButton.interactable = !isActive;

            if (_scenarioDropdown != null)
                _scenarioDropdown.interactable = !isActive;

            if (_modeDropdown != null)
                _modeDropdown.interactable = !isActive;

            if (_playerCountDropdown != null)
                _playerCountDropdown.interactable = !isActive;
        }

        private void OnPlayerJoined(int playerId)
        {
            AddLog($"Player {playerId} joined.");
        }

        private void OnPlayerLeft(int playerId)
        {
            AddLog($"Player {playerId} left.");
        }
    }
}
