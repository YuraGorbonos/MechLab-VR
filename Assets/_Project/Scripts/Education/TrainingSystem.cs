using System;
using System.Collections.Generic;
using SkillForge.Core;

namespace SkillForge.Education
{
    public class TrainingSystem : IEducationModule
    {
        private ScenarioConfig _config;
        private int _currentStepIndex;
        private readonly HashSet<string> _completedSteps;
        private readonly List<ActionLogEntry> _actionLog;
        private readonly float _startTime;

        public event Action<ScenarioStep> OnStepChanged;

        public TrainingSystem()
        {
            _completedSteps = new HashSet<string>();
            _actionLog = new List<ActionLogEntry>();
            _currentStepIndex = 0;
            _startTime = UnityEngine.Time.time;
        }

        public void Initialize(ScenarioConfig config)
        {
            _config = config;
            _currentStepIndex = 0;
            _completedSteps.Clear();
            _actionLog.Clear();
            OnStepChanged?.Invoke(GetCurrentStep());
        }

        public ScenarioStep GetCurrentStep()
        {
            if (_config == null || _config.steps == null || _config.steps.Length == 0)
                return null;

            if (_currentStepIndex >= _config.steps.Length)
                return null;

            return _config.steps[_currentStepIndex];
        }

        public bool IsStepCompleted(string stepId)
        {
            return _completedSteps.Contains(stepId);
        }

        public void OnActionLogged(Intention intent, bool success, string errorCode)
        {
            _actionLog.Add(new ActionLogEntry
            {
                timestamp = UnityEngine.Time.time - _startTime,
                playerId = intent.playerId,
                intentType = intent.intentType,
                targetObject = intent.targetObject,
                success = success,
                errorCode = errorCode
            });

            if (success)
            {
                var currentStep = GetCurrentStep();
                if (currentStep != null)
                {
                    _completedSteps.Add(currentStep.stepId);
                }
            }
        }

        public void AdvanceStep()
        {
            if (_config == null || _config.steps == null)
                return;

            _currentStepIndex++;

            if (_currentStepIndex < _config.steps.Length)
            {
                OnStepChanged?.Invoke(GetCurrentStep());
            }
        }

        public bool AllStepsCompleted()
        {
            if (_config == null || _config.steps == null)
                return false;

            foreach (var step in _config.steps)
            {
                if (!step.isOptional && !_completedSteps.Contains(step.stepId))
                    return false;
            }

            return true;
        }

        public SessionReport GenerateReport()
        {
            // TODO: integrate with ReportGenerator for full report
            var report = new SessionReport
            {
                scenarioId = _config?.scenarioId ?? string.Empty,
                mode = _config?.mode ?? SessionMode.Training,
                durationSeconds = UnityEngine.Time.time - _startTime,
                totalSteps = _config?.steps?.Length ?? 0,
                completedSteps = _completedSteps.Count,
                skippedSteps = 0,
                actionLog = new List<ActionLogEntry>(_actionLog),
                score = 0f,
                grade = "N/A"
            };

            return report;
        }
    }
}
