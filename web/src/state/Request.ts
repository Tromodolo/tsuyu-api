import axios, { AxiosError, AxiosResponse } from "axios";
import { userQuery } from "./user";

export const API_URL = process.env.API_URL ?? "http://localhost:7000";
let ApiKey: string | undefined = "";
userQuery.api_key.subscribe((api) => {
	ApiKey = api;
});

export interface ApiResponse<T> {
	status: number;
	json?: T;
	error?: string;
};
export type Endpoint = 
	| "upload"
	| "settings"
	| "delete"
	| "files"
	| "file-count"
	| "login"
	| "register"
	| "reset-token"
	| "change-password";
export type Method = "GET" | "POST" | "PUT" | "DELETE";
export async function SendRequest<T>(
	endpoint: Endpoint,
	method: Method,
	body?: { [key: string]: any },
	params?: [value: string],
): Promise<ApiResponse<T>> {
	let url = `${API_URL}/${endpoint}`;
	if (params !== undefined) {
		for (const val of params) {
			url += `/${val}`;
		}
	}

	let res: AxiosResponse;
	try {
		res = await axios({
			method,
			url,
			data: body,
			headers: {
				"Content-Type": "application/json",
				"Authorization": ApiKey ?? "",
			}
		});
	} catch (err) {
		const axErr: AxiosError = err;
		if (axErr.response){
			res = axErr.response;
		} else {
			throw new Error("Request failed");
		}
	}
	
	if (res.status === 200){
		return {
			status: 200,
			json: res.data,
		}
	} else {
		return {
			status: res.status,
			json: res.data,
			error: res.data,
		}
	}
}