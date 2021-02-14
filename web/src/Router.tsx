import React from "react";
import {
	BrowserRouter,
	Switch,
	Route
} from "react-router-dom";
import Navbar from "./components/Navbar";
import Contact from "./pages/Contact";
import Dashboard from "./pages/Dashboard";
import Home from "./pages/Home";
import { useAuthenticationState } from "./state/user";

const Router = () => {
	const { isLoggedIn } = useAuthenticationState();

	return (
		<BrowserRouter>
			<Switch>
				<Route path="/contact">
					<main className="content">
						<Navbar activePage="contact"></Navbar>
						<Contact />
					</main>
				</Route>
				{isLoggedIn ? (
					<Route path="/dashboard">
						<main className="content">
							<Navbar activePage="dashboard"></Navbar>
							<Dashboard />
						</main>
					</Route>
				) : null}
				<Route path="/">
					<main className="content">
						<Navbar activePage="home"></Navbar>
						<Home />
					</main>
				</Route>
			</Switch>
		</BrowserRouter>
	);
};

export default Router;