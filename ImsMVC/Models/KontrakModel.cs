using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImsMVC.Models
{
    public class KontrakModel
    {
        public string KontrakNo { get; set; }

        public string ClientName { get; set; }

        public decimal OTR { get; set; }

        public decimal DP { get; set; }

        public decimal PokokUtang { get; set; }

        public int Tenor { get; set; }

        public decimal AngsuranPerBulan { get; set; }

        public DateTime StartDate { get; set; }
    }
}