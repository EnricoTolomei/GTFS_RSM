using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using OfficeOpenXml.Table;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using static AtacFeed.TransitRealtime;

namespace AtacFeed
{
    /// <summary>
    /// Facade per orchestrare FeedVehicleManager e FeedAlertManager.
    /// Progettata per .NET Framework 4.8 / C# 7.3: API sincrone dei manager vengono eseguite in Task.Run per non bloccare la UI.
    /// Espone eventi per aggiornare la UI e metodi per polling automatico con cancellazione.
    /// </summary>
    public class FeedGTFSManager : IDisposable
    {
        private readonly object _sync = new object();
        private CancellationTokenSource _cts;
        private Task _pollingTask;
        private bool _disposed;
        public DateTime? DataResetMonitoraggio;

        private bool exportXlsx = true;
        private bool exportGrafico = true;
        private bool exportAlert = true;
        private bool exportAlertEnabled = true;
        private bool exportMonitoraggio = true;
        private bool exportAnomalieGTFS = true;
        private bool exportSovraffollamento = true;
        private bool exportCSV = false;
        private string fileName;

        private readonly FeedVehicleManager _vehicleManager;
        private FeedAlertManager _alertManager;

        public event EventHandler<Exception> ErrorOccurred;
        public event EventHandler<FeedMonitorSnapshotEventArgs> SnapshotUpdated;
        public class VehicleReadResultEventArgs : EventArgs
        {
            public string Url { get; }
            public bool Success { get; }
            public int Code { get; }
            public DateTime? Timestamp { get; }
            public Exception Error { get; }
            public bool IsRiserva { get; set; }

            public VehicleReadResultEventArgs(string url, bool success, int code, DateTime? timestamp, Exception error = null, bool isRiserva = false)
            {
                Url = url;
                Success = success;
                Code = code;
                Timestamp = timestamp;
                Error = error;
                IsRiserva = isRiserva;
            }
        }

        public event EventHandler<VehicleReadResultEventArgs> VehicleReadStarted;
        public event EventHandler<VehicleReadResultEventArgs> VehicleReadCompleted;
        public event EventHandler<VehicleReadResultEventArgs> VehicleElabStarted;
        public event EventHandler<VehicleReadResultEventArgs> VehicleElabCompleted;

        protected virtual void OnVehicleReadStarted(VehicleReadResultEventArgs url)
        {
            VehicleReadStarted?.Invoke(this, url);
        }
        protected virtual void OnVehicleReadCompleted(VehicleReadResultEventArgs args)
        {
            VehicleReadCompleted?.Invoke(this, args);
        }
        protected virtual void OnVehicleElabStarted(VehicleReadResultEventArgs url)
        {
            VehicleElabStarted?.Invoke(this, url);
        }

        protected virtual void OnVehicleElabCompleted(VehicleReadResultEventArgs args)
        {
            VehicleElabCompleted?.Invoke(this, args);
        }

        public int NumeroLetture { get; set; } = 0;
        public int NumeroFeedValidi { get; set; } = 0;
        public FeedGTFSManager()
        {
            _vehicleManager = new FeedVehicleManager();
            _alertManager = new FeedAlertManager();
        }

        /// <summary>
        /// Applica le opzioni di export/configurazione provenienti dalla UI/Settings.
        /// Questo permette di centralizzare il comportamento di salvataggio/esportazione nel manager.
        /// </summary>
        public void ConfigureExports(
            bool? saveXlsx = null,
            bool? saveGrafico = null,
            bool? saveAlert = null,
            bool? saveMonitoraggio = null,
            bool? saveAnomalieGTFS = null,
            bool? saveSovraffollamento = null,
            bool? saveCSV = null,
            bool alertEnabled = true)
        {
            exportXlsx = saveXlsx ?? exportXlsx;
            exportGrafico = saveGrafico ?? exportGrafico;
            exportAlert = saveAlert ?? exportAlert;
            exportMonitoraggio = saveMonitoraggio ?? exportMonitoraggio;
            exportAnomalieGTFS = saveAnomalieGTFS ?? exportAnomalieGTFS;
            exportSovraffollamento = saveSovraffollamento ?? exportSovraffollamento;
            exportCSV = saveCSV ?? exportCSV;
            exportAlertEnabled = alertEnabled;
        }

