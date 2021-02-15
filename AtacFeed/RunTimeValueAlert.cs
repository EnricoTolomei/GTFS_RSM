using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransitRealtime;

namespace AtacFeed
{
    class RunTimeValueAlert
    {
        public string TripID { get; set; }
        public string Linea { get; set; }
        public string Matricola { get; set; }
        public VehiclePosition.OccupancyStatus OccupancyStatus { get; set; }
        public uint PrimaFermata{ get; set; }
        public DateTime? PrimaVolta { get; set; }
        public uint? UltimaFermata { get; set; }
        public DateTime? UltimaVolta { get; set; }

        public RunTimeValueAlert(string tripID, string linea, string matricola, VehiclePosition.OccupancyStatus occupancyStatus, uint primaFermata, DateTime? primaVolta, uint? ultimaFermata, DateTime? ultimaVolta)
        {
            TripID = tripID;
            Linea = linea;
            Matricola = matricola;
            OccupancyStatus = occupancyStatus;
            PrimaFermata = primaFermata;
            PrimaVolta = primaVolta;
            UltimaFermata = ultimaFermata;
            UltimaVolta = ultimaVolta;
        }
    }
}
