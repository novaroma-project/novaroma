using System.ComponentModel.DataAnnotations;
using Novaroma.Properties;

namespace Novaroma {

    public enum Language {
        [Display(Name = "English", ResourceType = typeof(Resources))]
        English,
        [Display(Name = "Turkish", ResourceType = typeof(Resources))]
        Turkish,
        [Display(Name = "German", ResourceType = typeof(Resources))]
        German,
        [Display(Name = "French", ResourceType = typeof(Resources))]
        French,
        [Display(Name = "Italian", ResourceType = typeof(Resources))]
        Italian,
        [Display(Name = "Dutch", ResourceType = typeof(Resources))]
        Dutch,
        [Display(Name = "Russian", ResourceType = typeof(Resources))]
        Russian
    }
}
