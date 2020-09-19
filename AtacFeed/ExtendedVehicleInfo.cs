using CsvHelper.Configuration.Attributes;
using System;
using TransitRealtime;

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
        [Ignore]
        public uint? DirectionId { get; set; }

        [Index(7)]
        [Ignore]
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
        [Ignore]
        public VehiclePosition.VehicleStopStatus InTransitTo { get; set; }


        public ExtendedVehicleInfo(string idVettura, string matricola, string licensePlate, string routeId, string linea, string gestore, uint? directionId, uint currentStopSequence, VehiclePosition.CongestionLevel congestionLevel, VehiclePosition.OccupancyStatus occupancyStatus, string tripId, bool strict, DateTime data, string rimessa, string euro, string modello, float latitude , float longitude, VehiclePosition.VehicleStopStatus inTransitTo)
        {
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
        }

        public bool Equals(ExtendedVehicleInfo other)
        {
            if (other is null)
                return false;
            if (this.StrictMode)
                return this.IdVettura == other.IdVettura && (this.TripId == other.TripId); 
            else
                return this.IdVettura == other.IdVettura;
        }

        public override bool Equals(object obj) => Equals(obj as ExtendedVehicleInfo);
        public override int GetHashCode() {
            if (this.StrictMode)
                return (IdVettura, TripId).GetHashCode();
            else
                return IdVettura.GetHashCode();
        }
    }
}
