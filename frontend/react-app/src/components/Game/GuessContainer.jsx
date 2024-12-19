import React, { useEffect, useState } from "react";
import GuessForm from "./GuessForm";
import { useConnection } from "../../context/ConnectionContext";
import { useParams } from "react-router-dom";
import Message from "./Message";

const GuessContainer = ({ userIsDrawing, userGuess }) => {
  const { connection } = useConnection();
  const [users, setUsers] = useState([]);
  const [activeUser, setActiveUser] = useState("");

  useEffect(() => {
    if (connection) {
      connection.on("UsersInGame", (userValues, activeUserValues) => {
        setUsers(userValues);
        if (activeUserValues !== "") {
          setActiveUser(activeUserValues);
        }
      });
    }
  }, [connection]);

  useEffect(() => {
    if (users.length > 0) {
      userIsDrawing(
        users.find((user) => user.username == activeUser).isDrawing
      );
    }
  }, [users]);

  return (
    <div className="user-container">
      Spelare ({users.length})
      {users.map((user, index) => (
        <div
          className={user.isDrawing ? "user drawing" : "user"}
          style={user.hasGuessedCorrectly ? { borderColor: "#00FF2F" } : {}}
          key={index}
        >
          {userGuess.user === user.username && (
            <Message message={userGuess.guess} />
          )}
          <div>
            <p>
              {user.username} {user.username == activeUser ? "(Du)" : ""}
            </p>
            <p>{user.points} poäng</p>
          </div>
          {user.isDrawing && <span>ritar</span>}
        </div>
      ))}
    </div>
  );
};

export default GuessContainer;
