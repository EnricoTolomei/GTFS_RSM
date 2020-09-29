using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AtacFeed
{
    static class Program
    {
        /// <summary>
        /// Punto di ingresso principale dell'applicazione.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File("logs\\GTFS_RSM_.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();
            Log.Information("Avvio Programma");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            FormGTFS_RSM formGTFS_RSM = new FormGTFS_RSM();
            Application.Run(formGTFS_RSM);

            if (formGTFS_RSM.needToRestart)
                Application.Restart();

            Log.Information("Uscita Programma");
            Log.CloseAndFlush();
        }
    }
}
