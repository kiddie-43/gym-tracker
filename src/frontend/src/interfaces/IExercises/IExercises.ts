import { IMuscle } from "../muscles/IMuscles";
import { ITableFilters } from "../skeleton/IPaginated/IPaginated";
import { IUnit } from "../units/IUnit";


export interface IExercise {
    id?: string;
    code: string;
    name: string;
    description: string;
    primaryMuscles: IMuscle[];
    secondaryMuscles: IMuscle[];
    units: IUnit[];
} 

export interface IExerciseToSave  {
    code: string;
    name: string;
    description: string;
    primaryMuscles: string[];
    secondaryMuscles: string[];
    units: string[];
}

export interface IExercisesFilter extends ITableFilters {
    code?: string;
    name?: string;
    primaryMuscleId?: string[];
    secondaryMuscleId?: string[];
    unitId?: string[];
}