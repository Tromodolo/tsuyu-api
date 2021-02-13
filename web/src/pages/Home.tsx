import React from "react";
import Navbar from "../components/Navbar";
import "./Home.scss";



const Home = () => {
	const isLoggedIn = true;

	const upload = () => {
		if (!isLoggedIn) {
			return null;
		}

		return (
			<>
				{/* <input type="file" id="upload-button" name="files" style={{display: "none"}} />
				<label htmlFor="upload-button" id="upload-label">
					Click or drag file to upload
				</label> */}
			</>
		)
	};

	return (
		<>
			<section className="route" id="home">
				<section className="info">
					<h1 className="title">tsuyu</h1>
					<h3 className="subtitle">ツユ</h3>
					<p className="description">
						File uploading service lovingly crafted using Rust.
						{/* <br />						
						Fast uploads, sharing and file history, but more to come. */}
					</p>
				</section>
				<section className="interactions">
					{upload()}
				</section>
				<a className="credits" href="https://www.pixiv.net/en/artworks/71865345">Art source: Hang</a>
			</section>
		</>
	)
};

export default Home;