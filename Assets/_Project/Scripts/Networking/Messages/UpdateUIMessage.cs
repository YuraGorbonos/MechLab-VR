using Mirror;

public struct UpdateUIMessage : NetworkMessage
{
    public string instruction;
    public int stepIndex;
}