        /// <summary>
        /// Esegue il refresh dei due feed in parallelo (legate ai manager esistenti).
        /// Le chiamate ai manager sono sincrone, quindi vengono eseguite in Task.Run per non bloccare il chiamante.
        /// Restituisce quando entrambe le operazioni sono concluse o viene sollevata un'eccezione.
        /// </summary>
        public async Task RefreshAsync(
            string urlVehicle,
            string urlVehicleRiserva = "",
            string urlAlert = "", 
            string filtroLinea = "",
            bool filtroTripVuoti = true,
            bool filtroTuttoPercorso = false, 
            bool raggruppalineaRegola = false,
            bool nonRaggruppare = false,            
            CancellationToken cancellation = default
            )
        {
            cancellation.ThrowIfCancellationRequested();
            bool success = false;
            bool alertSuccess = false;
            try
            {
                // Esegui le letture in parallelo su threadpool per non bloccare la UI.
                var vehicleTask = Task.Run(() =>
                {
                    Exception lastException = null;
                    string attemptedUrl = null;
                    List<string> urlsVehicle = new List<string> { urlVehicle, urlVehicleRiserva };
                    try
                    {
                        NumeroLetture++;

                        for (int idx = 0; !success && idx < urlsVehicle?.Count; idx++)
                        {
                            string url = urlsVehicle[idx];
                            try
                            {
                                attemptedUrl = url;
                                OnVehicleReadStarted(new VehicleReadResultEventArgs(url: attemptedUrl, success: false, code: -100, timestamp: null, isRiserva: idx > 0));
                                Task.Delay(500).Wait(); // breve delay per permettere alla UI di aggiornare lo stato di "lettura in corso" prima di bloccare con la lettura sincrona
                                _vehicleManager.LeggiFeedValido(attemptedUrl);
                                if (_vehicleManager.CodeFeed == 0)
                                {
                                    success = true;
                                    OnVehicleReadCompleted(new VehicleReadResultEventArgs(attemptedUrl, true, _vehicleManager.CodeFeed, _vehicleManager.LastDataFeed, isRiserva: idx > 0));
                                    break; // esci dal ciclo se abbiamo successo
                                }
                                else
                                {
                                    // segnalazione completamento non-successo per il tentativo
                                    OnVehicleReadCompleted(new VehicleReadResultEventArgs(attemptedUrl, false, _vehicleManager.CodeFeed, _vehicleManager.LastDataFeed, isRiserva: idx > 0));
                                }
                            }
                            catch (Exception ex)
                            {
                                lastException = ex;
                                try { OnVehicleReadCompleted(new VehicleReadResultEventArgs(url, false, _vehicleManager?.CodeFeed ?? -999, _vehicleManager?.LastDataFeed, ex, isRiserva: idx > 0)); } catch { }
                                OnError(ex);
                            }
                        }
                        
                        // Se abbiamo avuto successo su uno dei due URL, allora elaboriamo l'ultimo feed valido
                        if (success)
                        {
                            OnVehicleElabStarted(new VehicleReadResultEventArgs(attemptedUrl, true, _vehicleManager.CodeFeed, _vehicleManager.LastDataFeed));
                            _vehicleManager.ElaboraUltimoFeedValido(
                                filtroLinea: filtroLinea,
                                filtroTripVuoti: filtroTripVuoti,
                                filtroTuttoPercorso: filtroTuttoPercorso,
                                raggruppalineaRegola: raggruppalineaRegola,
                                nonRaggruppare: nonRaggruppare);

                            NumeroFeedValidi++;
                            if (string.IsNullOrEmpty(fileName))
                            {
                                fileName = string.Format("NEWFeed_{0:yyyy-MM-dd (HH_mm_ss)}", _vehicleManager.LastDataFeed);
                            }
                            //Task.Delay(1500).Wait(); // breve delay per permettere alla UI di aggiornare lo stato di "lettura in corso" prima di bloccare con la lettura sincrona
                            OnVehicleElabCompleted(new VehicleReadResultEventArgs(attemptedUrl, true, _vehicleManager.CodeFeed, _vehicleManager.LastDataFeed));
                        }
                        else
                        {
                            // Nessun feed valido: rilanciamo l'ultima eccezione se presente, altrimenti generiamo una generica
                            if (lastException != null)
                                throw lastException;
                            else
                                throw new Exception("Nessuna lettura valida dal feed vehicle (principale e riserva).");
                        }
                    }
                    catch (Exception ex)
                    {
                        // assicurati di notificare l'errore e rilanciare per far fallire Task.WhenAll
                        OnError(ex);
                        throw;
                    }
                }, cancellation);
                
                var alertTask = Task.Run(() =>
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(urlAlert))
                        {
                            _alertManager.LeggiFeedValido(urlAlert);
                            if (_alertManager.CodeFeed == 0)
                            {
                                alertSuccess = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        OnError(ex);
                    }
                }, cancellation);                
                
                await Task.WhenAll(vehicleTask, alertTask).ConfigureAwait(false);
                if (success || alertSuccess)
                {
                    OnSnapshotUpdated(GetSnapshot());
                }
                // avvia esportazione in background (non bloccare)
                if (success)
                {                    
                    _ = ExportGrid();
                }
            }
            catch (OperationCanceledException)
            {
                // Non raise evento; chiamante gestisce la cancellazione
                throw;
            }
            catch (Exception)
            {
                // Già notificato nei singoli task; rilancio per informare il chiamante
                throw;
            }
        }

