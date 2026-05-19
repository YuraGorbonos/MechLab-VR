using System;
using System.Collections.Generic;
using SkillForge.Core;
using UnityEngine;

namespace SkillForge.WorkContexts
{
    [Serializable]
    public class CarStateData
    {
        public bool engineRunning;
        public float oilLevel;
        public float brakeFluidLevel;
        public bool timingBeltOk;
        public bool batteryCharged;
        public Dictionary<string, int> parts;
        public List<string> diagnosedSubsystems;
    }

    public class CarWorkContext : IWorkContext
    {
        private CarState _state;
        private readonly Dictionary<string, PartState> _parts = new Dictionary<string, PartState>();
        private readonly HashSet<string> _diagnosedSubsystems = new HashSet<string>();

        public event Action OnWorkStateChanged;

        public CarWorkContext()
        {
            ResetState();
        }

        private void ResetState()
        {
            _state = new CarState
            {
                engineRunning = false,
                oilLevel = 1.0f,
                brakeFluidLevel = 1.0f,
                timingBeltOk = true,
                batteryCharged = true
            };

            _parts.Clear();
            _diagnosedSubsystems.Clear();
        }

        public ValidationResult ValidateAction(Intention intent, ScenarioStep currentStep)
        {
            return ValidationResult.Success;
        }

        public void ApplyAction(Intention intent)
        {
            switch (intent.action)
            {
                case "loosen":
                    _parts[intent.targetObject] = PartState.Loosened;
                    break;

                case "remove":
                    _parts[intent.targetObject] = PartState.Removed;
                    break;

                case "install":
                    _parts[intent.targetObject] = PartState.Installed;
                    break;

                case "tighten":
                    _parts[intent.targetObject] = PartState.Default;
                    break;

                case "scan":
                    _diagnosedSubsystems.Add(intent.targetObject);
                    break;

                case "diagnose":
                    _diagnosedSubsystems.Add(intent.targetObject);
                    break;

                case "set_oil_level":
                    _state.oilLevel = 1.0f;
                    break;

                case "set_brake_fluid":
                    _state.brakeFluidLevel = 1.0f;
                    break;

                case "charge_battery":
                    _state.batteryCharged = true;
                    break;

                default:
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
            return false;
        }

        public DiagnosticResult GetDiagnosticStatus(string subsystem)
        {
            _diagnosedSubsystems.Add(subsystem);

            switch (subsystem.ToLower())
            {
                case "brakes":
                    bool brakesOk = _state.brakeFluidLevel >= 0.5f;
                    return new DiagnosticResult(subsystem, brakesOk,
                        brakesOk ? "Brake system OK" : $"Low brake fluid: {_state.brakeFluidLevel:F1}");

                case "engine":
                    bool engineOk = _state.oilLevel >= 0.3f && _state.timingBeltOk;
                    return new DiagnosticResult(subsystem, engineOk,
                        engineOk ? "Engine OK" : $"Oil: {_state.oilLevel:F1}, Timing belt: {_state.timingBeltOk}");

                case "battery":
                    return new DiagnosticResult(subsystem, _state.batteryCharged,
                        _state.batteryCharged ? "Battery charged" : "Battery discharged");

                default:
                    return new DiagnosticResult(subsystem, true, $"Unknown subsystem: {subsystem}");
            }
        }

        public PartState GetPartState(string partId)
        {
            return _parts.TryGetValue(partId, out var state) ? state : PartState.Default;
        }

        public CarState GetState()
        {
            return _state;
        }

        public bool WasSubsystemDiagnosed(string subsystem)
        {
            return _diagnosedSubsystems.Contains(subsystem);
        }

        public string SerializeState()
        {
            var data = new CarStateData
            {
                engineRunning = _state.engineRunning,
                oilLevel = _state.oilLevel,
                brakeFluidLevel = _state.brakeFluidLevel,
                timingBeltOk = _state.timingBeltOk,
                batteryCharged = _state.batteryCharged,
                parts = new Dictionary<string, int>(),
                diagnosedSubsystems = new List<string>(_diagnosedSubsystems)
            };

            foreach (var kvp in _parts)
            {
                data.parts[kvp.Key] = (int)kvp.Value;
            }

            return JsonUtility.ToJson(data);
        }

        public void DeserializeState(string json)
        {
            if (string.IsNullOrEmpty(json))
                return;

            var data = JsonUtility.FromJson<CarStateData>(json);
            if (data == null)
                return;

            _state = new CarState
            {
                engineRunning = data.engineRunning,
                oilLevel = data.oilLevel,
                brakeFluidLevel = data.brakeFluidLevel,
                timingBeltOk = data.timingBeltOk,
                batteryCharged = data.batteryCharged
            };

            _parts.Clear();
            if (data.parts != null)
            {
                foreach (var kvp in data.parts)
                {
                    _parts[kvp.Key] = (PartState)kvp.Value;
                }
            }

            _diagnosedSubsystems.Clear();
            if (data.diagnosedSubsystems != null)
            {
                foreach (var sub in data.diagnosedSubsystems)
                {
                    _diagnosedSubsystems.Add(sub);
                }
            }
        }
    }
}
