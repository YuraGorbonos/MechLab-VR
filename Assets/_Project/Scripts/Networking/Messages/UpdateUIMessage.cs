using Mirror;
using SkillForge.Core;

public struct UpdateUIMessage : NetworkMessage
{
    public string instruction;
    public int stepIndex;
    public StepInput input;
    public string hint;
    public string buttonLabel;
    public Intention intention;
    public string[] removedTargetIds;
}
