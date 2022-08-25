using Octokit;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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

        public bool? ExistNewerGTFS { get; set; }
        public bool ExistNewerConf { get; set; }

        public string UrlMD5 { get; set; }
        public string UrlGTFS { get; set; }
        public bool NewCSVDownloaded { get; set; }
        public bool NewGTFSDownloaded { get; set; }

        //private bool UpdatedFounded { get; set; }

        public async void CheckVersion() {            
            Version latestVersion = await GetLatestVersionAsync();
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
        public async Task<bool?> CheckGTFS() {
            Log.Information("CheckGTFS - Inizio Check");
            ExistNewerGTFS = false;
            string latestVersion = await GetLatestGTFSMD5Async();
            string actualVersion = string.Empty;
            string filepath = $"Config{Path.DirectorySeparatorChar}GTFS_Static{Path.DirectorySeparatorChar}gtfs.zip.md5";
            label3.Text= string.Empty;

            labelConfResult.Text = string.Empty;
            if (File.Exists(filepath))
            {
                actualVersion = File.ReadAllText(filepath);                
            }

            if (string.IsNullOrEmpty(latestVersion) ){
                labelUpdateGTFS.Text = "Impossibile determinare ultima versione disponibile sul server";
                label3.Text = "File MD5 di controllo non scaricato";
                labelConfResult.Text = "E' comunque possibile procedere con il download dell'ultima versione rilasciata.";
                ExistNewerGTFS = null;
            }
            else if (actualVersion != latestVersion)
            {
                labelUpdateGTFS.Text = "E' disponibile una nuova versione dei file GFTS statico";
                labelConfResult.Text = "E' consigliabile recuperare gli ultimi file di configurazione rilasciati";
                ExistNewerGTFS = true;
            }
            else {
                labelUpdateGTFS.Text = "I file di configurazione sono aggiornati";
                labelConfResult.Text = "E' comunque possibile reimpostare i file con l'ultima versione rilasciata.";
            }
            Log.Information("CheckGTFS - Fine Check con {ExistNewerGTFS}",ExistNewerGTFS);
            return ExistNewerGTFS;
        }

        public async Task<bool> CheckConf()
        {
            try
            {
                ExistNewerConf = false;
                decimal latestVersion = await GetLatestConfVersionAcync();
                decimal actualVersion = 0;
                string filepath = $"Config{Path.DirectorySeparatorChar}GTFS_Static{Path.DirectorySeparatorChar}LatestVersion.txt";

                if (File.Exists(filepath))
                {
                    string localVersion = File.ReadAllLines(filepath).FirstOrDefault();
                    decimal.TryParse(localVersion, out actualVersion);
                }
                if (actualVersion < latestVersion)
                {
                    labelUpdateCSV.Text = "E' disponibile una nuova versione dei file di configurazione";
                    labelUpdateCSV.Text = "E' consigliabile recuperare gli ultimi file di configurazione rilasciati";
                    ExistNewerConf = true;
                }
                else
                {
                    labelUpdateCSV.Text = "I file di configurazione sono aggiornati";
                    labelConfResult.Text = "E' comunque possibile reimpostare i file con l'ultima versione rilasciata.";
                }
            }
            catch (Exception exc)
            {
                labelUpdateCSV.Text = exc.Message;
            }
            return ExistNewerConf;            
        }

        public async Task<Version> GetLatestVersionAsync() {
            Version version = new Version();
            try
            {
                Uri uri = new Uri("https://github.com/EnricoTolomei/GTFS_RSM");
                var github = new GitHubClient(new ProductHeaderValue("GTFS_RSM"), uri);
                Release latastRelease = await github.Repository.Release.GetLatest("EnricoTolomei", "GTFS_RSM");

                string versione = latastRelease.Name.Replace("Release", "").Trim();
                string versioneTag = latastRelease.TagName.Replace("v_", "").Trim();
                var listStrLineElements = versioneTag.Split('.').ToList();
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
        public async Task<string> GetLatestGTFSMD5Async()
        {
            string vers = string.Empty;
            try
            {
                Uri uri = new Uri(UrlMD5);
                string localVersion = $"Config{Path.DirectorySeparatorChar}GTFS_Static{Path.DirectorySeparatorChar}/gtfs.zip.md5.new";
                using (WebClient wc = new WebClient())
                {
                    wc.DownloadProgressChanged += Wc_DownloadProgressChanged;
                    await wc.DownloadFileTaskAsync(uri, localVersion);
                }
                vers = File.ReadAllText(localVersion);
                return vers;
            }
            catch (Exception exc)
            {
                Log.Error(exc, "ERRORE RECUPERO GTFS MD5");
            }
            return vers;
        }
        public async Task<decimal> GetLatestConfVersionAcync()
        {
            Uri uriVersion = new Uri("https://raw.githubusercontent.com/EnricoTolomei/GTFS_RSM/master/AtacFeed/Config/GTFS_Static/LatestVersion.txt");
            string localVersion = $"Config{Path.DirectorySeparatorChar}GTFS_Static{Path.DirectorySeparatorChar}LatestVersion.txt.new";
            string vers = string.Empty;
            using (WebClient wc = new WebClient())
            {
                wc.DownloadProgressChanged += Wc_DownloadProgressChanged;                
                await wc.DownloadFileTaskAsync(uriVersion, localVersion);
            }
            vers = File.ReadAllText(localVersion);
            if (!decimal.TryParse(vers, out decimal serverConfVersion))
            {
                Log.Error("Errore conversione della stringa {Vers}", vers);
            }

            return serverConfVersion;
        }

        private void Wc_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            progressBar.Value = 0;
        }

        void Wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }


        private void LinkUrlDownload_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start($"{Properties.Settings.Default.UrlDownload}");
        }

        private void LinkHome_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {        
            Process.Start($"{Properties.Settings.Default.UrlHome}");
        }

        private void ButtonRestart_Click(object sender, EventArgs e)
        {            
            DialogResult = DialogResult.Yes;
            Close();
        }
        public async void Check()
        {
            if (ExistNewerGTFS.GetValueOrDefault(true) == false)
            {
                ExistNewerGTFS = await CheckGTFS();
            }
            if (ExistNewerConf == false)
            {
                ExistNewerConf = await CheckConf();
            }

            //CheckVersion();

            UpdateBox_Shown(null, null);
        }
        private void UpdateBox_Shown(object sender, EventArgs e)
        {            
            if (!ExistNewerGTFS.HasValue)
            {
                buttonAggiornaGTFS.ForeColor = System.Drawing.SystemColors.ControlText;
            }
            else if (ExistNewerGTFS.GetValueOrDefault(false))
            {
                buttonAggiornaGTFS.ForeColor = System.Drawing.SystemColors.ControlText;
            }
            else
            {
                buttonAggiornaGTFS.ForeColor = System.Drawing.SystemColors.ControlDark;
            }

            if (ExistNewerConf)
            {
                buttonAggiornaCSV.ForeColor = System.Drawing.SystemColors.ControlText;
            }
            else
            {
                buttonAggiornaCSV.ForeColor = System.Drawing.SystemColors.ControlDark;
            }
        }

        public async void ButtonAggiornaGTFS_Click(object sender, EventArgs e)
        {
            bool clickPerformed = (e is MouseEventArgs evt && evt.Clicks > 0);
            progressBar.Value = 0;
            bool esito = await DownloadGTFS(clickPerformed);
            labelResultConfUpdate.Visible = clickPerformed;
            buttonRestart.Visible = clickPerformed;
            if (clickPerformed && !esito)
            {
                labelResultConfUpdate.Text = $"Si è verificato un errore durante l'aggiornamento dei files.{Environment.NewLine}Riprovare più tardi.";
                labelResultConfUpdate.Visible = true;
            }
        }

        public async Task<bool> DownloadCSV(bool clickPerformed)
        {
            bool esitoDownload = false;
            try
            {
                Uri uriDettagli = new Uri("https://raw.githubusercontent.com/EnricoTolomei/GTFS_RSM/master/AtacFeed/Config/GTFS_Static/DettagliVettura.csv");
                Uri uriCriterioMedia = new Uri("https://raw.githubusercontent.com/EnricoTolomei/GTFS_RSM/master/AtacFeed/Config/CriterioMediaPonderata.txt");
                string localVersion = $"Config{Path.DirectorySeparatorChar}GTFS_Static{Path.DirectorySeparatorChar}LatestVersion.txt.new";
                string localDettagli = $"Config{Path.DirectorySeparatorChar}GTFS_Static{Path.DirectorySeparatorChar}DettagliVettura.csv.new";
                string localCriterioMedia = $"Config{Path.DirectorySeparatorChar}CriterioMediaPonderata.txt.new";

                using (WebClient wc = new WebClient())
                {
                    if (clickPerformed)
                    {
                        wc.DownloadProgressChanged += Wc_DownloadProgressChanged;
                    }
                    await wc.DownloadFileTaskAsync(uriDettagli, localDettagli);
                    await wc.DownloadFileTaskAsync(uriCriterioMedia, localCriterioMedia);
                }

                File.Copy(localVersion, Path.GetDirectoryName(localVersion) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(localVersion), true);
                File.Delete(localVersion);

                File.Copy(localDettagli, Path.GetDirectoryName(localDettagli) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(localDettagli), true);
                File.Delete(localDettagli);

                File.Copy(localCriterioMedia, Path.GetDirectoryName(localCriterioMedia) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(localCriterioMedia), true);
                File.Delete(localCriterioMedia);
                NewCSVDownloaded = true;
                esitoDownload = true;
            }
            catch (Exception exc)
            {
                Log.Error(exc, "Errore Download GTFS");
                esitoDownload = false;
            }
            return esitoDownload;
        }

        public async Task<bool> DownloadGTFS(bool clickPerformed)
        {
            bool esitoDownload = false;
            try
            {
                Uri uriGTFS = new Uri(UrlGTFS);
                string localGTFS = $"Config{Path.DirectorySeparatorChar}GTFS_Static{Path.DirectorySeparatorChar}GTFS.zip.new";

                using (WebClient wc = new WebClient())
                {
                    if (clickPerformed)
                    {
                        wc.DownloadProgressChanged += Wc_DownloadProgressChanged;
                    }
                    await wc.DownloadFileTaskAsync(uriGTFS, localGTFS);
                }
                string localGTFSMD5 = $"Config{Path.DirectorySeparatorChar}GTFS_Static{Path.DirectorySeparatorChar}gtfs.zip.md5.new";
                if (File.Exists(localGTFSMD5))
                {
                    File.Copy(localGTFSMD5, Path.GetDirectoryName(localGTFSMD5) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(localGTFSMD5), true);
                    File.Delete(localGTFSMD5);
                }
                File.Copy(localGTFS, Path.GetDirectoryName(localGTFS) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(localGTFS), true);
                File.Delete(localGTFS);
                NewGTFSDownloaded = true;
                //return localGTFS;
                esitoDownload = true;
            }
            catch (Exception exc) {
                Log.Error(exc,"Errore Download GTFS");
                esitoDownload = false;
            }
            return esitoDownload;
        }

        internal void ResetUI()
        {
            labelResultConfUpdate.Visible = false;
            buttonRestart.Visible= false;
        }

        private async void ButtonAggiornaCSV_Click(object sender, EventArgs e)
        {
            bool clickPerformed = (e is MouseEventArgs evt && evt.Clicks > 0);
            progressBar.Value = 0;
            bool esito = await DownloadCSV(clickPerformed);

            labelResultConfUpdate.Visible = clickPerformed;
            buttonRestart.Visible = clickPerformed;

            if (clickPerformed && !esito)
            {
                labelResultConfUpdate.Text = $"Si è verificato un errore durante l'aggiornamento dei files.{Environment.NewLine}Riprovare più tardi.";
                labelResultConfUpdate.Visible = true;
            }
        }
    }
}
