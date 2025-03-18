using System.Data;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http;
using System.IO;


namespace MyCVCSharpAppMVC.Controllers
{
    public class UserController : Controller
    {
        public IActionResult GetNavbar()
        {
            return PartialView("_Navbar");
        }
        private readonly string connectionString = "server=localhost;database=portfolidb;user=root;password=Client99@;";


        // GET: User/Login
        public ActionResult Login()
        {
            return View();
        }
        public ActionResult Logout()
        {
            // Clear session and redirect to Home
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
        // POST: User/Login
        [HttpPost]
        public ActionResult Login(string email, string password)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT Id, Name FROM users WHERE email = @Email AND password = @Password";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Password", password);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // Store User ID in Session
                        HttpContext.Session.SetInt32("UserId", Convert.ToInt32(reader["Id"]));
                        HttpContext.Session.SetString("UserName", reader["Name"].ToString());
                        return Json(new { success = true, redirectUrl = Url.Action("Profile", "User") });
                        // return RedirectToAction("Profile");
                    }
                    else
                    {
                        // Return JSON response for failed login
                        ViewBag.ErrorMessage = "Invalid email or password!";
                        return Json(new { success = false, message = "Invalid email or password!" });
                    }
                }
            }
            return View();
        }

        // GET: User/Signup
        public ActionResult Signup()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Signup(string name, string email, string password, IFormFile profilePicture)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                // 🔍 Check if email already exists
                string checkQuery = "SELECT COUNT(*) FROM users WHERE email = @Email";
                MySqlCommand checkCmd = new MySqlCommand(checkQuery, conn);
                checkCmd.Parameters.AddWithValue("@Email", email);
                int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                if (count > 0)
                {
                    ViewBag.ErrorMessage = "Email is already registered. Please use a different email.";
                    return View();
                }

                // ✅ Insert new user
                string query = "INSERT INTO users (Name, Email, Password, ProfilePicture) VALUES (@Name, @Email, @Password, @ProfilePicture)";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Password", password);

                // Handle profile picture
                if (profilePicture != null && profilePicture.Length > 0)
                {
                    using (var ms = new MemoryStream())
                    {
                        profilePicture.CopyTo(ms);
                        byte[] imageBytes = ms.ToArray();
                        cmd.Parameters.AddWithValue("@ProfilePicture", imageBytes);
                    }
                }
                else
                {
                    cmd.Parameters.AddWithValue("@ProfilePicture", DBNull.Value);
                }

                cmd.ExecuteNonQuery();
                ViewBag.SuccessMessage = "Signup successful! Please log in.";
            }

            return RedirectToAction("Login");
        }

        // GET: User/Profile
        public ActionResult Profile()
        {
            // int? userId = HttpContext.Session.GetInt32("UserId");
            // if (userId == null)
            // {
            //     return RedirectToAction("Login");
            // }

            // ViewBag.UserName = HttpContext.Session.GetString("UserName");
            // return View();
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (userId == 0)
                return RedirectToAction("Login");

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT * FROM Users WHERE Id=@UserId";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserId", userId);

                MySqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    ViewBag.Name = reader["Name"].ToString();
                    ViewBag.Email = reader["Email"].ToString();
                    byte[] imgData = reader["ProfilePicture"] as byte[];
                    if (imgData != null)
                    {
                        ViewBag.ProfilePicture = "data:image/png;base64," + Convert.ToBase64String(imgData);
                    }
                }
            }
            return View();
        }
    }
}
