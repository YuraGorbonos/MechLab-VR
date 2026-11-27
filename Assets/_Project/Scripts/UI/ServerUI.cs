using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using kcp2k;
using Mirror;
using SkillForge.Core;
using SkillForge.Education;
using SkillForge.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

namespace SkillForge.UI
{
    public class ServerUI : MonoBehaviour
    {
        private const string ClientSceneName = "ClientScene";
        private const int VisibleLogLines = 8;

        [SerializeField] private Button _startServerButton;
        [SerializeField] private Button _stopServerButton;
        [SerializeField] private Button _returnToGarageButton;
        [SerializeField] private TMP_Dropdown _scenarioDropdown;
        [SerializeField] private TMP_Dropdown _playerCountDropdown;
        [SerializeField] private TMP_Text _windowTitle;
        [SerializeField] private TMP_Text _statusText;
        [SerializeField] private TMP_Text _stepCountText;
        [SerializeField] private TMP_Text _portText;
        [SerializeField] private TMP_Text _playerStatusText;
        [SerializeField] private TMP_Text _progressText;
        [SerializeField] private Image _progressFill;
        [SerializeField] private TMP_Text _logText;
        [SerializeField] private ScenarioProvider _scenarioProvider;
        [SerializeField] private GameManager _gameManager;
        [SerializeField] private Camera _backdropCamera;

        [Inject] private CustomNetworkManager _networkManager;

        private readonly Queue<string> _recentLog = new();
        private ScenarioData[] _scenarios;
        private Canvas _serverCanvas;
        private EventSystem _serverEventSystem;
        private bool _isLoadingClient;
        private bool _isStopping;
        private (bool host, bool inGarage, bool session, int step, int clients, int selectedScenario, int seats) _displayedState;

        private void Start()
        {
            _serverCanvas = GetComponentInParent<Canvas>();
            _serverEventSystem = EventSystem.current;
            _startServerButton.onClick.AddListener(OnStartServerClicked);
            _stopServerButton.onClick.AddListener(OnStopServerClicked);
            _returnToGarageButton.onClick.AddListener(HideOperatorPanel);

            _scenarios = _scenarioProvider != null ? _scenarioProvider.GetScenarios() : Array.Empty<ScenarioData>();
            _scenarioDropdown.ClearOptions();
            _scenarioDropdown.AddOptions(_scenarios.Select(scenario => scenario.scenarioName).ToList());
            _playerCountDropdown.ClearOptions();
            _playerCountDropdown.AddOptions(new List<string> { "1 участник", "2 участника" });
            GameManager.OnReportGenerated += OnReportGenerated;
            AddLog("Параметры занятия загружены.");
            RefreshOperatorPanel();
        }

        private void OnDestroy()
        {
            GameManager.OnReportGenerated -= OnReportGenerated;
        }

        private void Update()
        {
            if (IsGarageLoaded() && Keyboard.current?.f1Key.wasPressedThisFrame == true)
            {
                _serverCanvas.enabled = !_serverCanvas.enabled;
                if (_serverCanvas.enabled) RefreshOperatorPanel();
            }

            if (_serverCanvas.enabled && ReadState() != _displayedState)
            {
                RefreshOperatorPanel();
            }
        }

        public void OnStartServerClicked()
        {
            if (_isLoadingClient || _isStopping || IsGarageLoaded() || _scenarios.Length == 0)
            {
                return;
            }

            if (_networkManager == null)
            {
                AddLog("Не найден сетевой менеджер.");
                return;
            }

            if (!NetworkServer.active)
            {
                _networkManager.StartHost();
            }

            if (!NetworkServer.active)
            {
                AddLog("Не удалось запустить сервер.");
                return;
            }

            AddLog("Сервер запущен.");
            StartCoroutine(LoadClientAndStartSession());
        }

        private IEnumerator LoadClientAndStartSession()
        {
            _isLoadingClient = true;
            RefreshOperatorPanel();
            _serverEventSystem.enabled = false;
            _backdropCamera.enabled = false;

            yield return SceneManager.LoadSceneAsync(ClientSceneName, LoadSceneMode.Additive);
            yield return null;
            _isLoadingClient = false;

            var clientScene = SceneManager.GetSceneByName(ClientSceneName);
            if (!clientScene.isLoaded || !NetworkClient.isConnected)
            {
                if (clientScene.isLoaded)
                {
                    yield return SceneManager.UnloadSceneAsync(clientScene);
                }

                _networkManager.StopHost();
                _serverEventSystem.enabled = true;
                _backdropCamera.enabled = true;
                AddLog("Не удалось открыть гараж для локального игрока.");
                RefreshOperatorPanel();
                yield break;
            }

            AddLog("Гараж открыт.");
            OnStartSessionClicked();
        }

