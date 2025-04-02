using CsvHelper.Configuration.Attributes;
using System;
using System.ComponentModel;

namespace AtacFeed
{
    public class ErroriGTFS : ExtendedVehicleInfo
    {
        [Index(1)]
        public new string Matricola { get => base.Matricola; set => base.Matricola = value; }

        [Index(2)]
        public new string Linea { get => base.Linea; set => base.Linea = value; }

        [Index(3)]
        [Description("Ora Violazione")]
        public new DateTime PrimaVolta { get => base.PrimaVolta; set => base.PrimaVolta = value; }

        [Index(3)]
        [Description("Fermata")]
        public new uint CurrentStopSequence { get => base.CurrentStopSequence; set => base.CurrentStopSequence = value; }

        [Index(4)]
        [Description("Fermate non monitorate")]
        public int Delta { get; set; }


        public ErroriGTFS(ExtendedVehicleInfo e, int delta) : base(e.IdVettura, e.Matricola, e.LicensePlate, e.RouteId, e.Linea, e.Gestore, e.DirectionId, e.CurrentStopSequence, e.CongestionLevel, e.OccupancyStatus, e.TripId, true, e.PrimaVolta, e.Rimessa, e.Euro, e.Modello, e.Latitude, e.Longitude, e.InTransitTo, e.TipoMezzoTrasporto, e.DistanzaPercorsa, false)
        {
            Delta = delta;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
