using CsvHelper.Configuration.Attributes;
using System;

namespace AtacFeed
{
    public class ViolazioneAlert : RegolaAlert
    {

        public string Violazione { get; set; }

        [Ignore]
        public DateTime? OraPrimaViolazione { get; set; }
        [Ignore]
        public DateTime? OraUltimaViolazione { get; set; }

        public ViolazioneAlert(DateTime? oraPrimaViolazione, DateTime? oraUltimaViolazione, RegolaAlert regolaAlert, string vetturaSbagliata)
        {
            OraPrimaViolazione = oraPrimaViolazione;
            OraUltimaViolazione = oraUltimaViolazione;
            Violazione = vetturaSbagliata;
            Linea = regolaAlert.Linea;
            Giorno = regolaAlert.Giorno;
            Da = regolaAlert.Da;
            A = regolaAlert.A;
            VetturaDa = regolaAlert.VetturaDa;
            VetturaA = regolaAlert.VetturaA;
        }

        public ViolazioneAlert(DateTime oraPrimaViolazione, DateTime? oraUltimaViolazione, string linea, string VetturaSbagliata)
        {
            OraPrimaViolazione = oraPrimaViolazione;
            OraUltimaViolazione = OraUltimaViolazione;
            Linea = linea;
            Giorno = "";
            Da = null;
            A = null;
            VetturaDa = null;
            VetturaA = null;
            Violazione = VetturaSbagliata;
        }
    }
}
