using CsvHelper;
using CsvHelper.Configuration.Attributes;
using FastMember;
using GTFS;
using GTFS.Entities;
using GTFS.IO;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Table;
using ProtoBuf;
using ScottPlot;
using Serilog;
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
using TransitRealtime;

namespace AtacFeed
{
    public partial class FormGTFS_RSM : Form
    {

        public bool needToRestart = false;

        private static readonly DateTime t0 = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        private List<ExtendedVehicleInfo> ElencoAggregatoVetture;
        private List<ExtendedVehicleInfo> ElencoPrecedente;
        private List<ErroriGTFS> AnomaliaGTFS;
        private List<LineaAgenzia> ElencoLineaAgenzia;
        private List<DettagliVettura> ElencoDettagliVettura;

        private List<MonitoraggioVettureGrafico> ElencoVettureGrafico;
        private List<RegolaMonitoraggio> RegoleMonitoraggio;
        private List<LineaMonitorata> ElencoLineeMonitorate;

        private List<AlertDaControllare> AlertsDaControllare;
        private List<CriterioMediaPonderata> CriteriMediaPonderata;
        
        private string fileName;
        private DateTime? LastDataFeedVehicle;
        private DateTime? FirstDataFeedVehicle;
        private DateTime? DataResetMonitoraggio;

        private List<RunTimeValueAlert> ElencoVettureSovraffollate;

        public FormGTFS_RSM()
        {
            InitializeComponent();
        }

        private GTFSFeed staticData = new GTFSFeed();

