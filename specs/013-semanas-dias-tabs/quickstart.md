# Quickstart: Configuracion Semanas y Dias

## 1. Prerrequisitos
- .NET SDK 8+
- Node.js 20+
- Dependencias instaladas en raiz y frontend

## 2. Levantar entorno local
1. `npm install`
2. `npm run install:all`
3. `npm run dev`

Resultado esperado:
- Backend disponible en `http://localhost:5092`
- Frontend disponible en `http://localhost:5173` (o puerto alternativo)

## 3. Flujo funcional principal (P1)
1. Iniciar sesion como usuario propietario.
2. Abrir planificacion mensual.
3. Verificar tabs de `Semana 1` a `Semana 4`.
4. Configurar dias globales, por ejemplo N=5.
5. Verificar que en cada semana aparecen `Dia 1`..`Dia 5`.
6. Agregar ejercicios en `Semana 2 > Dia 3`.
7. Cambiar a otra semana y volver; validar persistencia visual y de datos.

## 4. Recorte por reduccion de dias
1. Partiendo de N=5 con ejercicios en Dia 5, reducir a N=3.
2. Confirmar accion de recorte.
3. Validar que `Dia 4` y `Dia 5` no aparecen en plan activo.
4. Validar que historicos de entrenamiento siguen presentes para estadisticas.

## 5. Seguridad / autorizacion
1. Intentar leer/editar un plan de otro usuario.
2. Resultado esperado: rechazo de autorizacion.

## 6. Migracion dura
1. Ejecutar proceso de migracion definido para convertir datos legacy Rutina/Sesion a Semana/Dia.
2. Validar que el flujo de planificacion funciona sin referencias activas a campos legacy.
3. Verificar trazabilidad de conversion en registros de auditoria.

## 7. Pruebas recomendadas
- Backend
  - Unit tests: reglas de activeDays, recorte y ownership.
  - Integration tests: endpoints de plan mensual y migracion.
- Frontend
  - Component tests: tabs anidadas, estados vacio/carga/error.
  - Redux tests: create/update de plan y recorte de dias.

## 8. Observabilidad minima
- Validar logs estructurados para operaciones: create/update plan, truncation y migration.
- Validar correlacion por request y health checks de dependencias.

## 9. Validaciones de criterios clave
- SC-005 (rango dias):
  - Intentar guardar `activeDays=0` y `activeDays=8`.
  - Resultado esperado: `400` con mensaje de validacion, sin mutacion del plan confirmado.
- SC-006 (confirmacion de exclusion):
  - Con ejercicios en dias altos, reducir N y verificar confirmacion previa.
  - Resultado esperado: sin confirmacion no se aplica recorte; con confirmacion, se aplica.
- SC-009 (ownership):
  - Intentar leer/editar plan de otro usuario autenticado.
  - Resultado esperado: rechazo de autorizacion (`401/403`) en todos los endpoints de plan mensual.
