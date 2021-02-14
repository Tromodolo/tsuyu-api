import { Query } from "@datorama/akita";
import { Settings } from "./settings.model";
import { settingsStore, SettingsStore } from "./settings.store";

export class SettingsQuery extends Query<Settings> {
	maxFileSizeBytes$ = this.select((state) => state.max_file_size_bytes);
	registerEnabled$ = this.select((state) => state.register_enabled);

	isLoading$ = this.selectLoading();
	error$ = this.selectError();

	constructor(protected store: SettingsStore) {
		super(store);
	}
}

export const settingsQuery = new SettingsQuery(settingsStore);
