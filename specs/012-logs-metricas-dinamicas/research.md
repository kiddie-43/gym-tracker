# Research: Logs Metricos Dinamicos de Entrenamiento

**Feature**: 012-logs-metricas-dinamicas  
**Date**: 2026-05-28  
**Status**: Complete

## Decision 1: Agrupacion por set con idGrupo inmutable

- Decision: Cada set de captura genera un `idGrupo` unico global; todas las metricas del set comparten ese `idGrupo`.
- Rationale: Permite representar ejercicios con una o multiples metricas sin perder la relacion entre valores del mismo set.
- Alternatives considered:
  - Una fila por metrica sin agrupacion: descartada por ambiguedad para editar/borrar conjuntos.
  - Un solo valor compuesto por fila: descartado por menor trazabilidad y validacion mas compleja.

## Decision 2: Edicion acotada a valorMetrica

- Decision: La operacion de edicion solo modifica `valorMetrica`; el resto de campos del registro son inmutables para clientes externos.
- Rationale: Reduce riesgo de corrupcion de contexto historico y simplifica auditoria.
- Alternatives considered:
  - Permitir editar contexto (rutina/sesion/fecha/metrica): descartado por romper integridad historica.

## Decision 3: Borrado logico solo por idGrupo

- Decision: El borrado se ejecuta exclusivamente por `idGrupo` y marca logicamente todo el conjunto de metricas relacionadas.
- Rationale: Mantiene consistencia del set y evita estados parciales entre metricas que deben viajar juntas.
- Alternatives considered:
  - Borrado parcial por metrica: descartado por riesgo de grupos incompletos.
  - Borrado fisico: descartado por incumplir politica constitucional de eliminacion logica.

## Decision 4: Regla de vigencia diaria

- Decision: Para consulta de "actual" se usa la fecha actual; si hay varios grupos en el dia, el vigente es el de mayor `CreatedAt`.
- Rationale: Se alinea con gestion diaria y con una regla de desempate determinista.
- Alternatives considered:
  - Mayor `UpdatedAt`: descartado por introducir dependencia de ediciones posteriores.
  - Conflicto por multiples grupos: descartado por friccion de UX diaria.

## Decision 5: Respuesta sin datos

- Decision: Si no hay logs en la fecha actual para el contexto consultado, el endpoint devuelve `[]`.
- Rationale: Evita tratar ausencia de datos como error de negocio y simplifica frontend.
- Alternatives considered:
  - Responder `404`: descartado por no representar fallo real.
  - Responder `null`: descartado por inconsistencia con contratos de listas.

## Decision 6: Integracion frontend con patrones existentes

- Decision: La pagina de detalle de logs reutiliza `PopupDialog` y `ActionMenu` conectados al estado Redux del modulo.
- Rationale: Mantiene consistencia con paneles administrativos existentes y reduce deuda visual/arquitectonica.
- Alternatives considered:
  - Estado local por componente: descartado por perder trazabilidad del flujo CRUD.
  - Nuevos componentes de popup/menu: descartado por duplicacion.

## Decision 7: Frontera de datos y seguridad

- Decision: Definiciones de metricas y catalogo se consumen desde fuentes de catalogo; datos transaccionales de logs metricos y trazas se persisten en almacenamiento principal del proyecto.
- Rationale: Cumple constitucion de frontera de datos, seguridad y trazabilidad.
- Alternatives considered:
  - Persistir historial transaccional en fuente de catalogo externa: descartado por incumplimiento de frontera.

## Validation Notes

- La especificacion ya incluye reglas cerradas para:
  - `idGrupo` unico global.
  - Borrado logico por grupo completo.
  - Edicion solo de `valorMetrica`.
  - Consulta diaria por fecha actual y desempate por `CreatedAt`.
  - Retorno de array vacio cuando no existen registros del dia.

## ADR Reference

- La decision arquitectonica de reemplazo del modelo legacy y la estrategia de sunset quedaron fijadas en `docs/adr/ADR-012-logs-metricos-dinamicos.md`.
