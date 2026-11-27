using System.Linq;
using SkillForge.Core;

namespace SkillForge.Validation
{
    public class ValidationService : IValidationService
    {
        public ValidationResult Validate(Intention intent, ScenarioStep currentStep, IWorkContext workContext)
        {
            if (currentStep == null)
            {
                return ValidationResult.StepNotActive;
            }

            if (currentStep.requiredToolIds != null &&
                currentStep.requiredToolIds.Length > 0 &&
                (string.IsNullOrEmpty(intent.tool) || !currentStep.requiredToolIds.Contains(intent.tool)))
            {
                return ValidationResult.InvalidTool;
            }

            if (!string.IsNullOrEmpty(currentStep.highlightTargetId) &&
                intent.targetObject != currentStep.highlightTargetId)
            {
                return ValidationResult.InvalidTarget;
            }

            if (!string.IsNullOrEmpty(currentStep.action) && intent.action != currentStep.action)
            {
                return ValidationResult.InvalidAction;
            }

            return ValidationResult.Success;
        }
    }
}