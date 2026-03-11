using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ImsMVC.Models
{
    public class JadwalAngsuran
    {
        public int AngsuranKe { get; set; }

        public decimal AngsuranPerBulan { get; set; }

        public DateTime TanggalJatuhTempo { get; set; }
    }
}