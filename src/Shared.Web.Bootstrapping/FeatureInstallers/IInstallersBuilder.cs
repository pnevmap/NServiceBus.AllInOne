using System;
using Microsoft.Extensions.Configuration;

namespace Shared.Web.Bootstrapping.FeatureInstallers
{
    public interface IInstallersBuilder
    {
        Installers Build(Type startupType, IConfiguration configuration);
    }
}