import React from "react";
import GuessForm from "./GuessForm";

const GuessContainer = () => {
  const users = [
    {
      username: "UnmightSplash123",
      isActiveUser: false,
      isDrawing: true,
      points: 0,
      hasGuessedCorrectly: false,
    },
    {
      username: "Saga",
      isActiveUser: true,
      isDrawing: false,
      points: 0,
      hasGuessedCorrectly: false,
    },
    {
      username: "Anonymous",
      isActiveUser: false,
      isDrawing: true,
      points: 0,
      hasGuessedCorrectly: false,
    },
    {
      username: "Fladder",
      isActiveUser: false,
      isDrawing: false,
      points: 0,
      hasGuessedCorrectly: true,
    },
  ];
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
              {user.username} {user.isActiveUser ? "(Du)" : ""}
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
