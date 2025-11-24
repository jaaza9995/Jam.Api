import { Accessibility } from "./enums";

export interface UserDataDto {
	id: string;
	userName: string;
}

export interface StoryDataDto {
    id: number;
    title: string;
    accessibility: number;
    userId: string;
}