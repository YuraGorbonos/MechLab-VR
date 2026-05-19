using System.Collections.Generic;
using SkillForge.Core;

namespace SkillForge.Highlight
{
    public class HighlightService : IHighlightService
    {
        private readonly Dictionary<string, HighlightType> _activeHighlights;

        public HighlightService()
        {
            _activeHighlights = new Dictionary<string, HighlightType>();
        }

        public void SetHighlight(string targetId, HighlightType type)
        {
            // TODO: notify clients via Mirror RPC to render highlight
            _activeHighlights[targetId] = type;
        }

        public void ClearHighlights()
        {
            _activeHighlights.Clear();
        }

        public HighlightType? GetHighlight(string targetId)
        {
            return _activeHighlights.TryGetValue(targetId, out var type) ? type : null;
        }

        public IReadOnlyDictionary<string, HighlightType> GetAllHighlights()
        {
            return _activeHighlights;
        }
    }
}
