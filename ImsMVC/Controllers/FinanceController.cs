using ImsMVC.Models;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using MySqlConnector;
using System.Configuration;

namespace ImsMVC.Controllers
{
    public class FinanceController : Controller
    {
        string conn = ConfigurationManager
        .ConnectionStrings["FinanceDb"]
        .ConnectionString;

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Hitung(LoanModel model)
        {
            // hitung DP
            model.DP = model.OTR * 0.20m;

            // pokok utang
            model.PokokUtang = model.OTR - model.DP;

            // bunga berdasarkan tenor
            if (model.Tenor <= 12)
                model.Bunga = 0.12;
            else if (model.Tenor <= 24)
                model.Bunga = 0.14;
            else
                model.Bunga = 0.165;

            decimal bungaTotal = model.PokokUtang * (decimal)model.Bunga;

            // total hutang
            model.TotalHutang = model.PokokUtang + bungaTotal;

            // angsuran per bulan
            model.AngsuranPerBulan = model.TotalHutang / model.Tenor;

            // simpan ke database
            SaveKontrak(model);

            // generate jadwal
            SaveJadwal(model);

            return View("Result", model);
        }


        // ===============================
        // SIMPAN KONTRAK
        // ===============================
        void SaveKontrak(LoanModel model)
        {
            using (MySqlConnection con = new MySqlConnection(conn))
            {

                string sql = @"INSERT INTO KONTRAK
                (KONTRAK_NO,CLIENT_NAME,OTR,DP,POKOK_UTANG,
                TENOR,BUNGA,TOTAL_HUTANG,ANGSURAN_PER_BULAN,START_DATE)

                VALUES
                (@kontrak,@client,@otr,@dp,@pokok,
                @tenor,@bunga,@total,@angsuran,@start)";

                MySqlCommand cmd = new MySqlCommand(sql, con);

                cmd.Parameters.AddWithValue("@kontrak", model.KontrakNo);
                cmd.Parameters.AddWithValue("@client", model.ClientName);
                cmd.Parameters.AddWithValue("@otr", model.OTR);
                cmd.Parameters.AddWithValue("@dp", model.DP);
                cmd.Parameters.AddWithValue("@pokok", model.PokokUtang);
                cmd.Parameters.AddWithValue("@tenor", model.Tenor);
                cmd.Parameters.AddWithValue("@bunga", model.Bunga);
                cmd.Parameters.AddWithValue("@total", model.TotalHutang);
                cmd.Parameters.AddWithValue("@angsuran", model.AngsuranPerBulan);
                cmd.Parameters.AddWithValue("@start", model.StartDate);

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }


        // ===============================
        // GENERATE JADWAL ANGSURAN
        // ===============================
        void SaveJadwal(LoanModel model)
        {
            using (MySqlConnection con = new MySqlConnection(conn))
            {

                con.Open();

                for (int i = 1; i <= model.Tenor; i++)
                {
                    DateTime jatuhTempo = model.StartDate.AddMonths(i);

                    string sql = @"INSERT INTO JADWAL_ANGSURAN
                    (KONTRAK_NO,ANGSURAN_KE,ANGSURAN_PER_BULAN,TANGGAL_JATUH_TEMPO)

                    VALUES
                    (@kontrak,@ke,@angsuran,@tanggal)";

                    MySqlCommand cmd = new MySqlCommand(sql, con);

                    cmd.Parameters.AddWithValue("@kontrak", model.KontrakNo);
                    cmd.Parameters.AddWithValue("@ke", i);
                    cmd.Parameters.AddWithValue("@angsuran", model.AngsuranPerBulan);
                    cmd.Parameters.AddWithValue("@tanggal", jatuhTempo);

                    cmd.ExecuteNonQuery();
                }

            }
        }


        // ===============================
        // VIEW JADWAL
        // ===============================
        public ActionResult Jadwal(string kontrakNo)
        {
            List<dynamic> jadwal = new List<dynamic>();

            using (MySqlConnection con = new MySqlConnection(conn))
            {

                string sql = @"SELECT 
                                ANGSURAN_KE,
                                ANGSURAN_PER_BULAN,
                                TANGGAL_JATUH_TEMPO
                               FROM JADWAL_ANGSURAN
                               WHERE KONTRAK_NO = @kontrak";

                MySqlCommand cmd = new MySqlCommand(sql, con);

                cmd.Parameters.AddWithValue("@kontrak", kontrakNo);

                con.Open();

                MySqlDataReader rd = cmd.ExecuteReader();

                while (rd.Read())
                {
                    jadwal.Add(new
                    {
                        AngsuranKe = rd.GetInt32("ANGSURAN_KE"),
                        Angsuran = rd.GetDecimal("ANGSURAN_PER_BULAN"),
                        JatuhTempo = rd.GetDateTime("TANGGAL_JATUH_TEMPO")
                    });
                }
            }

            ViewBag.Jadwal = jadwal;

            return View();
        }

    }
}