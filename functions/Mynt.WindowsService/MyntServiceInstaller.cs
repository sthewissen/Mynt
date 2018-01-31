using Mynt.WindowsService;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace MyService
{
    [RunInstaller(true)]
    public class MyServiceInstaller : Installer
    {
        public MyServiceInstaller()
        {
            var spi = new ServiceProcessInstaller();
            var si = new ServiceInstaller();

            spi.Account = ServiceAccount.LocalSystem;
            spi.Username = null;
            spi.Password = null;

            si.DisplayName = Program.ServiceName;
            si.ServiceName = Program.ServiceName;
            si.Description = "Mynt Crypto Trader";
            si.StartType = ServiceStartMode.Automatic;

            Installers.Add(spi);
            Installers.Add(si);
        }
    }
}