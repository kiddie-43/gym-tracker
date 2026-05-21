---
name: frontend-architecture
description: 'Arquitectura y estructura de carpetas del frontend React de Gym Tracker. Usar cuando: se pregunta donde colocar un nuevo archivo, carpeta o modulo en el frontend, se decide si algo va en components/ vs pages/, se organiza cualquier carpeta dentro de src/frontend/src/, o se incorpora un nuevo dominio funcional al proyecto frontend.'
---

# Frontend — Arquitectura y Estructura de Carpetas

## Protocolo obligatorio antes de actuar

ANTES de crear, mover o modificar cualquier archivo del frontend, DEBES ejecutar estos pasos en orden:

1. **Leer el plan activo**: busca el archivo `specs/<numero>-<nombre>/plan.md` correspondiente a la feature en curso y leelo completo. STOP si la accion que vas a tomar no esta contemplada en ese plan — consulta al usuario antes de continuar.
2. **Leer el spec activo**: lee `specs/<numero>-<nombre>/spec.md` y verifica que el cambio corresponde a un requisito explicitamente solicitado.
3. **No actuar por inferencia estructural**: si la arquitectura "necesita" un archivo pero el usuario no lo solicito expresamente, MUST NOT crearlo. Crea solo lo que el usuario pidio.

## Mapa de carpetas obligatorio

Raiz: `src/frontend/src/`

```
src/frontend/src/
  components/     Componentes UI base reutilizables entre dominios, cada uno en su propia subcarpeta
  guards/         Guards de navegacion organizados por dominio (logica pura .ts)
  i18n/           Configuracion de i18next + traducciones por idioma en translates/
  interfaces/     Interfaces de dominio agrupadas por modulo funcional
  pages/          Vistas navigables + sus componentes directos organizados por dominio
  redux/          Estado global, incluyendo preferencias de usuario (ver skill frontend-redux)
  routes/         Definicion de rutas: importa paginas y aplica guards, exporta al App
  services/       Llamadas a API y servicios externos, incluyendo el cliente HTTP base
  theme/          Configuracion del tema MUI
  utils/          Funciones auxiliares puras reutilizables
```

## Criterio de ubicacion

| Pregunta | Respuesta |
|---|---|
| ¿El componente se usa en mas de un dominio? | `components/<Componente>/` |
| ¿El componente solo se usa dentro de un dominio concreto? | `pages/<dominio>/components/<Componente>/` |
| ¿El usuario puede navegar a ello por URL? | `pages/<dominio>/` — siempre, sin excepcion |
| ¿Es un componente directo de esa vista (lista, form, filtros, tabla)? | `pages/<dominio>/list\|form\|filters\|table/` |
| ¿Es la definicion de una ruta o aplicacion de un guard? | `routes/` |
| ¿Es un guard de navegacion (logica pura)? | `guards/<dominio>/<dominio>.ts` |
| ¿Es una llamada a API o servicio externo? | `services/api/<dominio>/<dominio>Api.ts` — ver skill `frontend-services` para el contenido |
| ¿Es estado global compartido entre modulos? | `redux/` |
| ¿Es una interfaz o tipo de dominio? | `interfaces/<dominio>/<dominio>.ts` |
| ¿Es una funcion auxiliar pura sin efectos secundarios? | `utils/` |

## Estructura de `components/`

Solo contiene componentes genuinamente reutilizables entre dominios. Cada componente MUST tener su propia subcarpeta:

```
components/
  DataTable/
    DataTable.tsx
  PopupDialog/
    PopupDialog.tsx
  AppHeader/
    AppHeader.tsx
  AppLayout/
    AppLayout.tsx
```

MUST NOT crearse un componente en `components/` si solo se usa en un dominio. MUST NOT duplicarse ni envolverse un componente base existente para añadir pequeñas variaciones — la personalizacion se resuelve por props.

## Estructura de `pages/<dominio>/`

TODA vista a la que el usuario pueda navegar MUST colocarse en `pages/<dominio>/`. STOP si intentas crear un archivo de pagina fuera de `pages/`. La pagina principal DEBE orquestar el estado Redux y pasar datos a sus componentes hijos por props. Estructura obligatoria:

```
pages/<dominio>/
  <Dominio>Page.tsx         ← registrada en routes/routes.tsx, lee Redux
  list/                     ← componente de lista principal
  form/                     ← dialogs de crear/editar
  filters/                  ← barra de filtros
  table/                    ← sub-componentes de tabla
  components/
    <Componente>/
      <Componente>.tsx      ← componentes exclusivos de este dominio
```

## Estructura de `routes/`

Centraliza la definicion de rutas. Importa las paginas y aplica los guards. `App.tsx` MUST importar las rutas desde aqui, nunca importar paginas directamente.

```
routes/
  routes.tsx        ← define todos los <Route>, importa paginas y aplica guards
```

Ejemplo:

