using System;
using SkillForge.Core;
using UnityEngine;

namespace SkillForge.WorkContexts
{
    public class CarWorkContext : IWorkContext
    {
        private CarState _state;

        public event Action OnWorkStateChanged;

        public CarWorkContext()
        {
            _state = new CarState
            {
                engineRunning = false,
                oilLevel = 1.0f,
                brakeFluidLevel = 1.0f,
                timingBeltOk = true,
                batteryCharged = true
            };
        }

        public ValidationResult ValidateAction(Intention intent, ScenarioStep currentStep)
        {
            // TODO: implement actual validation logic based on currentStep and CarState
            return ValidationResult.Success;
        }

        public void ApplyAction(Intention intent)
        {
            // TODO: implement actual state changes based on intent
            switch (intent.action)
            {
                case "remove":
                    Debug.Log($"[CarWorkContext] Removed {intent.targetObject}");
                    break;
                case "install":
                    Debug.Log($"[CarWorkContext] Installed {intent.targetObject}");
                    break;
                case "scan":
                    Debug.Log($"[CarWorkContext] Scanned {intent.targetObject}");
                    break;
                default:
                    Debug.Log($"[CarWorkContext] Applied {intent.action} on {intent.targetObject}");
                    break;
            }

            OnWorkStateChanged?.Invoke();
        }

        public bool CheckCondition(StepCondition condition)
        {
            if (condition == null)
                return true;

            return condition.IsMet(this);
        }

        public bool IsWorkCompleted()
        {
            // TODO: implement completion check based on scenario requirements
            return false;
        }

        public DiagnosticResult GetDiagnosticStatus(string subsystem)
        {
            // TODO: implement actual diagnostic logic
            return new DiagnosticResult(subsystem, true, "No issues detected (stub)");
        }

        public CarState GetState()
        {
            return _state;
        }
    }
}