        private async Task AcquisizioneAsync()
        {
            WebRequest req = HttpWebRequest.Create(urlVehicle.Text);
            req.Timeout = 10000;
            try
            {                
                string routeID = comboBox1.SelectedValue.ToString();
                FeedMessage actualFeedVehicle = Serializer.Deserialize<FeedMessage>(req.GetResponse().GetResponseStream());
                
                if (actualFeedVehicle.Entities.Count == 0)
                {
                    Log.Error("Feed VUOTO alle {DateTime:HH:mm:ss}" ,DateTime.Now);
                    throw new Exception("Il feed letto è vuoto");                    
                }
                DateTime dataFeedVehicle = t0.AddSeconds(actualFeedVehicle.Header.Timestamp).ToLocalTime();
                
                if (DataResetMonitoraggio.HasValue && dataFeedVehicle > DataResetMonitoraggio.GetValueOrDefault() )
                {                    
                    RestartFile();
                    DataResetMonitoraggio = DataResetMonitoraggio.Value.AddDays(1);
                    Log.Information("Prossimo data reset: {DataResetMonitoraggio:dd/MM/yyyy HH:mm:ss}", DataResetMonitoraggio);
                }
                
                if (LastDataFeedVehicle.GetValueOrDefault() != dataFeedVehicle)
                {
                    LastDataFeedVehicle = dataFeedVehicle;
                    textBox1.Clear();
                    textBox2.Clear();
                    
                    lblOraLettura.Text = $"{dataFeedVehicle:HH:mm:ss}";
                    labelFeedLetti.Text = (int.Parse(labelFeedLetti.Text) + 1).ToString();
                    List<FeedEntity> feedEntities = actualFeedVehicle.Entities
                        .Where(x => ((int.TryParse(routeID,out int res) && res < 0) || x.Vehicle.Trip?.RouteId == routeID)
                                && (!checkTripVuoti.Checked || x.Vehicle?.Trip?.TripId.Length > 0)
                           )
                        .ToList();
                    List<ExtendedVehicleInfo> elencoVetture = feedEntities
                        .GroupJoin(
                            inner: ElencoLineaAgenzia,
                            outerKeySelector: v => v.Vehicle.Trip?.RouteId,
                            innerKeySelector: l => l.Route.Id,
                            resultSelector: (v, l) => new { v.Vehicle, Linea = l.FirstOrDefault() })
                        .GroupJoin(
                            inner: ElencoDettagliVettura,
                            outerKeySelector: o2 => o2.Vehicle.Vehicle?.Label.Trim(),
                            innerKeySelector: i2 => i2.Matricola,
                            resultSelector: (o2, i2) => new { o2, DettagliVettura = i2.FirstOrDefault() })
                        .Select(x => new ExtendedVehicleInfo(
                                        x.o2.Vehicle.Vehicle.Id,
                                        x.o2.Vehicle.Vehicle.Label,
                                        x.o2.Vehicle.Vehicle.LicensePlate,
                                        x.o2.Vehicle.Trip?.RouteId,
                                        x.o2.Linea?.Route.ShortName,
                                        x.DettagliVettura?.Gestore ?? x.o2.Linea.Agency.Name,
                                        x.o2.Vehicle.Trip?.DirectionId,
                                        x.o2.Vehicle.CurrentStopSequence,
                                        x.o2.Vehicle.congestion_level,
                                        x.o2.Vehicle.occupancy_status,
                                        x.o2.Vehicle.Trip?.TripId,
                                        checkTripDuplicati.Checked,
                                        dataFeedVehicle,
                                        x.DettagliVettura?.Rimessa,
                                        x.DettagliVettura?.Euro,
                                        x.DettagliVettura?.Modello,
                                        x.o2.Vehicle.Position.Latitude,
                                        x.o2.Vehicle.Position.Longitude,
                                        x.o2.Vehicle.CurrentStatus,
                                        x.DettagliVettura?.TipoMezzoTrasporto.GetValueOrDefault(0) ?? ( string.Equals(x.o2.Linea.Agency.Name, "atac", StringComparison.OrdinalIgnoreCase) ? (x.o2.Linea.Route.Type == GTFS.Entities.Enumerations.RouteTypeExtended.TramService ? -1 : -2): -3)
                                        ,
                                        checkTuttoPercorso.Visible && checkTuttoPercorso.Checked
                                    )
                        )
                        .Distinct()
                        .OrderBy(x => x.IdVettura)
                        .ToList();
                    List<ExtendedVehicleInfo> vettureTolte = ElencoPrecedente.Except(elencoVetture).ToList();
                    List<ExtendedVehicleInfo> vettureAggiunte = elencoVetture.Except(ElencoPrecedente).ToList();

                    var vettureAggiunteTolte =
                        from tolte in vettureTolte
                        join aggiunte in vettureAggiunte on tolte.Matricola equals aggiunte.Matricola
                        select new { tolte.Matricola };

                    vettureTolte = vettureTolte.Where(x => !vettureAggiunteTolte.Any(c => c.Matricola == x.Matricola)).ToList();
                    vettureAggiunte = vettureAggiunte.Where(x => !vettureAggiunteTolte.Any(c => c.Matricola == x.Matricola)).ToList();

                    if (ElencoPrecedente.Count > 0)
                    {
                        foreach (ExtendedVehicleInfo vettura in vettureAggiunte)
                        {
                            textBox3.AppendText($"{vettura.IdVettura} - {vettura.Matricola} rilevata alle {dataFeedVehicle:HH:mm:ss} {Environment.NewLine}");
                        }

                        foreach (ExtendedVehicleInfo vettura in vettureTolte)
                        {
                            textBox4.AppendText($"{vettura.IdVettura} - {vettura.Matricola} NON rilevata alle {dataFeedVehicle:HH:mm:ss} {Environment.NewLine}");
                        }

                        /// Controllo accuratezza GTFS rispetto al precedente letto                                                                        
                        List<ExtendedVehicleInfo> vettureFresche = elencoVetture
                                .Where(x => !ElencoPrecedente.Any(c => c.Matricola == x.Matricola && c.TripId == x.TripId))
                                .ToList();
                                                
                        List<ExtendedVehicleInfo> partenzaAvanzata = vettureFresche
                            .Where(x => x.CurrentStopSequence>1 && !ElencoAggregatoVetture.Any(c => c.Matricola == x.Matricola && c.TripId == x.TripId))
                            .ToList();
                        if (partenzaAvanzata.Count() > 0)
                        {
                            textBox2.AppendText($"Vetture con 'partenza avanzata'{Environment.NewLine}");
                            foreach (ExtendedVehicleInfo errore in partenzaAvanzata)
                            {
                                textBox2.AppendText($"Matricola {errore.Matricola} Linea {errore.Linea} Fermata {errore.CurrentStopSequence}{Environment.NewLine}");
                                int lineNumberToSelect = textBox2.Lines.Count() - 2;
                                int start = textBox2.GetFirstCharIndexFromLine(lineNumberToSelect);
                                int length = textBox2.Lines[lineNumberToSelect].Length;
                                textBox2.Select(start, length);
                                textBox2.SelectionColor = Color.CornflowerBlue;
                                textBox2.SelectionIndent = 10;
                                AnomaliaGTFS.Add(new ErroriGTFS(errore, (int)errore.CurrentStopSequence));
                            }
                            textBox2.AppendText($"{Environment.NewLine}");
                        }

                        
                        List<ExtendedVehicleInfo> vettureRiagganciate = vettureFresche
                            .Where(x => ElencoAggregatoVetture.Any(c => c.Matricola == x.Matricola && c.TripId == x.TripId))
                            .ToList();
                        //.Except(partenzaAvanzata)
                        //.Where(x => x.CurrentStopSequence > 1)
                        //.ToList();
                        if (vettureRiagganciate.Count() > 0)
                        {
                            textBox2.AppendText($"Vetture 'riagganciate'{Environment.NewLine}");
                            foreach (ExtendedVehicleInfo errore in vettureRiagganciate)
                            {
                                textBox2.AppendText($"Matricola {errore.Matricola} Linea {errore.Linea} Fermata {errore.CurrentStopSequence}{Environment.NewLine}");
                                int lineNumberToSelect = textBox2.Lines.Count() - 2;
                                int start = textBox2.GetFirstCharIndexFromLine(lineNumberToSelect);
                                int length = textBox2.Lines[lineNumberToSelect].Length;
                                textBox2.Select(start, length);
                                textBox2.SelectionColor = Color.CornflowerBlue;
                                textBox2.SelectionIndent = 10;
                                uint ultimaFermataRilevata = ElencoAggregatoVetture
                                        .Where(x => x.TripId == errore.TripId && x.Matricola == errore.Matricola)
                                        .Max(x => x.CurrentStopSequence);
                                //   .OrderByDescending(x => x.CurrentStopSequence).FirstOrDefault();
                                int delta = (int)(errore.CurrentStopSequence - ultimaFermataRilevata);
                                AnomaliaGTFS.Add(new ErroriGTFS(errore, delta));
                            }
                            textBox2.AppendText($"{Environment.NewLine}");
                        }

                        var percorsoAnomalo =
                            from act in elencoVetture
                            join prec in ElencoPrecedente 
                                on (act.Matricola, act.TripId) equals (prec.Matricola, prec.TripId)
                            where (act.CurrentStopSequence - prec.CurrentStopSequence > 2) || (act.CurrentStopSequence - prec.CurrentStopSequence < 0)
                            select new { Vehicle = act, Delta = (int)(act.CurrentStopSequence - prec.CurrentStopSequence) };
                        if (percorsoAnomalo.Count() > 0)
                        {
                            textBox2.AppendText($"Vetture con progressivo fermate 'bucato'{Environment.NewLine}");
                            foreach (var errore in percorsoAnomalo)
                            {
                                textBox2.AppendText(text: $"Matricola {errore.Vehicle.Matricola} Linea {errore.Vehicle.Linea} Fermata {errore.Vehicle.CurrentStopSequence} => 'balzo' di {errore.Delta}{Environment.NewLine}");
                                int delta = errore.Delta;
                                int lineNumberToSelect = textBox2.Lines.Count() - 2;
                                int start = textBox2.GetFirstCharIndexFromLine(lineNumberToSelect);
                                int length = textBox2.Lines[lineNumberToSelect].Length;
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
                                AnomaliaGTFS.Add(new ErroriGTFS(errore.Vehicle, errore.Delta));
                            }
                            textBox2.AppendText($"{Environment.NewLine}");
                        }
                    }

                    /// Controllo accuratezza GTFS (Verifica TripId Duplicati)
                    IEnumerable<string> tripDuplicatiFeedVehicle = from trip in feedEntities
                                                                   where trip.Vehicle.Trip?.TripId != null
                                                                   group trip by trip.Vehicle.Trip.TripId into grp
                                                                   where grp.Count() > 1
                                                                   select grp.Key;
                    if (tripDuplicatiFeedVehicle.Count() > 0)
                    {
                        textBox2.AppendText($"Trip Duplicati{Environment.NewLine}");
                        foreach (string tripDuplicato in tripDuplicatiFeedVehicle)
                        {
                            var elencoVettureSuTripIdDuplicato = string.Join(", ", feedEntities.Where(x => x.Vehicle.Trip.TripId == tripDuplicato).Select(x => x.Vehicle.Vehicle.Label));
                            textBox2.AppendText($"Trip {tripDuplicato}\tVetture:[{elencoVettureSuTripIdDuplicato}]{Environment.NewLine}");
                            int lineNumberToSelect = textBox2.Lines.Count() - 2;
                            int start = textBox2.GetFirstCharIndexFromLine(lineNumberToSelect);
                            int length = textBox2.Lines[lineNumberToSelect].Length;
                            textBox2.Select(start, length);
                            textBox2.SelectionColor = Color.Tomato;
                            textBox2.SelectionIndent = 10;
                        }
                        textBox2.AppendText($"{Environment.NewLine}");
                    }
                    
                    /// Controllo accuratezza GTFS (Matricole o IDVehicle vuoto)
                    List<ExtendedVehicleInfo> vettureSenzaMatricola= elencoVetture.Where(x => string.IsNullOrEmpty(x.Matricola) || string.IsNullOrEmpty(x.IdVettura)).ToList();

                    if (vettureSenzaMatricola.Count() > 0)
                    {
                        textBox2.AppendText($"Vetture Senza Matricola{Environment.NewLine}");
                        foreach (ExtendedVehicleInfo vettura in vettureSenzaMatricola)
                        {
                            textBox2.AppendText($"IdVettura {vettura.IdVettura}\t Matricola:[{vettura.Matricola}]{Environment.NewLine}");
                            int lineNumberToSelect = textBox2.Lines.Count() - 2;
                            int start = textBox2.GetFirstCharIndexFromLine(lineNumberToSelect);
                            int length = textBox2.Lines[lineNumberToSelect].Length;
                            textBox2.Select(start, length);
                            textBox2.SelectionColor = Color.DarkGray;
                            textBox2.SelectionIndent = 10;
                        }
                        textBox2.AppendText($"{Environment.NewLine}");
                    }

                    textBox2.AppendText($"{Environment.NewLine}");
                    textBox2.Select(0, 0);

                    foreach (ExtendedVehicleInfo vettura in elencoVetture)
                    {
                        var presente = ElencoAggregatoVetture
                            .FirstOrDefault(x => x.Matricola == vettura.Matricola
                                                 && (!checkTripDuplicati.Checked || x.TripId == vettura.TripId)
                                                 && (!(checkTuttoPercorso.Visible && checkTuttoPercorso.Checked) || x.CurrentStopSequence == vettura.CurrentStopSequence)
                                                 );
                        if (presente != null)
                        {
                            vettura.PrimaVolta = presente.PrimaVolta;
                            vettura.OccupancyStatus = vettura.OccupancyStatus.CompareTo(presente.OccupancyStatus) >= 0 ? vettura.OccupancyStatus : presente.OccupancyStatus;
                        }
                    }

                    ElencoAggregatoVetture = elencoVetture.Union(ElencoAggregatoVetture).ToList();
                    labelTotaleRighe.Text = ElencoAggregatoVetture.Count.ToString();
                    int TotaleMatricola = ElencoAggregatoVetture.Select(i => i.Matricola.Trim()).Distinct().Count();

                    IEnumerable<(string Matricola, int TipoMezzoTrasporto)> elencoAggregatoAtac = ElencoAggregatoVetture
                        .Where(i => i.TipoMezzoTrasporto == 0 || i.TipoMezzoTrasporto == 1 || i.TipoMezzoTrasporto == 2 || i.TipoMezzoTrasporto == 5 || i.TipoMezzoTrasporto == 6 || i.TipoMezzoTrasporto == -2)
                        .Select(i => (Matricola: i.Matricola.Trim(), i.TipoMezzoTrasporto))
                        .Distinct();
                    int TotaleMatricolaAtac = elencoAggregatoAtac.Count();

                    IEnumerable <(string Matricola, int TipoMezzoTrasporto)> elencoAggregatoTPL = 
                        ElencoAggregatoVetture
                        .Where(i => i.TipoMezzoTrasporto == 3 || i.TipoMezzoTrasporto == 4 || i.TipoMezzoTrasporto == 3)
                        .Select(i => (Matricola: i.Matricola.Trim(), i.TipoMezzoTrasporto))
                        .Distinct();
                    int TotaleMatricolaTPL = elencoAggregatoTPL.Count();

                    int TotaleIdVettura = ElencoAggregatoVetture.Select(i => i.IdVettura).Distinct().Count();

                    int rilevatoBusAtac = elencoVetture.Where(x => x.TipoMezzoTrasporto == 0).Count();
                    int rilevatoTramAtac = elencoVetture.Where(x => x.TipoMezzoTrasporto == 1).Count();
                    int rilevatoFilobusAtac = elencoVetture.Where(x => x.TipoMezzoTrasporto == 2).Count();
                    int rilevatoMinibusElettrici = elencoVetture.Where(x => x.TipoMezzoTrasporto == 5).Count();
                    int rilevatoFurgoncini = elencoVetture.Where(x => x.TipoMezzoTrasporto == 6).Count();
                    int rilevatoFerro = elencoVetture.Where(x => x.TipoMezzoTrasporto == -1).Count();
                    int rilevatoAltroAtac = elencoVetture.Where(x => x.TipoMezzoTrasporto == -2).Count();
                    //ElencoAggregatoVetture.Where(i => string.Equals(i.Gestore, "atac", StringComparison.OrdinalIgnoreCase)).Select(i => i.Matricola.Trim()).Distinct().Count() - TotaleMatricolaAtac;





                    labelTotaleIdVettura.Text = TotaleIdVettura.ToString();
                    labelTotaleMatricola.Text = TotaleMatricola.ToString();
                    labelBUS.Text = rilevatoBusAtac.ToString();
                    labelTRAM.Text = rilevatoTramAtac.ToString();
                    labelFILOBUS.Text = rilevatoFilobusAtac.ToString();
                    labelMiniBusEle.Text = rilevatoMinibusElettrici.ToString();
                    labelFurgoncino.Text = rilevatoFurgoncini.ToString();
                    labelFerro.Text = rilevatoFerro.ToString();
                    labelAltro.Text = rilevatoAltroAtac.ToString();

                    labelTotaleMatricolaATAC.Text = TotaleMatricolaAtac.ToString();

                    labelTotaleMatricolaTPL.Text = elencoAggregatoTPL.Count().ToString();

                    dataGridVetture.DataSource = ElencoAggregatoVetture;

                    DataTable dt = new DataTable();
                    using (var reader = ObjectReader.Create(ElencoAggregatoVetture))
                    {
                        dt.Load(reader);
                    }

                    extendedVehicleInfoBindingSource.DataSource = dt;
                    advancedDataGridView1.DataSource = this.extendedVehicleInfoBindingSource;

                    List<string> urlTripList = new List<string>();
                    List<string> urlVehicleList = new List<string>();
                    foreach (FeedEntity entity in feedEntities)
                    {
                        if (entity.Vehicle != null && entity.Vehicle.Trip != null && !(int.TryParse(routeID, out int res) && res != -1)  && entity.Vehicle.Trip.RouteId == routeID)
                        {
                            textBox1.AppendText(entity.Vehicle.Vehicle.Id + Environment.NewLine);
                        }
                    }
                    
                    List<ExtendedVehicleInfo> listaMezziSuLinea = elencoVetture
                        .Where(x => x.TripId != null)
                        .ToList();
                    List<ExtendedVehicleInfo> listaBusAttesa = elencoVetture.Where(x => x.TripId == null).ToList();
                    
                    int numVettureTPLFeedVehicle = elencoVetture
                        .Where(i => i.TipoMezzoTrasporto == 3 || i.TipoMezzoTrasporto == 4 || i.TipoMezzoTrasporto == -3)
                        .Count();
                    var rrr = elencoVetture.Where(i => i.Gestore.Contains("tpl") && (i.TipoMezzoTrasporto == 3 || i.TipoMezzoTrasporto == 4 || i.TipoMezzoTrasporto == -3)).ToList();

                    int busLinea = listaMezziSuLinea.Count;
                    int busAttesa = listaBusAttesa.Count;
                    int busTotale = busLinea + busAttesa;
                    textBox1.AppendText($"Totale Vetture Rilevate sul Feed Vehicle {busTotale}" + Environment.NewLine);
                    textBox1.AppendText($"\tATAC {busTotale - numVettureTPLFeedVehicle}\tTPL {numVettureTPLFeedVehicle}" + Environment.NewLine);
                    textBox1.AppendText($"\tSu Linea {busLinea}\tIn Attesa {busAttesa}" + Environment.NewLine);
                    labelTPL.Text = $"{numVettureTPLFeedVehicle}";
                    labelAtac.Text = $"{busTotale - numVettureTPLFeedVehicle}";
                    //labelWait.Text = $"{busAttesa}";
                    labelTot.Text = $"{busTotale}";

                    if (vettureAggiunte.Count > 0 || vettureTolte.Count > 0 || ElencoPrecedente.Count == 0 || elencoVetture.Count > 0)
                    {
                        if (string.IsNullOrEmpty(fileName))
                        {
                            fileName = $"Feed_{dataFeedVehicle:yyyy-MM-dd (HH_mm_ss)}";
                            FirstDataFeedVehicle = dataFeedVehicle;
                        }
                        MonitoraggioVettureGrafico nuovoMonitoraggio = new MonitoraggioVettureGrafico
                        {
                            DateTime = dataFeedVehicle,
                            Aggregate = TotaleMatricola,
                            AggregateAtac = TotaleMatricolaAtac,
                            AggregateTPL = TotaleMatricolaTPL,
                            Rilevate = busTotale,
                            Atac = rilevatoBusAtac + rilevatoTramAtac+ rilevatoFilobusAtac + rilevatoMinibusElettrici + rilevatoFurgoncini + rilevatoAltroAtac, // busTotale - numVettureTPLFeedVehicle,
                            TPL = numVettureTPLFeedVehicle,
                            Aggiunte = ElencoPrecedente.Count > 0 ? vettureAggiunte.Count : 0,
                            Tolte = vettureTolte.Count
                        };
                        ElencoVettureGrafico.Add(nuovoMonitoraggio);

                        int campioniNecessari = CriteriMediaPonderata.Sum(x => x.NumeroCampioni);
                        if (ElencoVettureGrafico.Count >= campioniNecessari)
                        {
                            int startIndex = ElencoVettureGrafico.Count;
                            double ponderateAac = 0;
                            double ponderateTPL = 0;
                            foreach (var item in CriteriMediaPonderata)
                            {
                                int numeroCampioni = item.NumeroCampioni;
                                startIndex -= numeroCampioni;

                                ponderateAac +=
                                    item.Peso / numeroCampioni *
                                    ElencoVettureGrafico
                                        .Skip(startIndex)
                                        .Take(numeroCampioni)
                                        .Sum(x => x.Atac)
                                        ;
                                ponderateTPL +=
                                    (item.Peso / numeroCampioni *
                                    ElencoVettureGrafico
                                        .Skip(startIndex)
                                        .Take(numeroCampioni)
                                        .Sum(x => x.TPL));
                            }
                            labelPonderatiATAC.Text = Math.Round(ponderateAac).ToString();
                            labelPonderatiTPL.Text = Math.Round(ponderateTPL).ToString();
                        }

                        int giornosettimana = (int)dataFeedVehicle.DayOfWeek;
                        TimeSpan oraFeed = dataFeedVehicle.TimeOfDay;

                        if (tabMainForm.TabPages.Contains(tabMonitoraggio))
                        {
                            var vettureSuLinea = elencoVetture
                                .GroupBy(c => c.Linea)
                                .Select(g => new
                                {
                                    Linea = g.Key,
                                    count = g.Count(),
                                    date = g.Max(x => x.UltimaVolta)
                                })
                                .OrderByDescending(c => c.date);

                            

                            IEnumerable<RegolaMonitoraggio> regoleApplicabili = from regoleLineeScoperte in RegoleMonitoraggio
                                                                                where regoleLineeScoperte.Giorno.Contains(giornosettimana.ToString())
                                                                                      && regoleLineeScoperte.Da < oraFeed
                                                                                      && oraFeed <= regoleLineeScoperte.A.GetValueOrDefault(oraFeed)
                                                                                select regoleLineeScoperte;

                            IEnumerable<LineaMonitorata> lineeMonitorate = from regola in regoleApplicabili
                                                                           join vettura in vettureSuLinea on regola.Linea equals vettura.Linea into lrs
                                                                           from lr in lrs.DefaultIfEmpty()
                                                                           select new LineaMonitorata(
                                                                               OraPrimaViolazione: null,
                                                                               OraUltimaViolazione: null,
                                                                               RegolaViolata: regola,
                                                                               VettureRilevate: lr?.count ?? 0
                                                                               );

                            foreach (var lineaMonitorata in lineeMonitorate)
                            {
                                LineaMonitorata esiste = ElencoLineeMonitorate.Where(x => x.Linea == lineaMonitorata.Linea && x.OraUltimaViolazione.GetValueOrDefault() == lineaMonitorata.OraUltimaViolazione.GetValueOrDefault()).FirstOrDefault();

                                if (lineaMonitorata.VettureRilevate < lineaMonitorata.VetturePreviste)
                                {
                                    lineaMonitorata.OraPrimaViolazione = dataFeedVehicle;
                                }
                                else
                                {
                                    if (esiste != null && esiste.OraPrimaViolazione.HasValue)
                                    {
                                        esiste.OraUltimaViolazione = dataFeedVehicle;
                                    }
                                }

                                LineaMonitorata presente = ElencoLineeMonitorate.Where(x => x.Linea == lineaMonitorata.Linea && x.OraUltimaViolazione.GetValueOrDefault() == lineaMonitorata.OraUltimaViolazione.GetValueOrDefault()).FirstOrDefault();
                                if (presente != null)
                                {
                                    ElencoLineeMonitorate.Remove(presente);
                                    lineaMonitorata.OraPrimaViolazione = presente.OraPrimaViolazione ?? lineaMonitorata.OraPrimaViolazione;
                                }
                                ElencoLineeMonitorate.Add(lineaMonitorata);
                            }


                            List<LineaMonitorata> situazioneAttualeEdAlert = ElencoLineeMonitorate
                                .Where(riga => !riga.OraUltimaViolazione.HasValue
                                            || (riga.OraUltimaViolazione.GetValueOrDefault(LastDataFeedVehicle.GetValueOrDefault()) - riga.OraPrimaViolazione.GetValueOrDefault(DateTime.MinValue)).TotalMinutes > riga.TempoBonus)
                                .ToList()
                                ;

                            dataGridViolazioni.DataSource = situazioneAttualeEdAlert;
                            Colora();
                        }

                        foreach (var alert in AlertsDaControllare)
                        {
                            IEnumerable<RegolaAlert> regoleAlertApplicabili = from regoleAlert in alert.RegoleAlert //gruppoRegoleAlert
                                                                              where regoleAlert.Giorno.Contains(giornosettimana.ToString())
                                                                                    && regoleAlert.Da < oraFeed
                                                                                    && oraFeed <= regoleAlert.A.GetValueOrDefault(oraFeed)
                                                                              select regoleAlert;
                            IEnumerable<string> lineedaVerificareAlert = regoleAlertApplicabili
                                                                            .GroupBy(test => test.Linea)
                                                                            .Select(grp => grp.First().Linea);
                            List<ViolazioneAlert> violazioniAlertAttuali = new List<ViolazioneAlert>();
                            
                            foreach (string linea in lineedaVerificareAlert)
                            {

                                violazioniAlertAttuali = (
                                    from vettura in elencoVetture
                                    join regolaAlert in regoleAlertApplicabili on vettura.Linea equals regolaAlert.Linea
                                        into VetturaRegolaAlert
                                    from vetturaRegolaAlert in VetturaRegolaAlert
                                    where vetturaRegolaAlert.VetturaDa <= int.Parse(vettura.Matricola)
                                                && int.Parse(vettura.Matricola) <= vetturaRegolaAlert.VetturaA.GetValueOrDefault(vetturaRegolaAlert.VetturaDa.GetValueOrDefault(9999))
                                    select new ViolazioneAlert(dataFeedVehicle, null, vetturaRegolaAlert, vettura.Matricola)
                                    ).ToList();

                                List<ViolazioneAlert> violazioniLineaStar = (
                                    from vettura in elencoVetture
                                    join regolaAlert in regoleAlertApplicabili on "*" equals regolaAlert.Linea
                                        into VetturaRegolaAlert
                                    from vetturaRegolaAlert in VetturaRegolaAlert
                                    where vetturaRegolaAlert.VetturaDa <= int.Parse(vettura.Matricola)
                                                && int.Parse(vettura.Matricola) <= vetturaRegolaAlert.VetturaA.GetValueOrDefault(vetturaRegolaAlert.VetturaDa.GetValueOrDefault(9999))
                                    select new ViolazioneAlert(
                                            dataFeedVehicle,
                                            null,
                                            new RegolaAlert(vettura.Linea, vetturaRegolaAlert.Giorno, vetturaRegolaAlert.Da, vetturaRegolaAlert.Da
                                            , vetturaRegolaAlert.VetturaDa, vetturaRegolaAlert.VetturaA),
                                            vettura.Matricola)
                                    ).ToList();

                                violazioniAlertAttuali = violazioniAlertAttuali.Union(violazioniLineaStar).ToList();

                                if (radioLineaRegola.Enabled && radioLineaRegola.Checked)
                                {
                                    violazioniAlertAttuali = violazioniAlertAttuali
                                        .GroupBy(x => new { x.Linea, x.Giorno, x.Da, x.A, x.VetturaDa, x.VetturaA })
                                        .Select(group =>
                                            new ViolazioneAlert(
                                                dataFeedVehicle,
                                                null,
                                                new RegolaAlert(group.Key.Linea,
                                                                group.Key.Giorno,
                                                                group.Key.Da,
                                                                group.Key.A,
                                                                group.Key.VetturaDa,
                                                                group.Key.VetturaA),
                                                string.Join(", ", group.Select(bn => bn.Violazione).ToList())
                                            )
                                        )
                                        .ToList();
                                }
                                else if (radioLinea.Enabled && radioLinea.Checked)
                                {
                                    violazioniAlertAttuali = violazioniAlertAttuali
                                        .GroupBy(x => x.Linea)
                                        .Select(group =>
                                            new ViolazioneAlert(
                                                dataFeedVehicle,
                                                null,
                                                group.Key,
                                                string.Join(", ", group.Select(bn => bn.Violazione).ToList())
                                            )
                                        )
                                        .ToList();
                                }

                                foreach (var violazione in violazioniAlertAttuali)
                                {
                                    if (radioNonRaggruppare.Checked)
                                    {
                                        ViolazioneAlert esisteNonRaggruppata = alert.ViolazioniAlert
                                            .Where(x => x.Linea == violazione.Linea
                                                     && x.Giorno == violazione.Giorno
                                                     && x.Da == violazione.Da
                                                     && x.A == violazione.A
                                                     && x.VetturaDa == violazione.VetturaDa
                                                     && x.VetturaA == violazione.VetturaA
                                                     && x.Violazione == violazione.Violazione)
                                            .FirstOrDefault();

                                        if (esisteNonRaggruppata == null)
                                            alert.ViolazioniAlert.Add(violazione);
                                    }
                                    else if (radioLineaRegola.Checked)
                                    {
                                        ViolazioneAlert esisteLineaRegola = alert.ViolazioniAlert
                                                .Where(x => x.Linea == violazione.Linea
                                                         && x.Giorno == violazione.Giorno
                                                         && x.Da == violazione.Da
                                                         && x.A == violazione.A
                                                         && x.VetturaDa == violazione.VetturaDa
                                                         && x.VetturaA == violazione.VetturaA)
                                                .FirstOrDefault();
                                        if (esisteLineaRegola == null)
                                            alert.ViolazioniAlert.Add(violazione);
                                        else
                                        {
                                            List<string> vettureEsistenti = esisteLineaRegola.Violazione.Replace(" ", "").Split(',').ToList();
                                            List<string> vettureNuove = violazione.Violazione.Split(',').ToList();
                                            List<string> vettureAggiornate = vettureEsistenti
                                                .Union(vettureNuove)
                                                .OrderBy(q => q.Length)
                                                .ThenBy(q => q)
                                                .ToList();
                                            esisteLineaRegola.Violazione = string.Join(", ", vettureAggiornate);
                                        }
                                    }
                                    else
                                    {
                                        ViolazioneAlert esiste = alert.ViolazioniAlert
                                        .Where(x => x.Linea == violazione.Linea && string.IsNullOrEmpty(x.Violazione))
                                        .FirstOrDefault();

                                        if (esiste == null)
                                        {
                                            alert.ViolazioniAlert.Add(violazione);
                                        }
                                        else
                                        {
                                            List<string> vettureEsistenti = esiste.Violazione.Replace(" ", "").Split(',').ToList();
                                            List<string> vettureNuove = violazione.Violazione.Split(',').ToList();
                                            List<string> vettureAggiornate = vettureEsistenti
                                                                                .Union(vettureNuove)
                                                                                .OrderBy(q => q.Length)
                                                                                .ThenBy(q => q)
                                                                                .ToList();
                                            esiste.Violazione = string.Join(", ", vettureAggiornate);
                                        }
                                    }
                                }
                            }
                            if (checkBoxStorico.Checked)
                                alert.Griglia.DataSource = alert.ViolazioniAlert.ToList();
                            else
                                alert.Griglia.DataSource = violazioniAlertAttuali.ToList();
                        }


                        #region controllo sovraffollamento

                        var listaBusPieni = elencoVetture
                            .Where(x => x.OccupancyStatus >= VehiclePosition.OccupancyStatus.Full)
                            .Select(x => new RunTimeValueAlert(
                                x.TripId,
                                x.Linea,
                                x.Matricola,
                                x.OccupancyStatus,
                                x.CurrentStopSequence,
                                x.PrimaVolta,
                                x.CurrentStopSequence,
                                x.UltimaVolta
                                )
                            )
                            .ToList();;

                        foreach (RunTimeValueAlert busPieno in listaBusPieni)
                        {

                            RunTimeValueAlert presente = ElencoVettureSovraffollate
                                .Where(x => x.TripID == busPieno.TripID
                                        && x.Matricola == busPieno.Matricola 
                                        && (busPieno.PrimaFermata - x.UltimaFermata <= 1)
                                        )                                
                                .FirstOrDefault();
                            if (presente != null)
                            {
                                ElencoVettureSovraffollate.Remove(presente);
                                busPieno.PrimaVolta = presente.PrimaVolta;
                                busPieno.PrimaFermata = presente.PrimaFermata;
                            }
                            ElencoVettureSovraffollate.Add(busPieno);
                        }
                        #endregion

                        await ExportGrid();
                        
                        ElencoPrecedente = elencoVetture;
                        LastDataFeedVehicle = dataFeedVehicle;
                    }

                    AggiornaScottPlot();

                    if (!string.IsNullOrEmpty(urlTrip.Text) && checkFeedTrip.Checked)
                    {
                        req = HttpWebRequest.Create(urlTrip.Text);
                        FeedMessage feedTrip = Serializer.Deserialize<FeedMessage>(req.GetResponse().GetResponseStream());
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

                        List<FeedEntity> soloVehicle = feedEntities
                            .Where(vehicle => !feedTrip.Entities.Any(trip => vehicle.Vehicle.Vehicle.Label == trip.TripUpdate.Vehicle.Label))
                            .ToList();


                        List<FeedEntity> soloTrip = feedTrip.Entities
                            .Where(trip => !feedEntities.Any(vehicle => vehicle.Vehicle.Vehicle.Label == trip.TripUpdate.Vehicle.Label))
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

                        textBox1.AppendText($"Vetture rilevate solo sul feed Trip: {soloTrip.Count}" + Environment.NewLine);
                        textBox1.AppendText($"Vetture rilevate solo sul feed Vehicle: {soloVehicle.Count}" + Environment.NewLine);
                    }

                }
                else {
                    string msg= $"Il feed letto dal server di RSM alle {DateTime.Now:HH:mm:ss} {Environment.NewLine}è stato scartato in quanto ha il timestamp delle {LastDataFeedVehicle.GetValueOrDefault():HH:mm:ss} ed è stato acquisito in precedenza";
                    textBox1.Text = msg;
                    Log.Error(msg);
                }
            }
            catch (WebException ex) when ((ex.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.NotFound)
            {
                textBox1.AppendText($"{ex.Message}: {Environment.NewLine}Feed Non trovato al seguente indirizzo");
                textBox1.AppendText($"{Environment.NewLine}{ex.Response.ResponseUri}{Environment.NewLine}");                
                Log.Error(ex, "Feed {UrlFeed} Non trovato ", ex.Response.ResponseUri);                
            }
            catch (WebException ex) when (ex.Status == WebExceptionStatus.Timeout)
            {
                textBox1.AppendText($"{ex.Message} Problemi di connessione con il server di RSM{Environment.NewLine}");
                Log.Error(ex, "Errore Connessione Server {UrlRemoto}", req.RequestUri);

            }
            catch (Exception ex)
            {
                textBox1.AppendText($"{ex.Message}");
                Log.Error(ex, "Errore Generico");
            }        
        }

        private void RestartFile()
        {
            LastDataFeedVehicle = null;
            FirstDataFeedVehicle = null;
            fileName = string.Empty;

            ElencoLineeMonitorate.Clear();
            ElencoAggregatoVetture.Clear();
            ElencoPrecedente.Clear();
            AnomaliaGTFS.Clear();
            ElencoVettureGrafico.Clear();

            dataGridViolazioni.Invalidate();
            dataGridViolazioni.DataSource = null;

            dataGridVetture.Invalidate();
            dataGridVetture.DataSource = null;

            formsPlotTPL.Reset();
            formsPlotAtac.Reset();

            labelFeedLetti.Text = "0";
            labelTotaleRighe.Text = ElencoAggregatoVetture.Count.ToString();
            labelTotaleIdVettura.Text = "0";
            labelTotaleMatricola.Text = "0";
            labelTotaleMatricolaATAC.Text = "0";
            labelTotaleMatricolaTPL.Text = "0";
            labelFerro.Text = "0";
            labelBUS.Text = "0";
            labelTRAM.Text = "0";
            labelFILOBUS.Text = "0";
            labelMiniBusEle.Text =  "0";
            labelFurgoncino.Text = "0";


            lblOraLettura.Text = "--:--:--";
            labelAtac.Text = "0";
            labelTPL.Text = "0";
            //labelWait.Text = "0";
            labelTot.Text = "0";

            LeggiFileConfigurazione();
            LeggiRegoleAlertDaFile();

            foreach (var alert in AlertsDaControllare)
            {
                alert.ViolazioniAlert.Clear();
            }
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
                _ = AcquisizioneAsync();
            }
            if (!timerAcquisizione.Enabled && deltaMilliSec > 0)
            {
                _ = AcquisizioneAsync();
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
                if (deltaMilliSec>0)
                    Log.Information("Acquisizione in pausa");
            }
        }

