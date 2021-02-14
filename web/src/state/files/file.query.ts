import { Query } from "@datorama/akita";
import { FileState } from "./file.model";
import { fileStore, FileStore } from "./file.store";

export class FileQuery extends Query<FileState> {
	files$ = this.select((state) => state.files);
	recentlyUploaded$ = this.select((state) => state.recentlyUploaded);
	uploadProgress$ = this.select((state) => state.uploadProgress);
	currentPage$ = this.select((state) => state.currentPage);
	totalPages$ = this.select((state) => state.totalPages);

	isLoading$ = this.selectLoading();
	error$ = this.selectError();

	constructor(protected store: FileStore) {
		super(store);
	}
}

export const fileQuery = new FileQuery(fileStore);
