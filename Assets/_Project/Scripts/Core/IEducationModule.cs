using System;

namespace SkillForge.Core
{
    public interface IEducationModule
    {
        event Action<ScenarioStep> OnStepChanged;

        void Initialize(ScenarioConfig config);

        ScenarioStep GetCurrentStep();

        bool IsStepCompleted(string stepId);

        void OnActionLogged(Intention intent, bool success, string errorCode);

        void MarkCurrentStepCompleted();

        void AdvanceStep();

        bool AllStepsCompleted();

        SessionReport GenerateReport();
    }
}