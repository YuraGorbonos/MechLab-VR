using System.Linq;
using SkillForge.Core;

namespace SkillForge.Validation
{
    public class ValidationService : IValidationService
    {
        public ValidationResult Validate(Intention intent, ScenarioStep currentStep, IWorkContext workContext)
        {
            if (currentStep == null)
                return ValidationResult.StepNotActive;

            if (!string.IsNullOrEmpty(intent.tool) &&
                currentStep.requiredToolIds != null &&
                currentStep.requiredToolIds.Length > 0 &&
                !currentStep.requiredToolIds.Contains(intent.tool))
            {
                return ValidationResult.InvalidTool;
            }

            if (!string.IsNullOrEmpty(currentStep.highlightTargetId) &&
                !string.IsNullOrEmpty(intent.targetObject) &&
                intent.targetObject != currentStep.highlightTargetId)
            {
                return ValidationResult.InvalidTarget;
            }

            return ValidationResult.Success;
        }
    }
}
