using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtacFeed
{
    public class Avviso
    {
        public string Id { get; set; }
        [Ignore] 
        public bool IsDeleted { get; set; }
        public DateTime? ValidoDal { get; set; }
        public DateTime? ValidoAl { get; set; }
        [Description("Linea Interessata")]
        public string LineeCoinvolte { get; set; }
        public string Titolo { get; set; }
        public string Descrizione { get; set; }
        [Ignore] 
        public string Url { get; set; }
        [Description("Causa")] 
        public string Cause { get; set; }
        [Description("Effetto")] 
        public string Effect { get; set; }

        public Avviso(string id,
                      DateTime? validoDal,
                      DateTime? validoAl,
                      string titolo,
                      string descrizione,
                      string lineeCoinvolte,
                      string cause,
                      string effect,
                      bool isDeleted = false)
        {
            Id = id;
            ValidoDal = validoDal;
            ValidoAl = validoAl;
            Titolo = titolo;
            Descrizione = descrizione;
            LineeCoinvolte = lineeCoinvolte;
            Cause = cause;
            Effect = effect;
            IsDeleted = isDeleted;
        }

    }
}
