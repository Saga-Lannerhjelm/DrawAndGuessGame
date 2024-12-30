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
            string query = "INSERT INTO game_rounds (word, round_nr, round_complete, game_id) VALUES (@word, @roundNr, @roundComplete, @gameId)";
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

                return dbCommand.ExecuteNonQuery();
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
    }
}