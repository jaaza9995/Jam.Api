import React, { useEffect, useState, useRef } from "react";
import { Table, Button, Alert, Spinner } from "react-bootstrap";
import { useAuth } from "../auth/AuthContext";
import { getAdminStories, adminDeleteStory } from "./AdminService";
import { StoryDataDto } from "../types/admin";
import { getAccessibilityString } from "../utils/enumHelpers";
import DeleteModal from "../shared/DeleteModal";

const AdminStoriesPage: React.FC = () => {
	const { token } = useAuth();
	const [stories, setStories] = useState<StoryDataDto[]>([]);
	const [isLoading, setIsLoading] = useState(false);
	const [error, setError] = useState<string | null>(null);
	const [showDeleteModal, setShowDeleteModal] = useState(false);
	const [selectedStory, setSelectedStory] = useState<StoryDataDto | null>(null);
	const hasFetchedRef = useRef(false);

	const fetchStories = async () => {
		if (hasFetchedRef.current) return; // Prevent multiple fetches
		hasFetchedRef.current = true;

		if (!token) return;

		setIsLoading(true);
		setError(null);

		try {
			const data = await getAdminStories();
			setStories(data);
		} catch (err: any) {
			setError(err.message || "An unknown error occurred.");
		} finally {
			setIsLoading(false);
		}
	};

	const deleteStory = async (storyId: number) => {
		try {
			await adminDeleteStory(storyId);
			setStories((prevStories) =>
				prevStories.filter((s) => s.id !== storyId)
			);
		} catch (err: any) {
			setError(err.message || "Failed to delete story.");
		}
	};

	useEffect(() => {
		fetchStories();
	}, [token]);

	if (isLoading) {
		return <Spinner animation="border" />;
	}

	return (
		<div className="admin-page">
			{showDeleteModal && selectedStory && (
				<DeleteModal
					title={selectedStory.title}
					onConfirm={async () => {
						await deleteStory(selectedStory.id);
						setShowDeleteModal(false);
						setSelectedStory(null);
					}}
					onCancel={() => {
						setShowDeleteModal(false);
						setSelectedStory(null);
					}}
				/>
			)}

			<h2>Stories</h2>
			{error && <Alert variant="danger">{error}</Alert>}

			<Table striped bordered hover variant="dark">
				<thead>
					<tr>
						<th>Story ID</th>
						<th>Title</th>
						<th>Accessibility </th>
						<th>Creater (User ID)</th>
						<th>Action</th>
					</tr>
				</thead>
				<tbody>
					{stories.map((story) => (
						<tr key={story.id}>
							<td>{story.id}</td>
							<td>{story.title}</td>
							<td>
								{getAccessibilityString(story.accessibility)}
							</td>
							<td>
								{story.userId?.trim() ? (
									`${story.userId.substring(0, 8)}...`
								) : (
									<span>No owner (deleted)</span>
								)}
							</td>
							<td>
								<Button
									variant="danger"
									size="sm"
									onClick={() => {
										setSelectedStory(story);
										setShowDeleteModal(true);
									}}
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

export default AdminStoriesPage;
