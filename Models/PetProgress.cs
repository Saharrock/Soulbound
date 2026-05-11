namespace Soulbound.Models
{
    public class PetProgress
    {
        public string PetName { get; set; } = string.Empty;
        public string SelectedPetImage { get; set; } = "dotnet_bot.png";

        /// <summary>True after the user confirms pet name/avatar once; stored in Firebase under gameData.character.</summary>
        public bool PetOnboardingComplete { get; set; }

        /// <summary>Start of the week used for the last Sunday 00:00 stamina refill (local time).</summary>
        public DateTime LastSundayStaminaReset { get; set; }

        /// <summary>Weekly energy pool — refilled every Sunday at 00:00 local.</summary>
        public int Stamina { get; set; } = 100;

        /// <summary>Goals closed with Done (excludes deleted-before-done).</summary>
        public int CompletedGoalsLifetime { get; set; }
    }
}
