import React, { useEffect, useRef, useState } from "react";
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { useConnection } from "../../context/ConnectionContext";
import { useNavigate, useParams } from "react-router-dom";
import DrawingBoard from "./DrawingBoard";
import GuessContainer from "./GuessContainer";
import Users from "./Users";
import Header from "../Header";
import GuessForm from "./GuessForm";
import DrawingInfo from "./DrawingInfo";
import TopSection from "./TopSection";
import ResultCard from "./ResultCard";

const Game = () => {
  const { connection } = useConnection();
  const [roomName, setRoomName] = useState("");
  const [joinCode, setJoinCode] = useState("");
  const [isDrawing, setIsDrawing] = useState(false);
  const [gameActive, setGameActive] = useState(false);
  const [round, setRound] = useState(0);
  const [roundStarted, setRoundStarted] = useState(false);
  const [roundComplete, setRoundComplete] = useState(false);
  const [word, setWord] = useState("");
  const [time, setTime] = useState(30);
  const [userGuesses, setUserGuesses] = useState([]);

  const navigate = useNavigate();
  const params = useParams();
  const timeOutRef = useRef(null);

  useEffect(() => {
    if (connection === undefined) {
      navigate("/home");
    }
    if (connection) {
      setJoinCode(params.room);

      connection.on("GameCanStart", (canStart) => {
        // setGameActive(canStart);
        // setRound(round + 1);
      });

      connection.on("ReceiveGuess", (guess, user) => {
        setUserGuesses((prevGuesses) => {
          const existingGuessIndex = prevGuesses.findIndex(
            (g) => g.user === user
          );
          if (existingGuessIndex !== -1) {
            const updatedGuesses = [...prevGuesses];
            updatedGuesses[existingGuessIndex] = { user, guess };
            return updatedGuesses;
          } else {
            return [...prevGuesses, { user, guess }];
          }
        });
        displayMessage(user);
      });

      connection.on("receiveGameInfo", (game, round) => {
        setRoomName(game.roomName);
        setGameActive(game.hasStarted);
        setRound(game.rounds.length);
        setWord(round.word);
        setRoundComplete(round.roundComplete);
      });

      connection.on("ReceiveTimerData", (time) => {
        setTime(time);
      });

      connection.on("RoundEnded", (time) => {
        setRoundStarted(false);
      });

      connection.on("leaveGame", () => {
        connection.stop();
      });
    }
  }, [connection]);

  const displayMessage = (user) => {
    console.log("in display");
    if (timeOutRef.current) {
      clearTimeout(timeOutRef.current);
    }
    // bara det senaste skrivna meddelandet försvinner
    timeOutRef.current = setTimeout(() => {
      setUserGuesses((prevGuesses) => {
        const existingGuessIndex = prevGuesses.findIndex(
          (g) => g.user === user
        );
        const guesses = [...prevGuesses];
        guesses.splice(existingGuessIndex, 1);
        return guesses;
      });
      timeOutRef.current = null;
    }, 4000);
  };

  useEffect(() => {
    let interval;
    if (time >= 0 && roundStarted) {
      interval = setInterval(() => {
        updateTime();
      }, 1000);
    }
    return () => clearInterval(interval);
  }, [roundStarted, time]);

  const updateTime = async () => {
    await connection.invoke("SendTimerData", time);
  };

  const leaveRoom = async () => {
    console.log("leave");
    await connection.stop();
    navigate("/home");
  };

  const startRound = async () => {
    if (connection) {
      setTime(30);
      setRoundStarted(true);
      setRoundComplete(false);
      await connection.invoke("StartRound", joinCode);
    }
  };

  const sendGuess = async (guess) => {
    if (connection) {
      await connection.invoke("SendGuess", guess.toLowerCase());
    }
  };

  const endGame = async () => {
    if (connection) {
      await connection.invoke("EndGame");
    }
  };

  return (
    <>
      <Header
        roomName={roomName}
        joinCode={joinCode}
        onclick={leaveRoom}
        endGame={endGame}
      />
      <div className="game-container">
        <div>
          {roundComplete ? (
            <ResultCard startNewRound={startRound} />
          ) : gameActive ? (
            <>
              <TopSection time={time} round={round} />
              <div id="canvas-container">
                <DrawingBoard
                  gameRoom={joinCode}
                  gameActive={gameActive}
                  isDrawing={isDrawing}
                  word={word}
                />
              </div>
              {!isDrawing && <GuessForm sendGuess={sendGuess} />}
            </>
          ) : (
            <button onClick={startRound} className="btn">
              Starta spelet
            </button>
          )}
        </div>
        <GuessContainer
          // gameRoom={gameRoom}
          userIsDrawing={(bool) => setIsDrawing(bool)}
          userGuesses={userGuesses}
        />
      </div>
    </>
  );
};

export default Game;
