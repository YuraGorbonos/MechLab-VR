using SkillForge.Core;
using UnityEngine;

namespace SkillForge.Scenarios.Conditions
{
    [CreateAssetMenu(menuName = "SkillForge/Conditions/Scanner Connected")]
    public class DiagnosticScannerConnectedCondition : StepCondition
    {
        public override bool IsMet(IWorkContext context)
        {
            return true; // TODO: implement actual scanner connection check
        }
    }
}