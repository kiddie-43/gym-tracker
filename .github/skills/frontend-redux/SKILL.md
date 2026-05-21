---
name: frontend-redux
description: 'Estandares de Redux en el frontend de Gym Tracker. Usar cuando: se crea o modifica estado global, se añaden actions/reducers/states, se conecta un modulo CRUD al store, se define el contrato de estado de un modulo, se configura store.ts o se implementan selectores. Contiene estructura de carpetas, flujo obligatorio, contrato de estado por modulo, naming conventions, popUpCode estandar y reglas de tipado.'
---

# Frontend — Estructura y Flujo de Redux

## Tecnologia obligatoria

- MUST usarse **@reduxjs/toolkit** (RTK). MUST NOT implementarse Redux sin RTK.
- Acciones: MUST crearse con `createAction` de RTK.
- Reducers: MUST crearse con `createReducer` de RTK (permite mutacion imutable via Immer).
- Selectores derivados: MUST crearse con `createSelector` de RTK.
- Operaciones asincronas: MUST usarse `createAsyncThunk` de RTK. MUST NOT hacerse llamadas a API directamente en reducers.

## Estructura de carpetas obligatoria

Dentro de `src/frontend/src/redux/`:

```
redux/
  actions/
    <nombreModulo>/
      <nombreModulo>Actions.ts
  reducers/
    <nombreModulo>/
      <nombreModulo>Reducer.ts
  states/
    <nombreModulo>/
      <nombreModulo>State.ts
  store.ts
```

- El nombre del modulo MUST ser consistente en las 3 capas: `actions/`, `reducers/` y `states/`.
- MUST NOT usarse carpetas `example*` ni nombres genericos en codigo real.
- Al añadir un nuevo modulo MUST registrarse su reducer en `store.ts`.

## Convencion de naming obligatoria

**Acciones**: `set<Modulo><Campo>` para setters, `reset<Modulo>` para reset total.

```ts
// Ejemplos validos
export const setDietsList         = createAction<DietDto[]>('diets/setList');
export const setDietsForm         = createAction<UpsertDietRequest>('diets/setForm');
export const setDietsFilters      = createAction<DietsFilters>('diets/setFilters');
export const setDietsLoading      = createAction<boolean>('diets/setLoading');
export const setDietsError        = createAction<string | null>('diets/setError');
export const setDietsPopUpCode    = createAction<string | null>('diets/setPopUpCode');
export const setDietsPagination   = createAction<PaginationState>('diets/setPagination');
export const setDietsSort         = createAction<SortState>('diets/setSort');
export const resetDiets           = createAction('diets/reset');
```

**Type string de la accion**: MUST seguir el patron `'<modulo>/<campo>'` en camelCase.

## Flujo obligatorio

1. La UI MUST disparar acciones via `dispatch`.
2. Las acciones MUST representar eventos de negocio del modulo.
3. Los reducers MUST actualizar estado de forma inmutable (RTK usa Immer internamente).
4. `store.ts` MUST centralizar la configuracion y el registro de todos los reducers.
5. La UI MUST leer estado mediante selectores; MUST NOT accederse al store directamente.

## Patron de conexion a Redux en componentes

Solo el componente de pagina (`<Dominio>Page.tsx`) MUST conectarse a Redux via `useSelector`. Los componentes hijos (tabla, formulario, filtros) MUST recibir datos por props desde la pagina — MUST NOT usar `useSelector` directamente en componentes hijos.

**Razon**: conectar cada hijo al store crea dependencias ocultas, dificulta las pruebas, aumenta re-renders innecesarios y hace la estructura mas dificil de rastrear cuando algo falla.

```tsx
// DietsPage.tsx — UNICO componente que lee Redux
const { list, loading, error, pagination, sort, popUpCode } = useSelector(selectDietsState);
const dispatch = useDispatch();

// Pasa datos hacia abajo por props
<DietsTable
  rows={list}
  pagination={pagination}
  sort={sort}
  onEdit={() => dispatch(setDietsPopUpCode('edit'))}
/>
```

```tsx
// DietsTable.tsx — componente hijo, sin useSelector
interface DietsTableProps {
  rows: DietDto[];
  pagination: PaginationState;
  sort: SortState;
  onEdit: () => void;
}

export function DietsTable({ rows, pagination, sort, onEdit }: DietsTableProps) {
  // solo renderiza — no sabe nada de Redux
}
```

