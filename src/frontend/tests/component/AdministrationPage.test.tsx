import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, it, expect } from 'vitest';
import { I18nextProvider } from 'react-i18next';

import { i18n } from '../../src/i18n/i18n';
import { AdministrationPage } from '../../src/pages/administration/AdministrationPage';

function renderWithI18n() {
  return render(
    <I18nextProvider i18n={i18n}>
      <AdministrationPage />
    </I18nextProvider>
  );
}

describe('AdministrationPage', () => {
  it('renders 3 tabs with canonical labels', () => {
    renderWithI18n();

    expect(screen.getByRole('tab', { name: 'Músculos' })).toBeInTheDocument();
    expect(screen.getByRole('tab', { name: 'Mediciones' })).toBeInTheDocument();
    expect(screen.getByRole('tab', { name: 'Ejercicios' })).toBeInTheDocument();
  });

  it('shows the first tab active by default', () => {
    renderWithI18n();

    expect(screen.getByRole('tab', { name: 'Músculos' })).toHaveAttribute('aria-selected', 'true');
    expect(screen.getByRole('tab', { name: 'Mediciones' })).toHaveAttribute('aria-selected', 'false');
  });

  it('changes active tab when clicking the second tab', async () => {
    const user = userEvent.setup();
    renderWithI18n();

    await user.click(screen.getByRole('tab', { name: 'Mediciones' }));

    expect(screen.getByRole('tab', { name: 'Mediciones' })).toHaveAttribute('aria-selected', 'true');
    expect(screen.getByRole('tab', { name: 'Músculos' })).toHaveAttribute('aria-selected', 'false');
  });
});
