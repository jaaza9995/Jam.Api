export interface Props {
	onConfirm: () => void;
	onCancel: () => void;
}

export interface DeleteModalProps {
	title: string;
	onConfirm: () => void;
	onCancel: () => void;
}

export interface PlayConfirmModalProps {
	title: string;
	show: boolean;
	onConfirm: () => void;
	onCancel: () => void;
}