        /// <summary>
        /// Crea una copia dei dati correnti destinati alla UI senza esporre i manager interni.
        /// Le collezioni e i relativi elementi vengono copiati in DTO indipendenti dai modelli GTFS.
        /// </summary>
        public FeedMonitorSnapshot GetSnapshot()
        {
            FeedVehicleManager feedVehicleManager = _vehicleManager;
            FeedAlertManager feedAlertManager = _alertManager;
            List<ExtendedVehicleInfo> currentVehicles = feedVehicleManager?.ElencoVetture;
            List<ExtendedVehicleInfo> aggregatedVehicles = feedVehicleManager?.ElencoAggregatoVetture;

            int totalVehicles = currentVehicles?.Count ?? 0;
            int totalTplVehicles = currentVehicles?.Count(vehicle =>
                vehicle.TipoMezzoTrasporto == 3 || vehicle.TipoMezzoTrasporto == 4 || vehicle.TipoMezzoTrasporto == -3) ?? 0;

            return new FeedMonitorSnapshot
            {
                NumeroLetture = NumeroLetture,
                NumeroFeedValidi = NumeroFeedValidi,
                UltimoFeed = feedVehicleManager?.LastDataFeed,
                CodiceFeedVeicoli = feedVehicleManager?.CodeFeed ?? -1,
                CodiceFeedAvvisi = feedAlertManager?.CodeFeed ?? -1,
                MonitoraggioAttivo = IsMonitoring,

                TotaleRighe = aggregatedVehicles?.Count ?? 0,
                TotaleIdVettura = feedVehicleManager?.TotaleIdVettura ?? 0,
                TotaleMatricola = feedVehicleManager?.TotaleMatricola ?? 0,
                TotaleMatricolaAtac = feedVehicleManager?.TotaleMatricolaAtac ?? 0,
                TotaleMatricolaTpl = feedVehicleManager?.TotaleMatricolaTPL ?? 0,
                TotaleVettureAtac = totalVehicles - totalTplVehicles,
                TotaleVettureTpl = totalTplVehicles,
                TotaleVetture = totalVehicles,
                PonderateAtac = feedVehicleManager?.PonderateAtac ?? 0,
                PonderateTpl = feedVehicleManager?.PonderateTPL ?? 0,

                Statistiche = MapStatistics(feedVehicleManager?.StatisticheAttuali),
                VeicoliAttuali = MapVehicles(currentVehicles),
                VeicoliAggregati = MapVehicles(aggregatedVehicles),
                VeicoliAggiunti = MapVehicles(feedVehicleManager?.VettureAggiunte),
                VeicoliRimossi = MapVehicles(feedVehicleManager?.VettureTolte),
                ProblemiFeed = MapFeedIssues(feedVehicleManager),
                AnomalieGtfs = MapVehicleAnomalies(feedVehicleManager),
                Avvisi = MapNotices(feedAlertManager),
                GruppiAlert = MapAlertGroups(feedVehicleManager),
                ViolazioniMonitoraggio = MapMonitorViolations(feedVehicleManager),
                VettureSovraffollate = MapOvercrowding(feedVehicleManager?.ElencoVettureSovraffollate),
                PuntiGrafico = MapChartPoints(feedVehicleManager?.ElencoVettureGrafico),
                ServiziPerGestore = MapServiceStatistics(feedVehicleManager?.StatisticheAttuali)
            };
        }

        /// <summary>
        /// Carica i dati statici e le configurazioni correlate, restituendo solo dati di presentazione.
        /// </summary>
        public FeedConfigurationDto LoadConfiguration(string suffixDettagliVettura)
        {
            string monitorRulesFile = Path.Combine("Config", "MonitoraggioLinee", "RegoleMonitoraggio_yes.txt");
            string alertRulesPath = Path.Combine("Config", "Regole_Alert");
            string weightedCriteriaFile = Path.Combine("Config", "CriterioMediaPonderata.txt");
            var vehicleManager = _vehicleManager;
            try
            {
                vehicleManager.GTFS_RSM = new GTFS_RSM(Path.Combine("Config", "GTFS_Static"), suffixDettagliVettura);
            }
            catch (Exception ex)
            {
                Log.Information(ex, "Nessun GTFS statico");
            }

            try
            {
                vehicleManager.GTFS_RSM.LeggiRegoleMonitoraggio(monitorRulesFile);
            }
            catch (Exception ex)
            {
                Log.Information(ex, "Nessuna RegoleMonitoraggio");
            }

            bool alertsLoaded = vehicleManager.GTFS_RSM.LeggiAlertDaControllare(alertRulesPath);
            int weightedCriteriaResult = vehicleManager.GTFS_RSM.LeggiCriteriMediaPonderata(weightedCriteriaFile);
            return MapConfiguration(vehicleManager.GTFS_RSM, alertsLoaded, weightedCriteriaResult);
        }

        /// <summary>
        /// Aggiorna i dettagli vettura secondo l'opzione selezionata nella UI.
        /// </summary>
        public void ConfigureVehicleDetails(string suffixDettagliVettura)
        {
            _vehicleManager?.GTFS_RSM?.LeggiDettagliVettura(suffixDettagliVettura);
        }

        public FeedConfigurationDto ReloadAlertConfiguration(string alertRulesPath, string weightedCriteriaFile)
        {
            var gtfs = _vehicleManager?.GTFS_RSM;
            if (gtfs == null)
                return new FeedConfigurationDto();

            bool alertsLoaded = gtfs.LeggiAlertDaControllare(alertRulesPath);
            int weightedCriteriaResult = gtfs.LeggiCriteriMediaPonderata(weightedCriteriaFile);
            return MapConfiguration(gtfs, alertsLoaded, weightedCriteriaResult);
        }