**Excepcion permitida**: un componente global en `components/` puede leer `redux/states/preferences/` si necesita adaptar su presentacion (tema, idioma). MUST documentarse con un comentario en el componente.

## Patron estandar de async thunk

Cada operacion asincrona MUST seguir este patron en el archivo de acciones del modulo:

```ts
// dietsActions.ts
export const fetchDiets = createAsyncThunk(
  'diets/fetchList',
  async (_, { dispatch }) => {
    dispatch(setDietsLoading(true));
    dispatch(setDietsError(null));
    try {
      const data = await dietsApi.getAll();
      dispatch(setDietsList(data));
    } catch (e) {
      dispatch(setDietsError(getErrorMessage(e)));
    } finally {
      dispatch(setDietsLoading(false));
    }
  },
);
```

Reglas:
- MUST despacharse `setLoading(true)` al inicio y `setLoading(false)` en `finally`.
- MUST despacharse `setError(null)` antes de la llamada y `setError(mensaje)` en el `catch`.
- MUST NOT hacerse llamadas a la API directamente en un reducer.
- La funcion `getErrorMessage` MUST definirse en `utils/` para extraer el mensaje de cualquier tipo de error; MUST NOT duplicarse esta logica en cada thunk.

**Reset en desmontaje**: la pagina MUST resetear el estado del modulo al desmontarse para evitar datos obsoletos en navegaciones futuras:

```tsx
// DietsPage.tsx
useEffect(() => {
  dispatch(fetchDiets());
  return () => { dispatch(resetDiets()); };
}, []);
```

## Contrato obligatorio de estado por modulo (CRUD)

Cada modulo CRUD MUST exponer estos campos en su state:

| Campo | Tipo | Proposito |
|---|---|---|
| `list` | `EntidadDto[]` | Datos del listado |
| `filters` | objeto tipado | Estado de filtros aplicados (busqueda, incluir borrados, etc.) |
| `form` | `UpsertEntidadRequest` | Datos del formulario (crear / editar) |
| `error` | `string \| null` | Error de la ultima operacion |
| `loading` | `boolean` | Estado de carga (skeleton / spinner) |
| `popUpCode` | `string \| null` | Codigo del modal activo (ver valores estandar abajo) |
| `pagination` | `PaginationState` | Estado de paginacion para `DataTable` |
| `sort` | `SortState` | Estado de ordenacion para `DataTable` |

Tipos compartidos para `pagination` y `sort`:

```ts
// Definir en interfaces/common/ y reutilizar en todos los modulos
export type PaginationState = {
  page: number;          // 0-indexed
  rowsPerPage: number;   // default: 10
  total: number;
};

export type SortState = {
  field: string;         // identificador de columna definido por el modulo
  direction: 'asc' | 'desc';
};
```

## Que pertenece a Redux y que no

Esta regla elimina la ambiguedad sobre donde gestionar el estado en un modulo CRUD:

| Estado | Donde va | Por que |
|---|---|---|
| Datos del listado | Redux `list` | Compartido entre tabla, filtros y la pagina |
| Valores del formulario (crear y editar) | Redux `form` | La pagina y el dialog lo comparten; debe persistir entre renders |
| Valores de los filtros | Redux `filters` | La pagina los lee para llamar a la API; deben sobrevivir al re-render |
| Paginacion y ordenacion | Redux `pagination` / `sort` | La pagina los pasa a `DataTable` y los usa en la llamada a la API |
| Que modal esta abierto | Redux `popUpCode` | La pagina decide que dialog renderizar segun este valor |
| Estado de un input controlado durante la escritura | `useState` local | UI pura, no necesita salir del componente |
| Apertura de un tooltip, drawer secundario o menu | `useState` local | Estado efimero de presentacion |

**Regla de oro**: si el valor afecta a la llamada a la API, al renderizado de otro componente o a la apertura de un dialog, va en Redux. Si solo afecta al propio componente y es efimero, puede ser `useState`.

### Flujo de crear

```tsx
// 1. Usuario pulsa "Crear" en la pagina
dispatch(setDietsForm(initialDietForm));   // limpia el formulario
dispatch(setDietsPopUpCode('create'));      // abre el dialog

// 2. El dialog lee el formulario de Redux
const { form, popUpCode } = useSelector... // solo en DietsPage, se pasa por props

// 3. Cada cambio de campo dispatcha la accion correspondiente
dispatch(setDietsForm({ ...form, name: e.target.value }));

// 4. Al guardar
dispatch(createDiet(form));                // thunk: llama API y refresca list
dispatch(setDietsPopUpCode(null));         // cierra el dialog
```

