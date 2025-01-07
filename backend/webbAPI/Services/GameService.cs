using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webbAPI.DataService;
using webbAPI.Models;
using webbAPI.Models.ViewModels;
using webbAPI.Repositories;

namespace webbAPI.Services
{
    public class GameService(SharedDB sharedDB, GameRepository gameRepository, GameRoundRepository gameRoundRepository, UserRepository userRepository)
    {
        private readonly SharedDB _sharedDB = sharedDB;
        private readonly GameRepository _gameRepository = gameRepository;
        private readonly GameRoundRepository _gameRoundRepository = gameRoundRepository;
        private readonly UserRepository _userRepository = userRepository;

        public async Task<(bool gameIsFinished, Game game, GameRound round)> EndRound(string roomCode, Dictionary<int, int> drawingAmmounts)
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
                    if (drawingAmmounts.Count != 0)
                    {
                        if (user.Info.Id == drawingAmmounts.MaxBy(e => e.Value).Key)
                        {
                            if (allGuessingUsers?.Count == allCorrectGuessingUsers.Count)
                            {
                                AddPoints(user, 4);
                                
                            } else if (allCorrectGuessingUsers.Count >= 1){
                                AddPoints(user, 3);
                            }
                        }
                        else if (user.Info.Id == drawingAmmounts.MinBy(e => e.Value).Key)
                        {
                        if (allGuessingUsers?.Count == allCorrectGuessingUsers.Count)
                            {
                                AddPoints(user, 3);
                                
                            } else if (allCorrectGuessingUsers.Count >= 1){
                                AddPoints(user, 2);
                            }
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

            if (round.RoundNr >= currentGame.Rounds)
            {
                if (users?.Find(u => u.TotalRoundPoints > 0) != null)
                {
                    var allWinners = users?.FindAll(u => u.TotalRoundPoints == users?.MaxBy(c => c?.TotalRoundPoints).TotalRoundPoints) ?? new List<UserVM>();
                    foreach (var winnerInGame in allWinners)
                    {
                        winnerInGame.Info.Wins ++;
                        affectedRows = _userRepository.UpdateUser(winnerInGame.Info, out error);

                        if (!string.IsNullOrEmpty(error))
                        {
                            throw new Exception(error);
                        }
                    }
                }
                return (true, currentGame, round);
            }
            return (false, currentGame, round);
        }

        private void GetGameAndRound(string roomCode, out Game currentGame, out GameRound? currentRound)
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


    }
}