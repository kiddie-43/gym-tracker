---
name: backend-module-structure-crud
description: 'Plantilla generica para creacion de estructura backend C#/.NET en Gym Tracker. Usar cuando: se inicia un modulo nuevo CRUD o se extiende uno existente con entidad, contratos, servicio, repositorio, controlador y migraciones.'
---

# Backend Skill - Creacion de Estructura Modular CRUD

## Objetivo

Definir un patron reusable para crear cualquier modulo backend CRUD en Gym Tracker siguiendo arquitectura por capas y convenciones del proyecto.

Esta skill NO esta atada a una tabla concreta.

## Estructura base por modulo

Usar `<Modulo>` como nombre singular/plural segun corresponda:

```text
src/backend/src/
  GymTracker.Domain/Entities/<Modulo>/
    <Entidad>.cs

  GymTracker.Application/<Modulo>/
    I<Entidad>Repository.cs
    <Entidad>Contracts.cs
    <Entidad>Service.cs

  GymTracker.Infrastructure/<Modulo>/
    <Entidad>Repository.cs

  GymTracker.Api/Controllers/<Modulo>s/
    <Modulo>sController.cs

  GymTracker.Infrastructure/Admin/
    AdminDbContext.cs

  GymTracker.Infrastructure/Migrations/
    <timestamp>_Add<Modulo>Module.cs
    AdminDbContextModelSnapshot.cs
```

## Flujo obligatorio al crear un modulo nuevo

1. Crear entidad de dominio con validaciones y reglas de negocio.
2. Crear contratos (request/response/page) en Application.
3. Crear interface de repositorio en Application.
4. Crear servicio de aplicacion con reglas de negocio, filtros, paginacion y orden.
5. Crear repositorio EF Core en Infrastructure.
6. Crear controlador API con endpoints thin.
7. Registrar `DbSet` y mapeo `Configure<Entidad>` en `AdminDbContext`.
8. Crear migracion y sincronizar snapshot.
9. Validar errores de compilacion y contratos HTTP.

## Convenciones de dominio

Las entidades SHOULD:

- Heredar `AuditableEntity` cuando sean mutables y requieran soft delete.
- Validar campos obligatorios (`Guid.Empty`, strings vacios, rangos invalidos).
- Exponer metodos `Create(...)` y `Update(...)`.

## Convenciones de contratos

Nombrado recomendado:

- `Upsert<Entidad>Request`
- `<Entidad>Response`
- `<Entidades>PageResponse`

El listado paginado SHOULD devolver:

```json
{
  "items": [],
  "totalCount": 0,
  "page": 0,
  "pageSize": 10
}
```

## Convenciones de servicio

El servicio MUST:

- Normalizar entradas (trim, defaults de paginacion, sort).
- Aplicar `page` 0-indexed y `pageSize` max 100.
- Filtrar registros eliminados logicamente.
- Validar referencias cruzadas entre modulos cuando aplique.
- Lanzar:
  - `ArgumentException` para validacion (`400`).
  - `InvalidOperationException` para conflictos (`409`).

## Convenciones de repositorio

El repositorio MUST:

- Usar `AdminDbContext`.
- Implementar `GetByIdAsync`, `ListAsync` o `ListBy...Async`, `SaveAsync`, `DeleteAsync`.
- Aplicar soft delete (`entity.Delete(userId)` o equivalente).
- Incluir `Include(...)` cuando el servicio necesite navegaciones.

## Convenciones de API

El controlador MUST:

- Ser thin endpoint: sin logica de negocio.
- Mapear metodos HTTP estandar CRUD.
- Retornar codigos correctos: `200`, `201`, `204`, `400`, `401`, `404`, `409`.
- Devolver errores con formato:

```json
{
  "message": "..."
}
```

Si el modulo es por usuario autenticado, SHOULD heredar `CurrentUserControllerBase`.

## Convenciones EF Core

En `AdminDbContext` MUST agregarse:

- `DbSet<Entidad> <Entidades>`
- `Configure<Entidad>(modelBuilder)`

Mapeo minimo recomendado:

- `ToTable("<Entidades>")`
- `HasKey(...)`
- `Property(...).IsRequired()` segun reglas
- FKs con `OnDelete(DeleteBehavior.Restrict|Cascade)` segun caso
- Indices por busqueda/filtro
- Indice unico filtrado para evitar duplicados activos cuando aplique

## Template rapido para nuevo modulo

Sustituir placeholders:

- `<Entidad>` ejemplo: `WorkoutPlan`
- `<Entidades>` ejemplo: `WorkoutPlans`
- `<Modulo>` ejemplo: `WorkoutPlan`

## Checklist de Definition of Done

1. Estructura de carpetas completa en las 4 capas.
2. CRUD funcional en servicio, repo y controller.
3. Validaciones de negocio y referencias cruzadas implementadas.
4. Soft delete y auditoria aplicados si corresponde.
5. DbContext + migracion + snapshot sincronizados.
6. Respuestas HTTP y errores con contrato consistente.
7. `get_errors` sin errores.
