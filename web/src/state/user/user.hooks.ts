import { useEffect, useState } from "react";
import { User } from "./user.model";
import { userQuery } from "./user.query";

export const useAuthenticationState = (): {
	error: string;
	isLoading: boolean;
	isLoggedIn: boolean;
	user: User | undefined;
} => {
	const [isLoading, setIsLoading] = useState(false);
	const [error, setError] = useState("");
	const [user, setUser] = useState<User>();
	const [loggedIn, setLoggedIn] = useState<boolean>(false);

	useEffect(() => {
		const subscriptions: any[] = [
			userQuery.isLoading$.subscribe((x) => setIsLoading(x)),
			userQuery.error$.subscribe((x) => setError(x)),
			userQuery.user$.subscribe((x) => setUser(x)),
			userQuery.isLoggedIn$.subscribe((x) => setLoggedIn(x)),
		];

		return () => {
			subscriptions.map((it) => it.unsubscribe());
		};
	}, []);

	return {
		isLoggedIn: loggedIn,
		user,
		error,
		isLoading,
	};
};
