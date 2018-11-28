using Castle.MicroKernel.Registration;
using Core.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Abp.Dependency
{
    public static class ServiceEndpointSelectorIocManagerExtensions
    {
        public static IIocManager AddMinimumServiceEndpointSelectorAsDefault(this IIocManager iocManager)
        {
            iocManager.IocContainer.Register(
                   Component
                   .For<IServiceEndpointSelector>()
                   .ImplementedBy<MinimumConnectionServiceEndpointSelector>()
                   .NamedAutomatically(nameof(MinimumConnectionServiceEndpointSelector))
                   .LifestyleSingleton()
                   .IsDefault()
                   );
            return iocManager;
        }
    }
}
