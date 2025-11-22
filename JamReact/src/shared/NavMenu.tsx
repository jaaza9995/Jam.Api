import React from 'react';
import { Nav, Navbar, NavDropdown } from 'react-bootstrap';
import AuthSection from '../auth/AuthSection';
import './NavMenu.css';

const NavMenu: React.FC = () => {
  return (
    <Navbar expand="lg">
      <Navbar.Brand href="/">Math Universe</Navbar.Brand>
      <Navbar.Toggle aria-controls="basic-navbar-nav" />
      <Navbar.Collapse id="basic-navbar-nav">
        <Nav className="me-auto">
            {/* hvis vi får lyst til å legge inn navigasjon i menyen senere
          <Nav.Link href="/">Home</Nav.Link>
          <Nav.Link href="/items">Items</Nav.Link>
          */}
        </Nav>
      </Navbar.Collapse>
      <AuthSection />
  </Navbar>
  );
};

export default NavMenu;