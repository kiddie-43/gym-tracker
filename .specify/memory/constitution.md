<!--
Sync Impact Report
- Version change: template -> 1.0.0
- Modified principles:
	- Principle slot 1 -> I. API-First y Contrato OpenAPI
	- Principle slot 2 -> II. Seguridad y Control de Acceso con Firebase Auth
	- Principle slot 3 -> III. Calidad Verificable en Backend y Frontend
	- Principle slot 4 -> IV. Integridad de Datos y Fronteras Wger/Firebase
	- Principle slot 5 -> V. Progreso Medible, Observabilidad y Operacion
- Added sections:
	- Restricciones Tecnologicas Obligatorias
	- Flujo de Desarrollo, Definition of Done y Checklist de PR
- Removed sections:
	- Ninguna
- Templates requiring updates:
	- [x] .specify/templates/plan-template.md
	- [x] .specify/templates/spec-template.md
	- [x] .specify/templates/tasks-template.md
	- [ ] .specify/templates/commands/*.md (ruta no presente en este workspace)
- Follow-up TODOs:
	- Ninguno
-->

# Gym Tracker Constitution

## Core Principles

### I. API-First y Contrato OpenAPI
Todas las capacidades de backend MUST exponerse mediante API REST en C#/.NET y quedar
documentadas en Swagger/OpenAPI. Ningun endpoint se considera completado sin contrato
actualizado, ejemplos minimos y codigos de error definidos. Todo cambio breaking MUST
versionarse y MUST incluir plan de migracion para consumidores.

Rationale: el contrato es la fuente de verdad entre backend, frontend y pruebas.

### II. Seguridad y Control de Acceso con Firebase Auth
Los endpoints protegidos MUST validar tokens de Firebase Auth y MUST aplicar
autorizacion por usuario y/o rol cuando corresponda. Secretos, claves y tokens MUST NOT
almacenarse en codigo, logs o repositorio. Toda configuracion sensible MUST gestionarse
por variables de entorno o gestor de secretos.

Rationale: protege datos personales y evita exposicion accidental de credenciales.

### III. Calidad Verificable en Backend y Frontend
El backend MUST mantener separacion por capas (API, Aplicacion, Dominio,
Infraestructura), validacion de entrada y manejo centralizado de errores. El frontend
MUST usar React + TypeScript estricto (sin any no justificado) y componentes accesibles
consistentes con Material UI. Toda vista principal MUST incluir estados de carga, error
y vacio.

Rationale: reduce deuda tecnica y asegura una experiencia predecible para el usuario.

### IV. Integridad de Datos y Fronteras Wger/Firebase
La API de Wger MUST usarse para catalogo de ejercicios, alimentos y metadatos de
referencia. Firebase MUST almacenar todo historial y estado transaccional del usuario
(entrenamientos, comidas, comparativas y metas). El sistema MUST mantener trazabilidad al
identificador externo de Wger cuando aplique. La aplicacion MUST implementar cache de
catalogo, politica de expiracion y modo degradado cuando Wger no este disponible.

Rationale: separa catalogo externo de datos propios del usuario y mejora resiliencia.

### V. Progreso Medible, Observabilidad y Operacion
La funcionalidad principal MUST comparar entrenamientos actuales con referencias
equivalentes: ultima sesion, promedio de ultimas 4 sesiones y mejor marca reciente. Las
metricas obligatorias MUST incluir volumen total, carga total, repeticiones totales,
variacion porcentual y tendencia (mejora/estable/empeora). Backend MUST incluir logging
estructurado con correlacion por request y health checks de dependencias criticas.

Rationale: el producto existe para mostrar progreso real, no solo para registrar datos.

## Restricciones Tecnologicas Obligatorias

- Backend MUST implementarse en C# con .NET (API REST).
- OpenAPI/Swagger MUST ser el contrato oficial para documentar y probar endpoints.
- Frontend MUST implementarse en React + TypeScript + Material UI.
- Autenticacion e identidad MUST gestionarse con Firebase Auth.
- Persistencia principal MUST gestionarse en Firebase para registros del usuario.
- Integracion con Wger MUST cubrir catalogos de ejercicios y alimentos.
- La app MUST NOT depender de Wger para persistir historial personal.
- Listados grandes MUST usar paginacion y endpoints criticos MUST definir objetivo de
	latencia p95.

## Flujo de Desarrollo, Definition of Done y Checklist de PR

- Ninguna feature MUST pasar a implementacion sin flujo Spec Kit completo:
	constitution -> specify -> plan -> tasks -> implement.
- PRs MUST ser pequenos, revisables y con alcance claro.
- Decisiones arquitectonicas relevantes MUST documentarse en ADR.

Definition of Done (obligatoria para cada feature):

- Contrato OpenAPI actualizado y consistente con el comportamiento real.
- Seguridad revisada (authn/authz) y reglas de Firebase validadas.
- Pruebas requeridas en verde:
	- Backend: unitarias para logica de negocio e integracion para endpoints criticos.
	- Frontend: componentes y flujos principales.
- Metricas de comparacion funcionando y validadas contra datos reales.
- Documentacion minima y criterios de aceptacion actualizados.

Checklist de PR de cumplimiento constitucional:

- [ ] Contrato OpenAPI actualizado.
- [ ] Autenticacion y autorizacion verificadas.
- [ ] Secretos protegidos y configuracion por entorno aplicada.
- [ ] Pruebas requeridas agregadas/actualizadas y en verde.
- [ ] Manejo de errores y logging estructurado implementados.
- [ ] Reglas Firebase revisadas y aplicadas.
- [ ] Integracion Wger con cache y fallback validada.
- [ ] Comparativas de progreso correctas y trazables.
- [ ] UI con estados de carga/error/vacio implementados.
- [ ] Documentacion y ADR (si aplica) actualizadas.

## Governance

Esta constitucion prevalece sobre practicas informales del proyecto. Toda enmienda MUST
registrar impacto, secciones afectadas y fecha de modificacion.

Politica de versionado de la constitucion:

- MAJOR: cambios incompatibles en principios o eliminacion/redefinicion de obligaciones.
- MINOR: nuevas secciones o expansion material de reglas obligatorias.
- PATCH: aclaraciones, redaccion o correcciones sin cambio normativo.

Cumplimiento y revision:

- Toda PR MUST incluir verificacion explicita del checklist constitucional.
- En revisiones de plan y tareas MUST validarse trazabilidad con estos principios.
- Se permiten excepciones solo si quedan justificadas por escrito, con alcance y tiempo
	de vigencia definidos.

**Version**: 1.0.0 | **Ratified**: 2026-05-05 | **Last Amended**: 2026-05-05
