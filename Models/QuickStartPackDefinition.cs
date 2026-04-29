using System;

namespace Soulbound.Models
{
    public sealed class QuickStartPackDefinition
    {
        public string Id { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public bool IsPhysical { get; init; }
        public bool IsIntellectual { get; init; }
        public bool IsMental { get; init; }
        public IReadOnlyList<QuickStartTaskDefinition> Tasks { get; init; } = Array.Empty<QuickStartTaskDefinition>();
    }

    public sealed class QuickStartTaskDefinition
    {
        public string Title { get; init; } = string.Empty;
        public int HoursFromNow { get; init; }
        public int XpGain { get; init; }
    }
}
