using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace AspNetApplication.Pages
{
    public class IndexModel : PageModel
    {
        public string Login { get; set; }
        public string Password { get; set; }

        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }
        public IActionResult OnPost(string login, string password)
        {
            if (login == null && password == null)
            {
                login = "";
                password = "";
            }
            

            using (SqlConnection connection = DbConnector.OpenConnection())
            {
                SqlCommand cmd = new SqlCommand("SELECT user_id, role, ban_time FROM Users WHERE username=@login AND password=@password", connection);
                cmd.Parameters.AddWithValue("login", login);
                cmd.Parameters.AddWithValue("password", password);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // пользователь найден
                        int userId = reader.GetInt32(0);
                        string userRole = reader.GetString(1);

                        //Проверка на бан
                        DateTime banTime = Convert.ToDateTime(reader.GetString(2));
                        if (banTime > DateTime.Now)
                        {
                            return RedirectToPage("/BannedPage");
                        }
                        else
                        {
                            HttpContext.Session.SetString("userId", userId.ToString());
                            switch (userRole)
                            {
                                case "Student":
                                    return RedirectToPage("/StudentUI/StudentProfile");
                                case "Teacher":
                                    return RedirectToPage("/TeacherUI/TeacherProfile");
                                case "Director":

                                    break;
                            }
                        }                
                    }
                    else
                    {
                        reader.Close();
                        SqlCommand checkLogin = new SqlCommand("SELECT * FROM Users WHERE username=@login", connection);
                        checkLogin.Parameters.AddWithValue("login", login);
                        using (SqlDataReader dataReader = checkLogin.ExecuteReader())
                        {
                            if (dataReader.Read())
                            {
                                DateTime banTime = Convert.ToDateTime(dataReader.GetString(4));
                                if (banTime > DateTime.Now)
                                {
                                    return RedirectToPage("/BannedPage");
                                }

                                string loginAttempt = HttpContext.Session.GetString("loginAttempt");
                                switch (loginAttempt)
                                {
                                    case "2":
                                        HttpContext.Session.SetString("loginAttempt", "3");
                                        break;
                                    case "3":
                                        dataReader.Close();
                                        var bannedTime = DateTime.Now.AddMinutes(1);
                                        SqlCommand banCommand = new SqlCommand("UPDATE Users SET ban_time=@banTime WHERE username=@login", connection);
                                        banCommand.Parameters.AddWithValue("banTime", bannedTime.ToString());
                                        banCommand.Parameters.AddWithValue("login", login);
                                        banCommand.ExecuteNonQuery();

                                        SqlCommand sendBanReport = new SqlCommand("INSERT INTO LoginAttempts (DateLogin, user_login, Comment) VALUES (@dateLogin, @userLogin, @comment)", connection);
                                        sendBanReport.Parameters.AddWithValue("dateLogin", DateTime.Now);
                                        sendBanReport.Parameters.AddWithValue("userLogin", login);
                                        sendBanReport.Parameters.AddWithValue("comment", "Пользователь заблокирован за попытки входа");
                                        sendBanReport.ExecuteNonQuery();
                                        
                                        HttpContext.Session.Clear();
                                        return RedirectToPage("/BannedPage");
                                    default:
                                        HttpContext.Session.SetString("loginAttempt", "2");
                                        break;
                                }
                                dataReader.Close();
                                var currentDate = DateTime.Now;

                                SqlCommand sendReport = new SqlCommand("INSERT INTO LoginAttempts (DateLogin, user_login, Comment) VALUES (@dateLogin, @userLogin, @comment)", connection);
                                sendReport.Parameters.AddWithValue("dateLogin", DateTime.Now);
                                sendReport.Parameters.AddWithValue("userLogin", login);
                                sendReport.Parameters.AddWithValue("comment", "Неверный пароль");
                                sendReport.ExecuteNonQuery();

                            }
                        }
                    }
                }
            }
            return RedirectToPage("Index");
        }
    }
}
