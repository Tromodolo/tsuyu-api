import React, { FormEvent, useState } from "react";
import { useAuthenticationState, userService } from "../state/user";
import Button from "./Button";
import Input from "./Input";
import "./Login.scss";

interface LoginProps {
	anchorElement?: HTMLButtonElement | null;
	showRegister?: boolean;
	isLoading?: boolean;
	error?: string;
}

const Login = (props: LoginProps) => {
	const [username, setUsername] = useState("");
	const [password, setPassword] = useState("");
	const [email, setEmail] = useState("");
	const [type, setType] = useState<"login" | "register"> ("login");

	let right = 0;
	let top = 0;

	if (props.anchorElement){
		const { innerWidth } = window;
		const bounding = props.anchorElement.getBoundingClientRect();
		right = innerWidth - bounding.right + 20;
		top = bounding.bottom + 20;
	}

	const onLogin = (event?: FormEvent) => {
		if (props.isLoading) {
			return;
		}
		event?.preventDefault();
		if (type === "login") {
			userService.login({ 
				username, 
				password
			});
		} else {
			userService.register({ 
				username, 
				password,
				email,
			});
		}
	}

	return (
		<form className="login-popup" style={{right, top}} onSubmit={onLogin}>
			<div className="popup-arrow"></div>
			<label className="login-error">{props.error}</label>
			<Input id={"username"} placeholder={"Username"} onChange={(val) => setUsername(val)} />
			<Input id={"password"} placeholder={"Password"} type={"password"} onChange={(val) => setPassword(val)} />
			{type === "register" 
				? <Input id={"email"} placeholder={"Email"} type={"email"} onChange={(val) => setEmail(val)} /> 
				: null
			}
			<div className="login-buttons">
				{props.showRegister && type === "login" && <Button type={"button"} onClick={() => setType("register")} small={true} transparent={true} text={"Create account"}/>}
				{props.showRegister && type === "register" && <Button type={"button"} onClick={() => setType("login")} small={true} transparent={true} text={"Already have an account?"}/>}
				<Button text={type === "login" ? "Log In" : "Register"} isLoading={props.isLoading} />
			</div>
		</form>
	);
};

export default Login;