import React, { useEffect, useState } from "react";
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { useNavigate } from "react-router-dom";
import { useConnection } from "../context/ConnectionContext";

const Home = () => {
  const [userName, setUserName] = useState("");
  const [roomName, setRoomName] = useState("");
  const [randomUsername, setRandomUsername] = useState("");
  const [inviteCode, setInviteCode] = useState("");
  const [gameStatusSuccess, setGameStatusSuccess] = useState(false);
  const [loading, setLoading] = useState(true);

  const { setConnection, connection, setActiveUser, setUsers } =
    useConnection();

  const navigate = useNavigate();

  useEffect(() => {
    getRandomUsername().then((userN) => setRandomUsername(userN));
  }, []);

  // Also check so that it is unique compared to the existing games in the database
  const createRoom = async (roomName) => {
    const gameRoomCode = Math.round(Math.random() * 100000000);
    let userId;

    try {
      userId = await addUser(randomUsername);
    } catch (error) {
      console.error("Kunde inte lägga till användare:", error);
    }

    if (userId) {
      const game = {
        RoomName: roomName,
        JoinCode: gameRoomCode.toString(),
        IsActive: false,
        CreatorId: userId,
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
          joinRoom(gameRoomCode, userId);
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
    let userId;
    try {
      userId = await addUser(randomUsername);
    } catch (error) {
      console.error("Kunde inte lägga till användare:", error);
    }

    if (userId) {
      joinRoom(inviteCode, userId);
    }
  };

  const addUser = async (userName) => {
    console.log("in add");
    try {
      const response = await fetch("http://localhost:5034/User", {
        method: "POST",
        headers: {
          Accept: "application/json",
          "Content-Type": "application/json",
        },
        body: JSON.stringify(userName),
      });

      if (response.ok) {
        const data = await response.json();
        return data;
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

  const joinRoom = async (gameRoomCode, userId) => {
    console.log("USerID in join:", userId);
    if (!loading) {
      startConnection(randomUsername, gameRoomCode, userId);
      setActiveUser(randomUsername);
    }
  };

  async function startConnection(userName, gameRoomCode, userId) {
    if (!connection) {
      const newConnection = new HubConnectionBuilder()
        .withUrl("http://localhost:5034/draw")
        .configureLogging(LogLevel.Information)
        .withAutomaticReconnect()
        .build();

      newConnection.on("GameStatus", (msg, isSuccess) => {
        console.log(msg);
        console.log("gameStatus", isSuccess);
        setGameStatusSuccess(isSuccess);

        if (isSuccess) {
          console.log("in success");
          setConnection(newConnection);
          navigate(`/game/${gameRoomCode}`);
        }
      });

      newConnection.on("usersInGame", (userValues) => {
        setUsers(userValues);
      });

      newConnection.onclose(() => {
        setConnection();
        setGameStatusSuccess(false);
        setRoomName("");
      });

      try {
        await newConnection.start();
        const joinCode = gameRoomCode.toString();
        newConnection.invoke("JoinGame", { id: userId, userName, joinCode });
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
      return "Anonymous";
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
            joinExistingRoom();
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
