using AtacFeed.Properties;
using FastMember;
using GTFS.Entities;
using Newtonsoft.Json;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Log = Serilog.Log;

namespace AtacFeed
{
    public partial class FormGTFS_RSM : Form
    {
        private Dictionary<string, CityDefaults> cityDefaults;
        private bool isApplyingCityDefaults;

        private sealed class CityDefaults
        {
            public string UrlVehicle { get; set; }
            public string UrlTrip { get; set; }
            public string UrlAlert { get; set; }
            public string UrlVehicleRiserva { get; set; }
            public string UrlGTFSStatico { get; set; }
            public string UrlMD5GTFSStatico { get; set; }
            public string UrlDettagliVettura { get; set; }
            public bool? DettagliVettura { get; set; }
        }

        private static Dictionary<string, CityDefaults> LoadCityDefaults()
        {
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "CityDefaults.json");
            if (!File.Exists(configPath))
            {
                return new Dictionary<string, CityDefaults>(StringComparer.OrdinalIgnoreCase);
            }

            string json = File.ReadAllText(configPath);
            var defaults = JsonConvert.DeserializeObject<Dictionary<string, CityDefaults>>(json);
            return defaults ?? new Dictionary<string, CityDefaults>(StringComparer.OrdinalIgnoreCase);
        }

        private void InitializeCitySelector()
        {
            cityDefaults = LoadCityDefaults();
            comboCitta.Items.AddRange(cityDefaults.Keys.OrderBy(x => x).ToArray());
            comboCitta.Items.Add("Personalizzato");
            
            urlVehicle.TextChanged += UrlVehicle_TextChanged;
            urlTrip.TextChanged += UrlTrip_TextChanged;
            urlAlert.TextChanged += UrlAlert_TextChanged;
            urlVehicleRiserva.TextChanged += UrlVehicleRiserva_TextChanged;
            urlGTFS_Statico.TextChanged += UrlGTFS_Statico_TextChanged;
            urlMD5_GTFS_Statico.TextChanged += UrlMD5_GTFS_Statico_TextChanged;            
        }

        private void ComboCitta_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedCity = comboCitta.SelectedItem as string;
            if (string.IsNullOrWhiteSpace(selectedCity) || selectedCity.Equals("Personalizzato", StringComparison.OrdinalIgnoreCase))
            {
                UpdateCityVehicleDetailsDownloadSettings();
                return;
            }

            if (!cityDefaults.TryGetValue(selectedCity, out var defaults))
                return;

            isApplyingCityDefaults = true;
            try
            {
                urlVehicle.Text = defaults.UrlVehicle;
                urlTrip.Text = defaults.UrlTrip;
                urlAlert.Text = defaults.UrlAlert;
                urlVehicleRiserva.Text = defaults.UrlVehicleRiserva;
                urlGTFS_Statico.Text = defaults.UrlGTFSStatico;
                urlMD5_GTFS_Statico.Text = defaults.UrlMD5GTFSStatico;
                checkDettagliVettura.Checked = defaults.DettagliVettura?? false;
            }
            finally
            {
                isApplyingCityDefaults = false;
            }

