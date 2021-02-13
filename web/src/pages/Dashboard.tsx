import React, { useState } from "react";
import { NavLinkProps, useHistory } from "react-router-dom";
import Navbar from "../components/Navbar";
import { FaArrowLeft, FaArrowRight } from 'react-icons/fa';

import "./Dashboard.scss"
import { userService } from "../state/user";
import { File, fileService, useFileState } from "../state/files";
import Button from "../components/Button";
import { format } from "date-fns";
import { API_URL } from "../state/Request";

const Dashboard = () => {
	const history = useHistory();
	const [navState, setNavState] = useState("files");
	const { currentPage, totalPages, files } = useFileState();

	const copyToClipboard = (file: File) => {
		navigator.clipboard.writeText(`${API_URL}/${file.name}`);
	}

	const content = () => {
		switch (navState) {
			case "files":
				return (
					<section className="dashboard-files">
						<section className="file-table">
  							<div className="table-header">
								  <span className="header-name">Original file name</span>
								  <span className="header-size">Size</span>
								  <span className="header-date">Date uploaded</span>
								  <span className="header-link">Link</span>
							  </div>
							<div className="table-content">
								{files.map((x) => {
									return (
										<div className="table-row" key={x.id}>
											<div className="row-name">
												{x.original_name}
											</div>
											<div className="row-size">
												<span>{x.file_size > 1000 ? `${(x.file_size / 1000).toFixed(2)} MB` : `${x.file_size} KB`}</span>
											</div>
											<div className="row-date">
												<span>{format(new Date(x.created_at), "dd MMM HH:mm aaa")}</span>
											</div>
											<Button className="row-link" small={true} transparent={true} text={"Copy"} onClick={() => copyToClipboard(x)}/>
										</div>
									)
								})}
							</div>
						</section>
						<div className="table-nav">
							<button className={currentPage <= 1 ? "disabled" : ""} disabled={currentPage <= 1 } onClick={() => fileService.getPreviousPage()}>
								<FaArrowLeft color={currentPage <= 1 ? "#5B5C5E" : "#DAE1E7"} size={18} />
							</button>
							Page {currentPage}/{totalPages}
							<button className={currentPage >= totalPages ? "disabled" : ""} disabled={currentPage >= totalPages} onClick={() => fileService.getNextPage()}>
								<FaArrowRight color={currentPage >= totalPages ? "#5B5C5E" : "#DAE1E7"} size={18} />
							</button>
						</div>
					</section>
				);
			case "account":
				return null;
		}
	};

	const onLogout = () => {
		userService.logout();
		history.push("home");
		console.info("Logged out");
	}

	return (
		<>
			<section className="route" id="dashboard">
				<nav className="dashboard-menu">
					<button className="menu-item" onClick={() => setNavState("files")}>Files</button>
					{/* <button className="menu-item" onClick={() => setNavState("account")}>Account</button> */}
					<button className="menu-item" onClick={onLogout}>Log Out</button>
				</nav>
				<section className="dashboard-content">
					{content()}
				</section>
			</section>
		</>
	);
};

export default Dashboard;