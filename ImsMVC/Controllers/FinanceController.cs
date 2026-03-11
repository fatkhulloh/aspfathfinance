using ImsMVC.Models;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using MySql.Data.MySqlClient;
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
            return View(new LoanModel());
        }
        private double GetBungaByTenor(int tenor)
        {
            if (tenor <= 12)
                return 0.12;
            else if (tenor <= 24)
                return 0.14;
            else
                return 0.165;
        }
        [HttpPost]
        public ActionResult Hitung(LoanModel model)
        {
            try
            {
                // Validasi sederhana
                if (string.IsNullOrEmpty(model.KontrakNo) || string.IsNullOrEmpty(model.ClientName))
                {
                    ViewBag.ErrorMessage = "Contract Number dan Client Name wajib diisi!";
                    return View("Index", model);
                }

                if (model.OTR <= 0 || model.Tenor <= 0)
                {
                    ViewBag.ErrorMessage = "OTR dan Tenor harus lebih dari 0!";
                    return View("Index", model);
                }

                // Hitung DP dan pokok utang
                model.DP = model.OTR * 0.20m;
                model.PokokUtang = model.OTR - model.DP;

                // Tentukan bunga
                model.Bunga = GetBungaByTenor(model.Tenor);

                // Hitung total hutang & angsuran per bulan
                decimal bungaTotal = model.PokokUtang * (decimal)model.Bunga;
                model.TotalHutang = model.PokokUtang + bungaTotal;
                model.AngsuranPerBulan = model.TotalHutang / model.Tenor;

                // Simpan ke database
                SaveKontrak(model);
                SaveJadwal(model);

                return View("Result", model);
            }
            catch (MySqlException ex)
            {
                ViewBag.ErrorMessage = $"Database Error: {ex.Message}";
                return View("Index", model);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Unexpected Error: {ex.Message}";
                return View("Index", model);
            }
        }

        // SIMPAN KONTRAK
        void SaveKontrak(LoanModel model)
        {
            try
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
            catch (MySqlException ex)
            {
                throw new Exception($"Gagal simpan kontrak: {ex.Message}");
            }
        }

        // GENERATE JADWAL ANGSURAN
        void SaveJadwal(LoanModel model)
        {
            try
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
            catch (MySqlException ex)
            {
                throw new Exception($"Gagal simpan jadwal angsuran: {ex.Message}");
            }
        }

        // VIEW JADWAL
        public ActionResult Jadwal(string kontrakNo)
        {
            List<JadwalAngsuran> jadwal = new List<JadwalAngsuran>();

            try
            {
                using (MySqlConnection con = new MySqlConnection(conn))
                {
                    string sql = @"SELECT ANGSURAN_KE, ANGSURAN_PER_BULAN, TANGGAL_JATUH_TEMPO
                                   FROM JADWAL_ANGSURAN
                                   WHERE KONTRAK_NO = @kontrak
                                   ORDER BY ANGSURAN_KE";

                    MySqlCommand cmd = new MySqlCommand(sql, con);
                    cmd.Parameters.AddWithValue("@kontrak", kontrakNo);
                    con.Open();
                    var rd = cmd.ExecuteReader();

                    while (rd.Read())
                    {
                        jadwal.Add(new JadwalAngsuran
                        {
                            AngsuranKe = rd.GetInt32("ANGSURAN_KE"),
                            AngsuranPerBulan = rd.GetDecimal("ANGSURAN_PER_BULAN"),
                            TanggalJatuhTempo = rd.GetDateTime("TANGGAL_JATUH_TEMPO")
                        });
                    }
                }
            }
            catch (MySqlException ex)
            {
                ViewBag.ErrorMessage = $"Database Error: {ex.Message}";
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Unexpected Error: {ex.Message}";
            }

            return View(jadwal);
        }
    }
}