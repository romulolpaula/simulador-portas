using System.Windows;
using WpfApp1.Banco_de_Dados;

namespace WpfApp1
{
    public partial class App : Application
    {
        public static string CurrentUsername { get; set; }
        public static int CurrentUserId { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Database.InicializarBanco();
        }
    }
}
