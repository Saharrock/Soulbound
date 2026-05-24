namespace Soulbound.Models

{

    // Питомец и игрок. В Firebase: gameData.Character

    public class PetProgress

    {

        public string PetName { get; set; } = string.Empty;

        public string SelectedPetImage { get; set; } = "cat.png";



        // false до первого выбора питомца на PetSelectionPage

        public bool PetOnboardingComplete { get; set; }



        // Дата последнего воскресного сброса stamina (00:00 local)

        public DateTime LastSundayStaminaReset { get; set; }



        // Текущая выносливость; максимум 100 в неделю

        public int Stamina { get; set; } = 100;



        // Сколько целей закрыто Done за всё время

        public int CompletedGoalsLifetime { get; set; }

    }

}


