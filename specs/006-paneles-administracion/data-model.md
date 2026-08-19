# Data Model: Paneles de Administración con Tabs

**Feature**: 006-paneles-administracion  
**Date**: 2026-05-15

## Estado de la aplicación

Esta feature no introduce nuevas entidades de dominio ni modifica el modelo de datos existente.

El único estado introducido es **UI-local** a `AdministracionPage`:

| Campo | Tipo | Valor inicial | Descripción |
|-------|------|---------------|-------------|
| `activeTab` | `number` | `0` | Índice de la pestaña activa (0–3) |

## Mapeo de índice a pestaña

| Índice | Clave i18n | Texto (ES) |
|--------|-----------|------------|
| 0 | `administration.tabs.exercises` | Ejercicios |
| 1 | `administration.tabs.muscles` | Músculos |
| 2 | `administration.tabs.exerciseTypes` | Tipo de ejercicios |
| 3 | `administration.tabs.formTypes` | Tipo de formulario |

## Notas

- No se crean interfaces TypeScript nuevas (sin props ni contratos de datos).
- Las entidades de dominio (Músculo, Tipo de Ejercicio, etc.) se modelarán en las ramas futuras de cada panel CRUD.
