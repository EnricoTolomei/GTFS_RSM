using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using FastMember;
using GTFS;
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
using System.Threading.Tasks;
using System.Windows.Forms;
using static AtacFeed.TransitRealtime;
using Log = Serilog.Log;

namespace AtacFeed
{
    public partial class FormGTFS_RSM : Form
    {
        private string fileName;
        private DateTime? DataResetMonitoraggio;
        private DateTime? DataCheckUpdate;

        private readonly FeedVehicleManager FeedVehicleManager = new();
        private readonly FeedAlertManager FeedAlertManager = new();

        private readonly UpdateBox UpdateBox = new();

        public FormGTFS_RSM()
        {
            InitializeComponent();
        }

        private int NumeroLetture;
        private int NumeroFeedValidi;

        private void Acquisizione()
        {
            try
            {
                Exception ecc = null;
                imgUrl1.Image = null;
                imgUrl2.Image = null;
                textBox1.Clear();
                textBox2.Clear();
                string routeID = comboBox1.SelectedValue?.ToString() ?? "-1";
                if ((checkResetSempre.Visible && checkResetSempre.Checked) || (DataResetMonitoraggio.HasValue && DateTime.Now > DataResetMonitoraggio.GetValueOrDefault()))
                {
                    RestartFile();
                    DataResetMonitoraggio = DataResetMonitoraggio.GetValueOrDefault(DateTime.MinValue).AddDays(1);
                    Log.Information("Prossimo reset monitoraggio: {DataResetMonitoraggio:dd/MM/yyyy HH:mm:ss}", DataResetMonitoraggio);
                }

                #region Verifica Update GTFS STATICO                
                if (!DataCheckUpdate.HasValue)
                {
                    DataCheckUpdate = DateTime.Now.AddSeconds(20);
                    CheckUpdate(download: checkMD5.Checked);
                }
                else if (DateTime.Now > DataCheckUpdate.GetValueOrDefault())
                {
                    CheckUpdate(download: checkMD5.Checked);
                    DataCheckUpdate = DateTime.Now.AddHours(8);
                }
                #endregion

                string alertUrl = urlAlert.Text;
                if (!string.IsNullOrWhiteSpace(alertUrl))
                {
                    try
                    {
                        if (FeedAlertManager.LeggiFeedValido(urlAlert.Text) == 0)
                        {
                            DataTable dtAvvisi = new();
                            using (var reader = ObjectReader.Create(FeedAlertManager.Avvisi))
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

                bool feedAvailable = GetValidFeed() == 0;
                labelLetture.Text = NumeroLetture.ToString();

                if (feedAvailable)
                {
                    DateTime lastDataFeedVehicle = FeedVehicleManager.LastDataFeed.Value;

                    lblOraLettura.Text = $"{lastDataFeedVehicle:HH:mm:ss}";
                    labelFeedLetti.Text = NumeroFeedValidi.ToString();

                    string filtroLinea = routeID == "-1" ? string.Empty : routeID;
                    bool filtroTripVuoti = checkTripVuoti.Checked;
                    bool filtroTuttoPercorso = checkTuttoPercorso.Visible && checkTuttoPercorso.Checked;
                    bool raggruppalineaRegola = radioLineaRegola.Enabled && radioLineaRegola.Checked;
                    bool nonoRaggruppare = radioNonRaggruppare.Checked;
                    ecc = FeedVehicleManager.ElaboraUltimoFeedValido(filtroLinea, filtroTripVuoti, filtroTuttoPercorso, raggruppalineaRegola, nonoRaggruppare);
                    List<string> lineeAnomale = FeedVehicleManager.LineeAnomale();
                    if (lineeAnomale.Count > 0)
                    {
                        textBox2.Text = $"Le seguenti linee {string.Join(", ", lineeAnomale)}{Environment.NewLine} NON sono riportate nel file statico routes.txt{Environment.NewLine}{Environment.NewLine}";
                    }

                    int lineNumberToSelect = 0;
                    int start = 0;
                    int length = 0;
                    if (FeedVehicleManager.ElencoPrecedente.Count > 0)
                    {
                        foreach (ExtendedVehicleInfo vettura in FeedVehicleManager.VettureAggiunte)
                        {
                            textBox3.AppendText($"{vettura.IdVettura} - {vettura.Matricola} rilevata alle {lastDataFeedVehicle:HH:mm:ss} {Environment.NewLine}");
                        }

                        foreach (ExtendedVehicleInfo vettura in FeedVehicleManager.VettureTolte)
                        {
                            textBox4.AppendText($"{vettura.IdVettura} - {vettura.Matricola} NON rilevata alle {lastDataFeedVehicle:HH:mm:ss} {Environment.NewLine}");
                        }

                        if (FeedVehicleManager.PartenzaAvanzata?.Count > 0)
                        {
                            textBox2.AppendText($"Vetture con 'partenza avanzata'{Environment.NewLine}");
                            foreach (ExtendedVehicleInfo errore in FeedVehicleManager.PartenzaAvanzata)
                            {
                                textBox2.AppendText($"Matricola {errore.Matricola} Linea {errore.Linea} Fermata {errore.CurrentStopSequence}{Environment.NewLine}");
                                lineNumberToSelect = textBox2.Lines.Length - 2;
                                start = textBox2.GetFirstCharIndexFromLine(lineNumberToSelect);
                                length = textBox2.Lines[lineNumberToSelect].Length;
                                textBox2.Select(start, length);
                                textBox2.SelectionColor = Color.CornflowerBlue;
                                textBox2.SelectionIndent = 10;
                                FeedVehicleManager.AnomaliaGTFS.Add(new ErroriGTFS(errore, (int)errore.CurrentStopSequence));
                            }
                            textBox2.AppendText($"{Environment.NewLine}");
                        }

                        if (FeedVehicleManager.VettureRiagganciate?.Count > 0)
                        {
                            textBox2.AppendText($"Vetture 'riagganciate'{Environment.NewLine}");
                            foreach (ExtendedVehicleInfo errore in FeedVehicleManager.VettureRiagganciate)
                            {
                                textBox2.AppendText($"Matricola {errore.Matricola} Linea {errore.Linea} Fermata {errore.CurrentStopSequence}{Environment.NewLine}");
                                lineNumberToSelect = textBox2.Lines.Length - 2;
                                start = textBox2.GetFirstCharIndexFromLine(lineNumberToSelect);
                                length = textBox2.Lines[lineNumberToSelect].Length;
                                textBox2.Select(start, length);
                                textBox2.SelectionColor = Color.CornflowerBlue;
                                textBox2.SelectionIndent = 10;
                                uint ultimaFermataRilevata = FeedVehicleManager.ElencoAggregatoVetture
                                        .Where(x => x.TripId == errore.TripId && x.Matricola == errore.Matricola)
                                        .Max(x => x.CurrentStopSequence);
                                int delta = (int)(errore.CurrentStopSequence - ultimaFermataRilevata);
                                FeedVehicleManager.AnomaliaGTFS.Add(new ErroriGTFS(errore, delta));
                            }
                            textBox2.AppendText($"{Environment.NewLine}");
                        }

                        List<ErroriGTFS> percorsoAnomalo = FeedVehicleManager.PercorsoAnomalo;
                        if (percorsoAnomalo?.Count > 0)
                        {
                            textBox2.AppendText($"Vetture con progressivo fermate 'bucato'{Environment.NewLine}");
                            foreach (ErroriGTFS errore in percorsoAnomalo)
                            {
                                textBox2.AppendText(text: $"Matricola {errore.Matricola} Linea {errore.Linea} Fermata {errore.CurrentStopSequence} => 'balzo' di {errore.Delta}{Environment.NewLine}");
                                int delta = errore.Delta;
                                lineNumberToSelect = textBox2.Lines.Length - 2;
                                start = textBox2.GetFirstCharIndexFromLine(lineNumberToSelect);
                                length = textBox2.Lines[lineNumberToSelect].Length;
                                textBox2.Select(start, length);
                                if (delta < 0)
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
                            textBox2.AppendText($"{Environment.NewLine}");
                        }
                    }

                    List<string> tripDuplicatiFeedVehicle = FeedVehicleManager.TripDuplicati();
                    if (tripDuplicatiFeedVehicle.Count > 0)
                    {
                        textBox2.AppendText($"Trip Duplicati{Environment.NewLine}");
                        foreach (string tripDuplicato in tripDuplicatiFeedVehicle)
                        {
                            var elencoVettureSuTripIdDuplicato = string.Join(", ", FeedVehicleManager.FeedEntities.Where(x => x.Vehicle.Trip.TripId == tripDuplicato).Select(x => x.Vehicle.Vehicle.Label));
                            textBox2.AppendText($"Trip {tripDuplicato}\tVetture:[{elencoVettureSuTripIdDuplicato}]{Environment.NewLine}");
                            lineNumberToSelect = textBox2.Lines.Length - 2;
                            start = textBox2.GetFirstCharIndexFromLine(lineNumberToSelect);
                            length = textBox2.Lines[lineNumberToSelect].Length;
                            textBox2.Select(start, length);
                            textBox2.SelectionColor = Color.Tomato;
                            textBox2.SelectionIndent = 10;
                        }
                        textBox2.AppendText($"{Environment.NewLine}");
                    }

                    /// Controllo accuratezza GTFS (Matricole o IDVehicle vuoto)
                    List<Tuple<string, string, int>> matricoleDuplicate = FeedVehicleManager.MatricoleDuplicate();
                    if (matricoleDuplicate.Count > 0)
                    {
                        textBox2.AppendText($"Matricola Duplicate{Environment.NewLine}");
                        List<string> gestori = [.. matricoleDuplicate.Select(x => x.Item2).Distinct()];
                        foreach (var gestore in gestori)
                        {
                            textBox2.AppendText($" Gestore: {gestore}" + Environment.NewLine);
                            foreach (var servizio in matricoleDuplicate.Where(x => x.Item2 == gestore))
                            {
                                textBox2.AppendText($"  Matricola\t{servizio.Item1}\tRilevata {servizio.Item3} volte" + Environment.NewLine);
                            }
                            textBox2.AppendText(Environment.NewLine);
                        }
                    }
                    List<ExtendedVehicleInfo> vettureSenzaMatricola = FeedVehicleManager.VettureSenzaMatricola();
                    if (vettureSenzaMatricola.Count > 0)
                    {
                        textBox2.AppendText($"Vetture Senza Matricola{Environment.NewLine}");
                        lineNumberToSelect = textBox2.Lines.Length - 1;
                        start = textBox2.GetFirstCharIndexFromLine(lineNumberToSelect);
                        string vetture = string.Empty;
                        foreach (ExtendedVehicleInfo vettura in vettureSenzaMatricola)
                        {
                            vetture += ($"IdVettura {vettura.IdVettura}\t Matricola:[{vettura.Matricola}]{Environment.NewLine}");
                        }
                        textBox2.AppendText(vetture);
                        textBox2.Select(start, vetture.Length);
                        textBox2.SelectionColor = Color.DarkGray;
                        textBox2.SelectionIndent = 10;
                        textBox2.AppendText($"{Environment.NewLine}");
                    }

                    /// Matricola Duplicata
                    //List<>

                    textBox2.AppendText($"{Environment.NewLine}");
                    textBox2.Select(0, 0);

                    labelTotaleRighe.Text = FeedVehicleManager.ElencoAggregatoVetture.Count.ToString();

                    labelTotaleIdVettura.Text = FeedVehicleManager.TotaleIdVettura.ToString();
                    labelTotaleMatricola.Text = FeedVehicleManager.TotaleMatricola.ToString();
                    labelBusAtac.Text = FeedVehicleManager.StatisticheAttuali.RilevatoBusAtac.ToString();
                    labelTramAtac.Text = FeedVehicleManager.StatisticheAttuali.RilevatoTramAtac.ToString();
                    labelFilobusAtac.Text = FeedVehicleManager.StatisticheAttuali.RilevatoFilobusAtac.ToString();
                    labelMiniBusEleAtac.Text = FeedVehicleManager.StatisticheAttuali.RilevatoMinibusElettrici.ToString();
                    labelFurgoncinoAtac.Text = FeedVehicleManager.StatisticheAttuali.RilevatoFurgoncini.ToString();
                    labelFerroAtac.Text = FeedVehicleManager.StatisticheAttuali.RilevatoFerro.ToString();
                    labelAltroAtac.Text = FeedVehicleManager.StatisticheAttuali.RilevatoAltroAtac.ToString();
                    labelBusTPL.Text = FeedVehicleManager.StatisticheAttuali.RilevatoBusTpl.ToString();
                    labelPullmanTPL.Text = FeedVehicleManager.StatisticheAttuali.RilevatoPullmanTpl.ToString();
                    labelAltroTpl.Text = FeedVehicleManager.StatisticheAttuali.RilevatoAltroTpl.ToString();

                    labelTotaleMatricolaATAC.Text = FeedVehicleManager.TotaleMatricolaAtac.ToString();
                    labelTotaleMatricolaTPL.Text = FeedVehicleManager.TotaleMatricolaTPL.ToString();

                    DataTable dt = new();
                    using (var reader = ObjectReader.Create(FeedVehicleManager.ElencoAggregatoVetture))
                    {
                        dt.Load(reader);
                    }

                    extendedVehicleInfoBindingSource.DataSource = dt;
                    advancedDataGridView1.DataSource = extendedVehicleInfoBindingSource;

                    DataTable dtAttuale = new();
                    if ((bindingSourceAttuale.Sort?.Length ?? 0) == 0)
                    {
                        using var reader = ObjectReader.Create(FeedVehicleManager.ElencoVetture.OrderBy(x => x.Linea?.Length).ThenBy(x => x.Linea));
                        dtAttuale.Load(reader);
                    }
                    else
                    {
                        using var reader = ObjectReader.Create(FeedVehicleManager.ElencoVetture);
                        dtAttuale.Load(reader);
                    }

                    bindingSourceAttuale.DataSource = dtAttuale;
                    advancedDataGridView2.DataSource = bindingSourceAttuale;

                    List<string> urlTripList = [];
                    List<string> urlVehicleList = [];
                    //foreach (FeedEntity entity in FeedVehicleManager.FeedEntities)
                    //{
                    //    if (entity.Vehicle != null && entity.Vehicle.Trip != null && !(int.TryParse(routeID, out int res) && res != -1) && entity.Vehicle.Trip.RouteId == routeID)
                    //    {
                    //        textBox1.AppendText(entity.Vehicle.Vehicle.Id + Environment.NewLine);
                    //    }
                    //}

                    List<ExtendedVehicleInfo> listaMezziSuLinea = FeedVehicleManager.ElencoVetture
                        .Where(x => x.TripId != null)
                        .ToList();
                    List<ExtendedVehicleInfo> listaBusAttesa = FeedVehicleManager.ElencoVetture.Where(x => x.TripId == null).ToList();

                    int numVettureTPLFeedVehicle = FeedVehicleManager.ElencoVetture
                        .Where(i => i.TipoMezzoTrasporto == 3 || i.TipoMezzoTrasporto == 4 || i.TipoMezzoTrasporto == -3)
                        .Count();

                    //var rrr = elencoVetture.Where(i => i.Gestore.Contains("tpl") && (i.TipoMezzoTrasporto == 3 || i.TipoMezzoTrasporto == 4 || i.TipoMezzoTrasporto == -3)).ToList();

                    int busLinea = listaMezziSuLinea.Count;
                    int busAttesa = listaBusAttesa.Count;
                    int busTotale = busLinea + busAttesa;
                    textBox1.AppendText($"Totale Vetture Rilevate sul Feed Vehicle {busTotale}");

                    var raggruppatoGestore = FeedVehicleManager.StatisticheAttuali.ServizioRaggruppato
                        .GroupBy(x => x.Agenzia)
                        .Select(g => new
                        {
                            Gestore = g.Key,
                            Totale = g.Sum(x => x.Num)
                        }
                    );

                    foreach (var gestore in raggruppatoGestore)
                    {
                        textBox1.AppendText(Environment.NewLine + $"{gestore.Gestore} - {gestore.Totale}" + Environment.NewLine);
                        foreach (var servizio in FeedVehicleManager.StatisticheAttuali.ServizioRaggruppato.Where(x => x.Agenzia == gestore.Gestore))
                        {
                            textBox1.AppendText($"    {servizio.Servizio}\t{servizio.Num}" + Environment.NewLine);
                        }
                    }

                    labelTPL.Text = $"{numVettureTPLFeedVehicle}";
                    labelAtac.Text = $"{busTotale - numVettureTPLFeedVehicle}";
                    labelTot.Text = $"{busTotale}";

                    labelPonderatiATAC.Text = Math.Round(FeedVehicleManager.PonderateAtac).ToString();
                    labelPonderatiTPL.Text = Math.Round(FeedVehicleManager.PonderateTPL).ToString();

                    if (string.IsNullOrEmpty(fileName))
                    {
                        fileName = $"Feed_{lastDataFeedVehicle:yyyy-MM-dd (HH_mm_ss)}";
                    }

                    if (FeedVehicleManager.GTFS_RSM.RegoleMonitoraggio?.Count > 0)
                    {
                        dataGridViolazioni.DataSource = FeedVehicleManager.ViolazioniLeneeMonitorate();
                        if (tabMainForm.SelectedTab == tabMonitoraggio)
                        {
                            Colora();
                        }
                    }

                    foreach (var alert in FeedVehicleManager.GTFS_RSM.AlertsDaControllare)
                    {
                        if (checkBoxStorico.Checked)
                        {
                            (tabMainForm.TabPages[alert.Name].Controls[alert.Name] as DataGridView).DataSource = alert.ViolazioniAlert.ToList();
                        }
                        else
                        {
                            (tabMainForm.TabPages[alert.Name].Controls[alert.Name] as DataGridView).DataSource = FeedVehicleManager.ViolazioniAlertAttuali.ToList();
                        }
                    }

                    //_ = ExportGrid();
                    Task.Run(() => ExportGrid());

                    AggiornaScottPlot();

                    if (!string.IsNullOrEmpty(urlTrip.Text) && checkFeedTrip.Checked)
                    {
                        BaseFeedManager bfm = new();
                        if (bfm.LeggiFeedValido(urlTrip.Text) == 0)
                        {
                            FeedMessage feedTrip = bfm.LastValidFeed;
                            foreach (FeedEntity entity in feedTrip.Entities)
                            {
                                if (entity.TripUpdate.Vehicle != null && entity.TripUpdate.Trip != null && (entity.TripUpdate.Trip.RouteId == routeID))
                                {
                                    urlTripList.Add(entity.TripUpdate.Vehicle.Id);
                                }
                            }
                            textBox1.AppendText(string.Join(Environment.NewLine, urlTripList));
                            textBox1.AppendText(Environment.NewLine);

                            int numVettureFeedTrip = feedTrip.Entities
                                .Where(x => x.TripUpdate.Vehicle != null && !string.IsNullOrEmpty(x.TripUpdate.Vehicle.Id))
                                .Count();
                            int numVettureTPLFeedTrip = feedTrip.Entities
                                .Where(x => x.TripUpdate.Vehicle != null && !string.IsNullOrEmpty(x.TripUpdate.Vehicle.Id) && x.TripUpdate.Vehicle.Id.Length > 4)
                                .Count();

                            textBox1.AppendText($"Totale Vetture Rilevate sul Feed Trip: {numVettureFeedTrip}{Environment.NewLine}");

                            textBox1.AppendText($"\tATAC {numVettureFeedTrip - numVettureTPLFeedTrip}\tTPL {numVettureTPLFeedVehicle}{Environment.NewLine}");

                            List<FeedEntity> soloVehicle = FeedVehicleManager.FeedEntities
                                .Where(vehicle => !feedTrip.Entities.Any(trip => vehicle.Vehicle.Vehicle.Label == trip.TripUpdate.Vehicle?.Label))
                                .ToList();


                            List<FeedEntity> soloTrip = feedTrip.Entities
                                .Where(trip => !FeedVehicleManager.FeedEntities.Any(vehicle => vehicle.Vehicle.Vehicle.Label == trip.TripUpdate.Vehicle.Label))
                                .ToList();
                            IEnumerable<string> tripDuplicatiFeedTrip = from trip in feedTrip.Entities
                                                                        group trip by trip.TripUpdate.Trip.TripId into grp
                                                                        where grp.Count() > 1
                                                                        select grp.Key;
                            foreach (FeedEntity trip in soloTrip)
                            {
                                textBox2.AppendText($"Solo sul Feed Trip: {trip.TripUpdate.Vehicle.Label}" + Environment.NewLine);
                            }

                            foreach (FeedEntity vehicle in soloVehicle)
                            {
                                textBox2.AppendText($"Solo sul Feed Vehicle: {vehicle.Vehicle.Vehicle.Label}" + Environment.NewLine);
                            }
                            foreach (string tripDuplicato in tripDuplicatiFeedTrip)
                            {
                                textBox2.AppendText($"Trip Duplicato sul Feed Trip: {tripDuplicato}" + Environment.NewLine);
                                var dup = feedTrip.Entities.Where(x => x.TripUpdate.Trip.TripId == tripDuplicato).ToList();
                            }

                            textBox2.AppendText($"Vetture rilevate solo sul feed Trip: {soloTrip.Count}" + Environment.NewLine);
                            textBox2.AppendText($"Vetture rilevate solo sul feed Vehicle: {soloVehicle.Count}" + Environment.NewLine);
                        }
                    }
                    if (FeedVehicleManager.GTFS_RSM.OrarioProgrammato?.Count > 0)
                    {
                        
                        DataTable dtTabellato = new();
                        using (var reader = ObjectReader.Create(FeedVehicleManager.GTFS_RSM.OrarioProgrammato))
                        {
                            dtTabellato.Load(reader);
                        }
                        bindingSourceProgrammato.DataSource = dtTabellato;
                        dataGridView1.DataSource = bindingSourceProgrammato;
                    }
                }
                if (ecc != null)
                {
                    throw ecc;
                }
            }
            catch (Exception ex)
            {
                textBox1.AppendText($"{ex.Message}");
                Log.Error(ex, "Errore Generico");
            }
        }

        private int GetValidFeed()
        {
            imgUrl1.Image = null;
            imgUrl2.Image = null;
            List<Tuple<string, PictureBox>> tupleServer =
            [
                new Tuple<string, PictureBox>(urlVehicle.Text, imgUrl1),
                new Tuple<string, PictureBox>(urlVehicleRiserva.Text, imgUrl2)
            ];
            tupleServer.RemoveAll(x => string.IsNullOrEmpty(x.Item1));
            tupleServer.ForEach(x => x.Item2.Refresh());
            int codeFeed = -5;
            NumeroLetture++;
            foreach (Tuple<string, PictureBox> tupla in tupleServer)
            {
                string url = tupla.Item1;
                try
                {
                    codeFeed = FeedVehicleManager.LeggiFeedValido(url);

                    if (codeFeed == 0)
                    {
                        tupla.Item2.Image = Properties.Resources.verde;
                        NumeroFeedValidi++;
                        break;
                    }
                    else
                    {
                        tupla.Item2.Image = Properties.Resources.rosso;
                        if (codeFeed == -1)
                        {
                            textBox1.AppendText($"[{DateTime.Now:HH:mm:ss}] - Feed Scartato perchè NON LETTO{Environment.NewLine}");
                        }
                        else if (codeFeed == -2)
                        {
                            textBox1.AppendText($"[{DateTime.Now:HH:mm:ss}] - Feed Scartato perchè VUOTO{Environment.NewLine}");
                        }
                        else if (codeFeed == -10)
                        {
                            textBox1.AppendText($"Errore Lettura Feed{Environment.NewLine}");
                        }
                        else if (codeFeed == -3)
                        {
                            tupla.Item2.Image = Properties.Resources.arancio;
                            textBox1.AppendText($"[{DateTime.Now:HH:mm:ss}] - Feed scartato in quanto ha il timestamp SUPERATO{Environment.NewLine}");
                        }
                    }
                }
                catch (WebException ex) when (ex.Response is HttpWebResponse { StatusCode: HttpStatusCode.NotFound })
                {
                    textBox1.AppendText($"{ex.Message}: {Environment.NewLine}Feed Non trovato al seguente indirizzo");
                    textBox1.AppendText($"{Environment.NewLine}{ex.Response.ResponseUri}{Environment.NewLine}");
                    Log.Error(ex, "Feed {UrlFeed} Non trovato ", ex.Response.ResponseUri);
                    tupla.Item2.Image = Properties.Resources.rosso;
                }
                catch (WebException ex) when (ex.Status == WebExceptionStatus.Timeout)
                {
                    textBox1.AppendText($"{ex.Message} Problemi di connessione con il server{Environment.NewLine}");
                    Log.Error(ex, "Errore Connessione Server {UrlRemoto}", url);
                    tupla.Item2.Image = Properties.Resources.rosso;
                }
                catch (WebException ex) when (ex.Status == WebExceptionStatus.NameResolutionFailure)
                {
                    textBox1.AppendText($"{ex.Message} {Environment.NewLine}");
                    Log.Error(ex, "Errore Connessione Server {UrlRemoto}", url);
                    tupla.Item2.Image = Properties.Resources.rosso;
                }
                catch (Exception ex)
                {
                    textBox1.AppendText($"{ex.Message} {Environment.NewLine}");
                    Log.Error(ex, "Errore : ", ex.Message);
                    tupla.Item2.Image = Properties.Resources.rosso;
                }
            }
            return codeFeed;
        }

        private void RestartFile()
        {
            ResetUI();
            fileName = string.Empty;
            FeedVehicleManager.Reset();
            FeedAlertManager.Reset();
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

            NumeroFeedValidi = 0;
            NumeroLetture = 0;
        }

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
            if (deltaMilliSec == 0)
            {
                Log.Information("Acquisizione singola");
                Acquisizione();
            }
            if (!timerAcquisizione.Enabled && deltaMilliSec > 0)
            {
                Acquisizione();
                minuti.Enabled = false;
                secondi.Enabled = false;
                timerAcquisizione.Interval = deltaMilliSec;
                timerAcquisizione.Enabled = true;
                timerAcquisizione.Start();
                buttonPlayPause.BackgroundImage = Properties.Resources.pause;
                comboBox1.Enabled = false;
                buttonResetRegole.Enabled = false;
                Log.Information("Acquisizione attiva");
            }
            else
            {
                minuti.Enabled = true;
                secondi.Enabled = true;
                timerAcquisizione.Enabled = false;
                timerAcquisizione.Stop();
                buttonPlayPause.BackgroundImage = Properties.Resources.play;
                comboBox1.Enabled = true;
                buttonResetRegole.Enabled = true;
                if (deltaMilliSec > 0)
                {
                    Log.Information("Acquisizione in pausa");
                }
            }
        }

        private void AggiornaScottPlot()
        {
            if (FeedVehicleManager.ElencoVettureGrafico.Count > 0)
            {
                Render(plotAtac, plotTPL);
            }
        }

        public void Render(FormsPlot pltATAC, FormsPlot pltTPL)
        {
            var culture = CultureInfo.CreateSpecificCulture("it");
            var tempo = (from elenco in FeedVehicleManager.ElencoVettureGrafico select elenco.DateTime.ToOADate()).ToArray();
            var serieAtac = (from elenco in FeedVehicleManager.ElencoVettureGrafico select (double)elenco.Atac).ToArray();
            var serieAggregateATAC = (from elenco in FeedVehicleManager.ElencoVettureGrafico select (double)(elenco.AggregateAtac)).ToArray();

            pltATAC.Plot.Clear();

            //pltATAC.Plot.PlotSignalXY(tempo, serieAggregateATAC, markerSize: 5, color: Color.FromArgb(231, 109, 20), lineWidth: 4, label: "Aggregate");
            //pltATAC.Plot.PlotSignalXY(tempo, serieAtac, markerSize: 1, color: Color.FromArgb(137, 8, 39), lineWidth: 2, label: "Istantanee");
            var aggAtac = pltATAC.Plot.AddSignalXY(tempo, serieAggregateATAC, color: Color.FromArgb(231, 109, 20), label: "Aggregate");
            aggAtac.LineWidth = 2;
            aggAtac.MarkerSize = 2;
            var istAtac = pltATAC.Plot.AddSignalXY(tempo, serieAtac, color: Color.FromArgb(137, 8, 39), label: "Istantanee");
            istAtac.LineWidth = 2;
            istAtac.MarkerSize = 2;

            pltATAC.Plot.SetCulture(culture);
            pltATAC.Plot.XAxis.DateTimeFormat(true);
            //pltATAC.Plot.XAxis.TickLabelFormat("dd-MM HH:mm:ss", dateTimeFormat: true);            
            pltATAC.Plot.Legend(location: Alignment.UpperLeft);
            pltATAC.Plot.YAxis.Label(label: "Vetture rilevate");
            pltATAC.Plot.Title("Monitoraggio vetture ATAC");
            pltATAC.Plot.AxisAuto();
            pltATAC.Render();

            pltTPL.Plot.Clear();
            var serieTPL = (from elenco in FeedVehicleManager.ElencoVettureGrafico select (double)elenco.TPL).ToArray();
            var serieAggregateTPL = (from elenco in FeedVehicleManager.ElencoVettureGrafico select (double)(elenco.AggregateTPL)).ToArray();
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
            pltTPL.Plot.Title("Monitoraggio vetture TPL");
            pltTPL.Plot.AxisAuto();
            pltTPL.Render();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Version actualVersion = Assembly.GetExecutingAssembly().GetName().Version;
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
            radioRaggruppamento.Checked = true;
            checkAnomalieGTFS.Checked = Properties.Settings.Default.CheckAnomalie;
            checkSovraffollamento.Checked = Properties.Settings.Default.CheckSovraffollamento;
            checkMD5.Checked = Properties.Settings.Default.CheckMD5;
            checkDettagliVettura.Checked = Properties.Settings.Default.CheckDettagliVettura;
            checkTuttoPercorso.Visible = Properties.Settings.Default.ExtraSetting;
            checkResetSempre.Visible = Properties.Settings.Default.ExtraSetting;
            urlAlert.Visible = true; // Properties.Settings.Default.ExtraSetting;
            labelAlert.Visible = true; // Properties.Settings.Default.ExtraSetting;

            int totalSeconds = Properties.Settings.Default.DeltaTSec;
            minuti.Value = totalSeconds / 60;
            secondi.Value = totalSeconds % 60;

            #endregion

            LeggiFileConfigurazione();

            if (FeedVehicleManager.GTFS_RSM?.RegoleMonitoraggio?.Count > 0)
            {
                dataGridViolazioni.DataSource = FeedVehicleManager.GTFS_RSM.RegoleMonitoraggio;
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
            //FeedVehicleManager.LeggiGTFS($"Config{Path.DirectorySeparatorChar}GTFS_Static");
            bool usaDettagliVettura = checkDettagliVettura.Checked;
            FeedVehicleManager.GTFS_RSM = new GTFS_RSM($"Config{Path.DirectorySeparatorChar}GTFS_Static", usaDettagliVettura);
            UpdateBox.ExistNewerGTFS = false;
            UpdateBox.ExistNewerCSV = false;
            UpdateBox.ExistNewerVersion = false;
            UpdateBox.NewCSVDownloaded = false;
            UpdateBox.NewGTFSDownloaded = false;
            List<Route> elencoLinee = FeedVehicleManager.GTFS_RSM.StaticData.Routes
                .OrderBy(k => k.ShortName)
                .DefaultIfEmpty()
                .Distinct()
                .ToList();

            Route fittizia = new()
            {
                Id = "-1",
                ShortName = "    Tutte"
            };
            elencoLinee.Insert(0, fittizia);

            //comboBox1.DataSource = elencoLinee;
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
                //IsNullOrEmpty(x.LongName) ? x.ShortName : x.ShortName + " - " + x.LongName;
                return new Route { Id = x.Id, ShortName = linea };
            }).Distinct().ToList();
            comboBox1.DataSource = alternativa;
            comboBox1.ValueMember = "Id";
            comboBox1.DisplayMember = "ShortName";

            try
            {
                FeedVehicleManager.GTFS_RSM.LeggiRegoleMonitoraggio($"Config{Path.DirectorySeparatorChar}MonitoraggioLinee{Path.DirectorySeparatorChar}RegoleMonitoraggio_yes.txt");
            }
            catch
            {
                Log.Information("Nessuna RegoleMonitoraggio");
            }
        }

        private void LeggiRegoleAlertDaFile()
        {
            string pathAlert = $"Config{Path.DirectorySeparatorChar}Regole_Alert";
            bool esitoAlert = FeedVehicleManager.GTFS_RSM.LeggiAlertDaControllare(pathAlert);
            if (esitoAlert)
            {
                foreach (var alert in FeedVehicleManager.GTFS_RSM.AlertsDaControllare)
                {
                    if (tabMainForm.TabPages.ContainsKey(alert.Name))
                    {
                        tabMainForm.TabPages.RemoveByKey(alert.Name);
                    }

                    TabPage myNewTabItem = new()
                    {
                        Text = alert.Name,
                        Name = alert.Name
                    };
                    DataGridView myNewdataGridVetture = new()
                    {
                        AllowUserToAddRows = false,
                        AllowUserToDeleteRows = false,
                        AllowUserToOrderColumns = true,
                        AutoGenerateColumns = false,
                        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                        BackgroundColor = SystemColors.Control,
                        ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
                    };

                    DataGridViewTextBoxColumn columnLinea = new();
                    DataGridViewTextBoxColumn columnGiorno = new();
                    DataGridViewTextBoxColumn columnDa = new();
                    DataGridViewTextBoxColumn columnA = new();
                    DataGridViewTextBoxColumn columnVetturaDa = new();
                    DataGridViewTextBoxColumn columnVetturaA = new();
                    DataGridViewTextBoxColumn columnVetturaSbagliata = new();
                    DataGridViewTextBoxColumn columnOraPrimaViolazione = new();
                    DataGridViewTextBoxColumn columnOraUltimaViolazione = new();

                    myNewdataGridVetture.Columns.AddRange([
                                     columnLinea
                                    ,columnGiorno
                                    ,columnDa
                                    ,columnA
                                    ,columnVetturaDa
                                    ,columnVetturaA
                                    ,columnVetturaSbagliata
                                ]);

                    DataGridViewCellStyle timeFormat = new()
                    {
                        Format = "HH:mm:ss"
                    };

                    columnLinea.DataPropertyName = "Linea";
                    columnLinea.HeaderText = "Linea";
                    columnLinea.Name = "lineaDataGridViewTextBoxColumn1";
                    columnLinea.ReadOnly = true;

                    columnGiorno.DataPropertyName = "Giorno";
                    columnGiorno.HeaderText = "Giorno";
                    columnGiorno.ReadOnly = true;

                    columnDa.DataPropertyName = "Da";
                    columnDa.HeaderText = "Da";
                    columnDa.ReadOnly = true;

                    columnA.DataPropertyName = "A";
                    columnA.HeaderText = "A";
                    columnA.ReadOnly = true;

                    columnVetturaDa.DataPropertyName = "VetturaDa";
                    columnVetturaDa.HeaderText = "Vettura Da";
                    columnVetturaDa.ReadOnly = true;

                    columnVetturaA.DataPropertyName = "VetturaA";
                    columnVetturaA.HeaderText = "Vettura A";
                    columnOraUltimaViolazione.DefaultCellStyle = timeFormat;
                    columnVetturaA.ReadOnly = true;

                    columnVetturaSbagliata.DataPropertyName = "Violazione";
                    columnVetturaSbagliata.HeaderText = "Violazioni";
                    columnVetturaSbagliata.ReadOnly = true;

                    columnOraPrimaViolazione.DataPropertyName = "OraPrimaViolazione";
                    columnOraPrimaViolazione.HeaderText = "Prima Violazione";
                    columnOraPrimaViolazione.DefaultCellStyle = timeFormat;
                    columnOraPrimaViolazione.ReadOnly = true;

                    columnOraUltimaViolazione.DataPropertyName = "OraUltimaViolazione";
                    columnOraUltimaViolazione.HeaderText = "Ultima Violazione";
                    columnOraUltimaViolazione.DefaultCellStyle = timeFormat;
                    columnOraUltimaViolazione.ReadOnly = true;

                    myNewdataGridVetture.Dock = DockStyle.Fill;
                    myNewdataGridVetture.Location = new Point(3, 3);
                    myNewdataGridVetture.Name = alert.Name;
                    myNewdataGridVetture.ReadOnly = true;

                    myNewTabItem.Controls.Add(myNewdataGridVetture);
                    tabMainForm.TabPages.Add(myNewTabItem);

                    myNewdataGridVetture.Invalidate();
                    myNewdataGridVetture.Invalidate();
                    myNewdataGridVetture.DataSource = null;
                    myNewdataGridVetture.DataSource = alert.RegoleAlert;
                }

                if (FeedVehicleManager.GTFS_RSM.AlertsDaControllare.Count > 0)
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

            int esito = FeedVehicleManager.GTFS_RSM.LeggiCriteriMediaPonderata($"Config{Path.DirectorySeparatorChar}CriterioMediaPonderata.txt");
            if (esito == -1)
            {
                MessageBox.Show(text: "La somma dei pesi dei campioni deve essere 1", caption: "Attenzione", buttons: MessageBoxButtons.OK, icon: MessageBoxIcon.Error);
            }
        }

        private void TimerAcquisizione_Tick(object sender, EventArgs e)
        {
            Acquisizione();
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
            Properties.Settings.Default.RadioRaggruppamento = radioRaggruppamento.Name;
            Properties.Settings.Default.CheckSovraffollamento = checkSovraffollamento.Checked;
            Properties.Settings.Default.CheckMD5 = checkMD5.Checked;
            Properties.Settings.Default.CheckDettagliVettura = checkDettagliVettura.Checked;
            Properties.Settings.Default.Save();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (timerAcquisizione.Enabled)
            {
                DialogResult dialog = MessageBox.Show($"Interrompere il monitoraggio ed uscire",
                                                      $"Conferma Uscita",
                                                      MessageBoxButtons.YesNo,
                                                      MessageBoxIcon.Question);
                if (dialog == DialogResult.No)
                {
                    e.Cancel = true;
                }
                else if (!string.IsNullOrEmpty(fileName))
                {
                    _ = ExportGrid();
                }
            }
        }

        private async Task SaveAs(FileInfo outputFile, FileInfo altFileName)
        {
            if (checkXlsx.Checked)
            {
                using ExcelPackage excel = new(outputFile);
                try
                {
                    string excelSheetName;
                    if (FeedVehicleManager.LastValidationResultCode == 0)
                    {
                        excelSheetName = "Feed";
                        ElaboraSheet(excel, excelSheetName, FeedVehicleManager.ElencoAggregatoVetture);

                        if (checkGrafico.Checked)
                        {
                            foreach (ExcelWorksheet sheet in excel.Workbook.Worksheets)
                            {
                                if (sheet.Name == "Grafico")
                                {
                                    excel.Workbook.Worksheets.Delete("Grafico");
                                    break;
                                }
                            }

                            ExcelWorksheet workSheet = excel.Workbook.Worksheets.Add("Grafico");
                            PropertyInfo[] membersToInclude = typeof(MonitoraggioVettureGrafico)
                                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                .Where(p => !Attribute.IsDefined(p, typeof(IgnoreAttribute)))
                                .ToArray();

                            int colNumber = 2;
                            ExcelRangeBase range = workSheet.Cells[32, colNumber].LoadFromCollection(
                                FeedVehicleManager.ElencoVettureGrafico
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
                            lineChartATAC.Title.Text = $"Vetture Rilevate ATAC {FeedVehicleManager.FirstDataFeed:dd-MM-yyyy [HH:mm:ss}-{FeedVehicleManager.LastDataFeed:HH:mm:ss}] ";
                            lineChartTPL.Title.Text = $"Vetture Rilevate  TPL {FeedVehicleManager.FirstDataFeed:dd-MM-yyyy [HH:mm:ss}-{FeedVehicleManager.LastDataFeed:HH:mm:ss}] ";
                            ExcelRangeBase rangeLabel = range.Offset(1, 0, FeedVehicleManager.ElencoVettureGrafico.Count, 1);
                            ExcelRangeBase range1 = range.Offset(1, 2, FeedVehicleManager.ElencoVettureGrafico.Count, 1);
                            ExcelRangeBase range2 = range.Offset(1, 4, FeedVehicleManager.ElencoVettureGrafico.Count, 1);
                            ExcelRangeBase range3 = range.Offset(1, 3, FeedVehicleManager.ElencoVettureGrafico.Count, 1);
                            ExcelRangeBase range4 = range.Offset(1, 5, FeedVehicleManager.ElencoVettureGrafico.Count, 1);

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
                            List<LineaMonitorata> violazioniLineaMonitorata = FeedVehicleManager.ElencoLineeMonitorate.Where(x => x.OraPrimaViolazione.HasValue).ToList();
                            ElaboraSheet(excel, excelSheetName, violazioniLineaMonitorata);
                        }

                        if (checkAlert.Checked && checkAlert.Enabled)
                        {
                            foreach (AlertDaControllare alertDaControllare in FeedVehicleManager.GTFS_RSM.AlertsDaControllare)
                            {
                                excelSheetName = alertDaControllare.Name;
                                ElaboraSheet(excel, excelSheetName, alertDaControllare.ViolazioniAlert, dateFormat: "HH:mm:ss");
                            }
                        }

                        if (checkAnomalieGTFS.Checked)
                        {
                            excelSheetName = "AnomalieGTFS";
                            List<string> ammessi = ["Matricola", "Linea", "PrimaVolta", "TripId", "CurrentStopSequence", "Delta"];
                            ElaboraSheet(excel, excelSheetName, FeedVehicleManager.AnomaliaGTFS, ammessi);
                        }

                        if (checkSovraffollamento.Checked)
                        {
                            excelSheetName = "Sovraffollamneto";
                            ElaboraSheet(excel, excelSheetName, FeedVehicleManager.ElencoVettureSovraffollate);
                        }

                        if (FeedVehicleManager.GTFS_RSM.OrarioProgrammato?.Count > 0)
                        {
                            excelSheetName = "OrarioProgrammato";
                            ElaboraSheet(excel, excelSheetName, FeedVehicleManager.GTFS_RSM.OrarioProgrammato);
                        }
                    }

                    excelSheetName = "Avvisi";
                    if (FeedAlertManager.LastValidationResultCode == 0)
                    {
                        ElaboraSheet(excel, excelSheetName, FeedAlertManager.Avvisi);
                    }
                    else if (FeedAlertManager.FirstDataFeed.HasValue)
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

            if (checkCSV.Checked)
            {
                using var writer = new StreamWriter($"OUTPUT{Path.DirectorySeparatorChar}{fileName}.csv");
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ";",
                };
                using var csv = new CsvWriter(writer, config);
                await csv.WriteRecordsAsync(FeedVehicleManager.ElencoAggregatoVetture);
            }
        }
        /*
        private async Task<bool> SaveAs(FileInfo outputFile)
        {
            bool retVal = true;
            if (checkXlsx.Checked)
            {
                using (ExcelPackage excel = new ExcelPackage(outputFile))
                {
                    try
                    {
                        string excelSheetName ;
                        if (FeedVehicleManager.LastValidationResultCode == 0)
                        {
                            excelSheetName = "Feed";
                            ElaboraSheet(excel, excelSheetName, FeedVehicleManager.ElencoAggregatoVetture);

                            if (checkGrafico.Checked)
                            {
                                foreach (ExcelWorksheet sheet in excel.Workbook.Worksheets)
                                {
                                    if (sheet.Name == "Grafico")
                                    {
                                        excel.Workbook.Worksheets.Delete("Grafico");
                                        break;
                                    }
                                }

                                ExcelWorksheet workSheet = excel.Workbook.Worksheets.Add("Grafico");
                                PropertyInfo[] membersToInclude = typeof(MonitoraggioVettureGrafico)
                                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                    .Where(p => !Attribute.IsDefined(p, typeof(IgnoreAttribute)))
                                    .ToArray();

                                int colNumber = 2;
                                ExcelRangeBase range = workSheet.Cells[32, colNumber].LoadFromCollection(
                                    FeedVehicleManager.ElencoVettureGrafico
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
                                lineChartATAC.Title.Text = $"Vetture Rilevate ATAC {FeedVehicleManager.FirstDataFeed:dd-MM-yyyy [HH:mm:ss}-{FeedVehicleManager.LastDataFeed:HH:mm:ss}] ";
                                lineChartTPL.Title.Text = $"Vetture Rilevate  TPL {FeedVehicleManager.FirstDataFeed:dd-MM-yyyy [HH:mm:ss}-{FeedVehicleManager.LastDataFeed:HH:mm:ss}] ";
                                ExcelRangeBase rangeLabel = range.Offset(1, 0, FeedVehicleManager.ElencoVettureGrafico.Count, 1);
                                ExcelRangeBase range1 = range.Offset(1, 2, FeedVehicleManager.ElencoVettureGrafico.Count, 1);
                                ExcelRangeBase range2 = range.Offset(1, 4, FeedVehicleManager.ElencoVettureGrafico.Count, 1);
                                ExcelRangeBase range3 = range.Offset(1, 3, FeedVehicleManager.ElencoVettureGrafico.Count, 1);
                                ExcelRangeBase range4 = range.Offset(1, 5, FeedVehicleManager.ElencoVettureGrafico.Count, 1);

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
                                List<LineaMonitorata> violazioniLineaMonitorata = FeedVehicleManager.ElencoLineeMonitorate.Where(x => x.OraPrimaViolazione.HasValue).ToList();
                                ElaboraSheet(excel, excelSheetName, violazioniLineaMonitorata);
                            }

                            if (checkAlert.Checked && checkAlert.Enabled)
                            {
                                foreach (AlertDaControllare alertDaControllare in FeedVehicleManager.GTFS_RSM.AlertsDaControllare)
                                {
                                    excelSheetName = alertDaControllare.Name;
                                    ElaboraSheet(excel, excelSheetName, alertDaControllare.ViolazioniAlert, dateFormat: "HH:mm:ss");
                                }
                            }

                            if (checkAnomalieGTFS.Checked)
                            {
                                excelSheetName = "AnomalieGTFS";
                                List<string> ammessi = new List<string> { "Matricola", "Linea", "PrimaVolta", "TripId", "CurrentStopSequence", "Delta" };
                                ElaboraSheet(excel, excelSheetName, FeedVehicleManager.AnomaliaGTFS, ammessi);
                            }

                            if (checkSovraffollamento.Checked)
                            {
                                excelSheetName = "Sovraffollamneto";
                                ElaboraSheet(excel, excelSheetName, FeedVehicleManager.ElencoVettureSovraffollate);
                            }
                        }

                        excelSheetName = "Avvisi";
                        if (FeedAlertManager.LastValidationResultCode == 0)
                        {                            
                            ElaboraSheet(excel, excelSheetName, FeedAlertManager.Avvisi);
                        }
                        else if (FeedAlertManager.FirstDataFeed.HasValue)
                        {
                            excel.Workbook.Worksheets.MoveToEnd(excelSheetName);
                        }
                        
                        excel.Save();
                    }
                    catch (Exception exc)
                    {
                        retVal = false;
                        Log.Error("{Exception}", exc);
                    }
                }
            }

            if (checkCSV.Checked)
            {
                using (var writer = new StreamWriter($"OUTPUT{Path.DirectorySeparatorChar}{fileName}.csv"))
                {
                    var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        Delimiter = ";",
                    };
                    using (var csv = new CsvWriter(writer, config))
                    {
                        await csv.WriteRecordsAsync(FeedVehicleManager.ElencoAggregatoVetture);
                    }
                }
            }
            return retVal;
        }         */
        private static void ElaboraSheet<T>(ExcelPackage excel, string excelSheetName, List<T> record, List<string> ammessi = null, string dateFormat = "")
        {
            foreach (ExcelWorksheet sheet in excel.Workbook.Worksheets)
            {
                if (sheet.Name == excelSheetName)
                {
                    excel.Workbook.Worksheets.Delete(excelSheetName);
                    break;
                }
            }

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
            //else
            //{
            //    buttonVerificaAggiornamenti.Image = Properties.Resources.verde;
            //}
        }

        private async Task ExportGrid()
        {
            try
            {
                FileInfo file = new($"OUTPUT{Path.DirectorySeparatorChar}{fileName}.xlsx");
                FileInfo altFileName = new($"OUTPUT{Path.DirectorySeparatorChar}{fileName}.xlsx.bck");
                await SaveAs(file, altFileName);
            }
            catch (Exception e)
            {
                Log.Error("{Exception}", e);
                throw;
            }
        }

        private void Random(object sender, EventArgs e)
        {
            DateTime t0;
            if (FeedVehicleManager.ElencoVettureGrafico.Count == 0)
            {
                t0 = DateTime.Now;
            }
            else
            {
                t0 = FeedVehicleManager.ElencoVettureGrafico[^1].DateTime;
            }
            if (FeedVehicleManager.ElencoVettureGrafico.Count < 100000)
            {
                Random rand = new(0);
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
                    MonitoraggioVettureGrafico nuovoMonitoraggio = new()
                    {
                        DateTime = t0.AddSeconds(i * 30),
                        Aggregate = aggregate,
                        Rilevate = rilevate,
                        Atac = atac,
                        TPL = tpl,
                        Aggiunte = aggiunte,
                        Tolte = tolte
                    };
                    FeedVehicleManager.ElencoVettureGrafico.Add(nuovoMonitoraggio);
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
            IEnumerable<LineaMonitorata> dataSource = (IEnumerable<LineaMonitorata>)dataGridViolazioni.DataSource;
            LineaMonitorata riga;
            for (int i = 0; i < dataGridViolazioni.RowCount; i++)
            {
                DataGridViewRow row = dataGridViolazioni.Rows[i];
                riga = dataSource.ElementAt(i);
                if (riga.VettureRilevate < riga.VetturePreviste)
                {
                    TimeSpan span = riga.OraUltimaViolazione.GetValueOrDefault(FeedVehicleManager.LastDataFeed.GetValueOrDefault()) - riga.OraPrimaViolazione.GetValueOrDefault(DateTime.MinValue);

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
            if ((sender as TabControl).SelectedTab == tabMonitoraggio && FeedVehicleManager.LastDataFeed.HasValue)
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
                DataResetMonitoraggio = new DateTime(now.Year, now.Month, now.Day, oraReset.Hours, oraReset.Minutes, oraReset.Seconds);
            }
            else
            {
                DataResetMonitoraggio = null;
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

            buttonVerificaAggiornamenti.Image = Properties.Resources.available_updates_16;
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
            FeedVehicleManager.GTFS_RSM?.LeggiDettagliVettura(checkDettagliVettura.Checked);
        }

        private void GridAvvisi_FilterStringChanged(object sender, Zuby.ADGV.AdvancedDataGridView.FilterEventArgs e)
        {
            bindingSourceAvvisi.Filter = GridAvvisi.FilterString;
        }

        private void GridAvvisi_SortStringChanged(object sender, Zuby.ADGV.AdvancedDataGridView.SortEventArgs e)
        {
            bindingSourceAvvisi.Sort = GridAvvisi.SortString;
        }

        private void GridAvvisi_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

    }
}
