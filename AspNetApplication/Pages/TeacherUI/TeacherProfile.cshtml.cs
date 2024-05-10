using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace AspNetApplication.Pages.TeacherUI
{
    public class TeacherProfileModel : PageModel
    {
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }

        public string StudentFirstName { get; set; }
        public string StudentLastName { get; set; }
        public string SubjectName { get; set; }
        public string Grade { get; set; }

        public void OnGet()
        {
            int userId = Convert.ToInt32(HttpContext.Session.GetString("userId"));
            using (SqlConnection connection = DbConnector.OpenConnection())
            {
                SqlCommand cmd = new SqlCommand("SELECT first_name, last_name, subject FROM Teachers WHERE user_id=@userId", connection);
                cmd.Parameters.AddWithValue("userId", userId);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        UserFirstName = reader.GetString(0);
                        UserLastName = reader.GetString(1);
                    }
                }
            }
        }
        
        public void OnPost(string studentFirstName, string studentLastName, string subjectName, string grade)
        {
            int studentId = GetStudentId(studentFirstName, studentLastName);
            if (studentId == 0)
            {
                // студент не найден
            }
            else
            {
                int subjectId = GetSubjectId(subjectName);
                if (subjectId == 0)
                {
                    // Предмет не найден
                }
                else
                {
                    using (SqlConnection connection = DbConnector.OpenConnection())
                    {
                        SqlCommand command = new SqlCommand("INSERT INTO Grades (student_id, subject_id, grade, date) VALUES (@studentId, @subjectId, @grade, @date)", connection);
                        command.Parameters.AddWithValue("studentId", studentId);
                        command.Parameters.AddWithValue("subjectId", subjectId);
                        command.Parameters.AddWithValue("grade", grade);
                        command.Parameters.AddWithValue("date", DateTime.Now);
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        private int GetStudentId(string firstName, string lastName)
        {
            using(SqlConnection connection = DbConnector.OpenConnection())
            {
                SqlCommand cmd = new SqlCommand("SELECT student_id FROM Students WHERE first_name=@firstName AND last_name=@lastName", connection);
                cmd.Parameters.AddWithValue("firstName", firstName);
                cmd.Parameters.AddWithValue("lastName", lastName);
                using(SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return reader.GetInt32(0);
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
        }

        private int GetSubjectId(string subjectName)
        {
            using (SqlConnection connection = DbConnector.OpenConnection())
            {
                SqlCommand cmd = new SqlCommand("SELECT subject_id FROM Subjects WHERE subject_name=@subjectName", connection);
                cmd.Parameters.AddWithValue("subjectName", subjectName);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return reader.GetInt32(0);
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
        }
    }
}
