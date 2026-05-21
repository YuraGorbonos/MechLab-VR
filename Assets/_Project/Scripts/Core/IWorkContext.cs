using System;

namespace SkillForge.Core
{
    public interface IWorkContext
    {
        event Action OnWorkStateChanged;

        ValidationResult ValidateAction(Intention intent, ScenarioStep currentStep);

        void ApplyAction(Intention intent);

        bool CheckCondition(StepCondition condition);

        bool IsWorkCompleted();

        DiagnosticResult GetDiagnosticStatus(string subsystem);

        PartState GetPartState(string partId);

        string SerializeState();

        void DeserializeState(string json);
    }
}