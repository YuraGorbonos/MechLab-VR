namespace SkillForge.Core
{
    public interface IDiagnosticService
    {
        DiagnosticResult Diagnose(string subsystem, CarState state);
    }
}
