using OrchardCore.Disqus.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;
using System;
using System.Threading.Tasks;

namespace OrchardCore.Disqus.Settings
{
    public class DisqusPartSettingsDisplayDriver : ContentTypePartDefinitionDisplayDriver
    {
        public override IDisplayResult Edit(ContentTypePartDefinition contentTypePartDefinition)
        {
            if (!String.Equals(nameof(DisqusPart), contentTypePartDefinition.PartDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            return Initialize<DisqusPartSettingsViewModel>("DisqusPartSettings_Edit", model =>
            {
                var settings = contentTypePartDefinition.GetSettings<DisqusPartSettings>();

                model.ShortName = settings.ShortName;
                model.DisqusPartSettings = settings;

            }).Location("Content");
        }

        public override async Task<IDisplayResult> UpdateAsync(ContentTypePartDefinition contentTypePartDefinition, UpdateTypePartEditorContext context)
        {
            if (!String.Equals(nameof(DisqusPart), contentTypePartDefinition.PartDefinition.Name, StringComparison.Ordinal))
            {
                return null;
            }

            var model = new DisqusPartSettingsViewModel();

            if (await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.ShortName))
            {
                context.Builder.WithSettings(new DisqusPartSettings { ShortName = model.ShortName });
            }

            return Edit(contentTypePartDefinition, context.Updater);
        }
    }
}