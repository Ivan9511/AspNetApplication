using AspNetApplication.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Collections.ObjectModel;
using System.Reflection.PortableExecutable;

namespace AspNetApplication.Pages.StudentUI
{
    public class StudentProfileModel : PageModel
    {
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
        public ObservableCollection<Schedule> ScheduleCollection = new ObservableCollection<Schedule>();

        public void OnGet()
        {
            int userId = Convert.ToInt32(HttpContext.Session.GetString("userId"));
            using(SqlConnection connection  = DbConnector.OpenConnection())
            {
                SqlCommand cmd = new SqlCommand("SELECT first_name, last_name, group_id FROM Students WHERE user_id=@userId", connection);
                cmd.Parameters.AddWithValue("userId", userId);
                using(SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        UserFirstName = reader.GetString(0);
                        UserLastName = reader.GetString(1);
                    }
                }
            }
            GetSchedule();
        }

        private void GetSchedule()
        {
            using(SqlConnection connection = DbConnector.OpenConnection())
            {
                SqlCommand command = new SqlCommand("SELECT * FROM Schedule", connection);
                using(SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Schedule schedule = new Schedule();
                        schedule.schedule_id = reader.GetInt32(0);
                        schedule.subject_name = GetDataFromAnotherTables(reader.GetInt32(1), "Subjects");
                        schedule.teacher_name = GetDataFromAnotherTables(reader.GetInt32(2), "Teachers");
                        schedule.day_week = reader.GetString(3);
                        schedule.started_time = reader.GetString(4);
                        schedule.ending_time = reader.GetString(5);
                        ScheduleCollection.Add(schedule);
                    }
                }
            }
        }

        private string GetDataFromAnotherTables(int id, string tableName)
        {
            using(SqlConnection connection = DbConnector.OpenConnection())
            {
                switch (tableName)
                {
                    case "Subjects":
                        SqlCommand getSubject = new SqlCommand("SELECT subject_name FROM Subjects WHERE subject_id=@subject_id", connection);
                        getSubject.Parameters.AddWithValue("subject_id", id);
                        using (SqlDataReader reader = getSubject.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return reader.GetString(0);
                            }
                            else
                            {
                                return "null";
                            }
                        }
                    case "Teachers":
                        SqlCommand getTeacher = new SqlCommand("SELECT last_name FROM Teachers WHERE teacher_id=@teacher_id", connection);
                        getTeacher.Parameters.AddWithValue("teacher_id", id);
                        using (SqlDataReader reader = getTeacher.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return reader.GetString(0);
                            }
                            else
                            {
                                return "null";
                            }
                        }
                }
            }
            return "none";
        }
    }
}
