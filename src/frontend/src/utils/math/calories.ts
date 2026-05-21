export type CalorieStatus = 'below' | 'within' | 'above';

export function resolveCalorieStatus(consumed: number, target: number): CalorieStatus {
	const safeTarget = target > 0 ? target : 1;
	const lowerBound = safeTarget * 0.95;
	const upperBound = safeTarget * 1.05;

	if (consumed < lowerBound) {
		return 'below';
	}

	if (consumed > upperBound) {
		return 'above';
	}

	return 'within';
}
