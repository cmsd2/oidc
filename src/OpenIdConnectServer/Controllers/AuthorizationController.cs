/*
 * Licensed under the Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0)
 * See https://github.com/openiddict/openiddict-core for more information concerning
 * the license and the contributors participating to this project.
 */

using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Primitives;
using AspNet.Security.OpenIdConnect.Server;
using OpenIdConnectServer.Models.Shared;
using OpenIdConnectServer.Models.AuthorizationViewModels;
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

namespace OpenIdConnectServer.Controllers
{
    public class AuthorizationController : Controller
    {
        private readonly OpenIddictApplicationManager<DynamoIdentityApplication> _applicationManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly DynamoDeviceCodeStore<DynamoIdentityDeviceCode> _deviceCodesStore;
        private readonly ApplicationAuthorizationManager<DynamoIdentityAuthorization> _authorizationManager;

        public AuthorizationController(
            OpenIddictApplicationManager<DynamoIdentityApplication> applicationManager,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            OpenIddictAuthorizationManager<DynamoIdentityAuthorization> authorizationManager,
            DynamoDeviceCodeStore<DynamoIdentityDeviceCode> deviceCodesStore)
        {
            _applicationManager = applicationManager;
            _signInManager = signInManager;
            _userManager = userManager;
            _authorizationManager = authorizationManager as ApplicationAuthorizationManager<DynamoIdentityAuthorization>;
            _deviceCodesStore = deviceCodesStore;
        }

        [Authorize, HttpGet("~/connect/authorize")]
        public async Task<IActionResult> Authorize(OpenIdConnectRequest request, CancellationToken cancellationToken)
        {
            Debug.Assert(request.IsAuthorizationRequest(),
                "The OpenIddict binder for ASP.NET Core MVC is not registered. " +
                "Make sure services.AddOpenIddict().AddMvcBinders() is correctly called.");

            // Retrieve the application details from the database.
            var application = await _applicationManager.FindByClientIdAsync(request.ClientId, HttpContext.RequestAborted);
            if (application == null)
            {
                return View("Error", new ErrorViewModel
                {
                    Error = OpenIdConnectConstants.Errors.InvalidClient,
                    ErrorDescription = "Details concerning the calling client application cannot be found in the database"
                });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return View("Error", new ErrorViewModel
                {
                    Error = OpenIdConnectConstants.Errors.ServerError,
                    ErrorDescription = "An internal error has occurred"
                });
            }

            var authorization = await _authorizationManager.FindAsync(user.Id, application.Id, cancellationToken);
            if (authorization != null)
            {
                // if we didn't ask for any scopes that aren't already authorized
                if (false == request.GetScopes().Except(authorization.Scopes).Any())
                {
                    var ticket = await CreateTicketAsync(request, user);

                    return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
                }
            }

            // Flow the request_id to allow OpenIddict to restore
            // the original authorization request from the cache.
            return View(new AuthorizeViewModel
            {
                ApplicationName = application.DisplayName,
                RequestId = request.RequestId,
                Scope = request.Scope,
                ClientId = request.ClientId,
                ResponseType = request.ResponseType,
                ResponseMode = request.ResponseMode,
                RedirectUri = request.RedirectUri,
                State = request.State,
                Nonce = request.Nonce
            });
        }

        [Authorize, HttpGet("~/connect/authorize_device")]
        public IActionResult ConnectDeviceCode()
        {
            return View();
        }

