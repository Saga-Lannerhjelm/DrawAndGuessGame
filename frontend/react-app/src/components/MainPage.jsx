import React, { useEffect, useState } from "react";
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { useNavigate } from "react-router-dom";
import { useConnection } from "../context/ConnectionContext";
import GameMessage from "./GameMessage";
import { use } from "react";

const Home = () => {
  const [roomName, setRoomName] = useState("");
  const [username, setUsername] = useState("");
  const [inviteCode, setInviteCode] = useState("");
  const [gameMessage, setGameMessage] = useState("");

  const { setConnection, connection, setActiveUserId, setUsers } =
    useConnection();

  const navigate = useNavigate();

  useEffect(() => {
    setTimeout(() => {
      setGameMessage("");
    }, 3000);
  }, [gameMessage]);

  const createRoom = async (roomName) => {
    const gameRoomCode = Math.round(Math.random() * 100000000);
    let uId;

    try {
      var { userId, userNm } = await addUser("test first");
      uId = userId;
      console.log("username efter fetch:", userNm);
      setUsername(userNm);
    } catch (error) {
      console.error("Kunde inte lägga till användare:", error);
    }

    if (uId) {
      const game = {
        RoomName: roomName,
        JoinCode: gameRoomCode.toString(),
        IsActive: false,
        CreatorId: uId,
      };

      try {
        const response = await fetch("http://localhost:5034/Game", {
          method: "POST",
          headers: {
            Accept: "application/json",
            "Content-Type": "application/json",
          },
          body: JSON.stringify(game),
        });

        if (response.ok) {
          console.log("right before join");
          joinRoom(gameRoomCode, uId, userNm);
        } else {
          console.error(
            "Fel vid API-anrop:",
            response.status,
            await response.text()
          );
          throw new Error("API-anrop misslyckades");
        }
      } catch (error) {
        console.error("Ett fel inträffade:", error);
      }
    }
  };

  const joinExistingRoom = async () => {
    let uId;
    try {
      var { gameExists, error } = await checkIfGameExists();
      if (gameExists) {
        var { userId, userNm } = await addUser("test first");
        uId = userId;
        setUsername(userNm);
        if (userId) {
          joinRoom(inviteCode, userId, userNm);
        }
      } else {
        if (error) {
          setGameMessage(error);
        } else {
          setGameMessage(
            "Rummet du försöker ansluta till finns inte, eller så har spelet startat"
          );
        }
      }
    } catch (error) {
      console.error("Kunde inte lägga till användare:", error);
    }
  };

  const checkIfGameExists = async () => {
    const response = await fetch("http://localhost:5034/Game/room", {
      method: "POST",
      headers: {
        Accept: "application/json",
        "Content-Type": "application/json",
      },
      body: JSON.stringify(inviteCode),
    });

    if (response.ok) {
      const existingUser = await response.json();
      return { gameExists: existingUser, error: null };
    } else {
      console.error(
        "Fel vid API-anrop:",
        response.status,
        await response.text()
      );
      return { gameExists: false, error: "Ett fel inträffade" };
    }
  };

  const addUser = async (username) => {
    console.log("in add");
    try {
      const response = await fetch("http://localhost:5034/User", {
        method: "POST",
        headers: {
          Accept: "application/json",
          "Content-Type": "application/json",
        },
        body: JSON.stringify(username),
      });

      if (response.ok) {
        const data = await response.json();
        return { userId: data.userId, userNm: data.username };
      } else {
        console.error(
          "Fel vid API-anrop:",
          response.status,
          await response.text()
        );
        throw new Error("API-anrop misslyckades");
      }
    } catch (error) {
      console.error("Ett fel inträffade:", error);
    }
  };

  const joinRoom = async (gameRoomCode, userId, username) => {
    startConnection(username, gameRoomCode, userId);
    setActiveUserId(userId);
  };

  async function startConnection(name, gameRoomCode, userId) {
    if (!connection) {
      const newConnection = new HubConnectionBuilder()
        .withUrl("http://localhost:5034/draw")
        .configureLogging(LogLevel.Information)
        .withAutomaticReconnect()
        .build();

      newConnection.on("GameStatus", (msg, isSuccess) => {
        console.log("GameStatus", msg);
        setGameMessage(msg);

        if (isSuccess) {
          setConnection(newConnection);
          navigate(`/game/${gameRoomCode}`);
        }
      });

      newConnection.on("usersInGame", (userValues) => {
        setUsers(userValues);
      });

      newConnection.onclose(() => {
        setConnection();
        setRoomName("");
      });

      try {
        await newConnection.start();
        const joinCode = gameRoomCode.toString();

        newConnection.invoke("JoinGame", {
          id: userId,
          username: name,
          joinCode,
        });
      } catch (error) {
        console.error();
      }
    }
  }

  return (
    <>
      {gameMessage != "" && <GameMessage msg={gameMessage} />}
      <h2>Hem</h2>
      <h4>- Skapa ett spel -</h4>
      <div>
        <form
          className="standard-form"
          onSubmit={(e) => {
            e.preventDefault();
            createRoom(roomName);
          }}
        >
          {/* <input
            type="text"
            placeholder="Användarnamn"
            onChange={(e) => setUserName(e.target.value)}
          /> */}
          <input
            type="text"
            placeholder="Namn på spelrummet"
            onChange={(e) => setRoomName(e.target.value)}
          />
          <button className="btn" type="submit" disabled={roomName == ""}>
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
            joinExistingRoom();
          }}
        >
          <input
            type="text"
            placeholder="Anslutningskod"
            onChange={(e) => setInviteCode(e.target.value)}
            value={inviteCode}
            maxLength={8}
            minLength={8}
          />
          <button type="submit" className="btn" disabled={inviteCode == ""}>
            Gå med i spel
          </button>
        </form>
      </div>
    </>
  );
};

export default Home;
