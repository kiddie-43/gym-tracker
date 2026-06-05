# ADR-012: Rediseño de Logs Métricos Dinámicos de Entrenamiento

- Estado: Aceptado
- Fecha: 2026-05-29
- Feature: 012-logs-metricas-dinamicas

## Contexto

El modelo legacy de logs de ejercicio no soporta bien capturas dinámicas con múltiples métricas por set, ni reglas estrictas de edición limitada, borrado lógico por grupo y resolución diaria determinista.

La constitución del proyecto exige documentar en ADR cualquier decisión arquitectónica relevante. Esta feature introduce un reemplazo estructural del modelo anterior y fija la estrategia de compatibilidad para consumidores.

## Decisión

1. El modelo transaccional de entrenamiento se reemplaza por logs métricos dinámicos agrupados por `idGrupo`.
2. Cada set de captura genera un `idGrupo` único global e inmutable.
3. Solo `valorMetrica` es editable por clientes externos; el resto del contexto es inmutable.
4. El borrado lógico se ejecuta exclusivamente por grupo completo.
5. La consulta diaria resuelve el grupo vigente por fecha operativa actual y mayor `createdAt`.
6. La superficie soportada para consumidores queda fijada en `/api/training-metric-logs/*` con versión contractual OpenAPI `2.0.0`.
7. No existe ventana de compatibilidad dentro del mismo despliegue: al desplegar la versión contractual `2.0.0`, el flujo legacy deja de ser superficie soportada y los consumidores deben migrar antes de ese despliegue.

## Consecuencias

- Los contratos y pruebas deben tratar `/api/training-metric-logs/*` como única API vigente del módulo.
- La estrategia de sunset deja de ser ambigua: el disparador es el despliegue que introduce la versión contractual `2.0.0`.
- Cualquier necesidad futura de convivencia entre versiones deberá abrir un ADR nuevo con mecanismo cliente-visible explícito.