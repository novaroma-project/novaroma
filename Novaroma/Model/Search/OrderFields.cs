using System.ComponentModel.DataAnnotations;
using Novaroma.Properties;

namespace Novaroma.Model.Search {

    public enum OrderFields {
        [Display(Name = "Title", ResourceType = typeof(Resources))]
        Title,
        [Display(Name = "Year", ResourceType = typeof(Resources))]
        Year,
        [Display(Name = "Rating", ResourceType = typeof(Resources))]
        Rating,
    }
}
