# Research: Pagina de Rutinas de Entrenamiento

**Feature**: 011-rutinas-entrenamiento  
**Date**: 2026-05-18  
**Status**: Complete

## Decision 1: Flujo guiado con stepper en la misma ruta

- Decision: Ejecutar el entrenamiento en una vista tipo stepper a pantalla completa sin navegar a otra ruta, con arbol fijo `rutina -> sesion -> ejercicio -> datos del ejercicio`.
- Rationale: Cumple la clarificacion de foco del usuario durante el entrenamiento y reduce abandonos por cambios de contexto.
- Alternatives considered:
  - Navegacion por rutas separadas: descartada por romper continuidad y foco.
  - Modal superpuesta: descartada por limitaciones para recorridos largos y recuperacion de estado.

## Decision 2: Bloqueo de navegacion durante entrenamiento activo

- Decision: Activar un modo `trainingLocked` al pulsar "Iniciar entrenamiento" que restringe la navegacion al arbol interno del stepper; "Cancelar entrenamiento" desactiva el bloqueo.
- Rationale: Se alinea con el requerimiento de centrar al usuario en el entrenamiento y evitar salidas accidentales.
- Alternatives considered:
  - Advertencia sin bloqueo real: descartada por no prevenir abandonos.
  - Bloqueo total sin boton de cancelacion: descartada por mala usabilidad.

## Decision 3: Persistencia y restauracion del ultimo estado en curso

- Decision: Persistir `rutinaId`, `sesionId`, `ejercicioId`, `stepNode`, timestamp y estado de bloqueo por usuario para restaurar el flujo tras F5 o reapertura.
- Rationale: Cumple FR-041 y asegura continuidad operativa del entrenamiento.
- Alternatives considered:
  - Persistir solo en memoria: descartado por perder estado en recarga/cierre.
  - Reanudar siempre desde inicio de rutina: descartado por degradar experiencia.

## Decision 4: Origen de ejercicios y eliminacion logica de vinculos

- Decision: Permitir agregar ejercicios solo desde catalogo existente y quitarlos de la rutina mediante desvinculacion logica (no hard delete del ejercicio base).
- Rationale: Mantiene integridad del catalogo, cumple constitucion de borrado logico y evita perdida de referencias.
- Alternatives considered:
  - Ejercicios libres de texto: descartado por inconsistencia y baja trazabilidad.
  - Eliminacion fisica del ejercicio al quitarlo de rutina: descartado por violacion constitucional.

## Decision 5: Modelo de registro de entrenamiento por usuario

- Decision: Guardar registros de entrenamiento a nivel de ejercicio y usuario con series realizadas (kg/repeticiones), notas y adjuntos multimedia (max 5).
- Rationale: Cumple FR-028..FR-035 y habilita seguimiento personal de ejecucion real.
- Alternatives considered:
  - Notas globales por rutina: descartado por menor utilidad contextual.
  - Adjuntos sin limite: descartado por riesgos operativos y de costo.

## Decision 6: Concurrencia simplificada para esta fase

- Decision: Aplicar estrategia `last write wins` sin flujo de resolucion de conflictos multi-dispositivo.
- Rationale: Esta explicitamente aclarado en spec y reduce complejidad en V1.
- Alternatives considered:
  - Lock pesimista por rutina: descartado por sobrecosto de implementacion.
  - Merge interactivo de cambios: descartado por alcance actual.

## Decision 7: Frontera Wger/Firebase y degradacion

- Decision: Consumir catalogo de ejercicios desde frontera definida por backend (con cache/fallback), y persistir estado transaccional de rutinas/entrenamientos en Firebase.
- Rationale: Cumple principio constitucional IV y mantiene separacion de responsabilidades.
- Alternatives considered:
  - Persistir transacciones en Wger: descartado por no ser fuente de datos de usuario.
  - Depender de disponibilidad online sin fallback: descartado por baja resiliencia.

## Open Points Resueltos

- Limite de adjuntos por registro: resuelto en 5 archivos (fotos/videos).
- Regla de sesiones por dia: resuelto en 1 sesion activa por dia y rutina.
- Borrado: resuelto como borrado logico obligatorio con reactivacion.
- Flujo de ejecucion: resuelto con stepper full-screen y bloqueo de navegacion.

## End-to-End Validation Log (2026-05-18)

- Backend build ejecutado y exitoso.
- Pruebas unitarias clave de rutinas/training ejecutadas y exitosas.
- Pruebas de integracion de rutinas, sesiones, ejercicios de sesion, planned sets, flow, logs y contratos ejecutadas y exitosas.
- Build frontend ejecutado y exitoso.
- Pruebas de componentes de rutinas (tarjetas, estados, formulario de entrenamiento, comparativas, flujo stepper y catalogo de ejercicios) ejecutadas y exitosas.

Conclusiones:

- El flujo de entrenamiento guiado mantiene bloqueo de navegacion interna mientras `trainingLocked=true`.
- La politica de actualizacion concurrente se valida como last-write-wins en rutina.
- Se valida modo degradado del catalogo externo con aviso visible en la pagina de rutinas.
