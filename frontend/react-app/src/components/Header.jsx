import React from "react";

const Header = ({
  roomName,
  joinCode,
  onclick,
  endGame,
  roomOwner,
  activeUserId,
}) => {
  return (
    <div className="header">
      <h4>{roomName}</h4>
      <div>
        Anslutningskod: {joinCode}{" "}
        {roomOwner && roomOwner == activeUserId ? (
          <button onClick={() => endGame()}>Avsluta spelet</button>
        ) : (
          <button onClick={() => onclick()}>Lämna spelet</button>
        )}
      </div>
    </div>
  );
};

export default Header;
