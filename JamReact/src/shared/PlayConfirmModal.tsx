import React from "react";
import "./Modal.css";

interface PlayConfirmModalProps {
	title: string;
	show: boolean;
	onConfirm: () => void;
	onCancel: () => void;
}

const PlayConfirmModal: React.FC<PlayConfirmModalProps> = ({
	title,
	show,
	onConfirm,
	onCancel,
}) => {
	if (!show) return null;

	return (
		<div className="modal-overlay">
			<div className="modal-box">
				<h2 className="modal-title">Start Game?</h2>

				<p className="modal-text">
					Do you want to play <strong>"{title}"</strong>?
				</p>

				<div className="modal-buttons">
					<button className="pixel-btn back" onClick={onCancel}>
						Cancel
					</button>
					<button className="pixel-btn save" onClick={onConfirm}>
						Yes, play
					</button>
				</div>
			</div>
		</div>
	);
};

export default PlayConfirmModal;
