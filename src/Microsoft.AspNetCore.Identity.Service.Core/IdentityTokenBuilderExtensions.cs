﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Identity.Service;
using Microsoft.AspNetCore.Identity.Service.Internal;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class IdentityTokenBuilderExtensions
    {
        public static IIdentityClientApplicationsBuilder AddSigningCertificate(
            this IIdentityClientApplicationsBuilder builder,
            X509Certificate2 certificate)
        {
            CryptographyHelpers.ValidateRsaKeyLength(certificate);
            var key = new X509SecurityKey(certificate);
            builder.Services.Configure<ApplicationTokenOptions>(
                options =>
                {
                    var algorithm = CryptographyHelpers.FindAlgorithm(certificate);
                    options.SigningKeys.Add(new SigningCredentials(key, algorithm));
                });

            return builder;
        }

        public static IIdentityClientApplicationsBuilder AddSigningCertificates(
            this IIdentityClientApplicationsBuilder builder,
            IEnumerable<X509Certificate2> certificates)
        {
            foreach (var certificate in certificates)
            {
                builder.AddSigningCertificate(certificate);
            }

            return builder;
        }

        public static IIdentityClientApplicationsBuilder AddSigningCertificates(
            this IIdentityClientApplicationsBuilder builder,
            Func<IEnumerable<X509Certificate2>> certificatesLoader)
        {
            builder.Services.Configure<ApplicationTokenOptions>(o =>
            {
                var certificates = certificatesLoader();
                foreach (var certificate in certificates)
                {
                    var algorithm = CryptographyHelpers.FindAlgorithm(certificate);
                    o.SigningKeys.Add(new SigningCredentials(new X509SecurityKey(certificate), algorithm));
                }
            });

            return builder;
        }

        public static IIdentityClientApplicationsBuilder DisableDeveloperCertificate(this IIdentityClientApplicationsBuilder builder)
        {
            var services = builder.Services;
            foreach (var service in services.ToList())
            {
                if (service.ImplementationType == typeof(DeveloperCertificateSigningCredentialsSource))
                {
                    services.Remove(service);
                }
            }

            return builder;
        }

        public static IIdentityClientApplicationsBuilder AddSigningCertificate(this IIdentityClientApplicationsBuilder builder, Func<X509Certificate2> func)
        {
            var cert = func();
            if (cert == null)
            {
                return builder;
            }
            else
            {
                return builder.AddSigningCertificate(cert);
            }
        }
    }
}
