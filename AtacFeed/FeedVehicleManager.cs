using AtacFeed;
using GTFS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using static AtacFeed.TransitRealtime;

namespace AtacFeed
{
    public class FeedVehicleManager : BaseFeedManager
    {
        public List<FeedEntity> FeedEntities { get; set; }

        public List<ExtendedVehicleInfo> ElencoVetture { get; set; }
        public List<ExtendedVehicleInfo> ElencoPrecedente;

        public List<ExtendedVehicleInfo> ElencoAggregatoVetture;
        public List<ExtendedVehicleInfo> ElencoAggregatoPrecedente;
        public List<ErroriGTFS> AnomaliaGTFS;

        public List<ExtendedVehicleInfo> VettureSenzaMatricola() => ElencoVetture.Where(x => string.IsNullOrEmpty(x.Matricola) || string.IsNullOrEmpty(x.IdVettura)).ToList();
        public List<string> LineeAnomale() => FeedEntities.Select(x => x.Vehicle.Trip?.RouteId).Distinct().OrderBy(q => q).Where(p => !GTFS_RSM.ElencoLineaAgenzia.Any(p2 => p2.Route.Id == p)).ToList();


        public GTFS_RSM GTFS_RSM { get; set; }
        public List<LineaMonitorata> ViolazioniLeneeMonitorate() => ElencoLineeMonitorate.Where(riga => !riga.OraUltimaViolazione.HasValue || (riga.OraUltimaViolazione.GetValueOrDefault(LastDataFeed.GetValueOrDefault()) - riga.OraPrimaViolazione.GetValueOrDefault(DateTime.MinValue)).TotalMinutes > riga.TempoBonus).ToList();

        public int TotaleMatricola;
        public int TotaleIdVettura;

        public int TotaleMatricolaAtac;
        public int TotaleMatricolaTPL;

        public double PonderateAtac = 0;
        public double PonderateTPL = 0;

        public List<LineaMonitorata> ElencoLineeMonitorate;

        public List<ViolazioneAlert> ViolazioniAlertAttuali;
        public Statistiche StatisticheAttuali;

        public List<ExtendedVehicleInfo> VettureTolte;
        public List<ExtendedVehicleInfo> VettureAggiunte;
        public List<ExtendedVehicleInfo> VettureFresche;
        public List<ExtendedVehicleInfo> PartenzaAvanzata;
        public List<ExtendedVehicleInfo> VettureRiagganciate;
        public List<ErroriGTFS> PercorsoAnomalo;

        public List<RunTimeValueAlert> ElencoVettureSovraffollate;
        public List<MonitoraggioVettureGrafico> ElencoVettureGrafico;
        public FeedVehicleManager()
        {
            StatisticheAttuali = new Statistiche();
            ElencoAggregatoVetture = new List<ExtendedVehicleInfo>();
            ElencoPrecedente = new List<ExtendedVehicleInfo>();
            ElencoLineeMonitorate = new List<LineaMonitorata>();
            AnomaliaGTFS = new List<ErroriGTFS>();
            ElencoVettureSovraffollate = new List<RunTimeValueAlert>();
            ElencoVettureGrafico = new List<MonitoraggioVettureGrafico>();
        }

