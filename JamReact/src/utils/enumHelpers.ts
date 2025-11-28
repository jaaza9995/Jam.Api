import { Accessibility, DifficultyLevel } from "../types/enums";

/**
 * Converts Accessibility enum value (0/1) to readable string ("Public" / "Private").
 * @param value The numeric value received from the Backend.
 * @returns The string to display in the UI, or a fallback.
 */
export const getAccessibilityString = (
	value: number | undefined | null
): string => {
	if (value === undefined || value === null) {
		return "N/A";
	}
	const name = Accessibility[value];
	return name || "Ukjent";
};

/**
 * Converts DifficultyLevel enum value (0/1/2...) to readable string.
 * @param value The numeric value received from the Backend.
 * @returns The string to display in the UI, or a fallback.
 */
export const getDifficultyLevelString = (
	value: number | undefined | null
): string => {
	if (value === undefined || value === null) {
		return "N/A";
	}
	const name = DifficultyLevel[value];
	return name || "Ukjent";
};
