---
name: frontend-components
description: 'Estandares de componentes React para Gym Tracker. Usar cuando: se crea o modifica un componente (.tsx), se diseñan componentes UI base (PopupDialog, DataTable), se añaden o revisan interfaces de props, se aplican reglas de reutilizacion, se decide si usar estado local o Redux en un componente. Contiene reglas de MUI, estructura de carpetas, tipado obligatorio, componentes base existentes y sus contratos de props.'
---

# Frontend — Estandares de Componentes

> Para saber donde colocar un componente en la estructura de carpetas, ver skill `frontend-architecture`.

## Tecnologia obligatoria

- Todos los componentes MUST construirse sobre **Material UI (MUI)**.
- MUST NOT introducirse otra libreria de componentes (Tailwind, Chakra, etc.).
- MUST importarse cada componente MUI desde su subpath individual: `import Button from '@mui/material/Button'`, NEVER desde `@mui/material` barrel.
- MUST NOT instalarse ni usarse paquetes de iconos externos (`lucide-react`, `font-awesome`, `heroicons`, `react-icons`, `phosphor-react`, etc.). MUST usarse EXCLUSIVAMENTE `@mui/icons-material` para todos los iconos. STOP si ves un import de iconos desde un paquete distinto a `@mui/icons-material` — eliminalo y sustituyelo por el equivalente de MUI.

## Estilos y tema

El tema MUI (`theme/`) es la **unica fuente de verdad** para todos los tokens de diseño. Esto permite cambiar cualquier valor (espaciado, color, tipografia, variante de componente) en un unico lugar sin tocar ningun componente.

**Reglas obligatorias:**

- Colores MUST referenciarse desde el tema: `color: 'primary.main'`, `bgcolor: 'background.paper'`. MUST NOT usarse valores hexadecimales o nombres de color directos (`'#1976d2'`, `'red'`).
- Espaciado (gap, padding, margin) MUST usarse en unidades del sistema MUI (multiplos de 8px): `gap={2}`, `p={3}`, `mt={1}`. MUST NOT usarse valores en `px` directos (`gap: '16px'`).
- Tipografia MUST usarse mediante la prop `variant` ligada al tema: `<Typography variant="h6">`. MUST NOT definirse `fontSize`, `fontWeight` o `fontFamily` directamente en un componente.
- Variantes y overrides de componentes MUI (botones, inputs, cards, etc.) MUST definirse en `theme/` bajo la clave `components`. MUST NOT repetirse el mismo override en multiples componentes.
- La prop `sx` ONLY debe usarse para ajustes de layout **especificos de ese contexto** (ej: margen puntual, ancho maximo de un elemento concreto). MUST NOT usarse `sx` para redefinir tokens de diseño que ya estan o deberian estar en el tema.

**Ejemplo correcto** — el gap entre botones viene del tema:

```tsx
// theme/theme.ts — fuente de verdad
components: {
  MuiDialogActions: {
    styleOverrides: {
      root: { gap: 16 },  // cambialo aqui una vez, aplica en todos los dialogos
    },
  },
}

// Componente — no hay ningun valor hardcoded
<DialogActions>
  <Button onClick={onClose}>{closeLabel}</Button>
  <Button type="submit">{saveLabel}</Button>
</DialogActions>
```

**Ejemplo incorrecto** (valor repetido en cada componente):

```tsx
// MuscleFormDialog.tsx
<DialogActions sx={{ gap: '20px' }}>...</DialogActions>

// RoutineFormDialog.tsx
<DialogActions sx={{ gap: '20px' }}>...</DialogActions>  // duplicado
```

## Estructura de carpetas

Cada componente MUST organizarse en su propia subcarpeta, tanto en `components/` como en `pages/<dominio>/components/`:

```
components/
  DataTable/
    DataTable.tsx
  PopupDialog/
    PopupDialog.tsx

pages/<dominio>/components/
  <Componente>/
    <Componente>.tsx
```

## Reutilizacion obligatoria

MUST verificarse si existe un componente base que cumpla el requerimiento antes de crear uno nuevo. Los componentes base existentes son:

| Componente | Ubicacion | Proposito |
|---|---|---|
| `PopupDialog` | `components/PopupDialog/` | Modal con titulo, contenido inyectable y acciones |
| `DataTable` | `components/DataTable/` | Tabla con paginacion y ordenacion, cabecera/cuerpo inyectables |
| `FeedbackMessage` | `components/FeedbackMessage/` | Mensaje de estado para error o lista vacia |

MUST NOT duplicarse ni envolverse un componente base para añadir variaciones. La personalizacion MUST resolverse por props.

