#pragma warning disable IDE0130 // Namespace does not match folder structure
// MIT - Florian Grimm

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection {
    public static class LocalHostAuthenticationExtension {
        public static AuthenticationBuilder AddLocalHostAuthentication(
            this AuthenticationBuilder builder
            ) {
            builder.AddScheme<LocalHostAuthenticationSchemeOptions, LocalHostAuthenticationHandler>(
                LocalHostDefaults.AuthenticationScheme, null);
            return builder;
        }
    }
}

namespace Brimborium.Tracerit.Service {
    public class LocalHostDefaults {
        public const string AuthenticationScheme = "LocalHost";
    }

    public class LocalHostAuthenticationSchemeOptions
        : AuthenticationSchemeOptions {
    }

    public class LocalHostAuthenticationHandler
        : Microsoft.AspNetCore.Authentication.AuthenticationHandler<LocalHostAuthenticationSchemeOptions> {
        public LocalHostAuthenticationHandler(
            IOptionsMonitor<LocalHostAuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder
            ) : base(options, logger, encoder) { }
        protected override Task<AuthenticateResult> HandleAuthenticateAsync() {
            if (this.Context.Connection.RemoteIpAddress is { } remoteIpAddress) {
                if (remoteIpAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
                    if (IPAddress.Loopback.Equals(remoteIpAddress)) {
                        return Task.FromResult<AuthenticateResult>(
                            AuthenticateResult.Success(
                                new AuthenticationTicket(
                                    new System.Security.Claims.ClaimsPrincipal(
                                        new System.Security.Claims.ClaimsIdentity(
                                            new System.Security.Claims.Claim[] {
                                                new System.Security.Claims.Claim(
                                                    ClaimTypes.Upn,
                                                    "localhost"),
                                                new System.Security.Claims.Claim(
                                                    ClaimTypes.NameIdentifier,
                                                    "localhost")
                                            })),
                                    LocalHostDefaults.AuthenticationScheme)));
                    }
                }
                if (remoteIpAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6) {
                    if (IPAddress.IPv6Loopback.Equals(remoteIpAddress)) {
                        return Task.FromResult<AuthenticateResult>(
                            AuthenticateResult.Success(
                                new AuthenticationTicket(
                                    new System.Security.Claims.ClaimsPrincipal(
                                        new System.Security.Claims.ClaimsIdentity(
                                            new System.Security.Claims.Claim[] {
                                                new System.Security.Claims.Claim(
                                                    ClaimTypes.Upn,
                                                    "localhost"),
                                                new System.Security.Claims.Claim(
                                                    ClaimTypes.NameIdentifier,
                                                    "localhost")
                                            })),
                                    LocalHostDefaults.AuthenticationScheme)));
                    }
                }
            }
            return Task.FromResult<AuthenticateResult>(AuthenticateResult.NoResult());
        }
    }
}
