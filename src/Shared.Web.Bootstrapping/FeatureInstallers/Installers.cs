using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Web.Bootstrapping.FeatureInstallers
{
    public class Installers : 
        IReadOnlyList<IInstaller>,
        IConfigureKestrel,
        IRegisterServices,
        IBuildMvc,
        IInstallMiddleware
    {
        private readonly List<IInstaller> _installers = new List<IInstaller>();

        public int Order => 0;
        
        public IEnumerator<IInstaller> GetEnumerator()
        {
            return _installers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => _installers.Count;

        public IInstaller this[int index] => _installers[index];

        public T Get<T>() where T : class, IInstaller
        {
            foreach (var installer in this)
            {
                if (installer is T expected)
                    return expected;
            }

            return null;
        }

        public List<IConfigureKestrel> GetAllConfigureKestrel()
        {
            return GetAllImplementing<IConfigureKestrel>()
                .GroupBy(x => x.Order)
                .OrderBy(x => x.Key)
                .SelectMany(x => x)
                .ToList();
        }

        public List<IRegisterServices> GetAllRegisterServices()
        {
            return GetAllImplementing<IRegisterServices>()
                .GroupBy(x => x.Order)
                .OrderBy(x => x.Key)
                .SelectMany(x => x)
                .ToList();
        }

        public List<IBuildMvc> GetAllBuildMvc()
        {
            return GetAllImplementing<IBuildMvc>()
                .GroupBy(x => x.Order)
                .OrderBy(x => x.Key)
                .SelectMany(x => x)
                .ToList();
        }

        public List<IInstallMiddleware> GetAllInstallMiddleware()
        {
            return GetAllImplementing<IInstallMiddleware>()
                .GroupBy(x => x.Order)
                .OrderBy(x => x.Key)
                .SelectMany(x => x)
                .ToList();
        }
        
        public void Add(IInstaller installer)
        {
            _installers.Add(installer);
        }

        public void ConfigureKestrel(KestrelServerOptions options)
        {
            foreach (var installer in GetAllConfigureKestrel())
            {
                installer.ConfigureKestrel(options);
            }
        }

        public void RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            foreach (var installer in GetAllRegisterServices())
            {
                installer.RegisterServices(services, configuration);
            }
        }

        public void BuildMvc(IMvcBuilder mvcBuilder, IConfiguration configuration)
        {
            foreach (var installer in GetAllBuildMvc())
            {
                installer.BuildMvc(mvcBuilder, configuration);
            }
        }

        public void InstallMiddleware(IApplicationBuilder app, IServiceProvider serviceProvider)
        {
            foreach (var installer in GetAllInstallMiddleware())
            {
                installer.InstallMiddleware(app, serviceProvider);
            }
        }
 
        private IReadOnlyCollection<T> GetAllImplementing<T>()
        {
            var result = new List<T>();

            foreach (var installer in this)
            {
                if(installer is T expected)
                    result.Add(expected);
            }
            
            return result
                .AsReadOnly();
        }
    }
}