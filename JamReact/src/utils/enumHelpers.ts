import { Accessibility, DifficultyLevel } from '../types/enums';

/**
 * Konverterer Accessibility enum-verdi (0/1) til lesbar streng ("Public" / "Private").
 * @param value Den numeriske verdien mottatt fra Backenden.
 * @returns Strengen som skal vises i UI, eller en fallback.
 */
export const getAccessibilityString = (value: number | undefined | null): string => {
    if (value === undefined || value === null) {
        return "N/A";
    }
    // TypeScript enums kan slå opp navnet fra verdien
    const name = Accessibility[value]; 
    return name || "Ukjent"; 
};

/**
 * Konverterer DifficultyLevel enum-verdi (0/1/2...) til lesbar streng.
 * @param value Den numeriske verdien mottatt fra Backenden.
 * @returns Strengen som skal vises i UI, eller en fallback.
 */
export const getDifficultyLevelString = (value: number | undefined | null): string => {
    if (value === undefined || value === null) {
        return "N/A";
    }
    const name = DifficultyLevel[value]; 
    
    // Hvis DifficultyLevel er f.eks. 0=Easy, 1=Medium, 2=Hard
    // vil dette returnere "Easy", "Medium", eller "Hard"
    return name || "Ukjent"; 
};