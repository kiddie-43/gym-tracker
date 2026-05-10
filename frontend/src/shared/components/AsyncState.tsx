import type { ReactNode } from 'react';

type AsyncStateProps = {
  isLoading?: boolean;
  error?: string | null;
  isEmpty?: boolean;
  emptyMessage?: string;
  children: ReactNode;
};

export function AsyncState({
  isLoading = false,
  error = null,
  isEmpty = false,
  emptyMessage = 'Todavia no hay datos disponibles.',
  children,
}: AsyncStateProps) {
  if (isLoading) {
    return <p>Cargando...</p>;
  }

  if (error) {
    return <p role="alert">{error}</p>;
  }

  if (isEmpty) {
    return <p>{emptyMessage}</p>;
  }

  return <>{children}</>;
}
