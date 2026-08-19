---
name: backend-modular
description: 'Estandares de backend C#/.NET para Gym Tracker. Usar cuando: se crea o modifica un endpoint, controlador, servicio de aplicacion, entidad de dominio o implementacion de infraestructura, se diseña un nuevo modulo CRUD, se implementa paginacion/filtros/ordenacion, se añade auditoria, se validan entradas, se implementa eliminacion logica o se cruzan datos entre modulos. Contiene arquitectura por capas, thin endpoints, CRUD estandar, paginacion, auditoria y validacion.'
---

# Backend — Arquitectura Modular y Estandares

## Arquitectura por capas

```
src/backend/
  src/
    GymTracker.Api/           Transporte HTTP (controllers, middlewares)
    GymTracker.Application/   Casos de uso, servicios, contratos
    GymTracker.Domain/        Entidades, value objects, reglas de negocio
    GymTracker.Infrastructure/ Implementaciones tecnicas concretas
  tests/
    GymTracker.Application.UnitTests/
    GymTracker.Api.IntegrationTests/
```

Cada capa MUST organizarse por modulo/feature con el mismo nombre en todas las capas:
- `Api/Controllers/<Modulo>/`
- `Application/Services/<Modulo>/` y `Application/Contracts/<Modulo>/`
- `Infrastructure/<Modulo>/`

Las implementaciones concretas en `Infrastructure` MUST implementar contratos definidos en `Application`.

## Thin endpoints

Los endpoints MUST limitarse a:
1. Recibir y validar el request HTTP
2. Invocar el servicio/caso de uso
3. Devolver la respuesta HTTP con el codigo correcto

MUST NOT contener logica de negocio ni acceso directo a datos.
SHOULD encargarse de auth/authz, mapeo request-response, codigos HTTP y errores de transporte.

## Validacion y saneamiento de entradas

Todo endpoint MUST validar y sanear antes de ejecutar el caso de uso:

- Strings: normalizar eliminando espacios al inicio y al final.
- Fechas: validar y normalizar a UTC.
- Numericos y enums: validar contra rango y valores permitidos.
- Entradas malformadas o potencialmente maliciosas: rechazar con respuesta de validacion adecuada.
- Consultas y persistencia: MUST ejecutarse con mecanismos seguros (parametrizacion/ORM) para mitigar inyecciones.

## CRUD estandar obligatorio por modulo

Cada modulo MUST exponer estos endpoints. El orden de declaracion en el controlador MUST seguir esta tabla:

| Endpoint | Metodo | Descripcion | HTTP code exito |
|---|---|---|---|
| `GET /modulo` | listado paginado | Devuelve `PagedResult<EntidadDto>` | 200 |
| `GET /modulo/{id}` | por id | Devuelve `EntidadDto` | 200 |
| `POST /modulo` | crear | Devuelve `EntidadDto` creado | 201 |
| `PUT /modulo/{id}` | editar | Devuelve `EntidadDto` actualizado | 200 |
| `DELETE /modulo/{id}` | eliminacion logica | Sin cuerpo | 204 |
| `GET /modulo/search` | busqueda autocomplete | Solo si el modulo es referenciado en selectores de otro modulo | 200 |

### Endpoint de busqueda para autocomplete

Se implementa **solo si** otro modulo necesita buscar entidades de este dominio para alimentar un selector. Acepta un parametro `q` (texto libre) y devuelve un array plano **sin paginar**:

```
GET /modulo/search?q=texto
→ 200 [ EntidadDto, ... ]
```

MUST NOT reutilizarse el endpoint de listado paginado para alimentar selectores.

## Contratos de request y response

### Naming de contratos

Los contratos MUST seguir esta convencion, consistente con los tipos del frontend:

| Artefacto | Patron | Ejemplo |
|---|---|---|
| DTO de respuesta | `<Entidad>Dto` | `MuscleDto`, `RoutineDto` |
| Request de crear/editar | `Upsert<Entidad>Request` | `UpsertMuscleRequest` |
| Request de filtros (query params) | `<Entidades>Filters` | `MusclesFilters` |

### Forma del response paginado

El endpoint de listado MUST devolver exactamente esta estructura para que el frontend pueda asignar `items` a `list` y `total` a `pagination.total`:

```json
{
  "items": [ /* EntidadDto[] */ ],
  "total": 42
}
```

`total` MUST ser el total de registros que cumplen los filtros, no el numero de items de la pagina actual.

### Forma del response de error

Todos los errores MUST devolver el siguiente cuerpo para que `getErrorMessage` del frontend lo extraiga correctamente:

```json
{
  "message": "Descripcion del error legible por el usuario"
}
```

Errores de validacion (400) MUST incluir el campo `message` con un texto descriptivo. MUST NOT devolverse stacks, nombres de excepcion internos ni rutas de archivo.

## Paginacion, ordenacion y filtros

El endpoint de listado MUST aceptar estos query params. Los nombres MUST coincidir exactamente con los que envia el frontend (`frontend-services`):

| Parametro | Tipo | Descripcion |
|---|---|---|
| `page` | `int` | Numero de pagina (0-indexed) |
| `pageSize` | `int` | Registros por pagina |
| `sortField` | `string` | Identificador del campo por el que ordenar |
| `sortDir` | `string` | `asc` o `desc` |
| `[filtros]` | varios | Un parametro por cada campo filtrable del modulo |

`page` MUST ser 0-indexed para coincidir con `PaginationState.page` del frontend. El backend MUST traducir internamente a 1-indexed si el ORM lo requiere.

## Eliminacion logica

- La eliminacion MUST ser logica, nunca fisica en endpoints CRUD.
- Las consultas de lectura por defecto MUST excluir registros eliminados logicamente.

## Cruce de datos entre modulos

- La orquestacion MUST implementarse en servicios/casos de uso de `Application`, no en endpoints.
- Cuando intervengan multiples fuentes, SHOULD utilizarse un servicio orquestador por modulo.

## Auditoria obligatoria en operaciones mutables

Todas las operaciones de creacion, edicion y eliminacion logica MUST registrar:

| Campo | Valor |
|---|---|
| `actionAt` | Fecha y hora UTC — generada por el servidor |
| `actionBy` | Identificador del usuario autenticado — del contexto de auth |

Los valores de auditoria MUST NOT confiarse a datos enviados por el cliente.
