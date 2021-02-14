import { Store, StoreConfig } from "@datorama/akita";
import { Settings } from "./settings.model";

const CreateInitialState = () => ({});

@StoreConfig({
	name: "settings",
	idKey: "_id",
})
export class SettingsStore extends Store<Settings> {
	constructor() {
		super(CreateInitialState());
	}
}

export const settingsStore = new SettingsStore();
