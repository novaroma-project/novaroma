using System;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Navigation;
using Autofac.Integration.Wcf;
using Hardcodet.Wpf.TaskbarNotification;
using Novaroma.Interface;
using Novaroma.Win.Infrastructure;
using Novaroma.Win.ViewModels;
using Novaroma.Win.Views;

namespace Novaroma.Win {

    public partial class App {
        private static readonly Lazy<ObjectDataProvider> _resourceProvider = new Lazy<ObjectDataProvider>(() => (ObjectDataProvider)Current.FindResource("Resources"));
        private static ServiceHost _serviceHost;
        public static TaskbarIcon NotifyIcon;

        private async void App_OnStartup(object sender, StartupEventArgs e) {
            var client = Novaroma.Helper.CreateShellServiceClient();
            bool createdNew;
            try {
                client.Test();
                createdNew = false;
            }
            catch {
                createdNew = true;
            }

            if (!createdNew) {
                if (e.Args.Length > 0)
                    await client.HandleExeArgs(e.Args);
                else
                    await client.ShowMainWindow();

                Current.Shutdown();
                return;
            }

            IoCContainer.Build();

            var engine = IoCContainer.Resolve<INovaromaEngine>();
            engine.LanguageChanged += EngineOnLanguageChanged;

            _serviceHost = new ServiceHost(typeof(ShellService), new Uri(Constants.NetPipeUri));
            var binding = new NetNamedPipeBinding {
                MaxReceivedMessageSize = 20000000,
                MaxBufferPoolSize = 20000000,
                MaxBufferSize = 20000000
            };
            _serviceHost.AddServiceEndpoint(typeof(IShellService), binding, Constants.NetPipeEndpointName);
            _serviceHost.AddDependencyInjectionBehavior<IShellService>(IoCContainer.BaseContainer);
            _serviceHost.Open();

            var mainWindow = IoCContainer.Resolve<MainWindow>();
            var mainViewModel = IoCContainer.Resolve<MainViewModel>();
            await mainViewModel.ListData();
            if (!e.Args.Contains("StartHidden"))
                mainWindow.Show();

            NotifyIcon = (TaskbarIcon)FindResource("NotifyIcon");
            if (NotifyIcon != null)
                NotifyIcon.DataContext = IoCContainer.Resolve<NotifyIconViewModel>();

            if (e.Args.Length > 0) {
                var service = IoCContainer.Resolve<IShellService>();
                await service.HandleExeArgs(e.Args);
            }
        }

        private static void EngineOnLanguageChanged(object sender, EventArgs e) {
            _resourceProvider.Value.Refresh();
        }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e) {
            Process.Start(e.Uri.ToString());
        }

        public static ServiceHost ServiceHost {
            get { return _serviceHost; }
        }
    }
}
