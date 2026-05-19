namespace SkillForge.Core
{
    public interface IValidationService
    {
        ValidationResult Validate(Intention intent, ScenarioStep currentStep, IWorkContext workContext);
    }
}
