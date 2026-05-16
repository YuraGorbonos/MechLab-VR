using System;
using System.Collections.Generic;

namespace SkillForge.Core
{
    [Serializable]
    public class SessionReport
    {
        public string scenarioId;
        public SessionMode mode;
        public float durationSeconds;
        public int totalSteps;
        public int completedSteps;
        public int skippedSteps;
        public List<ActionLogEntry> actionLog;
        public float score;
        public string grade;
    }

    [Serializable]
    public class ActionLogEntry
    {
        public float timestamp;
        public int playerId;
        public string intentType;
        public string targetObject;
        public bool success;
        public string errorCode;
    }
}
