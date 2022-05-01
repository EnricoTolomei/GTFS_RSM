using CsvHelper;
using CsvHelper.Configuration;
using GTFS;
using GTFS.Entities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtacFeed
{
    public class GTFS_RSM
    {
        public GTFSFeed StaticData { get; set; }
        public List<Trip> Trips { get; set; }
        public List<LineaAgenzia> ElencoLineaAgenzia { get; set; }
        public List<string> LineeAgenzia { get; set;}
        public List<DettagliVettura> ElencoDettagliVettura { get; set; }
        public GTFS_RSM(string fileGTFSStatico)
        {
            var reader = new GTFSReader<GTFSFeed>();
            //fileGTFSStatico = Path.Combine(fileGTFSStatico, "test.zip");
            StaticData = reader.Read(fileGTFSStatico);
            Trips = (from trip in StaticData.Trips
                     select new Trip { RouteId = trip.RouteId, Headsign = trip.Headsign, Direction = trip.Direction }
                     )
                     .Distinct().ToList();

            ElencoLineaAgenzia = (from linea in StaticData.Routes
                                  join agenzia in StaticData.Agencies on linea.AgencyId equals agenzia.Id
                                  select new LineaAgenzia(linea, agenzia)
                                 ).ToList();

            LineeAgenzia = ElencoLineaAgenzia.Select(x => x.Route.Id).Distinct().ToList();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                HeaderValidated = null,
                MissingFieldFound = null,
                TrimOptions = TrimOptions.Trim,
                PrepareHeaderForMatch = args => args.Header.Trim(),
                AllowComments = true
            };


            using (var readerDettagli = new StreamReader($"Config{Path.DirectorySeparatorChar}GTFS_Static{Path.DirectorySeparatorChar}DettagliVettura.csv"))
            using (var csv = new CsvReader(readerDettagli, config))
            {
                ElencoDettagliVettura = csv.GetRecords<DettagliVettura>().ToList();
            }

            AlertsDaControllare = new List<AlertDaControllare>();
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

                /*
                foreach (var alertDaControllare in AlertsDaControllare)
                {
                    var esisteFileAlert = alertFiles.Where(x => x == alertDaControllare.Griglia.Name).Any();
                    if (!esisteFileAlert)
                        alertDaControllare.RegoleAlert = new List<RegolaAlert>();
                }
                */

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
                                    });
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
