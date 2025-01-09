import React, { useEffect, useState } from "react";
import { useConnection } from "../../context/ConnectionContext";
import { jwtDecode } from "jwt-decode";
import { LogLevel } from "@microsoft/signalr";

const Highscore = () => {
  const [usersInList, setUsersInList] = useState([]);
  const [userId, setUserId] = useState(undefined);

  const { jwt } = useConnection();

  useEffect(() => {
    getUsers();
    if (jwt) {
      const decoded = jwtDecode(jwt);
      setUserId(parseInt(decoded.id));
      console.log(decoded.id);
    }
  }, []);

  const getUsers = async () => {
    try {
      const response = await fetch("http://localhost:5034/User");

      if (!response.ok) throw new Error(`Response status: ${response.status}`);
      const result = await response.json();

      const filteredResults = result.filter((u) => u.totalPoints > 0);

      setUsersInList(filteredResults);
    } catch (error) {
      console.error(error);
      return null;
    }
  };

  return (
    <div>
      <h2>Topplistan</h2>
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
              style={{
                backgroundColor:
                  index % 2 === 0 ? "rgba(255, 255, 255, 0.1)" : "",
                border: userId === user.id ? "3px solid white" : "",
              }}
            >
              <p>
                {index + 1}. {user.username} {userId === user.id ? "(DU)" : ""}
              </p>
              <p>{user.wins}</p>
              <p>{user.totalPoints}</p>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default Highscore;