        private static FeedConfigurationDto MapConfiguration(GTFS_RSM gtfs, bool alertsLoaded, int weightedCriteriaResult)
        {
            var result = new FeedConfigurationDto
            {
                LetturaAlertRiuscita = alertsLoaded,
                EsitoCriteriMediaPonderata = weightedCriteriaResult
            };

            if (gtfs == null)
                return result;

            var routes = gtfs.StaticData?.Routes?.ToList() ?? new List<GTFS.Entities.Route>();
            var routeOptions = routes
                .OrderBy(route => route.ShortName)
                .Distinct()
                .Select(route => new RouteOptionDto
                {
                    Id = route.Id,
                    NomeVisualizzato = FormatRouteName(route.ShortName, route.LongName)
                })
                .ToList();
            routeOptions.Insert(0, new RouteOptionDto { Id = "-1", NomeVisualizzato = "    Tutte" });
            result.LineeDisponibili = routeOptions;

            var agencyRoutes = (gtfs.ElencoLineaAgenzia ?? new List<LineaAgenzia>())
                .Where(item => item.Agency != null && !string.IsNullOrEmpty(item.Agency.Name))
                .Select(item => new AgencyRouteDto
                {
                    Gestore = item.Agency.Name,
                    Linea = FormatRouteName(item.Route?.ShortName, item.Route?.LongName),
                    CodiceTipoTrasporto = item.Route == null ? 0 : (int)item.Route.Type,
                    TipoTrasporto = item.Route == null ? "Sconosciuto" : item.Route.Type.ToString()
                })
                .ToList();
            result.LineePerGestore = agencyRoutes;

            int totalRoutes = gtfs.ElencoLineaAgenzia?.Count ?? 0;
            var countsByAgency = agencyRoutes
                .GroupBy(item => item.Gestore)
                .ToDictionary(group => group.Key, group => group.Count());
            result.DistribuzioneGestori = (gtfs.StaticData?.Agencies ?? Enumerable.Empty<GTFS.Entities.Agency>())
                .Select(agency =>
                {
                    int count;
                    countsByAgency.TryGetValue(agency.Name, out count);
                    return new AgencyDistributionDto
                    {
                        Gestore = agency.Name,
                        NumeroCorse = count,
                        PercentualeCorse = totalRoutes == 0 ? 0 : count / (decimal)totalRoutes,
                        TotaleCorse = totalRoutes
                    };
                })
                .ToList();

            result.RegoleMonitoraggio = (gtfs.RegoleMonitoraggio ?? new List<RegolaMonitoraggio>())
                .Select(rule => new MonitorViolationDto
                {
                    Linea = rule.Linea,
                    Giorno = rule.Giorno,
                    Da = rule.Da,
                    A = rule.A,
                    TempoBonus = rule.TempoBonus,
                    VetturePreviste = rule.VetturePreviste
                })
                .ToList();

            result.GruppiAlert = (gtfs.AlertsDaControllare ?? new List<AlertDaControllare>())
                .Select(alert => new AlertRuleGroupDto
                {
                    Nome = alert.Name,
                    Regole = alert.RegoleAlert == null
                        ? new List<AlertViolationDto>()
                        : alert.RegoleAlert.Select(MapAlertRule).ToList()
                })
                .ToList();

            return result;
        }

        private static string FormatRouteName(string shortName, string longName)
        {
            if (shortName == longName || string.IsNullOrEmpty(longName))
                return shortName;
            if (string.IsNullOrEmpty(shortName))
                return longName;
            return shortName + " - " + longName;
        }

        private static IList<VehicleSnapshotDto> MapVehicles(IEnumerable<ExtendedVehicleInfo> vehicles)
        {
            return vehicles == null
                ? new List<VehicleSnapshotDto>()
                : vehicles.Where(vehicle => vehicle != null).Select(MapVehicle).ToList();
        }

        private static VehicleSnapshotDto MapVehicle(ExtendedVehicleInfo vehicle)
        {
            return new VehicleSnapshotDto
            {
                IdVettura = vehicle.IdVettura,
                Matricola = vehicle.Matricola,
                LicensePlate = vehicle.LicensePlate,
                RouteId = vehicle.RouteId,
                Linea = vehicle.Linea,
                Gestore = vehicle.Gestore,
                DirectionId = vehicle.DirectionId,
                Destinazione = vehicle.Destinazione,
                CurrentStopSequence = vehicle.CurrentStopSequence,
                CongestionLevel = vehicle.CongestionLevel.ToString(),
                OccupancyStatus = vehicle.OccupancyStatus.ToString(),
                TripId = vehicle.TripId,
                PrimaVolta = vehicle.PrimaVolta,
                UltimaVolta = vehicle.UltimaVolta,
                Rimessa = vehicle.Rimessa,
                Euro = vehicle.Euro,
                Modello = vehicle.Modello,
                Latitude = vehicle.Latitude,
                Longitude = vehicle.Longitude,
                InTransitTo = vehicle.InTransitTo.ToString(),
                TipoMezzoTrasporto = vehicle.TipoMezzoTrasporto,
                DistanzaPercorsa = vehicle.DistanzaPercorsa,
                NomeFermata = vehicle.NomeFermata,
                PartenzaProgrammata = vehicle.PartenzaProgrammata,
                PartenzaEffettiva = vehicle.PartenzaEffettiva
            };
        }

