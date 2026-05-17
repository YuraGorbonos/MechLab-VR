using SkillForge.Core;
using UnityEngine;

namespace SkillForge.Scenarios.Conditions
{
    [CreateAssetMenu(menuName = "SkillForge/Conditions/All Bolts Tightened")]
    public class AllBoltsTightenedCondition : StepCondition
    {
        public string[] boltIds;

        public override bool IsMet(IWorkContext context)
        {
            if (boltIds == null || boltIds.Length == 0)
                return false;

            foreach (var boltId in boltIds)
            {
                if (context.GetPartState(boltId) != PartState.Default)
                    return false;
            }

            return true;
        }
    }
}
