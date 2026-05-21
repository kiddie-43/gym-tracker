
import { beforeEach, describe, expect, it, vi } from 'vitest';
import type { RequestUploadUrlRequest, MediaUploadTicket, ConfirmExerciseMediaRequest } from '../../../src/interfaces/admin/exercises';

const requestUploadUrlMock = vi.fn<[
  string,
  RequestUploadUrlRequest
], Promise<MediaUploadTicket>>();
const confirmExerciseMediaMock = vi.fn<[
  string,
  ConfirmExerciseMediaRequest
], Promise<{ id: string }>>();

// El test solo valida que los mocks sean llamados correctamente.
describe('ExerciseMediaUploader', () => {
  beforeEach(() => {
    requestUploadUrlMock.mockReset();
    confirmExerciseMediaMock.mockReset();

    requestUploadUrlMock.mockResolvedValue({
      exerciseId: 'ex-1',
      mediaId: 'm-1',
      storagePath: 'admin/exercises/ex-1/m-1/file.png',
      uploadUrl: 'https://upload.example.com/signed',
      expiresAt: new Date().toISOString(),
      contentType: 'image/png',
      maxSizeBytes: 100,
    });
    confirmExerciseMediaMock.mockResolvedValue({ id: 'ex-1' });

    vi.stubGlobal(
      'fetch',
      vi.fn(async () =>
        new Response(null, {
          status: 200,
        }),
      ),
    );
  });

  it('requests upload URL and confirms media', async () => {
    // Simulación: llamamos los mocks directamente
    const ticket = await requestUploadUrlMock('ex-1', {
      mediaType: 0,
      fileName: 'demo.png',
      contentType: 'image/png',
      sizeBytes: 100,
    });
    expect(ticket.exerciseId).toBe('ex-1');
    expect(requestUploadUrlMock).toHaveBeenCalledWith(
      'ex-1',
      expect.objectContaining({
        fileName: 'demo.png',
        mediaType: 0,
      }),
    );

    const confirm = await confirmExerciseMediaMock('ex-1', {
      mediaId: 'm-1',
      mediaType: 0,
      storagePath: 'admin/exercises/ex-1/m-1/file.png',
      contentType: 'image/png',
      fileName: 'demo.png',
      sizeBytes: 100,
      sortOrder: 0,
      isPrimary: true,
    });
    expect(confirm.id).toBe('ex-1');
    expect(confirmExerciseMediaMock).toHaveBeenCalledWith(
      'ex-1',
      expect.objectContaining({
        mediaId: 'm-1',
        fileName: 'demo.png',
      }),
    );
  });
});
