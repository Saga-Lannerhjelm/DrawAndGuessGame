using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using webbAPI.DataService;
using webbAPI.Models;
using webbAPI.Models.ViewModels;
using webbAPI.Repositories;
using webbAPI.Services;

namespace webbAPI.Hubs
{
    [Authorize]
    public class DrawHub : Hub
    {   
        private readonly SharedDB _sharedDB;
        private readonly GameRepository _gameRepository;
        private readonly GameRoundRepository _gameRoundRepository;
        private readonly UserRepository _userRepository;
        private readonly WordService _wordService;

        private static readonly Dictionary<string, Dictionary<int, int>> drawingAmmounts = [];

        public DrawHub (SharedDB sharedDB, GameRepository gameRepository, GameRoundRepository gameRoundRepository, UserRepository userRepository, WordService wordService)
        {
            _sharedDB = sharedDB;
            _gameRepository = gameRepository;
            _gameRoundRepository = gameRoundRepository;
            _userRepository = userRepository;
            _wordService = wordService;
        }
        public async Task JoinGame (UserConnection userConn) 
        {
            // If game exist
            var existingGame = _gameRepository.GetGameByJoinCode(userConn.JoinCode, out string error) ?? new Game();

            if (existingGame?.Id != 0 && existingGame != null && string.IsNullOrEmpty(error) && !existingGame.IsActive)
            {
                // Add user (to game)
                await Groups.AddToGroupAsync(Context.ConnectionId, userConn.JoinCode);
                _sharedDB.Connection[Context.ConnectionId] = userConn;

                await Clients.Group(userConn.JoinCode).SendAsync("GameStatus", "", true);
                await UsersInGame(userConn.JoinCode);
                await GameInfo(userConn.JoinCode);
            }
            else 
            {
                await Clients.Caller.SendAsync("GameStatus", $"Spelet finns inte eller har redan startat", false);
            }
        }

        public async Task StartRound(string joinCode, int roundNr)
        {
            //Hämtar alla spelar i rummet
            var allUsersInGame = _sharedDB.Connection.Values
            .Where(g => g.JoinCode == joinCode).ToList();

            int minUsersInGame = 3;

            try
            {
                if (allUsersInGame.Count >= minUsersInGame)
                {
                    // Get current game
                    string error = "";
                    var currentGame = _gameRepository.GetGameByJoinCode(joinCode, out error) ?? new Game();

                    if (string.IsNullOrEmpty(error) || currentGame.Id != 0)
                    {
                        if (!currentGame.IsActive)
                        {
                            // Update gamestate to active
                            currentGame.IsActive = true;
                            currentGame.Rounds = roundNr;
                            var affectedRows = _gameRepository.UpdateGame(currentGame, out error);

                            if (affectedRows == 0 || !string.IsNullOrEmpty(error))
                            {
                                throw new Exception(error);
                            }
                        }

                        // Get word
                        string word = await _wordService.GetWord();  

                        // Add a new round to the game
                        var newGameRound = new GameRound {
                            GameId = currentGame.Id,
                            Word = word,
                            StartTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"))
                        };
                        var roundId = _gameRoundRepository.Insert(newGameRound, out error);

                        if (roundId == 0 || !string.IsNullOrEmpty(error))
                        {
                            throw new Exception(error);
                        }

                        // create a new userInRound for each user in the game
                        var usersInRoundList = new List<UserInRound>();
                        foreach (var user in allUsersInGame)
                        {
                            usersInRoundList.Add(new UserInRound {
                                UserId = user.Id,
                                IsDrawing = false,
                                GameRoundId = roundId,
                            });
                        }

                        // Select users that will draw
                        var rnd = new Random();
                        int randowmIndexOne = rnd.Next(allUsersInGame.Count);
                        int randomIndexTwo;
                        do
                        {
                            randomIndexTwo = rnd.Next(allUsersInGame.Count);
                        } while (randowmIndexOne == randomIndexTwo);

                        // Update IsDrawing to true
                        usersInRoundList[randowmIndexOne].IsDrawing = true;
                        usersInRoundList[randomIndexTwo].IsDrawing = true;

                        var drawingUserOne = allUsersInGame[randowmIndexOne];
                        var drawingUserTwo = allUsersInGame[randomIndexTwo];

                        // Add users to the round
                        foreach (var userInRound in usersInRoundList)
                        {
                            var affectedRows = _userRepository.InsertUserInRound(userInRound, out error);

                            if (affectedRows == 0 || !string.IsNullOrEmpty(error))
                            {
                                throw new Exception(error);
                            }
                        }
                        await Clients.Group(joinCode).SendAsync("Message", $"{drawingUserOne.Username} och {drawingUserTwo.Username} ritar!", "info");
                        await UsersInRound(roundId, currentGame.JoinCode);
                        await GameInfo(joinCode);                     
                    }
                    else {
                        await Clients.Group(joinCode).SendAsync("Message", $"Ett fel uppstod: {error}", "warning");
                    }
                }
                else 
                {
                    await Clients.Group(joinCode).SendAsync("Message", "Spelet måste ha minst tre spelare", "warning");
                }
            }
            catch (Exception ex)
            {
                await Clients.Group(joinCode).SendAsync("Message", $"Ett fel uppstod: {ex.Message}", "warning");
            }
        }

