using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Primitives;
using AspNet.Security.OpenIdConnect.Server;
using OpenIdConnectServer.ViewModels.Shared;
using OpenIdConnectServer.ViewModels.AuthorizationViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Core;
using OpenIddict.Models;
using OpenIdConnectServer.Models;
using OpenIdConnectServer.Helpers;
using AspNetCore.Identity.DynamoDB.OpenIddict;
using AspNetCore.Identity.DynamoDB.OpenIddict.Models;
using System.Threading;
using OpenIdConnectServer.Services;
using AspNetCore.Identity.DynamoDB.OpenIddict.Stores;
using System;
using OpenIddict.DeviceCodeFlow;

namespace OpenIdConnectServer.Controllers
{
    public partial class AuthorizationController
    {
        [Authorize, HttpGet("~/connect/authorize_device")]
        public IActionResult ConnectDeviceCode()
        {
            return View();
        }

        [Authorize]
        [HttpPost("~/connect/authorize_device"), ValidateAntiForgeryToken]
        public async Task<IActionResult> ConnectDeviceCodeConfirmation(AuthorizeDeviceCodeRequest model)
        {
            var deviceCode = await _deviceCodeManager.FindByUserCodeAsync(model.UserCode);
            if (deviceCode == null)
            {
                ModelState.AddModelError(string.Empty, "Unrecognised or expired code.");
                return View("ConnectDeviceCode", model);
            }

            if (deviceCode.Application == null)
            {
                return View("Error", new ErrorViewModel
                {
                    Error = OpenIdConnectConstants.Errors.InvalidClient,
                    ErrorDescription = "Details concerning the calling client application cannot be found in the database"
                });
            }

            var application = await _applicationManager.FindByIdAsync(deviceCode.Application, Request.HttpContext.RequestAborted);
            if (application == null)
            {
                return View("Error", new ErrorViewModel
                {
                    Error = OpenIdConnectConstants.Errors.InvalidClient,
                    ErrorDescription = "Details concerning the calling client application cannot be found in the database"
                });
            }

            // Retrieve the profile of the logged in user.
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return View("Error", new ErrorViewModel
                {
                    Error = OpenIdConnectConstants.Errors.ServerError,
                    ErrorDescription = "An internal error has occurred"
                });
            }

            return View(new AuthorizeDeviceCodeViewModel
            {
                UserCode = deviceCode.UserCode,
                Scope = string.Join(" ", deviceCode.Scopes),
                ApplicationName = application.DisplayName
            });
        }

        [Authorize, FormValueRequired("submit.Accept")]
        [HttpPost("~/connect/device_code_authorization"), ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptDevice(AuthorizeDeviceCodeRequest request)
        {
            var deviceCode = await _deviceCodeManager.FindByUserCodeAsync(request.UserCode);
            if (deviceCode == null)
            {
                ModelState.AddModelError(string.Empty, "Unrecognised or expired code.");
                return View("ConnectDeviceCode", request);
            }

            if (deviceCode.Application == null)
            {
                return View("Error", new ErrorViewModel
                {
                    Error = OpenIdConnectConstants.Errors.InvalidClient,
                    ErrorDescription = "Details concerning the calling client application cannot be found in the database"
                });
            }

            var application = await _applicationManager.FindByIdAsync(deviceCode.Application, Request.HttpContext.RequestAborted);
            if (application == null)
            {
                return View("Error", new ErrorViewModel
                {
                    Error = OpenIdConnectConstants.Errors.InvalidClient,
                    ErrorDescription = "Details concerning the calling client application cannot be found in the database"
                });
            }

            // Retrieve the profile of the logged in user.
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return View("Error", new ErrorViewModel
                {
                    Error = OpenIdConnectConstants.Errors.ServerError,
                    ErrorDescription = "An internal error has occurred"
                });
            }

            // Create a new authentication ticket.
            var ticket = await CreateTicketAsync(new OpenIdConnectRequest
            {
                ClientId = deviceCode.Application,
                Scope = string.Join(" ", deviceCode.Scopes),
            }, user);

            await _deviceCodeManager.Authorize(deviceCode, user.Id);


            return View("AuthorizeDeviceCodeResult", new AuthorizeDeviceCodeResultViewModel
            {
                ApplicationName = application.DisplayName,
                Scope = string.Join(" ", deviceCode.Scopes),
                Authorized = true
            });
        }

