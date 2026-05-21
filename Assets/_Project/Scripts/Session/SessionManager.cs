using SkillForge.Core;
using UnityEngine;

namespace SkillForge.Session
{
    public class SessionManager
    {
        public bool IsSessionActive => _isSessionActive;

        private IEducationModule _educationModule;
        private IWorkContext _workContext;
        private IValidationService _validationService;
        private IPlayerManager _playerManager;
        private IHighlightService _highlightService;
        private IDiagnosticService _diagnosticService;

        private SessionConfig _sessionConfig;
        private bool _isSessionActive;

        public SessionManager(
            IEducationModule educationModule,
            IWorkContext workContext,
            IValidationService validationService,
            IPlayerManager playerManager,
            IHighlightService highlightService,
            IDiagnosticService diagnosticService)
        {
            _educationModule = educationModule;
            _workContext = workContext;
            _validationService = validationService;
            _playerManager = playerManager;
            _highlightService = highlightService;
            _diagnosticService = diagnosticService;
        }

        public void StartSession(SessionConfig config)
        {
            _sessionConfig = config;
            _isSessionActive = true;

            _educationModule.Initialize(new ScenarioConfig
                                        {
                                            scenarioId = config.scenarioId,
                                            mode = config.mode,
                                            steps = null
                                        });

            Debug.Log($"[SessionManager] Session started: {config.scenarioId} | Mode: {config.mode} | Players: {config.playerCount}");
        }

        public void EndSession()
        {
            if (!_isSessionActive)
            {
                return;
            }

            var report = _educationModule.GenerateReport();
            _isSessionActive = false;

            Debug.Log($"[SessionManager] Session ended. Score: {report.score}% | Grade: {report.grade}");
        }

        public ValidationResult ProcessIntention(Intention intent)
        {
            if (!_isSessionActive)
            {
                Debug.LogWarning("[SessionManager] No active session.");
                return ValidationResult.StepNotActive;
            }

            var currentStep = _educationModule.GetCurrentStep();

            var validationResult = _validationService.Validate(intent, currentStep, _workContext);

            if (validationResult != ValidationResult.Success)
            {
                _educationModule.OnActionLogged(intent, false, validationResult.ToString());
                return validationResult;
            }

            _workContext.ApplyAction(intent);
            _educationModule.OnActionLogged(intent, true, null);

            if (_educationModule.AllStepsCompleted())
            {
                EndSession();
            }

            return ValidationResult.Success;
        }

        public IEducationModule GetEducationModule()
        {
            return _educationModule;
        }

        public IWorkContext GetWorkContext()
        {
            return _workContext;
        }

        public IPlayerManager GetPlayerManager()
        {
            return _playerManager;
        }

        public IHighlightService GetHighlightService()
        {
            return _highlightService;
        }

        public IDiagnosticService GetDiagnosticService()
        {
            return _diagnosticService;
        }
    }
}