        private void AggiornaScottPlot()
        {
            if (ElencoVettureGrafico.Count > 2)
            {
                Render(formsPlotAtac, formsPlotTPL);
            }
        }
        
        public void Render(FormsPlot pltATAC, FormsPlot pltTPL)
        {
            var culture = CultureInfo.CreateSpecificCulture("it");                        
            var tempo = (from elenco in ElencoVettureGrafico select elenco.DateTime.ToOADate()).ToArray();
            var serieAtac = (from elenco in ElencoVettureGrafico select (double)elenco.Atac).ToArray();
            var serieAggregateATAC = (from elenco in ElencoVettureGrafico select (double)(elenco.AggregateAtac)).ToArray();
            
            pltATAC.plt.Clear(); 
            pltATAC.plt.PlotSignalXY(tempo, serieAggregateATAC, markerSize: 5, color: Color.FromArgb(231, 109, 20), lineWidth: 4, label: "Aggregate");
            pltATAC.plt.PlotSignalXY(tempo, serieAtac, markerSize: 1, color: Color.FromArgb(137, 8, 39), lineWidth: 2, label: "Istantanee");            
            pltATAC.plt.SetCulture(culture);
            pltATAC.plt.Ticks(dateTimeX: true);
            pltATAC.plt.Ticks(useMultiplierNotation: false);
            pltATAC.plt.Legend(location: legendLocation.upperLeft);
            pltATAC.plt.Title("Monitoraggio vetture ATAC");
            pltATAC.plt.AxisAuto();
            pltATAC.Render();

            pltTPL.plt.Clear();
            var serieTPL = (from elenco in ElencoVettureGrafico select (double)elenco.TPL).ToArray();
            var serieAggregateTPL = (from elenco in ElencoVettureGrafico select (double)(elenco.AggregateTPL)).ToArray();
            pltTPL.plt.PlotSignalXY(tempo, serieAggregateTPL, markerSize: 5, color: Color.FromArgb(231, 109, 20), lineWidth: 4, maxRenderIndex: ElencoVettureGrafico.Count - 1, label: "Aggregate");
            pltTPL.plt.PlotSignalXY(tempo, serieTPL, markerSize: 1, color: Color.FromArgb(4, 65, 136), lineWidth: 2, maxRenderIndex: ElencoVettureGrafico.Count - 1, label: "Istantanee");            
            pltTPL.plt.SetCulture(culture);
            pltTPL.plt.Ticks(dateTimeX: true);
            pltTPL.plt.Ticks(useMultiplierNotation: false);
            pltTPL.plt.Legend(location: legendLocation.upperLeft);
            pltTPL.plt.Title("Monitoraggio vetture TPL");
            pltTPL.plt.AxisAuto();
            pltTPL.Render();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Properties.Settings settings = new Properties.Settings(); 
            
            ElencoAggregatoVetture = new List<ExtendedVehicleInfo>();
            ElencoPrecedente = new List<ExtendedVehicleInfo>();
            AnomaliaGTFS = new List<ErroriGTFS>();
            ElencoVettureGrafico = new List<MonitoraggioVettureGrafico>();
            
            ElencoLineeMonitorate = new List<LineaMonitorata>();
            AlertsDaControllare = new List<AlertDaControllare>();

            ElencoVettureSovraffollate = new List<RunTimeValueAlert>();

            CriteriMediaPonderata = new List<CriterioMediaPonderata>();

            Version actualVersion = Assembly.GetExecutingAssembly().GetName().Version;
            labelVer.Text = String.Format("Vers. {0}.{1:00}", actualVersion.Major, actualVersion.Minor);

            #region Load default settings
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
            int totalSeconds = Properties.Settings.Default.DeltaTSec;
            minuti.Value = totalSeconds / 60;
            secondi.Value = totalSeconds % 60;

            if (!Properties.Settings.Default.tabGrigliaOld)
                tabMainForm.TabPages.Remove(tabGriglia);

            #endregion

            LeggiFileConfigurazione();
            LeggiRegoleAlertDaFile();
        }
        
