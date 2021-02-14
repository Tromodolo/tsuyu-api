import { useEffect, useState } from "react";
import { File } from "./file.model";
import { fileQuery } from "./file.query";
import { fileService } from "./file.service";

export const useFileState = (): {
	error: string;
	isLoading: boolean;
	files: File[],
	recentlyUploaded: string[];
	uploadProgress: number;
	currentPage: number,
	totalPages: number,
} => {
	const [isLoading, setIsLoading] = useState(false);
	const [error, setError] = useState("");
	const [files, setFiles] = useState<File[]>([]);
	const [recentlyUploaded, setRecentlyUploaded] = useState<string[]>([]);
	const [uploadProgress, setUploadProgress] = useState(0);
	const [currentPage, setCurrentPage] = useState<number>(1);
	const [totalPages, setTotalPages] = useState<number>(1);

	useEffect(() => {
		fileService.getFileCount();
		fileService.getFiles();

		const subscriptions: any[] = [
			fileQuery.isLoading$.subscribe((x) => setIsLoading(x)),
			fileQuery.error$.subscribe((x) => setError(x)),
			fileQuery.files$.subscribe((x) => setFiles(x ?? [])),
			fileQuery.recentlyUploaded$.subscribe((x) => setRecentlyUploaded(x ?? [])),
			fileQuery.uploadProgress$.subscribe((x) => setUploadProgress(x ?? 0)),
			fileQuery.currentPage$.subscribe((x) => setCurrentPage(x)),
			fileQuery.totalPages$.subscribe((x) => setTotalPages(x)),
		];

		return () => {
			subscriptions.map((it) => it.unsubscribe());
		};
	}, []);

	return {
		error,
		isLoading,
		files,
		recentlyUploaded,
		uploadProgress,
		currentPage,
		totalPages,
	};
};
