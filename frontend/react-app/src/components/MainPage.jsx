import React, { useEffect, useState } from "react";
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { useNavigate } from "react-router-dom";
import { useConnection } from "../context/ConnectionContext";

const Home = () => {
  const [userName, setUserName] = useState("");
  const [randomUsername, setRandomUsername] = useState("");
  const [inviteCode, setInviteCode] = useState("");
  // const [connection, setConnection] = useState(undefined);
  const [loading, setLoading] = useState(true);

  const { setConnection, connection } = useConnection();

  const navigate = useNavigate();

  useEffect(() => {
    getRandomUsername().then((userN) => setRandomUsername(userN));
  }, []);

  // Also check so that it is unique compared to the existing games in the database
  const createRoom = async (userName) => {
    const gameRoomCode = Math.round(Math.random() * 100000000);
    startConnection(userName, gameRoomCode);
  };

  const joinRoom = async (gameRoomCode) => {
    if (!loading) {
      startConnection(randomUsername, gameRoomCode);
    }
  };

  async function startConnection(userName, gameRoomCode) {
    if (!connection) {
      const connection = new HubConnectionBuilder()
        .withUrl("http://localhost:5034/draw")
        .configureLogging(LogLevel.Information)
        .withAutomaticReconnect()
        .build();

      connection.on("GameStatus", (msg) => {
        console.log(msg);
      });

      try {
        await connection.start();
        const gameRoom = gameRoomCode.toString();
        connection.invoke("JoinGame", { userName, gameRoom, role: "hello" });
        setConnection(connection);
        navigate(`/game/${gameRoomCode}`);
      } catch (error) {
        console.error();
      }
    }
  }

  async function getRandomUsername() {
    try {
      // Link to API https://github.com/randomusernameapi/randomusernameapi.github.io?tab=readme-ov-file
      const response = await fetch(
        "https://usernameapiv1.vercel.app/api/random-usernames"
      );

      if (!response.ok) throw new Error(`Response status: ${response.status}`);
      const result = await response.json();
      return result.usernames[0];
    } catch (error) {
      console.error(error);
      return null;
    } finally {
      setLoading(false);
    }
  }

  return (
    <>
      <h2>Hem</h2>
      <h4>- Skapa ett spel -</h4>
      <div>
        <form
          className="standard-form"
          onSubmit={(e) => {
            e.preventDefault();
            createRoom(userName);
          }}
        >
          <input
            type="text"
            placeholder="Användarnamn"
            onChange={(e) => setUserName(e.target.value)}
          />
          <button className="btn" type="submit">
            Skapa rum
          </button>
        </form>
      </div>
      <h4>- Eller anlut till ett rum med en anslutningskod -</h4>
      <div>
        <form
          className="standard-form"
          onSubmit={(e) => {
            e.preventDefault();
            joinRoom(inviteCode);
          }}
        >
          <input
            type="text"
            placeholder="Anslutningskod"
            onChange={(e) => setInviteCode(e.target.value)}
            value={inviteCode}
          />
          <button type="submit" className="btn">
            Gå med i spel
          </button>
        </form>
      </div>
    </>
  );
};

export default Home;
