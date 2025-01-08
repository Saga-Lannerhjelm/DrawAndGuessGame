import React, { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import Cookies from "js-cookie";

const Login = () => {
  const [username, setUserName] = useState("");
  const [password, setPassword] = useState("");
  const [submitError, setSubmitError] = useState("");
  const navigate = useNavigate();

  const login = async (username, password) => {
    try {
      const response = await fetch("http://localhost:5034/api/Account/login", {
        method: "POST",
        headers: {
          Accept: "application/json",
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          username: username,
          password: password,
        }),
      });

      if (response.ok) {
        const data = await response.json();
        Cookies.set("jwt-cookie", data);
        console.log(data);
        navigate("/home");
      }

      if (!response.ok) {
        const errorMessage = await response.text();
        const cleanedMessage = errorMessage.substring(
          1,
          errorMessage.length - 1
        );
        setSubmitError(cleanedMessage);
      }
    } catch (error) {
      setSubmitError(error);
    }
  };
  return (
    <>
      <h2 style={{ marginBottom: "0" }}>Välkommen till DuoSkiss</h2>
      <h4 style={{ marginTop: "0" }}>
        - Logga in eller registrera dig för att fortsätta -
      </h4>
      <div>
        <form
          className="standard-form"
          onSubmit={(e) => {
            e.preventDefault();
            login(username, password);
          }}
        >
          <input
            type="text"
            placeholder="Användarnamn"
            onChange={(e) => {
              setUserName(e.target.value);
              setSubmitError("");
            }}
            value={username}
          />
          <input
            type="text"
            placeholder="Lösenord"
            onChange={(e) => {
              setPassword(e.target.value);
              setSubmitError("");
            }}
            value={password}
          />

          <small style={{ color: "#ff0070", padding: 0, margin: 0 }}>
            {submitError}
          </small>

          <button
            type="submit"
            disabled={password === "" || username == ""}
            className="btn"
          >
            Logga in
          </button>
          <small>
            Har du inget konto? <span>Registrera dig!</span>
          </small>
        </form>
      </div>
    </>
  );
};

export default Login;
