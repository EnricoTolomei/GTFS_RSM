using GTFS.Entities;
using GTFS.Entities.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtacFeed
{
    public class Programmato
    {
        public Programmato()
        {
        }

        public string Linea { get; set; }
        public string Direzione { get; set; }
        public TimeOfDay? PartenzaPrevista { get; set; }
        public TimeOfDay? PartenzaRilevata { get; set; }
        public RouteTypeExtended TipoTrasporto { get; set; }
        public string TripID { get; set; }

    }
}
