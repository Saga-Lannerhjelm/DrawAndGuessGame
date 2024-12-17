import React, { useState } from "react";
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";

const Home = () => {
  const [userName, setUserName] = useState("");
  const [inviteCode, setInviteCode] = useState("");
  const [connection, setConnection] = useState(undefined);

  // Also check so that it is unique compared to the existing games in the database
  const createRoom = async (userName) => {
    const gameRoomCode = Math.round(Math.random() * 100000000);
    startConnection(userName, gameRoomCode);
  };

  const joinRoom = async (userName, gameRoomCode) => {
    startConnection(userName, gameRoomCode);
  };

  async function startConnection(userName, gameRoomCode) {
    if (!connection) {
      const connection = new HubConnectionBuilder()
        .withUrl("http://localhost:5034/draw")
        .configureLogging(LogLevel.Information)
        .withAutomaticReconnect()
        .build();

      connection.on("JoinedGame", (msg) => {
        console.log(msg);
      });

      try {
        await connection.start();
        connection.invoke("JoinGame", userName, gameRoomCode.toString());
        setConnection(connection);
      } catch (error) {
        console.error();
      }
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
            joinRoom(userName, inviteCode);
          }}
        >
          <input
            type="text"
            placeholder="Användarnamn"
            onChange={(e) => setUserName(e.target.value)}
          />
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