### Criterio para promover un componente a global

Antes de crear un componente, evaluar su alcance:

- Si la **UI + logica** aparece o puede aparecer en mas de un dominio → va en `components/` (global).
- Si solo tiene sentido dentro de un dominio concreto → va en `pages/<dominio>/components/`.

**Ejemplo 1 — CopyButton**: un boton "copiar al portapapeles" tiene su propio estilo y su propia logica (`navigator.clipboard.writeText`). Si se necesita en varios dominios, MUST crearse como `components/CopyButton/CopyButton.tsx` — no duplicar la logica en cada dominio que lo use.

**Ejemplo 2 — FeedbackMessage**: mostrar un mensaje de error o de lista vacia es un patron que se repite en todas las paginas con listado. Ademas, el componente tiene necesidades propias de estilo (color, icono, tipografia) que pueden cambiar. Si estuviera duplicado en cada dominio, cualquier cambio de estilo requeriria editar todos los dominios. Al estar en `components/FeedbackMessage/`, el cambio se hace una vez y aplica en todo el proyecto.

**Señales de que algo debe subir a `components/`**:
- La misma logica o el mismo bloque JSX se copia en dos o mas dominios.
- Un cambio de comportamiento o estilo requeriria editar multiples archivos.
- La funcion no depende de ninguna entidad de negocio concreta.

## Patrones de layout de pagina

Toda pagina MUST seguir uno de estos dos layouts segun su dispositivo objetivo. La eleccion del layout MUST ser consistente: MUST NOT mezclarse elementos de un layout en el otro (ej: FAB en una pagina de tabla, o botones de accion a la izquierda).

### Layout Tabla — desktop / administracion

Para paginas cuyo uso principal es en escritorio, con `DataTable` como elemento central.

```
┌─────────────────────────────────────────────────────────┐
│  Titulo de la pagina            [⚡ Filtros]            │  ← titulo izq, filtros der
│                                                         │
│              [Accion secundaria]  [Accion secundaria]  [Añadir]  │  ← TOP-RIGHT
│  ┌──────────────────────────────────────────────────────│
│  │   DataTable (cabecera + filas + paginacion)          │
│  └──────────────────────────────────────────────────────│
└─────────────────────────────────────────────────────────┘
```

Reglas de posicion obligatorias:
- **Titulo**: top-left, `<Typography variant="h4">`.
- **Boton de filtros**: `<Button variant="outlined" startIcon={<FilterListIcon />}>` con el texto `t('administration.common.filters')`, en la **misma fila que el titulo**, alineado a la derecha (`justifyContent="space-between"` en el Stack del titulo). MUST NOT colocarse en una fila separada debajo de los botones de accion.
- **Botones de accion** (Añadir, Importar, Recargar, Borrar seleccion): MUST estar en el lado **DERECHO**, en una segunda fila debajo del titulo. MUST NOT colocarse en el lado izquierdo.- **Orden de los botones** (de izquierda a derecha dentro del grupo, el mas a la derecha es el primario):
  1. **Borrar seleccion** — `Button variant="outlined" color="error"` — MUST mostrarse **solo si** hay items seleccionados; MUST NOT renderizarse cuando no hay seleccion.
  2. **Recargar** — `Button variant="outlined"`.
  3. **Importar** — `Button variant="outlined"` — MUST mostrarse **solo si** el modulo soporta importacion.
  4. **Crear nuevo** — `Button variant="contained"`, el mas a la derecha, siempre visible.
- **Boton de filtros**: `<Button variant="outlined" startIcon={<FilterListIcon />}>` con el texto `t('administration.common.filters')`, alineado a la derecha, **en la misma fila que el titulo**. MUST NOT usarse solo el icono sin texto en este layout.
- MUST NOT usarse `Fab` en este layout.

```tsx
// Estructura JSX obligatoria del header de una pagina tabla
<Stack direction="row" justifyContent="space-between" alignItems="center">
  <Typography variant="h4">{t('muscles.title')}</Typography>
  <Button variant="outlined" startIcon={<FilterListIcon />} onClick={handleOpenFilters}>
    {t('administration.common.filters')}
  </Button>
</Stack>

<Stack direction="row" justifyContent="flex-end" gap={1}>
  {selectedIds.length > 0 && (
    <Button variant="outlined" color="error" onClick={handleDeleteSelected}>
      {t('common.actions.deleteSelected')}
    </Button>
  )}
  <Button variant="outlined" onClick={handleReload}>{t('common.actions.reload')}</Button>
  {/* STOP: este boton SOLO se incluye si el spec.md del modulo especifica capacidad de importacion.
      Su presencia MUST controlarse con un booleano declarado como prop de la pagina (ej: supportsImport: boolean),
      cuyo valor se establece segun el spec. NEVER inventes este booleano — si el spec no lo menciona, omite el boton. */}
  {supportsImport && (
    <Button variant="outlined" onClick={handleImport}>{t('common.actions.import')}</Button>
  )}
  <Button variant="contained" onClick={handleCreate}>{t('common.actions.create')}</Button>
</Stack>

<DataTable ... />
```

