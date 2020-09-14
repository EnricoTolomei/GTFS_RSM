using System.Collections.Generic;
using System.Windows.Forms;

namespace AtacFeed
{
    public class AlertDaControllare
    {
        public DataGridView Griglia { get; set; }
        public List<RegolaAlert> RegoleAlert { get; set; }
        public List<ViolazioneAlert> ViolazioniAlert { get; set; }
    }
}
