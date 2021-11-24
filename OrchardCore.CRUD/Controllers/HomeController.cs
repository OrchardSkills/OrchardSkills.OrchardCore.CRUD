using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Routing;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Navigation;
using OrchardCore.Routing;
using OrchardCore.Settings;
using YesSql;

namespace OrchardCore.CRUD.Controllers
{
    public class HomeController : Controller
    {

        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IContentItemDisplayManager _contentItemDisplayManager;
        private readonly IHtmlLocalizer H;
        private readonly dynamic New;
        private readonly INotifier _notifier;
        private readonly ISession _session;
        private readonly ISiteService _siteService;
        private readonly IUpdateModelAccessor _updateModelAccessor;
        private readonly IShapeFactory _shapeFactory;

        public HomeController(IContentManager contentManager, IContentDefinitionManager contentDefinitionManager, IContentItemDisplayManager contentItemDisplayManager, IHtmlLocalizer<HomeController> htmlLocalizer, INotifier notifier, ISession session, IShapeFactory shapeFactory, ISiteService siteService, IUpdateModelAccessor updateModelAccessor)
        {
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
            _contentItemDisplayManager = contentItemDisplayManager;
            _notifier = notifier;
            _session = session;
            _siteService = siteService;
            _updateModelAccessor = updateModelAccessor;

            H = htmlLocalizer;
            New = shapeFactory;
            _shapeFactory = shapeFactory;
        }

        public async Task<IActionResult> Index(string contentTypeName, PagerParameters pagerParameters) 
        {
            var siteSettings = await _siteService.GetSiteSettingsAsync();
            var pager = new Pager(pagerParameters, siteSettings.PageSize);
            
            var query = _session.Query<ContentItem, ContentItemIndex>();
            query = query.With<ContentItemIndex>(x => x.ContentType == contentTypeName);
            query = query.With<ContentItemIndex>(x => x.Published);
            query = query.OrderByDescending(x => x.PublishedUtc);

            var maxPagedCount = siteSettings.MaxPagedCount;
            if (maxPagedCount > 0 && pager.PageSize > maxPagedCount)
                pager.PageSize = maxPagedCount;

            var routeData = new RouteData();
            var pagerShape = (await New.Pager(pager)).TotalItemCount(maxPagedCount > 0 ? maxPagedCount : await query.CountAsync()).RouteData(routeData);

            var pageOfContentItems = await query.Skip(pager.GetStartIndex()).Take(pager.PageSize).ListAsync();
            IEnumerable<ContentItem> model = await query.ListAsync();

            // Prepare the content items Summary Admin shape
            var contentItemSummaries = new List<dynamic>();
            foreach (var contentItem in pageOfContentItems)
            {
                contentItemSummaries.Add(await _contentItemDisplayManager.BuildDisplayAsync(contentItem, _updateModelAccessor.ModelUpdater, "Summary"));
            }


            // Populate options pager summary values.
            var startIndex = (pagerShape.Page - 1) * (pagerShape.PageSize) + 1;
            var options = new ContentOptionsViewModel {
                StartIndex = startIndex,
                EndIndex = startIndex + contentItemSummaries.Count - 1,
                ContentItemsCount = contentItemSummaries.Count,
                TotalItemCount = pagerShape.TotalItemCount,
                SelectedContentType = contentTypeName
            };

            var viewModel = new ListContentsViewModel {
                ContentItems = contentItemSummaries,
                Pager = pagerShape,
                Options = options
            };

            var shapeViewModel = await _shapeFactory.CreateAsync<ListContentsViewModel>("ContentsAdminList", viewModel =>
            {
                viewModel.ContentItems = contentItemSummaries;
                viewModel.Pager = pagerShape;
                viewModel.Options = options;
                //viewModel.Header = header;
            });

            return View(viewModel);
        }

        public async Task<IActionResult> Create(string contentTypeName)
        {
            if (String.IsNullOrWhiteSpace(contentTypeName))
            {
                return NotFound();
            }

            var contentItem = await _contentManager.NewAsync(contentTypeName);


            var model = await _contentItemDisplayManager.BuildEditorAsync(contentItem, _updateModelAccessor.ModelUpdater, true);

            return View(model);
        }

        [HttpPost, ActionName("Create")]
        [FormValueRequired("submit.Publish")]
        public async Task<IActionResult> CreateAndPublishPOST(string contentTypeName, [Bind(Prefix = "submit.Publish")] string submitPublish, string returnUrl)
        {
            var stayOnSamePage = submitPublish == "submit.PublishAndContinue";
            // pass a dummy content to the authorization check to check for "own" variations
            var dummyContent = await _contentManager.NewAsync(contentTypeName);

            return await CreatePOST(contentTypeName, returnUrl, stayOnSamePage, async contentItem =>
            {
                await _contentManager.PublishAsync(contentItem);

                var typeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

                await _notifier.SuccessAsync(string.IsNullOrWhiteSpace(typeDefinition.DisplayName)
                    ? H["Your content has been published."]
                    : H["Your {0} has been published.", typeDefinition.DisplayName]);
            });
        }

