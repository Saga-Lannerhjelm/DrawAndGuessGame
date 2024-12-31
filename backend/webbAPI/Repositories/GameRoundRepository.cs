using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using webbAPI.Models;

namespace webbAPI.Repositories
{
    public class GameRoundRepository(IConfiguration configuration)
    {
        private readonly string? _connectionString = configuration.GetConnectionString("DefaultConnection");

        public int Insert(GameRound round, out string errorMsg) 
        {
            string query = "INSERT INTO game_rounds (word, round_nr, round_complete, game_id) VALUES (@word, (SELECT COUNT(*) + 1 FROM game_rounds WHERE game_id = @gameId), @roundComplete, @gameId); SELECT SCOPE_IDENTITY() AS id;";
            errorMsg = "";

            using SqlConnection dbConnection = new(_connectionString);
            try
            {
                var dbCommand = new SqlCommand(query, dbConnection);
                dbCommand.Parameters.Add("@word", SqlDbType.VarChar, 50).Value = round.Word;
                dbCommand.Parameters.Add("@round_nr", SqlDbType.Int).Value = round.RoundNr;
                dbCommand.Parameters.Add("@roundComplete", SqlDbType.TinyInt).Value = round.RoundComplete;
                dbCommand.Parameters.Add("@gameId", SqlDbType.Int).Value = round.GameId;

                dbConnection.Open();

                return Convert.ToInt32(dbCommand.ExecuteScalar());
            }
            catch (Exception e)
            {
                errorMsg = e.Message;
                return 0;
            }
        }

        // public int UpdateActiveState (Game game, out string errorMsg) 
        // {
        //     string query = "UPDATE games SET is_active = @isActive)";
        //     errorMsg = "";

        //     using SqlConnection dbConnection = new(_connectionString);
        //     try
        //     {
        //         var dbCommand = new SqlCommand(query, dbConnection);
        //         dbCommand.Parameters.Add("@isActive", SqlDbType.TinyInt).Value = game.IsActive;

        //         dbConnection.Open();

        //         return dbCommand.ExecuteNonQuery();
        //     }
        //     catch (Exception e)
        //     {
        //         errorMsg = e.Message;
        //         return 0;
        //     }
        // }


        // public int DELETE (int id, out string errorMsg) 
        // {
        //     string query = "DELETE FROM games WHERE id = @id";
        //     errorMsg = "";

        //     using SqlConnection dbConnection = new(_connectionString);
        //     try
        //     {
        //         var dbCommand = new SqlCommand(query, dbConnection);
        //         dbCommand.Parameters.Add("@creatorId", SqlDbType.Int).Value = id;

        //         dbConnection.Open();

        //         return dbCommand.ExecuteNonQuery();
        //     }
        //     catch (Exception e)
        //     {
        //         errorMsg = e.Message;
        //         return 0;
        //     }
        // }

        public GameRound? GetGameRoundByGameId (int gameId, out string errorMsg) 
        {
            string query = "SELECT * FROM game_rounds WHERE game_id = @gameId";
            errorMsg = "";

            using SqlConnection dbConnection = new(_connectionString);
            try
            {
                var dbCommand = new SqlCommand(query, dbConnection);
                dbCommand.Parameters.Add("@gameId", SqlDbType.VarChar, 8).Value = gameId;

                dbConnection.Open();

                SqlDataReader reader = dbCommand.ExecuteReader();
                var gameRound = new GameRound();

                while (reader.Read())
                {
                    gameRound = new GameRound{
                        Id = (int)reader["id"],
                        Word = (string)reader["word"],
                        RoundNr = (int)reader["round_nr"],
                        RoundComplete = (byte)reader["round_complete"] == 1,
                        GameId = (int)reader["game_id"],
                    };
                }

                return gameRound;
            }
            catch (Exception e)
            {
                errorMsg = e.Message;
                return null;
            }
        }
    }
}