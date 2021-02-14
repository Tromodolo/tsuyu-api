import { persistState } from "@datorama/akita";

export const InitPersist = () => {
	persistState({
		include: ["user"],
	});
};
