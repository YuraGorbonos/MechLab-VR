using System.Collections.Generic;
using SkillForge.Core;

namespace SkillForge.Core
{
    public interface IActionLogger
    {
        void Log(Intention intent, bool success, string errorCode = null);
        IReadOnlyList<ActionLogEntry> Entries { get; }
        void Clear();
    }
}