### Flujo de editar

```tsx
// 1. Usuario pulsa "Editar" en una fila
dispatch(setDietsForm(selectedDiet));      // carga los datos del item en el formulario
dispatch(setDietsPopUpCode('edit'));        // abre el dialog de edicion

// 2. Mismo flujo que crear — el dialog recibe form por props

// 3. Al guardar
dispatch(updateDiet(form));               // thunk: llama API y refresca list
dispatch(setDietsPopUpCode(null));        // cierra el dialog
```

### Flujo de filtros

```tsx
// Cada cambio en la barra de filtros
dispatch(setDietsFilters({ ...filters, search: value }));
dispatch(setDietsPagination({ ...pagination, page: 0 })); // reset a pagina 0

// La pagina reacciona al cambio de filtros y relanza la carga
useEffect(() => {
  dispatch(fetchDiets());
}, [filters, pagination.page, pagination.rowsPerPage, sort]);
```

## Valores estandar de `popUpCode`

MUST usarse estos valores y ninguno otro sin justificacion documentada:

| Valor | Cuando usarlo |
|---|---|
| `null` | Ningun modal abierto |
| `'create'` | Dialog de crear nueva entidad |
| `'edit'` | Dialog de editar entidad existente |
| `'delete'` | Dialog de confirmacion de borrado |
| `'view'` | Dialog de vista detalle (solo lectura) |

Uso en la UI:

```ts
dispatch(setDietsPopUpCode('create'));         // abrir
dispatch(setDietsPopUpCode(null));             // cerrar
open={popUpCode === 'create'}                  // controlar apertura del dialog
```

## Selectores

- El selector base del modulo MUST definirse en el state file:
  ```ts
  export const selectDietsState = (state: RootState) => state.diets;
  ```
- Los selectores derivados (con logica de filtrado, mapeo, etc.) MUST usarse `createSelector`:
  ```ts
  export const selectFilteredDiets = createSelector(
    [selectDietsState],
    ({ list, filters }) => list.filter(/* logica */),
  );
  ```
- Los selectores MUST definirse en el state file del modulo, no en los componentes.

## Tipado en Redux

- Redux MUST reutilizar las interfaces de dominio del modulo (definidas en `interfaces/`).
- MUST NOT duplicarse interfaces de entidad dentro de `states/` si ya existen en `interfaces/`.
- El state Redux envuelve esas interfaces con metadatos de UI (`loading`, `error`, `popUpCode`, `pagination`, `sort`).
- El uso de `any` MUST evitarse; si es imprescindible, MUST justificarse por escrito.

## Debugging con Redux DevTools

RTK habilita Redux DevTools automaticamente en desarrollo. Permite inspeccionar el historial de acciones y el estado del store en cualquier momento sin añadir configuracion extra.

**Flujo de depuracion ante un bug**:
1. Abrir DevTools del navegador → pestaña **Redux**.
2. Localizar la accion donde el estado cambio de forma inesperada.
3. Comparar el state **antes** y **despues** de esa accion (diff integrado en DevTools).
4. Remontar al thunk o componente que la dispatcho.

**Señales de problema habituales y su causa**:

| Sintoma en el historial | Causa probable |
|---|---|
| `setLoading(false)` antes de que lleguen los datos | El `finally` se ejecuto antes del `try` (error de flujo en el thunk) |
| `setError` recibe un objeto, no un string | `getErrorMessage` no extrae el mensaje correctamente |
| El state no se limpia al volver a una pagina | Falta `resetModulo` en el `return` del `useEffect` de la pagina |
| Una accion de otro modulo cambia el state actual | El reducer escucha acciones de otro modulo (revisar los `addCase`) |
| Multiples `setLoading(true)` seguidos sin `false` | Thunks concurrentes sin control; revisar si se cancela el anterior |

**Regla de naming para DevTools**: los type strings descriptivos (`'diets/setList'`, `'diets/setLoading'`) permiten leer el flujo como un log de eventos cronologico. MUST NOT nombrarse acciones con type strings vagos (`'SET'`, `'UPDATE'`, `'ACTION_1'`).
