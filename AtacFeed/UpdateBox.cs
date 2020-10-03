using Octokit;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace AtacFeed
{
    public partial class UpdateBox : Form
    {
        public  UpdateBox()
        {
            InitializeComponent();
            buttonRestart.DialogResult = DialogResult.Yes;            
        }

        public async void CheckVersion() {
            Version latestVersion = await GetLatestVersionAcync();
            Version actualVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            labelActualVersion.Text = $"Versione in uso {actualVersion.Major}.{actualVersion.Minor:00}";
            labelLastVersion.Text = $"Ultima versione {latestVersion.Major}.{latestVersion.Minor:00}";


            if (latestVersion == new Version()) {
                labelUpdateProgram.Text = "Problemi di connessione con il server GitHub";
            }
            else if (actualVersion < latestVersion)
            {
                labelUpdateProgram.Text = "E' disponibile una nuova versione del programma";
            }
            else {
                labelUpdateProgram.Text = "Stai utilizzando l'ultima versione";
            }            
        }
        
        public async void CheckConf() {
            try
            {
                
                decimal latestVersion = await GetLatestConfVersionAcync();
                decimal actualVersion = 0;
                //labelActualVersion.Text = String.Format("Versione in uso {0}", actualVersion);            
                //labelLastVersion.Text = String.Format("Ultima Release {0}", latestVersion);
                string filepath = $"Config{Path.DirectorySeparatorChar}GTFS_Static{Path.DirectorySeparatorChar}LatestVersion.txt";
                if (File.Exists(filepath)){
                    string localVersion = File.ReadAllLines(filepath).FirstOrDefault();
                    decimal.TryParse(localVersion, out actualVersion);
                }


                if (actualVersion < latestVersion)
                {
                    labelUpdateConf.Text = "E' disponibile una nuova versione dei file di configurazione";
                    labelConfResult.Text = "E' consigliabile recuperare gli ultimi file di configurazione rilasciati";
                }
                else
                {
                    labelUpdateConf.Text = "I file di configurazione sono aggiornati";
                    labelConfResult.Text = "E' comunque possibile reimpostare i file con l'ultima versione rilasciata.";
                }
            }
            catch (Exception exc) {
                labelUpdateConf.Text = exc.Message;
            }

        }

        public async Task<Version> GetLatestVersionAcync() {
            Version version = new Version();
            try
            {
                Uri uri = new Uri("https://github.com/EnricoTolomei/GTFS_RSM");
                var github = new GitHubClient(new ProductHeaderValue("GTFS_RSM"), uri);
                Release latastRelease = await github.Repository.Release.GetLatest("EnricoTolomei", "GTFS_RSM");

                string versione = latastRelease.Name.Replace("Release", "").Trim();
                var listStrLineElements = versione.Split('.').ToList();
                version = new Version(
                    int.Parse(listStrLineElements?.ElementAtOrDefault(0) ?? "0"),
                    int.Parse(listStrLineElements?.ElementAtOrDefault(1) ?? "0"),
                    int.Parse(listStrLineElements?.ElementAtOrDefault(2) ?? "0"),
                    int.Parse(listStrLineElements?.ElementAtOrDefault(3) ?? "0")
                    );
            }
            catch (Exception exc) {
                Log.Error(exc, "ERRORE RECUPERO VERSIONE");
                labelUpdateProgram.Text = exc.Message;
                //throw exc;
            }
            return version;
        }

        public async Task<decimal> GetLatestConfVersionAcync()
        {
            Uri uriVersion = new Uri("https://raw.githubusercontent.com/EnricoTolomei/GTFS_RSM/master/AtacFeed/Config/GTFS_Static/LatestVersion.txt");
            string localVersion = $"Config{Path.DirectorySeparatorChar}GTFS_Static{Path.DirectorySeparatorChar}LatestVersion.txt.new";
            string vers = string.Empty;
            using (WebClient wc = new WebClient())
            {
                wc.DownloadProgressChanged += Wc_DownloadProgressChanged;                
                await wc.DownloadFileTaskAsync(uriVersion,localVersion);
            }
            vers = File.ReadAllText(localVersion);
            if (!decimal.TryParse(vers, out decimal serverConfVersion))
                Log.Error("Errore conversione della stringa {Vers}", vers);

            return serverConfVersion;
        }

        private void Wc_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            //Thread.Sleep(400); 
            progressBar.Value = 0;
        }

        void Wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        private async void ButtonUpdate_ClickAsync(object sender, EventArgs e)
        {
            try
            {
                progressBar.Value = 0;
                Uri uriRoutes = new Uri("https://raw.githubusercontent.com/EnricoTolomei/GTFS_RSM/master/AtacFeed/Config/GTFS_Static/routes.txt");
                string localRoutes = $"Config{Path.DirectorySeparatorChar}GTFS_Static{Path.DirectorySeparatorChar}routes.txt.new";

                Uri uriDettagli = new Uri("https://raw.githubusercontent.com/EnricoTolomei/GTFS_RSM/master/AtacFeed/Config/GTFS_Static/DettagliVettura.csv");
                string localDettagli = $"Config{Path.DirectorySeparatorChar}GTFS_Static{Path.DirectorySeparatorChar}DettagliVettura.csv.new";

                Uri uriCriterioMedia = new Uri("https://raw.githubusercontent.com/EnricoTolomei/GTFS_RSM/master/AtacFeed/Config/CriterioMediaPonderata.txt");
                string localCriterioMedia = $"Config{Path.DirectorySeparatorChar}CriterioMediaPonderata.txt.new";

                string localVersion = $"Config{Path.DirectorySeparatorChar}GTFS_Static{Path.DirectorySeparatorChar}LatestVersion.txt.new";

                string vers = string.Empty;
                using (WebClient wc = new WebClient())
                {
                    wc.DownloadProgressChanged += Wc_DownloadProgressChanged;
                    await wc.DownloadFileTaskAsync(uriRoutes, localRoutes);
                    await wc.DownloadFileTaskAsync(uriDettagli, localDettagli);
                    await wc.DownloadFileTaskAsync(uriCriterioMedia, localCriterioMedia);
                }
                File.Copy(localRoutes, Path.GetDirectoryName(localRoutes) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(localRoutes), true);
                File.Delete(localRoutes);

                File.Copy(localDettagli, Path.GetDirectoryName(localDettagli) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(localDettagli), true);
                File.Delete(localDettagli);

                File.Copy(localCriterioMedia, Path.GetDirectoryName(localCriterioMedia) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(localCriterioMedia), true);
                File.Delete(localCriterioMedia);

                if (File.Exists(localVersion))
                {
                    File.Copy(localVersion, Path.GetDirectoryName(localVersion) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(localVersion), true);
                    File.Delete(localVersion);
                }                
                labelResultConfUpdate.Visible = true;
                buttonRestart.Visible = true;
            }
            catch (Exception exc)
            {
                Log.Error(exc, "ERRORE UPDATE CONFIGURATION FILES");
                labelResultConfUpdate.Text = $"si è verificato un errore durante l'aggiornamento dei files.{Environment.NewLine}Riprovare più tardi.";
                labelResultConfUpdate.Visible = true;
            }
        }

        private void LinkUrlDownload_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start($"{Properties.Settings.Default.UrlDownload}");
        }

        private void LinkHome_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {        
            Process.Start($"{Properties.Settings.Default.UrlHome}");
        }

        private void UpdateBox_Load(object sender, EventArgs e)
        {
            Thread.Sleep(100);
            CheckVersion();
            CheckConf();
        }

        private void buttonRestart_Click(object sender, EventArgs e)
        {            
            DialogResult = DialogResult.Yes;
            Close();
        }
    }
}
