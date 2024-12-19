import React from "react";

const Header = ({ gameRoom, onclick }) => {
  return (
    <div className="header">
      JoinCode: {gameRoom}{" "}
      <button onClick={() => onclick()}>Lämna spelet</button>
    </div>
  );
};

export default Header;
