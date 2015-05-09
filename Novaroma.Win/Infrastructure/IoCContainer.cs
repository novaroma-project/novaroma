using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Autofac;
using Autofac.Extras.Quartz;
using Db4objects.Db4o;
using Novaroma.Engine;
using Novaroma.Engine.Jobs;
using Novaroma.Interface;
using Novaroma.Win.Utilities;
using Novaroma.Win.ViewModels;
using Novaroma.Win.Views;

namespace Novaroma.Win.Infrastructure {

    public static class IoCContainer {
        public static IContainer BaseContainer { get; private set; }

        public static void Build() {
            if (BaseContainer == null) {
                var builder = new ContainerBuilder();

                var location = Environment.CurrentDirectory;
                var pluginPath = Path.Combine(location, "Plugins");
                var assemblies = new List<Assembly> { Assembly.GetAssembly(typeof(Services.ServiceNames)), Assembly.GetExecutingAssembly() };
                var pluginDirInfo = new DirectoryInfo(pluginPath);
                if (pluginDirInfo.Exists) {
                    var dllFiles = pluginDirInfo.GetFiles("*.dll", SearchOption.AllDirectories);
                    foreach (var dllFile in dllFiles) {
                        try {
                            var assembly = Assembly.LoadFile(dllFile.FullName);
                            assemblies.Add(assembly);
                        }
                        // ReSharper disable once EmptyGeneralCatchClause
                        catch {
                        }
                    }
                }
                assemblies.ForEach(a => RegisterAssemblyServices(builder, a));

                builder.Register(c => {
                    var path = Path.Combine(location, "Novaroma.db");
                    return Db4oEmbedded.OpenFile(path);
                })
                    .As<IEmbeddedObjectContainer>()
                    .SingleInstance();

                builder.RegisterType<NovaromaDb4OContext>()
                    .As<INovaromaContext>()
                    .AsSelf()
                    .SingleInstance();

                builder.RegisterType<Db4OContextFactory>()
                    .As<IContextFactory>()
                    .SingleInstance();

                builder.RegisterType<DbLogger>()
                    .As<ILogger>()
                    .SingleInstance();

                builder.RegisterType<BalloonExceptionHandler>()
                    .As<IExceptionHandler>()
                    .SingleInstance();

                builder.RegisterType<MetroDialogService>()
                    .As<IDialogService>()
                    .SingleInstance();

                builder.RegisterType<NovaromaEngine>()
                    .As<INovaromaEngine>()
                    .AsSelf()
                    .SingleInstance();

                builder.RegisterType<ShellService>()
                    .As<IShellService>()
                    .AsSelf()
                    .SingleInstance();

                builder.RegisterType<WebUIService>()
                    .As<IWebUIService>()
                    .AsSelf()
                    .SingleInstance();

                builder.RegisterModule(new QuartzAutofacFactoryModule());
                builder.RegisterModule(new QuartzAutofacJobsModule(typeof(DownloadJob).Assembly));

                builder.RegisterType<MainViewModel>()
                    .SingleInstance();
                builder.RegisterType<MainWindow>()
                    .SingleInstance();

                builder.RegisterType<NotifyIconViewModel>()
                    .SingleInstance();

                BaseContainer = builder.Build();
            }
        }

        public static TService Resolve<TService>() {
            return BaseContainer.Resolve<TService>();
        }

        private static void RegisterAssemblyServices(ContainerBuilder builder, Assembly assembly) {
            builder.RegisterAssemblyTypes(assembly)
                .Where(t => typeof(INovaromaService).IsAssignableFrom(t))
                .As(t => t.GetInterfaces())
                .AsSelf()
                .SingleInstance();
        }
    }
}
