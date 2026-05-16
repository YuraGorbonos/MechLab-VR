using UnityEngine;

namespace SkillForge.Core
{
    [System.Serializable]
    public class ScenarioStep
    {
        public string stepId;
        public string instruction;
        public StepCondition[] conditions;
        public string[] requiredToolIds;
        public string highlightTargetId;
        public bool isOptional;
    }
}
