using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using webbAPI.DataService;
using webbAPI.Models;
using webbAPI.Models.ViewModels;
using webbAPI.Repositories;

namespace webbAPI.Hubs
{
    public class DrawHub : Hub
    {   
        private readonly SharedDB _sharedDB;
        private readonly GameRepository _gameRepository;
        private readonly GameRoundRepository _gameRoundRepository;
        private readonly UserRepository _userRepository;

        private static readonly Dictionary<int, int> drawingAmmounts = [];

        public DrawHub (SharedDB sharedDB, GameRepository gameRepository, GameRoundRepository gameRoundRepository, UserRepository userRepository)
        {
            _sharedDB = sharedDB;
            _gameRepository = gameRepository;
            _gameRoundRepository = gameRoundRepository;
            _userRepository = userRepository;
        }
        public async Task JoinGame (UserConnection userConn) 
        {
            // If game exist
            var existingGame = _gameRepository.GetGameByJoinCode(userConn.JoinCode, out string error) ?? new Game();

            if (existingGame?.Id != 0 && string.IsNullOrEmpty(error) && !existingGame.IsActive)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userConn.JoinCode);
                // Add user (to game)
                _sharedDB.Connection[Context.ConnectionId] = userConn;

                 await Clients.OthersInGroup(userConn.JoinCode).SendAsync("GameStatus", $"{userConn.Username} anslöt till spelet", true);
                await Clients.Caller.SendAsync("GameStatus", $"Välkommen till spelet. Anslutningkoden är {userConn.JoinCode}", true);

