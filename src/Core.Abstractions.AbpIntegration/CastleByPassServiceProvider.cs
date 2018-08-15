using System;
using System.Collections.Generic;
using System.Text;
using Castle.MicroKernel;

namespace Core.Abstractions.AbpIntegration
{
    public class CastleByPassServiceProvider : IServiceProvider
    {
        private readonly IKernel _kernel;

        public CastleByPassServiceProvider(IKernel kernel)
        {
            this._kernel = kernel;
        }

        public object GetService(Type serviceType)
        {
            return this._kernel.Resolve(serviceType);
        }
    }
}
