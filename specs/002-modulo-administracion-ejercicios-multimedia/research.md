# Research: Modulo de Administracion de Ejercicios con Multimedia

## Decision 1: Persistencia admin sobre el patron actual de Firestore
- **Decision**: Mantener el patron actual basado en `FirestoreContext` y claves logicas dentro de `appDocuments`, creando namespaces `admin/...` e indices `admin-index/...` para codigos unicos y lecturas por prefijo.
- **Rationale**: Encaja con la infraestructura existente, conserva el fallback en memoria y evita introducir un segundo estilo de persistencia solo para esta feature.
- **Alternatives considered**:
  - Colecciones Firestore nativas solo para admin: rechazado por romper la consistencia de repositorios del backend.
  - Usar el `code` como `documentId`: rechazado porque cambia identidad si el codigo se renombra.

## Decision 2: Modelar la media como parte del agregado Exercise
- **Decision**: Persistir `ExerciseMedia[]` embebido dentro de cada `Exercise` en lugar de colecciones separadas por media.
- **Rationale**: El limite de 8 archivos por ejercicio hace viable el modelo embebido y simplifica invariantes como `isPrimary`, `sortOrder`, maximos por tipo y borrado logico atomico.
- **Alternatives considered**:
  - Documentos separados `exercise-media`: rechazado por complejidad extra para una galeria pequena.
  - Guardar solo una portada: rechazado porque no cumple la necesidad educativa de imagenes y videos multiples.

## Decision 3: Subidas directas con URLs firmadas de Firebase Storage
- **Decision**: Agregar un servicio de infraestructura basado en `Google.Cloud.Storage.V1` para emitir URLs firmadas V4 sobre el bucket de Firebase Storage y confirmar despues la persistencia del metadato.
- **Rationale**: Mantiene al backend como autoridad sobre `mediaId`, `storagePath`, expiracion, `contentType` y tamano maximo, pero evita transferir binarios grandes a traves de la API.
- **Alternatives considered**:
  - Subida directa con Firebase SDK desde frontend: rechazada por menor control del backend y peores garantias de auditoria.
  - Subida binaria proxy via backend: rechazada por costo operativo y latencia innecesaria en archivos de hasta 100 MB.

## Decision 4: Authorization policy explicita para administracion
- **Decision**: Introducir validacion real de Firebase ID tokens y una politica `AdminOnly` para todas las rutas `/api/admin/*`, manteniendo la proteccion de `/admin` en frontend como conveniencia UX.
- **Rationale**: El middleware actual no valida firmas ni claims; esta feature necesita autorizacion fuerte porque emite URLs firmadas y modifica catalogo maestro.
- **Alternatives considered**:
  - Ocultar `/admin` solo en frontend: rechazado por insuficiente.
  - Reglas de Firestore como unica defensa: rechazado porque el backend ya es la frontera real del sistema.

## Decision 5: Catalogo de workout desacoplado del catalogo general
- **Decision**: Crear un endpoint dedicado para seleccion de ejercicios de workout respaldado solo por ejercicios admin activos, sin reutilizar semanticamente el catalogo Wger existente durante la transicion.
- **Rationale**: Evita romper consumidores actuales de `/api/catalog/exercises`, permite migracion incremental del frontend y respeta la aclaracion de que workouts debe usar solo ejercicios administrados.
- **Alternatives considered**:
  - Reapuntar inmediatamente `/api/catalog/exercises`: rechazado por alto riesgo de regresion en otras vistas.
  - Mezclar Wger y admin en la busqueda: rechazado por contradecir la aclaracion cerrada en la spec.

## Decision 6: Snapshot minimo estable para historial y rutinas
- **Decision**: Persistir en workouts y rutinas un snapshot minimo del ejercicio admin con `exerciseId`, `name`, `coverStoragePath` o referencia equivalente, `formTypeId/formTypeCode` y `capturedAt`.
- **Rationale**: Preserva el historico cuando cambian nombre, portada o estado del ejercicio, y evita depender de URLs publicas permanentes para media privada.
- **Alternatives considered**:
  - Guardar solo `exerciseId`: rechazado porque rompe historico ante cambios o desactivaciones.
  - Guardar snapshot completo del ejercicio: rechazado por duplicacion innecesaria y mayor costo de evolucion.

## Decision 7: Borrado logico con indices de unicidad y filtros por estado
- **Decision**: Todas las entidades admin usaran `active`, `isDeleted`, `deletedAt`, `createdAt` y `updatedAt`; los codigos unicos se validaran con indices dedicados y los listados de negocio excluiran registros borrados logicamente.
- **Rationale**: Mantiene trazabilidad, preserva referencias historicas y deja claro que `active` es estado operativo, no eliminacion.
- **Alternatives considered**:
  - Borrado fisico: rechazado por riesgo de romper historial y relaciones.
  - Reutilizar `active=false` como eliminacion: rechazado porque mezcla conceptos distintos.

## Decision 8: Firestore y Storage administrados por backend
- **Decision**: La primera iteracion mantendra CRUD admin y registro de media como operaciones backend-only; Firestore rules y futuras Storage rules se endureceran para no depender de acceso directo del cliente a datos maestros.
- **Rationale**: Es el patron ya dominante del repositorio y evita acoplar la UI a estructuras internas de Firebase.
- **Alternatives considered**:
  - Cliente leyendo/escribiendo admin data directamente: rechazado por mayor superficie de seguridad.
  - Exponer URLs publicas permanentes para media: rechazado por privacidad y control deficiente del binario.
