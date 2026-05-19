namespace SkillForge.Core
{
    public enum HighlightType
    {
        Target,
        Tool,
        Warning,
        Info
    }

    public interface IHighlightService
    {
        void SetHighlight(string targetId, HighlightType type);
        void ClearHighlights();
    }
}
