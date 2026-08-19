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

Cada operacion asincrona MUST usar `createAsyncThunk` con `rejectWithValue` y delegar el ciclo de vida (`loading`, `error`) al **reducer** via los casos `.pending / .fulfilled / .rejected`. MUST NOT despacharse `setLoading` ni `setError` manualmente dentro del thunk.

```ts
// Tipo de configuracion del thunk — definir una vez por modulo
type AdminMusclesThunkConfig = {
  state: RootState;
  rejectValue: string;
};

// Thunk de listado — lee filtros y tabla desde el estado Redux
export const fetchAdminMuscles = createAsyncThunk<IMuscles, void, AdminMusclesThunkConfig>(
  'muscles/fetch',
  async (_arg, { getState, rejectWithValue }) => {
    const { filters, table } = getState().adminMuscles;
    try {
      return await listMusclesPage({
        includeDeleted: filters.includeDeleted,
        search: filters.search || undefined,
        sortBy: table.sortBy,
        sortDirection: table.sortDirection,
        page: table.page + 1,      // API es 1-indexed
        pageSize: table.rowsPerPage,
      });
    } catch (error) {
      return rejectWithValue(toErrorMessage(error, 'No se pudo cargar el listado.'));
    }
  },
);

// Thunk de submit de formulario — lee form desde el estado Redux
export const submitAdminMuscleForm = createAsyncThunk<void, void, AdminMusclesThunkConfig>(
  'muscles/submitForm',
  async (_arg, { getState, rejectWithValue }) => {
    const { form } = getState().adminMuscles;
    try {
      if (form.id) {
        await updateMuscle(form.id, buildRequest(form));
      } else {
        await createMuscleApi(buildRequest(form));
      }
    } catch (error) {
      return rejectWithValue(toErrorMessage(error, 'No se pudo guardar.'));
    }
  },
);

// Thunk con argumento explicito (ej: borrado por ids)
export const deleteAdminMuscles = createAsyncThunk<
  { failedCount: number; total: number },
  string[],
  AdminMusclesThunkConfig
>(
  'muscles/delete',
  async (ids, { rejectWithValue }) => { ... },
);
```

El reducer MUST manejar los tres casos de cada thunk:

```ts
// adminMusclesReducer.ts
builder
  .addCase(fetchAdminMuscles.pending, (state) => {
    state.loading = true;
    state.error = null;
  })
  .addCase(fetchAdminMuscles.fulfilled, (state, action) => {
    state.loading = false;
    state.table.list = action.payload.items;
    state.table.totalCount = action.payload.totalCount;
    // page viene 1-indexed desde la API — convertir a 0-indexed para MUI
    state.table.page = action.payload.page - 1;
  })
  .addCase(fetchAdminMuscles.rejected, (state, action) => {
    state.loading = false;
    state.error = action.payload ?? 'Error desconocido';
  });
```

Reglas:
- MUST NOT despacharse `setLoading` ni `setError` dentro del thunk — el reducer los gestiona via `.pending/.rejected`.
- El thunk de fetch MUST leer `filters` y `table` desde `getState()`, NO recibirlos como argumento.
- MUST NOT hacerse llamadas a la API directamente en un reducer.
- La funcion `toErrorMessage` MUST definirse en `utils/` para extraer el mensaje de cualquier tipo de error; MUST NOT duplicarse esta logica en cada thunk.

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
| `table` | objeto tipado | Agrupa TODOS los datos de la tabla: lista, paginacion, ordenacion y seleccion |
| `filters` | objeto tipado | Estado de filtros aplicados (busqueda, incluir borrados, etc.) |
| `form` | `Entidad` (interfaz de dominio) | Datos del formulario — MUST usarse la interfaz de dominio (con `id?`) para cubrir crear y editar sin duplicar tipos |
| `error` | `string \| null` | Error de la ultima operacion |
| `loading` | `boolean` | Estado de carga (skeleton / spinner) |
| `popUpCode` | `string \| null` | Codigo del modal activo (ver valores estandar abajo) |

Estructura obligatoria del campo `table`:

