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
        <p>Do you want to undo your changes?</p>

        <button className="pixel-btn pink" onClick={onConfirm}>
          Yes
        </button>

        <button className="pixel-btn blue" onClick={onCancel}>
          No
        </button>
      </div>
    </div>
  );
};

export default ConfirmUndoModal;
