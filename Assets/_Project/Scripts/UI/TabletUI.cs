using Mirror;
using SkillForge.Core;
using UnityEngine;

namespace SkillForge.UI
{
    public class TabletUI : NetworkBehaviour
    {
        [SyncVar]
        private string _instructionText;

        public void UpdateInstruction(string text)
        {
            _instructionText = text;
            // TODO: update UI text element
        }
    }
}
