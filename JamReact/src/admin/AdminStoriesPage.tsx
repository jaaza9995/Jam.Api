import React, { useEffect, useState, useRef } from 'react';
import { Table, Button, Alert, Spinner } from 'react-bootstrap';
import { useAuth } from '../auth/AuthContext'; 
import { getAdminStories } from './AdminService';
import { StoryDataDto } from '../types/admin';
import { Accessibility } from '../types/enums';

// Converts accessibility number (0 | 1) to string ("Public" | "Private")
const getAccessibilityString = (value: number): string => {
    const name = Accessibility[value]; 
    return name || "Ukjent"; 
};

const AdminStoriesPage: React.FC = () => {
    const { token } = useAuth();
    const [stories, setStories] = useState<StoryDataDto[]>([]);
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
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

    useEffect(() => {
        fetchStories();
    }, [token]);


    if (isLoading) {
        return <Spinner animation="border" />;
    }

    return (
        <div className="admin-page">
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
                    {stories.map(story => (
                        <tr key={story.id}>
                            <td>{story.id}</td> 
                            <td>{story.title}</td>
                            <td>{getAccessibilityString(story.accessibility)}</td>
                            <td>{story.userId.substring(0, 8)}...</td> 
                            <td>
                                {/* We can implement editing/deleting here later */}
                                <Button variant="info" size="sm" className="me-2" disabled>Edit</Button>
                                <Button variant="danger" size="sm" disabled>Delete</Button>
                            </td>
                        </tr>
                    ))}
                </tbody>
            </Table>
        </div>
    );
};

export default AdminStoriesPage;