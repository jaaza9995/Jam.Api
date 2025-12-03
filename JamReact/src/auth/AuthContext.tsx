import React, {
	createContext,
	useState,
	useContext,
	useEffect,
	ReactNode,
} from "react";
import { jwtDecode } from "jwt-decode";
import { LoginDto } from "../types/auth";
import { AuthContextType, User } from "../types/auth";
import * as authService from "./AuthService";

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: ReactNode }> = ({
	children,
}) => {
	const [user, setUser] = useState<User | null>(null);
	const [token, setToken] = useState<string | null>(
		localStorage.getItem("token")
	);
	const [isLoading, setIsLoading] = useState<boolean>(true);

	// --- Calculate Admin status ---
	// Checks if the user exists, and if the role includes "Admin"
	const isAdmin =
		!!user &&
		(Array.isArray(user.role)
			? user.role.includes("Admin")
			: user.role === "Admin");

	useEffect(() => {
		// Check token validity on mount and when token changes
		if (token) {
			try {
				const decodedUser: User = jwtDecode(token);
				// Check if the token is expired
				if (decodedUser.exp * 1000 > Date.now()) {
					setUser(decodedUser);
				} else {
					// Token is expired, clear it
					console.warn("Token expired");
					localStorage.removeItem("token");
					setUser(null);
					setToken(null);
				}
			} catch (error) {
				console.error("Invalid token", error);
				localStorage.removeItem("token");
			}
		}
		setIsLoading(false);
	}, [token]);

	const login = async (credentials: LoginDto) => {
		// Login and store token
		const { token } = await authService.login(credentials);
		localStorage.setItem("token", token);
		const decodedUser: User = jwtDecode(token);
		console.log(token);
		console.log("DECODED USER OBJECT:", decodedUser);
		setUser(decodedUser);
		setToken(token);
	};

	const logout = () => {
		// Clear token and user data
		localStorage.removeItem("token");
		setUser(null);
		setToken(null);
	};

	return (
		// Provide context to children
		<AuthContext.Provider
			value={{ user, token, login, logout, isLoading, isAdmin }}
		>
			{!isLoading && children}
		</AuthContext.Provider>
	);
};

export const useAuth = (): AuthContextType => {
	// Custom hook to use auth context
	const context = useContext(AuthContext);
	if (context === undefined) {
		throw new Error("useAuth must be used within an AuthProvider");
	}
	return context;
};
