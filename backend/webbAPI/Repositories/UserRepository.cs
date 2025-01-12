using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using webbAPI.Models;
using webbAPI.Models.ViewModels;

namespace webbAPI.Repositories
{
    public class UserRepository (IConfiguration configuration)
    {
        private readonly string? _connectionString = configuration.GetConnectionString("DefaultConnection");
        
        public int InsertUserInRound(UserInRound user, out string errorMsg) 
        {
            string query = "INSERT INTO user_in_round (is_drawing, points, guessed_correctly, guessed_first, user_id, game_round_id) VALUES (@isDrawing, @points, @guessedCorrectly, @guessedFirst, @userId, @gameRoundId);";
            errorMsg = "";

            using SqlConnection dbConnection = new(_connectionString);
            try
            {
                var dbCommand = new SqlCommand(query, dbConnection);
                dbCommand.Parameters.Add("@isDrawing", SqlDbType.TinyInt).Value = user.IsDrawing;
                dbCommand.Parameters.Add("@points", SqlDbType.Int).Value = user.Points;
                dbCommand.Parameters.Add("@guessedCorrectly", SqlDbType.TinyInt).Value = user.GuessedCorrectly;
                dbCommand.Parameters.Add("@guessedFirst", SqlDbType.TinyInt).Value = user.GuessedFirst;
                dbCommand.Parameters.Add("@userId", SqlDbType.Int).Value = user.UserId;
                dbCommand.Parameters.Add("@gameRoundId", SqlDbType.Int).Value = user.GameRoundId;

                dbConnection.Open();

                var affectedRows = dbCommand.ExecuteNonQuery();

                if (affectedRows == 0)
                {
                    errorMsg = "Användaren lades inte till";
                }

                return affectedRows;
            }
            catch (Exception e)
            {
                errorMsg = e.Message;
                return 0;
            }
        }

         public int UpdateUser(User user, out string errorMsg) 
        {
            string query = "UPDATE users SET username = @username, total_points = @totalPoints, wins = @wins WHERE id = @id;";
            errorMsg = "";

            using SqlConnection dbConnection = new(_connectionString);
            try
            {
                var dbCommand = new SqlCommand(query, dbConnection);
                dbCommand.Parameters.Add("@username", SqlDbType.VarChar, 50).Value = user.Username;
                dbCommand.Parameters.Add("@totalPoints", SqlDbType.Int).Value = user.TotalPoints;
                dbCommand.Parameters.Add("@wins", SqlDbType.Int).Value = user.Wins;
                dbCommand.Parameters.Add("@id", SqlDbType.Int).Value = user.Id;
                dbConnection.Open();

                var affectedRows = dbCommand.ExecuteNonQuery();

                if (affectedRows == 0)
                {
                    errorMsg = "Inga rader updaterades. Användaren kanske inte finns";
                }

                return affectedRows;
            }
            catch (Exception e)
            {
                errorMsg = e.Message;
                return 0;
            }
        }

        public int UpdateUserInRound(UserInRound user, out string errorMsg) 
        {
            string query = "UPDATE user_in_round SET is_drawing = @isDrawing, points = @points, guessed_correctly = @guessedCorrectly, guessed_first = @guessedFirst, user_id = @userId, game_round_id = @gameRoundId WHERE id = @userInRoundId;";
            errorMsg = "";

            using SqlConnection dbConnection = new(_connectionString);
            try
            {
                var dbCommand = new SqlCommand(query, dbConnection);
                dbCommand.Parameters.Add("@isDrawing", SqlDbType.TinyInt).Value = user.IsDrawing;
                dbCommand.Parameters.Add("@points", SqlDbType.Int).Value = user.Points;
                dbCommand.Parameters.Add("@guessedCorrectly", SqlDbType.TinyInt).Value = user.GuessedCorrectly;
                dbCommand.Parameters.Add("@guessedFirst", SqlDbType.TinyInt).Value = user.GuessedFirst;
                dbCommand.Parameters.Add("@userId", SqlDbType.Int).Value = user.UserId;
                dbCommand.Parameters.Add("@gameRoundId", SqlDbType.Int).Value = user.GameRoundId;
                dbCommand.Parameters.Add("@userInRoundId", SqlDbType.Int).Value = user.Id;

                dbConnection.Open();

                var affectedRows = dbCommand.ExecuteNonQuery();

                if (affectedRows == 0)
                {
                    errorMsg = "Inga rader updaterades. Användaren kanske inte finns";
                }

                return affectedRows;
            }
            catch (Exception e)
            {
                errorMsg = e.Message;
                return 0;
            }
        }