### Estados de carga, error y vacío en Layout Tabla

Los tres estados intermedios (cargando, error, sin datos) MUST seguir estas reglas:

- **Cargando**: MUST mostrarse `<CircularProgress>` centrado **sin** `<Paper>` envolvente. MUST NOT usarse `<Paper>` como contenedor del spinner — provoca un efecto de encogimiento visual al pasar al estado tabla. El spinner MUST centrarse tanto horizontal como verticalmente usando `flex: 1` para ocupar el espacio disponible.
  ```tsx
  {loading ? (
    <Stack alignItems="center" justifyContent="center" sx={{ flex: 1 }}>
      <CircularProgress aria-label={t('administration.common.loading')} />
    </Stack>
  ) : null}
  ```
- **Error**: MUST usarse `<FeedbackMessage type="error" message={error} />`. MUST NOT mostrarse el `<Paper>` de la tabla en este estado.
- **Sin datos**: MUST usarse `<FeedbackMessage type="empty" message={t('common.messages.noResults')} />`. MUST NOT envolverse con `<Paper>`.
- **Con datos**: MUST renderizarse el componente de tabla directamente, sin `<Paper>` adicional (el `DataTable` base ya gestiona su propio surface).
- MUST NOT usarse `<Paper>` en ningun estado que no sea la tabla de datos en si.



Para paginas cuyo uso principal es en movil, con tarjetas o lista como elemento central.

```
┌─────────────────────────────────────────────────┐
│  Titulo de la pagina            [⚡ Filtros]    │  ← titulo + filtros en misma fila
│                                                 │
│  ┌─────────────────────────────────────────────┐│
│  │  Card / ListItem                            ││
│  └─────────────────────────────────────────────┘│
│                                                 │
│                                             [+] │  ← Fab FIXED bottom-right
└─────────────────────────────────────────────────┘
```

Reglas de posicion obligatorias:
- **Titulo y boton de filtros**: MUST colocarse en la misma fila (`Stack direction="row" justifyContent="space-between"`).
- **Boton de filtros**: `<Button variant="outlined" startIcon={<FilterListIcon />}>` con texto `t('common.actions.filter')`, alineado a la derecha. MUST usar `FilterListIcon` importado de `@mui/icons-material/FilterList`.
- **Accion primaria** (Crear): MUST usarse `<Fab>` fijo en la esquina inferior derecha con `position: 'fixed'`. MUST NOT usarse `Button variant="contained"` para la accion de crear en este layout.
- **Acciones secundarias** (Recargar, Importar): MUST NOT incluirse como botones visibles en este layout — son exclusivas del Layout Tabla.
- MUST NOT colocarse el `Fab` en otro lugar que no sea `position: 'fixed'`, `bottom`, `right`.
- STOP si ves un `Fab` en una pagina de Layout Tabla, o un `Button variant="contained"` para crear en una pagina de Layout Lista — es una mezcla de layouts prohibida.

```tsx
// Estructura JSX obligatoria — Layout Lista
import AddIcon from '@mui/icons-material/Add';
import FilterListIcon from '@mui/icons-material/FilterList';

<Stack direction="row" justifyContent="space-between" alignItems="center">
  <Typography variant="h4">{t('routines.title')}</Typography>
  <Button variant="outlined" startIcon={<FilterListIcon />} onClick={handleOpenFilters}>
    {t('common.actions.filter')}
  </Button>
</Stack>

{/* lista de tarjetas o ListItems */}

<Fab
  color="primary"
  sx={{ position: 'fixed', bottom: 3, right: 3 }}
  onClick={handleCreate}
  aria-label={t('common.actions.create')}
>
  <AddIcon />
</Fab>
```

### Estandar del boton de filtros

El boton de filtros MUST usarse siempre con exactamente este patron en todas las paginas, independientemente del layout:

```tsx
<Button variant="outlined" startIcon={<FilterListIcon />} onClick={handleOpenFilters}>
  {t('common.actions.filter')}
</Button>
```