        [Authorize, FormValueRequired("submit.Deny")]
        [HttpPost("~/connect/device_code_authorization"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DenyDevice(AuthorizeDeviceCodeViewModel request)
        {
            var deviceCode = await _deviceCodeManager.FindByUserCodeAsync(request.UserCode);
            if (deviceCode == null)
            {
                ModelState.AddModelError(string.Empty, "Unrecognised or expired code.");
                return View("ConnectDeviceCode", request);
            }

            if (deviceCode.Application == null)
            {
                return View("Error", new ErrorViewModel
                {
                    Error = OpenIdConnectConstants.Errors.InvalidClient,
                    ErrorDescription = "Details concerning the calling client application cannot be found in the database"
                });
            }

            var application = await _applicationManager.FindByIdAsync(deviceCode.Application, Request.HttpContext.RequestAborted);
            if (application == null)
            {
                return View("Error", new ErrorViewModel
                {
                    Error = OpenIdConnectConstants.Errors.InvalidClient,
                    ErrorDescription = "Details concerning the calling client application cannot be found in the database"
                });
            }

            await _deviceCodeManager.Revoke(deviceCode);

            // Notify OpenIddict that the authorization grant has been denied by the resource owner
            // to redirect the user agent to the client application using the appropriate response_mode.
            return View("AuthorizeDeviceCodeResult", new AuthorizeDeviceCodeResultViewModel
            {
                ApplicationName = application.DisplayName,
                Scope = string.Join(" ", deviceCode.Scopes),
                Authorized = false
            });
        }

        [HttpPost("~/connect/device_token"), Produces("application/json")]
        public async Task<IActionResult> MintDeviceCode(string response_type, string client_id, string client_secret, string scope)
        {
            if (response_type != DeviceCodeFlowConstants.ResponseTypes.DeviceCode)
            {
                return BadRequest(new ErrorViewModel
                {
                    Error = OpenIdConnectConstants.Errors.InvalidRequest,
                    ErrorDescription = "Invalid response_type"
                });
            }

            if (client_id == null)
            {
                return BadRequest(new ErrorViewModel
                {
                    Error = OpenIdConnectConstants.Errors.InvalidRequest,
                    ErrorDescription = "Missing client_id"
                });
            }

            if (scope == null)
            {
                return BadRequest(new ErrorViewModel
                {
                    Error = OpenIdConnectConstants.Errors.InvalidRequest,
                    ErrorDescription = "Missing scope"
                });
            }


            var application = await _applicationManager.FindByClientIdAsync(client_id, HttpContext.RequestAborted);
            if (application == null)
            {
                return BadRequest(new ErrorViewModel
                {
                    Error = OpenIdConnectConstants.Errors.InvalidClient,
                    ErrorDescription = "Details concerning the calling client application cannot be found in the database"
                });
            }

            if (await _applicationManager.IsConfidentialAsync(application, Request.HttpContext.RequestAborted))
            {
                if (string.IsNullOrEmpty(client_secret) || !await _applicationManager.ValidateClientSecretAsync(application, client_secret, Request.HttpContext.RequestAborted))
                {
                    return BadRequest(new ErrorViewModel
                    {
                        Error = OpenIdConnectConstants.Errors.InvalidClient,
                        ErrorDescription = "Confidential clients must supply the client_secret"
                    });
                }
            }
            else if (!string.IsNullOrEmpty(client_secret))
            {
                return BadRequest(new ErrorViewModel
                {
                    Error = OpenIdConnectConstants.Errors.InvalidClient,
                    ErrorDescription = "Public clients must not submit a client_secret"
                });
            }

            var deviceCode = await _deviceCodeManager.CreateAsync(application.Id, scope);

            // issue user and device codes
            return Json(new DeviceCodeFlowViewModel
            {
                VerificationUri = Url.Action("ConnectDeviceCode", null, null, Request.Scheme),
                UserCode = deviceCode.UserCode,
                DeviceCode = deviceCode.DeviceCode,
                Interval = _deviceCodeOptions.Interval,
                ExpiresIn = (int)(deviceCode.ExpiresAt - DateTimeOffset.Now).TotalSeconds
            });
        }
    }
}
