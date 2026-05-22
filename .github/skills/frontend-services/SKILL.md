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

Todo servicio de API MUST importar la funcion `apiFetch` desde su ubicacion centralizada. MUST NOT crearse otro cliente HTTP:

```ts
import { apiFetch } from '../../apiFetch';
```

`apiFetch<T>` encapsula la configuracion de base URL y el manejo de errores HTTP. Los servicios construyen la ruta, los parametros y las cabeceras.

### Cabeceras de autenticacion

Los servicios del area de administracion MUST incluir `adminAuthHeaders` en todas sus llamadas:

```ts
import { adminAuthHeaders } from '../../adminAuthHeaders';
```

PASS siempre como `headers` en el objeto de opciones de `apiFetch`. MUST NOT hardcodearse cabeceras de autenticacion en cada llamada individualmente.

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

import { apiFetch } from '../../apiFetch';
import { adminAuthHeaders } from '../../adminAuthHeaders';
import type { IMuscle, IMuscles, IMusclesFilter, IUpsertMuscleRequest } from '../../../interfaces/muscles/IMuscles';

// Listado paginado — construye URLSearchParams filtrando valores vacios/undefined/null
export function listMusclesPage(query: IMusclesFilter): Promise<IMuscles> {
  const params = new URLSearchParams(
    Object.entries(query)
      .filter(([, v]) => v !== undefined && v !== null && v !== '')
      .map(([k, v]) => [k, String(v)]),
  );
  return apiFetch<IMuscles>(`/api/admin/muscles?${params.toString()}`, { headers: adminAuthHeaders });
}

// Notas sobre serializacion de parametros:
// - string vacio ('') SE EXCLUYE del querystring (filtro de texto sin valor = no filtrar)
// - undefined y null SE EXCLUYEN
// - boolean false NO SE EXCLUYE — false !== '' es TRUE, por lo que se serializa como 'includeDeleted=false'
// - boolean true se serializa como 'includeDeleted=true'
// STOP: no cambies el filtro a 'v' (falsy) — excluiria los booleanos false que son semanticamente validos

export function getMuscleById(id: string): Promise<IMuscle> {
  return apiFetch<IMuscle>(`/api/admin/muscles/${id}`, { headers: adminAuthHeaders });
}

export function createMuscleApi(request: IUpsertMuscleRequest): Promise<IMuscle> {
  return apiFetch<IMuscle>('/api/admin/muscles', {
    method: 'POST',
    headers: adminAuthHeaders,
    body: JSON.stringify(request),
  });
}

export function updateMuscle(id: string, request: IUpsertMuscleRequest): Promise<void> {
  return apiFetch<void>(`/api/admin/muscles/${id}`, {
    method: 'PUT',
    headers: adminAuthHeaders,
    body: JSON.stringify(request),
  });
}

export function deleteMuscle(id: string): Promise<void> {
  return apiFetch<void>(`/api/admin/muscles/${id}`, {
    method: 'DELETE',
    headers: adminAuthHeaders,
  });
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

## Tipo de retorno del listado paginado

El tipo de retorno del listado paginado MUST coincidir con lo que devuelve la API. MUST definirse en la carpeta `interfaces/<modulo>/` del dominio correspondiente:

```ts
// interfaces/muscles/IMuscles.ts
export interface IMuscles {
  items: IMuscle[];
  totalCount: number;  // total de registros en base de datos (para paginacion)
  page: number;        // pagina actual (1-indexed, viene de la API)
  pageSize: number;
}
```

`totalCount` es lo que consume Redux para `table.totalCount`. La API devuelve `page` como 1-indexed; el reducer MUST convertirlo a 0-indexed al almacenarlo en `table.page`:

```ts
// reducer
state.table.page = action.payload.page - 1;  // API 1-indexed → MUI 0-indexed
```

MUST NOT definirse un tipo `PagedResult<T>` generico en `interfaces/common/` si los campos devueltos por la API difieren entre dominios.

## Convencion de nombres de funciones

- Listado paginado: `list<Entidades>Page` — `listMusclesPage`, `listRoutinesPage`
- Por id: `get<Entidad>ById` — `getMuscleById`, `getRoutineById`
- Crear: `create<Entidad>Api` — `createMuscleApi`, `createRoutineApi` (el sufijo `Api` evita colision con el nombre de la entidad en componentes)
- Editar: `update<Entidad>` — `updateMuscle`, `updateRoutine`
- Borrar: `delete<Entidad>` — `deleteMuscle`, `deleteRoutine`
- Autocomplete: `search<Entidades>` — `searchMuscles`, `searchExercises`

El nombre de la entidad MUST ser consistente con el usado en `interfaces/`, `redux/` y `pages/`.

## Estructura de carpetas para servicios de administracion

Los servicios usados exclusivamente por el area admin MUST ubicarse bajo `services/api/admin/<modulo>/`:

```
services/
  api/
    admin/
      muscles/
        musclesApi.ts
      exercises/
        exercisesApi.ts
      measurements/
        measurementsApi.ts
```

Los servicios de areas no-admin (p.ej. endpoints publicos o de usuario) van directamente bajo `services/api/<modulo>/`.
