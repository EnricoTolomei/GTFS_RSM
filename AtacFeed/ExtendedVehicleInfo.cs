using CsvHelper.Configuration.Attributes;
using System;
using System.Globalization;
using static AtacFeed.TransitRealtime;
//using TransitRealtime;

namespace AtacFeed
{
    public class ExtendedVehicleInfo
    {

        [Index(0)]
        public string IdVettura { get; set; }
        
        [Index(1)]
        public string Matricola { get; set; }

        [Index(2)]
        [Ignore]
        public string LicensePlate { get; set; }
        
        [Index(3)] 
        public string RouteId { get; set; }
        
        [Index(4)]
        public string Linea { get; set; }
        
        [Index(5)]
        public string Gestore { get; set; }

        [Index(6)]
        public uint? DirectionId { get; set; }

        [Index(7)]
        public uint CurrentStopSequence { get; set; }

        [Index(8)]
        [Ignore]
        public VehiclePosition.CongestionLevel CongestionLevel { get; set; }
        
        [Index(9)]
        public VehiclePosition.OccupancyStatus OccupancyStatus { get; set; }

        [Index(10)]
        public string TripId { get; set; }

        [Ignore]
        private readonly bool StrictMode;

        [Ignore]
        private readonly bool SuperStrictMode;
        
        [Index(11)]
        public DateTime PrimaVolta { get; set; }

        [Index(12)]
        public DateTime? UltimaVolta { get; set; }

        [Index(13)]
        public string Rimessa { get; set; }

        [Index(14)]
        public string Euro { get; set; }

        [Index(15)]
        public string Modello { get; set; }

        [Index(16)]       
        public float Latitude { get; set; }

        [Index(17)]
        public float Longitude { get; set; }

        [Index(18)]
        public VehiclePosition.VehicleStopStatus InTransitTo { get; set; }

        [Index(19)]
        [Ignore]
        public int TipoMezzoTrasporto { get; set; }

        [Index(20)]
        [Ignore]
        public double DistanzaPercorsa { get; set; }

        [Ignore]
        public string NomeFermata { get; set; }

        [Ignore]
        public string Destinazione { get; set; }

        [Index(22)] 
        public DateTime? PartenzaProgrammata { get; set; }

        [Index(21)]
        public DateTime? PartenzaEffettiva { get; set; }



        public ExtendedVehicleInfo(string idVettura, string matricola, string licensePlate, string routeId, string linea, string gestore, uint? directionId, uint currentStopSequence, VehiclePosition.CongestionLevel congestionLevel, VehiclePosition.OccupancyStatus occupancyStatus, string tripId, bool strict, DateTime data, string rimessa, string euro, string modello, float latitude, float longitude, VehiclePosition.VehicleStopStatus inTransitTo, int tipoMezzoTrasporto, double distanzaPercorsa = 0, bool superStrictMode = false,
            string nomeFermata = "", string destinazione="", string dataProgrammata = "", string oraProgrammata = ""
            )
        {
            CultureInfo provider = CultureInfo.InvariantCulture;

            IdVettura = idVettura;
            Matricola = matricola;
            LicensePlate = licensePlate;
            RouteId = routeId;
            Linea = linea;
            Gestore = gestore;
            DirectionId = directionId;
            CurrentStopSequence = currentStopSequence;
            CongestionLevel = congestionLevel;
            OccupancyStatus = occupancyStatus;
            TripId = tripId;
            StrictMode = strict;
            PrimaVolta = data;
            UltimaVolta = data;
            Rimessa = rimessa;
            Euro = euro;
            Modello = modello;
            Latitude = latitude;
            Longitude = longitude;
            InTransitTo = inTransitTo;
            SuperStrictMode = superStrictMode;
            TipoMezzoTrasporto = tipoMezzoTrasporto;
            DistanzaPercorsa = distanzaPercorsa;
            NomeFermata = nomeFermata;
            Destinazione = destinazione;
            DateTime.TryParseExact(dataProgrammata, "yyyyMMdd", provider, DateTimeStyles.AssumeLocal, out DateTime dateTimePartenzaProgrammata);
            if (oraProgrammata.Length > 0)
            {
                TimeSpan timestamp = new TimeSpan(
                    int.Parse(oraProgrammata.Split(':')[0]),    // hours
                    int.Parse(oraProgrammata.Split(':')[1]),    // minutes
                    0                                           // seconds
                );
                dateTimePartenzaProgrammata = dateTimePartenzaProgrammata.Add(timestamp);
            }
            PartenzaProgrammata = dateTimePartenzaProgrammata;
        }

        public bool Equals(ExtendedVehicleInfo other)
        {
            if (other is null)
                return false;

            if (SuperStrictMode)
                return Matricola == other.Matricola && (TripId == other.TripId) && (CurrentStopSequence == other.CurrentStopSequence) ;
            else if (StrictMode)
                return Matricola == other.Matricola && (TripId == other.TripId); 
            else
                return Matricola == other.Matricola;
        }

        public override bool Equals(object obj) => Equals(obj as ExtendedVehicleInfo);
        
        public override int GetHashCode() {
            if (SuperStrictMode)
                return (Matricola, TripId, CurrentStopSequence).GetHashCode();
            else if (StrictMode)
                return (Matricola, TripId).GetHashCode();
            else
                return Matricola.GetHashCode();
        }
    }
}
