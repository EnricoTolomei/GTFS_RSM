using GTFS.Entities.Enumerations;
using System.Collections.Generic;

namespace AtacFeed
{
    public class Statistiche
    {
        public int RilevatoBusAtac { get; set; }
        public int RilevatoTramAtac { get; set; }
        public int RilevatoFilobusAtac { get; set; }
        public int RilevatoMinibusElettrici { get; set; }
        public int RilevatoFurgoncini { get; set; }
        public int RilevatoFerro { get; set; }
        public int RilevatoAltroAtac { get; set; }
        public int RilevatoBusTpl { get; set; }
        public int RilevatoPullmanTpl { get; set; }
        public int RilevatoAltroTpl { get; set; }
        public List<ServizioRaggruppato> ServizioRaggruppato { get; set; }
    }
    public class ServizioRaggruppato
    {
        public string Agenzia { get; set; }

        public string Servizio { get; set; }
        
        public RouteTypeExtended Tipo { get; set; }

        public int Num { get; set; }
    }
}