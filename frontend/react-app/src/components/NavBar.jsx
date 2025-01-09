import React from "react";
import { NavLink } from "react-router-dom";
import { useConnection } from "../context/ConnectionContext";

const NavBar = () => {
  const { jwt, connection } = useConnection();
  return (
    <nav
      className="navbar"
      style={{
        display: !jwt || connection ? "none" : "flex",
      }}
    >
      <NavLink
        to={"/"}
        className={({ isActive }) => (isActive ? "active" : "")}
      >
        Hem
      </NavLink>
      <NavLink
        to={"/highscore"}
        className={({ isActive }) => (isActive ? "active" : "")}
      >
        Topplista
      </NavLink>
    </nav>
  );
};

export default NavBar;
