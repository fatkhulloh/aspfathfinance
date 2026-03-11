using ImsMVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ImsMVC.Controllers
{
    public class FinanceController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Hitung(LoanModel model)
        {
            model.DP = model.OTR * 0.20m;
            model.PokokUtang = model.OTR - model.DP;

            if (model.Tenor <= 12)
                model.Bunga = 0.12;
            else if (model.Tenor <= 24)
                model.Bunga = 0.14;
            else
                model.Bunga = 0.165;

            decimal bungaTotal = model.PokokUtang * (decimal)model.Bunga;

            model.TotalHutang = model.PokokUtang + bungaTotal;

            model.AngsuranPerBulan = model.TotalHutang / model.Tenor;

            return View("Result", model);
        }


        public ActionResult Jadwal(LoanModel model)
        {
            List<dynamic> jadwal = new List<dynamic>();

            DateTime tanggal = model.StartDate;

            for (int i = 1; i <= model.Tenor; i++)
            {
                jadwal.Add(new
                {
                    AngsuranKe = i,
                    Angsuran = model.AngsuranPerBulan,
                    JatuhTempo = tanggal.AddMonths(i)
                });
            }

            ViewBag.Jadwal = jadwal;

            return View(model);
        }

    }
}