export interface User {
	id: number;
	username: string;
	email: string;
	is_admin: boolean;
	api_key?: string;
	last_update: Date;
	created_at: Date;
}

export interface UserRegister {
	username: string;
	password: string;
	email?: string;
}

export interface UserLogin {
	username: string;
	password: string;
}

export interface PasswordUpdate {
	password: string;
	new_password: string;
}
