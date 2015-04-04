using System;
using System.ComponentModel.DataAnnotations;
using Novaroma.Properties;

namespace Novaroma {

    [Flags]
    public enum MediaTypes {
        [Display(Name = "Movies", ResourceType = typeof(Resources))]
        Movie = 1,
        [Display(Name = "TvShows", ResourceType = typeof(Resources))]
        TvShow = 2,
        [Display(Name = "Documentary", ResourceType = typeof(Resources))]
        Documentary = 4,
        [Display(Name = "All", ResourceType = typeof(Resources))]
        All = Movie | TvShow | Documentary
    }
}
