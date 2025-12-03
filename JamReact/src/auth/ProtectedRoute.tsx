import React from "react";
import { Navigate, Outlet } from "react-router-dom";
import { useAuth } from "./AuthContext";

const ProtectedRoute: React.FC = () => {
	const { token } = useAuth();

	if (!token) {
		// Redirect user to the /login page, but save the current location they were trying
		// to go to. This allows us to send them along to that page after they login.
		return <Navigate to="/login" replace />;
	}

	return <Outlet />;
};

export default ProtectedRoute;