```tsx
// routes/routes.tsx
import { Route, Routes, Navigate } from 'react-router-dom';
import { DietsPage } from '../pages/diets/DietsPage';
import { canAccessDiets } from '../guards/diets/diets';

export function AppRoutes() {
  return (
    <Routes>
      <Route
        path="/diets"
        element={canAccessDiets() ? <DietsPage /> : <Navigate to="/" replace />}
      />
    </Routes>
  );
}
```

`App.tsx` MUST montar UNICAMENTE `<AppRoutes />` dentro de `<BrowserRouter>`. MUST NOT definirse rutas inline en `App.tsx`.

> **STOP — trampa de guards**: El ejemplo anterior importa `canAccessDiets` que ya existe en `guards/diets/diets.ts`. Si el guard del dominio que estas implementando NO existe todavia, DEBES CREARLO PRIMERO en `guards/<dominio>/<dominio>.ts` antes de referenciarlo aqui. NUNCA escribas una funcion guard inline en `routes.tsx` ni la coloques en ningun archivo fuera de `guards/<dominio>/<dominio>.ts`.

## Estructura de `guards/`

Logica pura de navegacion sin JSX. Son archivos `.ts` organizados por dominio:

```
guards/
  <dominio>/
    <dominio>.ts
```

IMPORTA y aplica los guards EXCLUSIVAMENTE en `routes/routes.tsx`. MUST NOT importarse un guard directamente en una pagina ni en ningun otro archivo fuera de `routes/routes.tsx`. STOP si el guard necesario no existe en `guards/<dominio>/<dominio>.ts` — crealo ahi antes de usarlo. STOP si encuentras logica de guard escrita fuera de `guards/`.

## Reglas de i18n — traducciones

La carpeta `i18n/` tiene esta estructura fija:

```
i18n/
  i18n.ts              ← configuracion de i18next, no se modifica para añadir idiomas
  resources.ts         ← importa todos los idiomas de translates/ y los exporta
  translates/
    es.ts              ← traducciones en español
    en.ts              ← traducciones en ingles
    fr.ts              ← (ejemplo de nuevo idioma)
```

**Añadir un nuevo idioma**: crear `i18n/translates/<codigo>.ts` e importarlo en `resources.ts`. MUST NOT modificarse `i18n.ts` para añadir idiomas.

`resources.ts` MUST seguir este patron:

```ts
import { es } from './translates/es';
import { en } from './translates/en';

export const resources = {
  es: { translation: es },
  en: { translation: en },
};
```

Cada archivo de idioma exporta un objeto plano tipado:

```ts
// translates/es.ts
export const es = {
  common: { ... },
  administration: { ... },
  routines: { ... },
};
```

**Reglas de claves**:

Las claves se dividen en dos categorias estrictas:

**`common` — claves reutilizables entre dominios** (MUST NOT duplicarse en ningun dominio):
```
common.actions.save       common.actions.cancel     common.actions.delete
common.actions.edit       common.actions.create     common.actions.restore
common.fields.name        common.fields.code        common.fields.description
common.fields.date        common.fields.status      common.fields.active
common.messages.loadError common.messages.noResults common.messages.codeReadOnly
```

**`<dominio>` — claves exclusivas del dominio** (texto que no puede generalizarse):
```
administration.muscles.title          ← titulo de la seccion/pagina
administration.muscles.createTitle    ← titulo del dialog de crear
administration.muscles.editTitle      ← titulo del dialog de editar
administration.muscles.deleteConfirm  ← mensaje de confirmacion de borrado
```

Estructura estandar obligatoria por dominio:
```ts
'<dominio>': {
  title:         'Nombre del listado',        // cabecera de la pagina/panel
  createTitle:   'Crear <entidad>',           // titulo del dialog de crear
  editTitle:     'Editar <entidad>',          // titulo del dialog de editar
  deleteConfirm: 'Texto de confirmacion...',  // mensaje del dialog de borrado
}
```

**Regla clave**: los labels de campos de formulario, cabeceras de columnas de tabla, textos de botones de accion y mensajes de error genericos MUST usar `common`. Solo va bajo el dominio lo que es semanticamente unico de esa entidad (titulo de seccion, textos de confirmacion con el nombre de la entidad, etc.).

- MUST NOT duplicarse una clave que ya existe en `common`. ANTES de añadir cualquier clave nueva, LEE el contenido completo de `i18n/translates/es.ts` usando una herramienta de busqueda y comprueba si ya existe una clave semanticamente equivalente bajo el objeto `common`. STOP si existe — reutiliza esa clave. Solo si tras esa lectura confirmas que no existe, puedes añadirla.
- Al añadir texto nuevo, MUST añadirse en **todos** los archivos de idioma existentes (`es.ts`, `en.ts` y cualquier otro en `i18n/translates/`). STOP si no puedes confirmar que todos los idiomas tienen la clave nueva.

## Estructura de `utils/`

