import { IExercise } from '../../../interfaces/IExercises/IExercises';
import type { ISession } from '../../../interfaces/ISession/ISession';
import { IPaginated } from '../../../interfaces/skeleton/IPaginated/IPaginated';
import { apiFetch } from '../../httpClient';

export function listSessionsByRoutineId(routineId: string) {
	return apiFetch<IPaginated<ISession>>(`/api/routines/${routineId}/sessions`);
}

export function createSession(routineId: string, request: ISession) {
	return apiFetch<ISession>(`/api/routines/${routineId}/sessions`, {
		method: 'POST',
		body: JSON.stringify(request),
	});
}

export function updateSession(routineId: string, sessionId: string, request: ISession) {
	return apiFetch<ISession>(`/api/routines/${routineId}/sessions/${sessionId}`, {
		method: 'PUT',
		body: JSON.stringify(request),
	});
}

export function deleteSession(routineId: string, sessionId: string) {
	return apiFetch<void>(`/api/routines/${routineId}/sessions/${sessionId}`, {
		method: 'DELETE',
	});
}

export function addSessionExercise(
	routineId: string,
	sessionId: string,
	request: { exerciseId: string; name: string },
) {
	return apiFetch<IExercise>(`/api/routines/${routineId}/sessions/${sessionId}/exercises`, {
		method: 'POST',
		body: JSON.stringify(request),
	});
}

export function unlinkSessionExercise(routineId: string, sessionId: string, exerciseId: string) {
	return apiFetch<void>(`/api/routines/${routineId}/sessions/${sessionId}/exercises/${exerciseId}`, {
		method: 'DELETE',
	});
}

export function getSessionExercises(routineId: string, sessionId: string) {
	return apiFetch<IExercise[]>(`/api/routines/${routineId}/sessions/${sessionId}/exercises`);
}

