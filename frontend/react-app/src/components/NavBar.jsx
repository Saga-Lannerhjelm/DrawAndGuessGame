import React from "react";
import { NavLink } from "react-router-dom";

const NavBar = () => {
  return (
    <nav className="navbar">
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
