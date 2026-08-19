---
name: spec-kit-workflow
description: 'Flujo de desarrollo Spec Kit para Gym Tracker. Usar cuando: se inicia una nueva feature, se ejecuta el flujo constitution→specify→plan→tasks→implement, se revisa el Definition of Done, se prepara un PR o se necesita el checklist constitucional de PR. Contiene el flujo obligatorio, la definicion de done y el checklist de cumplimiento constitucional.'
---

# Spec Kit — Flujo de Desarrollo y Definition of Done

## Flujo obligatorio por feature

Ninguna feature MUST pasar a implementacion sin completar este flujo en orden:

```
constitution → specify → plan → tasks → implement
```

- Cada tarea en `tasks.md` MUST indicar el skill o skills que aplican a su implementacion.
- PRs MUST ser pequeños, revisables y con alcance claro.
- Decisiones arquitectonicas relevantes MUST documentarse en ADR.

## Anotacion de skills en tareas

Cada tarea MUST incluir una referencia al skill relevante usando la etiqueta `[skill: nombre]`.
Si una tarea involucra multiples skills, se listan todos.

Formato:
```
- [ ] TXXX Descripcion de la tarea [skill: nombre-del-skill]
- [ ] TYYY Descripcion de la tarea [skill: backend-modular, skill: spec-kit-workflow]
```

Skills disponibles en el proyecto:

| Skill | Cuando usarlo |
|---|---|
| `frontend-architecture` | Estructura de carpetas frontend, ubicacion de modulos |
| `frontend-components` | Crear o modificar componentes React, UI base |
| `frontend-services` | Servicios en `services/` (api, firebase, storage, observability) |
| `frontend-redux` | Actions, reducers, states, store |
| `backend-modular` | Endpoints, controladores, servicios, CRUD, auditoria |
| `spec-kit-workflow` | Flujo de feature, DoD, checklist de PR |

## Definition of Done

Una feature se considera completada cuando:

- [ ] Contrato OpenAPI actualizado y consistente con el comportamiento real.
- [ ] Seguridad revisada (authn/authz) y reglas de Firebase validadas.
- [ ] Pruebas en verde:
  - Backend: unitarias para logica de negocio e integracion para endpoints criticos.
  - Frontend: componentes y flujos principales.
- [ ] Metricas de comparacion funcionando y validadas contra datos reales.
- [ ] Documentacion minima y criterios de aceptacion actualizados.

## Checklist de PR — Cumplimiento constitucional

Incluir en toda PR antes de solicitar revision:

- [ ] Contrato OpenAPI actualizado.
- [ ] Autenticacion y autorizacion verificadas.
- [ ] Secretos protegidos y configuracion por entorno aplicada.
- [ ] Pruebas requeridas agregadas/actualizadas y en verde.
- [ ] Manejo de errores y logging estructurado implementados.
- [ ] Reglas Firebase revisadas y aplicadas.
- [ ] Integracion Wger con cache y fallback validada (si aplica).
- [ ] Comparativas de progreso correctas y trazables (si aplica).
- [ ] UI con estados de carga/error/vacio implementados.
- [ ] Documentacion y ADR (si aplica) actualizadas.
