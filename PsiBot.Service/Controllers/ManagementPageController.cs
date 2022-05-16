// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ManagementPageController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// </copyright>
// <summary>
//   JoinCallController is a third-party controller (non-Bot Framework) that can be called in CVI scenario to trigger the bot to join a call
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PsiBot.Services.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using PsiBot.Model.Constants;
    using System.IO;
    using System.Net.Http;
    using System.Reflection;
    using System.Text;

    /// <summary>
    /// Management page allowing joining, listing, and leaving calls.
    /// </summary>
    public class ManagementPageController : ControllerBase
    {
        /// <summary>
        /// The join call async.
        /// </summary>
        /// <returns>
        /// The <see cref="HttpResponseMessage"/>.
        /// </returns>
        [HttpGet]
        [Route(HttpRouteConstants.Management + "/")]
        public ContentResult ManagementPage()
        {
            var exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string uiIndexPage = string.Format("{0}\\{1}", exePath, "UI\\index.html");
            string uiContent = System.IO.File.ReadAllText(uiIndexPage);
            return Content(uiContent, "text/html", Encoding.UTF8); ;
        }
    }
}