            UpdateBox.UrlGTFS = urlGTFS_Statico.Text;
            UpdateBox.UrlMD5 = urlMD5_GTFS_Statico.Text;
            UpdateCityVehicleDetailsDownloadSettings();
        }

        private void UpdateCityVehicleDetailsDownloadSettings()
        {
            string selectedCity = comboCitta.SelectedItem?.ToString();
            UpdateBox.CityName = selectedCity;

            if (!checkDettagliVettura.Checked ||
                string.IsNullOrWhiteSpace(selectedCity) ||
                !cityDefaults.TryGetValue(selectedCity, out var defaults))
            {
                UpdateBox.UrlDettagliVettura = null;
                return;
            }

            UpdateBox.UrlDettagliVettura = defaults.UrlDettagliVettura;
        }

        private void UrlVehicle_TextChanged(object sender, EventArgs e)
        {
            if (!isApplyingCityDefaults && comboCitta.SelectedItem != null)
                comboCitta.SelectedItem = "Personalizzato";
        }

        private void UrlTrip_TextChanged(object sender, EventArgs e)
        {
            if (!isApplyingCityDefaults && comboCitta.SelectedItem != null)
                comboCitta.SelectedItem = "Personalizzato";
        }

        private void UrlAlert_TextChanged(object sender, EventArgs e)
        {
            if (!isApplyingCityDefaults && comboCitta.SelectedItem != null)
                comboCitta.SelectedItem = "Personalizzato";
        }

        private void UrlVehicleRiserva_TextChanged(object sender, EventArgs e)
        {
            if (!isApplyingCityDefaults && comboCitta.SelectedItem != null)
                comboCitta.SelectedItem = "Personalizzato";
        }

        private void SetSelectedCityFromCurrentUrls()
        {
            var matchingCity = cityDefaults
                .Where(kvp => kvp.Value.UrlVehicle == urlVehicle.Text &&
                    kvp.Value.UrlTrip == urlTrip.Text &&
                    kvp.Value.UrlAlert == urlAlert.Text &&
                    kvp.Value.UrlVehicleRiserva == urlVehicleRiserva.Text &&
                    kvp.Value.UrlGTFSStatico == urlGTFS_Statico.Text &&
                    kvp.Value.UrlMD5GTFSStatico == urlMD5_GTFS_Statico.Text &&
                    kvp.Value.DettagliVettura == checkDettagliVettura.Checked)
                .Select(kvp => kvp.Key)
                .FirstOrDefault();

            comboCitta.SelectedItem = matchingCity ?? "Personalizzato";
        }

        private string fileName;

        private DateTime? _lastProcessedFeedTimestamp;
        private FeedMonitorSnapshot _currentSnapshot;
        private FeedConfigurationDto _currentConfiguration;
        // Sostituiti i due manager con la facade
        private readonly FeedGTFSManager _feedManager = new FeedGTFSManager();

        private readonly UpdateBox UpdateBox = new UpdateBox();

        public FormGTFS_RSM()
        {            
            InitializeComponent();
            InitializeCitySelector();

            // La facade invia uno snapshot DTO; solo il form effettua il marshal sul thread UI.
            _feedManager.SnapshotUpdated += (s, e) =>
            {
                if (IsHandleCreated && !IsDisposed)
                    BeginInvoke(new Action(() => UpdateUI(e.Snapshot)));
            };
            
            _feedManager.ErrorOccurred += (s, ex) =>
            {
                if (IsHandleCreated)
                {
                    BeginInvoke(new Action(() =>
                    {
                        Log.Error(ex, "Errore FeedGTFSManager");
                        // non bloccare l'UI con MessageBox automatico ma informare
                        textBox1.Text = ex.Message;
                        FeedMonitorSnapshot snapshot = _feedManager.GetSnapshot();
                        labelLetture.Text = snapshot.NumeroLetture.ToString();
                        labelFeedLetti.Text = snapshot.NumeroFeedValidi.ToString();
                        lblOraLettura.Text = snapshot.UltimoFeed?.ToString("HH:mm") ?? "--";
                        Log.Error("===> {Exception}", ex);
                    }));
                }
            };
            
            _feedManager.VehicleReadStarted += (s, args) =>
            {
                if (IsHandleCreated)
                    BeginInvoke(new Action(() =>
                    {
                        // mostrare stato nella statusbar / label
                        textBox1.Text += $"{Environment.NewLine}Lettura feed in corso: {Path.GetFileName(args.Url)}{Environment.NewLine}";
                        // cambiare icona/spinner
                        if (args.IsRiserva)
                        {
                            imgUrl2.Image = Resources.spinner;
                        }
                        else
                        {
                            imgUrl1.Image = Resources.spinner;
                            imgUrl2.Image = null;
                        }
                    }));
            };

            _feedManager.VehicleReadCompleted += (s, args) =>
            {
                if (IsHandleCreated)
                    BeginInvoke(new Action(() =>
                    {
                        if (args.Success)
                        {
                            //labelStatus.Text = $"Lettura completata: {Path.GetFileName(args.Url)} ({args.Timestamp:HH:mm:ss})";
                            if (args.IsRiserva)
                            {
                                imgUrl2.Image = Resources.verde; // esempio
                            }
                            else
                            {
                                imgUrl1.Image = Resources.verde; // esempio
                            }
                        }
                        else if (args.Error != null)
                        {
                            //labelStatus.Text = $"Lettura fallita: {Path.GetFileName(args.Url)} - {args.Error.Message}";
                            if (args.IsRiserva)
                            {
                                imgUrl2.Image = Resources.rosso; // esempio
                            }
                            else
                            {
                                imgUrl1.Image = Resources.rosso; // esempio
                            }
                        }
                        else
                        {
                            //labelStatus.Text = $"Lettura terminata (code {args.Code}): {Path.GetFileName(args.Url)}";
                            if (args.IsRiserva)
                            {
                                imgUrl2.Image = Resources.arancio; // esempio
                            }
                            else
                            {
                                imgUrl1.Image = Resources.arancio; // esempio
                            }
                        }

                    }));
            };

            _feedManager.VehicleElabStarted += (s, args) =>
            {
                if (IsHandleCreated)
                    BeginInvoke(new Action(() =>
                    {
                        //textBox1.Text += $"{Environment.NewLine} Inizio ELABORAZIONE FEED{Environment.NewLine}";
                        pictureBox1.Image = Resources.processing;
                    }));

            };

            _feedManager.VehicleElabCompleted += (s, args) =>
            {
                if (IsHandleCreated)
                    BeginInvoke(new Action(() =>
                    {
                        //textBox1.Text += $"{Environment.NewLine} FINE  ELABORAZIONE FEED {Environment.NewLine}";
                        pictureBox1.Image = null;

                    }));
            };
        }

        public void UpdateUI(FeedMonitorSnapshot snapshot)
        {
            if (snapshot == null)
            {
                Log.Information("UpdateUI: Snapshot null");
                return;
            }
            
            try
            {
                Log.Information("UpdateUI called with snapshot: {Snapshot}", snapshot.UltimoFeed.ToString());
                _currentSnapshot = snapshot;
                labelLetture.Text = snapshot.NumeroLetture.ToString();
                labelFeedLetti.Text = snapshot.NumeroFeedValidi.ToString();
                lblOraLettura.Text = snapshot.UltimoFeed?.ToString("HH:mm") ?? "--";
                textBox1.Clear();
                textBox2.Clear();

                if (snapshot.CodiceFeedAvvisi == 0)
                {
                    bindingSourceAvvisi.DataSource = CreateDataTable(snapshot.Avvisi);
                    GridAvvisi.DataSource = bindingSourceAvvisi;
                }

                if (snapshot.CodiceFeedVeicoli != 0)
                    return;

                if (snapshot.UltimoFeed.HasValue && _lastProcessedFeedTimestamp.HasValue &&
                    snapshot.UltimoFeed.Value == _lastProcessedFeedTimestamp.Value)
                    return;

                PopulateUIFromSnapshot(snapshot);
            }
            catch (Exception ex)
            {
                textBox1.AppendText(ex.Message);
                Log.Error(ex, "Errore durante la visualizzazione dello snapshot del feed");
            }
        }

        private void PopulateUIFromSnapshot(FeedMonitorSnapshot snapshot)
        {
            DateTime lastDataFeed = snapshot.UltimoFeed.GetValueOrDefault();

            if (snapshot.VeicoliAggiunti != null && snapshot.VeicoliAggiunti.Count > 0)
            {
                var added = new StringBuilder();
                foreach (VehicleSnapshotDto vehicle in snapshot.VeicoliAggiunti)
                    added.AppendFormat("{0} - {1} rilevata alle {2:HH:mm:ss}{3}", vehicle.IdVettura, vehicle.Matricola, lastDataFeed, Environment.NewLine);
                textBox3.AppendText(added.ToString());
            }

            if (snapshot.VeicoliRimossi != null && snapshot.VeicoliRimossi.Count > 0)
            {
                var removed = new StringBuilder();
                foreach (VehicleSnapshotDto vehicle in snapshot.VeicoliRimossi)
                    removed.AppendFormat("{0} - {1} NON rilevata alle {2:HH:mm:ss}{3}", vehicle.IdVettura, vehicle.Matricola, lastDataFeed, Environment.NewLine);
                textBox4.AppendText(removed.ToString());
            }

            RenderFeedIssues(snapshot.ProblemiFeed);
            RenderAnomalies(snapshot.AnomalieGtfs);

            labelTotaleRighe.Text = snapshot.TotaleRighe.ToString();
            labelTotaleIdVettura.Text = snapshot.TotaleIdVettura.ToString();
            labelTotaleMatricola.Text = snapshot.TotaleMatricola.ToString();
            labelTotaleMatricolaATAC.Text = snapshot.TotaleMatricolaAtac.ToString();
            labelTotaleMatricolaTPL.Text = snapshot.TotaleMatricolaTpl.ToString();

            FeedStatisticsDto statistics = snapshot.Statistiche ?? new FeedStatisticsDto();
            labelBusAtac.Text = statistics.RilevatoBusAtac.ToString();
            labelTramAtac.Text = statistics.RilevatoTramAtac.ToString();
            labelFilobusAtac.Text = statistics.RilevatoFilobusAtac.ToString();
            labelMiniBusEleAtac.Text = statistics.RilevatoMinibusElettrici.ToString();
            labelFurgoncinoAtac.Text = statistics.RilevatoFurgoncini.ToString();
            labelFerroAtac.Text = statistics.RilevatoFerro.ToString();
            labelAltroAtac.Text = statistics.RilevatoAltroAtac.ToString();
            labelBusTPL.Text = statistics.RilevatoBusTpl.ToString();
            labelPullmanTPL.Text = statistics.RilevatoPullmanTpl.ToString();
            labelAltroTpl.Text = statistics.RilevatoAltroTpl.ToString();

            extendedVehicleInfoBindingSource.DataSource = CreateDataTable(snapshot.VeicoliAggregati);
            advancedDataGridView1.DataSource = extendedVehicleInfoBindingSource;

            IEnumerable<VehicleSnapshotDto> currentVehicles = snapshot.VeicoliAttuali ?? new List<VehicleSnapshotDto>();
            if (string.IsNullOrEmpty(bindingSourceAttuale.Sort))
                currentVehicles = currentVehicles.OrderBy(vehicle => vehicle.Linea?.Length).ThenBy(vehicle => vehicle.Linea);

            bindingSourceAttuale.DataSource = CreateDataTable(currentVehicles);
            advancedDataGridView2.DataSource = bindingSourceAttuale;

            textBox1.AppendText($"Totale Vetture Rilevate sul Feed Vehicle {snapshot.TotaleVetture}{Environment.NewLine}");
            var services = snapshot.ServiziPerGestore ?? new List<ServiceStatisticDto>();
            var serviceText = new StringBuilder();
            foreach (var agencyGroup in services.GroupBy(service => service.Gestore))
            {
                serviceText.AppendLine();
                serviceText.AppendFormat("{0} - {1}{2}", agencyGroup.Key, agencyGroup.Sum(service => service.Numero), Environment.NewLine);
                foreach (ServiceStatisticDto service in agencyGroup)
                    serviceText.AppendFormat("    {0}\t{1}{2}", service.Servizio, service.Numero, Environment.NewLine);
            }
            textBox1.AppendText(serviceText.ToString());

            labelTPL.Text = snapshot.TotaleVettureTpl.ToString();
            labelAtac.Text = snapshot.TotaleVettureAtac.ToString();
            labelTot.Text = snapshot.TotaleVetture.ToString();
            labelPonderatiATAC.Text = Math.Round(snapshot.PonderateAtac).ToString();
            labelPonderatiTPL.Text = Math.Round(snapshot.PonderateTpl).ToString();

            if (string.IsNullOrEmpty(fileName))
                fileName = string.Format("Feed_{0:yyyy-MM-dd (HH_mm_ss)}", lastDataFeed);

            dataGridViolazioni.DataSource = snapshot.ViolazioniMonitoraggio;
            if (tabMainForm.SelectedTab == tabMonitoraggio)
                Colora();

            foreach (AlertRuleGroupDto alert in snapshot.GruppiAlert ?? new List<AlertRuleGroupDto>())
            {
                TabPage page = tabMainForm.TabPages[alert.Nome];
                DataGridView grid = page?.Controls[alert.Nome] as DataGridView;
                if (grid == null)
                    continue;

                grid.DataSource = checkBoxStorico.Checked
                    ? alert.ViolazioniStoriche.ToList()
                    : alert.ViolazioniAttuali.ToList();
            }

            AggiornaScottPlot(snapshot.PuntiGrafico);
            _lastProcessedFeedTimestamp = snapshot.UltimoFeed;
        }

        private static DataTable CreateDataTable<T>(IEnumerable<T> rows)
        {
            var table = new DataTable();
            if (rows == null)
                return table;

            using (var reader = ObjectReader.Create(rows))
                table.Load(reader);

            return table;
        }

        private void RenderFeedIssues(IEnumerable<FeedIssueDto> issues)
        {
            if (issues == null)
                return;

            var missingRoutes = issues
                .Where(issue => issue.Categoria == "Linea non presente nel GTFS statico")
                .Select(issue => issue.Messaggio)
                .ToList();
            if (missingRoutes.Count > 0)
                textBox2.AppendText($"Le seguenti linee {string.Join(", ", missingRoutes)}{Environment.NewLine} NON sono riportate nel file statico routes.txt{Environment.NewLine}{Environment.NewLine}");

            foreach (FeedIssueDto issue in issues.Where(issue => issue.Categoria != "Linea non presente nel GTFS statico"))
            {
                string detail = string.IsNullOrWhiteSpace(issue.Dettaglio) ? string.Empty : $" [{issue.Dettaglio}]";
                Color color = issue.Categoria == "Trip duplicato" ? Color.Tomato : Color.DarkGray;
                AppendColoredLine(textBox2, $"{issue.Categoria}: {issue.Messaggio}{detail}", color);
            }
        }

        private void RenderAnomalies(IEnumerable<VehicleAnomalyDto> anomalies)
        {
            if (anomalies == null)
                return;

            foreach (var category in anomalies.GroupBy(anomaly => anomaly.Categoria))
            {
                textBox2.AppendText(category.Key + Environment.NewLine);
                foreach (VehicleAnomalyDto anomaly in category)
                {
                    string message = $"Matricola {anomaly.Matricola} Linea {anomaly.Linea} Fermata {anomaly.Fermata}";
                    if (category.Key == "Percorso anomalo")
                        message += $" => 'balzo' di {anomaly.Delta}";

                    Color color = category.Key == "Percorso anomalo"
                        ? (anomaly.Delta < 0 ? Color.OrangeRed : Color.DarkOrange)
                        : Color.CornflowerBlue;
                    AppendColoredLine(textBox2, message, color, category.Key == "Percorso anomalo" && anomaly.Delta < 0);
                }
                textBox2.AppendText(Environment.NewLine);
            }
        }

        private static void AppendColoredLine(RichTextBox textBox, string text, Color color, bool bold = false)
        {
            string line = text + Environment.NewLine;
            int start = textBox.TextLength;
            textBox.AppendText(line);
            textBox.Select(start, line.Length);
            textBox.SelectionColor = color;
            if (bold)
                textBox.SelectionFont = new Font(textBox.SelectionFont, FontStyle.Bold);
            textBox.SelectionIndent = 10;
            textBox.Select(textBox.TextLength, 0);
        }

        private void RestartFile()
        {
            ResetUI();
            fileName = string.Empty;
            _feedManager.Reset();
            _currentSnapshot = _feedManager.GetSnapshot();
            _lastProcessedFeedTimestamp = null;
            labelLetture.Text = _currentSnapshot.NumeroLetture.ToString();
            labelFeedLetti.Text = _currentSnapshot.NumeroFeedValidi.ToString();
            lblOraLettura.Text = "--";

            FeedConfigurationDto configuration;
            if (UpdateBox.NewCSVDownloaded || UpdateBox.NewGTFSDownloaded)
            {
                configuration = LeggiFileConfigurazione();
                ApplicaRegoleMonitoraggio(configuration);
            }
            else
            {
                configuration = _feedManager.ReloadAlertConfiguration(
                    Path.Combine("Config", "Regole_Alert"),
                    Path.Combine("Config", "CriterioMediaPonderata.txt"));
            _currentConfiguration = configuration;
            }
            ApplicaRegoleAlert(configuration);

            dataGridViolazioni.Invalidate();
            dataGridViolazioni.DataSource = null; 

            GridAvvisi.Invalidate();
            GridAvvisi.DataSource = null;

            advancedDataGridView1.Invalidate();
            advancedDataGridView1.DataSource = null;

            advancedDataGridView2.Invalidate();
            advancedDataGridView2.DataSource = null;

            plotTPL.Reset();
            plotAtac.Reset();
        }


        private void ButtonPlayPause_Click(object sender, EventArgs e)
        {
            if (!checkCSV.Checked && !checkXlsx.Checked)
            {
                DialogResult dialog = MessageBox.Show(
                    "Avviare il monitoraggio senza export dei dati?",
                    "Avvio Monitoraggio",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );
                if (dialog == DialogResult.No)
                {
                    return;
                }
            }

            int deltaMilliSec = (int)(1000 * (60 * minuti.Value + secondi.Value));
            string routeID = comboLineaMonitorata.SelectedValue?.ToString() ?? "-1";
            string filtroLinea = routeID == "-1" ? string.Empty : routeID;
            bool filtroTripVuoti = checkTripVuoti.Checked;
            bool filtroTuttoPercorso = checkTuttoPercorso.Visible && checkTuttoPercorso.Checked;
            bool raggruppalineaRegola = radioLineaRegola.Enabled && radioLineaRegola.Checked;
            bool nonRaggruppare = radioNonRaggruppare.Checked;
            bool forzaResetACquisizione = checkResetSempre.Visible && checkResetSempre.Checked;
            if (deltaMilliSec == 0)
            {
                Log.Information("Acquisizione singola");
                // usa facade per refresh
                _ = _feedManager.RefreshAsync(
                        urlVehicle: urlVehicle.Text,
                        urlVehicleRiserva: urlVehicleRiserva.Text,
                        urlAlert: urlAlert.Text,
                        filtroLinea: filtroLinea,
                        filtroTripVuoti = true,
                        filtroTuttoPercorso: filtroTuttoPercorso,
                        raggruppalineaRegola: raggruppalineaRegola,
                        nonRaggruppare: nonRaggruppare,
                        cancellation: CancellationToken.None);
            }
            else if (!_feedManager.IsMonitoring && deltaMilliSec > 0)
            {
                minuti.Enabled = false;
                secondi.Enabled = false;
                _feedManager.StartAutoRefresh(urlVehicle.Text, urlVehicleRiserva.Text, urlAlert.Text, filtroLinea, filtroTripVuoti, filtroTuttoPercorso, raggruppalineaRegola, nonRaggruppare, deltaMilliSec, forzaResetACquisizione);
                buttonPlayPause.BackgroundImage = Resources.pause;
                comboLineaMonitorata.Enabled = false;
                buttonResetRegole.Enabled = false;
                Log.Information("Acquisizione attiva");
            }
            else
            {
                minuti.Enabled = true;
                secondi.Enabled = true;
                _feedManager.StopAutoRefresh();
                buttonPlayPause.BackgroundImage = Resources.play;
                comboLineaMonitorata.Enabled = true;
                buttonResetRegole.Enabled = true;
                if (deltaMilliSec > 0)
                {
                    Log.Information("Acquisizione in pausa");
                }
            }
        }

        // campi aggiuntivi per evitare render duplicati
        private int _lastGraficoCount = 0;
        private DateTime? _lastGraficoTimestamp = null;

        private void AggiornaScottPlot(IEnumerable<MonitorChartPointDto> points)
        {
            var graf = points?.ToList();
            if (graf == null || graf.Count == 0) return;

            // deduplica per timestamp (Ticks) scegliendo il campione con Rilevate>0 se presente
            var deduped = graf
                .GroupBy(x => x.DataOra.Ticks)
                .Select(g =>
                {
                    var nonZero = g.Where(v => v.Rilevate > 0).ToList();
                    return nonZero.Any() ? nonZero.Last() : g.Last();
                })
                .OrderBy(x => x.DataOra)
                .ToList();

            // se nulla è cambiato rispetto all'ultimo render, skip
            if (deduped.Count == _lastGraficoCount && _lastGraficoTimestamp.HasValue && deduped.Last().DataOra == _lastGraficoTimestamp.Value)
                return;

            _lastGraficoCount = deduped.Count;
            _lastGraficoTimestamp = deduped.Last().DataOra;

            // render con la lista pulita
            RenderInternal(plotAtac, plotTPL, deduped);
        }

        private void RenderInternal(FormsPlot pltATAC, FormsPlot pltTPL, IList<MonitorChartPointDto> grafico)
        {
            var culture = CultureInfo.CreateSpecificCulture("it");

            int n = grafico.Count;
            double[] tempo = new double[n];
            double[] serieAtac = new double[n];
            double[] serieAggregateATAC = new double[n];
            double[] serieTPL = new double[n];
            double[] serieAggregateTPL = new double[n];

            for (int i = 0; i < n; i++)
            {
                var item = grafico[i];
                tempo[i] = item.DataOra.ToOADate();
                serieAtac[i] = (double)item.Atac;
                serieAggregateATAC[i] = (double)item.AggregatoAtac;
                serieTPL[i] = (double)item.Tpl;
                serieAggregateTPL[i] = (double)item.AggregatoTpl;
            }

            pltATAC.Plot.Clear();
            var aggAtac = pltATAC.Plot.AddSignalXY(tempo, serieAggregateATAC, color: Color.FromArgb(231, 109, 20), label: "Aggregate");
            aggAtac.LineWidth = 2;
            aggAtac.MarkerSize = 2;
            var istAtac = pltATAC.Plot.AddSignalXY(tempo, serieAtac, color: Color.FromArgb(137, 8, 39), label: "Istantanee");
            istAtac.LineWidth = 2;
            istAtac.MarkerSize = 2;

            pltATAC.Plot.SetCulture(culture);
            pltATAC.Plot.XAxis.DateTimeFormat(true);
            pltATAC.Plot.Legend(location: Alignment.UpperLeft);
            pltATAC.Plot.YAxis.Label(label: "Vetture rilevate");
            pltATAC.Plot.Title("Monitoraggio vetture ATAC");
            pltATAC.Plot.AxisAuto();
            pltATAC.Render();

            pltTPL.Plot.Clear();
            var plotSignalAggragatoTPL = pltTPL.Plot.AddSignalXY(tempo, serieAggregateTPL, color: Color.FromArgb(231, 109, 20), label: "Aggregate");
            plotSignalAggragatoTPL.LineWidth = 3;
            plotSignalAggragatoTPL.MarkerSize = 3;
            var plotSignalActualTPL = pltTPL.Plot.AddSignalXY(tempo, serieTPL, color: Color.FromArgb(4, 65, 136), label: "Istantanee");
            plotSignalActualTPL.LineWidth = 2;
            plotSignalActualTPL.MarkerSize = 2;
            pltTPL.Plot.SetCulture(culture);
            pltTPL.Plot.XAxis.DateTimeFormat(true);
            pltTPL.Plot.YAxis.Label(label: "Vetture rilevate");
            pltTPL.Plot.YAxis.MinimumTickSpacing(1);
            pltTPL.Plot.Legend(location: Alignment.UpperLeft);
            pltTPL.Plot.Title("Monitoraggio vetture altri gestori");
            pltTPL.Plot.AxisAuto();
            pltTPL.Render();
        }

        // mantiene compatibilità pubblica
        public void Render(FormsPlot pltATAC, FormsPlot pltTPL)
        {
            var snapshot = _currentSnapshot ?? _feedManager.GetSnapshot();
            var grafico = snapshot.PuntiGrafico ?? new List<MonitorChartPointDto>();
            RenderInternal(pltATAC, pltTPL, grafico);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            System.Version actualVersion = Assembly.GetExecutingAssembly().GetName().Version;
            labelVer.Text = string.Format("Vers. {0}.{1:00}", actualVersion.Major, actualVersion.Minor);
            checkMD5.Text = "Aggiorna,se possibile, i file di configurazione in automatico.\r\nSaranno utilizzati al successivo riavvio del monitoraggio";

            #region Load default settings
            urlGTFS_Statico.Text = Properties.Settings.Default.UrlGTFS_Statico;
            urlMD5_GTFS_Statico.Text = Properties.Settings.Default.UrlMD5_GTFS_Statico;
            UpdateBox.UrlGTFS = urlGTFS_Statico.Text;
            UpdateBox.UrlMD5 = urlMD5_GTFS_Statico.Text;
            urlVehicle.Text = Properties.Settings.Default.UrlVehicle;
            urlTrip.Text = Properties.Settings.Default.UrlTrip;
            urlAlert.Text = Properties.Settings.Default.UrlAlert;
            SetSelectedCityFromCurrentUrls();
            checkTripDuplicati.Checked = Properties.Settings.Default.Duplicati;
            checkTripVuoti.Checked = Properties.Settings.Default.Vuote;
            checkCSV.Checked = Properties.Settings.Default.SalvaCSV;
            checkXlsx.Checked = Properties.Settings.Default.SalvaXlsx;
            checkGrafico.Checked = Properties.Settings.Default.SalvaGrafico;
            checkMonitoraggio.Checked = Properties.Settings.Default.SalvaMonitoraggio;
            checkAlert.Checked = Properties.Settings.Default.SalvaAlert;
            checkBoxStorico.Checked = Properties.Settings.Default.StoricoAlert;
            checkFeedTrip.Checked = Properties.Settings.Default.CheckTrip;
            CheckFeedTrip_CheckedChanged(null, null);
            var radioRaggruppamento = groupBoxMonitoraggio
                .Controls.OfType<RadioButton>()
                .FirstOrDefault(r => r.Name.Equals(Properties.Settings.Default.RadioRaggruppamento));
            if (radioRaggruppamento != null)
            {
                radioRaggruppamento.Checked = true;
            }

            checkAnomalieGTFS.Checked = Properties.Settings.Default.CheckAnomalie;
            checkSovraffollamento.Checked = Properties.Settings.Default.CheckSovraffollamento;
            checkMD5.Checked = Properties.Settings.Default.CheckMD5;
            checkDettagliVettura.Checked = Properties.Settings.Default.CheckDettagliVettura;
            UpdateCityVehicleDetailsDownloadSettings();
            checkTuttoPercorso.Visible = Properties.Settings.Default.ExtraSetting;
            checkResetSempre.Visible = Properties.Settings.Default.ExtraSetting;
            urlAlert.Visible = true;
            labelAlert.Visible = true;

            int totalSeconds = Properties.Settings.Default.DeltaTSec;
            minuti.Value = totalSeconds / 60;
            secondi.Value = totalSeconds % 60;
            #endregion

            FeedConfigurationDto configuration = LeggiFileConfigurazione();
            ApplicaRegoleMonitoraggio(configuration);
            ApplicaRegoleAlert(configuration);
        }

        private FeedConfigurationDto LeggiFileConfigurazione()
        {
            string selectedCity = comboCitta.SelectedItem?.ToString();
            FeedConfigurationDto configuration = _feedManager.LoadConfiguration(
                suffixDettagliVettura: checkDettagliVettura.Checked ? selectedCity : "");
            UpdateBox.ExistNewerGTFS = false;
            UpdateBox.ExistNewerCSV = false;
            UpdateBox.ExistNewerVersion = false;
            UpdateBox.NewCSVDownloaded = false;
            UpdateBox.NewGTFSDownloaded = false;

            comboLineaMonitorata.DataSource = configuration.LineeDisponibili.ToList();
            comboLineaMonitorata.ValueMember = "Id";
            comboLineaMonitorata.DisplayMember = "NomeVisualizzato";

            var dt = new DataTable();
            using (var reader = ObjectReader.Create(configuration.LineePerGestore))
            {
                dt.Load(reader);
            }
            lineaAgenziaBindingSource.DataSource = dt;
            advancedDataGridView3.DataSource = dt;

            double[] values = configuration.DistribuzioneGestori.Select(item => (double)item.NumeroCorse).ToArray();
            string[] labels = configuration.DistribuzioneGestori.Select(item => item.Gestore).ToArray();
            string[] legendLabels = configuration.DistribuzioneGestori.Select(item => string.Format(
                "{0}: {1} su {2} ({3:P})", item.Gestore, item.NumeroCorse, item.TotaleCorse, item.PercentualeCorse)).ToArray();

            plotGTFS.Plot.Clear();
            var pie = plotGTFS.Plot.AddPie(values);
            pie.SliceLabels = labels;
            pie.LegendLabels = legendLabels;
            pie.ShowLabels = true;
            pie.ShowPercentages = false;
            pie.Explode = true;
            pie.Size = .7;
            plotGTFS.Plot.Legend(location: Alignment.LowerRight);
            plotGTFS.Plot.Title("Distribuzione delle linee tra i gestori\n(valori dedotti dal GTFS Statico)");
            plotGTFS.Plot.Style(
                figureBackground: Color.LightSkyBlue,
                dataBackground: Color.DarkGray);
            plotGTFS.Render();

            return configuration;
        }

        private void ApplicaRegoleMonitoraggio(FeedConfigurationDto configuration)
        {
            if (configuration.RegoleMonitoraggio.Count > 0)
            {
                dataGridViolazioni.DataSource = configuration.RegoleMonitoraggio.ToList();
                if (!tabMainForm.TabPages.Contains(tabMonitoraggio))
                    tabMainForm.TabPages.Add(tabMonitoraggio);
            }
            else if (tabMainForm.TabPages.Contains(tabMonitoraggio))
            {
                tabMainForm.TabPages.Remove(tabMonitoraggio);
                Log.Information("Rimosso tab RegoleMonitoraggio");
            }
        }

        private void ApplicaRegoleAlert(FeedConfigurationDto configuration)
        {
            if (configuration.LetturaAlertRiuscita)
            {
                // Creazione tab dinamica: minimizzo allocazioni temporanee
                foreach (var alert in configuration.GruppiAlert)
                {
                    if (tabMainForm.TabPages.ContainsKey(alert.Nome))
                    {
                        tabMainForm.TabPages.RemoveByKey(alert.Nome);
                    }

                    var myNewTabItem = new TabPage { Text = alert.Nome, Name = alert.Nome };
                    var myNewdataGridVetture = new DataGridView
                    {
                        AllowUserToAddRows = false,
                        AllowUserToDeleteRows = false,
                        AllowUserToOrderColumns = true,
                        AutoGenerateColumns = false,
                        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                        BackgroundColor = SystemColors.Control,
                        ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                        Dock = DockStyle.Fill,
                        Location = new Point(3, 3),
                        Name = alert.Nome,
                        ReadOnly = true
                    };

                    var columnLinea = new DataGridViewTextBoxColumn { DataPropertyName = "Linea", HeaderText = "Linea", ReadOnly = true };
                    var columnGiorno = new DataGridViewTextBoxColumn { DataPropertyName = "Giorno", HeaderText = "Giorno", ReadOnly = true };
                    var columnDa = new DataGridViewTextBoxColumn { DataPropertyName = "Da", HeaderText = "Da", ReadOnly = true };
                    var columnA = new DataGridViewTextBoxColumn { DataPropertyName = "A", HeaderText = "A", ReadOnly = true };
                    var columnVetturaDa = new DataGridViewTextBoxColumn { DataPropertyName = "VetturaDa", HeaderText = "Vettura Da", ReadOnly = true };
                    var columnVetturaA = new DataGridViewTextBoxColumn { DataPropertyName = "VetturaA", HeaderText = "Vettura A", ReadOnly = true };
                    var columnVetturaSbagliata = new DataGridViewTextBoxColumn { DataPropertyName = "Violazione", HeaderText = "Violazioni", ReadOnly = true };
                    var columnOraPrimaViolazione = new DataGridViewTextBoxColumn { DataPropertyName = "OraPrimaViolazione", HeaderText = "Prima Violazione", ReadOnly = true };
                    var columnOraUltimaViolazione = new DataGridViewTextBoxColumn { DataPropertyName = "OraUltimaViolazione", HeaderText = "Ultima Violazione", ReadOnly = true };

                    DataGridViewCellStyle timeFormat = new DataGridViewCellStyle { Format = "HH:mm:ss" };
                    columnOraPrimaViolazione.DefaultCellStyle = timeFormat;
                    columnOraUltimaViolazione.DefaultCellStyle = timeFormat;

                    myNewdataGridVetture.Columns.AddRange(new DataGridViewColumn[] {
                        columnLinea, columnGiorno, columnDa, columnA, columnVetturaDa, columnVetturaA, columnVetturaSbagliata
                    });

                    myNewTabItem.Controls.Add(myNewdataGridVetture);
                    tabMainForm.TabPages.Add(myNewTabItem);

                    myNewdataGridVetture.DataSource = alert.Regole.ToList();
                }

                if (configuration.GruppiAlert.Count > 0)
                {
                    labelRaggruppaAlert.Visible = true;
                    radioLinea.Visible = true;
                    radioLineaRegola.Visible = true;
                    radioNonRaggruppare.Visible = true;
                    checkAlert.Visible = true;
                    checkBoxStorico.Visible = true;
                }
            }
            else
            {
                labelRaggruppaAlert.Visible = false;
                radioLinea.Visible = false;
                radioLineaRegola.Visible = false;
                radioNonRaggruppare.Visible = false;
                checkAlert.Visible = false;
                checkBoxStorico.Visible = false;
            }

            if (configuration.EsitoCriteriMediaPonderata == -1)
            {
                MessageBox.Show(text: "La somma dei pesi dei campioni deve essere 1", caption: "Attenzione", buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
            }
        }
        
        private void SalvaImpostazioni(object sender, EventArgs e)
        {
            Properties.Settings.Default.UrlGTFS_Statico = urlGTFS_Statico.Text;
            Properties.Settings.Default.UrlMD5_GTFS_Statico = urlMD5_GTFS_Statico.Text;
            Properties.Settings.Default.UrlVehicle = urlVehicle.Text;
            Properties.Settings.Default.UrlTrip = urlTrip.Text;
            Properties.Settings.Default.UrlAlert = urlAlert.Text;
            Properties.Settings.Default.DeltaTSec = (int)(60 * minuti.Value + secondi.Value);
            Properties.Settings.Default.Duplicati = checkTripDuplicati.Checked;
            Properties.Settings.Default.Vuote = checkTripVuoti.Checked;
            Properties.Settings.Default.SalvaCSV = checkCSV.Checked;
            Properties.Settings.Default.SalvaXlsx = checkXlsx.Checked;
            Properties.Settings.Default.SalvaGrafico = checkGrafico.Checked;
            Properties.Settings.Default.SalvaMonitoraggio = checkMonitoraggio.Checked;
            Properties.Settings.Default.SalvaAlert = checkAlert.Checked;
            Properties.Settings.Default.StoricoAlert = checkBoxStorico.Checked;
            Properties.Settings.Default.CheckAnomalie = checkAnomalieGTFS.Checked;
            var radioRaggruppamento = groupBoxMonitoraggio
                .Controls.OfType<RadioButton>()
                .FirstOrDefault(r => r.Checked);
            if (radioRaggruppamento != null) Properties.Settings.Default.RadioRaggruppamento = radioRaggruppamento.Name;
            Properties.Settings.Default.CheckSovraffollamento = checkSovraffollamento.Checked;
            Properties.Settings.Default.CheckMD5 = checkMD5.Checked;
            Properties.Settings.Default.CheckDettagliVettura = checkDettagliVettura.Checked;
            Properties.Settings.Default.Save();

            // Applica le impostazioni rilevanti al manager (export/serializzazione)
            try
            {
                _feedManager.ConfigureExports(
                    saveXlsx: Properties.Settings.Default.SalvaXlsx,
                    saveGrafico: Properties.Settings.Default.SalvaGrafico,
                    saveAlert: Properties.Settings.Default.SalvaAlert,
                    saveMonitoraggio: Properties.Settings.Default.SalvaMonitoraggio,
                    saveAnomalieGTFS: Properties.Settings.Default.CheckAnomalie,
                    saveSovraffollamento: Properties.Settings.Default.CheckSovraffollamento,
                    saveCSV: Properties.Settings.Default.SalvaCSV,
                    alertEnabled: true);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Impossibile applicare le impostazioni al FeedGTFSManager");
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_feedManager.IsMonitoring)
            {
                DialogResult dialog = MessageBox.Show("Interrompere il monitoraggio ed uscire", "Conferma Uscita", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialog == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
                else if (!string.IsNullOrEmpty(fileName))
                {
                    var _ = _feedManager.ExportGrid();
                }
            }

            // Dispose della facciata
            _feedManager?.Dispose();
        }

        private void Random(object sender, EventArgs e)
        {
            _currentSnapshot = _currentSnapshot ?? _feedManager.GetSnapshot();
            if (_currentSnapshot.PuntiGrafico == null)
                _currentSnapshot.PuntiGrafico = new List<MonitorChartPointDto>();

            DateTime t0;
            IList<MonitorChartPointDto> graf = _currentSnapshot.PuntiGrafico;
            if (graf.Count == 0)
            {
                t0 = DateTime.Now;
            }
            else
            {
                t0 = graf[graf.Count - 1].DataOra;
            }
            if (graf.Count < 100000)
            {
                var rand = new Random(0);
                int aggregate = 500;
                for (int i = 0; i < 500; i++)
                {
                    int aggiunte = rand.Next(0, 10);
                    int atac = rand.Next(400, 600);
                    int tpl = rand.Next(150, 280);
                    int rilevate = atac + tpl;
                    aggregate += (int)(3 * aggiunte / 10);
                    if (aggregate < rilevate)
                        aggregate = rilevate;
                    int tolte = rand.Next(0, 10);
                    var nuovoMonitoraggio = new MonitorChartPointDto
                    {
                        DataOra = t0.AddSeconds(i * 30),
                        TotaleAggregato = aggregate,
                        AggregatoAtac = aggregate * atac / Math.Max(rilevate, 1),
                        AggregatoTpl = aggregate * tpl / Math.Max(rilevate, 1),
                        Rilevate = rilevate,
                        Atac = atac,
                        Tpl = tpl,
                        Aggiunte = aggiunte,
                        Rimosse = tolte
                    };
                    graf.Add(nuovoMonitoraggio);
                }
            }
            AggiornaScottPlot(graf);
        }

        private void CheckXlsx_CheckedChanged(object sender, EventArgs e)
        {
            checkGrafico.Enabled = checkXlsx.Checked;
            checkMonitoraggio.Enabled = checkXlsx.Checked;
            checkAlert.Enabled = checkXlsx.Checked;
        }

        public void Colora()
        {
            if (_currentSnapshot == null || dataGridViolazioni.DataSource == null)
                return;

            dataGridViolazioni.SuspendLayout();
            var dataSource = dataGridViolazioni.DataSource as IEnumerable<MonitorViolationDto>;
            if (dataSource == null)
            {
                dataGridViolazioni.ResumeLayout();
                return;
            }

            List<MonitorViolationDto> violations = dataSource.ToList();
            int rowCount = Math.Min(dataGridViolazioni.RowCount, violations.Count);
            for (int i = 0; i < rowCount; i++)
            {
                DataGridViewRow row = dataGridViolazioni.Rows[i];
                MonitorViolationDto riga = violations[i];
                if (riga.VettureRilevate.HasValue && riga.VetturePreviste.HasValue &&
                    riga.VettureRilevate.Value < riga.VetturePreviste.Value)
                {
                    DateTime end = riga.OraUltimaViolazione ?? _currentSnapshot.UltimoFeed ?? DateTime.MinValue;
                    TimeSpan span = end - riga.OraPrimaViolazione.GetValueOrDefault(DateTime.MinValue);
                    double totalMinutes = span.TotalMinutes;
                    if (totalMinutes < riga.TempoBonus)
                    {
                        row.Cells[6].Style.ForeColor = ColorTranslator.FromHtml("#856404");
                        row.Cells[6].Style.BackColor = ColorTranslator.FromHtml("#fff3cd");
                    }
                    else
                    {
                        row.Cells[6].Style.ForeColor = ColorTranslator.FromHtml("#721c24");
                        row.Cells[6].Style.BackColor = ColorTranslator.FromHtml("#f8d7da");
                    }
                }
                else
                {
                    row.Cells[6].Style.ForeColor = ColorTranslator.FromHtml("#155724");
                    row.Cells[6].Style.BackColor = ColorTranslator.FromHtml("#d4edda");
                }
            }
            dataGridViolazioni.ResumeLayout();
        }

        private void TabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((sender as TabControl)?.SelectedTab == tabMonitoraggio && _currentSnapshot?.UltimoFeed.HasValue == true)
            {
                Colora();
            }
        }

        private async void ResetAcquisizioneAsync(object sender, EventArgs e)
        {
            await Task.Run(() => UpdateBox.TaskCheckUpdate(forceDownload: true));
            RestartFile();
        }

        private void CheckFeedTrip_CheckedChanged(object sender, EventArgs e)
        {
            urlTrip.Enabled = checkFeedTrip.Checked;
        }

        private void RileggiRegole(object sender, EventArgs e)
        {
            RestartFile();
        }

        private void CheckReset_CheckedChanged(object sender, EventArgs e)
        {
            if (checkReset.Checked)
            {
                TimeSpan oraReset = dateTimeReset.Value.AddSeconds(01).TimeOfDay;
                DateTime now = DateTime.Now;
                if (now.TimeOfDay > oraReset)
                {
                    now = now.AddDays(1);
                }
                _feedManager.DataResetMonitoraggio = new DateTime(now.Year, now.Month, now.Day, oraReset.Hours, oraReset.Minutes, oraReset.Seconds);
            }
            else
            {
                _feedManager.DataResetMonitoraggio = null;
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            UpdateBox.Check(true);
            var result = UpdateBox.ShowDialog();
            if (result == DialogResult.Yes)
            {
                RestartFile();
            }
        }

        public void ResetUI()
        {
            labelTotaleRighe.Text = "---";
            labelTotaleIdVettura.Text = "---";
            labelTotaleMatricola.Text = "---";
            labelTotaleMatricolaATAC.Text = "---";
            labelTotaleMatricolaTPL.Text = "---";
            labelFerroAtac.Text = "---";
            labelBusAtac.Text = "---";
            labelTramAtac.Text = "---";
            labelFilobusAtac.Text = "---";
            labelMiniBusEleAtac.Text = "---";
            labelFurgoncinoAtac.Text = "---";

            lblOraLettura.Text = "hh:mm:ss";
            labelAtac.Text = "---";
            labelTPL.Text = "---";
            labelTot.Text = "---";
            labelLetture.Text = "---";
            labelFeedLetti.Text = "---";

            labelBusTPL.Text = "---";
            labelPullmanTPL.Text = "---";
            labelAltroTpl.Text = "---";
            labelAltroAtac.Text = "---";
            labelPonderatiATAC.Text = "---";
            labelPonderatiTPL.Text = "---";

            textBox1.Clear();
            textBox2.Clear();
            textBox3.Clear();
            textBox4.Clear();
            imgUrl1.Image = null;
            imgUrl2.Image = null;
            UpdateBox.ResetUI();

            buttonVerificaAggiornamenti.Image = Resources.available_updates_16;
            Refresh();
        }

        #region Metodi Privati per gestire filtro/ordinamento delle Zuby.AdDGVAdvancedDataGridView
        private void AdvancedDataGridView1_SortStringChanged(object sender, Zuby.ADGV.AdvancedDataGridView.SortEventArgs e)
        {
            extendedVehicleInfoBindingSource.Sort = advancedDataGridView1.SortString;
        }

        private void AdvancedDataGridView1_FilterStringChanged(object sender, Zuby.ADGV.AdvancedDataGridView.FilterEventArgs e)
        {
            extendedVehicleInfoBindingSource.Filter = advancedDataGridView1.FilterString;
        }

        private void AdvancedDataGridView2_SortStringChanged(object sender, Zuby.ADGV.AdvancedDataGridView.SortEventArgs e)
        {
            bindingSourceAttuale.Sort = advancedDataGridView2.SortString;
        }

        private void AdvancedDataGridView2_FilterStringChanged(object sender, Zuby.ADGV.AdvancedDataGridView.FilterEventArgs e)
        {
            bindingSourceAttuale.Filter = advancedDataGridView2.FilterString;
        }
        #endregion

        private void UrlMD5_GTFS_Statico_TextChanged(object sender, EventArgs e)
        {
            if (!isApplyingCityDefaults && comboCitta.SelectedItem != null)
                comboCitta.SelectedItem = "Personalizzato";

            UpdateBox.UrlMD5 = urlMD5_GTFS_Statico.Text;
        }

        private void UrlGTFS_Statico_TextChanged(object sender, EventArgs e)
        {
            if (!isApplyingCityDefaults && comboCitta.SelectedItem != null)
                comboCitta.SelectedItem = "Personalizzato";

            UpdateBox.UrlGTFS = urlGTFS_Statico.Text;
        }

        private void CheckDettagliVettura_CheckedChanged(object sender, EventArgs e)
        {
            string selectedCity = comboCitta.SelectedItem?.ToString();
            bool isConfiguredCity = !string.IsNullOrWhiteSpace(selectedCity) && cityDefaults.ContainsKey(selectedCity);
            _feedManager.ConfigureVehicleDetails(checkDettagliVettura.Checked && isConfiguredCity ? selectedCity : "");
            UpdateCityVehicleDetailsDownloadSettings();
        }

        private void GridAvvisi_FilterStringChanged(object sender, Zuby.ADGV.AdvancedDataGridView.FilterEventArgs e)
        {
            bindingSourceAvvisi.Filter = GridAvvisi.FilterString;
        }

        private void GridAvvisi_SortStringChanged(object sender, Zuby.ADGV.AdvancedDataGridView.SortEventArgs e)
        {
            bindingSourceAvvisi.Sort = GridAvvisi.SortString;
        }

        private void checkAlert_CheckedChanged(object sender, EventArgs e)
        {
            _feedManager.ConfigureExports(
                saveAlert: checkAlert.Checked);
        }

        private void checkAnomalieGTFS_CheckedChanged(object sender, EventArgs e)
        {
            _feedManager.ConfigureExports(
                saveAnomalieGTFS: checkAnomalieGTFS.Checked);
        }

        private void checkGrafico_CheckedChanged(object sender, EventArgs e)
        {
            _feedManager.ConfigureExports(
                saveGrafico: checkGrafico.Checked);
        }
    }
}
