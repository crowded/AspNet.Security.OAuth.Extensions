﻿/*
 * Licensed under the Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0)
 * See https://github.com/aspnet-contrib/AspNet.Security.OAuth.Extensions for more information
 * concerning the license and the contributors participating to this project.
 */

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Owin;
using Microsoft.Owin.BuilderProperties;
using Microsoft.Owin.Security.Infrastructure;
using Microsoft.Owin.Security.Interop;

namespace Owin.Security.OAuth.Validation
{
    public class OAuthValidationMiddleware : AuthenticationMiddleware<OAuthValidationOptions>
    {
        public OAuthValidationMiddleware(
            [NotNull] OwinMiddleware next,
            [NotNull] IDictionary<string, object> properties,
            [NotNull] OAuthValidationOptions options)
            : base(next, options)
        {
            if (Options.Events == null)
            {
                Options.Events = new OAuthValidationEvents();
            }

            if (options.DataProtectionProvider == null)
            {
                // Use the application name provided by the OWIN host as the Data Protection discriminator.
                // If the application name cannot be resolved, throw an invalid operation exception.
                var discriminator = new AppProperties(properties).AppName;
                if (string.IsNullOrEmpty(discriminator))
                {
                    throw new InvalidOperationException("The application name cannot be resolved from the OWIN application builder. " +
                                                        "Consider manually setting the 'DataProtectionProvider' property in the " +
                                                        "options using 'DataProtectionProvider.Create([unique application name])'.");
                }

                options.DataProtectionProvider = DataProtectionProvider.Create(discriminator);
            }

            if (options.AccessTokenFormat == null)
            {
                // Note: the following purposes must match the ones used by ASOS.
                var protector = options.DataProtectionProvider.CreateProtector(
                    "OpenIdConnectServerMiddleware", "ASOS", "Access_Token", "v1");

                options.AccessTokenFormat = new AspNetTicketDataFormat(new DataProtectorShim(protector));
            }

            if (options.Logger == null)
            {
                options.Logger = new LoggerFactory().CreateLogger<OAuthValidationMiddleware>();
            }
        }

        protected override AuthenticationHandler<OAuthValidationOptions> CreateHandler()
        {
            return new OAuthValidationHandler();
        }
    }
}
