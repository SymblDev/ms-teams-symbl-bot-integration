// <copyright file="Program.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// </copyright>

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https.Internal;
using Microsoft.Extensions.Configuration;
using PsiBot.Service.Settings;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Server.Kestrel.Https;

namespace PsiBot.Services
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseKestrel((ctx, opt) =>
                {
                    var config = new BotConfiguration();
                    ctx.Configuration.GetSection(nameof(BotConfiguration)).Bind(config);
                    config.Initialize();

                    opt.Configure()
                        .Endpoint("HTTPS", listenOptions =>
                        {
                            listenOptions.HttpsOptions.SslProtocols = SslProtocols.Tls12;
                        });

                    opt.ListenAnyIP(443, listenOptions =>
                    {
                        listenOptions.UseHttps(httpsOptions =>
                        {
                            var symbldotaiCert = CertificateLoader.LoadFromStoreCert(
                                config.CertificateSubjectName, "My", StoreLocation.LocalMachine,
                                allowInvalid: false);

                            httpsOptions.ServerCertificateSelector = (connectionContext, name) =>
                            {
                                return symbldotaiCert;
                            };
                        });
                    });
                    opt.ListenAnyIP(config.CallSignalingPort, listenOptions =>
                    {
                        listenOptions.UseHttps(httpsOptions =>
                        {
                            var symbldotaiCert = CertificateLoader.LoadFromStoreCert(
                                config.CertificateSubjectName, "My", StoreLocation.LocalMachine,
                                allowInvalid: false);

                            httpsOptions.ServerCertificateSelector = (connectionContext, name) =>
                            {
                                return symbldotaiCert;
                            };
                        });
                    });

                    opt.ListenAnyIP(config.CallSignalingPort + 1);
                });
    }
}
