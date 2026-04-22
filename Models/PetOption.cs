namespace Soulbound.Models
{
    /// <summary>
    /// One selectable pet template (image file name and default display name).
    /// </summary>
    public class PetOption
    {
        public string Image { get; set; } = string.Empty;

        public string DefaultName { get; set; } = string.Empty;
    }
}
