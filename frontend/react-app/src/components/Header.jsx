import React from "react";

const Header = ({ roomName, joinCode, onclick, endGame }) => {
  return (
    <div className="header">
      <p>{roomName}</p>
      JoinCode: {joinCode}{" "}
      <button onClick={() => onclick()}>Lämna spelet</button>
      <button onClick={() => endGame()}>Avsluta spelet</button>
    </div>
  );
};

export default Header;
