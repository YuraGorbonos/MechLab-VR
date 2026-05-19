namespace SkillForge.Core
{
    public enum SessionMode
    {
        Training,
        Exam
    }

    public class SessionConfig
    {
        public string scenarioId;
        public int playerCount;
        public SessionMode mode;
    }
}
