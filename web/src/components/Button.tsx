import React from "react";
import "./Button.scss";

interface ButtonProps {
	text: string;
	onClick?: () => void;
}

const Button = (props: ButtonProps) => {
	return (
		<button className="button" onClick={() => props.onClick ? props.onClick() : null}>{props.text}</button>
	);
};

export default Button;