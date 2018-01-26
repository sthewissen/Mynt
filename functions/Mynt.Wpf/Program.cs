using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using Mynt.Core.Interfaces;
using Mynt.Extensibility;
using SimpleInjector;

namespace Mynt.Wpf
{
    class Program
    {
        [STAThread]
        static void Main()
        {
            var container = Bootstrap();

            RunApplication(container);
        }

        private static Container Bootstrap()
        {
            // Create the container as usual.
            var container = new Container();
            
            var tradingStrategyProviders = PluginLoader.Create<ITradingStrategyProvider>();

            // Register your windows and view models:
            container.Register<MainWindow>();

            return container;
        }

        private static void RunApplication(Container container)
        {
            try
            {
                var app = new App();
                var mainWindow = container.GetInstance<MainWindow>();
                app.Run(mainWindow);
            }
            catch (Exception ex)
            {
                //Log the exception and exit
            }
        }
    }
}
