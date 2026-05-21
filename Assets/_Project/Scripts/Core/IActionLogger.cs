using System.Collections.Generic;

namespace SkillForge.Core
{
    public interface IActionLogger
    {
        IReadOnlyList<ActionLogEntry> Entries { get; }

        void Log(Intention intent, bool success, string errorCode = null);

        void Clear();
    }
}