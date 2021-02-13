import React from "react";
import "./Input.scss";

interface InputProps {
	id: string;
	placeholder?: string;
	label?: string;
	type?: "text" | "password" | "number" | "email",
	onChange?: (val: string) => void,
}

const Input = (props: InputProps) => {
	
	return (
		<div style={{width: "100%"}}>
			{props.label && <label className="input-label" htmlFor={props.id}>{props.label}</label>}
			<input 
				className="text-input"
				id={props.id}
				type={props.type}
				placeholder={props.placeholder}
				onChange={(x) => props.onChange ? props.onChange(x.target.value) : null} 
			/>
		</div>
	);
};

export default Input;