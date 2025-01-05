import React from "react";
import { Link } from "react-router-dom";

const NavBar = () => {
  return (
    <>
      <Link to={"/home"}>Hem</Link>
      <Link to={"/highscore"}>Topplista</Link>
    </>
  );
};

export default NavBar;
