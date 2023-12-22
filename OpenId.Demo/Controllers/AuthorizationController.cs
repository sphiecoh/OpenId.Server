using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace OpenId.Demo.Controllers;

public class AuthorizationController : Controller
{
    [HttpPost("~/token")]
    public async ValueTask<IActionResult> Exchange()
    {
        //retrieve OIDC request from original request
        var request = HttpContext.GetOpenIddictServerRequest() ??
                      throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        if (request.IsClientCredentialsGrantType())
        {
            var clientId = request.ClientId;
            var identity = new ClaimsIdentity(authenticationType: TokenValidationParameters.DefaultAuthenticationType);

            identity.SetClaim(OpenIddictConstants.Claims.Subject, clientId);
            identity.SetScopes(request.GetScopes());
            var principal = new ClaimsPrincipal(identity);
            // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }
        if (request.IsRefreshTokenGrantType())
        {
            var claimsPrincipal = (await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;

            return SignIn(claimsPrincipal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        throw new NotImplementedException("The specified grant type is not implemented.");
    }
}