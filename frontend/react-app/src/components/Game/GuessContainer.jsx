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
      connection.on("UsersInGame", (userValues) => {
        setUsers(userValues);
        console.log("Users:", userValues);
        console.log("AcriveUser:", activeUser);

        if (users.length > 0 && users[0].username == undefined) {
          userIsDrawing(
            users.find((user) => user.user.username == activeUser).userInRound
              .isDrawing
          );
        }
      });
    }
  }, [connection]);

  // useEffect(() => {
  //   if (users.length > 0 && users[0].userInRound.isDrawing !== undefined) {
  //     userIsDrawing(
  //       users.find((user) => user.user.username == activeUser).userInRound
  //         .isDrawing
  //     );
  //   }
  // }, [users]);

  return (
    <div className="user-container">
      Spelare ({users.length})
      {users.map((user, index) => (
        <div className="message-and-user" key={index}>
          {userGuesses.find((g) => g.user === user.User.username) ? (
            <Message
              message={
                userGuesses.find((g) => g.user === user.User.username).guess
              }
              correct={user.UserInRound.hasGuessedCorrectly}
            />
          ) : (
            <div></div>
          )}
          {user.username == undefined ? (
            <div
              className={user.userInRound.isDrawing ? "user drawing" : "user"}
              style={
                user.userInRound.hasGuessedCorrectly
                  ? { borderColor: "#00FF2F" }
                  : {}
              }
            >
              <div>
                <p>
                  {user.user.username}{" "}
                  {user.user.username == activeUser ? "(Du)" : ""}
                </p>
                <p>{user.userInRound.points} poäng</p>
              </div>
              {user.userInRound.isDrawing && <span>ritar</span>}
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
