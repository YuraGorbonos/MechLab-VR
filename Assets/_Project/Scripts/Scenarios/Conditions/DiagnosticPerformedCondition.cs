using SkillForge.Core;
using UnityEngine;

namespace SkillForge.Scenarios.Conditions
{
    [CreateAssetMenu(menuName = "SkillForge/Conditions/Diagnostic Performed")]
    public class DiagnosticPerformedCondition : StepCondition
    {
        public string subsystem;

        public override bool IsMet(IWorkContext context)
        {
            if (string.IsNullOrEmpty(subsystem))
                return false;

            if (context is WorkContexts.CarWorkContext carContext)
            {
                return carContext.WasSubsystemDiagnosed(subsystem);
            }

            return false;
        }
    }
}
