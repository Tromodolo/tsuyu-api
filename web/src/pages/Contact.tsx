import React from "react";
import "./Contact.scss";

const Contact = () => {
	return (
		<>
			<section className="route" id="contact">
				<section className="contact">
					<h1 className="contact-title">Contact</h1>
					<p className="contact-info">
						For legal notices or takedown requests, please go contact me via <a href="mailto:filip.ekstrom98@gmail.com" className="link">email.</a>
					</p>
					<p className="contact-info">
						For other questions and answers, be sure to contact me either via <a href="mailto:filip.ekstrom98@gmail.com" className="link">email</a> or by joining my <a href="https://discord.gg/PhswpNNYTM" className="link">Discord Server.</a>
					</p>
				</section>
			</section>
		</>
	);
};

export default Contact;