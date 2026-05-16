using System;

namespace SkillForge.Core
{
    public interface IEducationModule
    {
        void Initialize(ScenarioConfig config);
        ScenarioStep GetCurrentStep();
        bool IsStepCompleted(string stepId);
        void OnActionLogged(Intention intent, bool success, string errorCode);
        void AdvanceStep();
        bool AllStepsCompleted();
        SessionReport GenerateReport();

        event Action<ScenarioStep> OnStepChanged;
    }
}