        private static FeedStatisticsDto MapStatistics(Statistiche statistics)
        {
            if (statistics == null)
                return new FeedStatisticsDto();

            return new FeedStatisticsDto
            {
                RilevatoBusAtac = statistics.RilevatoBusAtac,
                RilevatoTramAtac = statistics.RilevatoTramAtac,
                RilevatoFilobusAtac = statistics.RilevatoFilobusAtac,
                RilevatoMinibusElettrici = statistics.RilevatoMinibusElettrici,
                RilevatoFurgoncini = statistics.RilevatoFurgoncini,
                RilevatoFerro = statistics.RilevatoFerro,
                RilevatoAltroAtac = statistics.RilevatoAltroAtac,
                RilevatoBusTpl = statistics.RilevatoBusTpl,
                RilevatoPullmanTpl = statistics.RilevatoPullmanTpl,
                RilevatoAltroTpl = statistics.RilevatoAltroTpl
            };
        }

        private static IList<ServiceStatisticDto> MapServiceStatistics(Statistiche statistics)
        {
            return statistics?.ServizioRaggruppato == null
                ? new List<ServiceStatisticDto>()
                : statistics.ServizioRaggruppato.Select(item => new ServiceStatisticDto
                {
                    Gestore = item.Agenzia,
                    Servizio = item.Servizio,
                    Numero = item.Num
                }).ToList();
        }

        private static IList<FeedIssueDto> MapFeedIssues(FeedVehicleManager vehicleManager)
        {
            var issues = new List<FeedIssueDto>();
            if (vehicleManager == null)
                return issues;

            if (vehicleManager.FeedEntities != null && vehicleManager.GTFS_RSM?.ElencoLineaAgenzia != null)
            {
                foreach (string routeId in vehicleManager.LineeAnomale())
                {
                    issues.Add(new FeedIssueDto
                    {
                        Categoria = "Linea non presente nel GTFS statico",
                        Messaggio = routeId,
                        Dettaglio = "La linea rilevata non è riportata in routes.txt."
                    });
                }

                var duplicateTrips = vehicleManager.FeedEntities
                    .Where(entity => entity?.Vehicle?.Trip?.TripId != null)
                    .GroupBy(entity => entity.Vehicle.Trip.TripId)
                    .Where(group => group.Count() > 1);

                foreach (var group in duplicateTrips)
                {
                    string vehicleLabels = string.Join(", ", group
                        .Where(entity => entity.Vehicle?.Vehicle != null)
                        .Select(entity => entity.Vehicle.Vehicle.Label));

                    issues.Add(new FeedIssueDto
                    {
                        Categoria = "Trip duplicato",
                        Messaggio = group.Key,
                        Dettaglio = vehicleLabels
                    });
                }
            }

            if (vehicleManager.ElencoVetture != null)
            {
                foreach (ExtendedVehicleInfo vehicle in vehicleManager.VettureSenzaMatricola())
                {
                    issues.Add(new FeedIssueDto
                    {
                        Categoria = "Vettura senza matricola",
                        Messaggio = vehicle.IdVettura,
                        Dettaglio = vehicle.Matricola
                    });
                }
            }

            return issues;
        }

        private static IList<VehicleAnomalyDto> MapVehicleAnomalies(FeedVehicleManager vehicleManager)
        {
            var anomalies = new List<VehicleAnomalyDto>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            if (vehicleManager == null)
                return anomalies;

            Action<ErroriGTFS, string> addAnomaly = (anomaly, category) =>
            {
                if (anomaly == null)
                    return;

                string key = string.Join("|", category ?? string.Empty, anomaly.Matricola ?? string.Empty, anomaly.TripId ?? string.Empty,
                    anomaly.CurrentStopSequence.ToString(), anomaly.Delta.ToString());
                if (!seen.Add(key))
                    return;

                anomalies.Add(new VehicleAnomalyDto
                {
                    Categoria = category,
                    Matricola = anomaly.Matricola,
                    Linea = anomaly.Linea,
                    OraViolazione = anomaly.PrimaVolta,
                    TripId = anomaly.TripId,
                    Fermata = anomaly.CurrentStopSequence,
                    Delta = anomaly.Delta
                });
            };

            foreach (ErroriGTFS anomaly in vehicleManager.AnomaliaGTFS ?? new List<ErroriGTFS>())
                addAnomaly(anomaly, "Anomalia GTFS");

            foreach (ExtendedVehicleInfo vehicle in vehicleManager.PartenzaAvanzata ?? new List<ExtendedVehicleInfo>())
                addAnomaly(new ErroriGTFS(vehicle, (int)vehicle.CurrentStopSequence), "Partenza avanzata");

            foreach (ExtendedVehicleInfo vehicle in vehicleManager.VettureRiagganciate ?? new List<ExtendedVehicleInfo>())
            {
                uint? lastDetectedStop = vehicleManager.ElencoAggregatoVetture?
                    .Where(item => item.TripId == vehicle.TripId && item.Matricola == vehicle.Matricola)
                    .Select(item => (uint?)item.CurrentStopSequence)
                    .Max();

                if (lastDetectedStop.HasValue)
                    addAnomaly(new ErroriGTFS(vehicle, (int)vehicle.CurrentStopSequence - (int)lastDetectedStop.Value), "Vettura riagganciata");
            }

            foreach (ErroriGTFS anomaly in vehicleManager.PercorsoAnomalo ?? new List<ErroriGTFS>())
                addAnomaly(anomaly, "Percorso anomalo");

            return anomalies;
        }

