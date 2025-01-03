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

        public int Update(GameRound round, out string errorMsg) 
        {
            string query = "UPDATE game_rounds SET word = @word, round_nr = @round_nr, time = @time, round_complete = @roundComplete WHERE id = @id";
            errorMsg = "";

            using SqlConnection dbConnection = new(_connectionString);
            try
            {
                var dbCommand = new SqlCommand(query, dbConnection);
                dbCommand.Parameters.Add("@word", SqlDbType.VarChar, 50).Value = round.Word;
                dbCommand.Parameters.Add("@round_nr", SqlDbType.Int).Value = round.RoundNr;
                dbCommand.Parameters.Add("@time", SqlDbType.Int).Value = round.Time;
                dbCommand.Parameters.Add("@roundComplete", SqlDbType.TinyInt).Value = round.RoundComplete;
                dbCommand.Parameters.Add("@id", SqlDbType.Int).Value = round.Id;

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
            string query = "DELETE FROM game_rounds WHERE id = @id";
            errorMsg = "";

            using SqlConnection dbConnection = new(_connectionString);
            try
            {
                var dbCommand = new SqlCommand(query, dbConnection);
                dbCommand.Parameters.Add("@id", SqlDbType.Int).Value = id;

                dbConnection.Open();

                return dbCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                errorMsg = e.Message;
                return 0;
            }
        }

        public GameRound? GetGameRoundByGameId (int gameId, out string errorMsg) 
        {
            string query = "SELECT TOP 1 id, word, round_nr, time, round_complete, game_id FROM game_rounds WHERE game_id = @gameId ORDER BY round_nr DESC";
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
                        Time = (int)reader["time"],
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