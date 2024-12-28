import React, { useEffect, useState } from "react";
import GuessForm from "./GuessForm";
import { useConnection } from "../../context/ConnectionContext";
import { useParams } from "react-router-dom";
import Message from "./Message";

const GuessContainer = ({ userIsDrawing, userGuesses }) => {
  const { connection, activeUser } = useConnection();
  const [users, setUsers] = useState([]);
  // const [activeUser, setActiveUser] = useState("");

  useEffect(() => {
    if (connection) {
      // connection.on("UsersInGame", (userValues, activeUserValues) => {
      //   setUsers(userValues);
      //   if (activeUserValues !== "") {
      //     setActiveUser(activeUserValues);
      //   }
      // });
      connection.on("UsersInGame", (userValues) => {
        setUsers(userValues);
        // if (activeUserValues !== "") {
        //   setActiveUser(activeUserValues);
        // }
      });
    }
  }, [connection]);

  useEffect(() => {
    if (users.length > 0 && users[0].isDrawing !== undefined) {
      userIsDrawing(
        users.find((user) => user.userDetails.username == activeUser).isDrawing
      );
    }
  }, [users]);

  return (
    <div className="user-container">
      Spelare ({users.length})
      {users.map((user, index) => (
        <div className="message-and-user" key={index}>
          {userGuesses.find((g) => g.user === user.userDetails.username) ? (
            <Message
              message={
                userGuesses.find((g) => g.user === user.userDetails.username)
                  .guess
              }
              correct={user.hasGuessedCorrectly}
            />
          ) : (
            <div></div>
          )}
          {user.username == undefined ? (
            <div
              className={user.isDrawing ? "user drawing" : "user"}
              style={user.hasGuessedCorrectly ? { borderColor: "#00FF2F" } : {}}
            >
              <div>
                <p>
                  {user.userDetails.username}{" "}
                  {user.userDetails.username == activeUser ? "(Du)" : ""}
                </p>
                <p>{user.points} poäng</p>
              </div>
              {user.isDrawing && <span>ritar</span>}
            </div>
          ) : (
            <div
              className={user.isDrawing ? "user drawing" : "user"}
              style={user.hasGuessedCorrectly ? { borderColor: "#00FF2F" } : {}}
            >
              <div>
                <p>
                  {user.username} {user.username == activeUser ? "(Du)" : ""}
                </p>
                <p></p>
              </div>
            </div>
          )}
        </div>
      ))}
    </div>
  );
};

export default GuessContainer;
