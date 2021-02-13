import { Store, StoreConfig } from "@datorama/akita";
import { User } from "./user.model";

const CreateInitialState = () => ({});

@StoreConfig({
	name: "user",
	idKey: "_id",
})
export class UserStore extends Store<User> {
	constructor() {
		super(CreateInitialState());
	}
}

export const userStore = new UserStore();
