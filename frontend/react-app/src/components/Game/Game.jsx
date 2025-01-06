import React, { useEffect, useRef, useState } from "react";
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { useConnection } from "../../context/ConnectionContext";
import { useNavigate, useParams } from "react-router-dom";
import DrawingBoard from "./DrawingBoard";
import UserContainer from "./GuessContainer";
import Users from "./Users";
import Header from "../Header";
import GuessForm from "./GuessForm";
import DrawingInfo from "./DrawingInfo";
import TopSection from "./TopSection";
import ResultCard from "./ResultCard";

const Game = () => {
  const { connection, activeUserId, users } = useConnection();
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
        // setRound(round + 1);
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
        displayMessage(userId);
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
        // console.log(time);
      });

      connection.on("GameFinished", () => {
        setShowFinalResult(true);
      });

      connection.on("EndRound", (joinCode) => {
        console.log("in ended round");
        connection.invoke("EndRound", joinCode);
      });

      connection.on("leaveGame", () => {
        connection.stop();
      });
    }
  }, [connection]);

  const displayMessage = (userId) => {
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

  const leaveRoom = async () => {
    await connection.stop();
    navigate("/home");
  };

  const startRound = async () => {
    if (connection) {
      setTime(30);
      // setRoundStarted(true);
      setRoundComplete(false);
      await connection.invoke("StartRound", joinCode);
      // await connection.invoke("SendTimerData", time);
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
                startNewRound={startRound}
                showGameResult={false}
                roundData={round}
              />
            )
          ) : gameActive ? (
            <>
              <TopSection time={time} round={round} />
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
            <button
              onClick={startRound}
              className="btn"
              disabled={users.length < 3}
            >
              {users.length >= 3
                ? "Är alla spelare inne? Starta spelet"
                : "Spelet måste minst ha tre spelare"}
            </button>
          ) : (
            <p>Vänar på att ägaren startar spelet...</p>
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
