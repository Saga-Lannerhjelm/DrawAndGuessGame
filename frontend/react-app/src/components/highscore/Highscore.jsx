import React, { useEffect, useState } from "react";
import { useConnection } from "../../context/ConnectionContext";

const Highscore = () => {
  const [usersInList, setUsersInList] = useState([]);

  useEffect(() => {
    getUsers();
  }, []);

  const getUsers = async () => {
    try {
      const response = await fetch("http://localhost:5034/User");

      if (!response.ok) throw new Error(`Response status: ${response.status}`);
      const result = await response.json();

      setUsersInList(result);
    } catch (error) {
      console.error(error);
      return null;
    }
  };

  return (
    <div>
      {usersInList.length > 0 && (
        <div className="highscore-grid">
          <div>
            <h4>Användarnamn</h4>
            <h4>Vinster</h4>
            <h4>Totala poäng</h4>
          </div>
          {usersInList.map((user, index) => (
            <div
              key={index}
              style={
                index % 2 === 0
                  ? { backgroundColor: "rgba(255, 255, 255, 0.05)" }
                  : {}
              }
            >
              <p>{user.username}</p>
              <p>{user.wins}</p>
              <p>{user.totalPoints}</p>
            </div>
          ))}
        </div>
      )}
      <h2>Topplistan</h2>
    </div>
  );
};

export default Highscore;