        private async Task<IActionResult> CreatePOST(string contentTypeName, string returnUrl, bool stayOnSamePage, Func<ContentItem, Task> conditionallyPublish)
        {
            var contentItem = await _contentManager.NewAsync(contentTypeName);

            // Set the current user as the owner to check for ownership permissions on creation
            contentItem.Owner = User.Identity.Name;

            var model = await _contentItemDisplayManager.UpdateEditorAsync(contentItem, _updateModelAccessor.ModelUpdater, true);

            if (!ModelState.IsValid)
            {
                await _session.CancelAsync();
                return View(model);
            }

            await _contentManager.CreateAsync(contentItem, VersionOptions.Draft);

            await conditionallyPublish(contentItem);

            if ((!string.IsNullOrEmpty(returnUrl)) && (!stayOnSamePage))
            {
                return LocalRedirect(returnUrl);
            }

            //var adminRouteValues = (await _contentManager.PopulateAspectAsync<ContentItemMetadata>(contentItem)).AdminRouteValues;

            //if (!string.IsNullOrEmpty(returnUrl))
            //{
            //    adminRouteValues.Add("returnUrl", returnUrl);
            //}

            return RedirectToRoute(returnUrl);
        }

        public async Task<IActionResult> Edit(string contentItemId)
        {
            var contentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.Latest);

            if (contentItem == null)
            {
                return NotFound();
            }

            //if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.EditContent, contentItem))
            //{
            //    return Forbid();
            //}

            var model = await _contentItemDisplayManager.BuildEditorAsync(contentItem, _updateModelAccessor.ModelUpdater, false);

            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("submit.Save")]
        public Task<IActionResult> EditPOST(string contentItemId, [Bind(Prefix = "submit.Save")] string submitSave, string returnUrl)
        {
            var stayOnSamePage = submitSave == "submit.SaveAndContinue";
            return EditPOST(contentItemId, returnUrl, stayOnSamePage, async contentItem =>
            {
                await _contentManager.SaveDraftAsync(contentItem);

                var typeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

                await _notifier.SuccessAsync(string.IsNullOrWhiteSpace(typeDefinition.DisplayName)
                    ? H["Your content draft has been saved."]
                    : H["Your {0} draft has been saved.", typeDefinition.DisplayName]);
            });
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("submit.Publish")]
        public async Task<IActionResult> EditAndPublishPOST(string contentItemId, [Bind(Prefix = "submit.Publish")] string submitPublish, string returnUrl)
        {
            var stayOnSamePage = submitPublish == "submit.PublishAndContinue";

            var content = await _contentManager.GetAsync(contentItemId, VersionOptions.Latest);

            if (content == null)
            {
                return NotFound();
            }

            //if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.PublishContent, content))
            //{
            //    return Forbid();
            //}

            return await EditPOST(contentItemId, returnUrl, stayOnSamePage, async contentItem =>
            {
                await _contentManager.PublishAsync(contentItem);

                var typeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

                await _notifier.SuccessAsync(string.IsNullOrWhiteSpace(typeDefinition.DisplayName)
                    ? H["Your content has been published."]
                    : H["Your {0} has been published.", typeDefinition.DisplayName]);
            });
        }


        private async Task<IActionResult> EditPOST(string contentItemId, string returnUrl, bool stayOnSamePage, Func<ContentItem, Task> conditionallyPublish)
        {
            var contentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.DraftRequired);

            if (contentItem == null)
            {
                return NotFound();
            }

            //if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.EditContent, contentItem))
            //{
            //    return Forbid();
            //}

            var model = await _contentItemDisplayManager.UpdateEditorAsync(contentItem, _updateModelAccessor.ModelUpdater, false);

            if (!ModelState.IsValid)
            {
                await _session.CancelAsync();
                return View(nameof(Edit), model);
            }

            await conditionallyPublish(contentItem);

            if (returnUrl == null)
            {
                return RedirectToAction(nameof(Edit), new RouteValueDictionary { { "ContentItemId", contentItem.ContentItemId } });
            }
            else if (stayOnSamePage)
            {
                return RedirectToAction(nameof(Edit), new RouteValueDictionary { { "ContentItemId", contentItem.ContentItemId }, { "returnUrl", returnUrl } });
            }
            else
            {
                return this.LocalRedirect(returnUrl, true);
            }
        }

        public async Task<IActionResult> Remove(string contentItemId, string returnUrl)
        {
            var contentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.Latest);

            if (contentItem != null)
            {
                var typeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

                await _contentManager.RemoveAsync(contentItem);

                await _notifier.SuccessAsync(string.IsNullOrWhiteSpace(typeDefinition.DisplayName)
                    ? H["That content has been removed."]
                    : H["That {0} has been removed.", typeDefinition.DisplayName]);
            }

            return Url.IsLocalUrl(returnUrl) ? (IActionResult)LocalRedirect(returnUrl) : RedirectToAction("Index");
        }
    }
}
