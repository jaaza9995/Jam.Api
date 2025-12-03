import React from "react";
import "../shared/Modal.css";
import { StoryStatsModalProps } from "../types/home";

const StoryStatsModal: React.FC<StoryStatsModalProps> = ({
	storyTitle,
	played,
	finished,
	failed,
	dnf,
	onConfirm,
}) => {
	return (
		<div className="modal-overlay">
			<div className="modal-box">
				<h2 className="modal-title">Stats for "{storyTitle}"</h2>
				<div className="modal-stats">
					<p>Total Played: {played}</p>
					<p>Finished: {finished}</p>
					<p>Failed: {failed}</p>
					<p>Did Not Finish: {dnf}</p>
				</div>

				<div className="modal-buttons">
					<button className="pixel-btn back" onClick={onConfirm}>
						BACK
					</button>
				</div>
			</div>
		</div>
	);
};

export default StoryStatsModal;
