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
    onCancel,
}) => {
    return (
        <div className="modal-overlay">
            <div className="modal-box">
                <h2 className="modal-title">Statistics for "{storyTitle}"</h2>
                <div className="modal-stats">
                    <p>Total Played: {played}</p>
                    <p>Finished: {finished}</p>
                    <p>Failed: {failed}</p>
                    <p>Did Not Finish: {dnf}</p>
                </div>

                <div className="modal-buttons">
                    <button className="btn-white" onClick={onConfirm}>
                        OK
                    </button>
                    <button className="btn-pink" onClick={onCancel}>
                        Cancel
                    </button>
                </div>
            </div>
        </div>
    );
};

export default StoryStatsModal;