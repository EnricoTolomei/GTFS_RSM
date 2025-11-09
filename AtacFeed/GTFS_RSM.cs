using CsvHelper;
using CsvHelper.Configuration;
using GTFS;
using GTFS.Entities;
using GTFS.Entities.Collections;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace AtacFeed
{
    public class GTFS_RSM
    {
        public GTFSFeed StaticData { get; private set; }
        public List<LineaAgenzia> ElencoLineaAgenzia { get; private set; }
        public IEnumerable<DettagliVettura> ElencoDettagliVettura { get; private set; }

        // Configurazioni Csv riutilizzabili per evitare ricreazione ad ogni chiamata
        private static readonly CsvConfiguration CsvConfigSemicolon = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";",
            HeaderValidated = null,
            MissingFieldFound = null,
            TrimOptions = TrimOptions.Trim,
            PrepareHeaderForMatch = args => args.Header.Trim(),
            AllowComments = true
        };

        private static readonly CsvConfiguration CsvConfigComma = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ",",
            HeaderValidated = null,
            MissingFieldFound = null,
            TrimOptions = TrimOptions.Trim,
            PrepareHeaderForMatch = args => args.Header.Trim(),
            AllowComments = true
        };

        public List<CriterioMediaPonderata> CriteriMediaPonderata;
        public List<RegolaMonitoraggio> RegoleMonitoraggio;
        public List<AlertDaControllare> AlertsDaControllare;

        public GTFS_RSM(string pathGTFSStatico, bool usaDettagliVettura = true)
        {
            if (string.IsNullOrWhiteSpace(pathGTFSStatico))
                throw new ArgumentException(nameof(pathGTFSStatico));

            var reader = new GTFSReader<GTFSFeed>();
            string gtfsZipPath = Path.Combine(pathGTFSStatico, "GTFS.zip");

            // Se esiste GTFS.zip, applica backup e rimuovi file non necessari per ridurre memoria/tempo di parsing
            if (File.Exists(gtfsZipPath))
            {
                try
                {
                    string gtfsBck = Path.Combine(pathGTFSStatico, "GTFS_bck.zip");
                    File.Copy(gtfsZipPath, gtfsBck, true);

                    // Apri in modalità Update solo quando serve cancellare entry
                    using (var fs = new FileStream(gtfsZipPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                    using (var zipArchive = new ZipArchive(fs, ZipArchiveMode.Update))
                    {
                        // Se le entry non esistono il Delete non fa nulla: query solo due volte
                        var stopTimes = zipArchive.GetEntry("stop_times.txt");
                        stopTimes?.Delete();

                        var shapes = zipArchive.GetEntry("shapes.txt");
                        shapes?.Delete();
                    }
                }
                catch (Exception ex)
                {
                    // Non bloccare la costruzione dell'oggetto per problemi nel backup/zip, ma loggare
                    Log.Warning(ex, "Errore durante l'elaborazione del file GTFS.zip (backup/cancellazione entry)");
                }

                // Leggi dal file zip modificato
                pathGTFSStatico = gtfsZipPath;
            }

            // Carica i dati statici GTFS (il reader può essere pesante)
            StaticData = reader.Read(pathGTFSStatico) ?? new GTFSFeed();

            // Costruzione efficiente di ElencoLineaAgenzia evitando join LINQ costosi
            ElencoLineaAgenzia = BuildLineaAgenzia(StaticData.Routes, StaticData.Agencies);

            AlertsDaControllare = new List<AlertDaControllare>();

            LeggiDettagliVettura(usaDettagliVettura);
        }

        private static List<LineaAgenzia> BuildLineaAgenzia(IUniqueEntityCollection<Route> routes, IEnumerable<Agency> agencies)
        {
            if (routes == null) return new List<LineaAgenzia>();

            // Creiamo un dizionario per lookup O(1) invece di join costosi
            Dictionary<string, Agency> agencyById = agencies?.ToDictionary(a => a.Id) ?? new Dictionary<string, Agency>(0);

            var result = new List<LineaAgenzia>();
            foreach (var r in routes)
            {
                Agency ag = null;
                if (!string.IsNullOrEmpty(r.AgencyId))
                    agencyById.TryGetValue(r.AgencyId, out ag);

                result.Add(new LineaAgenzia(r, ag));
            }
            return result;
        }

        internal void LeggiDettagliVettura(bool usaDettagliVettura)
        {
            ElencoDettagliVettura = Array.Empty<DettagliVettura>();
            if (!usaDettagliVettura)
                return;

            string pathDettagli = Path.Combine("Config", "GTFS_Static", "DettagliVettura.csv");
            if (!File.Exists(pathDettagli))
                return;

            try
            {
                using (var readerDettagli = new StreamReader(pathDettagli))
                using (var csv = new CsvReader(readerDettagli, CsvConfigSemicolon))
                {
                    // ToList permette di chiudere subito lo stream prima di ulteriori elaborazioni
                    ElencoDettagliVettura = csv.GetRecords<DettagliVettura>().ToList();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Errore durante la lettura di DettagliVettura.csv");
                ElencoDettagliVettura = Array.Empty<DettagliVettura>();
            }
        }

        public int LeggiCriteriMediaPonderata(string file)
        {
            if (string.IsNullOrWhiteSpace(file)) return -1;
            var fi = new FileInfo(file);
            if (!fi.Exists) return -1;

            try
            {
                using (var readerMediaPonderata = new StreamReader(fi.FullName))
                using (var csvMediaPonderata = new CsvReader(readerMediaPonderata, CsvConfigComma))
                {
                    CriteriMediaPonderata = csvMediaPonderata.GetRecords<CriterioMediaPonderata>().ToList();
                    // Confronto con tolleranza per i double
                    double sumPeso = CriteriMediaPonderata.Sum(x => x.Peso);
                    if (Math.Abs(sumPeso - 1.0) > 1e-9)
                        return -1;
                }
                return 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Errore durante la lettura di CriteriMediaPonderata");
                return -1;
            }
        }

        public void LeggiRegoleMonitoraggio(string file)
        {
            if (string.IsNullOrWhiteSpace(file) || !File.Exists(file))
                return;

            try
            {
                using (var readerTempoBonus = new StreamReader(file))
                using (var csv = new CsvReader(readerTempoBonus, CsvConfigComma))
                {
                    RegoleMonitoraggio = csv.GetRecords<RegolaMonitoraggio>().ToList();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Errore durante la lettura di RegoleMonitoraggio");
                RegoleMonitoraggio = new List<RegolaMonitoraggio>();
            }
        }

        public bool LeggiAlertDaControllare(string pathAlert)
        {
            if (string.IsNullOrWhiteSpace(pathAlert) || !Directory.Exists(pathAlert))
                return false;

            bool nessunErrore = true;
            try
            {
                var alertFiles = Directory.GetFiles(pathAlert, "*.txt");
                if (alertFiles.Length == 0)
                    return true;

                // Per lookup rapido delle alerts già censite usiamo Dictionary temporaneo
                var map = AlertsDaControllare?.ToDictionary(a => a.Name, StringComparer.OrdinalIgnoreCase)
                          ?? new Dictionary<string, AlertDaControllare>(StringComparer.OrdinalIgnoreCase);

                foreach (var file in alertFiles)
                {
                    try
                    {
                        using (var reader = new StreamReader(file))
                        using (var csv = new CsvReader(reader, CsvConfigComma))
                        {
                            csv.Context.RegisterClassMap<RegolaAlertMap>();
                            var listaRegole = csv.GetRecords<RegolaAlert>().ToList();
                            string name = Path.GetFileNameWithoutExtension(file);

                            if (map.TryGetValue(name, out var existing))
                            {
                                existing.RegoleAlert = listaRegole;
                            }
                            else
                            {
                                var newAlert = new AlertDaControllare
                                {
                                    RegoleAlert = listaRegole,
                                    ViolazioniAlert = new List<ViolazioneAlert>(),
                                    Name = name
                                };
                                map[name] = newAlert;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        nessunErrore = false;
                        Log.Error(ex, "Errore durante la lettura dell'alert '{FileName}'", file);
                    }
                }

                // Sostituisci la collection originale con i risultati elaborati (preserva referenze se necessario)
                AlertsDaControllare = map.Values.ToList();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Errore durante l'elaborazione dei file degli alert");
                return false;
            }

            return nessunErrore;
        }
    }
}
