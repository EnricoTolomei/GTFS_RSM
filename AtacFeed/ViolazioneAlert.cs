using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtacFeed
{
    public class ViolazioneAlert : RegolaAlert
    {
        
        public string Violazione { get; set; }

        [Ignore] 
        public DateTime? OraPrimaViolazione { get; set; }
        [Ignore] 
        public DateTime? OraUltimaViolazione { get; set; }

        //[Ignore]
        //internal RegolaAlert RegolaAlert { get; set; }



        public ViolazioneAlert(DateTime? oraPrimaViolazione, DateTime? oraUltimaViolazione, RegolaAlert regolaAlert, string vetturaSbagliata)
        {
            this.OraPrimaViolazione = oraPrimaViolazione;
            this.OraUltimaViolazione = oraUltimaViolazione;
            //this.RegolaAlert = RegolaAlert;
            this.Violazione = vetturaSbagliata;
            this.Linea = regolaAlert.Linea;
            this.Giorno = regolaAlert.Giorno;
            this.Da = regolaAlert.Da;
            this.A = regolaAlert.A;
            this.VetturaDa = regolaAlert.VetturaDa;
            this.VetturaA = regolaAlert.VetturaA;
        }

        public ViolazioneAlert(DateTime OraPrimaViolazione, DateTime? OraUltimaViolazione, string linea, string VetturaSbagliata)
        {
            this.OraPrimaViolazione = OraPrimaViolazione;
            this.OraUltimaViolazione = OraUltimaViolazione;
            //this.RegolaAlert = new RegolaAlert { Linea = linea, Giorno = "", Da = null, A = null, VetturaDa = null, VetturaA = null };
            //this.VetturaSbagliata = VetturaSbagliata;
            this.Linea = linea;
            this.Giorno = "";
            this.Da = null;
            this.A = null;
            this.VetturaDa = null;
            this.VetturaA = null;
            this.Violazione = VetturaSbagliata;
        }
    }
}