Contiene funciones auxiliares **puras** (sin efectos secundarios, sin llamadas a API, sin acceso a Redux) reutilizables entre dominios.

**Criterio para que algo sea una util**:
- Es una funcion generica que podria usarse en cualquier dominio sin cambios.
- No conoce ninguna entidad de negocio concreta.
- Dado el mismo input, siempre devuelve el mismo output.

**Ejemplos que SON utils**:
- Mapear un array a opciones de selector con formato `{ value, label, description }`.
- Calcular una suma, media o porcentaje sobre un array de numeros.
- Formatear una fecha a string segun locale.
- Truncar un texto a N caracteres.
- Filtrar un array por una cadena de busqueda sobre multiples campos.

**Ejemplos que NO son utils** (van en el dominio):
- Una funcion que filtra `MuscleDto[]` segun las reglas de negocio de musculos.
- Una funcion que calcula el volumen total de un workout.
- Cualquier logica que dependa de una interfaz de dominio concreta.

**Estructura**: cada categoria en su propia subcarpeta con su archivo principal:

```
utils/
  array/
    array.ts     ← operaciones sobre arrays (mapear, filtrar, agrupar)
  format/
    format.ts    ← formateo de fechas, numeros, textos
  math/
    math.ts      ← calculos genericos (sumas, medias, porcentajes)
```

Ejemplo de util valida:

```ts
// utils/array/array.ts
export function toSelectorOptions<T>(
  items: T[],
  getValue: (item: T) => string,
  getLabel: (item: T) => string,
  getDescription?: (item: T) => string,
): Array<{ value: string; label: string; description?: string }> {
  return items.map(item => ({
    value: getValue(item),
    label: getLabel(item),
    description: getDescription?.(item),
  }));
}
```

Uso en cualquier dominio:
```ts
const options = toSelectorOptions(muscles, m => m.id, m => m.name, m => m.code);
```



- MUST NOT importar desde `pages/<dominio>/` hacia otra `pages/<otroDominio>/`. STOP si necesitas algo de otro dominio — subelo a `components/` o `interfaces/` primero y luego importalo desde ahi.
- MUST NOT importar desde `components/` hacia `pages/`, `redux/` ni `services/`. STOP si detectas ese patron.
- MUST NOT importar desde `services/api/` hacia `pages/` ni `components/`. STOP si detectas ese patron.
- TODO lo transversal entre dominios MUST subirse a su capa correspondiente (`components/`, `interfaces/`, `utils/`) ANTES de usarlo en cualquier dominio.

## Incorporar un nuevo dominio funcional

Al añadir un nuevo modulo CRUD (ej: `diets`), la estructura minima esperada es:

```
pages/diets/
  DietsPage.tsx               ← entrada del router
  list/DietList.tsx
  form/DietFormDialog.tsx
  table/DietsTable.tsx

routes/routes.tsx             ← MUST añadir el nuevo <Route path="/diets"> aqui

interfaces/diets/
  diets.ts                    ← DTOs e interfaces de dominio

services/api/diets/
  dietsApi.ts                 ← llamadas a la API

redux/actions/diets/
  dietsActions.ts
redux/reducers/diets/
  dietsReducer.ts
redux/states/diets/
  dietsState.ts

i18n/resources.ts             ← MUST añadir las claves del dominio bajo 'diets'
```

El nombre del modulo MUST ser consistente en todas las capas.

## Reglas obligatorias para interfaces y servicios

- `interfaces/` y `services/` MUST estar modularizados por dominio funcional.
- Dentro de cada modulo MUST NOT usarse `index.ts` como archivo principal.
- El archivo MUST llamarse igual que su carpeta: `muscles/muscles.ts`, `musclesApi.ts`.
- El cliente HTTP base esta en `services/api/httpClient.ts` y MUST reutilizarse; MUST NOT crearse otro cliente HTTP.
- El estado de preferencias de usuario (tema, perfil) MUST gestionarse en Redux bajo `redux/states/preferences/`. MUST NOT usarse React Context para estado global.
- Para el contenido e implementacion de los servicios (estructura interna, uso del cliente HTTP, integracion con Firebase, etc.) ver skill `frontend-services`.

## Reglas de nombrado de archivos

- Paginas: `<Dominio>Page.tsx` — `RoutinesPage.tsx`, `AdministrationPage.tsx`
- Dialogs: `<Entidad>FormDialog.tsx` — `MuscleFormDialog.tsx`, `RoutineFormDialog.tsx`
- Tablas de dominio: `<Entidades>Table.tsx` — `MusclesTable.tsx`, `ExercisesTable.tsx`
- APIs: `<dominio>Api.ts` — `musclesApi.ts`, `routinesApi.ts`
- Interfaces: `<dominio>.ts` — `muscles.ts`, `routines.ts`
- Guards: `<dominio>.ts` dentro de `guards/<dominio>/`
