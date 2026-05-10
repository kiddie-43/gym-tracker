import { render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';

import { CatalogStatusBanner } from '../../../src/features/catalog/components/CatalogStatusBanner';

describe('CatalogStatusBanner', () => {
  it('shows degraded warning when catalog is stale', () => {
    render(<CatalogStatusBanner availability={{ isStale: true, lastUpdatedAt: '2026-05-05T10:00:00Z' }} />);
    expect(screen.getByText(/degraded mode/i)).toBeInTheDocument();
  });
});
