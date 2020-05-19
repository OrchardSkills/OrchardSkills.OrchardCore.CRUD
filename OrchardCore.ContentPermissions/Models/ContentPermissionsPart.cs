using System;
using OrchardCore.ContentManagement;

namespace OrchardCore.ContentPermissions.Models
{
    public class ContentPermissionsPart : ContentPart
    {
        public bool Enabled { get; set; }
        public string[] Roles { get; set; } = Array.Empty<string>();
    }
}
