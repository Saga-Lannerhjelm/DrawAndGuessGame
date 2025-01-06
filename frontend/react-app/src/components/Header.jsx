import React from "react";

const Header = ({ roomName, joinCode, onclick, endGame }) => {
  return (
    <div className="header">
      <h4>{roomName}</h4>
      <div>
        Anslutningskod: {joinCode}{" "}
        <button onClick={() => onclick()}>Lämna spelet</button>
        <button onClick={() => endGame()}>Avsluta spelet</button>
      </div>
    </div>
  );
};

export default Header;
