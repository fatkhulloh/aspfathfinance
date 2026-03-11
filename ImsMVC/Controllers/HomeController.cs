using ImsMVC.Models;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Mvc;

namespace ImsMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly string conn = ConfigurationManager
            .ConnectionStrings["FinanceDb"].ConnectionString;

        public ActionResult Index(int page = 1, int pageSize = 10)
        {
            var list = new List<KontrakModel>();
            int offset = (page - 1) * pageSize;
            int totalCount = 0;

            using (var con = new MySqlConnection(conn))
            {
                // Ambil total count untuk paging
                using (var countCmd = new MySqlCommand("SELECT COUNT(*) FROM KONTRAK", con))
                {
                    con.Open();
                    totalCount = Convert.ToInt32(countCmd.ExecuteScalar());
                    con.Close();
                }

                // Ambil data sesuai page
                using (var cmd = new MySqlCommand(@"
                    SELECT KONTRAK_NO, CLIENT_NAME, OTR, DP, POKOK_UTANG, TENOR, ANGSURAN_PER_BULAN, START_DATE
                    FROM KONTRAK
                    ORDER BY START_DATE DESC
                    LIMIT @limit OFFSET @offset", con))
                {
                    cmd.Parameters.AddWithValue("@limit", pageSize);
                    cmd.Parameters.AddWithValue("@offset", offset);

                    con.Open();
                    using (var rd = cmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection))
                    {
                        while (rd.Read())
                        {
                            list.Add(new KontrakModel
                            {
                                KontrakNo = rd.GetString("KONTRAK_NO"),
                                ClientName = rd.GetString("CLIENT_NAME"),
                                OTR = rd.GetDecimal("OTR"),
                                DP = rd.GetDecimal("DP"),
                                PokokUtang = rd.GetDecimal("POKOK_UTANG"),
                                Tenor = rd.GetInt32("TENOR"),
                                AngsuranPerBulan = rd.GetDecimal("ANGSURAN_PER_BULAN"),
                                StartDate = rd.GetDateTime("START_DATE")
                            });
                        }
                    }
                }
            }

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)System.Math.Ceiling((double)totalCount / pageSize);

            return View(list);
        }
    }
}