import React from "react";
import "./Modal.css";

interface DeleteModalProps {
	title: string;
	onConfirm: () => void;
	onCancel: () => void;
}

const DeleteModal: React.FC<DeleteModalProps> = ({
	title,
	onConfirm,
	onCancel,
}) => {
	return (
		<div className="modal-overlay">
			<div className="modal-box">
				<h2 className="modal-title">Delete "{title}"?</h2>

				<p className="modal-warning">This action cannot be undone.</p>

				<div className="modal-buttons">
					<button className="btn-white" onClick={onConfirm}>
						Yes, delete
					</button>
					<button className="btn-pink" onClick={onCancel}>
						Cancel
					</button>
				</div>
			</div>
		</div>
	);
};

export default DeleteModal;
