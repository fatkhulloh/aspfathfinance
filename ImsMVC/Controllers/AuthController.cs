using ImsMVC.Models;
using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Web.Helpers;
using System.Web.Mvc;

namespace ImsMVC.Controllers
{
    public class AuthController : Controller
    {
        string connStr = ConfigurationManager.ConnectionStrings["Auth"].ConnectionString;

        // GET: /auth/register
        public ActionResult Register()
        {
            return View();
        }

        // POST: /auth/register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                using (var conn = new MySqlConnection(connStr))
                {
                    conn.Open();

                    // cek email sudah ada atau belum
                    string checkQuery = "SELECT COUNT(*) FROM Users WHERE Email=@Email";
                    using (var cmdCheck = new MySqlCommand(checkQuery, conn))
                    {
                        cmdCheck.Parameters.AddWithValue("@Email", model.Email);
                        long count = (long)cmdCheck.ExecuteScalar();
                        if (count > 0)
                        {
                            ModelState.AddModelError("", "Email sudah terdaftar");
                            return View(model);
                        }
                    }

                    // hash password
                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

                    string insertQuery = @"INSERT INTO Users (Username, Email, Password, Role)
                                           VALUES (@Username, @Email, @Password, @Role)";
                    using (var cmd = new MySqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", model.Username);
                        cmd.Parameters.AddWithValue("@Email", model.Email);
                        cmd.Parameters.AddWithValue("@Password", hashedPassword);
                        cmd.Parameters.AddWithValue("@Role", "User");

                        cmd.ExecuteNonQuery();
                    }
                }

                TempData["Success"] = "Registrasi berhasil! Silakan login.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Terjadi kesalahan: " + ex.Message);
                return View(model);
            }
        }

        // GET: /auth/login
        public ActionResult Login()
        {
            return View();
        }

        // POST: /auth/login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Email dan password wajib diisi");
                return View();
            }

            try
            {
                using (var conn = new MySqlConnection(connStr))
                {
                    conn.Open();

                    string query = "SELECT Id, Username, Email, Password, Role FROM Users WHERE Email=@Email LIMIT 1";
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                ModelState.AddModelError("", "Email atau password salah");
                                return View();
                            }

                            string storedHash = reader["Password"].ToString();
                            if (!BCrypt.Net.BCrypt.Verify(password, storedHash))
                            {
                                ModelState.AddModelError("", "Email atau password salah");
                                return View();
                            }

                            // set session
                            Session["UserId"] = Convert.ToInt32(reader["Id"]);
                            Session["Username"] = reader["Username"].ToString();
                            Session["Email"] = reader["Email"].ToString();
                            Session["Role"] = reader["Role"].ToString();

                            return RedirectToAction("Index", "Home");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Terjadi kesalahan: " + ex.Message);
                return View();
            }
        }

        // GET: /auth/logout
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login");
        }
    }
}