        private void LeggiFileConfigurazione() {
            
            RegoleMonitoraggio = new List<RegolaMonitoraggio>();

            var reader = new GTFSReader<GTFSFeed>();            
            staticData = reader.Read($"Config{Path.DirectorySeparatorChar}GTFS_Static");

            List<Route> elencoLinee = staticData.Routes
                .OrderBy(k => k.ShortName)
                .ToList();
            Route route = staticData.Routes.Take(1).FirstOrDefault();
            Route copia = new Route
            {
                Id = "-1",
                ShortName = "Tutte",
                AgencyId = route.AgencyId,
                LongName = route.LongName,
                Description = route.Description,
                Type = route.Type,
                Url = route.Url,
                Color = route.Color,
                TextColor = route.TextColor
            };

            elencoLinee.Insert(0, copia);
            comboBox1.DataSource = elencoLinee;
            comboBox1.ValueMember = "Id";
            comboBox1.DisplayMember = "ShortName";

            ElencoLineaAgenzia = (from linea in staticData.Routes
                                  join agenzia in staticData.Agencies on linea.AgencyId equals agenzia.Id
                                  select new LineaAgenzia(linea, agenzia)
                                 ).ToList();

            using (var readerDettagli = new StreamReader($"Config{Path.DirectorySeparatorChar}GTFS_Static{Path.DirectorySeparatorChar}DettagliVettura.csv"))
            using (var csv = new CsvReader(readerDettagli, CultureInfo.InvariantCulture))
            {
                csv.Configuration.Delimiter = ";";
                csv.Configuration.HeaderValidated = null;
                csv.Configuration.MissingFieldFound = null;
                csv.Configuration.TrimOptions = CsvHelper.Configuration.TrimOptions.Trim;
                csv.Configuration.PrepareHeaderForMatch = (string header, int index) => header.Trim();
                csv.Configuration.AllowComments = true;
                ElencoDettagliVettura = csv.GetRecords<DettagliVettura>().ToList();
            }
            try
            {
                using (var readerTempoBonus = new StreamReader($"Config{Path.DirectorySeparatorChar}MonitoraggioLinee{Path.DirectorySeparatorChar}RegoleMonitoraggio_yes.txt"))
                using (var csv = new CsvReader(readerTempoBonus, CultureInfo.InvariantCulture))
                {
                    csv.Configuration.Delimiter = ",";
                    csv.Configuration.HeaderValidated = null;
                    csv.Configuration.MissingFieldFound = null;
                    csv.Configuration.TrimOptions = CsvHelper.Configuration.TrimOptions.Trim;
                    csv.Configuration.PrepareHeaderForMatch = (string header, int index) => header.Trim();
                    csv.Configuration.AllowComments = true;                    
                    RegoleMonitoraggio = csv.GetRecords<RegolaMonitoraggio>().ToList();
                    dataGridViolazioni.DataSource = RegoleMonitoraggio;
                }
            }
            catch
            {
                if (tabMainForm.TabPages.Contains(tabMonitoraggio))
                {
                    tabMainForm.TabPages.Remove(tabMonitoraggio);
                    Log.Information("Rimosso tab RegoleMonitoraggio");
                }
            }
        }

