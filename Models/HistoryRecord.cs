using Microsoft.Maui.Graphics;

namespace Soulbound.Models
{
    /// <summary>
    /// One row in the activity log: task outcome and XP / stamina change.
    /// </summary>
    public class HistoryRecord
    {
        public string TaskName { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string ResultStatus { get; set; } = string.Empty;

        public int XpChange { get; set; }

        public int StaminaSpent { get; set; }

        public DateTime DateFinished { get; set; } = DateTime.Now;

        public string StatusText => ResultStatus;
        public Color StatusColor => ResultStatus switch
        {
            "Completed" => Color.FromArgb("#7ed957"),
            "Failed" => Color.FromArgb("#ff4444"),
            "Penalty" => Color.FromArgb("#ff9f43"),
            _ => Colors.Gray
        };

        public Color CategoryBarColor => Category switch
        {
            "Physical" => Color.FromArgb("#e03c31"),
            "Intellectual" => Color.FromArgb("#3c6fe0"),
            "Mental" => Color.FromArgb("#9b4de0"),
            _ => Color.FromArgb("#888888")
        };

        /// <summary>
        /// One line for the resources column: XP change and stamina spent.
        /// </summary>
        public string ResourcesLine
        {
            get
            {
                string xpPart;
                if (XpChange >= 0)
                {
                    xpPart = "+" + XpChange.ToString() + " XP";
                }
                else
                {
                    xpPart = XpChange.ToString() + " XP";
                }

                string energyPart;
                if (StaminaSpent > 0)
                {
                    energyPart = " / -" + StaminaSpent.ToString() + " stamina";
                }
                else
                {
                    energyPart = " / stamina: 0";
                }

                return xpPart + energyPart;
            }
        }
    }
}
