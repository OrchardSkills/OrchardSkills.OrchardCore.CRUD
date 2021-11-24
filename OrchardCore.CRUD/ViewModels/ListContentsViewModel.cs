using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Contents.ViewModels;

namespace OrchardCore.CRUD.ViewModels
{

    // This is almost the same class as in `OrchardCore.Contents.ViewModels.ListContentsViewModel`
    public class ListContentsViewModel
    {
        public ListContentsViewModel()
        {
            Options = new ContentOptionsViewModel();
        }

        public ContentOptionsViewModel Options { get; set; }

        [BindNever]
        public List<dynamic> ContentItems { get; set; }

        [BindNever]
        public dynamic Pager { get; set; }
    }    
}
