using UnityEngine;

namespace SkillForge.Core
{
    public abstract class StepCondition : ScriptableObject
    {
        public abstract bool IsMet(IWorkContext context);
    }
}
