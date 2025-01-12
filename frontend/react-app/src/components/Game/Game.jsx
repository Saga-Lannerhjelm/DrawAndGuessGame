import React, { useEffect, useRef, useState } from "react";
import { useConnection } from "../../context/ConnectionContext";
import { useNavigate, useParams } from "react-router-dom";
import DrawingBoard from "./DrawingBoard";
import UserContainer from "./GuessContainer";
import Header from "../Header";
import GuessForm from "./GuessForm";
import TopSection from "./TopSection";
import ResultCard from "./ResultCard";
import GameMessage from "../GameMessage";

const Game = () => {
  const { connection, activeUserId, users } = useConnection();
  const navigate = useNavigate();
  const params = useParams();
  const timeOutRef = useRef(null);

  const [roomName, setRoomName] = useState("");
  const [joinCode, setJoinCode] = useState("");
  const [isDrawing, setIsDrawing] = useState(false);
  const [gameActive, setGameActive] = useState(false);
  const [round, setRound] = useState(undefined);
  const [roomOwner, setRoomOwner] = useState(undefined);
  const [showFinalResult, setShowFinalResult] = useState(false);
  const [roundComplete, setRoundComplete] = useState(false);
  const [time, setTime] = useState(30);
  const [userGuesses, setUserGuesses] = useState([]);
  const [roundNr, setRoundNr] = useState(3);
  const [gameMessage, setGameMessage] = useState({});

  useEffect(() => {
    setTimeout(() => {
      setGameMessage("");
    }, 3000);
  }, [gameMessage]);

  useEffect(() => {
    if (connection === undefined) {
      navigate("/");
    }
    if (connection) {
      setJoinCode(params.room);

      connection.on("Message", (msg, type) => {
        setGameMessage({ msg: msg, type: type });
      });

      connection.on("receiveGameInfo", (game, round) => {
        setRoomName(game.roomName);
        setGameActive(game.isActive);
        setRoomOwner(game.creatorId);
        if (round.id != 0) {
          setRound(round);

          setTimeout(() => {
            setRoundComplete(round.roundComplete);
          }, 1000);
        }
      });

      connection.on("ReceiveTimerData", (time) => {
        time = time.length > 1 ? "0" + time : time;
        setTime(time);
      });

      connection.on("ReceiveGuess", (guess, userId) => {
        setUserGuesses((prevGuesses) => {
          const existingGuessIndex = prevGuesses.findIndex(
            (g) => g.userId === userId
          );
          if (existingGuessIndex !== -1) {
            const updatedGuesses = [...prevGuesses];
            updatedGuesses[existingGuessIndex] = { userId, guess };
            return updatedGuesses;
          } else {
            return [...prevGuesses, { userId, guess }];
          }
        });
        displayGuess(userId);
      });

      connection.on("EndRound", (joinCode) => {
        connection.invoke("EndRound", joinCode);
      });

      connection.on("GameFinished", () => {
        setShowFinalResult(true);
      });

      connection.on("leaveGame", () => {
        leaveGame();
      });
    }
  }, [connection]);

  const startRound = async (roundNr) => {
    if (connection) {
      setTime(30);
      setRoundComplete(false);
      await connection.invoke("StartRound", joinCode, parseInt(roundNr));
    }
  };

  const sendGuess = async (guess) => {
    if (connection) {
      await connection.invoke("SendGuess", guess.toLowerCase());
    }
  };

  const displayGuess = (userId) => {
    if (timeOutRef.current) {
      clearTimeout(timeOutRef.current);
    }
    // bara det senaste skrivna meddelandet försvinner
    timeOutRef.current = setTimeout(() => {
      setUserGuesses((prevGuesses) => {
        const existingGuessIndex = prevGuesses.findIndex(
          (g) => g.userId === userId
        );
        const guesses = [...prevGuesses];
        guesses.splice(existingGuessIndex, 1);
        return guesses;
      });
      timeOutRef.current = null;
    }, 4000);
  };

  const leaveGame = async () => {
    if (connection) {
      await connection.stop();
      navigate("/");
    }
  };

  const endGame = async () => {
    if (connection) {
      await connection.invoke("EndGame");
    }
  };

  return (
    <>
      {gameMessage.msg !== undefined && (
        <GameMessage msg={gameMessage.msg} type={gameMessage.type} />
      )}
      <Header
        roomName={roomName}
        joinCode={joinCode}
        onclick={leaveGame}
        endGame={endGame}
        roomOwner={roomOwner}
        activeUserId={activeUserId}
      />
      <div className="game-container">
        <div>
          {roundComplete ? (
            showFinalResult ? (
              <ResultCard showGameResult={true} endGame={endGame} />
            ) : (
              <ResultCard
                startNewRound={() => startRound(roundNr)}
                showGameResult={false}
                roundData={round}
              />
            )
          ) : gameActive && round ? (
            <>
              <TopSection time={time} round={round} roundNr={roundNr} />
              <div id="canvas-container">
                <DrawingBoard
                  gameRoom={joinCode}
                  gameActive={gameActive}
                  isDrawing={isDrawing}
                  round={round}
                />
              </div>
              {!isDrawing && <GuessForm sendGuess={sendGuess} />}
            </>
          ) : roomOwner && roomOwner == activeUserId ? (
            <form
              onSubmit={(e) => {
                e.preventDefault();
                startRound(roundNr);
              }}
              className="start-round-form"
            >
              <div>
                <label htmlFor="nrInput">Välj antal rundor:</label>
                <input
                  id="nrInput"
                  type="number"
                  min={3}
                  max={10}
                  value={roundNr}
                  onChange={(e) => setRoundNr(e.target.value)}
                />
              </div>
              <button type="submit" className="btn" disabled={users.length < 3}>
                {users.length >= 3
                  ? "Är alla spelare inne? Starta spelet"
                  : "Spelet måste minst ha tre spelare"}
              </button>
            </form>
          ) : (
            <p>Väntar på att ägaren startar spelet...</p>
          )}
        </div>
        <UserContainer
          userIsDrawing={(bool) => setIsDrawing(bool)}
          userGuesses={userGuesses}
          isActive={gameActive}
          roomOwner={roomOwner}
        />
      </div>
    </>
  );
};

export default Game;
