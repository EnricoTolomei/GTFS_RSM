using CsvHelper;
using CsvHelper.Configuration;
using GTFS;
using GTFS.Entities;
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
        public GTFSFeed StaticData { get; set; }
        public List<Trip> Trips { get; set; }
        public List<LineaAgenzia> ElencoLineaAgenzia { get; set; }
        public IEnumerable<DettagliVettura> ElencoDettagliVettura { get; set; }
        public GTFS_RSM(string pathGTFSStatico, bool usaDettagliVettura=true)
        {
            var reader = new GTFSReader<GTFSFeed>();
            string gtfsStatico = Path.Combine(pathGTFSStatico, "GTFS.zip");

            if (File.Exists(gtfsStatico))
            {
                string gtfsBck = Path.Combine(pathGTFSStatico, "GTFS_bck.zip");
                pathGTFSStatico = gtfsStatico;
                File.Copy(gtfsStatico, gtfsBck, true);
                using (ZipArchive zipArchive = new ZipArchive(new FileStream(pathGTFSStatico, FileMode.Open, FileAccess.ReadWrite, FileShare.None), ZipArchiveMode.Update))
                {
                    ZipArchiveEntry entry = zipArchive.GetEntry("stop_times.txt");
                    entry?.Delete();
                }
            }

            StaticData = reader.Read(pathGTFSStatico);

            Trips = StaticData.Trips
                        .Select(trip => new Trip { RouteId = trip.RouteId, Headsign = trip.Headsign, Direction = trip.Direction })
                        .Distinct()
                        .ToList();

            ElencoLineaAgenzia = (from linea in StaticData.Routes
                                  join agenzia in StaticData.Agencies on linea.AgencyId equals agenzia.Id
                                  select new LineaAgenzia(linea, agenzia)
                                 ).ToList();

            AlertsDaControllare = new List<AlertDaControllare>();

            LeggiDettagliVettura(usaDettagliVettura);
        }

        internal void LeggiDettagliVettura(bool usaDettagliVettura)
        {
            ElencoDettagliVettura = new DettagliVettura[] { };
            if (usaDettagliVettura)
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ";",
                    HeaderValidated = null,
                    MissingFieldFound = null,
                    TrimOptions = TrimOptions.Trim,
                    PrepareHeaderForMatch = args => args.Header.Trim(),
                    AllowComments = true
                };
                string pathDettagli = $"Config{Path.DirectorySeparatorChar}GTFS_Static{Path.DirectorySeparatorChar}DettagliVettura.csv";
                if (File.Exists(pathDettagli))
                {
                    using (var readerDettagli = new StreamReader(pathDettagli))
                    using (var csv = new CsvReader(readerDettagli, config))
                    {
                        ElencoDettagliVettura = csv.GetRecords<DettagliVettura>().ToList();
                    }
                }
            }
        }

        public List<CriterioMediaPonderata> CriteriMediaPonderata;
        public List<RegolaMonitoraggio> RegoleMonitoraggio;
        public List<AlertDaControllare> AlertsDaControllare;
        public int LeggiCriteriMediaPonderata(string file) {
            int result = 0;
            FileInfo fileMediaPonderata = new FileInfo(file);
            if (fileMediaPonderata.Exists)
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ",",
                    HeaderValidated = null,
                    TrimOptions = TrimOptions.Trim,
                    PrepareHeaderForMatch = args => args.Header.Trim(),
                    AllowComments = true,
                    MissingFieldFound = null
                };

                using (var readerMediaPonderata = new StreamReader(fileMediaPonderata.FullName))
                using (var csvMediaPonderata = new CsvReader(readerMediaPonderata, config))
                {
                    CriteriMediaPonderata = csvMediaPonderata.GetRecords<CriterioMediaPonderata>().ToList();
                    if (CriteriMediaPonderata.Sum(x => x.Peso) != 1)
                    {
                        result = -1;                        
                    }
                }
            }
            return result;
        }
        public void LeggiRegoleMonitoraggio(string file)
        {            
            using (var readerTempoBonus = new StreamReader(file))
            {
                var configTempoBonus = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ",",
                    HeaderValidated = null,
                    MissingFieldFound = null,
                    TrimOptions = TrimOptions.Trim,
                    PrepareHeaderForMatch = args => args.Header.Trim(),
                    AllowComments = true
                };

                using (var csv = new CsvReader(readerTempoBonus, configTempoBonus))
                {
                    RegoleMonitoraggio = csv.GetRecords<RegolaMonitoraggio>().ToList();
                }
            }
        }
        public bool LeggiAlertDaControllare(string pathAlert)
        {
            bool esito = false;
            bool nessunErrore = true;

            if (Directory.Exists(pathAlert))
            {
                string[] alertFiles = Directory.GetFiles(pathAlert, "*.txt");
                foreach (string item in alertFiles)
                {
                    try
                    {
                        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                        {
                            Delimiter = ",",
                            HeaderValidated = null,
                            TrimOptions = TrimOptions.Trim,
                            MissingFieldFound = null,
                            AllowComments = true,
                            PrepareHeaderForMatch = args => args.Header.Trim(),
                        };

                        using (var readerTempoBonus = new StreamReader(item))
                        using (var csv = new CsvReader(readerTempoBonus, config))
                        {
                            csv.Context.RegisterClassMap<RegolaAlertMap>();
                            List<RegolaAlert> listaRegole = csv.GetRecords<RegolaAlert>().ToList();
                            var alertGiaCensito = AlertsDaControllare.Where(x => x.Name == Path.GetFileNameWithoutExtension(item)).FirstOrDefault();
                            if (alertGiaCensito != null)
                            {
                                alertGiaCensito.RegoleAlert = listaRegole;
                            }
                            else
                            {
                                AlertsDaControllare.Add(
                                    new AlertDaControllare{
                                        RegoleAlert = listaRegole,
                                        ViolazioniAlert = new List<ViolazioneAlert>(),
                                        Name = Path.GetFileNameWithoutExtension(item)
                                    }
                                );
                            }
                        }
                    }
                    catch (Exception exc)
                    {
                        nessunErrore = false;
                        Log.Error(exc, "Errore Generico");
                    }
                }
                esito = true;
            }
            return esito && nessunErrore;
        }       
    }
}
