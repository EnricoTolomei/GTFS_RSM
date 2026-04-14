using AtacFeed.Properties;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using FastMember;
using GTFS.Entities;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Table;
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
        private string fileName;
        
        private DateTime? _lastProcessedFeedTimestamp;
        // Sostituiti i due manager con la facade
        private readonly FeedGTFSManager _feedManager = new FeedGTFSManager();

        private readonly UpdateBox UpdateBox = new UpdateBox();

        public FormGTFS_RSM()
        {
            InitializeComponent();

            // Subscription agli eventi del facade. UI thread marshal con BeginInvoke.
            _feedManager.VehiclesUpdated += (s, e) =>
            {
                if (IsHandleCreated)
                    BeginInvoke(new Action(UpdateUI));
            };
            _feedManager.AlertsUpdated += (s, e) =>
            {
                //if (this.IsHandleCreated)
                //    BeginInvoke(new Action(AcquisizioneNEW));
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
                        //Log.Error( ex.Message, "Errore Feed");
                        labelLetture.Text = _feedManager.NumeroLetture.ToString();
                        labelFeedLetti.Text = _feedManager.NumeroFeedValidi.ToString();
                        lblOraLettura.Text = _feedManager.VehicleManager.LastDataFeed?.ToString("HH:mm") ?? "--";
                        Log.Error("===> {Exception}", ex);
                    }));
                }
            };
            _feedManager.VehicleReadStarted += (s, args) =>
            {
                if (IsHandleCreated)
                    BeginInvoke(new Action(() =>
                    {
                        // esempio: mostrare stato nella statusbar / label
                        textBox1.Text += $"{Environment.NewLine}Lettura feed in corso: {Path.GetFileName(args.Url)}{Environment.NewLine}";
                        // opzionale: cambiare icona/spinner
                        //imgUrl1.Image = Properties.Resources.spinner; // se hai una risorsa spinner
                        if (args.IsRiserva)
                            imgUrl2.Image = Resources.spinner;
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
                                imgUrl2.Image = Resources.verde; // esempio
                            else
                                imgUrl1.Image = Resources.verde; // esempio
                        }
                        else if (args.Error != null)
                        {
                            //labelStatus.Text = $"Lettura fallita: {Path.GetFileName(args.Url)} - {args.Error.Message}";
                            if (args.IsRiserva)
                                imgUrl2.Image = Resources.rosso; // esempio
                            else
                                imgUrl1.Image = Resources.rosso; // esempio
                        }
                        else
                        {
                            //labelStatus.Text = $"Lettura terminata (code {args.Code}): {Path.GetFileName(args.Url)}";
                            if (args.IsRiserva)
                                imgUrl2.Image = Resources.arancio; // esempio
                            else
                                imgUrl1.Image = Resources.arancio; // esempio

                        }

                    }));
            };

            _feedManager.VehicleElabStarted += (s, args) =>
            {
                if (IsHandleCreated)
                    BeginInvoke(new Action(() =>
                    {
                        textBox1.Text += $"{Environment.NewLine} Inizio ELABORAZIONE FEED{Environment.NewLine}";
                        pictureBox1.Image = Resources.processing;
                    }));

            };
            _feedManager.VehicleElabCompleted += (s, args) =>
            {
                if (IsHandleCreated)
                    BeginInvoke(new Action(() =>
                    {
                        textBox1.Text += $"{Environment.NewLine} FINE  ELABORAZIONE FEED {Environment.NewLine}";
                        pictureBox1.Image = null;

                    }));
            };
        }

        public void UpdateUI()
        {
            try
            {
                labelLetture.Text = _feedManager.NumeroLetture.ToString();
                labelFeedLetti.Text = _feedManager.NumeroFeedValidi.ToString();
                lblOraLettura.Text = _feedManager.VehicleManager.LastDataFeed?.ToString("HH:mm") ?? "--";
                textBox1.Clear();
                textBox2.Clear();

                var vm = _feedManager.VehicleManager;
                // quick checks and delegate the heavy lifting
                if (vm == null || vm.CodeFeed != 0)
                    return;

                PopulateUIFromVehicleManager(vm);
            }
            catch (Exception ex)
            {
                textBox1.AppendText(ex.Message);
                Log.Error(ex, "Errore Generico");
            }
        }

        // Consolidated UI population extracted from duplicated UpdateUI/AcquisizioneNEW logic
        private void PopulateUIFromVehicleManager(FeedVehicleManager vm)
        {
            // Log diagnostico
            Log.Information("AcquisizioneNEW START - VehicleManager null? {IsNull} LastValidFeedEntities={LastCount} ElencoVettureCount={ElencoCount} ElencoAggregatoCount={AggCount}",
                vm == null,
                vm?.LastValidFeed?.Entities?.Count ?? -1,
                vm?.ElencoVetture?.Count ?? -1,
                vm?.ElencoAggregatoVetture?.Count ?? -1);

            // avoid reprocessing same timestamp
            if (vm.LastDataFeed.HasValue && _lastProcessedFeedTimestamp.HasValue && vm.LastDataFeed.Value == _lastProcessedFeedTimestamp.Value)
            {
                Log.Information("AcquisizioneNEW: feed con timestamp {Ts} già processato, skip", vm.LastDataFeed.Value);
                return;
            }

            DateTime lastDataFeedVehicle = vm.LastDataFeed.GetValueOrDefault();

            // AlertManager snapshot
            var alertMgr = _feedManager.AlertManager;
            if (!string.IsNullOrWhiteSpace(urlAlert.Text))
            {
                try
                {
                    if ((alertMgr.CodeFeed == 0) && alertMgr.DiversoDaPrecedente)
                    {
                        var dtAvvisi = new DataTable();
                        using (var reader = ObjectReader.Create(alertMgr.Avvisi))
                        {
                            dtAvvisi.Load(reader);
                        }
                        bindingSourceAvvisi.DataSource = dtAvvisi;
                        GridAvvisi.DataSource = bindingSourceAvvisi;
                    }
                }
                catch (Exception exc)
                {
                    textBox1.AppendText($"Feed Alert NON LETTO {Environment.NewLine}");
                    textBox1.AppendText($"{exc.Message} {Environment.NewLine}{Environment.NewLine}");
                }
            }

            // Linee anomale
            var lineeAnomale = vm.LineeAnomale();
            if (lineeAnomale != null && lineeAnomale.Count > 0)
            {
                textBox2.Text += $"Le seguenti linee {string.Join(", ", lineeAnomale)}{Environment.NewLine} NON sono riportate nel file statico routes.txt{Environment.NewLine}{Environment.NewLine}";
            }

            // Aggiunte/Tolte
            if (vm.ElencoPrecedente != null && vm.ElencoPrecedente.Count > 0)
            {
                if (vm.VettureAggiunte != null && vm.VettureAggiunte.Count > 0)
                {
                    var sbA = new StringBuilder();
                    foreach (var vettura in vm.VettureAggiunte)
                    {
                        sbA.AppendFormat("{0} - {1} rilevata alle {2:HH:mm:ss} {3}", vettura.IdVettura, vettura.Matricola, lastDataFeedVehicle, Environment.NewLine);
                    }
                    textBox3.AppendText(sbA.ToString());
                }

                if (vm.VettureTolte != null && vm.VettureTolte.Count > 0)
                {
                    var sbT = new StringBuilder();
                    foreach (var vettura in vm.VettureTolte)
                    {
                        sbT.AppendFormat("{0} - {1} NON rilevata alle {2:HH:mm:ss} {3}", vettura.IdVettura, vettura.Matricola, lastDataFeedVehicle, Environment.NewLine);
                    }
                    textBox4.AppendText(sbT.ToString());
                }

                // Partenza avanzata
                if (vm.PartenzaAvanzata != null && vm.PartenzaAvanzata.Any())
                {
                    textBox2.AppendText("Vetture con 'partenza avanzata'" + Environment.NewLine);
                    foreach (var errore in vm.PartenzaAvanzata)
                    {
                        string line = $"Matricola {errore.Matricola} Linea {errore.Linea} Fermata {errore.CurrentStopSequence}{Environment.NewLine}";
                        int start = textBox2.TextLength;
                        textBox2.AppendText(line);
                        textBox2.Select(start, line.Length);
                        textBox2.SelectionColor = Color.CornflowerBlue;
                        textBox2.SelectionIndent = 10;
                        vm.AnomaliaGTFS.Add(new ErroriGTFS(errore, (int)errore.CurrentStopSequence));
                    }
                    textBox2.AppendText(Environment.NewLine);
                }

                // Riagganciate
                if (vm.VettureRiagganciate != null && vm.VettureRiagganciate.Any())
                {
                    textBox2.AppendText("Vetture 'riagganciate'" + Environment.NewLine);
                    foreach (var errore in vm.VettureRiagganciate)
                    {
                        string line = $"Matricola {errore.Matricola} Linea {errore.Linea} Fermata {errore.CurrentStopSequence}{Environment.NewLine}";
                        int start = textBox2.TextLength;
                        textBox2.AppendText(line);
                        textBox2.Select(start, line.Length);
                        textBox2.SelectionColor = Color.CornflowerBlue;
                        textBox2.SelectionIndent = 10;

                        uint ultimaFermataRilevata = vm.ElencoAggregatoVetture
                                .Where(x => x.TripId == errore.TripId && x.Matricola == errore.Matricola)
                                .Max(x => x.CurrentStopSequence);
                        int delta = (int)(errore.CurrentStopSequence - ultimaFermataRilevata);
                        vm.AnomaliaGTFS.Add(new ErroriGTFS(errore, delta));
                    }
                    textBox2.AppendText(Environment.NewLine);
                }

                // Percorso anomalo
                if (vm.PercorsoAnomalo != null && vm.PercorsoAnomalo.Count > 0)
                {
                    textBox2.AppendText("Vetture con progressivo fermate 'bucato'" + Environment.NewLine);
                    foreach (var errore in vm.PercorsoAnomalo)
                    {
                        string line = $"Matricola {errore.Matricola} Linea {errore.Linea} Fermata {errore.CurrentStopSequence} => 'balzo' di {errore.Delta}{Environment.NewLine}";
                        int start = textBox2.TextLength;
                        textBox2.AppendText(line);
                        textBox2.Select(start, line.Length);
                        if (errore.Delta < 0)
                        {
                            textBox2.SelectionColor = Color.OrangeRed;
                            textBox2.SelectionFont = new Font(textBox2.SelectionFont, FontStyle.Bold);
                        }
                        else
                        {
                            textBox2.SelectionColor = Color.DarkOrange;
                        }
                        textBox2.SelectionIndent = 10;
                    }
                    textBox2.AppendText(Environment.NewLine);
                }
            }

            // Trip duplicati
            var tripDuplicatiFeedVehicle = vm.TripDuplicati();
            if (tripDuplicatiFeedVehicle != null && tripDuplicatiFeedVehicle.Count > 0)
            {
                textBox2.AppendText("Trip Duplicati" + Environment.NewLine);
                foreach (var tripDuplicato in tripDuplicatiFeedVehicle)
                {
                    var elencoVettureSuTripIdDuplicato = string.Join(", ", vm.FeedEntities.Where(x => x.Vehicle.Trip != null && x.Vehicle.Trip.TripId == tripDuplicato).Select(x => x.Vehicle.Vehicle.Label));
                    string line = $"Trip {tripDuplicato}\tVetture:[{elencoVettureSuTripIdDuplicato}]{Environment.NewLine}";
                    int start = textBox2.TextLength;
                    textBox2.AppendText(line);
                    textBox2.Select(start, line.Length);
                    textBox2.SelectionColor = Color.Tomato;
                    textBox2.SelectionIndent = 10;
                }
                textBox2.AppendText(Environment.NewLine);
            }

            // Vetture senza matricola
            var vettureSenzaMatricola = vm.VettureSenzaMatricola();
            if (vettureSenzaMatricola != null && vettureSenzaMatricola.Count > 0)
            {
                textBox2.AppendText("Vetture Senza Matricola" + Environment.NewLine);
                int start = textBox2.TextLength;
                var sb = new StringBuilder();
                foreach (var vettura in vettureSenzaMatricola)
                {
                    sb.AppendFormat("IdVettura {0}\t Matricola:[{1}]{2}", vettura.IdVettura, vettura.Matricola, Environment.NewLine);
                }
                string vetture = sb.ToString();
                textBox2.AppendText(vetture);
                textBox2.Select(start, vetture.Length);
                textBox2.SelectionColor = Color.DarkGray;
                textBox2.SelectionIndent = 10;
                textBox2.AppendText(Environment.NewLine);
            }

            textBox2.AppendText(Environment.NewLine);
            textBox2.Select(0, 0);

            labelTotaleRighe.Text = vm.ElencoAggregatoVetture.Count.ToString();
            labelTotaleIdVettura.Text = vm.TotaleIdVettura.ToString();
            labelTotaleMatricola.Text = vm.TotaleMatricola.ToString();

            // Statistiche
            var stat = vm.StatisticheAttuali ?? new Statistiche();
            labelBusAtac.Text = stat.RilevatoBusAtac.ToString();
            labelTramAtac.Text = stat.RilevatoTramAtac.ToString();
            labelFilobusAtac.Text = stat.RilevatoFilobusAtac.ToString();
            labelMiniBusEleAtac.Text = stat.RilevatoMinibusElettrici.ToString();
            labelFurgoncinoAtac.Text = stat.RilevatoFurgoncini.ToString();
            labelFerroAtac.Text = stat.RilevatoFerro.ToString();
            labelAltroAtac.Text = stat.RilevatoAltroAtac.ToString();
            labelBusTPL.Text = stat.RilevatoBusTpl.ToString();
            labelPullmanTPL.Text = stat.RilevatoPullmanTpl.ToString();
            labelAltroTpl.Text = stat.RilevatoAltroTpl.ToString();

            labelTotaleMatricolaATAC.Text = vm.TotaleMatricolaAtac.ToString();
            labelTotaleMatricolaTPL.Text = vm.TotaleMatricolaTPL.ToString();

            // DataTable per grid principali - carica usando ObjectReader
            var dt = new DataTable();
            using (var reader = ObjectReader.Create(vm.ElencoAggregatoVetture))
            {
                dt.Load(reader);
            }
            extendedVehicleInfoBindingSource.DataSource = dt;
            advancedDataGridView1.DataSource = extendedVehicleInfoBindingSource;

            // Attuale - applica ordinamento se necessario evitando ToList doppio
            var elencoAttuale = vm.ElencoVetture;
            DataTable dtAttuale = new DataTable();
            if (string.IsNullOrEmpty(bindingSourceAttuale.Sort))
            {
                using (var reader = ObjectReader.Create(elencoAttuale.OrderBy(x => x.Linea?.Length).ThenBy(x => x.Linea)))
                {
                    dtAttuale.Load(reader);
                }
            }
            else
            {
                using (var reader = ObjectReader.Create(elencoAttuale))
                {
                    dtAttuale.Load(reader);
                }
            }
            bindingSourceAttuale.DataSource = dtAttuale;
            advancedDataGridView2.DataSource = bindingSourceAttuale;

            // Liste e conteggi
            var listaMezziSuLinea = vm.ElencoVetture.Where(x => x.TripId != null).ToList();
            var listaBusAttesa = vm.ElencoVetture.Where(x => x.TripId == null).ToList();
            int numVettureTPLFeedVehicle = vm.ElencoVetture.Count(i => i.TipoMezzoTrasporto == 3 || i.TipoMezzoTrasporto == 4 || i.TipoMezzoTrasporto == -3);

            int busLinea = listaMezziSuLinea.Count;
            int busAttesa = listaBusAttesa.Count;
            int busTotale = busLinea + busAttesa;
            textBox1.AppendText($"Totale Vetture Rilevate sul Feed Vehicle {busTotale}");

            // Raggruppamento per gestore - cache StatisticheAttuali.ServizioRaggruppato
            var sbGestori = new StringBuilder();
            var raggruppatoGestore = vm.StatisticheAttuali?.ServizioRaggruppato ?? Enumerable.Empty<ServizioRaggruppato>();
            var grouped = raggruppatoGestore.GroupBy(x => x.Agenzia).Select(g => new { Gestore = g.Key, Totale = g.Sum(x => x.Num) });

            foreach (var gestore in grouped)
            {
                sbGestori.AppendLine();
                sbGestori.AppendFormat("{0} - {1}{2}", gestore.Gestore, gestore.Totale, Environment.NewLine);
                foreach (var servizio in raggruppatoGestore.Where(x => x.Agenzia == gestore.Gestore))
                {
                    sbGestori.AppendFormat("    {0}\t{1}{2}", servizio.Servizio, servizio.Num, Environment.NewLine);
                }
            }
            textBox1.AppendText(sbGestori.ToString());

            labelTPL.Text = numVettureTPLFeedVehicle.ToString();
            labelAtac.Text = (busTotale - numVettureTPLFeedVehicle).ToString();
            labelTot.Text = busTotale.ToString();

            labelPonderatiATAC.Text = Math.Round(vm.PonderateAtac).ToString();
            labelPonderatiTPL.Text = Math.Round(vm.PonderateTPL).ToString();

            if (string.IsNullOrEmpty(fileName))
            {
                fileName = string.Format("Feed_{0:yyyy-MM-dd (HH_mm_ss)}", lastDataFeedVehicle);
            }

            if (vm.GTFS_RSM.RegoleMonitoraggio != null && vm.GTFS_RSM.RegoleMonitoraggio.Count > 0)
            {
                dataGridViolazioni.DataSource = vm.ViolazioniLeneeMonitorate();
                if (tabMainForm.SelectedTab == tabMonitoraggio)
                {
                    Colora();
                }
            }

            // Alert tabs - prendo snapshot da GTFS_RSM
            var alerts = vm.GTFS_RSM?.AlertsDaControllare;
            if (alerts != null)
            {
                foreach (var alert in alerts)
                {
                    var page = tabMainForm.TabPages[alert.Name];
                    if (page == null) continue;
                    var ctrl = page.Controls[alert.Name] as DataGridView;
                    if (ctrl == null) continue;
                    if (checkBoxStorico.Checked)
                    {
                        ctrl.DataSource = alert.ViolazioniAlert.ToList();
                    }
                    else
                    {
                        ctrl.DataSource = vm.ViolazioniAlertAttuali != null ? vm.ViolazioniAlertAttuali.ToList() : null;
                    }
                }
            }

            // Grafico
            AggiornaScottPlot();

            // Segnamo come processato questo timestamp (solo se abbiamo raggiunto il rendering)
            try
            {
                if (vm.LastDataFeed.HasValue)
                    _lastProcessedFeedTimestamp = vm.LastDataFeed.Value;
            }
            catch
            {
                // ignore
            }
        }
        private void AcquisizioneNEW()
        {
            try
            {
                textBox1.Clear();
                textBox2.Clear();
                string routeID = comboBox1.SelectedValue?.ToString() ?? "-1";

                // Usa AlertManager tramite facade
                var alertMgr = _feedManager.AlertManager;
                if (!string.IsNullOrWhiteSpace(urlAlert.Text))
                {
                    try
                    {
                        if ((alertMgr.CodeFeed == 0) && alertMgr.DiversoDaPrecedente)
                        {
                            var dtAvvisi = new DataTable();
                            using (var reader = ObjectReader.Create(alertMgr.Avvisi))
                            {
                                dtAvvisi.Load(reader);
                            }
                            bindingSourceAvvisi.DataSource = dtAvvisi;
                            GridAvvisi.DataSource = bindingSourceAvvisi;
                        }
                    }
                    catch (Exception exc)
                    {
                        textBox1.AppendText($"Feed Alert NON LETTO {Environment.NewLine}");
                        textBox1.AppendText($"{exc.Message} {Environment.NewLine}{Environment.NewLine}");
                    }
                }

                // Usa VehicleManager tramite facade
                var vm = _feedManager.VehicleManager;

                // Log diagnostico: verifichiamo cosa c'è prima di procedere
                Log.Information("AcquisizioneNEW START - VehicleManager null? {IsNull} LastValidFeedEntities={LastCount} ElencoVettureCount={ElencoCount} ElencoAggregatoCount={AggCount}",
                    vm == null,
                    vm?.LastValidFeed?.Entities?.Count ?? -1,
                    vm?.ElencoVetture?.Count ?? -1,
                    vm?.ElencoAggregatoVetture?.Count ?? -1);

                bool feedAvailable = vm != null && vm.CodeFeed == 0;
                labelLetture.Text = _feedManager.NumeroLetture.ToString();

                if (!feedAvailable)
                    return;

                DateTime lastDataFeedVehicle = vm.LastDataFeed.GetValueOrDefault();

                // Evitiamo di processare più volte lo stesso feed: confronto per timestamp
                if (vm.LastDataFeed.HasValue && _lastProcessedFeedTimestamp.HasValue && vm.LastDataFeed.Value == _lastProcessedFeedTimestamp.Value)
                {
                    Log.Information("AcquisizioneNEW: feed con timestamp {Ts} già processato, skip", vm.LastDataFeed.Value);
                    return;
                }

                // Linee anomale
                var lineeAnomale = vm.LineeAnomale();
                if (lineeAnomale != null && lineeAnomale.Count > 0)
                {
                    textBox2.Text = $"Le seguenti linee {string.Join(", ", lineeAnomale)}{Environment.NewLine} NON sono riportate nel file statico routes.txt{Environment.NewLine}{Environment.NewLine}";
                }

                // Aggiunte/Tolte: accumulo stringhe in StringBuilder per ridurre aggiornamenti UI ripetuti
                if (vm.ElencoPrecedente != null && vm.ElencoPrecedente.Count > 0)
                {
                    if (vm.VettureAggiunte != null && vm.VettureAggiunte.Count > 0)
                    {
                        
                        var sbA = new StringBuilder();
                        foreach (var vettura in vm.VettureAggiunte)
                        {
                            sbA.AppendFormat("{0} - {1} rilevata alle {2:HH:mm:ss} {3}", vettura.IdVettura, vettura.Matricola, lastDataFeedVehicle, Environment.NewLine);
                        }
                        textBox3.AppendText(sbA.ToString());                                                
                    }

                    if (vm.VettureTolte != null && vm.VettureTolte.Count > 0)
                    {
                        var sbT = new StringBuilder();
                        foreach (var vettura in vm.VettureTolte)
                        {
                            sbT.AppendFormat("{0} - {1} NON rilevata alle {2:HH:mm:ss} {3}", vettura.IdVettura, vettura.Matricola, lastDataFeedVehicle, Environment.NewLine);
                        }
                        textBox4.AppendText(sbT.ToString());
                    }

                    // Partenza avanzata
                    if (vm.PartenzaAvanzata != null && vm.PartenzaAvanzata.Any())
                    {
                        textBox2.AppendText("Vetture con 'partenza avanzata'" + Environment.NewLine);
                        foreach (var errore in vm.PartenzaAvanzata)
                        {
                            string line = $"Matricola {errore.Matricola} Linea {errore.Linea} Fermata {errore.CurrentStopSequence}{Environment.NewLine}";
                            int start = textBox2.TextLength;
                            textBox2.AppendText(line);
                            textBox2.Select(start, line.Length);
                            textBox2.SelectionColor = Color.CornflowerBlue;
                            textBox2.SelectionIndent = 10;
                            vm.AnomaliaGTFS.Add(new ErroriGTFS(errore, (int)errore.CurrentStopSequence));
                        }
                        textBox2.AppendText(Environment.NewLine);
                    }

                    // Riagganciate
                    if (vm.VettureRiagganciate != null && vm.VettureRiagganciate.Any())
                    {
                        textBox2.AppendText("Vetture 'riagganciate'" + Environment.NewLine);
                        foreach (var errore in vm.VettureRiagganciate)
                        {
                            string line = $"Matricola {errore.Matricola} Linea {errore.Linea} Fermata {errore.CurrentStopSequence}{Environment.NewLine}";
                            int start = textBox2.TextLength;
                            textBox2.AppendText(line);
                            textBox2.Select(start, line.Length);
                            textBox2.SelectionColor = Color.CornflowerBlue;
                            textBox2.SelectionIndent = 10;

                            uint ultimaFermataRilevata = vm.ElencoAggregatoVetture
                                    .Where(x => x.TripId == errore.TripId && x.Matricola == errore.Matricola)
                                    .Max(x => x.CurrentStopSequence);
                            int delta = (int)(errore.CurrentStopSequence - ultimaFermataRilevata);
                            vm.AnomaliaGTFS.Add(new ErroriGTFS(errore, delta));
                        }
                        textBox2.AppendText(Environment.NewLine);
                    }

                    // Percorso anomalo
                    if (vm.PercorsoAnomalo != null && vm.PercorsoAnomalo.Count > 0)
                    {
                        textBox2.AppendText("Vetture con progressivo fermate 'bucato'" + Environment.NewLine);
                        foreach (var errore in vm.PercorsoAnomalo)
                        {
                            string line = $"Matricola {errore.Matricola} Linea {errore.Linea} Fermata {errore.CurrentStopSequence} => 'balzo' di {errore.Delta}{Environment.NewLine}";
                            int start = textBox2.TextLength;
                            textBox2.AppendText(line);
                            textBox2.Select(start, line.Length);
                            if (errore.Delta < 0)
                            {
                                textBox2.SelectionColor = Color.OrangeRed;
                                textBox2.SelectionFont = new Font(textBox2.SelectionFont, FontStyle.Bold);
                            }
                            else
                            {
                                textBox2.SelectionColor = Color.DarkOrange;
                            }
                            textBox2.SelectionIndent = 10;
                        }
                        textBox2.AppendText(Environment.NewLine);
                    }
                }

                // Trip duplicati
                var tripDuplicatiFeedVehicle = vm.TripDuplicati();
                if (tripDuplicatiFeedVehicle != null && tripDuplicatiFeedVehicle.Count > 0)
                {
                    textBox2.AppendText("Trip Duplicati" + Environment.NewLine);
                    foreach (var tripDuplicato in tripDuplicatiFeedVehicle)
                    {
                        var elencoVettureSuTripIdDuplicato = string.Join(", ", vm.FeedEntities.Where(x => x.Vehicle.Trip != null && x.Vehicle.Trip.TripId == tripDuplicato).Select(x => x.Vehicle.Vehicle.Label));
                        string line = $"Trip {tripDuplicato}\tVetture:[{elencoVettureSuTripIdDuplicato}]{Environment.NewLine}";
                        int start = textBox2.TextLength;
                        textBox2.AppendText(line);
                        textBox2.Select(start, line.Length);
                        textBox2.SelectionColor = Color.Tomato;
                        textBox2.SelectionIndent = 10;
                    }
                    textBox2.AppendText(Environment.NewLine);
                }

                // Vetture senza matricola
                var vettureSenzaMatricola = vm.VettureSenzaMatricola();
                if (vettureSenzaMatricola != null && vettureSenzaMatricola.Count > 0)
                {
                    textBox2.AppendText("Vetture Senza Matricola" + Environment.NewLine);
                    int start = textBox2.TextLength;
                    var sb = new StringBuilder();
                    foreach (var vettura in vettureSenzaMatricola)
                    {
                        sb.AppendFormat("IdVettura {0}\t Matricola:[{1}]{2}", vettura.IdVettura, vettura.Matricola, Environment.NewLine);
                    }
                    string vetture = sb.ToString();
                    textBox2.AppendText(vetture);
                    textBox2.Select(start, vetture.Length);
                    textBox2.SelectionColor = Color.DarkGray;
                    textBox2.SelectionIndent = 10;
                    textBox2.AppendText(Environment.NewLine);
                }

                textBox2.AppendText(Environment.NewLine);
                textBox2.Select(0, 0);

                labelTotaleRighe.Text = vm.ElencoAggregatoVetture.Count.ToString();
                labelTotaleIdVettura.Text = vm.TotaleIdVettura.ToString();
                labelTotaleMatricola.Text = vm.TotaleMatricola.ToString();

                // Statistiche
                var stat = vm.StatisticheAttuali ?? new Statistiche();
                labelBusAtac.Text = stat.RilevatoBusAtac.ToString();
                labelTramAtac.Text = stat.RilevatoTramAtac.ToString();
                labelFilobusAtac.Text = stat.RilevatoFilobusAtac.ToString();
                labelMiniBusEleAtac.Text = stat.RilevatoMinibusElettrici.ToString();
                labelFurgoncinoAtac.Text = stat.RilevatoFurgoncini.ToString();
                labelFerroAtac.Text = stat.RilevatoFerro.ToString();
                labelAltroAtac.Text = stat.RilevatoAltroAtac.ToString();
                labelBusTPL.Text = stat.RilevatoBusTpl.ToString();
                labelPullmanTPL.Text = stat.RilevatoPullmanTpl.ToString();
                labelAltroTpl.Text = stat.RilevatoAltroTpl.ToString();

                labelTotaleMatricolaATAC.Text = vm.TotaleMatricolaAtac.ToString();
                labelTotaleMatricolaTPL.Text = vm.TotaleMatricolaTPL.ToString();

                // DataTable per grid principali - carica usando ObjectReader
                var dt = new DataTable();
                using (var reader = ObjectReader.Create(vm.ElencoAggregatoVetture))
                {
                    dt.Load(reader);
                }
                extendedVehicleInfoBindingSource.DataSource = dt;
                advancedDataGridView1.DataSource = extendedVehicleInfoBindingSource;

                // Attuale - applica ordinamento se necessario evitando ToList doppio
                var elencoAttuale = vm.ElencoVetture;
                DataTable dtAttuale = new DataTable();
                if (string.IsNullOrEmpty(bindingSourceAttuale.Sort))
                {
                    using (var reader = ObjectReader.Create(elencoAttuale.OrderBy(x => x.Linea?.Length).ThenBy(x => x.Linea)))
                    {
                        dtAttuale.Load(reader);
                    }
                }
                else
                {
                    using (var reader = ObjectReader.Create(elencoAttuale))
                    {
                        dtAttuale.Load(reader);
                    }
                }
                bindingSourceAttuale.DataSource = dtAttuale;
                advancedDataGridView2.DataSource = bindingSourceAttuale;

                // Liste e conteggi
                var listaMezziSuLinea = vm.ElencoVetture.Where(x => x.TripId != null).ToList();
                var listaBusAttesa = vm.ElencoVetture.Where(x => x.TripId == null).ToList();
                int numVettureTPLFeedVehicle = vm.ElencoVetture.Count(i => i.TipoMezzoTrasporto == 3 || i.TipoMezzoTrasporto == 4 || i.TipoMezzoTrasporto == -3);

                int busLinea = listaMezziSuLinea.Count;
                int busAttesa = listaBusAttesa.Count;
                int busTotale = busLinea + busAttesa;
                textBox1.AppendText($"Totale Vetture Rilevate sul Feed Vehicle {busTotale}");

                // Raggruppamento per gestore - cache StatisticheAttuali.ServizioRaggruppato
                var sbGestori = new StringBuilder();
                var raggruppatoGestore = vm.StatisticheAttuali?.ServizioRaggruppato ?? Enumerable.Empty<ServizioRaggruppato>();
                var grouped = raggruppatoGestore.GroupBy(x => x.Agenzia).Select(g => new { Gestore = g.Key, Totale = g.Sum(x => x.Num) });

                foreach (var gestore in grouped)
                {
                    sbGestori.AppendLine();
                    sbGestori.AppendFormat("{0} - {1}{2}", gestore.Gestore, gestore.Totale, Environment.NewLine);
                    foreach (var servizio in raggruppatoGestore.Where(x => x.Agenzia == gestore.Gestore))
                    {
                        sbGestori.AppendFormat("    {0}\t{1}{2}", servizio.Servizio, servizio.Num, Environment.NewLine);
                    }
                }
                textBox1.AppendText(sbGestori.ToString());

                labelTPL.Text = numVettureTPLFeedVehicle.ToString();
                labelAtac.Text = (busTotale - numVettureTPLFeedVehicle).ToString();
                labelTot.Text = busTotale.ToString();

                labelPonderatiATAC.Text = Math.Round(vm.PonderateAtac).ToString();
                labelPonderatiTPL.Text = Math.Round(vm.PonderateTPL).ToString();

                if (string.IsNullOrEmpty(fileName))
                {
                    fileName = string.Format("Feed_{0:yyyy-MM-dd (HH_mm_ss)}", lastDataFeedVehicle);
                }

                if (vm.GTFS_RSM.RegoleMonitoraggio != null && vm.GTFS_RSM.RegoleMonitoraggio.Count > 0)
                {
                    dataGridViolazioni.DataSource = vm.ViolazioniLeneeMonitorate();
                    if (tabMainForm.SelectedTab == tabMonitoraggio)
                    {
                        Colora();
                    }
                }

                // Alert tabs - prendo snapshot da GTFS_RSM
                var alerts = vm.GTFS_RSM?.AlertsDaControllare;
                if (alerts != null)
                {
                    foreach (var alert in alerts)
                    {
                        var page = tabMainForm.TabPages[alert.Name];
                        if (page == null) continue;
                        var ctrl = page.Controls[alert.Name] as DataGridView;
                        if (ctrl == null) continue;
                        if (checkBoxStorico.Checked)
                        {
                            ctrl.DataSource = alert.ViolazioniAlert.ToList();
                        }
                        else
                        {
                            ctrl.DataSource = vm.ViolazioniAlertAttuali != null ? vm.ViolazioniAlertAttuali.ToList() : null;
                        }
                    }
                }

                // Grafico
                AggiornaScottPlot();

                // Segnamo come processato questo timestamp (solo se abbiamo raggiunto il rendering)
                try
                {
                    if (vm.LastDataFeed.HasValue)
                        _lastProcessedFeedTimestamp = vm.LastDataFeed.Value;
                }
                catch
                {
                    // ignore
                }
            }
            catch (Exception ex)
            {
                textBox1.AppendText(ex.Message);
                Log.Error(ex, "Errore Generico");
            }
        }

        // Mantengo LeggiValidFeedNEW per compatibilità, ma il polling ora può usare _feedManager.RefreshAsync
        private void LeggiValidFeedNEW(string routeID, bool filtroTripVuoti, bool filtroTuttoPercorso, bool raggruppalineaRegola, bool nonRaggruppare
            , IProgress<Tuple<string, PictureBox, Bitmap>> progress)
        {
            try
            {
                Tuple<string, PictureBox, Bitmap> tuplaReport = null;
                var tupleServer = new List<Tuple<string, PictureBox>>
                {
                    new Tuple<string, PictureBox>(urlVehicle.Text, imgUrl1),
                    new Tuple<string, PictureBox>(urlVehicleRiserva.Text, imgUrl2)
                };
                tupleServer.RemoveAll(x => string.IsNullOrEmpty(x.Item1));

                _feedManager.NumeroLetture++;
                for (int i = 0; i < tupleServer.Count; i++)
                {
                    var tupla = tupleServer[i];
                    string url = tupla.Item1;
                    try
                    {
                        _feedManager.VehicleManager.LeggiFeedValido(url);
                        string errorMsg = string.Empty;
                        switch (_feedManager.VehicleManager.CodeFeed)
                        {
                            case 0:
                                string filtroLinea = routeID == "-1" ? string.Empty : routeID;
                                tuplaReport = new Tuple<string, PictureBox, Bitmap>(string.Empty, tupla.Item2, Properties.Resources.verde);
                                _feedManager.VehicleManager.ElaboraUltimoFeedValido(filtroLinea, filtroTripVuoti, filtroTuttoPercorso, raggruppalineaRegola, nonRaggruppare);
                                _feedManager.NumeroFeedValidi++;
                                break;
                            case -1:
                                errorMsg = string.Format("[{0:HH:mm:ss}] - Feed Scartato perchè NON LETTO{1}", DateTime.Now, Environment.NewLine);
                                tuplaReport = new Tuple<string, PictureBox, Bitmap>(errorMsg, tupla.Item2, Properties.Resources.rosso);
                                break;
                            case -2:
                                errorMsg = string.Format("[{0:HH:mm:ss}] - Feed Scartato perchè VUOTO{1}", DateTime.Now, Environment.NewLine);
                                tuplaReport = new Tuple<string, PictureBox, Bitmap>(errorMsg, tupla.Item2, Properties.Resources.rosso);
                                break;
                            case -10:
                                errorMsg = "Errore Lettura Feed" + Environment.NewLine;
                                tuplaReport = new Tuple<string, PictureBox, Bitmap>(errorMsg, tupla.Item2, Properties.Resources.rosso);
                                break;
                            case -3:
                                errorMsg = string.Format("[{0:HH:mm:ss}] - Feed scartato in quanto ha il timestamp SUPERATO{1}", DateTime.Now, Environment.NewLine);
                                tuplaReport = new Tuple<string, PictureBox, Bitmap>(errorMsg, tupla.Item2, Properties.Resources.arancio);
                                break;
                        }

                        if (progress != null && tuplaReport != null)
                        {
                            progress.Report(tuplaReport);
                        }

                        if (_feedManager.VehicleManager.CodeFeed == 0)
                        {
                            break;
                        }
                    }
                    catch (WebException ex) when ((ex.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.NotFound)
                    {
                        _feedManager.VehicleManager.CodeFeed = -100;
                        string errorMsg = string.Format("{0}: {1}{2}Feed Non trovato al seguente indirizzo{2}{3}{2}", ex.Message, Environment.NewLine, Environment.NewLine, ex.Response.ResponseUri);
                        tuplaReport = new Tuple<string, PictureBox, Bitmap>(errorMsg, tupla.Item2, Properties.Resources.rosso);
                        Log.Error(ex, "Feed {UrlFeed} Non trovato ", ex.Response.ResponseUri);
                    }
                    catch (WebException ex) when (ex.Status == WebExceptionStatus.Timeout)
                    {
                        _feedManager.VehicleManager.CodeFeed = -101;
                        string errorMsg = ex.Message + " Problemi di connessione con il server" + Environment.NewLine;
                        tuplaReport = new Tuple<string, PictureBox, Bitmap>(errorMsg, tupla.Item2, Properties.Resources.rosso);
                        Log.Error(ex, "Errore Connessione Server {UrlRemoto}", url);
                    }
                    catch (WebException ex) when (ex.Status == WebExceptionStatus.NameResolutionFailure)
                    {
                        _feedManager.VehicleManager.CodeFeed = -102;
                        string errorMsg = ex.Message;
                        tuplaReport = new Tuple<string, PictureBox, Bitmap>(errorMsg, tupla.Item2, Properties.Resources.rosso);
                        Log.Error(ex, "Errore Connessione Server {UrlRemoto}", url);
                    }
                    catch (Exception ex)
                    {
                        _feedManager.VehicleManager.CodeFeed = -103;
                        string errorMsg = ex.Message;
                        tuplaReport = new Tuple<string, PictureBox, Bitmap>(errorMsg, tupla.Item2, Properties.Resources.rosso);
                        Log.Error(ex, "Errore : ", ex.Message);
                    }
                    if (progress != null && tuplaReport != null)
                    {
                        progress.Report(tuplaReport);
                    }
                }
                _feedManager.AlertManager.LeggiFeedValido(urlAlert.Text);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Errore : ", ex.Message);
            }
        }

        private void RestartFile()
        {
            ResetUI();
            fileName = string.Empty;
            //_feedManager.Reset();
            if (UpdateBox.NewCSVDownloaded || UpdateBox.NewGTFSDownloaded)
            {
                LeggiFileConfigurazione();
            }
            LeggiRegoleAlertDaFile();

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

        // Rendere async per usare RefreshAsync in acqusizione singola
        private void ButtonPlayPause_Click(object sender, EventArgs e)
        {
            if (!checkCSV.Checked && !checkXlsx.Checked)
            {
                DialogResult dialog = MessageBox.Show(
                    "Avviare il monitoraggio senza export dei dati?",
                    "Avvio Monitoraggio",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                if (dialog == DialogResult.No)
                    return;
            }

            int deltaMilliSec = (int)(1000 * (60 * minuti.Value + secondi.Value));
            string routeID = comboBox1.SelectedValue?.ToString() ?? "-1";
            string filtroLinea = routeID == "-1" ? string.Empty : routeID;
            bool filtroTripVuoti = checkTripVuoti.Checked;
            bool filtroTuttoPercorso = checkTuttoPercorso.Visible && checkTuttoPercorso.Checked;
            bool raggruppalineaRegola = radioLineaRegola.Enabled && radioLineaRegola.Checked;
            bool nonRaggruppare = radioNonRaggruppare.Checked;
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
            else if (!timerAcquisizione.Enabled && deltaMilliSec > 0)
            {
                //TimerAcquisizione_Tick(this, EventArgs.Empty);

                minuti.Enabled = false;
                secondi.Enabled = false;
                timerAcquisizione.Interval = deltaMilliSec;
                timerAcquisizione.Enabled = true;
                //timerAcquisizione.Start();
                _feedManager.StartAutoRefresh(urlVehicle.Text, urlVehicleRiserva.Text, urlAlert.Text, filtroLinea, filtroTripVuoti, filtroTuttoPercorso, raggruppalineaRegola, nonRaggruppare, deltaMilliSec);
                buttonPlayPause.BackgroundImage = Resources.pause;
                comboBox1.Enabled = false;
                buttonResetRegole.Enabled = false;
                Log.Information("Acquisizione attiva");
            }
            else
            {
                minuti.Enabled = true;
                secondi.Enabled = true;
                timerAcquisizione.Enabled = false;
                //timerAcquisizione.Stop();
                _feedManager.StopAutoRefresh();
                buttonPlayPause.BackgroundImage = Resources.play;
                comboBox1.Enabled = true;
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

        private void AggiornaScottPlot()
        {
            var graf = _feedManager.VehicleManager.ElencoVettureGrafico;
            if (graf == null || graf.Count == 0) return;

            // deduplica per timestamp (Ticks) scegliendo il campione con Rilevate>0 se presente
            var deduped = graf
                .GroupBy(x => x.DateTime.Ticks)
                .Select(g =>
                {
                    var nonZero = g.Where(v => v.Rilevate > 0).ToList();
                    return nonZero.Any() ? nonZero.Last() : g.Last();
                })
                .OrderBy(x => x.DateTime)
                .ToList();

            // se nulla è cambiato rispetto all'ultimo render, skip
            if (deduped.Count == _lastGraficoCount && _lastGraficoTimestamp.HasValue && deduped.Last().DateTime == _lastGraficoTimestamp.Value)
                return;

            _lastGraficoCount = deduped.Count;
            _lastGraficoTimestamp = deduped.Last().DateTime;

            // render con la lista pulita
            RenderInternal(plotAtac, plotTPL, deduped);
        }

        private void RenderInternal(FormsPlot pltATAC, FormsPlot pltTPL, List<MonitoraggioVettureGrafico> grafico)
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
                tempo[i] = item.DateTime.ToOADate();
                serieAtac[i] = (double)item.Atac;
                serieAggregateATAC[i] = (double)item.AggregateAtac;
                serieTPL[i] = (double)item.TPL;
                serieAggregateTPL[i] = (double)item.AggregateTPL;
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
            var grafico = _feedManager.VehicleManager.ElencoVettureGrafico ?? new List<MonitoraggioVettureGrafico>();
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
            checkTuttoPercorso.Visible = Properties.Settings.Default.ExtraSetting;
            checkResetSempre.Visible = Properties.Settings.Default.ExtraSetting;
            urlAlert.Visible = true;
            labelAlert.Visible = true;

            int totalSeconds = Properties.Settings.Default.DeltaTSec;
            minuti.Value = totalSeconds / 60;
            secondi.Value = totalSeconds % 60;
            #endregion

            LeggiFileConfigurazione();

            var vm = _feedManager.VehicleManager;
            if (vm.GTFS_RSM != null && vm.GTFS_RSM.RegoleMonitoraggio != null && vm.GTFS_RSM.RegoleMonitoraggio.Count > 0)
            {
                dataGridViolazioni.DataSource = vm.GTFS_RSM.RegoleMonitoraggio;
            }
            else
            {
                if (tabMainForm.TabPages.Contains(tabMonitoraggio))
                {
                    tabMainForm.TabPages.Remove(tabMonitoraggio);
                    Log.Information("Rimosso tab RegoleMonitoraggio");
                }
            }
            LeggiRegoleAlertDaFile();
        }

        private void LeggiFileConfigurazione()
        {
            bool usaDettagliVettura = checkDettagliVettura.Checked;
            var vm = _feedManager.VehicleManager;
            vm.GTFS_RSM = new GTFS_RSM(Path.Combine("Config", "GTFS_Static"), usaDettagliVettura);
            UpdateBox.ExistNewerGTFS = false;
            UpdateBox.ExistNewerCSV = false;
            UpdateBox.ExistNewerVersion = false;
            UpdateBox.NewCSVDownloaded = false;
            UpdateBox.NewGTFSDownloaded = false;

            var routes = vm.GTFS_RSM.StaticData.Routes ?? Enumerable.Empty<Route>();
            var elencoLinee = routes.OrderBy(k => k.ShortName).Distinct().ToList();

            var fittizia = new Route { Id = "-1", ShortName = "    Tutte" };
            elencoLinee.Insert(0, fittizia);

            var alternativa = elencoLinee.Select(x =>
            {
                string linea = string.Empty;
                if (x.ShortName == x.LongName || string.IsNullOrEmpty(x.LongName))
                {
                    linea = x.ShortName;
                }
                else if (string.IsNullOrEmpty(x.ShortName))
                {
                    linea = x.LongName;
                }
                else
                {
                    linea = x.ShortName + " - " + x.LongName;
                }
                return new Route { Id = x.Id, ShortName = linea };
            }).Distinct().ToList();

            comboBox1.DataSource = alternativa;
            comboBox1.ValueMember = "Id";
            comboBox1.DisplayMember = "ShortName";

            var j = vm.GTFS_RSM.ElencoLineaAgenzia
                .Where(x => !string.IsNullOrEmpty(x.Agency.Name))
                .Select(lineaAgenzia => new
                {
                    Gestore = lineaAgenzia.Agency.Name,
                    Linea = (lineaAgenzia.Route.ShortName == lineaAgenzia.Route.LongName || string.IsNullOrEmpty(lineaAgenzia.Route.LongName))
                                ? lineaAgenzia.Route.ShortName
                                : string.IsNullOrEmpty(lineaAgenzia.Route.ShortName)
                                        ? lineaAgenzia.Route.LongName
                                        : lineaAgenzia.Route.ShortName + " - " + lineaAgenzia.Route.LongName,
                    TipoTrasporto = lineaAgenzia.Route.Type
                })
                .ToList();

            var dt = new DataTable();
            using (var reader = ObjectReader.Create(j))
            {
                dt.Load(reader);
            }
            lineaAgenziaBindingSource.DataSource = dt;
            advancedDataGridView3.DataSource = dt;

            int totCorseCensite = vm.GTFS_RSM.ElencoLineaAgenzia.Count();
            var gr = j
                .GroupBy(x => x.Gestore)
                .Select(x => new
                {
                    A = x.Key,
                    B = x.Count(),
                    C = x.Count() / (decimal)totCorseCensite
                });

            var distribuzioneCorseGestoreGTFS = (
                from agency in vm.GTFS_RSM.StaticData.Agencies
                join g in gr on agency.Name equals g.A into grouped
                from gg in grouped.DefaultIfEmpty()
                select new
                {
                    gestore = gg?.A ?? agency.Name,
                    numCorseGTFS = gg?.B ?? 0,
                    percCorseGTFS = gg?.C ?? 0
                }).ToList();

            double[] values = distribuzioneCorseGestoreGTFS.Select(p => (double)p.numCorseGTFS).ToArray();
            string[] labels = distribuzioneCorseGestoreGTFS.Select(p => p.gestore).ToArray();
            string[] legendLabels = distribuzioneCorseGestoreGTFS.Select(p => string.Format("{0}: {1} su {2} ({3:P})", p.gestore, p.numCorseGTFS, totCorseCensite, p.percCorseGTFS)).ToArray();

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

            try
            {
                vm.GTFS_RSM.LeggiRegoleMonitoraggio(Path.Combine("Config", "MonitoraggioLinee", "RegoleMonitoraggio_yes.txt"));
            }
            catch
            {
                Log.Information("Nessuna RegoleMonitoraggio");
            }
        }

        private void LeggiRegoleAlertDaFile()
        {
            string pathAlert = Path.Combine("Config", "Regole_Alert");
            var vm = _feedManager.VehicleManager;
            bool esitoAlert = vm.GTFS_RSM.LeggiAlertDaControllare(pathAlert);
            if (esitoAlert)
            {
                // Creazione tab dinamica: minimizzo allocazioni temporanee
                foreach (var alert in vm.GTFS_RSM.AlertsDaControllare)
                {
                    if (tabMainForm.TabPages.ContainsKey(alert.Name))
                    {
                        tabMainForm.TabPages.RemoveByKey(alert.Name);
                    }

                    var myNewTabItem = new TabPage { Text = alert.Name, Name = alert.Name };
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
                        Name = alert.Name,
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

                    myNewdataGridVetture.DataSource = alert.RegoleAlert;
                }

                if (vm.GTFS_RSM.AlertsDaControllare.Count > 0)
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

            int esito = vm.GTFS_RSM.LeggiCriteriMediaPonderata(Path.Combine("Config", "CriterioMediaPonderata.txt"));
            if (esito == -1)
            {
                MessageBox.Show(text: "La somma dei pesi dei campioni deve essere 1", caption: "Attenzione", buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
            }
        }

        private async void TimerAcquisizione_Tick(object sender, EventArgs e)
        {
            /*
            string routeID = comboBox1.SelectedValue?.ToString() ?? "-1";
            FeedVehicleManager vmPostRefresh = null;

            try
            {
                // prova primo feed vehicle
                try
                {
                    await _feedManager.RefreshAsync(urlVehicle.Text, urlAlert.Text, routeID, CancellationToken.None);
                }
                catch
                {
                    // fallback: prova urlVehicleRiserva se presente
                    if (!string.IsNullOrEmpty(urlVehicleRiserva.Text))
                    {
                        try
                        {
                            await _feedManager.RefreshAsync(urlVehicleRiserva.Text, urlAlert.Text, routeID, CancellationToken.None);
                        }
                        catch (Exception exFallback)
                        {
                            Log.Error(exFallback, "Entrambi i feed vehicle falliti");
                            // come prima: continua (AcquisizioneNEW verrà chiamato dagli eventi se c'è successo)
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Errore in RefreshAsync");
            }

            // aggiorna i contatori letture / feed validi UI (una sola lettura per tick)
            try
            {
                NumeroLetture++;
                vmPostRefresh = _feedManager.VehicleManager;
                if (vmPostRefresh != null && vmPostRefresh.CodeFeed == 0)
                {
                    NumeroFeedValidi++;
                }
                labelLetture.Text = NumeroLetture.ToString();
                labelFeedLetti.Text = NumeroFeedValidi.ToString();

                // Se il feed è stato scaricato (entità > 0) ma le liste non sono ancora elaborate,
                // forziamo l'elaborazione dell'ultimo feed valido prima di proseguire con la UI
                int lastValidEntities = vmPostRefresh?.LastValidFeed?.Entities?.Count ?? 0;
                int elencoCount = vmPostRefresh?.ElencoVetture?.Count ?? 0;
                Log.Information("Post-Refresh: LastValidFeed.Entities={LastValidEntities}, ElencoVetture={ElencoCount}", lastValidEntities, elencoCount);

                if (lastValidEntities > 0 && elencoCount == 0 && vmPostRefresh != null)
                {
                    bool filtroTripVuoti = checkTripVuoti.Checked;
                    bool filtroTuttoPercorso = checkTuttoPercorso.Visible && checkTuttoPercorso.Checked;
                    bool raggruppalineaRegola = radioLineaRegola.Enabled && radioLineaRegola.Checked;
                    bool nonRaggruppare = radioNonRaggruppare.Checked;
                    string filtroLinea = routeID == "-1" ? string.Empty : routeID;
                    try
                    {
                        Log.Information("Forzo ElaboraUltimoFeedValido per costruire ElencoVetture (entities {Count})", lastValidEntities);
                        vmPostRefresh.ElaboraUltimoFeedValido(filtroLinea, filtroTripVuoti, filtroTuttoPercorso, raggruppalineaRegola, nonRaggruppare);
                    }
                    catch (Exception exElab)
                    {
                        Log.Error(exElab, "Errore forzando ElaboraUltimoFeedValido (timer)");
                    }
                }
            }
            catch (Exception exCnt)
            {
                Log.Error(exCnt, "Errore aggiornamento contatori post-refresh");
            }


            // diagnostica se il feed risulta vuoto
            var vmDiag = _feedManager.VehicleManager;
            int? entitiesCount = vmDiag?.LastValidFeed?.Entities?.Count;
            Log.Information("After RefreshAsync: LastValidFeed entities count = {Count}", entitiesCount ?? -1);
            if ((entitiesCount ?? 0) == 0)
            {
                // salva raw dal primo URL e dalla riserva per confronto
                _ = DebugSaveRawFeedAsync(urlVehicle.Text, "vehicle_debug");
                if (!string.IsNullOrEmpty(urlVehicleRiserva.Text))
                    _ = DebugSaveRawFeedAsync(urlVehicleRiserva.Text, "vehicle_reserva_debug");
            }
            await Task.Run(() => ExportGrid());

            // invochiamo AcquisizioneNEW solo se l'elenco è stato costruito
            if (vmPostRefresh?.ElencoVetture != null && vmPostRefresh.ElencoVetture.Count > 0)
            {
                if (this.IsHandleCreated)
                    BeginInvoke(new Action(AcquisizioneNEW));
            }
            else
            {
                Log.Information("Acquisizione singola: elenco vetture non ancora costruito, attendo evento");
            }
            */
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
            if (timerAcquisizione.Enabled)
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


        private async Task SaveAs(FileInfo outputFile, FileInfo altFileName)
        {
            if (checkXlsx.Checked)
            {
                using (ExcelPackage excel = new ExcelPackage(outputFile))
                {
                    try
                    {
                        string excelSheetName;
                        var vm = _feedManager.VehicleManager;
                        var am = _feedManager.AlertManager;
                        if (vm.LastValidationResultCode == 0)
                        {
                            excelSheetName = "Feed";
                            ElaboraSheet(excel, excelSheetName, vm.ElencoAggregatoVetture);

                            if (checkGrafico.Checked)
                            {
                                var existing = excel.Workbook.Worksheets[excelSheetName];
                                // remove "Grafico" if exists
                                var grafSheet = excel.Workbook.Worksheets["Grafico"];
                                if (grafSheet != null) excel.Workbook.Worksheets.Delete("Grafico");

                                ExcelWorksheet workSheet = excel.Workbook.Worksheets.Add("Grafico");
                                PropertyInfo[] membersToInclude = typeof(MonitoraggioVettureGrafico)
                                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                    .Where(p => !Attribute.IsDefined(p, typeof(IgnoreAttribute)))
                                    .ToArray();

                                int colNumber = 2;
                                ExcelRangeBase range = workSheet.Cells[32, colNumber].LoadFromCollection(
                                    vm.ElencoVettureGrafico
                                    , true
                                    , TableStyles.Medium2
                                    , BindingFlags.Public | BindingFlags.Instance
                                    , membersToInclude);

                                foreach (PropertyInfo exportedProperty in membersToInclude)
                                {
                                    if (exportedProperty.PropertyType == typeof(DateTime) || exportedProperty.PropertyType == typeof(DateTime?))
                                    {
                                        workSheet.Column(colNumber).Style.Numberformat.Format = "HH:mm:ss";
                                    }
                                    colNumber++;
                                }

                                workSheet.Cells.AutoFitColumns();

                                ExcelLineChart lineChartATAC = workSheet.Drawings.AddChart("lineChartATAC", eChartType.Line) as ExcelLineChart;
                                ExcelLineChart lineChartTPL = workSheet.Drawings.AddChart("lineChartTPL", eChartType.Line) as ExcelLineChart;
                                lineChartATAC.Title.Text = string.Format("Vetture Rilevate ATAC {0:dd-MM-yyyy [HH:mm:ss}-{1:HH:mm:ss}] ", vm.FirstDataFeed, vm.LastDataFeed);
                                lineChartTPL.Title.Text = string.Format("Vetture Rilevate  TPL {0:dd-MM-yyyy [HH:mm:ss}-{1:HH:mm:ss}] ", vm.FirstDataFeed, vm.LastDataFeed);

                                ExcelRangeBase rangeLabel = range.Offset(1, 0, vm.ElencoVettureGrafico.Count, 1);
                                ExcelRangeBase range1 = range.Offset(1, 2, vm.ElencoVettureGrafico.Count, 1);
                                ExcelRangeBase range2 = range.Offset(1, 4, vm.ElencoVettureGrafico.Count, 1);
                                ExcelRangeBase range3 = range.Offset(1, 3, vm.ElencoVettureGrafico.Count, 1);
                                ExcelRangeBase range4 = range.Offset(1, 5, vm.ElencoVettureGrafico.Count, 1);

                                lineChartATAC.Series.Add(range1, rangeLabel);
                                lineChartATAC.Series.Add(range2, rangeLabel);
                                lineChartTPL.Series.Add(range3, rangeLabel);
                                lineChartTPL.Series.Add(range4, rangeLabel);

                                lineChartATAC.Series[0].Header = "Aggregate";
                                lineChartATAC.Series[1].Header = "Istantanee";
                                lineChartTPL.Series[0].Header = "Aggregate";
                                lineChartTPL.Series[1].Header = "Istantanee";

                                lineChartATAC.Legend.Position = eLegendPosition.Right;
                                lineChartATAC.SetSize(900, 250);
                                lineChartATAC.SetPosition(0, 3, 0, 3);
                                lineChartTPL.Legend.Position = eLegendPosition.Right;
                                lineChartTPL.SetSize(900, 250);
                                lineChartTPL.SetPosition(14, 3, 0, 3);
                            }

                            if (tabMainForm.TabPages.Contains(tabMonitoraggio) && checkMonitoraggio.Checked)
                            {
                                excelSheetName = "Monitoraggio Linee";
                                List<LineaMonitorata> violazioniLineaMonitorata = vm.ElencoLineeMonitorate.Where(x => x.OraPrimaViolazione.HasValue).ToList();
                                ElaboraSheet(excel, excelSheetName, violazioniLineaMonitorata);
                            }

                            if (checkAlert.Checked && checkAlert.Enabled)
                            {
                                foreach (AlertDaControllare alertDaControllare in vm.GTFS_RSM.AlertsDaControllare)
                                {
                                    excelSheetName = alertDaControllare.Name;
                                    ElaboraSheet(excel, excelSheetName, alertDaControllare.ViolazioniAlert, dateFormat: "HH:mm:ss");
                                }
                            }

                            if (checkAnomalieGTFS.Checked)
                            {
                                excelSheetName = "AnomalieGTFS";
                                List<string> ammessi = new List<string> { "Matricola", "Linea", "PrimaVolta", "TripId", "CurrentStopSequence", "Delta" };
                                ElaboraSheet(excel, excelSheetName, vm.AnomaliaGTFS, ammessi);
                            }

                            if (checkSovraffollamento.Checked)
                            {
                                excelSheetName = "Sovraffollamneto";
                                ElaboraSheet(excel, excelSheetName, vm.ElencoVettureSovraffollate);
                            }
                        }

                        excelSheetName = "Avvisi";
                        if (am.LastValidationResultCode == 0 && am.Avvisi is List<Avviso> avvisi)
                        {
                            ElaboraSheet(excel, excelSheetName, avvisi);
                        }
                        else if (am.FirstDataFeed.HasValue)
                        {
                            excel.Workbook.Worksheets.MoveToEnd(excelSheetName);
                        }
                        try
                        {
                            excel.Save();
                            altFileName.Delete();
                        }
                        catch (Exception ex)
                        {
                            Log.Error("{Exception}", ex);
                            excel.SaveAs(altFileName);
                        }
                    }
                    catch (Exception exc)
                    {
                        Log.Error("{Exception}", exc);
                    }
                }
            }

            if (checkCSV.Checked)
            {
                using (var writer = new StreamWriter(Path.Combine("OUTPUT", fileName + ".csv")))
                {
                    var config = new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";" };
                    using (var csv = new CsvWriter(writer, config))
                    {
                        await csv.WriteRecordsAsync(_feedManager.VehicleManager.ElencoAggregatoVetture);
                    }
                }
            }
        }

        private void ElaboraSheet<T>(ExcelPackage excel, string excelSheetName, List<T> record, List<string> ammessi = null, string dateFormat = "")
        {
            var existing = excel.Workbook.Worksheets[excelSheetName];
            if (existing != null) excel.Workbook.Worksheets.Delete(excelSheetName);

            ExcelWorksheet workSheet = excel.Workbook.Worksheets.Add(excelSheetName);

            PropertyInfo[] membersToInclude = typeof(T)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => !Attribute.IsDefined(p, typeof(IgnoreAttribute))
                            && (ammessi == null || ammessi.Contains(p.Name)))
                .ToArray();

            ExcelRangeBase range = workSheet.Cells[1, 1].LoadFromCollection(
                record
                , true
                , TableStyles.Medium2
                , BindingFlags.Public | BindingFlags.Instance
                , membersToInclude);

            int colNumber = 1;

            foreach (PropertyInfo exportedProperty in membersToInclude)
            {
                if (exportedProperty.PropertyType == typeof(DateTime) || exportedProperty.PropertyType == typeof(DateTime?))
                {
                    workSheet.Column(colNumber).Style.Numberformat.Format = string.IsNullOrEmpty(dateFormat) ? "MM/dd/yyyy HH:mm:ss" : dateFormat;
                }
                else if (exportedProperty.PropertyType == typeof(TimeSpan) || exportedProperty.PropertyType == typeof(TimeSpan?))
                {
                    workSheet.Column(colNumber).Style.Numberformat.Format = "HH:mm:ss";
                }

                colNumber++;
            }
            workSheet.Cells.AutoFitColumns();

            excel.Workbook.Worksheets.MoveToEnd(excelSheetName);
        }

        private async void CheckUpdate(bool download = false, bool forceDownload = false)
        {
            bool? newVersion = await Task.Run(() => UpdateBox.TaskCheckUpdate(download, forceDownload));

            if (UpdateBox.NewGTFSDownloaded || UpdateBox.NewCSVDownloaded)
            {
                buttonVerificaAggiornamenti.Image = Properties.Resources.giallo;
            }
            else if (!newVersion.HasValue)
            {
                buttonVerificaAggiornamenti.Image = Properties.Resources.arancio;
            }
            else if (newVersion.Value)
            {
                buttonVerificaAggiornamenti.Image = Properties.Resources.rosso;
            }
        }

        private void Random(object sender, EventArgs e)
        {
            DateTime t0;
            var graf = _feedManager.VehicleManager.ElencoVettureGrafico;
            if (graf.Count == 0)
            {
                t0 = DateTime.Now;
            }
            else
            {
                t0 = graf[graf.Count - 1].DateTime;
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
                    var nuovoMonitoraggio = new MonitoraggioVettureGrafico
                    {
                        DateTime = t0.AddSeconds(i * 30),
                        Aggregate = aggregate,
                        Rilevate = rilevate,
                        Atac = atac,
                        TPL = tpl,
                        Aggiunte = aggiunte,
                        Tolte = tolte
                    };
                    graf.Add(nuovoMonitoraggio);
                }
            }
            AggiornaScottPlot();
        }

        private void CheckXlsx_CheckedChanged(object sender, EventArgs e)
        {
            checkGrafico.Enabled = checkXlsx.Checked;
            checkMonitoraggio.Enabled = checkXlsx.Checked;
            checkAlert.Enabled = checkXlsx.Checked;
        }

        public void Colora()
        {
            dataGridViolazioni.SuspendLayout();
            var dataSource = (IEnumerable<LineaMonitorata>)dataGridViolazioni.DataSource;
            int rowCount = dataGridViolazioni.RowCount;
            for (int i = 0; i < rowCount; i++)
            {
                DataGridViewRow row = dataGridViolazioni.Rows[i];
                var riga = dataSource.ElementAt(i);
                if (riga.VettureRilevate < riga.VetturePreviste)
                {
                    TimeSpan span = riga.OraUltimaViolazione.GetValueOrDefault(_feedManager.VehicleManager.LastDataFeed.GetValueOrDefault()) - riga.OraPrimaViolazione.GetValueOrDefault(DateTime.MinValue);
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
            if ((sender as TabControl).SelectedTab == tabMonitoraggio && _feedManager.VehicleManager.LastDataFeed.HasValue)
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
            LeggiRegoleAlertDaFile();
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
            UpdateBox.UrlMD5 = urlMD5_GTFS_Statico.Text;
        }

        private void UrlGTFS_Statico_TextChanged(object sender, EventArgs e)
        {
            UpdateBox.UrlGTFS = urlGTFS_Statico.Text;
        }

        private void CheckDettagliVettura_CheckedChanged(object sender, EventArgs e)
        {
            _feedManager.VehicleManager.GTFS_RSM?.LeggiDettagliVettura(checkDettagliVettura.Checked);
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