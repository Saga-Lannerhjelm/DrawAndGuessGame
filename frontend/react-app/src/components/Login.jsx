import React, { useState } from "react";
import { Link } from "react-router-dom";

const Login = () => {
  const [userName, setUserName] = useState("");
  const [password, setPassword] = useState("");
  return (
    <>
      <h2 style={{ marginBottom: "0" }}>Välkommen till DuoSkiss</h2>
      <h4 style={{ marginTop: "0" }}>
        - Logga in eller registrera dig för att fortsätta -
      </h4>
      <div>
        <form className="standard-form">
          <input
            type="text"
            placeholder="Användarnamn"
            onChange={(e) => setUserName(e.target.value)}
            value={userName}
          />
          <input
            type="text"
            placeholder="Lösenord"
            onChange={(e) => setPassword(e.target.value)}
            value={password}
          />
          {/* <button type="submit"> */}
          <Link to={"/home"} className="btn">
            Logga in
          </Link>
          {/* </button> */}
          <small>
            Har du inget konto? <span>Registrera dig!</span>
          </small>
        </form>
      </div>
    </>
  );
};

export default Login;
