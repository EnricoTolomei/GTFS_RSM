using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtacFeed
{
    public class Avviso(string id,
                  DateTime? validoDal,
                  DateTime? validoAl,
                  string titolo,
                  string descrizione,
                  string lineeCoinvolte,
                  string cause,
                  string effect,
                  bool isDeleted = false)
    {
        public string Id { get; set; } = id;
        [Ignore]
        public bool IsDeleted { get; set; } = isDeleted;
        public DateTime? ValidoDal { get; set; } = validoDal;
        public DateTime? ValidoAl { get; set; } = validoAl;
        [Description("Linea Interessata")]
        public string LineeCoinvolte { get; set; } = lineeCoinvolte;
        public string Titolo { get; set; } = titolo;
        public string Descrizione { get; set; } = descrizione;
        [Ignore] 
        public string Url { get; set; }
        [Description("Causa")]
        public string Cause { get; set; } = cause;
        [Description("Effetto")]
        public string Effect { get; set; } = effect;
    }
}
