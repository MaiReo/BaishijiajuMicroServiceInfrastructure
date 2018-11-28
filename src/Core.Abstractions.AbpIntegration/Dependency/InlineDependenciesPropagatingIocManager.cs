using Abp.Dependency;
using Castle.Core;
using Castle.DynamicProxy;
using Castle.MicroKernel;
using Castle.MicroKernel.Context;
using Castle.MicroKernel.Resolvers;
using Castle.Windsor;
using Castle.Windsor.Installer;
using Castle.Windsor.Proxy;
using Core.Session;
using System;

namespace Core.Dependency
{
    public class InlineDependenciesPropagatingIocManager : IocManager, IIocManager, IIocResolver
    {
        private static readonly ProxyGenerator ProxyGeneratorInstance = new ProxyGenerator();

        protected override IWindsorContainer CreateContainer()
        {
            var resolver = new InlineDependenciesPropagatingDependencyResolver();
            resolver.AddSubResolver(new CoreSessionProviderDependencyResolver(resolver));
            return new WindsorContainer(new DefaultKernel(
                 resolver,
                 new DefaultProxyFactory(ProxyGeneratorInstance)
                 ), new DefaultComponentInstaller());
        }

        public class CoreSessionProviderDependencyResolver : ISubDependencyResolver
        {
            private InlineDependenciesPropagatingDependencyResolver _resolver;

            public CoreSessionProviderDependencyResolver(InlineDependenciesPropagatingDependencyResolver resolver)
            {
                this._resolver = resolver;
            }


            public bool CanResolve(CreationContext context, ISubDependencyResolver contextHandlerResolver, ComponentModel model, DependencyModel dependency)
            {
                if (typeof(ICoreSession).IsAssignableFrom(dependency.TargetItemType) || typeof(ICoreSessionProvider).IsAssignableFrom(dependency.TargetItemType))
                {
                    return true;
                    //return context.HasAdditionalArguments;
                }

                return false;
            }

            public object Resolve(CreationContext context, ISubDependencyResolver contextHandlerResolver, ComponentModel model, DependencyModel dependency)
            {
                if (context.HasAdditionalArguments)
                {
                    if (typeof(ICoreSession).IsAssignableFrom(dependency.TargetItemType))
                    {
                        return context.AdditionalArguments[typeof(ICoreSession)];
                    }
                    if (typeof(ICoreSessionProvider).IsAssignableFrom(dependency.TargetItemType))
                    {
                        return context.AdditionalArguments[typeof(ICoreSessionProvider)];
                    }
                }
                if (context.CanResolve(context,contextHandlerResolver,model,dependency))
                {
                    return context.Resolve(context, contextHandlerResolver, model, dependency);
                }
                if (contextHandlerResolver.CanResolve(context, contextHandlerResolver, model, dependency))
                {
                    return contextHandlerResolver.Resolve(context, contextHandlerResolver, model, dependency);
                }

                return _resolver.ResolveFromKernel(context, model, dependency);
            }
        }

        public class InlineDependenciesPropagatingDependencyResolver : DefaultDependencyResolver
        {
            protected override CreationContext RebuildContextForParameter(
                CreationContext current, Type parameterType)
            {
                //if (parameterType.ContainsGenericParameters)
                //{
                //    return current;
                //}

                return new CreationContext(parameterType, current, true);
            }



            public new object ResolveFromKernel(CreationContext context, ComponentModel model, DependencyModel dependency)
            {
                return base.ResolveFromKernel(context, model, dependency);
            }
        }
    }
}
