import { FileStore, fileStore } from "./file.store";
import { File as IFile, FileCount, FileState } from "./file.model";
import { API_URL, SendRequest } from "../Request";
import axios, { AxiosError, AxiosResponse } from "axios";
import { UserStore } from "../user";
import { userStore } from "../user/user.store";

export class FileService {
	constructor(private store: FileStore, private userStore: UserStore) {}

	async getFileCount() {
		this.store.setLoading(true);
		this.store.setError("");

		try {
			const res = await SendRequest<FileCount>("file-count", "GET");
			if (res.status === 200){
				this.store.update({
					totalPages: Math.ceil((res.json?.num_count ?? 0.1) / 12),
				});
			} else {
				this.store.setError(res.error);
			}
		} catch (e) {
			this.store.setError(e.message);
		}

		this.store.setLoading(false);
	}

	async getFiles() {
		this.store.setLoading(true);
		this.store.setError("");

		const state: FileState = this.store.getValue();

		try {
			const res = await SendRequest<IFile[]>("files", "GET", undefined, [state.currentPage.toString()]);
			if (res.status === 200){
				this.store.update({
					files: res.json,
				});
			} else {
				this.store.setError(res.error);
			}
		} catch (e) {
			this.store.setError(e.message);
		}

		this.store.setLoading(false);
	}

	async getNextPage() {
		const state: FileState = this.store.getValue();
		if (state.currentPage >= state.totalPages){
			return;
		}
		this.store.update({
			currentPage: state.currentPage + 1,
		})
		await this.getFiles();
	}

	async getPreviousPage() {
		const state: FileState = this.store.getValue();
		if (state.currentPage <= 1){
			return;
		}
		this.store.update({
			currentPage: state.currentPage - 1,
		})
		await this.getFiles();
	}

	async uploadFile(file: File) {
		this.store.setLoading(true);
		this.store.setError("");
		const userState = this.userStore.getValue();
		const fileState = this.store.getValue();

		try {
			let res: AxiosResponse;
			try {
				let formData = new FormData();
				formData.append("file", file);

				res = await axios.post(
					`${API_URL}/upload`,
					formData,
					{ 
						headers: {
							"Content-Type": "multipart/form-data",
							"Authorization": userState.api_key,
						},
						onUploadProgress: (prog) => {
							const percentage = Math.round(prog.loaded / prog.total * 100);
							this.store.update({
								uploadProgress: percentage,
							});
						}
					}
				);
			} catch (err) {
				const axErr: AxiosError = err;
				if (axErr.response){
					res = axErr.response;
				} else {
					throw new Error("Request failed");
				}
			}
			if (res.status === 200){
				this.store.update({
					uploadProgress: 0,
					recentlyUploaded: [
						...(fileState.recentlyUploaded ?? []),
						res.data,
					]
				});
			} else {
				this.store.setError(res.data);
			}
		} catch (e) {
			this.store.setError(e.message);
		}

		this.store.setLoading(false);
	}
}

export const fileService = new FileService(fileStore, userStore);