```ts
export type <Modulo>Table = {
  list: Entidad[];          // items de la pagina actual
  page: number;             // 0-indexed
  rowsPerPage: number;      // default: 10
  totalCount: number;       // total en base de datos (para paginacion)
  sortBy: 'campo1' | 'campo2' | 'campo3';  // columnas permitidas del modulo
  sortDirection: 'asc' | 'desc';
  selectedIds: string[];    // ids de filas seleccionadas (borrado multiple, etc.)
};
```

Estructura obligatoria del campo `filters`:

```ts
export type <Modulo>Filters = {
  search: string;           // busqueda de texto libre
  // campos adicionales especificos del modulo:
  includeDeleted: boolean;  // si el modulo tiene soft-delete
  // otros filtros especificos...
};
```

La interfaz de dominio se usa DIRECTAMENTE como tipo de `form`:

```ts
// CORRECTO — reutiliza la interfaz de dominio; id? cubre crear (undefined) y editar (string)
form: IMuscle
// form: { id: undefined, name: '', code: '', ... } → crear
// form: { id: '123', name: 'Bicep', code: 'BIC', ... } → editar

// INCORRECTO — duplica campos que ya existen en IMuscle
form: { name: string; code: string; description: string; active: boolean }
```

Ejemplo completo de estado de un modulo CRUD:

```ts
import type { IMuscle, IImportMusclesResult } from '../../../interfaces/muscles/IMuscles';

export type AdminMusclesTable = {
  list: IMuscle[];
  page: number;
  rowsPerPage: number;
  totalCount: number;
  sortBy: 'code' | 'name' | 'description';
  sortDirection: 'asc' | 'desc';
  selectedIds: string[];
};

export type AdminMusclesFilters = {
  search: string;
  code: string;
  name: string;
  includeDeleted: boolean;
};

export type AdminMusclesState = {
  table: AdminMusclesTable;
  filters: AdminMusclesFilters;
  form: IMuscle;
  csvResult: IImportMusclesResult | null;  // opcional: solo si el modulo soporta importacion
  error: string | null;
  loading: boolean;
  popUpCode: string | null;
};

export const adminMusclesInitialState: AdminMusclesState = {
  table: { list: [], page: 0, rowsPerPage: 10, totalCount: 0, sortBy: 'name', sortDirection: 'asc', selectedIds: [] },
  filters: { search: '', code: '', name: '', includeDeleted: false },
  form: { id: undefined, name: '', code: '', description: null, active: true, muscleGroupIds: [], isDeleted: false, deletedAt: null },
  csvResult: null,
  error: null,
  loading: false,
  popUpCode: null,
};
```

## Que pertenece a Redux y que no

Esta regla elimina la ambiguedad sobre donde gestionar el estado en un modulo CRUD:

| Estado | Donde va | Por que |
|---|---|---|
| Datos del listado | Redux `table.list` | Compartido entre tabla, filtros y la pagina |
| Paginacion y ordenacion | Redux `table.page / table.rowsPerPage / table.sortBy / table.sortDirection` | La pagina los pasa a `DataTable` y los usa en la llamada a la API |
| Ids seleccionados (borrado multiple) | Redux `table.selectedIds` | La pagina los necesita para el boton de borrar seleccion |
| Valores del formulario (crear y editar) | Redux `form` | La pagina y el dialog lo comparten; debe persistir entre renders |
| Valores de los filtros | Redux `filters` | La pagina los lee para llamar a la API; deben sobrevivir al re-render |
| Que modal esta abierto | Redux `popUpCode` | La pagina decide que dialog renderizar segun este valor |
| Estado de un input controlado durante la escritura | `useState` local | UI pura, no necesita salir del componente |
| Apertura de un tooltip, drawer secundario o menu | `useState` local | Estado efimero de presentacion |

**Regla de oro**: si el valor afecta a la llamada a la API, al renderizado de otro componente o a la apertura de un dialog, va en Redux. Si solo afecta al propio componente y es efimero, puede ser `useState`.

### Flujo de crear