        private static IList<FeedNoticeDto> MapNotices(FeedAlertManager alertManager)
        {
            if (alertManager == null || alertManager.CodeFeed != 0)
                return new List<FeedNoticeDto>();

            List<Avviso> notices = alertManager.Avvisi;
            return notices == null
                ? new List<FeedNoticeDto>()
                : notices.Select(notice => new FeedNoticeDto
                {
                    Id = notice.Id,
                    ValidoDal = notice.ValidoDal,
                    ValidoAl = notice.ValidoAl,
                    LineeCoinvolte = notice.LineeCoinvolte,
                    Titolo = notice.Titolo,
                    Descrizione = notice.Descrizione,
                    Cause = notice.Cause,
                    Effect = notice.Effect
                }).ToList();
        }

        private static IList<AlertRuleGroupDto> MapAlertGroups(FeedVehicleManager vehicleManager)
        {
            var groups = new List<AlertRuleGroupDto>();
            List<AlertDaControllare> configuredAlerts = vehicleManager?.GTFS_RSM?.AlertsDaControllare;
            if (configuredAlerts == null)
                return groups;

            foreach (AlertDaControllare alert in configuredAlerts)
            {
                groups.Add(new AlertRuleGroupDto
                {
                    Nome = alert.Name,
                    Regole = alert.RegoleAlert == null
                        ? new List<AlertViolationDto>()
                        : alert.RegoleAlert.Select(MapAlertRule).ToList(),
                    ViolazioniStoriche = MapAlertViolations(alert.ViolazioniAlert),
                    ViolazioniAttuali = MapAlertViolations(vehicleManager.ViolazioniAlertAttuali)
                });
            }

            return groups;
        }

        private static AlertViolationDto MapAlertRule(RegolaAlert rule)
        {
            return new AlertViolationDto
            {
                Linea = rule.Linea,
                Giorno = rule.Giorno,
                Da = rule.Da,
                A = rule.A,
                VetturaDa = rule.VetturaDa,
                VetturaA = rule.VetturaA
            };
        }

        private static IList<AlertViolationDto> MapAlertViolations(IEnumerable<ViolazioneAlert> violations)
        {
            return violations == null
                ? new List<AlertViolationDto>()
                : violations.Where(violation => violation != null).Select(violation => new AlertViolationDto
                {
                    Linea = violation.Linea,
                    Giorno = violation.Giorno,
                    Da = violation.Da,
                    A = violation.A,
                    VetturaDa = violation.VetturaDa,
                    VetturaA = violation.VetturaA,
                    Violazione = violation.Violazione,
                    OraPrimaViolazione = violation.OraPrimaViolazione,
                    OraUltimaViolazione = violation.OraUltimaViolazione
                }).ToList();
        }

        private static IList<MonitorViolationDto> MapMonitorViolations(FeedVehicleManager vehicleManager)
        {
            if (vehicleManager?.ElencoLineeMonitorate == null)
                return new List<MonitorViolationDto>();

            return vehicleManager.ViolazioniLeneeMonitorate().Select(violation => new MonitorViolationDto
            {
                Linea = violation.Linea,
                Giorno = violation.Giorno,
                Da = violation.Da,
                A = violation.A,
                TempoBonus = violation.TempoBonus,
                VetturePreviste = violation.VetturePreviste,
                VettureRilevate = violation.VettureRilevate,
                OraPrimaViolazione = violation.OraPrimaViolazione,
                OraUltimaViolazione = violation.OraUltimaViolazione
            }).ToList();
        }

        private static IList<OvercrowdingDto> MapOvercrowding(IEnumerable<RunTimeValueAlert> vehicles)
        {
            return vehicles == null
                ? new List<OvercrowdingDto>()
                : vehicles.Where(vehicle => vehicle != null).Select(vehicle => new OvercrowdingDto
                {
                    TripId = vehicle.TripID,
                    Linea = vehicle.Linea,
                    Matricola = vehicle.Matricola,
                    OccupancyStatus = vehicle.OccupancyStatus.ToString(),
                    PrimaFermata = vehicle.PrimaFermata,
                    PrimaVolta = vehicle.PrimaVolta,
                    UltimaFermata = vehicle.UltimaFermata,
                    UltimaVolta = vehicle.UltimaVolta
                }).ToList();
        }