        public List<RunTimeValueAlert> GetBusPieni() => ElencoVetture
                .Where(x => x.OccupancyStatus >= VehiclePosition.OccupancyStatus.Full)
                .Select(x => new RunTimeValueAlert(
                    x.TripId,
                    x.Linea,
                    x.Matricola,
                    x.OccupancyStatus,
                    x.CurrentStopSequence,
                    x.PrimaVolta,
                    x.CurrentStopSequence,
                    x.UltimaVolta)
                )
                .ToList();
        public Exception ElaboraUltimoFeedValido_BCK(string filtroLinea, bool filtroTripVuoti, bool filtroTuttoPercorso, bool raggruppalineaRegola, bool nonRaggruppare)
        {
            Exception ecc = null;
            FeedEntities = LastValidFeed.Entities
                            .Where(x => !x.IsDeleted
                                        && (string.IsNullOrEmpty(filtroLinea) || x.Vehicle?.Trip?.RouteId == filtroLinea)
                                        && (!filtroTripVuoti || x.Vehicle?.Trip?.TripId.Length > 0))
                            .ToList();

            // Saniamo FeedEntities con RouteId null, recuperandolo tramite Routes
            foreach (FeedEntity entity in FeedEntities.Where(v => string.IsNullOrEmpty(v.Vehicle.Trip?.RouteId)))
            {
                var routeId = GTFS_RSM.StaticData.Trips.Where(x => x.Id == entity.Vehicle.Trip.TripId).FirstOrDefault();
                entity.Vehicle.Trip.RouteId = routeId?.RouteId ?? string.Empty;
            }
            //var ff = GTFS_RSM.StaticData.Routes.Where(x => x.ShortName== "246").FirstOrDefault();
            ElencoVetture = (from fe in FeedEntities
                                 .GroupJoin(
                                     inner: GTFS_RSM.ElencoLineaAgenzia,
                                     outerKeySelector: v => v.Vehicle.Trip?.RouteId,
                                     innerKeySelector: l => l.Route.Id,
                                     resultSelector: (v, l) => (v.Vehicle, Linea: l.FirstOrDefault()))
                                 .GroupJoin(
                                     inner: GTFS_RSM.ElencoDettagliVettura,
                                     outerKeySelector: o2 => o2.Vehicle.Vehicle?.Label.Trim(),
                                     innerKeySelector: i2 => i2.Matricola,
                                     resultSelector: (o2, i2) => (o2.Linea, o2.Vehicle, DettagliVettura: i2.FirstOrDefault()))
                                 .GroupJoin(
                                     inner: GTFS_RSM.StaticData.Stops,
                                     outerKeySelector: z => z.Vehicle.StopId,
                                     innerKeySelector: y => y.Code,
                                     resultSelector: (z, y) => (z.Linea, z.Vehicle, z.DettagliVettura, NomeFermata: y.FirstOrDefault()))
                             from trip in GTFS_RSM.StaticData.Trips
                                            .Where(x => x.Id == fe.Vehicle.Trip?.TripId)
                                            .DefaultIfEmpty()
                             select (fe.Linea, fe.Vehicle, fe.DettagliVettura, fe.NomeFermata, trip?.Headsign, trip?.Direction)
                             )
                             .Select(x =>
                             {
                                 string linea = string.Empty;
                                 if (x.Linea?.Route.ShortName == x.Linea?.Route.LongName || string.IsNullOrEmpty(x.Linea?.Route.LongName))
                                 {
                                     linea = x.Linea?.Route.ShortName;
                                 }
                                 else if (string.IsNullOrEmpty(x.Linea?.Route.ShortName))
                                 {
                                     linea = x.Linea?.Route.LongName;
                                 }
                                 else
                                 {
                                     linea = x.Linea?.Route.ShortName + " - " + x.Linea?.Route.LongName;
                                 }

                                 return new ExtendedVehicleInfo(
                                                    idVettura: x.Vehicle.Vehicle?.Id,
                                                    matricola: x.Vehicle.Vehicle?.Label.Trim(),
                                                    licensePlate: x.Vehicle.Vehicle?.LicensePlate,
                                                    routeId: x.Vehicle.Trip?.RouteId,
                                                    linea: linea,
                                                    gestore: x.DettagliVettura?.Gestore ?? x.Linea?.Agency.Name.ToUpper(),
                                                    //directionId: x.Vehicle.Trip?.DirectionId,
                                                    directionId: (uint?)x.Direction ?? x.Vehicle.Trip?.DirectionId,
                                                    currentStopSequence: x.Vehicle.CurrentStopSequence,
                                                    congestionLevel: x.Vehicle.congestion_level,
                                                    occupancyStatus: x.Vehicle.occupancy_status,
                                                    tripId: x.Vehicle.Trip?.TripId,
                                                    strict: filtroTripVuoti,
                                                    data: LastDataFeed.Value,
                                                    rimessa: x.DettagliVettura?.Rimessa,
                                                    euro: x.DettagliVettura?.Euro,
                                                    modello: x.DettagliVettura?.Modello,
                                                    latitude: x.Vehicle.Position?.Latitude ?? 0,
                                                    longitude: x.Vehicle.Position?.Longitude ?? 0,
                                                    inTransitTo: x.Vehicle.CurrentStatus,
                                                    tipoMezzoTrasporto: x.DettagliVettura?.TipoMezzoTrasporto.GetValueOrDefault(0) ?? (string.Equals(x.Linea?.Agency.Name, "atac", StringComparison.OrdinalIgnoreCase) ? (x.Linea?.Route.Type == GTFS.Entities.Enumerations.RouteTypeExtended.TramService ? -1 : -2) : -3),
                                                    distanzaPercorsa: x.Vehicle.Position?.Odometer ?? 0,
                                                    superStrictMode: filtroTuttoPercorso,
                                                    nomeFermata: x.NomeFermata?.Name,
                                                    destinazione: x.Headsign,
                                                    dataProgrammata: x.Vehicle.Trip?.StartDate,
                                                    oraProgrammata: x.Vehicle.Trip?.StartTime
                                    );
                             })
                             .Distinct()
                             .OrderBy(x => x.IdVettura)
                             .ToList();
            foreach (ExtendedVehicleInfo vettura in ElencoVetture)
            {
                var presente = ElencoAggregatoVetture
                    .Where(x => x.Matricola == vettura.Matricola
                                && (!filtroTripVuoti || x.TripId == vettura.TripId)
                                && (!filtroTuttoPercorso || x.CurrentStopSequence == vettura.CurrentStopSequence)
                          )
                    .OrderByDescending(x => x.UltimaVolta)
                    .FirstOrDefault();

                if (presente != null)
                {
                    if (presente.PartenzaEffettiva.HasValue)
                    {
                        vettura.PartenzaEffettiva = presente.PartenzaEffettiva;
                    }
                    else if (presente.CurrentStopSequence <= 1 && presente.InTransitTo == VehiclePosition.VehicleStopStatus.StoppedAt
                                && vettura.CurrentStopSequence >= 1 && vettura.InTransitTo == VehiclePosition.VehicleStopStatus.InTransitTo)
                    {
                        vettura.PartenzaEffettiva = presente.UltimaVolta.GetValueOrDefault().AddSeconds((vettura.UltimaVolta.GetValueOrDefault() - presente.PrimaVolta).Seconds);
                    }
                    else if (presente.CurrentStopSequence <= 3 && ElencoAggregatoVetture.Count > 0 && presente.InTransitTo != VehiclePosition.VehicleStopStatus.StoppedAt)
                    {
                        vettura.PartenzaEffettiva = presente.PrimaVolta;
                    }
                    vettura.PrimaVolta = presente.PrimaVolta;
                    vettura.OccupancyStatus = vettura.OccupancyStatus.CompareTo(presente.OccupancyStatus) >= 0 ? vettura.OccupancyStatus : presente.OccupancyStatus;
                }
            }


            ElencoAggregatoVetture = ElencoVetture.Union(ElencoAggregatoVetture).ToList();

            //var group_RouteType= 
            //    from vetture in ElencoVetture

            //    join route in GTFS_RSM.StaticData.Routes
            //    on vetture.RouteId equals route.Id

            //    join agency in GTFS_RSM.StaticData.Agencies
            //    on route.AgencyId equals agency.Id

            //    select new { route, agency } 
            //    into joinTable

            //    group joinTable by new { joinTable.route.AgencyId, joinTable.route.Type, joinTable.agency.Name }
            //    into grp
            //    orderby grp.Key.Name

            //    select new { 
            //        Agenzia = grp.Key.Name,
            //        Tipo = grp.Key.Type.ToString(),
            //        Num = grp.Count()
            //    };

            List<ServizioRaggruppato> group_RouteType2 = ElencoVetture
                .Join(
                    GTFS_RSM.StaticData.Routes,
                    outerKeySelector: v => v.RouteId,
                    innerKeySelector: l => l.Id,
                    resultSelector: (v, l) => new { l }
                )
                .Join(
                    GTFS_RSM.StaticData.Agencies,
                    o2 => o2.l.AgencyId,
                    i2 => i2.Id,
                    (o2, i2) => new { i2.Name, o2.l.Type, i2.Id }
                )
                .GroupBy(x => new { x.Id, x.Type, x.Name })
                .Select(x => new ServizioRaggruppato
                {
                    Agenzia = x.Key.Name,
                    Servizio = x.Key.Type.ToString(),
                    Tipo = x.Key.Type,
                    Num = x.Count()
                })
                .OrderBy(x => x.Agenzia)
                .ThenByDescending(x => x.Num)
                .ToList();
            ;
            StatisticheAttuali.ServizioRaggruppato = group_RouteType2;

            TotaleMatricola = ElencoAggregatoVetture.Select(i => i.Matricola?.Trim()).Distinct().Count();

            IEnumerable<(string Matricola, int TipoMezzoTrasporto)> elencoAggregatoAtac = ElencoAggregatoVetture
                .Where(i => i.TipoMezzoTrasporto == 0 || i.TipoMezzoTrasporto == 1 || i.TipoMezzoTrasporto == 2 || i.TipoMezzoTrasporto == 5 || i.TipoMezzoTrasporto == 6 || i.TipoMezzoTrasporto == -2)
                .Select(i => (Matricola: i.Matricola.Trim(), i.TipoMezzoTrasporto))
                .Distinct();
            TotaleMatricolaAtac = elencoAggregatoAtac.Count();

            IEnumerable<(string Matricola, int TipoMezzoTrasporto)> elencoAggregatoTPL =
                ElencoAggregatoVetture
                .Where(i => i.TipoMezzoTrasporto == 3 || i.TipoMezzoTrasporto == 4 || i.TipoMezzoTrasporto == -3)
                .Select(i => (Matricola: i.Matricola?.Trim(), i.TipoMezzoTrasporto))
                .Distinct();
            TotaleMatricolaTPL = elencoAggregatoTPL.Count();

            TotaleIdVettura = ElencoAggregatoVetture.Select(i => i.IdVettura).Distinct().Count();

            StatisticheAttuali.RilevatoBusAtac = ElencoVetture.Where(x => x.TipoMezzoTrasporto == 0).Count();
            StatisticheAttuali.RilevatoTramAtac = ElencoVetture.Where(x => x.TipoMezzoTrasporto == 1).Count();
            StatisticheAttuali.RilevatoFilobusAtac = ElencoVetture.Where(x => x.TipoMezzoTrasporto == 2).Count();
            StatisticheAttuali.RilevatoMinibusElettrici = ElencoVetture.Where(x => x.TipoMezzoTrasporto == 5).Count();
            StatisticheAttuali.RilevatoFurgoncini = ElencoVetture.Where(x => x.TipoMezzoTrasporto == 6).Count();
            StatisticheAttuali.RilevatoFerro = ElencoVetture.Where(x => x.TipoMezzoTrasporto == -1 || x.TipoMezzoTrasporto == 7).Count();
            StatisticheAttuali.RilevatoAltroAtac = ElencoVetture.Where(x => x.TipoMezzoTrasporto == -2).Count();
            StatisticheAttuali.RilevatoBusTpl = ElencoVetture.Where(x => x.TipoMezzoTrasporto == 3).Count();
            StatisticheAttuali.RilevatoPullmanTpl = ElencoVetture.Where(x => x.TipoMezzoTrasporto == 4).Count();
            StatisticheAttuali.RilevatoAltroTpl = ElencoVetture.Where(x => x.TipoMezzoTrasporto == -3).Count();

            int giornosettimana = (int)LastDataFeed.Value.DayOfWeek;
            TimeSpan oraFeed = LastDataFeed.Value.TimeOfDay;

            if (GTFS_RSM.RegoleMonitoraggio?.Count > 0)
            {
                var vettureSuLinea = ElencoVetture
                .GroupBy(c => c.Linea)
                .Select(g => new
                {
                    Linea = g.Key,
                    count = g.Count(),
                    date = g.Max(x => x.UltimaVolta)
                })
                .OrderByDescending(c => c.date);


                IEnumerable<RegolaMonitoraggio> regoleApplicabili = from regolaMonitoraggio in GTFS_RSM.RegoleMonitoraggio
                                                                    where regolaMonitoraggio.Giorno.Contains(giornosettimana.ToString())
                                                                          && regolaMonitoraggio.Da < oraFeed
                                                                          && oraFeed <= regolaMonitoraggio.A.GetValueOrDefault(oraFeed)
                                                                    select regolaMonitoraggio;

                IEnumerable<LineaMonitorata> lineeMonitorate = from regola in regoleApplicabili
                                                               join vettura in vettureSuLinea
                                                                    on regola.Linea equals vettura.Linea
                                                               into lrs
                                                               from lr in lrs.DefaultIfEmpty()
                                                               select new LineaMonitorata(
                                                                   oraPrimaViolazione: null,
                                                                   oraUltimaViolazione: null,
                                                                   regolaViolata: regola,
                                                                   vettureRilevate: lr?.count ?? 0
                                                                );

                foreach (var lineaMonitorata in lineeMonitorate)
                {
                    LineaMonitorata esiste = ElencoLineeMonitorate
                        .Where(x => x.Linea == lineaMonitorata.Linea
                                    && x.OraUltimaViolazione.GetValueOrDefault() == lineaMonitorata.OraUltimaViolazione.GetValueOrDefault())
                        .FirstOrDefault();

                    if (lineaMonitorata.VettureRilevate < lineaMonitorata.VetturePreviste)
                    {
                        lineaMonitorata.OraPrimaViolazione = LastDataFeed;
                    }
                    else
                    {
                        if (esiste != null && esiste.OraPrimaViolazione.HasValue)
                        {
                            esiste.OraUltimaViolazione = LastDataFeed;
                        }
                    }

                    LineaMonitorata presente = ElencoLineeMonitorate
                        .Where(x => x.Linea == lineaMonitorata.Linea
                                    && x.OraUltimaViolazione.GetValueOrDefault() == lineaMonitorata.OraUltimaViolazione.GetValueOrDefault())
                        .FirstOrDefault();
                    if (presente != null)
                    {
                        ElencoLineeMonitorate.Remove(presente);
                        lineaMonitorata.OraPrimaViolazione = presente.OraPrimaViolazione ?? lineaMonitorata.OraPrimaViolazione;
                    }
                    ElencoLineeMonitorate.Add(lineaMonitorata);
                }
            }

            ///////////////////////////////////////////////////////////////////////////

            VettureTolte = ElencoPrecedente.Except(ElencoVetture).ToList();
            VettureAggiunte = ElencoVetture.Except(ElencoPrecedente).ToList();
            var vettureAggiunteTolte =
                from tolte in VettureTolte
                join aggiunte in VettureAggiunte on tolte.Matricola equals aggiunte.Matricola
                select new { tolte.Matricola };
            VettureTolte = VettureTolte.Where(x => !vettureAggiunteTolte.Any(c => c.Matricola == x.Matricola)).ToList();
            VettureAggiunte = VettureAggiunte.Where(x => !vettureAggiunteTolte.Any(c => c.Matricola == x.Matricola)).ToList();

            if (ElencoPrecedente.Count > 0)
            {
                /// Controllo accuratezza GTFS rispetto al precedente letto                                                                        
                VettureFresche = ElencoVetture
                        .Where(x => !ElencoPrecedente.Any(c => c.Matricola == x.Matricola && c.TripId == x.TripId))
                        .ToList();

                PartenzaAvanzata = VettureFresche
                    .Where(x => x.CurrentStopSequence > 1 && !ElencoAggregatoVetture.Any(c => c.Matricola == x.Matricola && c.TripId == x.TripId))
                    .ToList();

                VettureRiagganciate = VettureFresche
                    .Where(x => ElencoAggregatoVetture.Any(c => c.Matricola == x.Matricola && c.TripId == x.TripId))
                    .ToList();
                //.Except(partenzaAvanzata)
                //.Where(x => x.CurrentStopSequence > 1)
                //.ToList();
                if (VettureRiagganciate.Count() > 0)
                {
                    foreach (ExtendedVehicleInfo errore in VettureRiagganciate)
                    {
                        uint ultimaFermataRilevata = ElencoAggregatoVetture
                                .Where(x => x.TripId == errore.TripId && x.Matricola == errore.Matricola)
                                .Max(x => x.CurrentStopSequence);
                        int delta = (int)(errore.CurrentStopSequence - ultimaFermataRilevata);
                        AnomaliaGTFS.Add(new ErroriGTFS(errore, delta));
                    }
                }

                PercorsoAnomalo =
                    (from act in ElencoVetture
                     join prec in ElencoPrecedente
                         on (act.Matricola, act.TripId) equals (prec.Matricola, prec.TripId)
                     where (act.CurrentStopSequence - prec.CurrentStopSequence > 2) || (act.CurrentStopSequence - prec.CurrentStopSequence < 0)
                     select new ErroriGTFS(act, (int)(act.CurrentStopSequence - prec.CurrentStopSequence))
                    ).ToList();
                if (PercorsoAnomalo.Count() > 0)
                {
                    foreach (var errore in PercorsoAnomalo)
                    {
                        AnomaliaGTFS.Add(errore);
                    }
                }
            }

            int numVettureTPLFeedVehicle = ElencoVetture
                .Where(i => i.TipoMezzoTrasporto == 3 || i.TipoMezzoTrasporto == 4 || i.TipoMezzoTrasporto == -3)
                .Count();
            List<ExtendedVehicleInfo> listaMezziSuLinea = ElencoVetture
                .Where(x => x.TripId != null)
                .ToList();
            List<ExtendedVehicleInfo> listaBusAttesa = ElencoVetture.Where(x => x.TripId == null).ToList();

            int busLinea = listaMezziSuLinea.Count;
            int busAttesa = listaBusAttesa.Count;
            int busTotale = busLinea + busAttesa;
            MonitoraggioVettureGrafico nuovoMonitoraggio = new MonitoraggioVettureGrafico
            {
                DateTime = LastDataFeed.Value,
                Aggregate = TotaleMatricola,
                AggregateAtac = TotaleMatricolaAtac,
                AggregateTPL = TotaleMatricolaTPL,
                Rilevate = busTotale,
                Atac = StatisticheAttuali.RilevatoBusAtac + StatisticheAttuali.RilevatoTramAtac + StatisticheAttuali.RilevatoFilobusAtac + StatisticheAttuali.RilevatoMinibusElettrici + StatisticheAttuali.RilevatoFurgoncini + StatisticheAttuali.RilevatoAltroAtac, // busTotale - numVettureTPLFeedVehicle,
                TPL = numVettureTPLFeedVehicle,
                Aggiunte = ElencoPrecedente.Count > 0 ? VettureAggiunte.Count : 0,
                Tolte = VettureTolte.Count
            };
            ElencoVettureGrafico.Add(nuovoMonitoraggio);

            #region Controllo Custom Alert
            try
            {
                foreach (var alert in GTFS_RSM.AlertsDaControllare)
                {
                    IEnumerable<RegolaAlert> regoleAlertApplicabili = from regoleAlert in alert.RegoleAlert //gruppoRegoleAlert
                                                                      where regoleAlert.Giorno.Contains(giornosettimana.ToString())
                                                                            && regoleAlert.Da < oraFeed
                                                                            && oraFeed <= regoleAlert.A.GetValueOrDefault(oraFeed)
                                                                      select regoleAlert;
                    IEnumerable<string> lineedaVerificareAlert = regoleAlertApplicabili
                                                                    .GroupBy(test => test.Linea)
                                                                    .Select(grp => grp.First().Linea);


                    ViolazioniAlertAttuali = new List<ViolazioneAlert>();

                    foreach (string linea in lineedaVerificareAlert)
                    {
                        ViolazioniAlertAttuali = (
                            from vettura in ElencoVetture
                            join regolaAlert in regoleAlertApplicabili on vettura.Linea equals regolaAlert.Linea
                                into VetturaRegolaAlert
                            from vetturaRegolaAlert in VetturaRegolaAlert
                            where MatricolaToHexValue(vetturaRegolaAlert.VetturaDa) <= MatricolaToHexValue(vettura.Matricola)
                                        && MatricolaToHexValue(vettura.Matricola) <= MatricolaToHexValue(vetturaRegolaAlert.VetturaA)
                            select new ViolazioneAlert(LastDataFeed, null, vetturaRegolaAlert, vettura.Matricola)
                            ).ToList();

                        List<ViolazioneAlert> violazioniLineaStar = (
                            from vettura in ElencoVetture
                            join regolaAlert in regoleAlertApplicabili on "*" equals regolaAlert.Linea
                                into VetturaRegolaAlert
                            from vetturaRegolaAlert in VetturaRegolaAlert
                            where MatricolaToHexValue(vetturaRegolaAlert.VetturaDa) <= MatricolaToHexValue(vettura.Matricola)
                                        //&& Convert.ToInt32(vettura.Matricola, 16) <= vetturaRegolaAlert.VetturaA.GetValueOrDefault(vetturaRegolaAlert.VetturaDa.GetValueOrDefault(9999))
                                        && MatricolaToHexValue(vettura.Matricola) <= MatricolaToHexValue(vetturaRegolaAlert.VetturaA)
                            select new ViolazioneAlert(
                                    LastDataFeed,
                                    null,
                                    new RegolaAlert(vettura.Linea, vetturaRegolaAlert.Giorno, vetturaRegolaAlert.Da, vetturaRegolaAlert.Da
                                    , vetturaRegolaAlert.VetturaDa, vetturaRegolaAlert.VetturaA),
                                    vettura.Matricola)
                            ).ToList();

                        ViolazioniAlertAttuali = ViolazioniAlertAttuali.Union(violazioniLineaStar).ToList();

                        if (raggruppalineaRegola)
                        {
                            ViolazioniAlertAttuali = ViolazioniAlertAttuali
                                .GroupBy(x => new { x.Linea, x.Giorno, x.Da, x.A, x.VetturaDa, x.VetturaA })
                                .Select(group =>
                                    new ViolazioneAlert(
                                        LastDataFeed,
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
                        else if (raggruppalineaRegola)
                        {
                            ViolazioniAlertAttuali = ViolazioniAlertAttuali
                                .GroupBy(x => x.Linea)
                                .Select(group =>
                                    new ViolazioneAlert(
                                        LastDataFeed.Value,
                                        null,
                                        group.Key,
                                        string.Join(", ", group.Select(bn => bn.Violazione).ToList())
                                    )
                                )
                                .ToList();
                        }

                        foreach (var violazione in ViolazioniAlertAttuali)
                        {
                            if (nonRaggruppare)
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
                            else if (raggruppalineaRegola)
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
                                .Where(x => x.Linea == violazione.Linea /*&& string.IsNullOrEmpty(x.Violazione)*/)
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
                }
            }
            catch (Exception exc)
            {
                ecc = new Exception("Si è verificato un errore durante il controllo degli alert", exc);

            }
            #endregion

            #region Controllo sovraffollamento

            var listaBusPieni = GetBusPieni();

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

            #region Calcolo Media Ponderata            
            if (VettureAggiunte.Count > 0 || VettureTolte.Count > 0 || ElencoPrecedente.Count == 0 || ElencoVetture.Count > 0)
            {
                int campioniNecessari = GTFS_RSM.CriteriMediaPonderata.Sum(x => x.NumeroCampioni);
                if (ElencoVettureGrafico.Count >= campioniNecessari)
                {
                    PonderateAtac = 0;
                    PonderateTPL = 0;
                    int startIndex = ElencoVettureGrafico.Count;
                    foreach (var item in GTFS_RSM.CriteriMediaPonderata)
                    {
                        int numeroCampioni = item.NumeroCampioni;
                        startIndex -= numeroCampioni;

                        PonderateAtac +=
                            item.Peso / numeroCampioni *
                            ElencoVettureGrafico
                                .Skip(startIndex)
                                .Take(numeroCampioni)
                                .Sum(x => x.Atac)
                                ;
                        PonderateTPL +=
                            (item.Peso / numeroCampioni *
                            ElencoVettureGrafico
                                .Skip(startIndex)
                                .Take(numeroCampioni)
                                .Sum(x => x.TPL));
                    }
                }
            }
            #endregion

            ElencoPrecedente = ElencoVetture;

            return ecc;
        }



        public Exception ElaboraUltimoFeedValido(string filtroLinea, bool filtroTripVuoti, bool filtroTuttoPercorso, bool raggruppalineaRegola, bool nonRaggruppare)
        {
            Exception ecc = null;

            // micro-optim: local references
            var gtfs = GTFS_RSM;
            var staticTrips = (gtfs?.StaticData?.Trips) ?? Enumerable.Empty<Trip>();
            var staticStops = (gtfs?.StaticData?.Stops) ?? Enumerable.Empty<Stop>();
            var elencoDettagli = gtfs?.ElencoDettagliVettura ?? Enumerable.Empty<DettagliVettura>();
            var elencoLineaAgenzia = gtfs?.ElencoLineaAgenzia ?? Enumerable.Empty<LineaAgenzia>();

            try
            {
                // 1) Filtra entità valide una sola volta
                var feedEntities = LastValidFeed?.Entities;
                if (feedEntities == null)
                {
                    ElencoVetture = new List<ExtendedVehicleInfo>();
                    return null;
                }

                FeedEntities = feedEntities
                    .Where(x => !x.IsDeleted
                                && (string.IsNullOrEmpty(filtroLinea) || x.Vehicle?.Trip?.RouteId == filtroLinea)
                                && (!filtroTripVuoti || (x.Vehicle?.Trip?.TripId?.Length > 0)))
                    .ToList();

                // 2) Pre-build lookup dictionaries to avoid repeated O(N) searches
                var tripsById = staticTrips.ToDictionary(t => t.Id, t => t);
                var stopsByCode = staticStops.Where(s => !string.IsNullOrEmpty(s.Code)).ToDictionary(s => s.Code, s => s);
                // map routeId -> LineaAgenzia (first match)
                var lineaByRouteId = new Dictionary<string, LineaAgenzia>(StringComparer.OrdinalIgnoreCase);
                foreach (var la in elencoLineaAgenzia)
                {
                    var id = la?.Route?.Id;
                    if (string.IsNullOrEmpty(id)) continue;
                    if (!lineaByRouteId.ContainsKey(id))
                        lineaByRouteId[id] = la;
                }
                // dettagli per matricola, key trimmed uppercase to reduce collisions
                var dettagliByMat = new Dictionary<string, DettagliVettura>(StringComparer.OrdinalIgnoreCase);
                foreach (var d in elencoDettagli)
                {
                    var key = d?.Matricola?.Trim();
                    if (string.IsNullOrEmpty(key)) continue;
                    if (!dettagliByMat.ContainsKey(key))
                        dettagliByMat[key] = d;
                }

                // 3) Sanitize routeId for entities missing it (single pass)
                foreach (var entity in FeedEntities)
                {
                    var trip = entity.Vehicle?.Trip;
                    if (trip != null && string.IsNullOrEmpty(trip.RouteId))
                    {
                        if (tripsById.TryGetValue(trip.TripId, out Trip foundTrip))
                        {
                            trip.RouteId = foundTrip.RouteId ?? string.Empty;
                        }
                    }
                }

                // 4) Build new ElencoVetture efficiently
                var newList = new List<ExtendedVehicleInfo>(FeedEntities.Count)
                {
                    Capacity = FeedEntities.Count
                };

                foreach (var fe in FeedEntities)
                {
                    var vehiclePos = fe.Vehicle;
                    var mat = vehiclePos?.Vehicle?.Label?.Trim();
                    var tripId = vehiclePos?.Trip?.TripId;
                    var routeId = vehiclePos?.Trip?.RouteId;

                    LineaAgenzia lineaObj = null;
                    if (!string.IsNullOrEmpty(routeId))
                        lineaByRouteId.TryGetValue(routeId, out lineaObj);

                    DettagliVettura dettagli = null;
                    if (!string.IsNullOrEmpty(mat))
                        dettagliByMat.TryGetValue(mat, out dettagli);

                    Stop nomeFermata = null;
                    if (!string.IsNullOrEmpty(vehiclePos?.StopId))
                        stopsByCode.TryGetValue(vehiclePos.StopId, out nomeFermata);

                    Trip trip = null;
                    if (!string.IsNullOrEmpty(tripId))
                        tripsById.TryGetValue(tripId, out trip);

                    // compute linea string
                    string linea = string.Empty;
                    var r = lineaObj?.Route;
                    if (r != null)
                    {
                        if (r.ShortName == r.LongName || string.IsNullOrEmpty(r.LongName))
                            linea = r.ShortName;
                        else if (string.IsNullOrEmpty(r.ShortName))
                            linea = r.LongName;
                        else
                            linea = r.ShortName + " - " + r.LongName;
                    }

                    // tipo mezzo: prefer dettaglio, fallback to agency & route type
                    int tipoMezzo = 0;
                    if (dettagli != null && dettagli.TipoMezzoTrasporto.HasValue)
                    {
                        tipoMezzo = dettagli.TipoMezzoTrasporto.Value;
                    }
                    else
                    {
                        var agName = lineaObj?.Agency?.Name;
                        if (string.Equals(agName, "atac", StringComparison.OrdinalIgnoreCase))
                        {
                            if (r != null && r.Type == GTFS.Entities.Enumerations.RouteTypeExtended.TramService)
                                tipoMezzo = -1;
                            else
                                tipoMezzo = -2;
                        }
                        else
                        {
                            tipoMezzo = -3; // other
                        }
                    }

                    var ev = new ExtendedVehicleInfo(
                        idVettura: vehiclePos?.Vehicle?.Id,
                        matricola: mat,
                        licensePlate: vehiclePos?.Vehicle?.LicensePlate,
                        routeId: routeId,
                        linea: linea,
                        gestore: dettagli?.Gestore ?? lineaObj?.Agency?.Name?.ToUpper(),
                        directionId: (uint?)trip?.Direction ?? vehiclePos?.Trip?.DirectionId,
                        currentStopSequence: vehiclePos?.CurrentStopSequence ?? 0,
                        congestionLevel: vehiclePos?.congestion_level ?? VehiclePosition.CongestionLevel.UnknownCongestionLevel,
                        occupancyStatus: vehiclePos?.occupancy_status ?? 0,
                        tripId: tripId,
                        strict: filtroTripVuoti,
                        data: LastDataFeed.GetValueOrDefault(),
                        rimessa: dettagli?.Rimessa,
                        euro: dettagli?.Euro,
                        modello: dettagli?.Modello,
                        latitude: vehiclePos?.Position?.Latitude ?? 0,
                        longitude: vehiclePos?.Position?.Longitude ?? 0,
                        inTransitTo: vehiclePos.CurrentStatus,
                        tipoMezzoTrasporto: tipoMezzo,
                        distanzaPercorsa: vehiclePos?.Position?.Odometer ?? 0,
                        superStrictMode: filtroTuttoPercorso,
                        nomeFermata: nomeFermata?.Name,
                        destinazione: trip?.Headsign,
                        dataProgrammata: vehiclePos?.Trip?.StartDate,
                        oraProgrammata: vehiclePos?.Trip?.StartTime
                    );

                    newList.Add(ev);
                }

                // 5) Deduplicate by IdVettura + Matricola + TripId (prefer latest by UltimaVolta if duplicates)
                var dedupDict = new Dictionary<string, ExtendedVehicleInfo>(StringComparer.OrdinalIgnoreCase);
                foreach (var v in newList)
                {
                    var keyId = (v.IdVettura ?? string.Empty) + "|" + (v.Matricola ?? string.Empty) + "|" + (v.TripId ?? string.Empty);
                    ExtendedVehicleInfo exists;
                    if (!dedupDict.TryGetValue(keyId, out exists))
                    {
                        dedupDict[keyId] = v;
                    }
                    else
                    {
                        // keep the one with latest UltimaVolta (nulls considered older)
                        var exTime = exists.UltimaVolta ?? DateTime.MinValue;
                        var newTime = v.UltimaVolta ?? DateTime.MinValue;
                        if (newTime > exTime)
                            dedupDict[keyId] = v;
                    }
                }
                ElencoVetture = dedupDict.Values.OrderBy(x => x.IdVettura).ToList();

                // 6) Prepare lookup for aggregated existing vehicles to speed up updates
                var aggLookup = new Dictionary<string, ExtendedVehicleInfo>(StringComparer.OrdinalIgnoreCase);
                foreach (var agg in ElencoAggregatoVetture)
                {
                    var key = (agg.Matricola ?? string.Empty).Trim() + "|" + (agg.TripId ?? string.Empty);
                    ExtendedVehicleInfo cur;
                    if (!aggLookup.TryGetValue(key, out cur) || (agg.UltimaVolta ?? DateTime.MinValue) > (cur.UltimaVolta ?? DateTime.MinValue))
                        aggLookup[key] = agg;
                }

                // 7) Update per-vehicle details based on aggregated lookup (single pass)
                foreach (var vettura in ElencoVetture)
                {
                    var lookupKey = (vettura.Matricola ?? string.Empty).Trim() + "|" + (vettura.TripId ?? string.Empty);
                    ExtendedVehicleInfo presente;
                    if (aggLookup.TryGetValue(lookupKey, out presente))
                    {
                        if (presente.PartenzaEffettiva.HasValue)
                            vettura.PartenzaEffettiva = presente.PartenzaEffettiva;
                        else if (presente.CurrentStopSequence <= 1 &&
                                 presente.InTransitTo == VehiclePosition.VehicleStopStatus.StoppedAt &&
                                 vettura.CurrentStopSequence >= 1 &&
                                 vettura.InTransitTo == VehiclePosition.VehicleStopStatus.InTransitTo)
                        {
                            vettura.PartenzaEffettiva = presente.UltimaVolta.GetValueOrDefault().AddSeconds((vettura.UltimaVolta.GetValueOrDefault() - presente.PrimaVolta).Seconds);
                        }
                        else if (presente.CurrentStopSequence <= 3 && ElencoAggregatoVetture.Count > 0 && presente.InTransitTo != VehiclePosition.VehicleStopStatus.StoppedAt)
                        {
                            vettura.PartenzaEffettiva = presente.PrimaVolta;
                        }

                        vettura.PrimaVolta = presente.PrimaVolta;
                        vettura.OccupancyStatus = vettura.OccupancyStatus.CompareTo(presente.OccupancyStatus) >= 0 ? vettura.OccupancyStatus : presente.OccupancyStatus;
                    }
                }

                // 8) Merge into ElencoAggregatoVetture: keep existing aggregated plus new ones (update by Matricola+TripId)
                var newAggDict = new Dictionary<string, ExtendedVehicleInfo>(StringComparer.OrdinalIgnoreCase);
                // start from existing
                foreach (var a in ElencoAggregatoVetture)
                {
                    var k = (a.Matricola ?? string.Empty).Trim() + "|" + (a.TripId ?? string.Empty);
                    newAggDict[k] = a;
                }
                // add/update with new values (prefer newer UltimaVolta)
                foreach (var a in ElencoVetture)
                {
                    var k = (a.Matricola ?? string.Empty).Trim() + "|" + (a.TripId ?? string.Empty);
                    if (!newAggDict.TryGetValue(k, out ExtendedVehicleInfo cur) || (a.UltimaVolta ?? DateTime.MinValue) >= (cur.UltimaVolta ?? DateTime.MinValue))
                        newAggDict[k] = a;
                }
                ElencoAggregatoVetture = newAggDict.Values.ToList();

                // 9) Statistiche and totals - use efficient counting

                var group_RouteType2 = ElencoVetture
                    .Join(
                        GTFS_RSM.StaticData.Routes,
                        outerKeySelector: v => v.RouteId,
                        innerKeySelector: r => r.Id,
                        resultSelector: (v, r) => new { r })
                    .Join(
                        GTFS_RSM.StaticData.Agencies,
                        o => o.r.AgencyId,
                        a => a.Id,
                        (o, a) => new { a.Name, o.r.Type, a.Id })
                    .GroupBy(x => new { x.Id, x.Type, x.Name })
                    .Select(g => new ServizioRaggruppato
                    {
                        Agenzia = g.Key.Name,
                        Servizio = g.Key.Type.ToString(),
                        Tipo = g.Key.Type,
                        Num = g.Count()
                    })
                    .OrderBy(x => x.Agenzia)
                    .ThenByDescending(x => x.Num)
                    .ToList();

                StatisticheAttuali.ServizioRaggruppato = group_RouteType2;


                TotaleMatricola = ElencoAggregatoVetture.Select(i => (i.Matricola ?? string.Empty).Trim()).Distinct().Count();
                TotaleIdVettura = ElencoAggregatoVetture.Select(i => i.IdVettura).Distinct().Count();

                TotaleMatricolaAtac = ElencoAggregatoVetture
                    .Where(i => i.TipoMezzoTrasporto == 0 || i.TipoMezzoTrasporto == 1 || i.TipoMezzoTrasporto == 2 || i.TipoMezzoTrasporto == 5 || i.TipoMezzoTrasporto == 6 || i.TipoMezzoTrasporto == -2)
                    .Select(i => (i.Matricola ?? string.Empty).Trim())
                    .Distinct().Count();

                TotaleMatricolaTPL = ElencoAggregatoVetture
                    .Where(i => i.TipoMezzoTrasporto == 3 || i.TipoMezzoTrasporto == 4 || i.TipoMezzoTrasporto == -3)
                    .Select(i => (i.Matricola ?? string.Empty).Trim())
                    .Distinct().Count();

                // recompute StatisticheAttuali using efficient grouping (reuse ElencoVetture)
                StatisticheAttuali.RilevatoBusAtac = ElencoVetture.Count(x => x.TipoMezzoTrasporto == 0);
                StatisticheAttuali.RilevatoTramAtac = ElencoVetture.Count(x => x.TipoMezzoTrasporto == 1);
                StatisticheAttuali.RilevatoFilobusAtac = ElencoVetture.Count(x => x.TipoMezzoTrasporto == 2);
                StatisticheAttuali.RilevatoMinibusElettrici = ElencoVetture.Count(x => x.TipoMezzoTrasporto == 5);
                StatisticheAttuali.RilevatoFurgoncini = ElencoVetture.Count(x => x.TipoMezzoTrasporto == 6);
                StatisticheAttuali.RilevatoFerro = ElencoVetture.Count(x => x.TipoMezzoTrasporto == -1 || x.TipoMezzoTrasporto == 7);
                StatisticheAttuali.RilevatoAltroAtac = ElencoVetture.Count(x => x.TipoMezzoTrasporto == -2);
                StatisticheAttuali.RilevatoBusTpl = ElencoVetture.Count(x => x.TipoMezzoTrasporto == 3);
                StatisticheAttuali.RilevatoPullmanTpl = ElencoVetture.Count(x => x.TipoMezzoTrasporto == 4);
                StatisticheAttuali.RilevatoAltroTpl = ElencoVetture.Count(x => x.TipoMezzoTrasporto == -3);

                // 10) Detections comparing to previous list
                VettureTolte = ElencoPrecedente.Except(ElencoVetture, new ExtendedVehicleInfoComparer()).ToList();
                VettureAggiunte = ElencoVetture.Except(ElencoPrecedente, new ExtendedVehicleInfoComparer()).ToList();
                VettureTolte = ElencoPrecedente.Except(ElencoVetture).ToList();
                VettureAggiunte = ElencoVetture.Except(ElencoPrecedente).ToList();

                var aggiunteSet = new HashSet<string>(VettureAggiunte.Select(x => x.Matricola));
                VettureTolte = VettureTolte.Where(x => !aggiunteSet.Contains(x.Matricola)).ToList();
                VettureAggiunte = VettureAggiunte.Where(x => !VettureTolte.Select(t => t.Matricola).Contains(x.Matricola)).ToList();

                // compute fresh/riagganciate/percorsoAnomalo etc using optimized approaches (reuse dictionaries)
                if (ElencoPrecedente.Count > 0)
                {
                    var precLookup = ElencoPrecedente.ToDictionary(p => (p.Matricola ?? string.Empty) + "|" + (p.TripId ?? string.Empty), StringComparer.OrdinalIgnoreCase);
                    VettureFresche = ElencoVetture.Where(x => !precLookup.ContainsKey((x.Matricola ?? string.Empty) + "|" + (x.TripId ?? string.Empty))).ToList();

                    PartenzaAvanzata = VettureFresche.Where(x => x.CurrentStopSequence > 1 && !ElencoAggregatoVetture.Any(c => c.Matricola == x.Matricola && c.TripId == x.TripId)).ToList();

                    VettureRiagganciate = VettureFresche.Where(x => ElencoAggregatoVetture.Any(c => c.Matricola == x.Matricola && c.TripId == x.TripId)).ToList();

                    PercorsoAnomalo = (from act in ElencoVetture
                                       join prec in ElencoPrecedente on (act.Matricola, act.TripId) equals (prec.Matricola, prec.TripId)
                                       let delta = (int)(act.CurrentStopSequence - prec.CurrentStopSequence)
                                       where delta > 2 || delta < 0
                                       select new ErroriGTFS(act, delta))
                                      .ToList();
                    if (PercorsoAnomalo.Any())
                        AnomaliaGTFS.AddRange(PercorsoAnomalo);
                }

                // 11) Grafico sample
                int numVettureTPLFeedVehicle = ElencoVetture.Count(i => i.TipoMezzoTrasporto == 3 || i.TipoMezzoTrasporto == 4 || i.TipoMezzoTrasporto == -3);
                var listaMezziSuLinea = ElencoVetture.Where(x => x.TripId != null).ToList();
                var listaBusAttesa = ElencoVetture.Where(x => x.TripId == null).ToList();
                int busTotale = listaMezziSuLinea.Count + listaBusAttesa.Count;

                MonitoraggioVettureGrafico nuovoMonitoraggio = new MonitoraggioVettureGrafico
                {
                    DateTime = LastDataFeed.GetValueOrDefault(),
                    Aggregate = TotaleMatricola,
                    AggregateAtac = TotaleMatricolaAtac,
                    AggregateTPL = TotaleMatricolaTPL,
                    Rilevate = busTotale,
                    Atac = StatisticheAttuali.RilevatoBusAtac + StatisticheAttuali.RilevatoTramAtac + StatisticheAttuali.RilevatoFilobusAtac + StatisticheAttuali.RilevatoMinibusElettrici + StatisticheAttuali.RilevatoFurgoncini + StatisticheAttuali.RilevatoAltroAtac,
                    TPL = numVettureTPLFeedVehicle,
                    Aggiunte = ElencoPrecedente.Count > 0 ? VettureAggiunte.Count : 0,
                    Tolte = VettureTolte.Count
                };
                ElencoVettureGrafico.Add(nuovoMonitoraggio);

                // 12) Alerts and sovraffollamento (kept as original logic but using local collections)
                try
                {
                    foreach (var alert in GTFS_RSM.AlertsDaControllare)
                    {
                        var regoleAlertApplicabili = alert.RegoleAlert
                            .Where(r => r.Giorno.Contains(((int)LastDataFeed.GetValueOrDefault().DayOfWeek).ToString())
                                        && r.Da < LastDataFeed.GetValueOrDefault().TimeOfDay
                                        && LastDataFeed.GetValueOrDefault().TimeOfDay <= r.A.GetValueOrDefault(LastDataFeed.GetValueOrDefault().TimeOfDay));
                        var lineedaVerificareAlert = regoleAlertApplicabili
                            .GroupBy(r => r.Linea)
                            .Select(g => g.First().Linea);

                        var violazioni = new List<ViolazioneAlert>();
                        foreach (var linea in lineedaVerificareAlert)
                        {
                            violazioni = (from vettura in ElencoVetture
                                          from regolaAlert in regoleAlertApplicabili.Where(ra => ra.Linea == linea)
                                          where MatricolaToHexValue(regolaAlert.VetturaDa) <= MatricolaToHexValue(vettura.Matricola)
                                                && MatricolaToHexValue(vettura.Matricola) <= MatricolaToHexValue(regolaAlert.VetturaA)
                                          select new ViolazioneAlert(LastDataFeed, null, regolaAlert, vettura.Matricola))
                                         .ToList();

                            // star rules
                            var violazioniStar = (from vettura in ElencoVetture
                                                  from regolaAlert in regoleAlertApplicabili.Where(ra => ra.Linea == "*")
                                                  where MatricolaToHexValue(regolaAlert.VetturaDa) <= MatricolaToHexValue(vettura.Matricola)
                                                        && MatricolaToHexValue(vettura.Matricola) <= MatricolaToHexValue(regolaAlert.VetturaA)
                                                  select new ViolazioneAlert(LastDataFeed, null, new RegolaAlert(vettura.Linea, regolaAlert.Giorno, regolaAlert.Da, regolaAlert.A, regolaAlert.VetturaDa, regolaAlert.VetturaA), vettura.Matricola))
                                                 .ToList();

                            violazioni = violazioni.Union(violazioniStar).ToList();

                            // grouping logic preserved
                            if (raggruppalineaRegola)
                            {
                                violazioni = violazioni
                                    .GroupBy(x => new { x.Linea, x.Giorno, x.Da, x.A, x.VetturaDa, x.VetturaA })
                                    .Select(g => new ViolazioneAlert(LastDataFeed, null,
                                        new RegolaAlert(g.Key.Linea, g.Key.Giorno, g.Key.Da, g.Key.A, g.Key.VetturaDa, g.Key.VetturaA),
                                        string.Join(", ", g.Select(bn => bn.Violazione).ToList())))
                                    .ToList();
                            }

                            foreach (var violazione in violazioni)
                            {
                                if (nonRaggruppare)
                                {
                                    if (!alert.ViolazioniAlert.Any(x => x.Linea == violazione.Linea
                                                                       && x.Giorno == violazione.Giorno
                                                                       && x.Da == violazione.Da
                                                                       && x.A == violazione.A
                                                                       && x.VetturaDa == violazione.VetturaDa
                                                                       && x.VetturaA == violazione.VetturaA
                                                                       && x.Violazione == violazione.Violazione))
                                    {
                                        alert.ViolazioniAlert.Add(violazione);
                                    }
                                }
                                else if (raggruppalineaRegola)
                                {
                                    var es = alert.ViolazioniAlert.FirstOrDefault(x => x.Linea == violazione.Linea
                                                                                    && x.Giorno == violazione.Giorno
                                                                                    && x.Da == violazione.Da
                                                                                    && x.A == violazione.A
                                                                                    && x.VetturaDa == violazione.VetturaDa
                                                                                    && x.VetturaA == violazione.VetturaA);
                                    if (es == null) alert.ViolazioniAlert.Add(violazione);
                                    else
                                    {
                                        var existing = es.Violazione.Replace(" ", "").Split(',').ToList();
                                        var incoming = violazione.Violazione.Split(',').ToList();
                                        es.Violazione = string.Join(", ", existing.Union(incoming).OrderBy(q => q.Length).ThenBy(q => q));
                                    }
                                }
                                else
                                {
                                    var es = alert.ViolazioniAlert.FirstOrDefault(x => x.Linea == violazione.Linea);
                                    if (es == null) alert.ViolazioniAlert.Add(violazione);
                                    else
                                    {
                                        var existing = es.Violazione.Replace(" ", "").Split(',').ToList();
                                        var incoming = violazione.Violazione.Split(',').ToList();
                                        es.Violazione = string.Join(", ", existing.Union(incoming).OrderBy(q => q.Length).ThenBy(q => q));
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ecc = new Exception("Si è verificato un errore durante il controllo degli alert", ex);
                }

                // sovraffollamento (kept logic)
                var listaBusPieni = GetBusPieni();
                foreach (RunTimeValueAlert busPieno in listaBusPieni)
                {
                    var presente = ElencoVettureSovraffollate.FirstOrDefault(x =>
                        x.TripID == busPieno.TripID
                        && x.Matricola == busPieno.Matricola
                        && (busPieno.PrimaFermata - x.UltimaFermata <= 1));
                    if (presente != null)
                    {
                        ElencoVettureSovraffollate.Remove(presente);
                        busPieno.PrimaVolta = presente.PrimaVolta;
                        busPieno.PrimaFermata = presente.PrimaFermata;
                    }
                    ElencoVettureSovraffollate.Add(busPieno);
                }

                // media ponderata (kept but optimized by avoiding repeated enumeration)
                if (VettureAggiunte.Count > 0 || VettureTolte.Count > 0 || ElencoPrecedente.Count == 0 || ElencoVetture.Count > 0)
                {
                    int campioniNecessari = GTFS_RSM.CriteriMediaPonderata.Sum(x => x.NumeroCampioni);
                    if (ElencoVettureGrafico.Count >= campioniNecessari)
                    {
                        PonderateAtac = 0;
                        PonderateTPL = 0;
                        int startIndex = ElencoVettureGrafico.Count;
                        foreach (var item in GTFS_RSM.CriteriMediaPonderata)
                        {
                            int numeroCampioni = item.NumeroCampioni;
                            startIndex -= numeroCampioni;

                            var slice = ElencoVettureGrafico.Skip(startIndex).Take(numeroCampioni);
                            PonderateAtac += item.Peso / (double)numeroCampioni * slice.Sum(x => x.Atac);
                            PonderateTPL += item.Peso / (double)numeroCampioni * slice.Sum(x => x.TPL);
                        }
                    }
                }

                ElencoPrecedente = ElencoVetture;
            }
            catch (Exception ex)
            {
                // fallback: propagate as exception return
                return ex;
            }

            return ecc;
        }


        private int MatricolaToHexValue(string matricola)
        {
            //try
            //{
            return string.IsNullOrEmpty(matricola)
                ? 0
                : Convert.ToInt32(matricola
                        .Replace('A', 'A')
                        .Replace('R', 'B')
                        .Replace('T', 'C')
                        .Replace('M', 'D')
                        .Replace('V', 'E')
                        , 16);
            //}
            //catch (Exception)
            //{
            //    return 0;
            //}
        }

        public List<string> TripDuplicati() => (from trip in FeedEntities
                                                where trip.Vehicle.Trip?.TripId != null
                                                group trip by trip.Vehicle.Trip.TripId into grp
                                                where grp.Count() > 1
                                                select grp.Key)
                                              .ToList();

        internal void LeggiGTFS(string path)
        {
            GTFS_RSM = new GTFS_RSM(path);
        }

        public override void Reset()
        {
            base.Reset();
            ElencoLineeMonitorate.Clear();
            ElencoAggregatoVetture.Clear();
            ElencoPrecedente.Clear();
            ElencoVetture?.Clear();
            VettureTolte?.Clear();
            VettureAggiunte?.Clear();
            VettureFresche?.Clear();
            PartenzaAvanzata?.Clear();
            VettureRiagganciate?.Clear();
            TotaleMatricola = 0;
            TotaleIdVettura = 0;
            TotaleMatricolaAtac = 0;
            TotaleMatricolaTPL = 0;
            PonderateAtac = 0;
            PonderateTPL = 0;
            AnomaliaGTFS.Clear();
            ElencoVettureGrafico.Clear();
            foreach (var alert in GTFS_RSM.AlertsDaControllare)
            {
                alert.ViolazioniAlert.Clear();
            }
        }

        // Note: small helper equality comparer used above for Except operations.
        // Add this nested class somewhere accessible (e.g. inside this file/class)
        private class ExtendedVehicleInfoComparer : IEqualityComparer<ExtendedVehicleInfo>
        {
            public bool Equals(ExtendedVehicleInfo x, ExtendedVehicleInfo y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (x == null || y == null) return false;
                return string.Equals(x.Matricola, y.Matricola, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(x.TripId, y.TripId, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(x.IdVettura, y.IdVettura, StringComparison.OrdinalIgnoreCase);
            }

            public int GetHashCode(ExtendedVehicleInfo obj)
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 23 + (obj.Matricola ?? string.Empty).ToUpperInvariant().GetHashCode();
                    hash = hash * 23 + (obj.TripId ?? string.Empty).GetHashCode();
                    hash = hash * 23 + (obj.IdVettura ?? string.Empty).GetHashCode();
                    return hash;
                }
            }
        }
    }
}