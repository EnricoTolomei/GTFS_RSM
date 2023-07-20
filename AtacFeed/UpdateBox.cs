using Octokit;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
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

        public static bool? ExistNewerVersion { get; set; }
        public static bool? ExistNewerGTFS { get; set; }
        public static bool? ExistNewerCSV { get; set; }

        public string UrlMD5 { get; set; }
        public string UrlGTFS { get; set; }
        public bool NewCSVDownloaded { get; set; }
        public bool NewGTFSDownloaded { get; set; }

        public async Task<bool?> CheckVersion(bool clickPerformed = false)
        {
            try
            {
                Log.Information("CheckVersion - Inizio Check");
                Version latestVersion = await GetLatestVersionAsync(clickPerformed);
                Version actualVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                labelActualVersion.Text = $"Versione in uso {actualVersion.Major}.{actualVersion.Minor:00}";
                labelLastVersion.Text = $"Ultima versione {latestVersion.Major}.{latestVersion.Minor:00}";
                if (clickPerformed)
                {
                    if (latestVersion == new Version())
                    {
                        labelUpdateProgram.Text = "Problemi di connessione con il server GitHub";
                        ExistNewerVersion = null;
                    }
                    else if (actualVersion < latestVersion)
                    {
                        labelUpdateProgram.Text = "E' disponibile una nuova versione del programma";
                        ExistNewerVersion = true;
                    }
                    else
                    {
                        labelUpdateProgram.Text = "Stai utilizzando l'ultima versione";
                        ExistNewerVersion = false;
                    }
                }
                Log.Information("CheckGTFS - Fine Check con {ExistNewerGTFS}", ExistNewerVersion);
            }
            catch(Exception exc)
            {
                Log.Error(exc, "UPS CheckVersion");
            }
            return ExistNewerVersion;
        }
        public async Task<bool?> CheckGTFS(bool clickPerformed = false)
        {
            try
            {
                Log.Information("CheckGTFS - Inizio Check");
                if (clickPerformed)
                {
                    label3.Text = string.Empty;
                    labelConfResult.Text = string.Empty;
                }
                string latestVersion = await GetLatestGtfsMd5Async(clickPerformed);
                string actualVersion = string.Empty;
                string filepath = $"Config{Path.DirectorySeparatorChar}GTFS_Static{Path.DirectorySeparatorChar}gtfs.zip.md5";
                if (File.Exists(filepath))
                {
                    actualVersion = File.ReadAllText(filepath);
                }
                string updateText, confResulut, text3 = string.Empty;

                if (string.IsNullOrEmpty(latestVersion))
                {
                    updateText = "Impossibile determinare ultima versione disponibile sul server";
                    text3 = "File MD5 di controllo non scaricato";
                    confResulut = "E' comunque possibile procedere con il download dell'ultima versione rilasciata.";
                    ExistNewerGTFS = null;
                }
                else if (actualVersion != latestVersion)
                {
                    updateText = "E' disponibile una nuova versione dei file GFTS statico";
                    confResulut = "E' consigliabile recuperare gli ultimi file di configurazione rilasciati";
                    ExistNewerGTFS = true;
                }
                else
                {
                    updateText = "I file di configurazione sono aggiornati";
                    confResulut = "E' comunque possibile reimpostare i file con l'ultima versione rilasciata.";
                    ExistNewerGTFS = false;
                }
                if (clickPerformed)
                {
                    labelUpdateGTFS.Text = updateText;
                    labelConfResult.Text = confResulut;
                    label3.Text = text3;
                }
                Log.Information("CheckGTFS Fine Check con {ExistNewerGTFS}", ExistNewerGTFS);
            }
            catch (Exception exc)
            {
                Log.Error(exc, "UPS");
            }
            return ExistNewerGTFS;
        }

        public async Task<bool?> CheckCSV(bool clickPerformed = false)
        {
            try
            {
                ExistNewerCSV = false;
                decimal latestVersion = await GetLatestCsvVersionAsync();
                decimal actualVersion = 0;
                string filepath = $"Config{Path.DirectorySeparatorChar}GTFS_Static{Path.DirectorySeparatorChar}LatestVersion.txt";
                if (File.Exists(filepath))
                {
                    string localVersion = File.ReadAllLines(filepath).FirstOrDefault();
                    decimal.TryParse(localVersion, out actualVersion);
                }
                if (clickPerformed)
                {
                    if (actualVersion < latestVersion)
                    {
                        labelUpdateCSV.Text = "E' disponibile una nuova versione dei file di configurazione";
                        labelUpdateCSV.Text = "E' consigliabile recuperare gli ultimi file di configurazione rilasciati";
                        ExistNewerCSV = true;
                    }
                    else
                    {
                        labelUpdateCSV.Text = "I file di configurazione sono aggiornati";
                        labelConfResult.Text = "E' comunque possibile reimpostare i file con l'ultima versione rilasciata.";
                    }
                }
            }
            catch (Exception exc)
            {
                labelUpdateCSV.Text = exc.Message;
                ExistNewerCSV = null;
            }
            return ExistNewerCSV;
        }

        public async Task<Version> GetLatestVersionAsync(bool clickPerformed = false)
        {
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
                if (clickPerformed)
                {
                    labelUpdateProgram.Text = exc.Message;
                }
            }
            return version;
        }
        public async Task<string> GetLatestGtfsMd5Async(bool clickPerformed = false)
        {
            string vers = string.Empty;
            try
            {
                Uri uri = new Uri(UrlMD5);
                string localVersion = $"Config{Path.DirectorySeparatorChar}GTFS_Static{Path.DirectorySeparatorChar}/gtfs.zip.md5.new";
                using (WebClient wc = new WebClient())
                {
                    if (clickPerformed)
                    {
                        wc.DownloadProgressChanged += Wc_DownloadProgressChanged;
                    }
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
        public async Task<decimal> GetLatestCsvVersionAsync()
        {
            decimal serverConfVersion = 0;
            try
            {
                Uri uriVersion = new Uri("https://raw.githubusercontent.com/EnricoTolomei/GTFS_RSM/master/AtacFeed/Config/GTFS_Static/LatestVersion.txt");
                string localVersion = $"Config{Path.DirectorySeparatorChar}GTFS_Static{Path.DirectorySeparatorChar}LatestVersion.txt.new";
                using (WebClient wc = new WebClient())
                {
                    await wc.DownloadFileTaskAsync(uriVersion, localVersion);
                }
                string vers = File.ReadAllText(localVersion);
                if (!decimal.TryParse(vers, out serverConfVersion))
                {
                    Log.Error("Errore conversione della stringa {Vers}", vers);
                }
            }
            catch (Exception exc)
            {
                Log.Error(exc, "ERRORE RECUPERO VERSIONE CSV");
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
        public async void Check(bool clickPerformed=false)
        {
            if (!NewGTFSDownloaded && !ExistNewerGTFS.GetValueOrDefault(false))
            {
                ExistNewerGTFS = await CheckGTFS(clickPerformed);
            }
            if (!NewCSVDownloaded && !ExistNewerCSV.GetValueOrDefault(false))
            {
                ExistNewerCSV = await CheckCSV(clickPerformed);
            }

            await CheckVersion(clickPerformed);

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

            if (!ExistNewerCSV.HasValue)
            {
                buttonAggiornaCSV.ForeColor = System.Drawing.SystemColors.ControlText;
            }
            else if (ExistNewerGTFS.GetValueOrDefault(false))
            {
                buttonAggiornaCSV.ForeColor = System.Drawing.SystemColors.ControlText;
            }
            else
            {
                buttonAggiornaCSV.ForeColor = System.Drawing.SystemColors.ControlDark;
            }

            if (NewCSVDownloaded || NewGTFSDownloaded)
            {
                labelResultConfUpdate.Visible = true;
                buttonRestart.Visible = true;
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
            progressBar.Value = 0;
            bool esito = await DownloadCSV(true);
            buttonRestart.Visible = esito;
            labelResultConfUpdate.Visible= true;
            labelResultConfUpdate.Text = esito ? "Il download è terminato.\n\rLa configurazione sarà utilizzata al prossimo riavvio del monitoraggio" : $"Si è verificato un errore durante l'aggiornamento dei files.{Environment.NewLine}Riprovare più tardi.";
        }
        public async void ButtonAggiornaGTFS_Click(object sender, EventArgs e)
        {            
            progressBar.Value = 0;
            bool esito = await DownloadGTFS(true);
            buttonRestart.Visible = esito;
            labelResultConfUpdate.Visible = true;
            labelResultConfUpdate.Text = esito ? "Il download è terminato.\n\rLa configurazione sarà utilizzata al prossimo riavvio del monitoraggio": $"Si è verificato un errore durante l'aggiornamento dei files.{Environment.NewLine}Riprovare più tardi.";
        }

        public async Task<bool?> TaskCheckUpdate(bool download = false, bool forceDownload = false)
        {
            bool? existNewConf = await CheckCSV();
            bool? existNewGTFS = string.IsNullOrEmpty(UrlMD5) ? null : await CheckGTFS();
            bool? existNewVersion = await CheckVersion();

            if (forceDownload || (download && existNewConf.GetValueOrDefault(false)))
            {
                await DownloadCSV(false);
            }

            if (forceDownload || (download && existNewGTFS.GetValueOrDefault(false)))
            {
                await DownloadGTFS(false);
            }

            return !existNewVersion.HasValue || !existNewGTFS.HasValue || !existNewGTFS.HasValue
                        ? null
                        : existNewGTFS.HasValue
                            ? existNewConf.GetValueOrDefault(false) || existNewGTFS.GetValueOrDefault(false)
                            : existNewGTFS;
        }
    }
}