```tsx
// 1. Usuario pulsa "Crear" en la pagina
dispatch(setAdminMusclesForm(adminMusclesInitialState.form));  // limpia el formulario
dispatch(setAdminMusclesPopUpCode('CREATE'));                   // abre el dialog

// 2. El dialog recibe formState por props (mapeado desde form de Redux)
// formState={{ id: form.id ?? null, name: form.name, code: form.code, ... }}

// 3. Cada cambio de campo dispatcha solo los campos necesarios
dispatch(setAdminMusclesForm({ ...form, name: patch.name ?? form.name }));

// 4. Al guardar
void dispatch(submitAdminMuscleForm());    // thunk: llama API (create o update segun form.id)
dispatch(setAdminMusclesPopUpCode(null));  // cierra el dialog
void dispatch(fetchAdminMuscles());        // refresca el listado
```

### Flujo de editar

```tsx
// 1. Usuario pulsa "Editar" en una fila
dispatch(setAdminMusclesForm(selectedRow));   // carga los datos del item en el formulario
dispatch(setAdminMusclesPopUpCode('EDIT'));   // abre el dialog de edicion

// 2. Mismo flujo que crear — el dialog recibe formState por props

// 3. Al guardar — submitAdminMuscleForm detecta form.id para saber si crear o editar
void dispatch(submitAdminMuscleForm());
dispatch(setAdminMusclesPopUpCode(null));
void dispatch(fetchAdminMuscles());
```

### Flujo de filtros

Los cambios de filtros se acumulan en Redux SIN relanzar la carga. La carga ONLY se dispara al pulsar el boton "Aplicar".

```tsx
// Cada cambio de un campo de filtro — solo actualiza Redux, NO hace fetch
dispatch(setAdminMusclesFilters({ ...filters, search: value }));
dispatch(setAdminMusclesFilters({ ...filters, includeDeleted: checked }));

// Boton "Aplicar" del drawer de filtros
// 1. Resetea la pagina a 0 ANTES del fetch
dispatch(setAdminMusclesTable({ ...table, page: 0 }));
// 2. Cierra el drawer
dispatch(setAdminMusclesPopUpCode(null));
// 3. Dispara el fetch — el thunk lee filters y table actualizados desde getState()
void dispatch(fetchAdminMuscles());
```

MUST NOT usarse `useEffect` que observe `filters` para disparar el fetch automaticamente — provoca fetches no deseados durante la escritura y dificulta el control del flujo.

## Valores estandar de `popUpCode`

MUST usarse estos valores en MAYUSCULAS y ninguno otro sin justificacion documentada:

| Valor | Cuando usarlo |
|---|---|
| `null` | Ningun modal abierto |
| `'CREATE'` | Dialog de crear nueva entidad |
| `'EDIT'` | Dialog de editar entidad existente |
| `'DELETE'` | Dialog de confirmacion de borrado |
| `'VIEW'` | Dialog de vista detalle (solo lectura) |
| `'FILTERS'` | Drawer/dialog de filtros |
| `'CSV_IMPORT'` | Dialog de importacion CSV (solo si el modulo lo soporta) |

Uso en la UI:

```ts
dispatch(setAdminMusclesPopUpCode('CREATE'));   // abrir
dispatch(setAdminMusclesPopUpCode(null));        // cerrar
open={popUpCode === 'CREATE'}                   // controlar apertura del dialog

// Apertura de filtros desde el boton del header
dispatch(setAdminMusclesPopUpCode('FILTERS'));
// Cierre al aplicar filtros
dispatch(setAdminMusclesPopUpCode(null));
```

**Nota importante**: al abrir un dialog de crear o editar, MUST limpiarse o cargarse el formulario ANTES de cambiar `popUpCode`:

```ts
// Crear
dispatch(setAdminMusclesForm(adminMusclesInitialState.form));
dispatch(setAdminMusclesPopUpCode('CREATE'));

// Editar — cargar el item seleccionado en el form
dispatch(setAdminMusclesForm(selectedRow));
dispatch(setAdminMusclesPopUpCode('EDIT'));
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