        private void LeggiRegoleAlertDaFile()
        {
            string pathAlert = $"Config{Path.DirectorySeparatorChar}Regole_Alert";
            if (Directory.Exists(pathAlert))
            {
                string[] alertFiles = Directory.GetFiles(pathAlert, "*.txt");

                foreach (var alertDaControllare in AlertsDaControllare)
                {
                    var esisteFileAlert = alertFiles.Where(x => x == alertDaControllare.Griglia.Name).Any();
                    if (!esisteFileAlert)
                        alertDaControllare.RegoleAlert = new List<RegolaAlert>();
                }

                foreach (string item in alertFiles)
                {
                    try
                    {
                        using (var readerTempoBonus = new StreamReader(item))
                        using (var csv = new CsvReader(readerTempoBonus, CultureInfo.InvariantCulture))
                        {
                            csv.Configuration.Delimiter = ",";
                            csv.Configuration.HeaderValidated = null;
                            csv.Configuration.TrimOptions = CsvHelper.Configuration.TrimOptions.Trim;
                            csv.Configuration.MissingFieldFound = null;
                            csv.Configuration.RegisterClassMap<RegolaAlertMap>();
                            csv.Configuration.AllowComments = true;
                            csv.Configuration.PrepareHeaderForMatch = (string header, int index) => header.Trim();
                            List<RegolaAlert> listaRegole = csv.GetRecords<RegolaAlert>().ToList();
                            TabPage myNewTabItem = new TabPage
                            {
                                Text = Path.GetFileNameWithoutExtension(item)
                            };
                            DataGridView myNewdataGridVetture = new DataGridView();
                            List<ViolazioneAlert> violazioneAlert = new List<ViolazioneAlert>();

                            var alertGiaCensito = AlertsDaControllare.Where(x => x.Griglia.Name == Path.GetFileNameWithoutExtension(item)).FirstOrDefault();
                            if (alertGiaCensito != null)
                            {
                                alertGiaCensito.RegoleAlert = listaRegole;
                            }
                            else
                            {
                                AlertsDaControllare.Add(new AlertDaControllare { Griglia = myNewdataGridVetture, RegoleAlert = listaRegole, ViolazioniAlert = violazioneAlert });

                                myNewdataGridVetture.AllowUserToAddRows = false;
                                myNewdataGridVetture.AllowUserToDeleteRows = false;
                                myNewdataGridVetture.AllowUserToOrderColumns = true;
                                myNewdataGridVetture.AutoGenerateColumns = false;
                                myNewdataGridVetture.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                                myNewdataGridVetture.BackgroundColor = SystemColors.Control;
                                myNewdataGridVetture.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;

                                DataGridViewTextBoxColumn columnLinea = new DataGridViewTextBoxColumn();
                                DataGridViewTextBoxColumn columnGiorno = new DataGridViewTextBoxColumn();
                                DataGridViewTextBoxColumn columnDa = new DataGridViewTextBoxColumn();
                                DataGridViewTextBoxColumn columnA = new DataGridViewTextBoxColumn();
                                DataGridViewTextBoxColumn columnVetturaDa = new DataGridViewTextBoxColumn();
                                DataGridViewTextBoxColumn columnVetturaA = new DataGridViewTextBoxColumn();
                                DataGridViewTextBoxColumn columnVetturaSbagliata = new DataGridViewTextBoxColumn();
                                DataGridViewTextBoxColumn columnOraPrimaViolazione = new DataGridViewTextBoxColumn();
                                DataGridViewTextBoxColumn columnOraUltimaViolazione = new DataGridViewTextBoxColumn();

                                myNewdataGridVetture.Columns.AddRange(new DataGridViewColumn[] {
                                columnLinea
                                ,columnGiorno
                                ,columnDa
                                ,columnA
                                ,columnVetturaDa
                                ,columnVetturaA
                                ,columnVetturaSbagliata
                            });

                                DataGridViewCellStyle timeFormat = new DataGridViewCellStyle
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
                                myNewdataGridVetture.Name = Path.GetFileNameWithoutExtension(item);
                                myNewdataGridVetture.ReadOnly = true;

                                myNewTabItem.Controls.Add(myNewdataGridVetture);
                                tabMainForm.TabPages.Add(myNewTabItem);                                
                            }
                        }
                    }
                    catch (Exception exc)
                    {
                        textBox1.AppendText($"{exc.Message} {Environment.NewLine} {exc.StackTrace}");
                    }
                }

                foreach (var alert in AlertsDaControllare)
                {
                    alert.Griglia.Invalidate();
                    alert.Griglia.DataSource = null;
                    alert.Griglia.DataSource = alert.RegoleAlert;
                }

                if (alertFiles.Length > 0)
                {
                    labelRaggruppaAlert.Visible = true;
                    radioLinea.Visible = true;
                    radioLineaRegola.Visible = true;
                    radioNonRaggruppare.Visible = true;
                    checkAlert.Visible = true;
                    checkBoxStorico.Visible = true;
                    //buttonResetRegole.Visible = true;
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
                //buttonResetRegole.Visible = false;
            }

            FileInfo fileMediaPonderata = new FileInfo($"Config{Path.DirectorySeparatorChar}CriterioMediaPonderata.txt");
            if (fileMediaPonderata.Exists)
            {
                using (var readerMediaPonderata = new StreamReader(fileMediaPonderata.FullName))
                using (var csvMediaPonderata = new CsvReader(readerMediaPonderata, CultureInfo.InvariantCulture))
                {
                    csvMediaPonderata.Configuration.Delimiter = ",";
                    csvMediaPonderata.Configuration.HeaderValidated = null;
                    csvMediaPonderata.Configuration.TrimOptions = CsvHelper.Configuration.TrimOptions.Trim;
                    csvMediaPonderata.Configuration.PrepareHeaderForMatch = (string header, int index) => header.Trim();
                    csvMediaPonderata.Configuration.AllowComments = true;
                    csvMediaPonderata.Configuration.MissingFieldFound = null;
                    CriteriMediaPonderata = csvMediaPonderata.GetRecords<CriterioMediaPonderata>().ToList();
                    
                    if (CriteriMediaPonderata.Sum(x => x.Peso) != 1)
                    {

                        MessageBox.Show(text:"La somma dei pesi dei campioni deve essere 1", caption:"Attenzione", buttons:MessageBoxButtons.OK, icon:MessageBoxIcon.Error);
                    }
                }
            }
            else {
            
            }
        }

        private void TimerAcquisizione_Tick(object sender, EventArgs e)
        {
            _ = AcquisizioneAsync();
        }

        private void SalvaImpostazioni(object sender, EventArgs e)
        {
            Properties.Settings.Default.UrlVehicle = urlVehicle.Text;
            Properties.Settings.Default.UrlTrip = urlTrip.Text;
            Properties.Settings.Default.UrlAlert= urlAlert.Text;
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
            Properties.Settings.Default.Save();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (timerAcquisizione.Enabled)
            {
                DialogResult dialog = MessageBox.Show($"Interrompere il monitoraggio {(needToRestart? "e riavviare" : "ed uscire")} ? ",
                                $"Conferma {(needToRestart? "Riavvio" : "Uscita")}",
                                 MessageBoxButtons.YesNo,
                                 MessageBoxIcon.Question);
                if (dialog == DialogResult.No)
                {
                    e.Cancel = true;
                    needToRestart = false;
                }
                else if (!string.IsNullOrEmpty(fileName))
                {
                    _ = ExportGrid();
                }
            }
        }

        private async Task<bool> SaveAs(FileInfo outputFile) {
            bool retVal = true;
            if (checkXlsx.Checked)
            {
                using (ExcelPackage excel = new ExcelPackage(outputFile))
                    try
                    {
                        ExcelWorksheet workSheet;
                        foreach (ExcelWorksheet sheet in excel.Workbook.Worksheets)
                        {
                            if (sheet.Name == "Feed")
                            {
                                excel.Workbook.Worksheets.Delete("Feed");
                                break;
                            }
                        }

                        workSheet = excel.Workbook.Worksheets.Add("Feed");
                        List<string> Ammessi = new List<string> { "IdVettura", "Matricola" };
                        PropertyInfo[] membersToInclude = typeof(ExtendedVehicleInfo)
                            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                            .Where(p => !Attribute.IsDefined(p, typeof(IgnoreAttribute))
                            // && Ammessi.Contains(p.Name)                        
                            )
                            .ToArray();

                        ExcelRangeBase range = workSheet.Cells[1, 1].LoadFromCollection(
                            ElencoAggregatoVetture
                            , true
                            , TableStyles.Medium2
                            , BindingFlags.Public | BindingFlags.Instance
                            , membersToInclude);

                        int colNumber = 1;

                        foreach (PropertyInfo exportedProperty in membersToInclude)
                        {
                            if (exportedProperty.PropertyType == typeof(DateTime) || exportedProperty.PropertyType == typeof(DateTime?))
                            {
                                workSheet.Column(colNumber).Style.Numberformat.Format = "MM/dd/yyyy HH:mm:ss";
                            }
                            colNumber++;
                        }
                        workSheet.Cells.AutoFitColumns();

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

                            workSheet = excel.Workbook.Worksheets.Add("Grafico");
                            membersToInclude = typeof(MonitoraggioVettureGrafico)
                                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                .Where(p => !Attribute.IsDefined(p, typeof(IgnoreAttribute)))
                                .ToArray();

                            colNumber = 4;
                            range = workSheet.Cells[32, colNumber].LoadFromCollection(
                                ElencoVettureGrafico
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
                            lineChartATAC.Title.Text = $"Vetture Rilevate ATAC {FirstDataFeedVehicle:dd-MM-yyyy [HH:mm:ss}-{LastDataFeedVehicle:HH:mm:ss}] ";
                            lineChartTPL.Title.Text = $"Vetture Rilevate  TPL {FirstDataFeedVehicle:dd-MM-yyyy [HH:mm:ss}-{LastDataFeedVehicle:HH:mm:ss}] ";
                            ExcelRangeBase rangeLabel = range.Offset(1, 0, ElencoVettureGrafico.Count, 1);
                            ExcelRangeBase range1 = range.Offset(1, 2, ElencoVettureGrafico.Count, 1);
                            ExcelRangeBase range2 = range.Offset(1, 4, ElencoVettureGrafico.Count, 1);
                            ExcelRangeBase range3 = range.Offset(1, 3, ElencoVettureGrafico.Count, 1);
                            ExcelRangeBase range4 = range.Offset(1, 5, ElencoVettureGrafico.Count, 1);

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
                            foreach (ExcelWorksheet sheet in excel.Workbook.Worksheets)
                            {
                                if (sheet.Name == "Monitoraggio Linee")
                                {
                                    excel.Workbook.Worksheets.Delete("Monitoraggio Linee");
                                    break;
                                }
                            }
                            workSheet = excel.Workbook.Worksheets.Add("Monitoraggio Linee");

                            membersToInclude = typeof(LineaMonitorata)
                                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                .Where(p => !Attribute.IsDefined(p, typeof(IgnoreAttribute)))
                                .ToArray();


                            List<LineaMonitorata> violazioniLineaMonitorata = ElencoLineeMonitorate.Where(x => x.OraPrimaViolazione.HasValue).ToList();

                            range = workSheet.Cells[1, 1].LoadFromCollection(
                                violazioniLineaMonitorata
                                , true
                                , TableStyles.Medium2
                                , BindingFlags.Public | BindingFlags.Instance
                                , membersToInclude);
                            colNumber = 1;
                            foreach (PropertyInfo exportedProperty in membersToInclude)
                            {
                                if (exportedProperty.PropertyType == typeof(TimeSpan) || exportedProperty.PropertyType == typeof(TimeSpan?) || exportedProperty.PropertyType == typeof(DateTime?))
                                {
                                    workSheet.Column(colNumber).Style.Numberformat.Format = "HH:mm:ss";
                                }
                                colNumber++;
                            }

                            workSheet.Cells.AutoFitColumns();
                        }

                        if (checkAlert.Checked && checkAlert.Enabled)
                        {
                            foreach (AlertDaControllare alertDaControllare in AlertsDaControllare)
                            {
                                string excelSheetName = alertDaControllare.Griglia.Name;
                                foreach (ExcelWorksheet sheet in excel.Workbook.Worksheets)
                                {
                                    if (sheet.Name == excelSheetName)
                                    {
                                        excel.Workbook.Worksheets.Delete(excelSheetName);
                                        break;
                                    }
                                }
                                workSheet = excel.Workbook.Worksheets.Add(excelSheetName);

                                membersToInclude = typeof(ViolazioneAlert)
                                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                    .Where(p => !Attribute.IsDefined(p, typeof(IgnoreAttribute)))
                                    .ToArray();

                                List<ViolazioneAlert> violazioniAlert = alertDaControllare.ViolazioniAlert;

                                range = workSheet.Cells[1, 1].LoadFromCollection(
                                    violazioniAlert
                                    , true
                                    , TableStyles.Medium2
                                    , BindingFlags.Public | BindingFlags.Instance
                                    , membersToInclude);
                                colNumber = 1;
                                foreach (PropertyInfo exportedProperty in membersToInclude)
                                {
                                    if (exportedProperty.PropertyType == typeof(TimeSpan) || exportedProperty.PropertyType == typeof(TimeSpan?) || exportedProperty.PropertyType == typeof(DateTime?))
                                    {
                                        workSheet.Column(colNumber).Style.Numberformat.Format = "HH:mm:ss";
                                    }
                                    colNumber++;
                                }

                                workSheet.Cells.AutoFitColumns();
                            }
                        }




                        if (checkAnomalieGTFS.Checked)
                        {
                            string excelSheetName = "AnomalieGTFS";
                            foreach (ExcelWorksheet sheet in excel.Workbook.Worksheets)
                            {
                                if (sheet.Name == excelSheetName)
                                {
                                    excel.Workbook.Worksheets.Delete(excelSheetName);
                                    break;
                                }
                            }
                            workSheet = excel.Workbook.Worksheets.Add(excelSheetName);


                            Ammessi = new List<string> { "Matricola", "Linea", "PrimaVolta", "TripId", "CurrentStopSequence", "Delta" };
                            membersToInclude = typeof(ErroriGTFS)
                                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                .Where(p => !Attribute.IsDefined(p, typeof(IgnoreAttribute))
                                 && Ammessi.Contains(p.Name)
                                )
                                .ToArray();

                            range = workSheet.Cells[1, 1].LoadFromCollection(
                                AnomaliaGTFS
                                , true
                                , TableStyles.Medium2
                                , BindingFlags.Public | BindingFlags.Instance
                                , membersToInclude);

                            colNumber = 1;

                            foreach (PropertyInfo exportedProperty in membersToInclude)
                            {
                                if (exportedProperty.PropertyType == typeof(DateTime) || exportedProperty.PropertyType == typeof(DateTime?))
                                {
                                    workSheet.Column(colNumber).Style.Numberformat.Format = "MM/dd/yyyy HH:mm:ss";
                                }
                                colNumber++;
                            }
                            workSheet.Cells.AutoFitColumns();
                        }

                        if (checkSovraffollamento.Checked)
                        {
                            string excelSheetName = "Sovraffollamneto";
                            foreach (ExcelWorksheet sheet in excel.Workbook.Worksheets)
                            {
                                if (sheet.Name == excelSheetName)
                                {
                                    excel.Workbook.Worksheets.Delete(excelSheetName);
                                    break;
                                }
                            }
                            workSheet = excel.Workbook.Worksheets.Add(excelSheetName);


                            //Ammessi = new List<string> { "Matricola", "Linea", "PrimaVolta", "TripId", "CurrentStopSequence", "Delta" };
                            membersToInclude = typeof(RunTimeValueAlert)
                                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                .Where(p => !Attribute.IsDefined(p, typeof(IgnoreAttribute))
                                //&& Ammessi.Contains(p.Name)
                                )
                                .ToArray();

                            range = workSheet.Cells[1, 1].LoadFromCollection(
                                ElencoVettureSovraffollate
                                , true
                                , TableStyles.Medium2
                                , BindingFlags.Public | BindingFlags.Instance
                                , membersToInclude);

                            colNumber = 1;

                            foreach (PropertyInfo exportedProperty in membersToInclude)
                            {
                                if (exportedProperty.PropertyType == typeof(DateTime) || exportedProperty.PropertyType == typeof(DateTime?))
                                {
                                    workSheet.Column(colNumber).Style.Numberformat.Format = "MM/dd/yyyy HH:mm:ss";
                                }
                                colNumber++;
                            }
                            workSheet.Cells.AutoFitColumns();
                        }

                        excel.Save();
                    }
                    catch (Exception exc)
                    {
                        retVal = false;
                        Log.Error("{Exception}", exc);
                    }
            }

            if (checkCSV.Checked)
            {
                using (var writer = new StreamWriter($"OUTPUT{Path.DirectorySeparatorChar}{fileName}.csv"))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.Configuration.Delimiter = ";";
                    await csv.WriteRecordsAsync(ElencoAggregatoVetture);
                }
            }
            return retVal;
        }

