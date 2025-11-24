import React, { useEffect, useState, useRef } from "react";
import { Table, Button, Alert, Spinner } from "react-bootstrap";
import { useAuth } from "../auth/AuthContext"; 
import { UserDataDto } from "../types/admin";
import { getAdminUsers } from "./AdminService";

const AdminUsersPage: React.FC = () => {
	const { token, user } = useAuth();
	const [users, setUsers] = useState<UserDataDto[]>([]);
	const [isLoading, setIsLoading] = useState(false);
	const [error, setError] = useState<string | null>(null);
	const hasFetchedRef = useRef(false);

	const fetchUsers = async () => {
		if (hasFetchedRef.current) return; // Prevent multiple fetches
		hasFetchedRef.current = true;

		if (!token) return;

		setIsLoading(true);
		setError(null);

		try {
			const data = await getAdminUsers();
			setUsers(data);
		} catch (err: any) {
			setError(err.message || "An unknown error occurred.");
		} finally {
			setIsLoading(false);
		}
	};

	useEffect(() => {
		fetchUsers();
	}, [token]);

	if (isLoading) {
		return <Spinner animation="border" />;
	}

	return (
		<div className="admin-page">
			<h2>Users</h2>
			{error && <Alert variant="danger">{error}</Alert>}

			<Table striped bordered hover variant="dark">
				<thead>
					<tr>
						<th>User ID</th>
						<th>Username</th>
						<th>Email</th>
						<th>Action</th>
					</tr>
				</thead>
				<tbody>
					{users.map((u) => (
						<tr key={u.id}>
							<td>{u.id.substring(0, 8)}...</td>
							<td>{u.userName}</td>
							<td>
								{/* Vi implementerer sletting senere */}
								<Button
									variant="danger"
									size="sm"
                                    // So the admin cannot delete themselves (dont know if it works)
									disabled={!!(user && user.sub === u.id)}
								>
									Delete
								</Button>
							</td>
						</tr>
					))}
				</tbody>
			</Table>
		</div>
	);
};

export default AdminUsersPage;
