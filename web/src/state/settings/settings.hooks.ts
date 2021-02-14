import { useEffect, useState } from "react";
import { settingsQuery } from "./settings.query";
import { settingsService } from "./settings.service";

export const useSettings = (): {
	register_enabled: boolean;
	max_file_size_bytes: number;
	error: string;
	isLoading: boolean;
} => {
	const [isLoading, setIsLoading] = useState(false);
	const [error, setError] = useState("");
	const [registerEnabled, setRegisterEnabled] = useState<boolean>(false);
	const [maxFileSizeBytes, setMaxFileSizeBytes] = useState<number>(0);

	useEffect(() => {
		settingsService.fetchSettings();

		const subscriptions: any[] = [
			settingsQuery.isLoading$.subscribe((x) => setIsLoading(x)),
			settingsQuery.error$.subscribe((x) => setError(x)),
			settingsQuery.registerEnabled$.subscribe((x) => setRegisterEnabled(x)),
			settingsQuery.maxFileSizeBytes$.subscribe((x) => setMaxFileSizeBytes(x)),
		];

		return () => {
			subscriptions.map((it) => it.unsubscribe());
		};
	}, []);

	return {
		register_enabled: registerEnabled,
		max_file_size_bytes: maxFileSizeBytes,
		error,
		isLoading,
	};
};