- MUST NOT usarse solo el icono sin texto (`<IconButton>`) para abrir filtros.
- MUST NOT crearse un componente wrapper para este boton — la estandarizacion esta en el patron de uso y en el tema MUI.
- El estilo (color, tamaño, borde) MUST venir del tema. MUST NOT añadirse `sx` para redefinir el aspecto del boton.

## Tipado obligatorio con interfaces

- Todas las props de componentes MUST tiparse con `interface` explicita (no `type` inline anonimo).
- Las interfaces reutilizables entre modulos MUST almacenarse en `interfaces/` organizadas por dominio funcional.
- Las interfaces locales de un unico componente pueden declararse en el mismo archivo o en un `.ts` hermano.
- El uso de `any` MUST evitarse; si es imprescindible, MUST justificarse por escrito en el codigo.

## Contratos de componentes base existentes

### PopupDialog

Props:

```ts
type PopupDialogProps = {
  open: boolean;
  title: string;
  onClose: () => void;
  children: ReactNode;
  onSubmit?: () => void;         // usar si NO hay formId
  formId?: string;               // usar para submit nativo de <form>
  saveLabel?: string;            // default: 'Guardar'
  closeLabel?: string;           // default: 'Cerrar'
  disableSave?: boolean;
  isSaving?: boolean;
  maxWidth?: Breakpoint;         // default: 'sm'
};
```

**Patron con formulario nativo** (MUST usar cuando el dialogo contiene un `<form>`):

```tsx
const FORM_ID = 'my-feature-form';

<PopupDialog open={open} title="Titulo" onClose={onClose} formId={FORM_ID} saveLabel="Guardar" closeLabel="Cancelar">
  <Stack component="form" id={FORM_ID} onSubmit={handleSubmit}>
    {/* campos */}
  </Stack>
</PopupDialog>
```

**Patron sin formulario nativo** (para confirmaciones o dialogs sin `<form>`):

```tsx
<PopupDialog open={open} title="Titulo" onClose={onClose} onSubmit={handleConfirm}>
  <Typography>¿Confirmas la accion?</Typography>
</PopupDialog>
```

MUST NOT usar `onSubmit` y `formId` al mismo tiempo. Si hay `formId`, el boton Guardar hace `type="submit"` sobre el formulario; `onSubmit` se ignora.

> **STOP — contrato PopupDialog cerrado**: MUST NOT añadirse props fuera de este contrato (`footer`, `subtitle`, `extraActions`, `headerColor`, etc.). Si necesitas una variacion visual, resuelvela mediante el tema MUI o las props existentes. NEVER extiendas la interfaz de PopupDialog sin modificar el componente real.

### DataTable

Unico componente de tabla del proyecto. MUST usarse para toda tabla de datos. Cabecera y cuerpo son `ReactNode` inyectables. Paginacion y ordenacion son **obligatorias** — el estado de ambas lo gestiona el componente padre (Redux o local).

```ts
type DataTableProps = {
  ariaLabel: string;
  columnWidths: string[];
  head: ReactNode;
  body: ReactNode;
  pagination: {
    count: number;
    page: number;
    rowsPerPage: number;
    onPageChange: (_event: unknown, newPage: number) => void;
    onRowsPerPageChange: (event: ChangeEvent<HTMLInputElement>) => void;
    rowsPerPageOptions?: number[];
  };
  sort: {
    field: string;         // identificador de columna definido por el modulo (ej: 'name', 'createdAt')
    direction: 'asc' | 'desc';
    onSort: (field: string) => void;
  };
  maxHeight?: string | number | Record<string, string | number>;
  size?: 'small' | 'medium';
};
```

Ejemplo de uso:

```tsx
<DataTable
  ariaLabel="tabla de musculos"
  columnWidths={['40%', '20%', '30%', '10%']}
  head={<TableRow>...</TableRow>}
  body={rows.map(row => <TableRow key={row.id}>...</TableRow>)}
  pagination={{ count: total, page, rowsPerPage, onPageChange, onRowsPerPageChange }}
  sort={{ field: sortField, direction: sortDir, onSort: handleSort }}
/>
```

## Estados de carga, error y vacio

> **STOP — contrato DataTable cerrado**: MUST NOT pasarse a `DataTable` ninguna prop fuera de este contrato (`columns`, `data`, `rows`, `headers`, `onRowClick`, etc.). Si el archivo `components/DataTable/DataTable.tsx` no expone todavia las props `pagination` y `sort`, STOP — no uses el componente hasta que su implementacion coincida con este contrato. Reporta la discrepancia al usuario.