                await UsersInGame(userConn.JoinCode);
                await GameInfo(userConn.JoinCode);
            }
            else 
            {
                await Clients.Caller.SendAsync("GameStatus", $"Spelet finns inte eller har redan startat", false);
            }
        }

        public async Task StartRound(string joinCode)
        {
            var allUsersInGame = _sharedDB.Connection.Values
            .Where(g => g.JoinCode == joinCode).ToList();

            int usersInGameNr = allUsersInGame.Count;

            try
            {
                if (usersInGameNr >= 3)
                {
                    // Current game
                    // Get game by join code
                    string error = "";
                    var currentGame = _gameRepository.GetGameByJoinCode(joinCode, out error) ?? new Game();

                    if (string.IsNullOrEmpty(error) && currentGame.Id != 0)
                    {
                        if (!currentGame.IsActive)
                        {
                            // Update gamestate to active
                            UpdateActiveState(currentGame);
                            currentGame.IsActive = true;
                        }

                        // Get word
                        string word = "default word";
                        try
                        {
                            word = await _gameRoundRepository.GetWord();  
                        }
                        catch (Exception ex)
                        {
                            
                            Console.WriteLine($"An error occurred: {ex.Message}");
                        }

                        // Add a new round
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
                        var usersInRoundList = new List<UserInRound>();

                        foreach (var user in allUsersInGame)
                        {
                            usersInRoundList.Add(new UserInRound {
                                UserId = user.Id,
                                IsDrawing = false,
                                GameRoundId = roundId,
                            });
                        }

                        // Select users to draw
                        var rnd = new Random();

                        int randomNr1 = rnd.Next(usersInGameNr);
                        int randomNr2 = rnd.Next(usersInGameNr);

                        while (randomNr2 == randomNr1)
                        {
                            randomNr2 = rnd.Next(usersInGameNr);
                        }

                        var drawingUserOne = allUsersInGame[randomNr1];
                        var drawingUserTwo = allUsersInGame[randomNr2];

                        usersInRoundList[randomNr1].IsDrawing = true;
                        usersInRoundList[randomNr2].IsDrawing = true;

                        // Make sure that only the ones that hasn't drawn yet are selected
                        // Maybe filter out the ones that already have drawn

                        // Add users to the round
                        foreach (var userInRound in usersInRoundList)
                        {
                            var affectedRows = _userRepository.InsertUserInRound(userInRound, out error);

                            if (affectedRows == 0 || !string.IsNullOrEmpty(error))
                            {
                                throw new Exception(error);
                            }
                        }
                        await Clients.Group(joinCode).SendAsync("GameCanStart", true);
                        await Clients.Group(joinCode).SendAsync("GameStatus", $"{drawingUserOne.Username} och {drawingUserTwo.Username} ritar!");
                        await UsersInRound(roundId, currentGame.JoinCode);
                        await GameInfo(joinCode);
                        // currentGame.Rounds[^1].Users.Add(new User{UserDetails = user});
                        
                    }
                    else {
                         throw new Exception(error);
                    }
                   
                }
                else 
                {
                    await Clients.Group(joinCode).SendAsync("GameCanStart", false);
                }
            }
            catch (Exception ex)
            {
                
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        public async Task RequestNewWord (string gameRoom, GameRound round) {
            string newWord = await _gameRoundRepository.GetWord();
            round.Word = newWord;

            var users = _userRepository.GetUsersByRound(round.Id, out string error) ?? new List<UserVM>();

            if (users.Count > 0 || string.IsNullOrEmpty(error))
            {
                foreach (var user in users)
                {
                    if (!user.Round.IsDrawing)
                    {
                        user.Round.GuessedCorrectly = false;
                        user.Round.GuessedFirst = false;
                        var rows = _userRepository.UpdateUserInRound(user.Round, out error);
                        
                        if (!string.IsNullOrEmpty(error) || rows == 0)
                        {
                            Console.WriteLine(error);
                        }
                    }
                }
                var affectedRows = _gameRoundRepository.Update(round, out error);
                if (string.IsNullOrEmpty(error) || affectedRows != 0)
                {
                    await UsersInRound(round.Id, gameRoom);
                    await GameInfo(gameRoom); 
                }
            }

        }

        public async Task Drawing(Point start, Point end, string color, string gameRoom) 
        {
            if (_sharedDB.Connection.TryGetValue(Context.ConnectionId, out UserConnection? userConn))
            {
                if (drawingAmmounts.TryGetValue(userConn.Id, out int value))
                {
                    drawingAmmounts[userConn.Id] = value + 1;
                } else 
                {
                    drawingAmmounts.Add(userConn.Id, 1);
                }

                Console.WriteLine(drawingAmmounts);
                await Clients.OthersInGroup(gameRoom).SendAsync("Drawing", start, end, color, userConn.Id);
            }
        }

        public async Task SendGuess(string guess)
        {
            if (_sharedDB.Connection.TryGetValue(Context.ConnectionId, out UserConnection? userConn))
            {
                try
                {
                    GetGameAndRound(userConn.JoinCode, out Game? currentGame, out GameRound? currentRound);
                    currentGame ??= new Game();
                    currentRound ??= new GameRound();

                    if (guess == currentRound?.Word)
                    {
                        var users = _userRepository.GetUsersByRound(currentRound.Id, out string error);

                        var guessingUser = users?.Find(u => u.Info.Username == userConn.Username) ?? new UserVM();

                        if (users == null || !string.IsNullOrEmpty(error))
                        {
                            throw new Exception(error);
                        }

                        if (!users.Where(user => user.Round.GuessedCorrectly).Any())
                        {
                            guessingUser.Round.GuessedFirst = true;
                        }

                        guessingUser.Round.GuessedCorrectly = true;


                        // Update user
                        var effectedRows = _userRepository.UpdateUserInRound(guessingUser.Round, out error);

                        if (effectedRows == 0 && string.IsNullOrEmpty(error))
                        {
                            throw new Exception(error);
                        }

                        if (!users.Where(user => !user.Round.IsDrawing && !user.Round.GuessedCorrectly).Any())
                        {
                            await EndRound(userConn.JoinCode);
                        } else {
                            await UsersInRound(currentRound.Id, userConn.JoinCode);
                            await GameInfo(currentGame.JoinCode);
                        }

                        await Clients.Caller.SendAsync("ReceiveGuess", guess, userConn.Id);
                        await Clients.OthersInGroup(currentGame.JoinCode).SendAsync("ReceiveGuess", "Gissade rätt", userConn.Id);
                    }
                    else
                    {
                        await Clients.Group(currentGame.JoinCode).SendAsync("ReceiveGuess", guess, userConn.Id);
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error:", ex);
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
            try
            {
                var users = _sharedDB.Connection
                .Where(g => g.Value.JoinCode == gameRoom).ToList();
                var userValues = users.Select((users) => users.Value);

                return Clients.Group(gameRoom).SendAsync("UsersInGame", userValues);
                
            }
            catch (Exception)
            {
                return null;
            }
        }

        public Task UsersInRound (int roundId, string joinCode) 
        {
            var users = _userRepository.GetUsersByRound(roundId, out string error);

            if (users == null || !string.IsNullOrEmpty(error))
            {
                Console.WriteLine("Errir: ", error);
                return Clients.Group(joinCode).SendAsync("UsersInGame", null);  
            }
            return Clients.Group(joinCode).SendAsync("UsersInGame", users);  
        }

        public Task GameInfo(string joinCode) 
        {
            string error = "";
            var currentGame = new Game();
            var currentRound = new GameRound();

            try
            {
                currentGame = _gameRepository.GetGameByJoinCode(joinCode, out error);

                if (currentGame != null && string.IsNullOrEmpty(error))
                {
                    currentRound = _gameRoundRepository.GetGameRoundByGameId(currentGame.Id, out error);

                    if (!string.IsNullOrEmpty(error))
                    {
                        throw new Exception(error);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error:", ex);
            }

            return Clients.Group(joinCode).SendAsync("receiveGameInfo", currentGame, currentRound);
        }

        public async Task SendClearCanvas (string gameRoom) 
        {
            await Clients.Group(gameRoom).SendAsync("clearCanvas");
        }

        public async Task EndRound (string roomCode) 
        {
            try
            {
                GetGameAndRound(roomCode, out Game? currentGame, out GameRound? round);
                currentGame ??= new Game();
                round ??= new GameRound();

                round.RoundComplete = true;

                var users = _userRepository.GetUsersByRound(round.Id, out string error);

                if (users == null || !string.IsNullOrEmpty(error))
                {
                    throw new Exception(error);
                }

                var winner = users?.Find(user => user.Round.GuessedFirst);
                if (winner != null)
                {
                    AddPoints(winner, 5);
                }

                var allGuessingUsers = users?.FindAll(user => !user.Round.IsDrawing) ?? [];
                var allCorrectGuessingUsers = allGuessingUsers?.FindAll(user => user.Round.GuessedCorrectly) ?? [];
                var usersGuessedCorrectlyButNotFirst = allGuessingUsers?.FindAll(user => !user.Round.GuessedFirst && user.Round.GuessedCorrectly) ?? [];
                
                if (usersGuessedCorrectlyButNotFirst.Count != 0 || winner != null)
                {
                    if (usersGuessedCorrectlyButNotFirst.Count != 0)
                    {
                        foreach (var user in usersGuessedCorrectlyButNotFirst)
                        {
                            AddPoints(user, 3);
                        }
                    }
                    var artists = users?.FindAll(user => user.Round.IsDrawing) ?? new List<UserVM>();

                    // Give points to artists
                    foreach (var user in artists)
                    {
                        if (user.Info.Id == drawingAmmounts.MaxBy(e => e.Value).Key)
                        {
                            if (allGuessingUsers.Count == allCorrectGuessingUsers.Count)
                            {
                                AddPoints(user, 4);
                                
                            } else if (allCorrectGuessingUsers.Count >= 1){
                                AddPoints(user, 3);
                            }
                        }
                        else if (user.Info.Id == drawingAmmounts.MinBy(e => e.Value).Key)
                        {
                           if (allGuessingUsers.Count == allCorrectGuessingUsers.Count)
                            {
                                AddPoints(user, 3);
                                
                            } else if (allCorrectGuessingUsers.Count >= 1){
                                AddPoints(user, 2);
                            }
                        }
                    }
                    drawingAmmounts.Clear();
                }


                var affectedRows = _gameRoundRepository.Update(round, out error);

                if (affectedRows == 0 || !string.IsNullOrEmpty(error))
                {
                    throw new Exception(error);
                }

                if (round.RoundNr >= 3)
                {
                    if (users?.Find(u => u.TotalRoundPoints > 0) != null)
                    {
                        var gameWinner = users?.MaxBy(c => c?.TotalRoundPoints)?.Info ?? new User();
                        if (gameWinner.Id != 0)
                        {
                            gameWinner.Wins ++;
                            affectedRows = _userRepository.UpdateUser(gameWinner, out error);

                            if (string.IsNullOrEmpty(error))
                            {
                                await Clients.Group(roomCode).SendAsync("GameFinished");
                            }
                        }
                    }
                }
                
            await Clients.Group(roomCode).SendAsync("RoundEnded");
            await UsersInRound(round.Id, currentGame.JoinCode);
            await GameInfo(currentGame.JoinCode);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void AddPoints(UserVM user, int points)
        {
            user.Round.Points = points;
            user.Info.TotalPoints += points;

            var affectedRows = _userRepository.AddPoints(user, out string error);
            if (affectedRows == 0 || !string.IsNullOrEmpty(error))
            {
                throw new Exception(error);
            }
        }

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
                _sharedDB.Connection.Remove(Context.ConnectionId, out _);
                Clients.Group(userConn.JoinCode).SendAsync("GameStatus", $"{userConn.Username} har lämnat spelet");

                var usersInGame = _sharedDB.Connection.Where(e => e.Value.JoinCode == userConn.JoinCode).ToList();

                try
                {
                    GetGameAndRound(userConn.JoinCode, out Game? currentGame, out GameRound? currentRound);
                    currentGame ??= new Game();
                    currentRound ??= new GameRound();
                    string error = "";

                    if (usersInGame.Count == 0 && currentGame.IsActive)
                    {
                        UpdateActiveState(currentGame);
                        GameInfo(currentGame.JoinCode);
                    }

                    // If game has no round or users
                    if (currentRound.Id == 0 && usersInGame.Count == 0)
                    {
                        var affectedRows = _gameRepository.Delete(currentGame.Id, out error);
                        if (affectedRows == 0 || string.IsNullOrEmpty(error))
                        {
                            Console.WriteLine("Deleted game");
                        }
                    }

                    // if game has no users and round is 1 and round is not finished
                    if ( usersInGame.Count == 0 && currentRound.RoundNr == 1 && currentRound.RoundComplete == false)
                    {
                        DeleteGame(currentGame);
                    }

                    if (currentGame.IsActive && currentRound.Id != 0)
                    {
                        var users = _userRepository.GetUsersByRound(currentRound.Id, out error);
                        var disconnectedUser = users?.Find(u => u.Info.Id == userConn.Id) ?? new UserVM();
                        if (disconnectedUser.TotalRoundPoints == 0)
                        {
                            // Delete user in round if it hasn't received any points
                            var affectedRows = _userRepository.DeleteUserInRound(disconnectedUser.Round.Id, out error);

                            if (!string.IsNullOrEmpty(error))
                            {
                                Console.WriteLine("Error: ", error);
                            }
                            users?.Remove(disconnectedUser);
                            UsersInRound(currentRound.Id, userConn.JoinCode);    
                        }

                        if ((users == null || users.Count == 0)&& currentRound.RoundComplete == false) {
                            // Delete game_round
                            var affectedRows = _gameRoundRepository.Delete(currentRound.Id, out error);
                            if (affectedRows == 0 || string.IsNullOrEmpty(error))
                            {
                                Console.WriteLine("Deleted round");
                            }
                            if ( !string.IsNullOrEmpty(error))
                            {
                                Console.WriteLine("Error: ", error);
                            }
                        }
                    }
                    else {
                        UsersInGame(userConn.JoinCode);    
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
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