import React, { useEffect, useState } from "react";
import GuessForm from "./GuessForm";
import { useConnection } from "../../context/ConnectionContext";
import { useParams } from "react-router-dom";

const GuessContainer = ({ gameRoom }) => {
  const { connection } = useConnection();
  const [users, setUsers] = useState([]);
  const [activeUser, setActiveUser] = useState("");

  useEffect(() => {
    if (connection) {
      connection.on("UsersInGame", (users, activeUser) => {
        setUsers(users);
        if (activeUser !== "") {
          setActiveUser(activeUser);
        }
      });
    }
  }, [connection]);

  return (
    <div className="user-container">
      Spelare ({users.length})
      {users.map((user, index) => (
        <div
          className={user.isDrawing ? "user drawing" : "user"}
          style={user.hasGuessedCorrectly ? { borderColor: "#00FF2F" } : {}}
          key={index}
        >
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
