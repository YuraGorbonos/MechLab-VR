using UnityEngine;

namespace SkillForge.Core
{
    public enum StepInput
    {
        Shortcut,
        HeldTool,
        TabletButton
    }

    [CreateAssetMenu(menuName = "SkillForge/Scenario Step")]
    public class ScenarioStep : ScriptableObject
    {
        public string stepId;
        public string instruction;
        public string hint;
        public StepInput input;
        public string buttonLabel;
        public string intentType = "use_tool";
        public string action;
        public StepCondition[] conditions;
        public string[] requiredToolIds;
        public string highlightTargetId;
        public bool isOptional;

        public Intention CreateIntention()
        {
            return new Intention
            {
                intentType = intentType,
                targetObject = highlightTargetId,
                tool = requiredToolIds != null && requiredToolIds.Length > 0 ? requiredToolIds[0] : null,
                action = action
            };
        }
    }
}