        public int AddPoints(UserVM roundData, out string errorMsg) 
        {
            string query = "EXEC add_points @userId, @totalPoints, @wins, @userInRoundId, @isDrawing, @points";
            errorMsg = "";

            using SqlConnection dbConnection = new(_connectionString);
            try
            {
                var dbCommand = new SqlCommand(query, dbConnection);
                dbCommand.Parameters.Add("userId", SqlDbType.Int).Value = roundData.Info.Id;
                dbCommand.Parameters.Add("totalPoints", SqlDbType.Int).Value = roundData.Info.TotalPoints;
                dbCommand.Parameters.Add("wins", SqlDbType.Int).Value = roundData.Info.Wins;
                dbCommand.Parameters.Add("userInRoundId", SqlDbType.Int).Value = roundData.Round.Id;
                dbCommand.Parameters.Add("isDrawing", SqlDbType.TinyInt).Value = roundData.Round.IsDrawing;
                dbCommand.Parameters.Add("@points", SqlDbType.Int).Value = roundData.Round.Points;

                dbConnection.Open();

                var affectedRows = dbCommand.ExecuteNonQuery();

                return affectedRows;
            }
            catch (Exception e)
            {
                errorMsg = e.Message;
                return 0;
            }
        }

        public int DeleteUserInRound (int id, out string errorMsg) 
        {
            string query = "DELETE FROM user_in_round WHERE id = @id";
            errorMsg = "";

            using SqlConnection dbConnection = new(_connectionString);
            try
            {
                var dbCommand = new SqlCommand(query, dbConnection);
                dbCommand.Parameters.Add("@id", SqlDbType.Int).Value = id;

                dbConnection.Open();

                var rowsAffected = dbCommand.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    errorMsg = "Inga rader togs bort. Id:t kanske inte finns";
                }

                return rowsAffected;
            }
            catch (Exception e)
            {
                errorMsg = e.Message;
                return 0;
            }
        }

        public List<UserVM>? GetUsersByRound (int roundId, out string errorMsg) 
        {
            string query = "SELECT * FROM all_users_in_round WHERE game_round_id = @roundId ORDER BY userId;";
            errorMsg = "";

            using SqlConnection dbConnection = new(_connectionString);
            try
            {
                var dbCommand = new SqlCommand(query, dbConnection);
                dbCommand.Parameters.Add("@roundId", SqlDbType.Int).Value = roundId;

                dbConnection.Open();

                SqlDataReader reader = dbCommand.ExecuteReader();
                var users = new List<UserVM>();

                while (reader.Read())
                {
                    users.Add(
                        new UserVM{
                            Round = new UserInRound{
                                Id = (int)reader["userInRoundId"],
                                IsDrawing = (byte)reader["is_drawing"] == 1,
                                Points = (int)reader["points"],
                                GuessedCorrectly = (byte)reader["guessed_correctly"] == 1,
                                GuessedFirst = (byte)reader["guessed_first"] == 1,
                                UserId = (int)reader["user_id"],
                                GameRoundId = (int)reader["game_round_id"],
                            },
                            Info = new User{
                                Id = (int)reader["userId"],
                                Username = reader["username"].ToString() ?? "",
                                TotalPoints = (int)reader["total_points"],
                                Wins = (int)reader["wins"],
                                // ActiveGameId = reader["active_game_id"] != DBNull.Value ? (int)reader["active_game_id"] : (int?)null,
                            },
                            TotalRoundPoints = (int)reader["totalPointsFromGame"]
                        }
                    );
                }

                return users;
            }
            catch (Exception e)
            {
                errorMsg = e.Message;
                return null;
            }
        }

        public UserInRound? GetUser (int id, out string errorMsg) 
        {
            string query = "SELECT * FROM user_in_round WHERE user_id = @id";
            errorMsg = "";

            using SqlConnection dbConnection = new(_connectionString);
            try
            {
                var dbCommand = new SqlCommand(query, dbConnection);
                dbCommand.Parameters.Add("@id", SqlDbType.Int).Value = id;

                dbConnection.Open();

                SqlDataReader reader = dbCommand.ExecuteReader();
                var user = new UserInRound();

                while (reader.Read())
                {
                    user = new UserInRound{
                        Id = (int)reader["userInRoundId"],
                        IsDrawing = (byte)reader["is_drawing"] == 1,
                        Points = (int)reader["points"],
                        GuessedCorrectly = (byte)reader["guessed_correctly"] == 1,
                        GuessedFirst = (byte)reader["guessed_first"] == 1,
                        UserId = (int)reader["user_id"],
                        GameRoundId = (int)reader["game_round_id"],
                    };
                }

                return user;
            }
            catch (Exception e)
            {
                errorMsg = e.Message;
                return null;
            }
        }
        
        public List<User> GetAllUsers (out string errorMsg) 
        {
            string query = "SELECT * FROM users ORDER BY wins DESC, total_points DESC;";
            errorMsg = "";

            using SqlConnection dbConnection = new(_connectionString);
            try
            {
                var dbCommand = new SqlCommand(query, dbConnection);
                dbConnection.Open();

                SqlDataReader reader = dbCommand.ExecuteReader();
                var users = new List<User>();

                while (reader.Read())
                {
                    users.Add(new User{
                        Id = (int)reader["id"],
                        Username = reader["username"].ToString() ?? "",
                        TotalPoints = (int)reader["total_points"],
                        Wins = (int)reader["wins"],
                    });
                }

                return users;
            }
            catch (Exception e)
            {
                errorMsg = e.Message;
                return [];
            }
        }
    }
}