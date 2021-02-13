import React from "react";
import "./Button.scss";
import BeatLoader from "react-spinners/BeatLoader";
interface ButtonProps {
	className?: string;
	text: string;
	onClick?: () => void;
	isLoading?: boolean;
	small?: boolean;
	transparent?: boolean;
	type?: "button" | "submit" | "reset";
}

const Button = (props: ButtonProps) => {
	let className = "button";

	if (props.small) {
		className += " button-small";
	}
	if (props.transparent) {
		className += " button-transparent";
	}

	return (
		<button className={`${className} ${props.className}`} type={props.type} onClick={() => props.onClick?.()} disabled={props.isLoading}>
			{props.isLoading ? <BeatLoader color="#000000" size={8} /> : props.text}
		</button>
	);
};

export default Button;