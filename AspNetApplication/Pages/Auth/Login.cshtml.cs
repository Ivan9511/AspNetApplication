using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace AspNetApplication.Pages.Auth
{
    public class LoginModel : PageModel
    {
        public string Login { get; set; }
        public string Password { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPost(string login, string password)
        {
            using (SqlConnection connection = DbConnector.OpenConnection())
            {
                SqlCommand cmd = new SqlCommand("SELECT user_id, role, ban_time FROM Users WHERE username=@login AND password=@password", connection);
                cmd.Parameters.AddWithValue("login", login);
                cmd.Parameters.AddWithValue("password", password);
                using(SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // пользователь найден
                        int userId = reader.GetInt32(0);
                        string userRole = reader.GetString(1);
                        switch(userRole)
                        {
                            case "Student":
                                return RedirectToPage("/StudentUI/StudentProfile");
                            case "Teacher":
                                return RedirectToPage("/TeacherUI/TeacherProfile");
                            case "Director":

                                break;
                        }
                    }
                    else
                    {
                        reader.Close();
                        SqlCommand checkLogin = new SqlCommand("SELECT * FROM Users WHERE username=@login", connection);
                        checkLogin.Parameters.AddWithValue("login", login);
                        using(SqlDataReader dataReader = checkLogin.ExecuteReader())
                        {
                            if (dataReader.Read())
                            {
                                string loginAttempt = HttpContext.Session.GetString("loginAttempt");
                                switch (loginAttempt)
                                {
                                    case "2":
                                        HttpContext.Session.SetString("loginAttempt", "3");
                                        break;
                                    case "3":
                                        // бан
                                        dataReader.Close();
                                        var banTime = DateTime.Now.AddMinutes(10);
                                        SqlCommand banCommand = new SqlCommand("UPDATE Users SET ban_time=@banTime WHERE username=@login", connection);
                                        banCommand.Parameters.AddWithValue("banTime", banTime.ToString());
                                        banCommand.Parameters.AddWithValue("login", login);
                                        banCommand.ExecuteNonQuery();

                                        HttpContext.Session.Clear();
                                        return RedirectToPage("/BannedPage");
                                    default:
                                        HttpContext.Session.SetString("loginAttempt", "2");
                                        break;
                                }
                                dataReader.Close();
                                var currentDate = DateTime.Now;

                                SqlCommand sendReport = new SqlCommand("INSERT INTO LoginAttempts (DateLogin, user_login) VALUES (@dateLogin, @userLogin)", connection);
                                sendReport.Parameters.AddWithValue("dateLogin", currentDate);
                                sendReport.Parameters.AddWithValue("userLogin", login);
                                sendReport.ExecuteNonQuery();
                                
                            }
                        }
                    }
                }
            }
            return RedirectToPage("Login"); 
        }
    }
}