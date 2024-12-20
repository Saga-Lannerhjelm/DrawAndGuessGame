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

const Game = () => {
  const { connection } = useConnection();
  const [gameRoom, setGameRoom] = useState("");
  const [isDrawing, setIsDrawing] = useState(false);
  const [gameActive, setGameActive] = useState(false);
  const [round, setRound] = useState(0);
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
      setGameRoom(params.room);

      connection.on("GameCanStart", (canStart) => {
        setGameActive(canStart);
        setRound(round + 1);
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

  // useEffect(() => {
  //   let interval;
  //   if (gameActive && time > 0) {
  //     interval = setInterval(() => {
  //       setTime((prevTime) => prevTime - 1);
  //     }, 1000);
  //   } else if (time === 0) {
  //     setGameActive(false);
  //   }
  //   return () => clearInterval(interval);
  // }, [time, gameActive]);

  const leaveRoom = async () => {
    console.log("leave");
    await connection.stop();
    navigate("/home");
  };

  const startRound = async () => {
    if (connection) {
      await connection.invoke("StartRound", gameRoom);
    }
  };

  const sendGuess = async (guess) => {
    if (connection) {
      await connection.invoke("SendGuess", guess);
    }
  };

  return (
    <>
      <Header gameRoom={gameRoom} onclick={leaveRoom} />
      <div className="game-container">
        <div>
          {gameActive ? (
            <>
              <TopSection time={time} round={round} />
              <div id="canvas-container">
                <DrawingBoard
                  gameRoom={gameRoom}
                  gameActive={gameActive}
                  isDrawing={isDrawing}
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