        private async Task ExportGrid()
        {
            try
            {
                await Task.Run(async () =>
                {
                    FileInfo file = new FileInfo($"OUTPUT{Path.DirectorySeparatorChar}{fileName}.xlsx");
                    FileInfo altFileName = new FileInfo($"OUTPUT{Path.DirectorySeparatorChar}{fileName}.xlsx.bck");
                    bool esito = await SaveAs(file);
                    if (!esito)
                    {
                        _ = await SaveAs(altFileName);
                    }
                    else if (altFileName.Exists)
                        altFileName.Delete();
                });
            }
            catch (Exception e)
            {
                Log.Error("{Exception}", e);
            }


        }

        private void Random(object sender, EventArgs e)
        {
            DateTime t0;
            if (ElencoVettureGrafico.Count == 0)
                t0 = System.DateTime.Now;
            else
                t0 = ElencoVettureGrafico[ElencoVettureGrafico.Count - 1].DateTime;
            if (ElencoVettureGrafico.Count < 100000)
            {
                Random rand = new Random(0);
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
                    MonitoraggioVettureGrafico nuovoMonitoraggio = new MonitoraggioVettureGrafico
                    {
                        DateTime = t0.AddSeconds(i * 30),
                        Aggregate = aggregate,
                        Rilevate = rilevate,
                        Atac = atac,
                        TPL = tpl,
                        Aggiunte = aggiunte,
                        Tolte = tolte
                    };
                    ElencoVettureGrafico.Add(nuovoMonitoraggio);
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

        public void Colora() {
            dataGridViolazioni.SuspendLayout();
            IEnumerable<LineaMonitorata> dataSource = (IEnumerable<LineaMonitorata>)dataGridViolazioni.DataSource;
            LineaMonitorata riga;
            for (int i = 0; i < dataGridViolazioni.RowCount; i++)
            {
                DataGridViewRow row = dataGridViolazioni.Rows[i];
                riga = dataSource.ElementAt(i);
                if (riga.VettureRilevate < riga.VetturePreviste)
                {
                    TimeSpan span = riga.OraUltimaViolazione.GetValueOrDefault(LastDataFeedVehicle.GetValueOrDefault()) - riga.OraPrimaViolazione.GetValueOrDefault(DateTime.MinValue);
                    
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
            string tabSelezoinato = (sender as TabControl).SelectedTab.Name;
            if (tabSelezoinato == "tabMonitoraggio" && LastDataFeedVehicle.HasValue)
            {
                Colora();
            }
        }

        private void ResetAcquisizione(object sender, EventArgs e)
        {
            // Application.Restart();
            needToRestart = true;            
            this.Close();

            /*
            LastdataFeedVehicle = null;
            FirstdataFeedVehicle = null;
            fileName = string.Empty;

            ElencoLineeMonitorate.Clear();
            ElencoAggregatoVetture.Clear();
            ElencoPrecedente.Clear();
            ElencoVettureGrafico.Clear();

            dataGridViolazioni.Invalidate();
            dataGridViolazioni.DataSource = null;

            dataGridVetture.Invalidate(); 
            dataGridVetture.DataSource = null;

            formsPlotTPL.Reset();
            formsPlotAtac.Reset();

            labelFeedLetti.Text = "0";
            labelTotaleRighe.Text = ElencoAggregatoVetture.Count.ToString();
            labelTotaleIdVettura.Text = "0";
            labelTotaleMatricola.Text = "0";
            labelTotaleMatricolaATAC.Text = "0";
            labelTotaleMatricolaTPL.Text  = "0";

            lblOraLettura.Text = "--:--:--";
            labelAtac.Text = "0";
            labelTPL.Text = "0";
            //labelWait.Text = "0";
            labelTot.Text = "0";

            LeggiFileConfigurazione();
            LeggiRegoleAlertDaFile();

            foreach (var alert in AlertsDaControllare)
            {
                alert.ViolazioniAlert.Clear();
            }
            */
        }

        private void CheckFeedTrip_CheckedChanged(object sender, EventArgs e)
        {
            urlTrip.Enabled = checkFeedTrip.Checked;
        }

        private void RileggiRegole(object sender, EventArgs e)
        {
            LeggiRegoleAlertDaFile();
        }

        private void CheckReset_CheckedChanged(object sender, EventArgs e)
        {
            if (checkReset.Checked)
            {
                TimeSpan oraReset = dateTimeReset.Value.AddSeconds(01).TimeOfDay;
                DateTime now = DateTime.Now;
                if (now.TimeOfDay > oraReset)
                    now = now.AddDays(1);
                DataResetMonitoraggio = new DateTime(now.Year, now.Month, now.Day, oraReset.Hours, oraReset.Minutes, oraReset.Seconds);
            }
            else
                DataResetMonitoraggio = null;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            UpdateBox updateBox = new UpdateBox();
            var result = updateBox.ShowDialog();
            if (result == DialogResult.Yes) {
                //Application.Restart();
                needToRestart = true;
                Close();
            }
            else
                needToRestart = false;
        }

        private void AdvancedDataGridView1_SortStringChanged(object sender, Zuby.ADGV.AdvancedDataGridView.SortEventArgs e)
        {
            extendedVehicleInfoBindingSource.Sort = advancedDataGridView1.SortString;
        }

        private void AdvancedDataGridView1_FilterStringChanged(object sender, Zuby.ADGV.AdvancedDataGridView.FilterEventArgs e)
        {
            extendedVehicleInfoBindingSource.Filter = advancedDataGridView1.FilterString;
        }
    }
}
