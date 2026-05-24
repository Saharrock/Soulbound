namespace Soulbound.Models

{

    // Профиль после входа (в AppService, не в gameData). В FB: users/{uid}/profile

    internal class AuthUser

    {

        public string? Email { get; set; }

        public string? Id { get; set; }       // Firebase UID

        public string? UserName { get; set; }

    }

}


