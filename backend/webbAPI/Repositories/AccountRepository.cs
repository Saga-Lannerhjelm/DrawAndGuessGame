using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using webbAPI.Models;

namespace webbAPI.Repositories
{
    public class AccountRepository(IConfiguration configuration)
    {
        private readonly string? _connectionString = configuration.GetConnectionString("DefaultConnection");

        public User? GetUserCredentials (User user, out string errorMsg) 
        {
            string query = "SELECT id, username, password, salt FROM users WHERE username = @username;";
            errorMsg = "";

            using SqlConnection dbConnection = new(_connectionString);
            try
            {
                var dbCommand = new SqlCommand(query, dbConnection);
                dbCommand.Parameters.Add("@username", SqlDbType.VarChar, 50).Value = user.Username;

                dbConnection.Open();

                SqlDataReader reader = dbCommand.ExecuteReader();
                var fetchedUser = new User();

                while (reader.Read())
                { 
                    fetchedUser = new User{
                        Id = (int)reader["id"],
                        Username = reader["username"].ToString() ?? "",
                        Password = reader["password"].ToString() ?? "",
                        Salt = reader["salt"].ToString() ?? "",
                    };
                }

                return fetchedUser;
            }
            catch (Exception e)
            {
                errorMsg = e.Message;
                return null;
            }
        }

        public int Insert(User user, out string errorMsg) 
        {
            string query = "INSERT INTO users (username, total_points, wins, password, salt) VALUES (@username, @totalPoints, @wins, @password, @salt); SELECT SCOPE_IDENTITY() AS id;";
            errorMsg = "";

            using SqlConnection dbConnection = new(_connectionString);
            try
            {
                var dbCommand = new SqlCommand(query, dbConnection);
                dbCommand.Parameters.Add("@username", SqlDbType.VarChar, 50).Value = user.Username;
                dbCommand.Parameters.Add("@totalPoints", SqlDbType.Int).Value = user.TotalPoints;
                dbCommand.Parameters.Add("@wins", SqlDbType.Int).Value = user.Wins;
                dbCommand.Parameters.Add("@password", SqlDbType.VarChar, 500).Value = user.Password;
                dbCommand.Parameters.Add("@salt", SqlDbType.VarChar, 50).Value = user.Salt;
                dbConnection.Open();

                int insertedId = Convert.ToInt16(dbCommand.ExecuteScalar());

                return insertedId;
            }
            catch (Exception e)
            {
                errorMsg = e.Message;
                return 0;
            }
        }

        
    }
}