# Firestore Indexes

## Recommended Composite Indexes

1. `users/{userId}/workouts`
- Fields: `performedAt` descending
- Use case: workout history paging and recent-session lookups.

2. `users/{userId}/meal-logs`
- Fields: `loggedDate` descending, `slotType` ascending
- Use case: meal history and day views.

3. `users/{userId}/progress-snapshots`
- Fields: `exerciseId` ascending, `createdAt` descending
- Use case: latest progress snapshot by exercise.

4. `users/{userId}/routines`
- Fields: `name` ascending
- Use case: sorted routine listings.

## Notes

- Keep page sizes between 10 and 50 for stable latency.
- Prefer filtering by user path prefix before ordering fields.
- Cache catalog responses in memory before hitting Firestore to reduce read volume.
