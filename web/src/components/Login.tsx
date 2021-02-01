import React from "react";
import Button from "./Button";
import Input from "./Input";
import "./Login.scss";

interface LoginProps {
	anchorElement?: HTMLButtonElement | null;
}

const Login = (props: LoginProps) => {
	let right = 0;
	let top = 0;

	if (props.anchorElement){
		const { innerWidth: width, innerHeight: height } = window;
		const bounding = props.anchorElement.getBoundingClientRect();
		right = width - bounding.right;
		top = bounding.bottom + 20;
	}

	return (
		<section className="login-popup" style={{right, top}}>
			<div className="popup-arrow"></div>
			<Input id={"username"} placeholder={"Username"} />
			<Input id={"password"} placeholder={"Password"} type={"password"} />
			<Button text={"Log In"} onClick={() => console.log(123)} />
		</section>
	);
};

export default Login;