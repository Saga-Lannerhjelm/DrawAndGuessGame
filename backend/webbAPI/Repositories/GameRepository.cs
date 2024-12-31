using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using webbAPI.Models;

namespace webbAPI.Repositories
{
    public class GameRepository(IConfiguration configuration)
    {
        private readonly string? _connectionString = configuration.GetConnectionString("DefaultConnection");

        public int Insert(Game game, out string errorMsg) 
        {
            string query = "INSERT INTO games (name, join_code, is_active, creator_id) VALUES (@name, @joinCode, @isActive,  @creatorId)";
            errorMsg = "";

            using SqlConnection dbConnection = new(_connectionString);
            try
            {
                var dbCommand = new SqlCommand(query, dbConnection);
                dbCommand.Parameters.Add("@name", SqlDbType.VarChar, 50).Value = game.RoomName;
                dbCommand.Parameters.Add("@joinCode", SqlDbType.VarChar, 8).Value = game.JoinCode;
                dbCommand.Parameters.Add("@isActive", SqlDbType.TinyInt).Value = game.IsActive;
                dbCommand.Parameters.Add("@creatorId", SqlDbType.Int).Value = game.CreatorId;

                dbConnection.Open();

                return dbCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                errorMsg = e.Message;
                return 0;
            }
        }

        public int UpdateActiveState (int gameId, bool activeState, out string errorMsg) 
        {
            string query = "UPDATE games SET is_active = @isActive WHERE id = @gameId;";
            errorMsg = "";

            using SqlConnection dbConnection = new(_connectionString);
            try
            {
                var dbCommand = new SqlCommand(query, dbConnection);
                dbCommand.Parameters.Add("@gameId", SqlDbType.Int).Value = gameId;
                dbCommand.Parameters.Add("@isActive", SqlDbType.TinyInt).Value = activeState;

                dbConnection.Open();

                return dbCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                errorMsg = e.Message;
                return 0;
            }
        }


        public int Delete (int id, out string errorMsg) 
        {
            string query = "DELETE FROM games WHERE id = @id";
            errorMsg = "";

            using SqlConnection dbConnection = new(_connectionString);
            try
            {
                var dbCommand = new SqlCommand(query, dbConnection);
                dbCommand.Parameters.Add("@creatorId", SqlDbType.Int).Value = id;

                dbConnection.Open();

                return dbCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                errorMsg = e.Message;
                return 0;
            }
        }

        public Game? GetGameByJoinCode (string joinCode, out string errorMsg) 
        {
            string query = "SELECT * FROM games WHERE join_code = @joinCode";
            errorMsg = "";

            using SqlConnection dbConnection = new(_connectionString);
            try
            {
                var dbCommand = new SqlCommand(query, dbConnection);
                dbCommand.Parameters.Add("@joinCode", SqlDbType.VarChar, 8).Value = joinCode;

                dbConnection.Open();

                SqlDataReader reader = dbCommand.ExecuteReader();
                var game = new Game();

                while (reader.Read())
                {
                    game = new Game{
                        Id = reader.GetInt32("id"),
                        RoomName = reader.GetString("name"),
                        JoinCode = reader.GetString("join_code"),
                        IsActive = reader.GetByte("is_active") == 1,
                        CreatorId = reader.GetInt32("creator_id")
                    };
                }

                return game;
            }
            catch (Exception e)
            {
                errorMsg = e.Message;
                return null;
            }
        }
    }
}