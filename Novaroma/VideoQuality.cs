using System.ComponentModel.DataAnnotations;
using Novaroma.Properties;

namespace Novaroma {

    public enum VideoQuality {
        [Display(Name = "Any", ResourceType = typeof(Resources))]
        Any,
        [Display(Name = "P720", ResourceType = typeof(Resources))]
        P720,
        [Display(Name = "P1080", ResourceType = typeof(Resources))]
        P1080
    }
}