Cada pagina con listado MUST gestionar los tres estados. Patrones obligatorios:

- **Carga** (`loading === true`): usar `<CircularProgress>` centrado o `<Skeleton>` para sustituir el contenido.
- **Error** o **lista vacia**: MUST usarse `<FeedbackMessage>` — MUST NOT escribirse `<Alert>` o `<Typography>` sueltos para estos casos.

```tsx
{loading && <CircularProgress />}
{!loading && error && <FeedbackMessage type="error" message={error} />}
{!loading && !error && list.length === 0 && <FeedbackMessage type="empty" message={t('common.messages.noResults')} />}
{!loading && !error && list.length > 0 && <DietsTable rows={list} />}
```

### FeedbackMessage

Componente unificado para estados de feedback no relacionados con carga. Centraliza el estilo y evita duplicar `<Alert>` y `<Typography>` para los mismos casos en cada pagina.

```ts
type FeedbackMessageProps = {
  type: 'error' | 'empty';
  message: string;
};
```

- `type="error"`: renderiza con estilo de alerta de error (color rojo, icono de error).
- `type="empty"`: renderiza con estilo informativo neutro (color secundario, icono de lista vacia).

El estilo concreto de cada tipo MUST venir del tema. MUST NOT hardcodearse colores ni tamaños dentro del componente.

> **STOP — contrato FeedbackMessage cerrado**: MUST NOT añadirse props como `title`, `icon`, `action`, `variant`, `children` que no estan en este contrato. La personalizacion visual MUST hacerse en el tema MUI, no como nuevas props del componente.

## Patron de formState en dialogs de formulario

Los dialogs de formulario (sufijo `Dialog`) MUST recibir los valores del formulario mediante una prop `formState` tipada, mapeada desde la interfaz de dominio almacenada en Redux. MUST NOT leer Redux directamente en el dialog.

```ts
// Tipo de la prop formState del dialog \u2014 solo los campos que el dialog necesita mostrar/editar
interface MuscleFormState {
  id: string | null;        // null = crear, string = editar
  name: string;
  code: string;
  description: string;      // string vacio en vez de null/undefined para inputs controlados
}

interface MuscleFormDialogProps {\n  open: boolean;\n  formState: MuscleFormState;\n  onChange: (patch: Partial<MuscleFormState>) => void;\n  onClose: () => void;\n  onSubmit: () => void;\n  isSaving?: boolean;\n  error?: string | null;\n}
```\n\nLa pagina mapea la interfaz de dominio (`IMuscle`) a `MuscleFormState` antes de pasarla:\n\n```tsx\n// MusclesPanel.tsx \u2014 mapeo de IMuscle (Redux) a MuscleFormState (prop del dialog)\n<MuscleFormDialog\n  formState={{ id: form.id ?? null, name: form.name, code: form.code, description: form.description ?? '' }}\n  onChange={(patch) => dispatch(setAdminMusclesForm({\n    ...form,\n    name: patch.name ?? form.name,\n    code: patch.code ?? form.code,\n    description: patch.description ?? form.description,\n  }))}\n  ...\n/>\n```\n\nReglas:\n- El tipo `formState` MUST usar `string` para todos los inputs de texto (no `string | null`) para evitar inputs no controlados.\n- El mapeo `null/undefined \u2192 ''` MUST hacerse en la pagina al pasar la prop, no en el dialog.\n- El `onChange` MUST mapear los campos del patch de vuelta a la interfaz de dominio al despachar a Redux.\n\n## Estado local vs Redux en componentes\n\n- **Estado local** (`useState`): usar para estado de UI puro que no necesita compartirse (ej: campo de busqueda local, estado de un input controlado, apertura de un drawer secundario).\n- **Redux**: MUST usarse cuando el estado es compartido entre componentes de distinto nivel, o cuando representa datos del servidor que deben persistir entre navegaciones dentro de la misma sesion.\n- Un componente de pagina SHOULD leer su estado de Redux y pasar datos a hijos por props, no conectar hijos directamente al store salvo que sea necesario para evitar prop drilling excesivo.

## Reglas de nombres

- Nombres MUST ser genericos y orientados al rol visual: `MuscleFormDialog`, `MusclesTable`, `RoutineList`.
- MUST NOT usar nombres que oculten el proposito real: `PopUpForm`, `MyModal`, `Component1`.
- Componentes de dialogo/formulario MUST llevar el sufijo `Dialog`: `MuscleFormDialog`, `RoutineFormDialog`.
- Componentes de tabla de dominio MUST llevar el sufijo `Table`: `MusclesTable`, `ExercisesTable`.
