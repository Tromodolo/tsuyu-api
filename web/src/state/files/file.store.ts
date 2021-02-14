import { Store, StoreConfig } from "@datorama/akita";
import { FileState } from "./file.model";

const CreateInitialState = (): FileState => ({
	currentPage: 1,
	totalPages: 1,
	files: [],
});

@StoreConfig({
	name: "files",
	idKey: "_id",
})
export class FileStore extends Store<FileState> {
	constructor() {
		super(CreateInitialState());
	}
}

export const fileStore = new FileStore();
