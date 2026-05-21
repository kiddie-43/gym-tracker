import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, describe, expect, it, vi } from 'vitest';


import { MusclesPanel } from '../../../src/components/administration/MusclesPanel';
import * as musclesApi from '../../../src/services/api/admin/muscles/musclesApi';

const listMusclesMock = vi.fn();
const createMuscleMock = vi.fn();
const updateMuscleMock = vi.fn();
const deleteMuscleMock = vi.fn();

vi.spyOn(musclesApi, 'listMuscles').mockImplementation(listMusclesMock);
vi.spyOn(musclesApi, 'createMuscle').mockImplementation(createMuscleMock);
vi.spyOn(musclesApi, 'updateMuscle').mockImplementation(updateMuscleMock);
vi.spyOn(musclesApi, 'deleteMuscle').mockImplementation(deleteMuscleMock);

describe('AdminCrudTables', () => {
  beforeEach(() => {
    listMusclesMock.mockReset();
    createMuscleMock.mockReset();
    updateMuscleMock.mockReset();
    deleteMuscleMock.mockReset();

    listMusclesMock.mockResolvedValue([
      {
        id: 'muscle-1',
        name: 'Pecho',
        code: 'CHEST',
        description: 'Grupo muscular',
        active: true,
        isDeleted: false,
        deletedAt: null,
        muscleGroupIds: [],
      },
    ]);
    createMuscleMock.mockResolvedValue({ id: 'muscle-2' });
    updateMuscleMock.mockResolvedValue({ id: 'muscle-1' });
    deleteMuscleMock.mockResolvedValue(undefined);
  });

  it('renders admin table and creates a muscle', async () => {
    const user = userEvent.setup();

    render(<MusclesPanel />);

    await waitFor(() => expect(listMusclesMock).toHaveBeenCalled());
    expect(screen.getByText('Pecho')).toBeInTheDocument();

    await user.click(screen.getByRole('button', { name: /añadir/i }));

    await user.clear(screen.getByLabelText(/nombre/i));
    await user.type(screen.getByLabelText(/nombre/i), 'Espalda');

    await user.clear(screen.getByLabelText(/código/i));
    await user.type(screen.getByLabelText(/código/i), 'BACK');

    await user.click(screen.getByRole('button', { name: /guardar/i }));

    await waitFor(() => {
      expect(createMuscleMock).toHaveBeenCalledWith(
        expect.objectContaining({
          name: 'Espalda',
          code: 'BACK',
        }),
      );
    });
  });
});
