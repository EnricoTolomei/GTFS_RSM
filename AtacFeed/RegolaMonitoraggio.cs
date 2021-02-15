using System;

namespace AtacFeed
{
    public class RegolaMonitoraggio
    {
        public string Linea { get; set; }
        public string Giorno { get; set; }
        public TimeSpan Da { get; set; }
        public TimeSpan? A { get; set; }
        public int TempoBonus { get; set; }
        public int? VetturePreviste { get; set; }
    }
}
