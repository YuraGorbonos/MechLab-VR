using SkillForge.Core;

namespace SkillForge.Validation
{
    public class ValidationService : IValidationService
    {
        public ValidationResult Validate(Intention intent, ScenarioStep currentStep, IWorkContext workContext)
        {
            // TODO: implement full validation logic
            // 1. Check if currentStep allows this action
            // 2. Check if required tools match
            // 3. Check workContext-specific conditions
            return ValidationResult.Success;
        }
    }
}
