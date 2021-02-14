import React from "react";
import Button from "../components/Button";
import UploadButton from "../components/UploadButton";
import { fileService, useFileState } from "../state/files";
import { useAuthenticationState } from "../state/user";
import "./Home.scss";



const Home = () => {
	const { isLoggedIn } = useAuthenticationState();
	const { recentlyUploaded, uploadProgress } = useFileState();

	const copyToClipboard = (x: string) => {
		navigator.clipboard.writeText(x);
	}

	const onUpload = (file?: File) => {
		if (!file) {
			return;
		}
		fileService.uploadFile(file);
	};

	const upload = () => {
		if (!isLoggedIn) {
			return null;
		}

		return (
			<>
				<UploadButton onUpload={onUpload} percentage={uploadProgress} />
				{(recentlyUploaded?.length > 0) && <h4>Uploaded files (Last 3)</h4>}
				{[...recentlyUploaded].reverse().slice(0, 3).map((x) => {
					return (
						<div key={x} className="recent-file">
							<Button text={"Copy URL"} onClick={() => copyToClipboard(x)} small={true} />
							<span>{x.split(`${window.location.host}/`)[1]}</span>
						</div>
					);
				})}
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