        [Authorize]
        [HttpPost("~/connect/authorize_device"), ValidateAntiForgeryToken]
        public async Task<IActionResult> ConnectDeviceCodeConfirm(AuthorizeDeviceCodeViewModel model, CancellationToken cancellationToken)
        {
            var deviceCode = await _deviceCodesStore.FindByUserCodeAsync(model.UserCode, cancellationToken);
            if (deviceCode == null)
            {
                ModelState.AddModelError(string.Empty, "Unrecognised or expired code.");
                return View("ConnectDeviceCode", model);
            }

            var application = await _applicationManager.FindByIdAsync(deviceCode.Application, HttpContext.RequestAborted);
            if (application == null)
            {
                return View("Error", new ErrorViewModel
                {
                    Error = OpenIdConnectConstants.Errors.InvalidClient,
                    ErrorDescription = "Details concerning the calling client application cannot be found in the database"
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
        public async Task<IActionResult> AcceptDevice(AuthorizeDeviceCodeViewModel request, CancellationToken cancellationToken)
        {
            var deviceCode = await _deviceCodesStore.FindByUserCodeAsync(request.UserCode, cancellationToken);
            if (deviceCode == null)
            {
                ModelState.AddModelError(string.Empty, "Unrecognised or expired code.");
                return View("ConnectDeviceCode", request);
            }

            var application = await _applicationManager.FindByIdAsync(deviceCode.Application, HttpContext.RequestAborted);
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
            var ticket = await CreateTicketAsync(request, user);
            
            var authorization = await _authorizationManager.FindAsync(user.Id, application.Id, cancellationToken);
            if (authorization != null)
            {
                if (false == request.GetScopes().Except(authorization.Scopes).Any())
                {
                    authorization.Scopes = authorization.Scopes.Union(request.GetScopes()).ToList();

                    await _authorizationManager.UpdateAsync(authorization, cancellationToken);
                }
            }
            else
            {
                authorization = new DynamoIdentityAuthorization()
                {
                    Application = application.Id,
                    Subject = user.Id,
                    Scopes = ticket.GetScopes().ToList()
                };

                await _authorizationManager.CreateAsync(authorization, cancellationToken);
            }

            await _deviceCodesStore.Authorize(deviceCode, user.Id, cancellationToken);


            return View("AuthorizedDeviceCode", new AuthorizedDeviceResultViewModel
            {
                ApplicationName = application.DisplayName,
                Scope = string.Join(" ", deviceCode.Scopes),
                Authorized = true
            });
        }

        [Authorize, FormValueRequired("submit.Deny")]
        [HttpPost("~/connect/device_code_authorization"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DenyDevice(AuthorizeDeviceCodeViewModel request, CancellationToken cancellationToken)
        {
            var deviceCode = await _deviceCodesStore.FindByUserCodeAsync(request.UserCode, cancellationToken);
            if (deviceCode == null)
            {
                ModelState.AddModelError(string.Empty, "Unrecognised or expired code.");
                return View("ConnectDeviceCode", request);
            }

            var application = await _applicationManager.FindByIdAsync(deviceCode.Application, HttpContext.RequestAborted);
            if (application == null)
            {
                return View("Error", new ErrorViewModel
                {
                    Error = OpenIdConnectConstants.Errors.InvalidClient,
                    ErrorDescription = "Details concerning the calling client application cannot be found in the database"
                });
            }

            await _deviceCodesStore.Revoke(deviceCode.Id, cancellationToken);

            // Notify OpenIddict that the authorization grant has been denied by the resource owner
            // to redirect the user agent to the client application using the appropriate response_mode.
            return View("AuthorizedDeviceCode", new AuthorizedDeviceResultViewModel
            {
                ApplicationName = application.DisplayName,
                Scope = string.Join(" ", deviceCode.Scopes),
                Authorized = false
            });
        }

        [Authorize, FormValueRequired("submit.Accept")]
        [HttpPost("~/connect/authorize"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Accept(OpenIdConnectRequest request, CancellationToken cancellationToken)
        {
            Debug.Assert(request.IsAuthorizationRequest(),
                "The OpenIddict binder for ASP.NET Core MVC is not registered. " +
                "Make sure services.AddOpenIddict().AddMvcBinders() is correctly called.");

            var application = await _applicationManager.FindByClientIdAsync(request.ClientId, HttpContext.RequestAborted);
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
            var ticket = await CreateTicketAsync(request, user);

            var authorization = await _authorizationManager.FindAsync(user.Id, application.Id, cancellationToken);
            if (authorization != null)
            {
                if (false == request.GetScopes().Except(authorization.Scopes).Any())
                {
                    authorization.Scopes = authorization.Scopes.Union(request.GetScopes()).ToList();

                    await _authorizationManager.UpdateAsync(authorization, cancellationToken);
                }
            }
            else
            {
                authorization = new DynamoIdentityAuthorization()
                {
                    Application = application.Id,
                    Subject = user.Id,
                    Scopes = ticket.GetScopes().ToList()
                };

                await _authorizationManager.CreateAsync(authorization, cancellationToken);
            }

            // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
            return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
        }

        [Authorize, FormValueRequired("submit.Deny")]
        [HttpPost("~/connect/authorize"), ValidateAntiForgeryToken]
        public IActionResult Deny()
        {
            // Notify OpenIddict that the authorization grant has been denied by the resource owner
            // to redirect the user agent to the client application using the appropriate response_mode.
            return Forbid(OpenIdConnectServerDefaults.AuthenticationScheme);
        }

        [HttpGet("~/connect/logout")]
        public IActionResult Logout(OpenIdConnectRequest request)
        {
            // Flow the request_id to allow OpenIddict to restore
            // the original logout request from the distributed cache.
            return View(new LogoutViewModel
            {
                RequestId = request.RequestId,
                PostLogoutRedirectUri = request.PostLogoutRedirectUri,
                IdTokenHint = request.IdTokenHint,
                State = request.State
            });
        }

        [HttpPost("~/connect/logout"), ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Ask ASP.NET Core Identity to delete the local and external cookies created
            // when the user agent is redirected from the external identity provider
            // after a successful authentication flow (e.g Google or Facebook).
            await _signInManager.SignOutAsync();

            // Returning a SignOutResult will ask OpenIddict to redirect the user agent
            // to the post_logout_redirect_uri specified by the client application.
            return SignOut(OpenIdConnectServerDefaults.AuthenticationScheme);
        }

        [HttpPost("~/connect/device_token"), Produces("application/json")]
        public async Task<IActionResult> MintDeviceCode(string response_type, string client_id, string client_secret, string scope, CancellationToken cancellationToken)
        {
            var application = await _applicationManager.FindByClientIdAsync(client_id, HttpContext.RequestAborted);
            if (application == null)
            {
                return View("Error", new ErrorViewModel
                {
                    Error = OpenIdConnectConstants.Errors.InvalidClient,
                    ErrorDescription = "Details concerning the calling client application cannot be found in the database"
                });
            }

            var deviceCode = await _deviceCodesStore.CreateAsync(application.Id, scope.Split(' ').ToList(), cancellationToken);

            // issue user and device codes
            return Json(new DeviceCodeFlowViewModel
            {
                VerificationUri = Url.Action("AuthorizeDevice"),
                UserCode = deviceCode.UserCode,
                DeviceCode = deviceCode.DeviceCode,
                Interval = 3
            });
        }

        [HttpPost("~/connect/token"), Produces("application/json")]
        public async Task<IActionResult> Exchange(string device_code, OpenIdConnectRequest request, CancellationToken cancellationToken)
        {
            Debug.Assert(request.IsTokenRequest(),
                "The OpenIddict binder for ASP.NET Core MVC is not registered. " +
                "Make sure services.AddOpenIddict().AddMvcBinders() is correctly called.");
            
            if (request.IsDeviceCodeGrantType())
            {
                if (request.ClientId == null)
                {
                    return BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.InvalidClient,
                        ErrorDescription = "Missing required parameter client_id."
                    });
                }

                // exchange device code for tokens
                var application = await _applicationManager.FindByClientIdAsync(request.ClientId, HttpContext.RequestAborted);
                if (application == null)
                {
                    return View("Error", new ErrorViewModel
                    {
                        Error = OpenIdConnectConstants.Errors.InvalidClient,
                        ErrorDescription = "Details concerning the calling client application cannot be found in the database"
                    });
                }

                var deviceCode = await _deviceCodesStore.FindByDeviceCodeAsync(device_code, cancellationToken);
                var user = await _userManager.FindByIdAsync(deviceCode.Subject);

                await _deviceCodesStore.Revoke(deviceCode.Id, cancellationToken);

                // Ensure the user is still allowed to sign in.
                if (!await _signInManager.CanSignInAsync(user))
                {
                    return BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.InvalidGrant,
                        ErrorDescription = "The user is no longer allowed to sign in."
                    });
                }

                var ticket = await CreateTicketAsync(new OpenIdConnectRequest
                {
                    Scope = string.Join(" ", deviceCode.Scopes),
                    ClientId = request.ClientId,
                    ClientSecret = request.ClientSecret,
                    GrantType = request.GrantType
                }, user);

                return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
            }
            else if (request.IsPasswordGrantType())
            {
                // not implemented
            }
            else if (request.IsAuthorizationCodeGrantType())
            {
                // Retrieve the claims principal stored in the authorization code.
                var info = await HttpContext.Authentication.GetAuthenticateInfoAsync(
                    OpenIdConnectServerDefaults.AuthenticationScheme);

                // Retrieve the user profile corresponding to the authorization code.
                var user = await _userManager.GetUserAsync(info.Principal);
                if (user == null)
                {
                    return BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.InvalidGrant,
                        ErrorDescription = "The authorization code is no longer valid."
                    });
                }

                // Ensure the user is still allowed to sign in.
                if (!await _signInManager.CanSignInAsync(user))
                {
                    return BadRequest(new OpenIdConnectResponse
                    {
                        Error = OpenIdConnectConstants.Errors.InvalidGrant,
                        ErrorDescription = "The user is no longer allowed to sign in."
                    });
                }

                // Create a new authentication ticket, but reuse the properties stored
                // in the authorization code, including the scopes originally granted.
                var ticket = await CreateTicketAsync(request, user, info.Properties);

                return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
            }

            return BadRequest(new OpenIdConnectResponse
            {
                Error = OpenIdConnectConstants.Errors.UnsupportedGrantType,
                ErrorDescription = "The specified grant type is not supported."
            });
        }

        private async Task<AuthenticationTicket> CreateTicketAsync(
            OpenIdConnectRequest request, ApplicationUser user,
            AuthenticationProperties properties = null)
        {
            // Create a new ClaimsPrincipal containing the claims that
            // will be used to create an id_token, a token or a code.
            var principal = await _signInManager.CreateUserPrincipalAsync(user);

            // Note: by default, claims are NOT automatically included in the access and identity tokens.
            // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
            // whether they should be included in access tokens, in identity tokens or in both.

            foreach (var claim in principal.Claims)
            {
                // In this sample, every claim is serialized in both the access and the identity tokens.
                // In a real world application, you'd probably want to exclude confidential claims
                // or apply a claims policy based on the scopes requested by the client application.
                claim.SetDestinations(OpenIdConnectConstants.Destinations.AccessToken,
                                      OpenIdConnectConstants.Destinations.IdentityToken);
            }

            // Create a new authentication ticket holding the user identity.
            var ticket = new AuthenticationTicket(principal, properties,
                OpenIdConnectServerDefaults.AuthenticationScheme);

            if (!request.IsAuthorizationCodeGrantType() && !request.IsRefreshTokenGrantType())
            {
                // Set the list of scopes granted to the client application.
                // Note: the offline_access scope must be granted
                // to allow OpenIddict to return a refresh token.
                ticket.SetScopes(new[]
                {
                    OpenIdConnectConstants.Scopes.OpenId,
                    OpenIdConnectConstants.Scopes.Email,
                    OpenIdConnectConstants.Scopes.Profile,
                    OpenIddictConstants.Scopes.Roles
                }.Intersect(request.GetScopes()));
            }

            ticket.SetResources("resource_server");

            return ticket;
        }
    }
}