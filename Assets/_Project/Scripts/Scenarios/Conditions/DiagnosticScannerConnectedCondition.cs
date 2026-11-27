using SkillForge.Core;
using UnityEngine;

namespace SkillForge.Scenarios.Conditions
{
    [CreateAssetMenu(menuName = "SkillForge/Conditions/Scanner Connected")]
    public class DiagnosticScannerConnectedCondition : StepCondition
    {
        public string targetId;

        public override bool IsMet(IWorkContext context)
        {
            return !string.IsNullOrEmpty(targetId) &&
                   context is WorkContexts.CarWorkContext carContext &&
                   carContext.WasSubsystemDiagnosed(targetId);
        }
    }
}