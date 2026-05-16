using System;
using System.Collections.Generic;
using SkillForge.Core;
using UnityEngine;

namespace SkillForge.Education
{
    public class ActionLogger
    {
        private readonly List<ActionLogEntry> _entries;

        public IReadOnlyList<ActionLogEntry> Entries => _entries;

        public ActionLogger()
        {
            _entries = new List<ActionLogEntry>();
        }

        public void Log(Intention intent, bool success, string errorCode = null)
        {
            _entries.Add(new ActionLogEntry
            {
                timestamp = Time.time,
                playerId = intent.playerId,
                intentType = intent.intentType,
                targetObject = intent.targetObject,
                success = success,
                errorCode = errorCode
            });

            Debug.Log($"[ActionLogger] Player {intent.playerId} | {intent.intentType} -> {intent.targetObject} | {(success ? "OK" : errorCode)}");
        }

        public void Clear()
        {
            _entries.Clear();
        }
    }
}
