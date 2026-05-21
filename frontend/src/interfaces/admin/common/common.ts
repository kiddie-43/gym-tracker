export type AdminEntityBase = {
  id: string;
  name: string;
  code: string;
  description?: string | null;
  active: boolean;
  isDeleted?: boolean;
};