        public async Task RequestNewWord (string gameRoom, GameRound round) {
            try
            {
                var users = _userRepository.GetUsersByRound(round.Id, out string error) ?? new List<UserVM>();

                if (users.Count > 0 || string.IsNullOrEmpty(error))
                {
                    if (!users.Any(u => u.Round.GuessedCorrectly))
                    {
                        string newWord = await _wordService.GetWord();
                        round.Word = newWord;
                        var affectedRows = _gameRoundRepository.Update(round, out error);
                        if (affectedRows != 0 || string.IsNullOrEmpty(error))
                        {
                            await GameInfo(gameRoom);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
               await Clients.Group(gameRoom).SendAsync("Message", $"Ett fel uppstod: {ex.Message}", "warning");
            }
        }

        public async Task Drawing(Point start, Point end, string color, string gameRoom) 
        {
            if (_sharedDB.Connection.TryGetValue(Context.ConnectionId, out UserConnection? userConn))
            {
                AddDrawingAmmounts(gameRoom, userConn);

                await Clients.OthersInGroup(gameRoom).SendAsync("Drawing", start, end, color, userConn.Id);
            }
        }

        private static void AddDrawingAmmounts(string gameRoom, UserConnection userConn)
        {
            if (drawingAmmounts.TryGetValue(gameRoom, out Dictionary<int, int> roomDictionary))
            {
                if (roomDictionary.TryGetValue(userConn.Id, out int value))
                {
                    roomDictionary[userConn.Id] = value + 1;
                }
                else
                {
                    roomDictionary.Add(userConn.Id, 1);
                }
            }
            else
            {
                drawingAmmounts.Add(gameRoom, []);
                AddDrawingAmmounts(gameRoom, userConn);
            }
        }

        public async Task SendGuess(string guess)
        {
            if (_sharedDB.Connection.TryGetValue(Context.ConnectionId, out UserConnection? userConn))
            {
                try
                {
                    GetGameAndRound(userConn.JoinCode, out Game? currentGame, out GameRound? currentRound);
                    currentRound ??= new GameRound();

                    // Om gissa rätt
                    if (guess == currentRound?.Word)
                    {
                        // hämta alla använare i rundan
                        var users = _userRepository.GetUsersByRound(currentRound.Id, out string error) ?? [];

                        // hämta den som skickat gissningen
                        var guessingUser = users?.Find(u => u.Info.Username == userConn.Username) ?? new UserVM();

                        if (users != null || string.IsNullOrEmpty(error))
                        {
                            if (!guessingUser.Round.GuessedCorrectly)
                            {
                                // Markera ifall användaren gissat först
                                if (!users.Any(user => user.Round.GuessedCorrectly))
                                {
                                    guessingUser.Round.GuessedFirst = true;
                                }
                                guessingUser.Round.GuessedCorrectly = true;

                                // Update user
                                var affectedRows = _userRepository.UpdateUserInRound(guessingUser.Round, out error);
                                if (affectedRows != 0 || string.IsNullOrEmpty(error))
                                {
                                    // Om alla som inte ritar har gissar rätt avslutas rundan
                                    if (!users.Any(user => !user.Round.IsDrawing && !user.Round.GuessedCorrectly))
                                    {
                                        await EndRound(userConn.JoinCode);
                                    } else {
                                        await UsersInRound(currentRound.Id, userConn.JoinCode);
                                    }
                                }
                            }
                            // Skicka gissningen till klienterna
                            await Clients.Caller.SendAsync("ReceiveGuess", guess, userConn.Id);
                            await Clients.OthersInGroup(userConn.JoinCode).SendAsync("ReceiveGuess", "Gissade rätt", userConn.Id);
                        }
                    }
                    else
                    {
                        await Clients.Group(userConn.JoinCode).SendAsync("ReceiveGuess", guess, userConn.Id);
                    }
                }
                catch (Exception ex)
                {
                    await Clients.Group(userConn.JoinCode).SendAsync("Message", $"Ett fel uppstod: {ex.Message}", "warning");
                }
            }
        }

        private void GetGameAndRound(string roomCode, out Game? currentGame, out GameRound? currentRound)
        {
            currentGame = _gameRepository.GetGameByJoinCode(roomCode, out string error);
            currentGame ??= new Game();

            if (currentGame.Id == 0 && !string.IsNullOrEmpty(error))
            {
                throw new Exception(error);
            }
            
            currentRound = _gameRoundRepository.GetGameRoundByGameId(currentGame.Id, out error);

            if (currentRound == null || !string.IsNullOrEmpty(error))
            {
                throw new Exception(error);
            }
        }

        public Task? UsersInGame(string gameRoom) 
        {
            var users = _sharedDB.Connection
            .Where(g => g.Value.JoinCode == gameRoom).ToList();
            var userValues = users.Select((users) => users.Value);

            return Clients.Group(gameRoom).SendAsync("UsersInGame", userValues);
        }

        public Task UsersInRound (int roundId, string joinCode) 
        {
            var users = _userRepository.GetUsersByRound(roundId, out string error);

            if (users == null || !string.IsNullOrEmpty(error))
            {
                Clients.Group(joinCode).SendAsync("Message", $"Ett fel uppstod: {error}", "warning");
                return Clients.Group(joinCode).SendAsync("UsersInGame", null);  
            }
            return Clients.Group(joinCode).SendAsync("UsersInGame", users);  
        }

        public Task GameInfo(string joinCode) 
        {
            var currentGame = _gameRepository.GetGameByJoinCode(joinCode, out string error) ?? new Game();

            var currentRound = new GameRound();
            if (currentGame != null && string.IsNullOrEmpty(error))
            {
                currentRound = _gameRoundRepository.GetGameRoundByGameId(currentGame.Id, out error);

                if (!string.IsNullOrEmpty(error))
                {
                    Clients.Group(joinCode).SendAsync("Message", $"Ett fel uppstod: {error}", "warning");
                }
            }

            return Clients.Group(joinCode).SendAsync("receiveGameInfo", currentGame, currentRound);
        }

        public async Task SendClearCanvas (string gameRoom) 
        {
            if (drawingAmmounts.TryGetValue(gameRoom, out Dictionary<int, int> roomDictionary))
            {
                roomDictionary.Clear();
            }   
            await Clients.Group(gameRoom).SendAsync("clearCanvas");
        }

        #region test

        public async Task EndRound (string roomCode) 
        {
            try
            {
                GetGameAndRound(roomCode, out Game? currentGame, out GameRound? round);
                currentGame ??= new Game();
                round ??= new GameRound();
                if (!round.RoundComplete)
                {
                    // set round to completed
                    round.RoundComplete = true;
                    // Get users in the round
                    var users = _userRepository.GetUsersByRound(round.Id, out string error);
                    if (users != null || string.IsNullOrEmpty(error))
                    {
                        // find alla guessing users
                        var allGuessingUsers = users?.FindAll(user => !user.Round.IsDrawing) ?? [];
                        // find all that guessed correctly
                        var allCorrectGuessingUsers = allGuessingUsers?.FindAll(user => user.Round.GuessedCorrectly) ?? [];
                        if (allCorrectGuessingUsers.Count != 0)
                        {
                            // Give points to the rest of the guessing users
                            GivePointsToGuessers(allCorrectGuessingUsers);
                            // Give points to artists
                            var artists = users?.FindAll(user => user.Round.IsDrawing) ?? new List<UserVM>();
                            GivePointsToDrawer(allGuessingUsers, allCorrectGuessingUsers, artists, roomCode);
                            if (drawingAmmounts.TryGetValue(roomCode, out Dictionary<int, int> roomDictionary))
                            {
                                roomDictionary.Clear();
                            } 
                        }
                        // Update gameRound
                        var affectedRows = _gameRoundRepository.Update(round, out error);
                        if (affectedRows == 0 || !string.IsNullOrEmpty(error))
                        {
                            await Clients.Group(roomCode).SendAsync("Message", $"Ett fel uppstod: {error}", "warning");
                        }
                        // If the last round has been reached 
                        if (round.RoundNr >= currentGame.Rounds)
                        {
                            var updateError = FindWinners(round.Id);
                            if (updateError != null)
                            {
                                await Clients.Group(roomCode).SendAsync("Message", $"Ett fel uppstod: {updateError}", "warning");
                            }
                            await Clients.Group(roomCode).SendAsync("GameFinished");
                        }
                        await UsersInRound(round.Id, currentGame.JoinCode);
                        await GameInfo(currentGame.JoinCode);
                    }
                }

            }
            catch (Exception ex)
            {
                await Clients.Group(roomCode).SendAsync("Message", $"Ett fel uppstod: {ex.Message}", "warning");
            }
        }

        private string? FindWinners(int roundId)
        {
            var users = _userRepository.GetUsersByRound(roundId, out string error);
            if (users != null || string.IsNullOrEmpty(error))
            {
                if (users?.Find(u => u.TotalRoundPoints > 0) != null || users?.Find(u => u.Round.Points > 0) != null)
                {
                    // If anyone has gotten points in the round -> find winner
                    var allWinners = users?.FindAll(u => u.TotalRoundPoints == users?.MaxBy(c => c?.TotalRoundPoints)?.TotalRoundPoints) ?? [] ;
                    foreach (var winnerInGame in allWinners)
                    {
                        winnerInGame.Info.Wins++;
                        var rows = _userRepository.UpdateUser(winnerInGame.Info, out string updateError);

                        if (!string.IsNullOrEmpty(updateError))
                        {
                            return updateError;
                        }
                    }
                }
            }
            return null;
        }

        private void GivePointsToGuessers(List<UserVM>? allCorrectGuessingUsers)
        {
            var winner = allCorrectGuessingUsers?.Find(user => user.Round.GuessedFirst);
            foreach (var user in allCorrectGuessingUsers)
            {
                if (user == winner)
                {
                    AddPoints(user, 5);
                }
                AddPoints(user, 3);
            }
        }

        private void GivePointsToDrawer(List<UserVM>? allGuessingUsers, List<UserVM>? allCorrectGuessingUsers, List<UserVM> artists, string roomCode)
        {
            foreach (var user in artists)
            {
                if (drawingAmmounts.TryGetValue(roomCode, out Dictionary<int, int> roomDictionary))
                {
                    if (roomDictionary.Count != 0)
                    {
                        // Give most points to the one that drew the most
                        if (user.Info.Id == roomDictionary.MaxBy(e => e.Value).Key)
                        {
                            // Give more points if everyone answered correctly
                            if (allGuessingUsers?.Count == allCorrectGuessingUsers?.Count)
                            {
                                AddPoints(user, 4);

                            }
                            else if (allCorrectGuessingUsers?.Count >= 1)
                            {
                                AddPoints(user, 3);
                            }
                        }
                        else if (user.Info.Id == roomDictionary.MinBy(e => e.Value).Key)
                        {
                            if (allGuessingUsers?.Count == allCorrectGuessingUsers?.Count)
                            {
                                AddPoints(user, 3);

                            }
                            else if (allCorrectGuessingUsers?.Count >= 1)
                            {
                                AddPoints(user, 2);
                            }
                        }
                    }
                } 
            }
        }

        private void AddPoints(UserVM user, int points)
        {
            user.Round.Points = points;
            user.Info.TotalPoints += points;

            var affectedRows = _userRepository.AddPoints(user, out string error);
            if (!string.IsNullOrEmpty(error) && affectedRows == 0)
            {
                throw new Exception(error);
            }
        }

         #endregion test

       
        public async Task EndGame() 
        {
            if (_sharedDB.Connection.TryGetValue(Context.ConnectionId, out UserConnection? userConn))
            {
                var currentGame = _gameRepository.GetGameByJoinCode(userConn.JoinCode, out string error);

                if (currentGame != null && string.IsNullOrEmpty(error))
                {
                    UpdateActiveState(currentGame);
                    await GameInfo(currentGame.JoinCode);
                }
                await Clients.Group(userConn.JoinCode).SendAsync("leaveGame");
            }
        }

        private void UpdateActiveState(Game currentGame)
        {
            currentGame.IsActive = !currentGame.IsActive;
            var affectedRows = _gameRepository.UpdateActiveState(currentGame.Id, currentGame.IsActive, out string error);

            if (affectedRows == 0 || !string.IsNullOrEmpty(error))
            {
                Console.WriteLine(error);
                throw new Exception(error);
            } 
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            if (_sharedDB.Connection.TryGetValue(Context.ConnectionId, out UserConnection? userConn))
            {
                // Remove connection from sharedDB and send a message to the group
                _sharedDB.Connection.Remove(Context.ConnectionId, out _);
                Clients.Group(userConn.JoinCode).SendAsync("Message", $"{userConn.Username} har lämnat rummet", "info");

                // Find all remaining users in the game
                var countUsersInGame = _sharedDB.Connection.Count(e => e.Value.JoinCode == userConn.JoinCode);

                try
                {
                    GetGameAndRound(userConn.JoinCode, out Game? currentGame, out GameRound? currentRound);
                    currentGame ??= new Game();
                    currentRound ??= new GameRound();
                    string error = "";

                    // If no remaining users are in the game and it is marked as active, change the active state to false
                    if (countUsersInGame == 0 && currentGame.IsActive)
                    {
                        UpdateActiveState(currentGame);
                    }

                    // If a game has no round nor remaining users or if game has no users and round is 1 and round is not finished then delete the game
                    if ((currentRound.Id == 0 && countUsersInGame == 0) || (countUsersInGame == 0 && currentRound.RoundNr == 1 && currentRound.RoundComplete == false))
                    {
                        DeleteGame(currentGame);
                    }

                    // If game is active and has rounds
                    if (currentGame.IsActive && currentRound.Id != 0)
                    {
                        // Get the current round's users, find the user that left the game
                        var users = _userRepository.GetUsersByRound(currentRound.Id, out error);
                        var disconnectedUser = users?.Find(u => u.Info.Id == userConn.Id) ?? new UserVM();
                        if (disconnectedUser.TotalRoundPoints == 0)
                        {
                            // Delete user in round if it hasn't received any points
                            var affectedRows = _userRepository.DeleteUserInRound(disconnectedUser.Round.Id, out error);

                            if (!string.IsNullOrEmpty(error))
                            {
                                Clients.Group(userConn.JoinCode).SendAsync("Message", $"Ett fel uppstod: {error}", "warning");
                            }
                            users?.Remove(disconnectedUser);
                            UsersInRound(currentRound.Id, userConn.JoinCode);    
                        }
                        // If a round has no users and it is not completed
                        if ((users == null || users.Count == 0)&& currentRound.RoundComplete == false) {
                            // Delete game_round
                            var affectedRows = _gameRoundRepository.Delete(currentRound.Id, out error);
                            if (affectedRows == 0 || string.IsNullOrEmpty(error))
                            {
                                Clients.Group(userConn.JoinCode).SendAsync("Message", $"Ett fel uppstod: {error}", "warning");
                            }
                            if ( !string.IsNullOrEmpty(error))
                            {
                                Clients.Group(userConn.JoinCode).SendAsync("Message", $"Ett fel uppstod: {error}", "warning");
                            }
                        }
                    }
                    else {
                        UsersInGame(userConn.JoinCode);    
                    }
                }
                catch (Exception ex)
                {
                    Clients.Group(userConn.JoinCode).SendAsync("Message", $"Ett fel uppstod: {ex.Message}", "warning");
                }
            }
            return base.OnDisconnectedAsync(exception);
        }

        private string DeleteGame(Game currentGame)
        {
            string error;
            var affectedRows = _gameRepository.Delete(currentGame.Id, out error);
            if (affectedRows == 0 || string.IsNullOrEmpty(error))
            {
                Console.WriteLine("Deleted game");
            }

            return error;
        }
    }
}