namespace SkillForge.Core
{
    public struct DiagnosticResult
    {
        public string subsystem;
        public bool isFunctional;
        public string details;

        public DiagnosticResult(string subsystem, bool isFunctional, string details)
        {
            this.subsystem = subsystem;
            this.isFunctional = isFunctional;
            this.details = details;
        }
    }
}
