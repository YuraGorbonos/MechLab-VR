using System;
using System.Collections.Generic;
using Mirror;
using SkillForge.Core;
using SkillForge.Education;
using SkillForge.WorkContexts;
using SkillForge.Validation;
using SkillForge.Players;
using SkillForge.Highlight;
using UnityEngine;
using Zenject;

namespace SkillForge.Networking
{
    public class GameManager : NetworkBehaviour
    {
        [Inject] private IEducationModule _educationModule;
        [Inject] private IWorkContext _workContext;
        [Inject] private IValidationService _validationService;
        [Inject] private IPlayerManager _playerManager;
        [Inject] private IHighlightService _highlightService;

        [SyncVar]
        private int _currentStepIndex;

        [SyncVar]
        private string _currentInstruction;

        [SyncVar]
        private bool _isSessionActive;

        [SyncVar(hook = nameof(OnCarStateChangedHook))]
        private string _carStateJson;

        public event Action<CarState> OnCarStateChanged;

        private readonly Dictionary<int, NetworkConnectionToClient> _playerConnections;

        public int CurrentStepIndex => _currentStepIndex;
        public string CurrentInstruction => _currentInstruction;
        public bool IsSessionActive => _isSessionActive;

        public GameManager()
        {
            _playerConnections = new Dictionary<int, NetworkConnectionToClient>();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            _workContext.OnWorkStateChanged += OnWorkContextStateChanged;

            if (_educationModule is TrainingSystem trainingSystem)
            {
                var actionLogger = new ActionLogger();
                trainingSystem.SetActionLogger(actionLogger);
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
            var scenarioConfig = new ScenarioConfig
            {
                scenarioId = config.scenarioId,
                mode = config.mode,
                steps = null
            };

            _educationModule.Initialize(scenarioConfig);
            _isSessionActive = true;
            _currentStepIndex = 0;

            var currentStep = _educationModule.GetCurrentStep();
            _currentInstruction = currentStep?.instruction ?? string.Empty;

            _carStateJson = _workContext.SerializeState();

            Debug.Log($"[GameManager] Session started: {config.scenarioId}");
        }

        [Server]
        public void ProcessIntention(Intention intent)
        {
            if (!_isSessionActive)
            {
                Debug.LogWarning("[GameManager] No active session.");
                return;
            }

            if (_playerConnections.TryGetValue(intent.playerId, out var connection))
            {
                var currentStep = _educationModule.GetCurrentStep();

                var validationResult = _validationService.Validate(intent, currentStep, _workContext);

                if (validationResult != ValidationResult.Success)
                {
                    _educationModule.OnActionLogged(intent, false, validationResult.ToString());
                    TargetRpcShowError(connection, validationResult.ToString());
                    return;
                }

                _workContext.ApplyAction(intent);
                _educationModule.OnActionLogged(intent, true, null);

                _carStateJson = _workContext.SerializeState();

                if (ShouldAdvanceStep())
                {
                    _educationModule.AdvanceStep();
                    _currentStepIndex++;

                    var nextStep = _educationModule.GetCurrentStep();
                    _currentInstruction = nextStep?.instruction ?? string.Empty;

                    if (_educationModule.AllStepsCompleted())
                    {
                        EndSession();
                    }
                }
            }
        }

        [Server]
        public void RegisterPlayerConnection(int playerId, NetworkConnectionToClient connection)
        {
            _playerConnections[playerId] = connection;
        }

        [Server]
        public void RemovePlayerConnection(int playerId)
        {
            _playerConnections.Remove(playerId);
        }

        private bool ShouldAdvanceStep()
        {
            var currentStep = _educationModule.GetCurrentStep();
            if (currentStep == null || currentStep.conditions == null)
                return true;

            foreach (var condition in currentStep.conditions)
            {
                if (!_workContext.CheckCondition(condition))
                    return false;
            }

            return true;
        }

        [Server]
        private void EndSession()
        {
            _isSessionActive = false;
            var report = _educationModule.GenerateReport();
            Debug.Log($"[GameManager] Session ended. Score: {report.score:F1}% | Grade: {report.grade}");
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
            _workContext.DeserializeState(newValue);
            OnCarStateChanged?.Invoke(((CarWorkContext)_workContext).GetState());
        }

        [TargetRpc]
        private void TargetRpcShowError(NetworkConnectionToClient target, string message)
        {
            // TODO: show error UI on client
            Debug.LogWarning($"[GameManager] Error for player: {message}");
        }

        public CarState GetCarState()
        {
            return ((CarWorkContext)_workContext).GetState();
        }
    }
}
