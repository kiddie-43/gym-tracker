export interface ISession {
    id:string;
    name: string;
    daysOfWeek: string[];
    countExercices?: number;
}

export interface ISessionFilter {
    search?: string;
    daysOfWeek?: string[];
}