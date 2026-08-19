# Research: Configuracion Semanas y Dias

## Decision 1: Modelo de navegacion mensual con 4 semanas fijas
- Decision: El flujo reemplaza Rutina/Sesion por 4 semanas fijas (Semana 1..4) y tabs anidadas por dias.
- Rationale: Simplifica la experiencia mensual, elimina pasos intermedios y mantiene una estructura predecible para el usuario.
- Alternatives considered: Mantener Rutina/Sesion con alias visual a semanas; descartado por complejidad y doble semantica.

## Decision 2: Configuracion global de dias (1..7)
- Decision: Se define un unico N de dias para todo el mes y aplica igual a las 4 semanas.
- Rationale: Reduce complejidad de datos/UI, evita inconsistencias entre semanas y acelera validacion.
- Alternatives considered: N independiente por semana; descartado por mayor complejidad y riesgo de configuraciones ambiguas.

## Decision 3: Reduccion de dias con recorte de plan y preservacion historica
- Decision: Al reducir N, se vacian los dias fuera de rango del plan activo y se conserva historico para estadisticas.
- Rationale: Cumple expectativa de limpieza del plan actual sin perder trazabilidad ni datos para analitica de progreso.
- Alternatives considered: Borrado definitivo; descartado por perdida irreversible de informacion. Bloqueo del cambio hasta vaciado manual; descartado por friccion UX.

## Decision 4: Migracion dura en una sola entrega
- Decision: Migrar historial de Rutina/Sesion a Semana/Dia en una unica entrega, sin ventana de compatibilidad.
- Rationale: Evita sostener dos modelos en paralelo y reduce deuda de mantenimiento a medio plazo.
- Alternatives considered: Compatibilidad progresiva; descartada por mayor costo operativo y riesgo de divergencia funcional.

## Decision 5: Control de acceso por propietario
- Decision: Solo el propietario del plan puede crear/editar su configuracion Semana > Dia.
- Rationale: Minimiza riesgo de modificaciones no autorizadas y mantiene un modelo simple de autorizacion.
- Alternatives considered: Edicion por admins/entrenadores; descartada en esta iteracion por no estar en alcance funcional.

## Decision 6: Contrato API dedicado para plan mensual
- Decision: Exponer endpoints especificos de plan mensual (get/create-update, recorte por dias y migracion) con OpenAPI.
- Rationale: Asegura contrato claro frontend-backend y permite pruebas de contrato e integracion.
- Alternatives considered: Reutilizar endpoints legacy de Rutina/Sesion; descartado por incompatibilidad semantica con migracion dura.

## Decision 7: Calidad operacional minima obligatoria
- Decision: Definir objetivos p95, logs estructurados y health checks para el nuevo flujo.
- Rationale: Reduce riesgo en despliegue de un cambio breaking y facilita monitoreo post-release.
- Alternatives considered: Posponer observabilidad para una iteracion posterior; descartado por riesgo de diagnostico insuficiente.
