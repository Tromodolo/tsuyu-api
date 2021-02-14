import React from "react";
import "./UploadButton.scss";

interface UploadButtonProps {
	percentage?: number;
	onUpload?: (file?: File) => void;
}

const UploadButton = (props: UploadButtonProps) => {
	return (
		<div className="upload-button-wrapper">
			<input type="file" id="upload-button" name="files" style={{display: "none"}} onChange={(x) => props.onUpload?.(x.target.files?.[0])} multiple={false} disabled={props.percentage !== 0}/>
			<label htmlFor="upload-button" id="upload-label" className="upload-label" {...{"progress": "50px"}}>
				Click or drag file to upload
			</label>
			<div className="upload-progress" style={{width: `${props.percentage ?? 0}%`}}></div>
		</div>
	);
}

export default UploadButton;