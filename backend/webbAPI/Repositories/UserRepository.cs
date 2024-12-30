using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using webbAPI.Models;

namespace webbAPI.Repositories
{
    public class UserRepository (IConfiguration configuration)
    {
        private readonly string? _connectionString = configuration.GetConnectionString("DefaultConnection");
        
        public int Insert(User user, out string errorMsg) 
            {
                string query = "INSERT INTO users (username, total_points, wins) VALUES (@username, @totalPoints, @wins); SELECT SCOPE_IDENTITY() AS id;";
                errorMsg = "";

                using SqlConnection dbConnection = new(_connectionString);
                try
                {
                    var dbCommand = new SqlCommand(query, dbConnection);
                    dbCommand.Parameters.Add("@username", SqlDbType.VarChar, 50).Value = user.Username;
                    dbCommand.Parameters.Add("@totalPoints", SqlDbType.Int).Value = user.TotalPoints;
                    dbCommand.Parameters.Add("@wins", SqlDbType.Int).Value = user.Wins;

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