        private static IList<MonitorChartPointDto> MapChartPoints(IEnumerable<MonitoraggioVettureGrafico> points)
        {
            return points == null
                ? new List<MonitorChartPointDto>()
                : points.Where(point => point != null).Select(point => new MonitorChartPointDto
                {
                    DataOra = point.DateTime,
                    TotaleAggregato = point.Aggregate,
                    AggregatoAtac = point.AggregateAtac,
                    AggregatoTpl = point.AggregateTPL,
                    Atac = point.Atac,
                    Tpl = point.TPL,
                    Rilevate = point.Rilevate,
                    Aggiunte = point.Aggiunte,
                    Rimosse = point.Tolte
                }).ToList();
        }

        /// <summary>
        /// Avvia polling periodico asincrono. Se è già in esecuzione la chiamata non fa nulla.
        /// </summary>
        public void StartAutoRefresh(string urlVehicle, string urlVehicleRiserva, string urlAlert, string filtroLinea, bool filtroTripVuoti,
            bool filtroTuttoPercorso, bool raggruppalineaRegola, bool nonRaggruppare,
            int intervalMilliseconds, bool forzaResetAcquisizione)
        {
            if (intervalMilliseconds <= 0)
                throw new ArgumentOutOfRangeException("intervalMilliseconds");

            lock (_sync)
            {
                if (_pollingTask != null && !_pollingTask.IsCompleted)
                {
                    // già in esecuzione
                    return;
                }

                _cts = new CancellationTokenSource();
                CancellationToken ct = _cts.Token;

                _pollingTask = Task.Run(async () =>
                {
                    while (!ct.IsCancellationRequested)
                    {
                        try
                        {
                            if (forzaResetAcquisizione || DataResetMonitoraggio.HasValue && DateTime.Now > DataResetMonitoraggio.GetValueOrDefault())
                            {
                                //RestartFile();
                                fileName = string.Empty;
                                Reset();

                                DataResetMonitoraggio = DataResetMonitoraggio.GetValueOrDefault(DateTime.MinValue).AddDays(1);
                                Log.Information("Prossimo reset monitoraggio: {DataResetMonitoraggio:dd/MM/yyyy HH:mm:ss}", DataResetMonitoraggio);
                            }

                            await RefreshAsync(
                                urlVehicle: urlVehicle,
                                urlVehicleRiserva: urlVehicleRiserva,
                                urlAlert: urlAlert,
                                filtroLinea: filtroLinea,
                                filtroTripVuoti: filtroTripVuoti,
                                filtroTuttoPercorso: filtroTuttoPercorso,
                                raggruppalineaRegola: raggruppalineaRegola,
                                nonRaggruppare: nonRaggruppare,
                                ct).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException)
                        {
                            // uscita pulita
                            break;
                        }
                        catch (Exception ex)
                        {
                            // Notifica e continua (evita che un errore blocchi il polling)
                            OnError(ex);
                        }

                        try
                        {
                            await Task.Delay(intervalMilliseconds, ct).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                    }
                }, ct);
            }
        }

        /// <summary>
        /// Ferma il polling automatico (se in esecuzione).
        /// </summary>
        public void StopAutoRefresh()
        {
            lock (_sync)
            {
                if (_cts != null && !_cts.IsCancellationRequested)
                {
                    try
                    {
                        _cts.Cancel();
                    }
                    catch
                    {
                        // ignored
                    }
                }
                _pollingTask = null;
                _cts = null;
            }
        }

        /// <summary>
        /// Indica se è in corso un monitoraggio automatico (polling).
        /// Restituisce true quando esiste un task di polling attivo e non è stato cancellato.
        /// </summary>
        public bool IsMonitoring
        {
            get
            {
                lock (_sync)
                {
                    return _pollingTask != null && !_pollingTask.IsCompleted && _cts != null && !_cts.IsCancellationRequested;
                }
            }
        }

        /// <summary>
        /// Restituisce una copia sicura (shallow) dell'elenco aggregato delle vetture.
        /// Utile per binding o snapshot UI senza esporre la lista interna.
        /// </summary>
        public List<ExtendedVehicleInfo> GetAggregatedVehiclesSnapshot()
        {
            var src = _vehicleManager.ElencoAggregatoVetture;
            if (src == null) return new List<ExtendedVehicleInfo>();
            lock (_sync)
            {
                return new List<ExtendedVehicleInfo>(src);
            }
        }

        /// <summary>
        /// Restituisce copia delle feed entities.
        /// </summary>
        public List<FeedEntity> GetFeedEntitiesSnapshot()
        {
            var src = _vehicleManager.FeedEntities;
            if (src == null) return new List<FeedEntity>();
            lock (_sync)
            {
                return new List<FeedEntity>(src);
            }
        }

        /// <summary>
        /// Restituisce copia degli alert gestiti.
        /// </summary>
        public List<AlertDaControllare> GetAlertsSnapshot()
        {
            var src = _vehicleManager.GTFS_RSM?.AlertsDaControllare;
            if (src == null) 
                return new List<AlertDaControllare>();
            lock (_sync)
            {
                return new List<AlertDaControllare>(src);
            }
        }

        public void Reset()
        {
            lock (_sync)
            {
                //StopAutoRefresh();
                _vehicleManager.Reset();
                _alertManager.Reset();
                NumeroFeedValidi = 0;
                NumeroLetture = 0;
                fileName = null;
            }
            OnSnapshotUpdated(GetSnapshot());
        }

        protected virtual void OnSnapshotUpdated(FeedMonitorSnapshot snapshot)
        {
            SnapshotUpdated?.Invoke(this, new FeedMonitorSnapshotEventArgs(snapshot));
        }

        protected virtual void OnError(Exception ex)
        {
            ErrorOccurred?.Invoke(this, ex);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            StopAutoRefresh();
            // non si dispone dei manager perché Form li potrebbe riusare, ma se vuoi li puoi nullare qui
        }



        public async Task ExportGrid()
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                try
                {
                    FileInfo file = new FileInfo(Path.Combine("OUTPUT", fileName + ".xlsx"));                    
                    await SaveAs(file);
                    if (DataResetMonitoraggio.HasValue)
                    {

                    }
                }
                catch (Exception e)
                {
                    Log.Error("{Exception}", e);
                    throw;
                }
            }
        }

        private async Task SaveAs(FileInfo outputFile)
        {
            if (exportXlsx)
            {
                using (ExcelPackage excel = new ExcelPackage(outputFile))
                {
                    try
                    {
                        string excelSheetName;
                        if (_vehicleManager.LastValidationResultCode == 0)
                        {
                            excelSheetName = "Feed";
                            ElaboraSheet(excel, excelSheetName, _vehicleManager.ElencoAggregatoVetture);

                            if (exportGrafico)
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
                                    _vehicleManager.ElencoVettureGrafico
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
                                lineChartATAC.Title.Text = string.Format("Vetture Rilevate ATAC {0:dd-MM-yyyy [HH:mm:ss}-{1:HH:mm:ss}] ", _vehicleManager.FirstDataFeed, _vehicleManager.LastDataFeed);
                                lineChartTPL.Title.Text = string.Format("Vetture Rilevate  TPL {0:dd-MM-yyyy [HH:mm:ss}-{1:HH:mm:ss}] ", _vehicleManager.FirstDataFeed, _vehicleManager.LastDataFeed);

                                ExcelRangeBase rangeLabel = range.Offset(1, 0, _vehicleManager.ElencoVettureGrafico.Count, 1);
                                ExcelRangeBase range1 = range.Offset(1, 2, _vehicleManager.ElencoVettureGrafico.Count, 1);
                                ExcelRangeBase range2 = range.Offset(1, 4, _vehicleManager.ElencoVettureGrafico.Count, 1);
                                ExcelRangeBase range3 = range.Offset(1, 3, _vehicleManager.ElencoVettureGrafico.Count, 1);
                                ExcelRangeBase range4 = range.Offset(1, 5, _vehicleManager.ElencoVettureGrafico.Count, 1);

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

                            //if (tabMainForm.TabPages.Contains(tabMonitoraggio) && checkMonitoraggioChecked)
                            if (exportMonitoraggio)
                            {
                                excelSheetName = "Monitoraggio Linee";
                                List<LineaMonitorata> violazioniLineaMonitorata = _vehicleManager.ElencoLineeMonitorate.Where(x => x.OraPrimaViolazione.HasValue).ToList();
                                ElaboraSheet(excel, excelSheetName, violazioniLineaMonitorata);
                            }

                            if (exportAlert && exportAlertEnabled)
                            {
                                foreach (AlertDaControllare alertDaControllare in _vehicleManager.GTFS_RSM.AlertsDaControllare)
                                {
                                    excelSheetName = alertDaControllare.Name;
                                    ElaboraSheet(excel, excelSheetName, alertDaControllare.ViolazioniAlert, dateFormat: "HH:mm:ss");
                                }
                            }

                            if (exportAnomalieGTFS)
                            {
                                excelSheetName = "AnomalieGTFS";
                                List<string> ammessi = new List<string> { "Matricola", "Linea", "PrimaVolta", "TripId", "CurrentStopSequence", "Delta" };
                                ElaboraSheet(excel, excelSheetName, _vehicleManager.AnomaliaGTFS, ammessi);
                            }

                            if (exportSovraffollamento)
                            {
                                excelSheetName = "Sovraffollamento";
                                ElaboraSheet(excel, excelSheetName, _vehicleManager.ElencoVettureSovraffollate);
                            }
                        }

                        excelSheetName = "Avvisi";
                        if (_alertManager.LastValidationResultCode == 0 && _alertManager.Avvisi is List<Avviso> avvisi)
                        {
                            ElaboraSheet(excel, excelSheetName, avvisi);
                        }
                        else if (_alertManager.FirstDataFeed.HasValue)
                        {
                            excel.Workbook.Worksheets.MoveToEnd(excelSheetName);
                        }
                        FileInfo altFileName = new FileInfo(Path.Combine("OUTPUT", fileName + ".xlsx.bck"));
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

            if (exportCSV)
            {
                using (var writer = new StreamWriter(Path.Combine("OUTPUT", fileName + ".csv")))
                {
                    var config = new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";" };
                    using (var csv = new CsvWriter(writer, config))
                    {
                        await csv.WriteRecordsAsync(_vehicleManager.ElencoAggregatoVetture);
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
    }
}