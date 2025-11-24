import React from "react";
import { Nav, Navbar, NavDropdown } from "react-bootstrap";
import AuthSection from "../auth/AuthSection";
import { useAuth } from "../auth/AuthContext";
import "./NavMenu.css";

const NavMenu: React.FC = () => {
	// Hent isAdmin-statusen fra AuthContext
	const { user, isAdmin } = useAuth();

	return (
		<Navbar expand="lg">
			<Navbar.Brand href="/">Math Universe</Navbar.Brand>
			<Navbar.Toggle aria-controls="basic-navbar-nav" />
			<Navbar.Collapse id="basic-navbar-nav">
				<Nav className="me-auto">
					{/* ---------------- ADMIN NAVIGATION ---------------- */}
					{user && isAdmin && (
						// <-- Show only if Admin is logged in
						<NavDropdown title="Admin" id="admin-nav-dropdown">
							<NavDropdown.Item href="/admin/stories">
								See all Stories
							</NavDropdown.Item>
							<NavDropdown.Item href="/admin/users">
								See all Users
							</NavDropdown.Item>
						</NavDropdown>
					)}
				</Nav>
			</Navbar.Collapse>
			<AuthSection />
		</Navbar>
	);
};

export default NavMenu;
