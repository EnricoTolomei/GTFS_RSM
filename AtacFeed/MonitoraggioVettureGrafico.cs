using System;

namespace AtacFeed
{
    public class MonitoraggioVettureGrafico
    {
        public DateTime DateTime { get; set; }
        public int Aggregate { get; set; }
        public int AggregateAtac { get; set; }
        public int AggregateTPL { get; set; }
        public int Atac { get; set; }
        public int TPL { get; set; }
        public int Rilevate { get; set; }
        public int Aggiunte { get; set; }
        public int Tolte { get; set; }
    }
}
