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
  onCancel
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
          <button className="pixel-btn teal" onClick={onConfirm}>
            Yes, play
          </button>

          <button className="pixel-btn pink" onClick={onCancel}>
            Cancel
          </button>
        </div>

      </div>
    </div>
  );
};

export default PlayConfirmModal;
