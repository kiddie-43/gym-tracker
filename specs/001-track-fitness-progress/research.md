# Research: App de Seguimiento Fitness y Progreso

## Decision 1: Backend .NET como único gateway de negocio
- **Decision**: El frontend llamará al backend .NET para todas las operaciones de negocio, acceso a Firestore y consumo de Wger. Firebase Auth se usará en cliente para login, pero el token se enviará al backend para autorización y aislamiento de reglas.
- **Rationale**: Centraliza validaciones, cálculo de comparativas, caché de catálogo, logging, control de errores y contrato OpenAPI.
- **Alternatives considered**:
  - Cliente llamando directamente a Firestore y Wger: rechazado por exponer más lógica y dificultar trazabilidad, seguridad y consistencia.
  - Cloud Functions como capa principal: rechazado porque el stack constitucional obliga a backend .NET como API central.

## Decision 2: Persistencia en Cloud Firestore
- **Decision**: Usar Cloud Firestore como base de datos principal para datos transaccionales del usuario y caché persistente de catálogo.
- **Rationale**: Encaja con Firebase Auth, simplifica multiusuario, ofrece documentos/subcolecciones adecuados para rutinas, dietas, entrenamientos y comidas, y permite cachear entidades externas.
- **Alternatives considered**:
  - Realtime Database: menos adecuada para consultas por rango, filtros y estructuras agregadas del dominio.
  - Base relacional adicional: rechazada por aumentar complejidad fuera del alcance de la primera versión.

## Decision 3: Caché híbrida para Wger
- **Decision**: Implementar caché en dos niveles para Wger: memoria en backend para lecturas frecuentes y colección persistente en Firestore para supervivencia ante caídas del proveedor.
- **Rationale**: Reduce latencia, minimiza llamadas repetidas y habilita modo degradado controlado cuando Wger no esté disponible.
- **Alternatives considered**:
  - Sin caché: rechazada por dependencia fuerte del proveedor y peor experiencia.
  - Solo caché en memoria: rechazada porque no sobrevive reinicios ni permite continuidad estable.

## Decision 4: Cálculo de progreso como servicio de dominio con snapshots persistidos
- **Decision**: Calcular comparativas en backend mediante un servicio de dominio y persistir snapshots de progreso al registrar o recalcular entrenamientos.
- **Rationale**: Garantiza reproducibilidad, facilita historial de comparativas y evita recalcular todo el historial en cada consulta.
- **Alternatives considered**:
  - Cálculo solo en lectura: más simple inicialmente, pero peor rendimiento y más difícil de auditar.
  - Cálculo en frontend: rechazado por inconsistencias potenciales y exposición de reglas de negocio.

## Decision 5: Frontend SPA con React, TypeScript y Material UI
- **Decision**: Crear una SPA con React + TypeScript, Material UI como sistema visual, React Router para navegación, TanStack Query para fetch/cache de API y React Hook Form para formularios complejos.
- **Rationale**: Permite flujos ricos para formularios de rutinas, dietas y entrenamiento, con tipado fuerte y buen manejo de estados remotos.
- **Alternatives considered**:
  - Estado manual con fetch nativo: posible, pero peor escalabilidad para invalidación y sincronización.
  - Framework fullstack adicional: rechazado para mantener el backend .NET como frontera única.

## Decision 6: Estrategia de pruebas por capas
- **Decision**: Backend con xUnit + FluentAssertions + WebApplicationFactory; frontend con Vitest + React Testing Library; contratos OpenAPI validados con tests de integración del backend.
- **Rationale**: Alinea pruebas con la constitución y cubre lógica crítica, endpoints y flujos de UI sin sobrecarga excesiva.
- **Alternatives considered**:
  - Solo unit tests: insuficiente para auth, contratos y reglas de integración.
  - E2E como base principal: útil después, pero demasiado costoso para ser la primera línea de validación.

## Decision 7: Preferencia calórica explícita por usuario
- **Decision**: Modelar el seguimiento calórico como una preferencia del usuario con tres estados operativos visibles: desactivado, activo con datos completos y activo con datos incompletos.
- **Rationale**: Evita inferencias erróneas y mantiene el comportamiento coherente con la spec.
- **Alternatives considered**:
  - Activarlo por defecto para todos: rechazado porque rompe el requisito de opt-in.
  - Estimar calorías automáticamente cuando falten datos: rechazado por riesgo de mostrar información falsa.
