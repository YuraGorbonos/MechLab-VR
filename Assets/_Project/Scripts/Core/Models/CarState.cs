using System;

namespace SkillForge.Core
{
    [Serializable]
    public struct CarState
    {
        public bool engineRunning;
        public float oilLevel;
        public float brakeFluidLevel;
        public bool timingBeltOk;
        public bool batteryCharged;
    }
}
