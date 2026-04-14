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

        public FeedVehicleManager VehicleManager { get; private set; }
        public FeedAlertManager AlertManager { get; private set; }

        // Eventi di notifica (UI deve effettuare Invoke/BeginInvoke)
        public event EventHandler VehiclesUpdated;
        public event EventHandler AlertsUpdated;
        public event EventHandler<Exception> ErrorOccurred;
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
            VehicleManager = new FeedVehicleManager();
            AlertManager = new FeedAlertManager();
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
                                VehicleManager.LeggiFeedValido(attemptedUrl);
                                if (VehicleManager.CodeFeed == 0)
                                {
                                    success = true;
                                    OnVehicleReadCompleted(new VehicleReadResultEventArgs(attemptedUrl, true, VehicleManager.CodeFeed, VehicleManager.LastDataFeed, isRiserva: idx > 0));
                                    break; // esci dal ciclo se abbiamo successo
                                }
                                else
                                {
                                    // segnalazione completamento non-successo per il tentativo
                                    OnVehicleReadCompleted(new VehicleReadResultEventArgs(attemptedUrl, false, VehicleManager.CodeFeed, VehicleManager.LastDataFeed, isRiserva: idx > 0));
                                }
                            }
                            catch (Exception ex)
                            {
                                lastException = ex;
                                try { OnVehicleReadCompleted(new VehicleReadResultEventArgs(url, false, VehicleManager?.CodeFeed ?? -999, VehicleManager?.LastDataFeed, ex, isRiserva: idx > 0)); } catch { }
                                OnError(ex);
                            }
                        }
                        
                        // Se abbiamo avuto successo su uno dei due URL, allora elaboriamo l'ultimo feed valido
                        if (success)
                        {
                            OnVehicleElabStarted(new VehicleReadResultEventArgs(attemptedUrl, true, VehicleManager.CodeFeed, VehicleManager.LastDataFeed));
                            VehicleManager.ElaboraUltimoFeedValido(
                                filtroLinea: filtroLinea,
                                filtroTripVuoti: filtroTripVuoti,
                                filtroTuttoPercorso: filtroTuttoPercorso,
                                raggruppalineaRegola: raggruppalineaRegola,
                                nonRaggruppare: nonRaggruppare);

                            NumeroFeedValidi++;
                            if (string.IsNullOrEmpty(fileName))
                            {
                                fileName = string.Format("NEWFeed_{0:yyyy-MM-dd (HH_mm_ss)}", VehicleManager.LastDataFeed);
                            }
                            Task.Delay(2500).Wait(); // breve delay per permettere alla UI di aggiornare lo stato di "lettura in corso" prima di bloccare con la lettura sincrona
                            // Notifica locale dopo successo
                            OnVehiclesUpdated();
                            OnVehicleElabCompleted(new VehicleReadResultEventArgs(attemptedUrl, true, VehicleManager.CodeFeed, VehicleManager.LastDataFeed));
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
                            AlertManager.LeggiFeedValido(urlAlert);
                            if (AlertManager.CodeFeed == 0)
                            {
                                // se AlertManager avesse elaborazioni aggiuntive, chiamarle qui
                                //OnAlertsUpdated();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        OnError(ex);
                        throw;
                    }
                }, cancellation);                
                await Task.WhenAll(vehicleTask).ConfigureAwait(false);
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
        /// Avvia polling periodico asincrono. Se è già in esecuzione la chiamata non fa nulla.
        /// </summary>
        public void StartAutoRefresh(string urlVehicle, string urlVehicleRiserva, string urlAlert, string filtroLinea, bool filtroTripVuoti,
            bool filtroTuttoPercorso, bool raggruppalineaRegola, bool nonRaggruppare,
            int intervalMilliseconds)
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


                            //if ((checkResetSempre.Visible && checkResetSempre.Checked) || (DataResetMonitoraggio.HasValue && DateTime.Now > DataResetMonitoraggio.GetValueOrDefault()))
                            if ( DataResetMonitoraggio.HasValue && DateTime.Now > DataResetMonitoraggio.GetValueOrDefault())
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
        /// Restituisce una copia sicura (shallow) dell'elenco aggregato delle vetture.
        /// Utile per binding o snapshot UI senza esporre la lista interna.
        /// </summary>
        public List<ExtendedVehicleInfo> GetAggregatedVehiclesSnapshot()
        {
            var src = VehicleManager.ElencoAggregatoVetture;
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
            var src = VehicleManager.FeedEntities;
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
            var src = VehicleManager.GTFS_RSM?.AlertsDaControllare;
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
                VehicleManager.Reset();
                AlertManager.Reset();
                NumeroFeedValidi = 0;
                NumeroLetture = 0;
            }
        }

        protected virtual void OnVehiclesUpdated()
        {
            VehiclesUpdated?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnAlertsUpdated()
        {
            AlertsUpdated?.Invoke(this, EventArgs.Empty);
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
                        if (VehicleManager.LastValidationResultCode == 0)
                        {
                            excelSheetName = "Feed";
                            ElaboraSheet(excel, excelSheetName, VehicleManager.ElencoAggregatoVetture);

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
                                    VehicleManager.ElencoVettureGrafico
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
                                lineChartATAC.Title.Text = string.Format("Vetture Rilevate ATAC {0:dd-MM-yyyy [HH:mm:ss}-{1:HH:mm:ss}] ", VehicleManager.FirstDataFeed, VehicleManager.LastDataFeed);
                                lineChartTPL.Title.Text = string.Format("Vetture Rilevate  TPL {0:dd-MM-yyyy [HH:mm:ss}-{1:HH:mm:ss}] ", VehicleManager.FirstDataFeed, VehicleManager.LastDataFeed);

                                ExcelRangeBase rangeLabel = range.Offset(1, 0, VehicleManager.ElencoVettureGrafico.Count, 1);
                                ExcelRangeBase range1 = range.Offset(1, 2, VehicleManager.ElencoVettureGrafico.Count, 1);
                                ExcelRangeBase range2 = range.Offset(1, 4, VehicleManager.ElencoVettureGrafico.Count, 1);
                                ExcelRangeBase range3 = range.Offset(1, 3, VehicleManager.ElencoVettureGrafico.Count, 1);
                                ExcelRangeBase range4 = range.Offset(1, 5, VehicleManager.ElencoVettureGrafico.Count, 1);

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
                                List<LineaMonitorata> violazioniLineaMonitorata = VehicleManager.ElencoLineeMonitorate.Where(x => x.OraPrimaViolazione.HasValue).ToList();
                                ElaboraSheet(excel, excelSheetName, violazioniLineaMonitorata);
                            }

                            if (exportAlert && exportAlertEnabled)
                            {
                                foreach (AlertDaControllare alertDaControllare in VehicleManager.GTFS_RSM.AlertsDaControllare)
                                {
                                    excelSheetName = alertDaControllare.Name;
                                    ElaboraSheet(excel, excelSheetName, alertDaControllare.ViolazioniAlert, dateFormat: "HH:mm:ss");
                                }
                            }

                            if (exportAnomalieGTFS)
                            {
                                excelSheetName = "AnomalieGTFS";
                                List<string> ammessi = new List<string> { "Matricola", "Linea", "PrimaVolta", "TripId", "CurrentStopSequence", "Delta" };
                                ElaboraSheet(excel, excelSheetName, VehicleManager.AnomaliaGTFS, ammessi);
                            }

                            if (exportSovraffollamento)
                            {
                                excelSheetName = "Sovraffollamneto";
                                ElaboraSheet(excel, excelSheetName, VehicleManager.ElencoVettureSovraffollate);
                            }
                        }

                        excelSheetName = "Avvisi";
                        if (AlertManager.LastValidationResultCode == 0 && AlertManager.Avvisi is List<Avviso> avvisi)
                        {
                            ElaboraSheet(excel, excelSheetName, avvisi);
                        }
                        else if (AlertManager.FirstDataFeed.HasValue)
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
                        await csv.WriteRecordsAsync(VehicleManager.ElencoAggregatoVetture);
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