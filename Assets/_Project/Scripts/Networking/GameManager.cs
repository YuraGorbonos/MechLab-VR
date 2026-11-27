using System;
using Mirror;
using SkillForge.Core;
using SkillForge.Education;
using SkillForge.UI;
using SkillForge.WorkContexts;
using UnityEngine;
using Zenject;

namespace SkillForge.Networking
{
    public class GameManager : NetworkBehaviour
    {
        public int CurrentStepIndex => _currentStepIndex;

        public string CurrentInstruction => _currentInstruction;

        public bool IsSessionActive => _isSessionActive;

        public static event Action<bool> OnActiveSessionChanged;

        public event Action<CarState> OnCarStateChanged;

        public static event Action<string> OnErrorReceived;

        public static event Action<string> OnReportGenerated;

        [Inject]
        private IEducationModule _educationModule;

        [Inject]
        private IWorkContext _workContext;

        [Inject]
        private IValidationService _validationService;

        [Inject]
        private IPlayerManager _playerManager;

        [Inject]
        private IHighlightService _highlightService;

        [Inject]
        private IReportGenerator _reportGenerator;

        [Inject]
        private IActionLogger _actionLogger;

        [SyncVar]
        private int _currentStepIndex;

        [SyncVar]
        private string _currentInstruction;

        [SyncVar]
        private bool _isSessionActive;

        [SerializeField] private ScenarioProvider _scenarioProvider;

        private bool _sessionEnded;

        [SyncVar(hook = nameof(OnCarStateChangedHook))]
        private string _carStateJson;

        public override void OnStartServer()
        {
            base.OnStartServer();

            _workContext.OnWorkStateChanged += OnWorkContextStateChanged;

            if (_educationModule is TrainingSystem trainingSystem)
            {
                trainingSystem.SetActionLogger(_actionLogger);
            }

            Debug.Log("[GameManager] Server started, services initialized.");
        }

        public override void OnStopServer()
        {
            _workContext.OnWorkStateChanged -= OnWorkContextStateChanged;
            base.OnStopServer();
        }

        [Server]
        public void StartSession(SessionConfig config)
        {
            var scenario = _scenarioProvider != null ? _scenarioProvider.GetScenario(config.scenarioId) : null;
            if (scenario == null || scenario.steps == null || scenario.steps.Length == 0 ||
                Array.Exists(scenario.steps, step => step == null || string.IsNullOrEmpty(step.action)))
            {
                Debug.LogError($"[GameManager] Scenario unavailable: {config.scenarioId}");
                return;
            }

            _sessionEnded = false;
            _workContext.ResetState();

            var scenarioConfig = new ScenarioConfig
                                 {
                                     scenarioId = config.scenarioId,
                                     mode = config.mode,
                                     steps = scenario.steps
                                 };

            _educationModule.Initialize(scenarioConfig);
            _isSessionActive = true;
            SendSessionState(true);
            _currentStepIndex = 0;

            var currentStep = _educationModule.GetCurrentStep();
            _currentInstruction = currentStep?.instruction ?? string.Empty;

            _carStateJson = _workContext.SerializeState();

            SendUIUpdate(currentStep);

            Debug.Log($"[GameManager] Session started: {config.scenarioId}");
        }

        [Server]
        public void ProcessIntention(NetworkConnectionToClient connection, Intention intent)
        {
            Debug.Log($"[GameManager] ProcessIntention: type={intent.intentType}, target={intent.targetObject}, playerId={intent.playerId}");

            if (!_isSessionActive)
            {
                Debug.LogWarning("[GameManager] No active session.");
                return;
            }

            var currentStep = _educationModule.GetCurrentStep();

            var validationResult = _validationService.Validate(intent, currentStep, _workContext);

            if (validationResult != ValidationResult.Success)
            {
                _educationModule.OnActionLogged(intent, false, validationResult.ToString());
                SendErrorToClient(connection, validationResult.ToString());
                return;
            }

            _workContext.ApplyAction(intent);
            _educationModule.OnActionLogged(intent, true, null);

            _carStateJson = _workContext.SerializeState();

            if (ShouldAdvanceStep())
            {
                _educationModule.MarkCurrentStepCompleted();
                _educationModule.AdvanceStep();
                _currentStepIndex++;

                var nextStep = _educationModule.GetCurrentStep();
                _currentInstruction = nextStep?.instruction ?? string.Empty;

                SendUIUpdate(nextStep);

                if (_educationModule.AllStepsCompleted())
                {
                    EndSession();
                }
            }
        }

        [Server]
        public void RequestEndSession(NetworkConnectionToClient sender = null)
        {
            if (!_isSessionActive)
            {
                Debug.LogWarning("[GameManager] RequestEndSession: no active session.");
                return;
            }

            EndSession();
        }

        public CarState GetCarState()
        {
            return ((CarWorkContext)_workContext).GetState();
        }

        private bool ShouldAdvanceStep()
        {
            var currentStep = _educationModule.GetCurrentStep();

            if (currentStep == null || currentStep.conditions == null)
            {
                return true;
            }

            foreach (var condition in currentStep.conditions)
            {
                if (!_workContext.CheckCondition(condition))
                {
                    return false;
                }
            }

            return true;
        }

        [Server]
        private void EndSession()
        {
            if (_sessionEnded)
            {
                return;
            }

            _sessionEnded = true;
            _isSessionActive = false;
            SendSessionState(false);
            var report = _educationModule.GenerateReport();
            var filePath = _reportGenerator.Generate(report);
            Debug.Log($"[GameManager] Session ended. Score: {report.score:F1}% | Grade: {report.grade}");
            SendReport(filePath);
        }

        private void SendSessionState(bool active)
        {
            var msg = new GameNetworkMessage { type = "session_state", payload = active ? "1" : "0" };
            NetworkServer.SendToAll(msg);
        }

        private void SendErrorToClient(NetworkConnectionToClient target, string message)
        {
            Debug.LogWarning($"[GameManager] Error for player: {message}");
            OnErrorReceived?.Invoke(message);
            var msg = new GameNetworkMessage { type = "error", payload = message };
            target.Send(msg);
        }

        private void SendErrorToAll(string message)
        {
            Debug.LogWarning($"[GameManager] Broadcast error: {message}");
            OnErrorReceived?.Invoke(message);
            var msg = new GameNetworkMessage { type = "error", payload = message };
            NetworkServer.SendToAll(msg);
        }

        private void SendReport(string filePath)
        {
            Debug.Log($"[GameManager] Report generated: {filePath}");
            OnReportGenerated?.Invoke(filePath);
            var msg = new GameNetworkMessage { type = "report", payload = filePath };
            NetworkServer.SendToAll(msg);
        }

        private void SendUIUpdate(ScenarioStep step)
        {
            NetworkServer.SendToAll(new UpdateUIMessage
            {
                instruction = _currentInstruction,
                stepIndex = _currentStepIndex,
                input = step != null ? step.input : default,
                hint = step != null ? step.hint : null,
                buttonLabel = step != null ? step.buttonLabel : null,
                intention = step != null ? step.CreateIntention() : default,
                removedTargetIds = _workContext.GetRemovedTargetIds()
            });
        }

        private void OnWorkContextStateChanged()
        {
            if (isServer)
            {
                _carStateJson = _workContext.SerializeState();
            }
        }

        private void OnCarStateChangedHook(string oldValue, string newValue)
        {
            if (!isServer)
            {
                _workContext.DeserializeState(newValue);
            }
            OnCarStateChanged?.Invoke(((CarWorkContext)_workContext).GetState());
        }
    }
}