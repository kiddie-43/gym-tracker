import { render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';

import { MetricNameBlocks } from '../../../../../src/pages/routines/exercices/detail/components/MetricNameBlocks';

describe('MetricNameBlocks', () => {
  it('renders one block per metric name', () => {
    render(
      <MetricNameBlocks
        metrics={[
          { metricId: 'm1', name: 'Peso', dataType: 'decimal', unit: 'kg' },
          { metricId: 'm2', name: 'Repeticiones', dataType: 'integer', unit: null },
        ]}
        loading={false}
        error={null}
        degraded={false}
      />,
    );

    expect(screen.getByText('Peso')).toBeInTheDocument();
    expect(screen.getByText('Repeticiones')).toBeInTheDocument();
  });

  it('shows loading state', () => {
    render(
      <MetricNameBlocks
        metrics={[]}
        loading
        error={null}
        degraded={false}
      />,
    );

    expect(screen.getByLabelText('Cargando metricas')).toBeInTheDocument();
  });

  it('shows error state', () => {
    render(
      <MetricNameBlocks
        metrics={[]}
        loading={false}
        error="Fallo de carga"
        degraded={false}
      />,
    );

    expect(screen.getByText('Fallo de carga')).toBeInTheDocument();
  });

  it('shows empty state when metrics list is empty', () => {
    render(
      <MetricNameBlocks
        metrics={[]}
        loading={false}
        error={null}
        degraded={false}
      />,
    );

    expect(screen.getByText('No hay metricas disponibles para este ejercicio.')).toBeInTheDocument();
  });

  it('shows degraded notice when degraded flag is enabled', () => {
    render(
      <MetricNameBlocks
        metrics={[{ metricId: 'm1', name: 'Tiempo', dataType: 'time', unit: 's' }]}
        loading={false}
        error={null}
        degraded
      />,
    );

    expect(screen.getByText(/Modo degradado activo/i)).toBeInTheDocument();
  });

  it('shows degraded fallback message when metrics are empty', () => {
    render(
      <MetricNameBlocks
        metrics={[]}
        loading={false}
        error={null}
        degraded
      />,
    );

    expect(screen.getByText(/Modo degradado activo/i)).toBeInTheDocument();
  });
});