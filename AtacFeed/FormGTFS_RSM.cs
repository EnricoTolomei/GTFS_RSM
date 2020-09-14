using CsvHelper;
using CsvHelper.Configuration.Attributes;
using GTFS;
using GTFS.Entities;
using GTFS.IO;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Table;
using ProtoBuf;
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
using TransitRealtime;

namespace AtacFeed
{
    public partial class FormGTFS_RSM : Form
    {
        private static readonly DateTime t0 = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        private List<ExtendedVehicleInfo> ElencoAggregatoVetture;
        private List<ExtendedVehicleInfo> ElencoPrecedente;
        private List<LineaAgenzia> ElencoLineaAgenzia;
        private List<DettagliVettura> ElencoDettagliVettura;
        private List<MonitoraggioVettureGrafico> ElencoVettureGrafico;
        
        private List<RegolaMonitoraggio> RegoleMonitoraggio;
        private List<LineaMonitorata> ElencoLineeMonitorate;        

        private List<AlertDaControllare> AlertsDaControllare;

        private string fileName;
        private DateTime? LastdataFeedVehicle;
        private DateTime? FirstdataFeedVehicle;
        private DateTime? DataResetMonitoraggio;

        public FormGTFS_RSM()
        {
            InitializeComponent();
        }

        private GTFSFeed staticData = new GTFSFeed();