        public void OnStopServerClicked()
        {
            if (_isLoadingClient || _isStopping || !NetworkServer.active)
            {
                return;
            }

            StartCoroutine(StopSessionAndReturn());
        }

        private IEnumerator StopSessionAndReturn()
        {
            _isStopping = true;
            RefreshOperatorPanel();
            if (_gameManager.IsSessionActive)
            {
                _gameManager.RequestEndSession();
            }

            if (NetworkClient.active)
                _networkManager.StopHost();
            else
                _networkManager.StopServer();

            var clientScene = SceneManager.GetSceneByName(ClientSceneName);
            if (clientScene.isLoaded)
            {
                yield return SceneManager.UnloadSceneAsync(clientScene);
            }

            _serverEventSystem.enabled = true;
            _backdropCamera.enabled = true;
            _serverCanvas.enabled = true;
            _isStopping = false;
            AddLog("Сервер остановлен.");
            RefreshOperatorPanel();
        }

        public void OnStartSessionClicked()
        {
            if (_isLoadingClient || !NetworkServer.active || _scenarios.Length == 0)
            {
                return;
            }

            var scenario = _scenarios[_scenarioDropdown.value];
            _networkManager.StartSession(new SessionConfig
            {
                scenarioId = scenario.scenarioId,
                mode = SessionMode.Training,
                playerCount = _playerCountDropdown.value + 1
            });
        }

        public void UpdateSessionState(bool isActive)
        {
            if (isActive && IsGarageLoaded()) HideOperatorPanel();
            RefreshOperatorPanel();
        }

        public void AddLog(string message)
        {
            if (_recentLog.Count == VisibleLogLines) _recentLog.Dequeue();
            _recentLog.Enqueue($"[{DateTime.Now:HH:mm:ss}] {message}");
            _logText.text = string.Join("\n", _recentLog);
        }

        private void OnReportGenerated(string path)
        {
            AddLog("Отчёт занятия сохранён.");
            RefreshOperatorPanel();
        }

        private void HideOperatorPanel()
        {
            _serverCanvas.enabled = false;
        }

        private static bool IsGarageLoaded()
        {
            return SceneManager.GetSceneByName(ClientSceneName).isLoaded;
        }

        private (bool host, bool inGarage, bool session, int step, int clients, int selectedScenario, int seats) ReadState()
        {
            var host = NetworkServer.active;
            return (host, IsGarageLoaded(), host && _gameManager.IsSessionActive,
                host ? _gameManager.CurrentStepIndex : 0, host ? NetworkServer.connections.Count : 0,
                _scenarioDropdown.value, _playerCountDropdown.value + 1);
        }

        private void RefreshOperatorPanel()
        {
            var state = ReadState();
            _displayedState = state;
            var count = _scenarios.Length > 0 ? _scenarios[state.selectedScenario].steps.Length : 0;
            var canConfigure = !state.host && !_isStopping && !_isLoadingClient;

            _windowTitle.text = state.inGarage ? "Текущее занятие" : "Запуск занятия";
            _statusText.text = _isStopping ? "Остановка" : _isLoadingClient ? "Открываем гараж" :
                state.session ? "Занятие идёт" : state.inGarage ? "Занятие завершено" :
                state.host ? "Сервер запущен" : "Сервер остановлен";
            _stepCountText.text = $"Шагов: {count}";
            _portText.text = (Transport.active ?? _networkManager.transport) is KcpTransport kcp ? $"{kcp.Port} · KCP" : "—";
            _playerStatusText.text = $"{state.clients} / {state.seats}";
            _startServerButton.gameObject.SetActive(!state.host && !_isStopping);
            _startServerButton.interactable = canConfigure && count > 0;
            _stopServerButton.gameObject.SetActive(state.host || _isStopping);
            _stopServerButton.interactable = !_isLoadingClient && !_isStopping;
            _returnToGarageButton.gameObject.SetActive(state.inGarage && !_isStopping);
            _scenarioDropdown.interactable = canConfigure && _scenarios.Length > 1;
            _playerCountDropdown.interactable = canConfigure;

            _progressText.gameObject.SetActive(state.inGarage);
            _progressFill.transform.parent.gameObject.SetActive(state.inGarage);
            _progressText.text = state.session
                ? $"Шаг {state.step + 1} из {count}\n{_gameManager.CurrentInstruction}"
                : $"Выполнено шагов: {state.step} из {count}";
            var completed = count > 0 ? (float)state.step / count : 0f;
            _progressFill.rectTransform.anchorMax = new Vector2(Mathf.Clamp01(completed), 1f);
        }
    }
}
