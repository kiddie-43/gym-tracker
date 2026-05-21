---
name: frontend-services
description: 'Estandares de servicios en el frontend React de Gym Tracker. Usar cuando: se crea o modifica un servicio en src/frontend/src/services/, se añade integracion con la API REST, Firebase Auth, Firestore, almacenamiento local o logging/metricas. Contiene la estructura de carpetas obligatoria de services/, reglas de modularidad y la prohibicion de index.ts centralizados.'
---

# Frontend — Estructura de Servicios

## Estructura de carpetas obligatoria

Dentro de `src/frontend/src/services/`:

```
services/
  api/                   Llamadas a la API REST interna/externa
    auth/
      authApi.ts
      authApi.test.ts
    user/
      userApi.ts
      userApi.test.ts
    <modulo>/
      <modulo>Api.ts
      <modulo>Api.test.ts

  firebase/              Servicios especificos de Firebase
    auth/
      authService.ts
      authService.test.ts
    firestore/
      firestoreService.ts
      firestoreService.test.ts

  storage/               Almacenamiento local o de sesion
    localStorage/
      localStorageService.ts
      localStorageService.test.ts
    sessionStorage/
      sessionStorageService.ts
      sessionStorageService.test.ts

  observability/         Logging y monitoreo
    logger/
      logger.ts
      logger.test.ts
    metrics/
      metrics.ts
      metrics.test.ts
```

## Reglas

- Cada servicio MUST tener su propia carpeta con archivo de implementacion y archivo de pruebas.
- NO se utilizaran archivos `index.ts` para exportaciones centralizadas. Cada servicio MUST importarse directamente desde su archivo correspondiente.
- La estructura MUST mantenerse por dominio funcional, no por tipo tecnico.

## Cliente HTTP

Todo servicio de API MUST importar el cliente HTTP base desde su ubicacion centralizada. MUST NOT crearse otro cliente HTTP:

```ts
import { httpClient } from '../../httpClient';
```

`httpClient` encapsula la configuracion de base URL, cabeceras y manejo de errores HTTP. Los servicios solo construyen la ruta y los parametros.

## Contrato estandar de un servicio CRUD

Todo servicio de dominio bajo `services/api/<modulo>/` MUST implementar estas funciones. El orden de declaracion MUST ser el que aparece aqui:

| Funcion | Cuando es obligatoria | Proposito |
|---|---|---|
| `get<Entidades>Page` | Siempre | Listado paginado con filtros y ordenacion |
| `get<Entidad>ById` | Siempre | Obtener un item por id |
| `create<Entidad>` | Siempre | Crear una nueva entidad |
| `update<Entidad>` | Siempre | Editar una entidad existente |
| `delete<Entidad>` | Siempre | Borrar (logico o fisico) una entidad |
| `search<Entidades>` | Solo si hay autocomplete | Busqueda rapida por texto para selectores |

### Firmas de funcion

```ts
// <modulo>Api.ts

import { httpClient } from '../../httpClient';
import type { MuscleDto, UpsertMuscleRequest, MusclesFilters } from '../../../interfaces/muscles/muscles';
import type { PagedResult, PaginationState, SortState } from '../../../interfaces/common/common';

export async function getMusclesPage(
  filters: MusclesFilters,
  pagination: PaginationState,
  sort: SortState,
): Promise<PagedResult<MuscleDto>> {
  return httpClient.get('/muscles', {
    params: {
      ...filters,
      page: pagination.page,
      pageSize: pagination.rowsPerPage,
      sortField: sort.field,
      sortDir: sort.direction,
    },
  });
}

export async function getMuscleById(id: string): Promise<MuscleDto> {
  return httpClient.get(`/muscles/${id}`);
}

export async function createMuscle(request: UpsertMuscleRequest): Promise<MuscleDto> {
  return httpClient.post('/muscles', request);
}

export async function updateMuscle(id: string, request: UpsertMuscleRequest): Promise<MuscleDto> {
  return httpClient.put(`/muscles/${id}`, request);
}

export async function deleteMuscle(id: string): Promise<void> {
  return httpClient.delete(`/muscles/${id}`);
}
```

### Funcion de busqueda para autocomplete (opcional)

Se añade **solo si** existe un campo de selector en otro modulo que necesite buscar entidades de este dominio por texto. Devuelve un array plano, no paginado:

```ts
export async function searchMuscles(query: string): Promise<MuscleDto[]> {
  return httpClient.get('/muscles/search', { params: { q: query } });
}
```

Esta funcion se usa en un thunk de autocomplete separado del thunk de listado principal. MUST NOT reutilizarse `get<Entidades>Page` para alimentar un selector.

## Tipo `PagedResult<T>`

El tipo de retorno del listado paginado MUST definirse en `interfaces/common/common.ts` y reutilizarse en todos los servicios:

```ts
// interfaces/common/common.ts
export type PagedResult<T> = {
  items: T[];
  total: number;
};
```

`total` es el total de registros en base de datos (no el numero de items de la pagina). Lo consume Redux para el campo `pagination.total`.

## Convencion de nombres de funciones

- Listado paginado: `get<Entidades>Page` — `getMusclesPage`, `getRoutinesPage`
- Por id: `get<Entidad>ById` — `getMuscleById`, `getRoutineById`
- Crear: `create<Entidad>` — `createMuscle`, `createRoutine`
- Editar: `update<Entidad>` — `updateMuscle`, `updateRoutine`
- Borrar: `delete<Entidad>` — `deleteMuscle`, `deleteRoutine`
- Autocomplete: `search<Entidades>` — `searchMuscles`, `searchExercises`

El nombre de la entidad MUST ser consistente con el usado en `interfaces/`, `redux/` y `pages/`.