        private void Acquisizione()
        {
            try
            {
                
                string routeID = comboBox1.SelectedValue.ToString();
                WebRequest req = HttpWebRequest.Create(urlVehicle.Text);
                req.Timeout = 10000;
                FeedMessage feedVehicleCompleto = Serializer.Deserialize<FeedMessage>(req.GetResponse().GetResponseStream());
                if (feedVehicleCompleto.Entities.Count == 0)
                    throw new Exception( "Il feed letto è vuoto");
                DateTime dataFeedVehicle = t0.AddSeconds(feedVehicleCompleto.Header.Timestamp).ToLocalTime();
                //dataFeedVehicle = DateTime.Now;

                //dataFeedVehicle = dataFeedVehicle.AddHours(4);

                if (dataFeedVehicle > DataResetMonitoraggio.GetValueOrDefault() )
                {
                    ResetAcquisizione(null, null);
                    DataResetMonitoraggio.Value.AddDays(1);
                }
                
                if (LastdataFeedVehicle.GetValueOrDefault() != dataFeedVehicle)
                {
                    LastdataFeedVehicle = dataFeedVehicle;
                    textBox1.Text = string.Empty;
                    textBox2.Text = string.Empty;
                    lblOraLettura.Text = $"{ dataFeedVehicle: HH:mm:ss}";
                    labelFeedLetti.Text = (int.Parse(labelFeedLetti.Text) + 1).ToString();
                    List<FeedEntity> feedEntities = feedVehicleCompleto.Entities
                        .Where(x => (int.Parse(routeID) < 0 || x.Vehicle.Trip?.RouteId == routeID)
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
                                        x.DettagliVettura?.Gestore ?? ((x.o2.Vehicle.Vehicle.Id.Length > 4) ? "tpl" : "atac"),
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
                                        x.o2.Vehicle.CurrentStatus
                                    )
                        )
                        .Distinct()
                        .OrderBy(x => x.IdVettura)
                        .ToList();

                    List<ExtendedVehicleInfo> vettureTolte = ElencoPrecedente.Except(elencoVetture).ToList();
                    List<ExtendedVehicleInfo> vettureAggiunte = elencoVetture.Except(ElencoPrecedente).ToList();

                    var vettureAggiunteTolte =
                        from tolte in vettureTolte
                        join aggiunte in vettureAggiunte on tolte.IdVettura equals aggiunte.IdVettura
                        select new { tolte.IdVettura };

                    vettureTolte = vettureTolte.Where(x => !vettureAggiunteTolte.Any(c => c.IdVettura == x.IdVettura)).ToList();
                    vettureAggiunte = vettureAggiunte.Where(x => !vettureAggiunteTolte.Any(c => c.IdVettura == x.IdVettura)).ToList();

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
                    }

                    foreach (ExtendedVehicleInfo vettura in elencoVetture)
                    {
                        var presente = ElencoAggregatoVetture.FirstOrDefault(x => x.IdVettura == vettura.IdVettura && (!checkTripDuplicati.Checked || x.TripId == vettura.TripId));
                        if (presente != null)
                            vettura.PrimaVolta = presente.PrimaVolta;
                    }

                    ElencoAggregatoVetture = elencoVetture.Union(ElencoAggregatoVetture).ToList();
                    labelTotaleRighe.Text = ElencoAggregatoVetture.Count.ToString();
                    int TotaleMatricola = ElencoAggregatoVetture.Select(i => i.Matricola.Trim()).Distinct().Count();
                    int TotaleMatricolaAtac = ElencoAggregatoVetture.Where(i => string.Equals(i.Gestore, "atac", StringComparison.OrdinalIgnoreCase)).Select(i => i.Matricola.Trim()).Distinct().Count();
                    int TotaleMatricolaTPL = ElencoAggregatoVetture.Where(i => string.Equals(i.Gestore, "tpl", StringComparison.OrdinalIgnoreCase)).Select(i => i.Matricola.Trim()).Distinct().Count();
                    int TotaleIdVettura = ElencoAggregatoVetture.Select(i => i.IdVettura).Distinct().Count();
                    labelTotaleIdVettura.Text = TotaleIdVettura.ToString();
                    labelTotaleMatricola.Text = TotaleMatricola.ToString();
                    labelTotaleMatricolaATAC.Text = TotaleMatricolaAtac.ToString();
                    labelTotaleMatricolaTPL.Text = TotaleMatricolaTPL.ToString();

                    dataGridVetture.DataSource = ElencoAggregatoVetture;
                    List<string> urlTripList = new List<string>();
                    List<string> urlVehicleList = new List<string>();
                    foreach (FeedEntity entity in feedEntities)
                    {
                        if (entity.Vehicle != null && entity.Vehicle.Trip != null && (int.Parse(routeID) > 0 && entity.Vehicle.Trip.RouteId == routeID))
                        {
                            textBox1.AppendText(entity.Vehicle.Vehicle.Id + Environment.NewLine);
                        }
                    }
                    //List<FeedEntity> busLinea = feedEntities.Where(x => x.Vehicle.Trip != null).ToList();
                    List<ExtendedVehicleInfo> busLinea = elencoVetture.Where(x => x.TripId != null).ToList();
                    int numVettureFeedVehicle = feedEntities.Where(x => x.Vehicle.Vehicle != null && !string.IsNullOrEmpty(x.Vehicle.Vehicle.Id)).Count();
                    //int numVettureTPLFeedVehicle = busLinea.Where(x => x.Vehicle.Vehicle != null && !string.IsNullOrEmpty(x.Vehicle.Vehicle.Id) && x.Vehicle.Vehicle.Id.Length > 4).Count();
                    int numVettureTPLFeedVehicle = elencoVetture.Where(x => x.Gestore != null && x.Gestore == "TPL").Count();
                    int numVettureTPLATAC = elencoVetture.Where(x => x.Gestore != null && x.Gestore == "ATAC").Count();
                    
                    int busAttesa = feedEntities.Where(x => x.Vehicle.Trip == null).Count();
                    int busTotale = busLinea.Count + busAttesa;
                    textBox1.AppendText($"Totale Vetture Rilevate sul Feed Vehicle {busTotale}" + Environment.NewLine);
                    textBox1.AppendText($"\tATAC {busTotale - numVettureTPLFeedVehicle}\tTPL {numVettureTPLFeedVehicle}" + Environment.NewLine);
                    textBox1.AppendText($"\tSu Linea {busLinea.Count()}\tIn Attesa {busAttesa}" + Environment.NewLine);
                    labelTPL.Text = $"{numVettureTPLFeedVehicle}";
                    labelAtac.Text = $"{busTotale - numVettureTPLFeedVehicle}";
                    labelWait.Text = $"{busAttesa}";
                    labelTot.Text = $"{busTotale}";
                    
                    if (vettureAggiunte.Count > 0 || vettureTolte.Count > 0 || ElencoPrecedente.Count == 0 || elencoVetture.Count > 0)
                    {
                        if (string.IsNullOrEmpty(fileName))
                        {
                            fileName = $"Feed_{dataFeedVehicle:yyyy-MM-dd (HH_mm_ss)}";
                            FirstdataFeedVehicle = dataFeedVehicle;
                        }
                        MonitoraggioVettureGrafico nuovoMonitoraggio = new MonitoraggioVettureGrafico
                        {
                            DateTime = dataFeedVehicle,
                            Aggregate = TotaleMatricola,
                            AggregateAtac = TotaleMatricolaAtac,
                            AggregateTPL = TotaleMatricolaTPL,
                            Rilevate = busTotale,
                            Atac = busTotale - numVettureTPLFeedVehicle,
                            TPL = numVettureTPLFeedVehicle,
                            Aggiunte = ElencoPrecedente.Count > 0 ? vettureAggiunte.Count : 0,
                            Tolte = vettureTolte.Count
                        };
                        ElencoVettureGrafico.Add(nuovoMonitoraggio);

                        if (tabControl1.TabPages.Contains(tabMonitoraggio))
                        {
                            int giornosettimana = (int)dataFeedVehicle.DayOfWeek;
                            var vettureSuLinea = elencoVetture
                                .GroupBy(c => c.Linea)
                                .Select(g => new
                                {
                                    Linea = g.Key,
                                    count = g.Count(),
                                    date = g.Max(x => x.UltimaVolta)
                                })
                                .OrderByDescending(c => c.date);

                            TimeSpan oraFeed = dataFeedVehicle.TimeOfDay;

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


                            List<LineaMonitorata>  situazioneAttualeEdAlert = ElencoLineeMonitorate
                                .Where(riga=> !riga.OraUltimaViolazione.HasValue  
                                            || (riga.OraUltimaViolazione.GetValueOrDefault(LastdataFeedVehicle.GetValueOrDefault()) - riga.OraPrimaViolazione.GetValueOrDefault(DateTime.MinValue)).TotalMinutes > riga.TempoBonus)
                                .ToList()
                                ;
                            
                            dataGridViolazioni.DataSource = situazioneAttualeEdAlert; // ElencoLineeMonitorate;
                            Colora();
                            
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
                                                else {                                                    
                                                    List<string> vettureEsistenti = esisteLineaRegola.Violazione.Replace(" ",""). Split(',').ToList();
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
                        }
                        ExportGrid();
                        ElencoPrecedente = elencoVetture;
                        LastdataFeedVehicle = dataFeedVehicle;
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

                        textBox1.AppendText(
                            $"Totale Vetture Rilevate sul Feed Trip: {numVettureFeedTrip} TPL({numVettureTPLFeedTrip})  feed.Entities.Count{feedTrip.Entities.Count}" + Environment.NewLine
                        );

                        List<FeedEntity> soloVehicle = feedEntities
                            .Where(vehicle => !feedTrip.Entities.Any(trip => vehicle.Vehicle.Vehicle.Id == trip.TripUpdate.Vehicle.Id))
                            .ToList();

                        IEnumerable<string> tripDuplicatiFeedVehicle = from trip in feedEntities
                                                                       where trip.Vehicle.Trip?.TripId != null
                                                                       group trip by trip.Vehicle.Trip.TripId into grp
                                                                       where grp.Count() > 1
                                                                       select grp.Key;
                        foreach (string tripDuplicato in tripDuplicatiFeedVehicle)
                        {
                            textBox2.AppendText($"Trip Duplicato sul Feed Vehicle: {tripDuplicato}" + Environment.NewLine);
                            var dup = feedEntities.Where(x => x.Vehicle.Trip?.TripId == tripDuplicato).ToList();
                        }

                        List<FeedEntity> soloTrip = feedTrip.Entities
                            .Where(trip => !feedEntities.Any(vehicle => vehicle.Vehicle.Vehicle.Id == trip.TripUpdate.Vehicle.Id))
                            .ToList();
                        IEnumerable<string> tripDuplicatiFeedTrip = from trip in feedTrip.Entities
                                                                    group trip by trip.TripUpdate.Trip.TripId into grp
                                                                    where grp.Count() > 1
                                                                    select grp.Key;
                        foreach (FeedEntity trip in soloTrip)
                        {
                            textBox2.AppendText($"Solo sul Feed Trip: {trip.TripUpdate.Vehicle.Id}" + Environment.NewLine);
                        }

                        foreach (FeedEntity vehicle in soloVehicle)
                        {
                            textBox2.AppendText($"Solo sul Feed Vehicle: {vehicle.Vehicle.Vehicle.Id}" + Environment.NewLine);
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
            }
            catch (WebException ex) when ((ex.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.NotFound)
            {
                textBox1.AppendText($"{ex.Message}: {Environment.NewLine}Feed Non trovato al seguente indirizzo");
                textBox1.AppendText($"{Environment.NewLine}{ex.Response.ResponseUri}");
                textBox1.AppendText($"{Environment.NewLine}{Environment.NewLine}==== Informazioni di Debug per gli sviluppatori ===");
                textBox1.AppendText($"{Environment.NewLine}{Environment.NewLine}{ex.StackTrace}");
                textBox1.AppendText($"{Environment.NewLine}===================================================");
            }
            catch (WebException ex) when (ex.Status == WebExceptionStatus.Timeout)
            {
                textBox1.AppendText($"{ex.Message}: Problemi di connessione con il server di RSM");
                textBox1.AppendText($"{Environment.NewLine}{Environment.NewLine}==== Informazioni di Debug per gli sviluppatori ===");
                textBox1.AppendText($"{Environment.NewLine}{ex.StackTrace}");
                textBox1.AppendText($"{Environment.NewLine}===================================================");

            }
            catch (Exception ex)
            {
                textBox1.AppendText($"{ex.Message} {Environment.NewLine} {ex.StackTrace}");
                textBox1.AppendText($"{Environment.NewLine}{Environment.NewLine}==== Informazioni di Debug per gli sviluppatori ===");
                textBox1.AppendText($"{Environment.NewLine}{ex.StackTrace}");
                textBox1.AppendText($"{Environment.NewLine}===================================================");
            }
        }
        
        private void Button1_Click(object sender, EventArgs e)
        {
            if (!checkCSV.Checked && !checkXlsx.Checked)
            {
                DialogResult dialog = MessageBox.Show("Avviare il monitoraggio senza export dei dati?",
                "Avvio Monitoraggio",
                 MessageBoxButtons.YesNo,
                 MessageBoxIcon.Question);
                if (dialog == DialogResult.No)
                    return;
            }
            Timer1_Tick(sender, e);
            int deltaSec = (int)(1000 * (60 * minuti.Value + secondi.Value));
            if (!timer1.Enabled && deltaSec > 0)
            {
                minuti.Enabled = false;
                secondi.Enabled = false;
                timer1.Interval = deltaSec;
                timer1.Enabled = true;
                timer1.Start();
                button1.BackgroundImage = Properties.Resources.pause;
                comboBox1.Enabled = false;
                buttonResetRegole.Enabled = false;
            }
            else
            {
                minuti.Enabled = true;
                secondi.Enabled = true;
                timer1.Enabled = false;
                timer1.Stop();
                button1.BackgroundImage = Properties.Resources.play;
                comboBox1.Enabled = true;
                buttonResetRegole.Enabled = true;
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
            /*
            var serieAggregate = (from elenco in ElencoVettureGrafico select (double)(elenco.Aggregate)).ToArray();
            var serieRilevate = (from elenco in ElencoVettureGrafico select (double)(elenco.Rilevate)).ToArray();
            */

            var serieAtac = (from elenco in ElencoVettureGrafico select (double)(elenco.Atac)).ToArray();
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
            var serieTPL = (from elenco in ElencoVettureGrafico select (double)(elenco.TPL)).ToArray();
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
            ElencoAggregatoVetture = new List<ExtendedVehicleInfo>();
            ElencoPrecedente = new List<ExtendedVehicleInfo>();
            ElencoVettureGrafico = new List<MonitoraggioVettureGrafico>();
            RegoleMonitoraggio = new List<RegolaMonitoraggio>();
            ElencoLineeMonitorate = new List<LineaMonitorata>();

            AlertsDaControllare = new List<AlertDaControllare>();

            var reader = new GTFSReader<GTFSFeed>();
            staticData = reader.Read($"Config{Path.DirectorySeparatorChar}GTFS_Static");

            List<Route> elencoLinee = staticData.Routes
                .OrderBy(k => k.ShortName)
                .ToList();

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

            int totalSeconds = Properties.Settings.Default.DeltaTSec;
            minuti.Value = totalSeconds / 60;
            secondi.Value = totalSeconds % 60;
            #endregion

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
                ElencoDettagliVettura = csv.GetRecords<DettagliVettura>().ToList();
            }
            try
            {
                using (var readerTempoBonus = new StreamReader($"Config{Path.DirectorySeparatorChar}MonitoraggioLinee{Path.DirectorySeparatorChar}RegoleMonitoraggio.txt"))
                using (var csv = new CsvReader(readerTempoBonus, CultureInfo.InvariantCulture))
                {
                    csv.Configuration.Delimiter = ",";
                    csv.Configuration.HeaderValidated = null;
                    csv.Configuration.MissingFieldFound = null;
                    RegoleMonitoraggio = csv.GetRecords<RegolaMonitoraggio>().ToList();
                    dataGridViolazioni.DataSource = RegoleMonitoraggio;
                }
            }
            catch
            {                
                tabControl1.TabPages.Remove(tabMonitoraggio);
            }
            
            LeggiRegoleAlertDaFile();

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
                            csv.Configuration.MissingFieldFound = null;
                            csv.Configuration.RegisterClassMap<RegolaAlertMap>();
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
                                //columnDa.DefaultCellStyle = timeFormat; 
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
                                tabControl1.TabPages.Add(myNewTabItem);

                                //myNewdataGridVetture.DataSource = listaRegole.OrderBy(x => x.Linea).ToList();
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
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            Acquisizione();
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
            var radioRaggruppamento = groupBoxMonitoraggio
                .Controls.OfType<RadioButton>()
                .FirstOrDefault(r => r.Checked);
            Properties.Settings.Default.RadioRaggruppamento = radioRaggruppamento.Name;
            Properties.Settings.Default.Save();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (timer1.Enabled)
            {
                DialogResult dialog = MessageBox.Show("Interrompere il monitoraggio ed uscire? ",
                                "Conferma chiusura programma",
                                 MessageBoxButtons.YesNo,
                                 MessageBoxIcon.Question);
                if (dialog == DialogResult.No)
                {
                    e.Cancel = true;
                }

                if (!string.IsNullOrEmpty(fileName))
                {
                    ExportGrid();
                }
            }
        }

        private Task ExportGrid()
        {
            return Task.Run(() =>
            {
                if (checkXlsx.Checked)
                {
                    var file = new FileInfo($"OUTPUT{Path.DirectorySeparatorChar}{fileName}.xlsx");
                    using (ExcelPackage excel = new ExcelPackage(file))
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
                            lineChartATAC.Title.Text = $"Vetture Rilevate ATAC {FirstdataFeedVehicle:dd-MM-yyyy [HH:mm:ss}-{LastdataFeedVehicle:HH:mm:ss}] ";
                            lineChartTPL.Title.Text = $"Vetture Rilevate  TPL {FirstdataFeedVehicle:dd-MM-yyyy [HH:mm:ss}-{LastdataFeedVehicle:HH:mm:ss}] ";
                            ExcelRangeBase rangeLabel = range.Offset(1, 0, ElencoVettureGrafico.Count, 1);
                            ExcelRangeBase range1 = range.Offset(1, 2, ElencoVettureGrafico.Count, 1);
                            ExcelRangeBase range2 = range.Offset(1, 4, ElencoVettureGrafico.Count, 1);
                            ExcelRangeBase range3 = range.Offset(1, 3, ElencoVettureGrafico.Count, 1);
                            ExcelRangeBase range4 = range.Offset(1, 5, ElencoVettureGrafico.Count, 1);

                            lineChartATAC.Series.Add(range1, rangeLabel);
                            lineChartATAC.Series.Add(range2, rangeLabel);
                            lineChartTPL.Series.Add(range3, rangeLabel);
                            lineChartTPL.Series.Add(range4, rangeLabel);

                            lineChartATAC.Series[0].Header = "Aggregate"; //range.Skip(1).First().Value.ToString();
                            lineChartATAC.Series[1].Header = "Istantanee"; // range.Skip(2).First().Value.ToString();
                            lineChartTPL.Series[0].Header = "Aggregate"; // range.Skip(3).First().Value.ToString();
                            lineChartTPL.Series[1].Header = "Istantanee"; // range.Skip(4).First().Value.ToString();

                            lineChartATAC.Legend.Position = eLegendPosition.Right;
                            lineChartATAC.SetSize(900, 250);
                            lineChartATAC.SetPosition(0, 3, 0, 3);
                            lineChartTPL.Legend.Position = eLegendPosition.Right;
                            lineChartTPL.SetSize(900, 250);
                            lineChartTPL.SetPosition(14, 3, 0, 3);
                        }

                        if (tabControl1.TabPages.Contains(tabMonitoraggio) && checkMonitoraggio.Checked)
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

                        excel.Save();

                    }
                }

                if (checkCSV.Checked)
                {
                    using (var writer = new StreamWriter($"OUTPUT{Path.DirectorySeparatorChar}{fileName}.csv"))
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        csv.Configuration.Delimiter = ";";
                        csv.WriteRecords(ElencoAggregatoVetture);
                    }
                }

            });
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
                    TimeSpan span = riga.OraUltimaViolazione.GetValueOrDefault(LastdataFeedVehicle.GetValueOrDefault()) - riga.OraPrimaViolazione.GetValueOrDefault(DateTime.MinValue);
                    
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
            if (tabSelezoinato == "tabMonitoraggio" && LastdataFeedVehicle.HasValue)
            {
                Colora();
            }
        }

        private void ResetAcquisizione(object sender, EventArgs e)
        {
            
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
            labelWait.Text = "0";
            labelTot.Text = "0";
            
            LeggiRegoleAlertDaFile();
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
    }
}
