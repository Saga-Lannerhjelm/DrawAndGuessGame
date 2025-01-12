using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using webbAPI.Models;

namespace webbAPI.Repositories
{
    public class GameRoundRepository(IConfiguration configuration)
    {
        private readonly string? _connectionString = configuration.GetConnectionString("DefaultConnection");

        public int Insert(GameRound round, out string errorMsg) 
        {
            string query = "INSERT INTO game_rounds (word, round_nr, start_time, round_complete, game_id) VALUES (@word, (SELECT COUNT(*) + 1 FROM game_rounds WHERE game_id = @gameId), @startTime, @roundComplete, @gameId); SELECT SCOPE_IDENTITY() AS id;";
            errorMsg = "";

            using SqlConnection dbConnection = new(_connectionString);
            try
            {
                var dbCommand = new SqlCommand(query, dbConnection);
                dbCommand.Parameters.Add("@word", SqlDbType.VarChar, 50).Value = round.Word;
                dbCommand.Parameters.Add("@round_nr", SqlDbType.Int).Value = round.RoundNr;
                dbCommand.Parameters.Add("@startTime", SqlDbType.DateTime).Value = round.StartTime;
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

        public int Update(GameRound round, out string errorMsg) 
        {
            string query = "UPDATE game_rounds SET word = @word, round_nr = @round_nr, round_complete = @roundComplete WHERE id = @id";
            errorMsg = "";

            using SqlConnection dbConnection = new(_connectionString);
            try
            {
                var dbCommand = new SqlCommand(query, dbConnection);
                dbCommand.Parameters.Add("@word", SqlDbType.VarChar, 50).Value = round.Word;
                dbCommand.Parameters.Add("@round_nr", SqlDbType.Int).Value = round.RoundNr;
                dbCommand.Parameters.Add("@roundComplete", SqlDbType.TinyInt).Value = round.RoundComplete;
                dbCommand.Parameters.Add("@id", SqlDbType.Int).Value = round.Id;

                dbConnection.Open();

                var affectedRows = dbCommand.ExecuteNonQuery();

                if (affectedRows == 0)
                {
                    errorMsg = "Inga rader updaterades. Id:t kanske inte finns";
                }

                return affectedRows;
            }
            catch (Exception e)
            {
                errorMsg = e.Message;
                return 0;
            }
        }

        public int Delete (int id, out string errorMsg) 
        {
            string query = "DELETE FROM game_rounds WHERE id = @id";
            errorMsg = "";

            using SqlConnection dbConnection = new(_connectionString);
            try
            {
                var dbCommand = new SqlCommand(query, dbConnection);
                dbCommand.Parameters.Add("@id", SqlDbType.Int).Value = id;

                dbConnection.Open();

                var affectedRows = dbCommand.ExecuteNonQuery();

                if (affectedRows == 0)
                {
                    errorMsg = "Inga rader togs bort. Id:t kankse inte finns";
                }

                return affectedRows;
            }
            catch (Exception e)
            {
                errorMsg = e.Message;
                return 0;
            }
        }

        public GameRound? GetGameRoundByGameId (int gameId, out string errorMsg) 
        {
            string query = "SELECT TOP 1 id, word, round_nr, start_time, round_complete, game_id FROM game_rounds WHERE game_id = @gameId ORDER BY round_nr DESC";
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
                        StartTime = (DateTime)reader["start_time"],
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