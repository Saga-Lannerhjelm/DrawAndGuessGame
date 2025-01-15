import React, { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import Cookies from "js-cookie";
import { useConnection } from "../context/ConnectionContext";
import RedoIcon from "../assets/RedoIcon";

const Register = () => {
  const [username, setUserName] = useState("");
  const [password, setPassword] = useState("");
  const [submitError, setSubmitError] = useState("");

  const navigate = useNavigate();

  const register = async (username, password) => {
    try {
      const response = await fetch("http://localhost:5034/Account/register", {
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
        navigate("/login");
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

  const getRandomUsername = async () => {
    try {
      // Link to API https://github.com/randomusernameapi/randomusernameapi.github.io?tab=readme-ov-file
      const response = await fetch(
        "https://usernameapiv1.vercel.app/api/random-usernames"
      );

      if (!response.ok) throw new Error(`Response status: ${response.status}`);
      const result = await response.json();
      return result.usernames[0];
    } catch (error) {
      setSubmitError(error);
      return "Anonymous";
    }
  };

  const useRandomUsername = async () => {
    let userN = await getRandomUsername();
    userN = userN.substring(0, userN.length - 1);
    setUserName(userN);
  };

  return (
    <>
      <h4 style={{ marginBottom: "0" }}>Registrera dig</h4>
      <div>
        <form
          className="standard-form"
          onSubmit={(e) => {
            e.preventDefault();
            register(username, password);
          }}
        >
          <div className="register-user-input">
            <input
              type="text"
              placeholder="Användarnamn"
              onChange={(e) => {
                setUserName(e.target.value);
                setSubmitError("");
              }}
              value={username}
            />
            <div onClick={() => useRandomUsername()}>
              <RedoIcon />
            </div>
          </div>
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
        </form>
      </div>
    </>
  );
};

export default Register;
