using Microsoft.Data.SqlClient;

namespace AspNetApplication
{
    public class DbConnector
    {
        public static SqlConnection OpenConnection()
        {
            string server = "DESKTOP-AFF6I83";
            SqlConnection connection = new SqlConnection($"Server={server}; Database=dnevnik; Integrated Security=true; TrustServerCertificate=true");
            connection.Open();
            return connection;
        }
    }
}