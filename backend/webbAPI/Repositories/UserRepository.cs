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
                string query = "INSERT INTO games (username, total_points, wins, creator_id, active_game_id) VALUES (@username, @totalPoints, @wins,  @creatorId, @activeGameId); SELECT SCOPE_IDENTITY() AS id;";
                errorMsg = "";

                using SqlConnection dbConnection = new(_connectionString);
                try
                {
                    var dbCommand = new SqlCommand(query, dbConnection);
                    dbCommand.Parameters.Add("@username", SqlDbType.VarChar, 50).Value = user.Username;
                    dbCommand.Parameters.Add("@totalPoints", SqlDbType.Int).Value = user.TotalPoints;
                    dbCommand.Parameters.Add("@wins", SqlDbType.TinyInt).Value = user.Wins;
                    dbCommand.Parameters.Add("@activeGameId", SqlDbType.Int).Value = user.ActiveGameId;

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