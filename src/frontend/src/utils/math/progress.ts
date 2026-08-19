export function getTrendLabel(deltaPercent: number): 'up' | 'down' | 'flat' {
	if (deltaPercent > 0) {
		return 'up';
	}

	if (deltaPercent < 0) {
		return 'down';
	}

	return 'flat';
}

export function formatPercentage(value: number): string {
	if (!Number.isFinite(value)) {
		return '0%';
	}

	const rounded = Math.round(value * 10) / 10;
	return `${rounded}%`;
}
