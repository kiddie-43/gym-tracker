import { Card, CardContent, Skeleton as MuiSkeleton, Stack } from '@mui/material';

type SkeletonProps = {
    variant?: 'default' | 'card';
    cardHeight?: number;
};

type CardSkeletonProps = {
    cardHeight: number;
};

export const CardSkeleton = ({ cardHeight }: CardSkeletonProps) => (
    <Card variant="outlined" sx={{ width: '100%' }}>
        <MuiSkeleton variant="rectangular" height={cardHeight} />
        <CardContent>
            <Stack spacing={1.25}>
                <MuiSkeleton variant="text" width="65%" height={32} />
                <MuiSkeleton variant="text" width="90%" />
                <MuiSkeleton variant="text" width="75%" />
            </Stack>
        </CardContent>
    </Card>
);

export const Skeleton = ({ variant = 'default', cardHeight = 160 }: SkeletonProps) => {
    if (variant === 'card') {
        return <CardSkeleton cardHeight={cardHeight} />;
    }

    return (
        <Stack spacing={2} sx={{ width: '100%' }}>
            <MuiSkeleton variant="text" width="60%" />
            <MuiSkeleton variant="text" width="40%" />
            <MuiSkeleton variant="rectangular" height={150} />
        </Stack>
    );
};

