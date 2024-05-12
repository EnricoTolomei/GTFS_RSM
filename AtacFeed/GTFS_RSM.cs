using CsvHelper;
using CsvHelper.Configuration;
using GTFS;
using GTFS.Entities.Collections;
using GTFS.Entities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using GTFS.Attributes;
using GTFS.Entities.Enumerations;
using CsvHelper.Configuration.Attributes;
using CsvHelper.TypeConversion;
using System.Text.Json.Nodes;

namespace AtacFeed
{
    public class GTFS_RSM
    {
        public GTFSFeed StaticData { get; set; }
        public List<Programmato> OrarioProgrammato { get; set; }
        public List<LineaAgenzia> ElencoLineaAgenzia { get; set; }
        public IEnumerable<DettagliVettura> ElencoDettagliVettura { get; set; }

        public class StopTimeMap : ClassMap<StopTime>
        {
            public StopTimeMap()
            {
                Map(m => m.TripId)
                    .Index(0)
                    .Name("trip_id");

                Map(m => m.ArrivalTime)
                    .Index(1)
                    .Name("arrival_time")
                    .TypeConverter<TimeOfDayConverter>();

                Map(m => m.DepartureTime)
                    .Index(2)
                    .Name("departure_time")
                    .TypeConverter<TimeOfDayConverter>();

                Map(m => m.StopId)
                    .Index(3)
                    .Name("stop_id");
                Map(m => m.StopSequence)
                    .Index(4)
                    .Name("stop_sequence");
                Map(m => m.StopHeadsign)
                    .Index(5)
                    .Name("stop_headsign");
                Map(m => m.PickupType)
                    .Index(6)
                    .Name("pickup_type");
                Map(m => m.DropOffType)
                    .Index(7)
                    .Name("drop_off_type");
                Map(m => m.ShapeDistTravelled)
                    .Index(8).
                    Name("shape_dist_traveled");
                Map(m => m.TimepointType)
                    .Index(9)
                    .Name("timepoint");

            }
        }


        public class TimeOfDayConverter : DefaultTypeConverter
        {
            public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
            {
                return TimeOfDay.FromString(text);
            }
        }


        public GTFS_RSM(string pathGTFSStatico, bool usaDettagliVettura=true)
        {
            try
            {
                var reader = new GTFSReader<GTFSFeed>();
                string gtfsStatico = Path.Combine(pathGTFSStatico, "GTFS.zip");

                if (File.Exists(gtfsStatico))
                {
                    string gtfsBck = Path.Combine(pathGTFSStatico, "GTFS_bck.zip");
                    File.Copy(gtfsStatico, gtfsBck, true);
                    string ff = Path.Combine(Directory.GetParent(gtfsStatico).FullName, "stop_times.txt");
                   
                    using (ZipArchive zipArchive = new(new FileStream(gtfsStatico, FileMode.Open, FileAccess.ReadWrite, FileShare.None), ZipArchiveMode.Update))
                    {
                        ZipArchiveEntry entry = zipArchive.GetEntry("stop_times.txt");
                        entry?.ExtractToFile(ff,true);
                        if (entry != null) 
                        entry?.Delete();
                    }
                    StaticData = reader.Read(gtfsStatico);
                    if (File.Exists(ff))
                        Task.Run(() => Corse(ff));
                }
                else
                {
                    StaticData = reader.Read(pathGTFSStatico);
                }
                
                ElencoLineaAgenzia = (from linea in StaticData.Routes
                                      join agenzia in StaticData.Agencies on linea.AgencyId equals agenzia.Id
                                      select new LineaAgenzia(linea, agenzia)
                                     ).ToList();

                //AlertsDaControllare = new List<AlertDaControllare>();
                AlertsDaControllare = [];

                LeggiDettagliVettura(usaDettagliVettura);
            }
            catch (Exception ex)
            {
                Log.Error("{Exception}", ex);
                throw;
            }
        }

        private void Corse(string f)
        {
            //string f= Path.Combine(Directory.GetParent(file).FullName, "stop_times");
            List<StopTime> StopTimes = [];

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
                HeaderValidated = null,
                MissingFieldFound = null,
                TrimOptions = TrimOptions.Trim,
                PrepareHeaderForMatch = args => args.Header.Trim(),
                AllowComments = true
            };
            using var readerDettagli = new StreamReader(f);
            using var csv = new CsvReader(readerDettagli, config);
            csv.Context.RegisterClassMap<StopTimeMap>();
            StopTimes = csv.GetRecords<StopTime>().ToList();


            var date = StaticData.CalendarDates
                
                .Where(x => x.Date == DateTime.Today)
                .Select(x => new { x.ServiceId, x.Date })
                
