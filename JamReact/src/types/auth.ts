export interface LoginDto {
	username: string;
	password: string;
}

export interface RegisterDto {
	username: string;
	email: string;
	password: string;
}

export interface AuthContextType {
	user: User | null;
	token: string | null;
	login: (credentials: LoginDto) => Promise<void>;
	logout: () => void;
	isLoading: boolean;
	isAdmin: boolean;
}

export interface User {
	userId: string;
	// subject/ username
	sub: string;
    // name of the user
	unique_name: string;
	// (JWT ID):id for JWT to prevenet replay attacks
	jti: string;
	// (Issued At):timestamp when token was issued
	iat: number;
	// (Expiration Time): timestamp when token will expire
	exp: number;
	// (Issuer): identifies authority that issued the token
	iss: string;
	// (Audience): identifies recipients that the token is intended for
	aud: string;
	// (Roles): roles assigned to the user
	role: string | string[];
}
