using System;
using System.Collections.Generic;
using System.Linq;
using SkillForge.Core;
using UnityEngine;

namespace SkillForge.Education
{
    public class TrainingSystem : IEducationModule
    {
        public ScenarioData DefaultScenario;

        private ScenarioConfig _config;
        private int _currentStepIndex;
        private readonly HashSet<string> _completedSteps;
        private readonly List<ActionLogEntry> _actionLog;
        private float _startTime;
        private IActionLogger _actionLogger;

        public event Action<ScenarioStep> OnStepChanged;

        public TrainingSystem()
        {
            _completedSteps = new HashSet<string>();
            _actionLog = new List<ActionLogEntry>();
            _currentStepIndex = 0;
            _startTime = Time.time;
        }

        public void SetActionLogger(IActionLogger logger)
        {
            _actionLogger = logger;
        }

        public void Initialize(ScenarioConfig config)
        {
            if ((config.steps == null || config.steps.Length == 0))
            {
                ScenarioData data = DefaultScenario;

                if (data == null && !string.IsNullOrEmpty(config.scenarioId))
                    data = Resources.Load<ScenarioData>($"Scenarios/Data/{config.scenarioId}");

                if (data != null)
                {
                    config.steps = data.steps;
                    if (string.IsNullOrEmpty(config.scenarioId))
                        config.scenarioId = data.name;
                }
            }

            _config = config;
            _currentStepIndex = 0;
            _completedSteps.Clear();
            _actionLog.Clear();
            _startTime = Time.time;
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
            var currentStep = GetCurrentStep();
            var logEntry = new ActionLogEntry
            {
                timestamp = Time.time - _startTime,
                playerId = intent.playerId,
                intentType = intent.intentType,
                targetObject = intent.targetObject,
                stepId = currentStep?.stepId,
                success = success,
                errorCode = errorCode
            };

            _actionLog.Add(logEntry);
            _actionLogger?.Log(intent, success, errorCode);

            if (success && currentStep != null)
            {
                _completedSteps.Add(currentStep.stepId);
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
            var errorCount = _actionLog.Count(e => !e.success);
            var rawScore = 100f - errorCount;
            var score = Mathf.Max(0f, rawScore);
            var grade = score >= 90f ? "A" : score >= 75f ? "B" : score >= 60f ? "C" : score >= 40f ? "D" : "F";

            var stepResults = new List<StepResult>();
            if (_config?.steps != null)
            {
                foreach (var step in _config.steps)
                {
                    var completed = _completedSteps.Contains(step.stepId);
                    var stepErrors = _actionLog.Count(e => e.stepId == step.stepId && !e.success);
                    stepResults.Add(new StepResult
                    {
                        stepId = step.stepId,
                        instruction = step.instruction,
                        completed = completed,
                        errorCount = stepErrors
                    });
                }
            }

            var report = new SessionReport
            {
                scenarioId = _config?.scenarioId ?? string.Empty,
                mode = _config?.mode ?? SessionMode.Training,
                durationSeconds = Time.time - _startTime,
                totalSteps = _config?.steps?.Length ?? 0,
                completedSteps = _completedSteps.Count,
                skippedSteps = 0,
                actionLog = new List<ActionLogEntry>(_actionLog),
                stepResults = stepResults,
                score = score,
                grade = grade
            };

            return report;
        }
    }
}
