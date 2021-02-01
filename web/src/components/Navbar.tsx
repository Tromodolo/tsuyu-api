import React, { useRef, useState } from "react";
import { Link } from "react-router-dom";
import Login from "./Login";
import "./Navbar.scss";

interface NavbarProps {
	activePage?: string;
}

const Navbar = (props: NavbarProps) => {
	const isLoggedIn = true;
	const loginButton = useRef<HTMLButtonElement | null>();
	const [showLoginPopup, setShowLoginPopup] = useState(false);

	return (
		<nav className="nav-bar">
			<section className="nav-start">
				<h3 className="name">tsuyu</h3>
			</section>
			<section className="nav-end">
				<Link to="" className={props.activePage  === "home" ? "nav-item active" : "nav-item"}>Home</Link>
				<Link to="contact"  className={props.activePage  === "contact" ? "nav-item active" : "nav-item"}>Contact</Link>
				{/* {isLoggedIn ? ( */}
					<button className="nav-item" onClick={() => setShowLoginPopup(!showLoginPopup)} ref={(x) => loginButton.current = x}>Log in</button>
				{/* ) : ( */}
					<Link to="dashboard"  className={props.activePage  === "dashboard" ? "nav-item active" : "nav-item"}>Dashboard</Link>
				{/* )} */}
			</section>
			{showLoginPopup ? (
				<Login anchorElement={loginButton.current} />
			): null}
		</nav>
	);
};

export default Navbar;