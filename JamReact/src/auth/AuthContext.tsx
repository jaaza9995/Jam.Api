import React, { createContext, useState, useContext, useEffect, ReactNode } from 'react';
import { jwtDecode } from 'jwt-decode';
import { User } from '../types/user';
import { LoginDto } from '../types/auth';
import * as authService from './AuthService';

interface AuthContextType { // Define the shape of the auth context
    user: User | null;
    token: string | null;
    login: (credentials: LoginDto) => Promise<void>;
    logout: () => void;
    isLoading: boolean;
    isAdmin: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
    const [user, setUser] = useState<User | null>(null);
    const [token, setToken] = useState<string | null>(localStorage.getItem('token'));
    const [isLoading, setIsLoading] = useState<boolean>(true);

    // --- NY LOGIKK: Beregne Admin-status ---
    // Sjekker om brukeren finnes, og om rollen inkluderer "Admin"
    const isAdmin = !!user && (
        Array.isArray(user.role) // Hvis rollen er en array
            ? user.role.includes("Admin")
            : user.role === "Admin" // Hvis rollen er en enkelt streng
    );
    // ---------- Slutt på ny logikk ----------

    useEffect(() => { // Check token validity on mount and when token changes
        if (token) {
            try {
                const decodedUser: User = jwtDecode(token);
                // Check if the token is expired
                if (decodedUser.exp * 1000 > Date.now()) {
                    setUser(decodedUser);
                } else {
                    // Token is expired, clear it
                    console.warn("Token expired");
                    localStorage.removeItem('token');
                    setUser(null);
                    setToken(null);
                }
            } catch (error) {
                console.error("Invalid token", error);
                localStorage.removeItem('token');
            }
        }
        setIsLoading(false);
    }, [token]);

    const login = async (credentials: LoginDto) => { // Login and store token
        const { token } = await authService.login(credentials);
        localStorage.setItem('token', token);
        const decodedUser: User = jwtDecode(token);
        // console.log(token); // Debugging line to check the token
        // --- LEGG TIL DENNE LOGGEN MIDLERTIDIG ---
        console.log("DECODED USER OBJECT:", decodedUser); 
        // ------------------------------------------
        setUser(decodedUser);
        setToken(token);
    };

    const logout = () => { // Clear token and user data
        localStorage.removeItem('token');
        setUser(null);
        setToken(null);
    };

    return ( // Provide context to children
        <AuthContext.Provider value={{ user, token, login, logout, isLoading, isAdmin }}>
            {!isLoading && children}
        </AuthContext.Provider>
    );
};

export const useAuth = (): AuthContextType => { // Custom hook to use auth context
    const context = useContext(AuthContext);
    if (context === undefined) {
        throw new Error('useAuth must be used within an AuthProvider');
    }
    return context;
};