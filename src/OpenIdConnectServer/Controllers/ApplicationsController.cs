using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using OpenIdConnectServer.Models;
using OpenIdConnectServer.ViewModels.AccountViewModels;
using OpenIdConnectServer.Services;
using AspNetCore.Identity.DynamoDB.Models;
using OpenIddict.Core;
using AspNetCore.Identity.DynamoDB.OpenIddict;
using OpenIdConnectServer.ViewModels.Applications;
using System;

namespace OpenIdConnectServer.Controllers
{
    [Authorize]
    public class ApplicationsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationApplicationManager _applicationsManager;
        private readonly ILogger _logger;

        public ApplicationsController(UserManager<ApplicationUser> userManager,
            OpenIddictApplicationManager<DynamoIdentityApplication> applicationsManager,
            ILoggerFactory loggerFactory)
        {
            _userManager = userManager;
            _applicationsManager = applicationsManager as ApplicationApplicationManager;
            _logger = loggerFactory.CreateLogger<ApplicationsController>();
        }

        public async Task<IActionResult> Index()
        {
            return View(new ApplicationsIndexViewModel
            {
                Applications = await _applicationsManager.FindAsync(HttpContext.RequestAborted)
            });
        }

        public async Task<IActionResult> New(ApplicationViewModel model)
        {
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ApplicationViewModel model)
        {
            if (model.Type != OpenIddictConstants.ClientTypes.Confidential
                && model.Type != OpenIddictConstants.ClientTypes.Public)
            {
                //add model errors
                return View("Edit", model);
            }

            var application = new DynamoIdentityApplication
            {
                DisplayName = model.DisplayName,
                LogoutRedirectUri = model.LogoutRedirectUri,
                RedirectUri = model.RedirectUri,
                Type = model.Type
            };

            if (application.Type == OpenIddictConstants.ClientTypes.Confidential)
            {
                await _applicationsManager.CreateAsync(application, Guid.NewGuid().ToString(), HttpContext.RequestAborted);
            }
            else
            {
                await _applicationsManager.CreateAsync(application, HttpContext.RequestAborted);
            }

            
            return View(new ApplicationWithSecretViewModel(model, application.ClientSecret));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var application = await _applicationsManager.FindByIdAsync(id, HttpContext.RequestAborted);

            await _applicationsManager.DeleteAsync(application, HttpContext.RequestAborted);
            
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(ApplicationViewModel model)
        {
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Update(ApplicationViewModel model)
        {
            var application = await _applicationsManager.FindByIdAsync(model.ClientId, HttpContext.RequestAborted);

            if (application == null)
            {
                return NotFound();
            }

            application.DisplayName = model.DisplayName;
            application.LogoutRedirectUri = model.LogoutRedirectUri;
            application.RedirectUri = model.RedirectUri;

            await _applicationsManager.UpdateAsync(application, HttpContext.RequestAborted);

            return RedirectToAction(nameof(Edit));
        }
    }
}
