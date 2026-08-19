import { PopUpCode } from "../../../enums/popUp/popUp";
import { IPaginated } from "../IPaginated/IPaginated";

export interface IReduxState<T, U> {
    filters: T;
    form: U;
    table: IPaginated<U>;
    message: string | null;
    error: string | null;
    loading: boolean;
    popUpCode: PopUpCode;
    restTimerSeconds?: number;
    csvResult?: unknown;
}