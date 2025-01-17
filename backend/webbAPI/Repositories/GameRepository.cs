using System.Data;
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

                var affectedRows = dbCommand.ExecuteNonQuery();

                if (affectedRows == 0)
                {
                    errorMsg = "Inget spel skapades";
                }

                return affectedRows;
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

                var affectedRows = dbCommand.ExecuteNonQuery();

                if (affectedRows == 0)
                {
                    errorMsg = "Inga rader updaterades. Spelet kanske inte finns";
                }

                return affectedRows;
            }
            catch (Exception e)
            {
                errorMsg = e.Message;
                return 0;
            }
        }

        public int UpdateGame (Game game, out string errorMsg) 
        {
            string query = "UPDATE games SET is_active = @isActive, rounds = @rounds WHERE id = @gameId;";
            errorMsg = "";

            using SqlConnection dbConnection = new(_connectionString);
            try
            {
                var dbCommand = new SqlCommand(query, dbConnection);
                dbCommand.Parameters.Add("@gameId", SqlDbType.Int).Value = game.Id;
                dbCommand.Parameters.Add("@rounds", SqlDbType.Int).Value = game.Rounds;
                dbCommand.Parameters.Add("@isActive", SqlDbType.TinyInt).Value = game.IsActive;

                dbConnection.Open();

                var affectedRows = dbCommand.ExecuteNonQuery();

                if (affectedRows == 0)
                {
                    errorMsg = "Inga rader updaterades. Spelet kanske inte finns";
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
            string query = "DELETE FROM games WHERE id = @id";
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
                    errorMsg = "Inga rader togs bort. Id:t kanske inte finns";
                }

                return affectedRows;
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
                        Rounds = reader.GetInt32("rounds"),
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

         public List<Game>? GetActiveGames (out string errorMsg) 
        {
            string query = "SELECT * FROM games WHERE is_active = 1";
            errorMsg = "";

            using SqlConnection dbConnection = new(_connectionString);
            try
            {
                var dbCommand = new SqlCommand(query, dbConnection);

                dbConnection.Open();

                SqlDataReader reader = dbCommand.ExecuteReader();
                var games = new List<Game>();

                while (reader.Read())
                {
                    games.Add(new Game{
                        Id = reader.GetInt32("id"),
                        RoomName = reader.GetString("name"),
                        JoinCode = reader.GetString("join_code"),
                        Rounds = reader.GetInt32("rounds"),
                        IsActive = reader.GetByte("is_active") == 1,
                        CreatorId = reader.GetInt32("creator_id")
                    });
                }

                return games;
            }
            catch (Exception e)
            {
                errorMsg = e.Message;
                return null;
            }
        }
    }
}