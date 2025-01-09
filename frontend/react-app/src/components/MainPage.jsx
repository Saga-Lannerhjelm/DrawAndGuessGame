import React, { useEffect, useState } from "react";
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { useNavigate } from "react-router-dom";
import { useConnection } from "../context/ConnectionContext";
import GameMessage from "./GameMessage";
import Cookies from "js-cookie";
import { jwtDecode } from "jwt-decode";

const Home = () => {
  const [userName, setUserName] = useState("");
  const [roomName, setRoomName] = useState("");
  // const [randomUsername, setRandomUsername] = useState("");
  const [inviteCode, setInviteCode] = useState("");
  const [loading, setLoading] = useState(true);
  const [gameMessage, setGameMessage] = useState("");

  const { setConnection, connection, setActiveUserId, setUsers, jwt } =
    useConnection();

  const navigate = useNavigate();

  useEffect(() => {
    setTimeout(() => {
      setGameMessage("");
    }, 3000);
  }, [gameMessage]);

  useEffect(() => {
    if (jwt) {
      const decoded = jwtDecode(jwt);
      setUserName(decoded.name);
    } else {
      if (connection) {
        connection.stop();
        setConnection();
      }
      navigate("/login");
    }
  }, [jwt]);

  const createRoom = async (roomName) => {
    const gameRoomCode = Math.round(Math.random() * 100000000);
    var { userId, jwtValue, username } = getValuesFromToken();

    if (jwtValue) {
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
            Authorization: `Bearer  ${jwt}`,
          },
          body: JSON.stringify(game),
          credentials: "include",
        });

        if (response.ok) {
          joinRoom(gameRoomCode, userId, jwt, username);
        } else if (response.status === 401) {
          navigate("/login");
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
    try {
      var { gameExists, error } = await checkIfGameExists(jwt);

      if (gameExists) {
        var { userId, jwtValue, username } = getValuesFromToken();

        if (jwtValue) {
          joinRoom(inviteCode, userId, jwt, username);
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

  const checkIfGameExists = async (jwt) => {
    const response = await fetch("http://localhost:5034/Game/room", {
      method: "POST",
      headers: {
        Accept: "application/json",
        "Content-Type": "application/json",
        Authorization: `Bearer  ${jwt}`,
      },
      body: JSON.stringify(inviteCode),
    });

    if (response.ok) {
      const existingUser = await response.json();
      return { gameExists: existingUser, error: null };
    } else if (response.status === 401) {
      navigate("/login");
    } else {
      console.error(
        "Fel vid API-anrop:",
        response.status,
        await response.text()
      );
      return { gameExists: false, error: "Ett fel inträffade" };
    }
  };

  function getValuesFromToken() {
    // const jwtValue = Cookies.get("jwt-cookie");
    const jwtValue = jwt;
    let userId;
    let username;
    if (jwt) {
      const decoded = jwtDecode(jwt);
      userId = parseInt(decoded.id);
      username = decoded.name;
    }
    return { userId, jwtValue, username };
  }

  const joinRoom = async (gameRoomCode, userId, jwt, username) => {
    console.log("USerID in join:", userId);
    console.log("username in join:", username);
    // if (!loading) {
    startConnection(gameRoomCode, userId, jwt, username);
    setActiveUserId(userId);
    // }
  };

  async function startConnection(gameRoomCode, userId, jwt, user) {
    if (!connection) {
      console.log("in start conn");
      const newConnection = new HubConnectionBuilder()
        .withUrl("http://localhost:5034/draw", {
          accessTokenFactory: () => jwt,
        })
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
        console.log(userId, user, joinCode);
        newConnection.invoke("JoinGame", {
          id: userId,
          username: user,
          joinCode,
        });
      } catch (error) {
        if (error.message.includes("401")) {
          navigate("/login");
        }
      }
    }
  }

  // async function getRandomUsername() {
  //   try {
  //     // Link to API https://github.com/randomusernameapi/randomusernameapi.github.io?tab=readme-ov-file
  //     const response = await fetch(
  //       "https://usernameapiv1.vercel.app/api/random-usernames"
  //     );

  //     if (!response.ok) throw new Error(`Response status: ${response.status}`);
  //     const result = await response.json();
  //     return result.usernames[0];
  //   } catch (error) {
  //     console.error(error);
  //     return "Anonymous";
  //   } finally {
  //     setLoading(false);
  //   }
  // }

  return (
    <>
      {gameMessage != "" && <GameMessage msg={gameMessage} />}
      <h2>Välkommen {userName}</h2>
      <h4>- Skapa ett spel -</h4>
      <div>
        <form
          className="standard-form"
          onSubmit={(e) => {
            e.preventDefault();
            createRoom(roomName);
          }}
        >
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
