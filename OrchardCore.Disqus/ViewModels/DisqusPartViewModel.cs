using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement;
using OrchardCore.Disqus.Settings;
using OrchardCore.Disqus.Models;

namespace OrchardCore.Disqus.ViewModels
{
    public class DisqusPartViewModel
    {
        public string ShortName { get; set; }

        public bool HideComments { get; set; }

        [BindNever]
        public ContentItem ContentItem { get; set; }

        [BindNever]
        public DisqusPart DisqusPart { get; set; }

        [BindNever]
        public DisqusPartSettings Settings { get; set; }
    }
}
