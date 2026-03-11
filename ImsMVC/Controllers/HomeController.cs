using ImsMVC.Models;
using MySqlConnector;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Mvc;

namespace ImsMVC.Controllers
{
    public class HomeController : Controller
    {

        string conn = ConfigurationManager
        .ConnectionStrings["FinanceDb"]
        .ConnectionString;

        public ActionResult Index()
        {

            List<KontrakModel> list = new List<KontrakModel>();

            using (MySqlConnection con = new MySqlConnection(conn))
            {

                string sql = @"SELECT 
                                KONTRAK_NO,
                                CLIENT_NAME,
                                OTR,
                                DP,
                                POKOK_UTANG,
                                TENOR,
                                ANGSURAN_PER_BULAN,
                                START_DATE
                               FROM KONTRAK
                               ORDER BY START_DATE DESC";

                MySqlCommand cmd = new MySqlCommand(sql, con);

                con.Open();

                MySqlDataReader rd = cmd.ExecuteReader();

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

            return View(list);
        }

    }
}