import React, { useEffect, useState } from "react";
import GuessForm from "./GuessForm";
import { useConnection } from "../../context/ConnectionContext";
import { useParams } from "react-router-dom";
import Message from "./Message";

const GuessContainer = ({ userIsDrawing, userGuesses }) => {
  const { users, activeUser } = useConnection();

  useEffect(() => {
    if (users.length > 0 && users[0].username == undefined) {
      userIsDrawing(
        users.find((user) => user.info.username == activeUser).round.isDrawing
      );
    }
  }, [users]);

  return (
    <div className="user-container">
      Spelare ({users.length})
      {users.map((user, index) => (
        <div className="message-and-user" key={index}>
          {userGuesses.find((g) => g.userId === user.info.id) ? (
            <Message
              message={userGuesses.find((g) => g.userId === user.info.id).guess}
              correct={user.round.guessedCorrectly}
            />
          ) : (
            <div></div>
          )}
          {user.username == undefined ? (
            <div
              className={user.round.isDrawing ? "user drawing" : "user"}
              style={
                user.round.guessedCorrectly ? { borderColor: "#00FF2F" } : {}
              }
            >
              <div>
                <p>
                  {user.info.username}{" "}
                  {user.info.username == activeUser ? "(Du)" : ""}
                </p>
                <p>{user.round.points} poäng</p>
              </div>
              {user.round.isDrawing && <span>ritar</span>}
            </div>
          ) : (
            <div
              className={user.isDrawing ? "user drawing" : "user"}
              style={user.guessedCorrectly ? { borderColor: "#00FF2F" } : {}}
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
