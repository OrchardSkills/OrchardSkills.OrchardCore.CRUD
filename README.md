# OrchardSkills.OrchardCore.CRUD

Using CRUD operations in Orchard Core CMS

## Etch UK Ltd. Content Permissions module

The Content Permissions module for Orchard Core allows you to configuring access level at a item per content type.

### Original Authors GitHub Repository

[Etch UK Ltd. Content Permissions Repository](https://github.com/EtchUK/Etch.OrchardCore.ContentPermissions)

### Article

[Etch UK Ltd. Community Love: The Value of Open Source](https://www.etchuk.com/insights/community-love-the-value-of-open-source)

### Getting Started

Enabled the "Content Permissions" feature, which will make a new "Content Permissions" part available. Attach this part to the desired content types, which will add a new "Security" tab to the content editor. From this tab the content item permissions can be enabled, which will display all the roles in the CMS. Select the roles that can access the content item and publish. Users not associated to one of the specified roles will receive a 403 response and redirected to /Error/403. The redirect URL can be customized within the settings for the content part.

## Antoine Griffard Disqus module

The Disqus module for Orchard Core allows you to add a Disqus comment section to a content type.

### Original Authors GitHub Repository

[Antoine Griffard agriffard Disqus.OrchardCore](https://github.com/agriffard/Disqus.OrchardCore)

### Getting Started

* Add a Disqus Part in the Definition of the Content Type on which you want to add Disqus comments (BlogPosts)
* Edit the settings of the Disqus Part and enter the name of the site that you created at https://disqus.com/.
* Add {{ Model.Content.DisqusPart | shape_render }} to the bottom of the Content-BlogPost.liquid file.
* Set the Base Url (ex: https://localhost:5001 ) in /Admin/Settings/general so that the urls will be absolute.
