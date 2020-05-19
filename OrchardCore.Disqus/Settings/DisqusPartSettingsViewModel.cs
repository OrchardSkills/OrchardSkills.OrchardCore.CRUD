using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Disqus.Settings
{
    public class DisqusPartSettingsViewModel
    {
        public string ShortName { get; set; }

        [BindNever]
        public DisqusPartSettings DisqusPartSettings { get; set; }
    }
}