                .Union(
                    StaticData.Calendars
                    .Where(x=> x.CoversDate(DateTime.Today))
                    .Select(x=> new { x.ServiceId, Date = DateTime.Today } )

                )                
                .ToList();




            //           select service_id, calendar_dates.date
            //   from calendar_dates
            //   where calendar_dates.date in (20240205)--, 20230929, 20230930, 20231001, 20231001, 20231002)
            //union
            //   select service_id, 20240205

            //   from calendar

            //   where 20240205 between calendar.start_date and calendar.end_date


            OrarioProgrammato = StopTimes
                .Where(x => x.ShapeDistTravelled==0)
                .Join ( StaticData.Trips,
                    outerKeySelector: v => v.TripId,
                    innerKeySelector: l => l.Id,
                    resultSelector: (v, l) => new { st = v, tr = l }
                    )
                //inner join trips on trips.trip_id = stop_times.trip_id


                .Join(StaticData.Routes,
                    outerKeySelector: v => v.tr.RouteId,
                    innerKeySelector: r => r.Id,
                    resultSelector: (v, r) => new { v.st, v.tr, r }
                    )
                .Where(x=> x.r.Type!= RouteTypeExtended.UrbanRailwayService /*&& x.r.ShortName == "170" */ )
                // inner join routes on routes.route_id=trips.route_id  



                //inner join dates on dates.service_id = trips.service_id
                .Join(
                    date,
                    outerKeySelector: v => v.tr.ServiceId,
                    innerKeySelector: l => l.ServiceId,
                    resultSelector: (v, l) => new Programmato {
                            Linea = v.r.ShortName+ " - " + v.r.LongName, 
                            Direzione = v.tr.Headsign,
                            PartenzaPrevista = v.st.ArrivalTime,
                            TipoTrasporto = v.r.Type,
                            TripID = v.st.TripId                        
                    }
                )
                .ToList();
        }

        internal void LeggiDettagliVettura(bool usaDettagliVettura)
        {
            //ElencoDettagliVettura = new DettagliVettura[] { };
            ElencoDettagliVettura = [];
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
                    using var readerDettagli = new StreamReader(pathDettagli);
                    using var csv = new CsvReader(readerDettagli, config);
                    ElencoDettagliVettura = csv.GetRecords<DettagliVettura>().ToList();
                }
            }
        }

        public List<CriterioMediaPonderata> CriteriMediaPonderata;
        public List<RegolaMonitoraggio> RegoleMonitoraggio;
        public List<AlertDaControllare> AlertsDaControllare;
        public int LeggiCriteriMediaPonderata(string file) {
            int result = 0;
            FileInfo fileMediaPonderata = new(file);
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

                using var readerMediaPonderata = new StreamReader(fileMediaPonderata.FullName);
                using var csvMediaPonderata = new CsvReader(readerMediaPonderata, config);
                CriteriMediaPonderata = csvMediaPonderata.GetRecords<CriterioMediaPonderata>().ToList();
                if (CriteriMediaPonderata.Sum(x => x.Peso) != 1)
                {
                    result = -1;
                }
            }
            return result;
        }
        public void LeggiRegoleMonitoraggio(string file)
        {
            using var readerTempoBonus = new StreamReader(file);
            var configTempoBonus = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
                HeaderValidated = null,
                MissingFieldFound = null,
                TrimOptions = TrimOptions.Trim,
                PrepareHeaderForMatch = args => args.Header.Trim(),
                AllowComments = true
            };

            using var csv = new CsvReader(readerTempoBonus, configTempoBonus);
            RegoleMonitoraggio = csv.GetRecords<RegolaMonitoraggio>().ToList();
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

                        using var readerTempoBonus = new StreamReader(item);
                        using CsvReader csv = new(readerTempoBonus, config);
                        csv.Context.RegisterClassMap<RegolaAlertMap>();
                        List<RegolaAlert> listaRegole = csv.GetRecords<RegolaAlert>().ToList();

                        var alertGiaCensito = AlertsDaControllare.FirstOrDefault(x => x.Name == Path.GetFileNameWithoutExtension(item));
                        if (alertGiaCensito != null)
                        {
                            alertGiaCensito.RegoleAlert = listaRegole;
                        }
                        else
                        {
                            AlertsDaControllare.Add(
                                new AlertDaControllare
                                {
                                    RegoleAlert = listaRegole,
                                    //ViolazioniAlert = new List<ViolazioneAlert>(),
                                    ViolazioniAlert = [],
                                    Name = Path.GetFileNameWithoutExtension(item)
                                }
                            );
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
