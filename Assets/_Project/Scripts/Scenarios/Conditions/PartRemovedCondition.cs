using SkillForge.Core;
using UnityEngine;

namespace SkillForge.Scenarios.Conditions
{
    [CreateAssetMenu(menuName = "SkillForge/Conditions/Part Removed")]
    public class PartRemovedCondition : StepCondition
    {
        public string[] partIds;

        public override bool IsMet(IWorkContext context)
        {
            if (partIds == null || partIds.Length == 0)
                return false;

            foreach (var partId in partIds)
            {
                if (context.GetPartState(partId) != PartState.Removed)
                    return false;
            }

            return true;
        }
    }
}
