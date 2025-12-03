import React from "react";
import { DeleteModalProps } from "../types/shared";
import "./Modal.css";

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
					<button className="pixel-btn back" onClick={onCancel}>
						Cancel
					</button>
					<button className="pixel-btn delete" onClick={onConfirm}>
						Yes, delete
					</button>
				</div>
			</div>
		</div>
	);
};

export default DeleteModal;
