import React from "react";

const Header = ({ roomName, joinCode, onclick }) => {
  return (
    <div className="header">
      <p>{roomName}</p>
      JoinCode: {joinCode}{" "}
      <button onClick={() => onclick()}>Lämna spelet</button>
    </div>
  );
};

export default Header;
