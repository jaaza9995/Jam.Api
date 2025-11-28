import React, { createContext, useContext, useState, ReactNode } from "react";
import "./toast.css";

type ToastContextType = {
	showToast: (msg: string) => void;
};

const ToastContext = createContext<ToastContextType>({
	showToast: () => {},
});

export const useToast = () => useContext(ToastContext);

export const ToastProvider = ({ children }: { children: ReactNode }) => {
	const [message, setMessage] = useState<string>("");

	const showToast = (msg: string) => {
		setMessage(msg);
		setTimeout(() => setMessage(""), 3000);
	};

	return (
		<ToastContext.Provider value={{ showToast }}>
			{children}
			{message && <div className="jam-toast">{message}</div>}
		</ToastContext.Provider>
	);
};
