using System;

namespace SkillForge.Core
{
    public interface IWorkContext
    {
        ValidationResult ValidateAction(Intention intent, ScenarioStep currentStep);
        void ApplyAction(Intention intent);
        bool CheckCondition(StepCondition condition);
        bool IsWorkCompleted();
        DiagnosticResult GetDiagnosticStatus(string subsystem);

        event Action OnWorkStateChanged;
    }
}
