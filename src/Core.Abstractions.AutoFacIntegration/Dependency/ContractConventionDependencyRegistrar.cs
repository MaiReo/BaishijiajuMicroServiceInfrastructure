using Core.Abstractions.Dependency;
using System.Reflection;

namespace Autofac
{

    /// <summary>
    /// 按约定注册服务
    /// </summary>
    public static class ContractConventionDependencyRegistrar
    {
        /// <summary>
        /// 从包含<typeparamref name="T"/>类型的程序集扫描服务注册到容器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        public static void RegisterAssemblyByConvention(this ContainerBuilder builder, Assembly assembly)
        {
            builder.RegisterAssemblyTypes(assembly)
                   .AssignableTo<ILifestyleSingleton>()
                   .AsImplementedInterfaces()
                   .AsSelf()
                   .PropertiesAutowired()
                   .SingleInstance();

            builder.RegisterAssemblyTypes(assembly)
                   .AssignableTo<ILifestyleTransient>()
                   .AsImplementedInterfaces()
                   .AsSelf()
                   .PropertiesAutowired()
                   .InstancePerDependency();

            builder.RegisterAssemblyTypes(assembly)
                  .AssignableTo<ILifestyleSingletonSelf>()
                  .AsSelf()
                  .PropertiesAutowired()
                  .SingleInstance();

            builder.RegisterAssemblyTypes(assembly)
                   .AssignableTo<ILifestyleTransientSelf>()
                   .AsSelf()
                   .PropertiesAutowired()
                   .InstancePerDependency();
        }
    }
}
