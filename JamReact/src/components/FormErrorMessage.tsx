import React from "react";

interface Props {
  message?: string | string[] | null;
}

const FormErrorMessage: React.FC<Props> = ({ message }) => {
  if (!message) return null;

  // normalize to array
  const messages = Array.isArray(message) ? message : [message];

  return (
    <div className="error-msg">
      {messages.map((m, i) => (
        <p key={i} style={{ margin: 0, padding: 0 }}>
          {m}
        </p>
      ))}
    </div>
  );
};

export default FormErrorMessage;
