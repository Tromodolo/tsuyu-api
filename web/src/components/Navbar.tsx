import React, { useEffect, useRef, useState } from "react";
import { Link } from "react-router-dom";
import { useSettings } from "../state/settings";
import { useAuthenticationState } from "../state/user";
import Login from "./Login";
import "./Navbar.scss";

interface NavbarProps {
	activePage?: string;
}

const Navbar = (props: NavbarProps) => {
	const { isLoggedIn, error, isLoading } = useAuthenticationState();
	const { register_enabled } = useSettings();
	const loginButton = useRef<HTMLButtonElement | null>();
	const [showLoginPopup, setShowLoginPopup] = useState(false);

	useEffect(() => {
		setShowLoginPopup(false);
	}, [isLoggedIn]);

	return (
		<nav className="nav-bar">
			<section className="nav-start">
				<h3 className="name">tsuyu</h3>
			</section>
			<section className="nav-end">
				<Link to="" className={props.activePage  === "home" ? "nav-item active" : "nav-item"}>Home</Link>
				<Link to="contact"  className={props.activePage  === "contact" ? "nav-item active" : "nav-item"}>Contact</Link>
				{isLoggedIn ? ( 
					<Link to="dashboard"  className={props.activePage  === "dashboard" ? "nav-item active" : "nav-item"}>Dashboard</Link>
				) : (
					<button className="nav-item" onClick={() => setShowLoginPopup(!showLoginPopup)} ref={(x) => loginButton.current = x}>Log in</button>
				)}
			</section>
			{showLoginPopup ? (
				<Login anchorElement={loginButton.current} showRegister={register_enabled} error={error} isLoading={isLoading}/>
			): null}
		</nav>
	);
};

export default Navbar;