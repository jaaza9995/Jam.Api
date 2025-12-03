import React from "react";
import "./Modal.css";

interface Props {
	onConfirm: () => void;
	onCancel: () => void;
}

const ConfirmUndoModal: React.FC<Props> = ({ onConfirm, onCancel }) => {
	return (
		<div className="modal-overlay">
			<div className="modal-box">
				<p className="modal-title">Do you want to leave without saving your changes?</p>

				<div className="modal-buttons">
					<button className="pixel-btn back" onClick={onConfirm}>
						YES
					</button>

					<button className="pixel-btn save" onClick={onCancel}>
						NO!
					</button>
				</div>
			</div>
		</div>
	);
};

export default ConfirmUndoModal;
