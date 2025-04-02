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
        public Exception ElaboraUltimoFeedValido(string filtroLinea, bool filtroTripVuoti, bool filtroTuttoPercorso, bool raggruppalineaRegola, bool nonRaggruppare)
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

        private int MatricolaToHexValue(string matricola)
        {

            return string.IsNullOrEmpty(matricola)
                ? 0
                : Convert.ToInt32(matricola
                        .Replace('A', 'A')
                        .Replace('R', 'B')
                        .Replace('T', 'C')
                        .Replace('M', 'D')
                        , 16);
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
    }
}
