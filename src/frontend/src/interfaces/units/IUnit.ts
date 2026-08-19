import { ITableFilters } from "../skeleton/IPaginated/IPaginated";

export interface IUnit {
  id?: string;
  code: string;
  name: string;
  description?: string | null;
}

export interface IUnitFilters extends ITableFilters {
  code: string;
  name:string;
  description:string;

};
