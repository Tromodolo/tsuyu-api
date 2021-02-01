import React from "react";
import {
	BrowserRouter,
	Switch,
	Route
} from "react-router-dom";
import Contact from "./pages/Contact";
import Dashboard from "./pages/Dashboard";
import Home from "./pages/Home";

const Router = () => {
	const isAuthenticated = true;

	return (
		<BrowserRouter>
			<Switch>
				<Route path="/contact">
					<Contact />
				</Route>
				{isAuthenticated ? (
					<Route path="/dashboard">
						<Dashboard />
					</Route>
				) : null}
				<Route path="/">
					<Home />
				</Route>
			</Switch>
		</BrowserRouter>
	);
};

export default Router;