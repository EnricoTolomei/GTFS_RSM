using CsvHelper.Configuration.Attributes;
using System;

namespace AtacFeed
{
    public class LineaMonitorata
    {
        public string Linea { get { return RegolaViolata.Linea; } }

        public string Giorno { get { return RegolaViolata.Giorno; } }
        public TimeSpan Da { get { return RegolaViolata.Da; } }
        public TimeSpan? A { get { return RegolaViolata.A; } }
        public int TempoBonus { get { return RegolaViolata.TempoBonus; } }
        public int? VetturePreviste { get { return RegolaViolata.VetturePreviste; } }
        public int? VettureRilevate { get; set; }

        public DateTime? OraPrimaViolazione { get; set; }
        public DateTime? OraUltimaViolazione { get; set; }

        [Ignore] 
        public RegolaMonitoraggio RegolaViolata { get; set; }

        public LineaMonitorata(DateTime? oraPrimaViolazione, DateTime? oraUltimaViolazione, RegolaMonitoraggio regolaViolata, int vettureRilevate)
        {
            OraPrimaViolazione = oraPrimaViolazione;
            OraUltimaViolazione = oraUltimaViolazione;
            RegolaViolata = regolaViolata;
            VettureRilevate = vettureRilevate;
        }
        
        public bool Equals(LineaMonitorata other)
        {
            if (other is null)
                return false;
            return this.Linea == other.Linea && (this.OraUltimaViolazione.GetValueOrDefault(DateTime.MaxValue) == other.OraUltimaViolazione.GetValueOrDefault(DateTime.MaxValue));
        }

        public override bool Equals(object obj) => Equals(obj as LineaMonitorata);
        public override int GetHashCode()
        {
            return (Linea, OraUltimaViolazione.GetValueOrDefault(DateTime.MaxValue)).GetHashCode();
        }
    }
}
