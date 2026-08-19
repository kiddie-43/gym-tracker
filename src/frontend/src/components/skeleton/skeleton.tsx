import { Box, Card, Skeleton as MuiSkeleton, Stack } from '@mui/material';

type SkeletonProps = {
    variant?: 'default' | 'card';
    skeletons?: number;
    cardHeight?: number;
};

type CardSkeletonProps = {
    cardHeight: number;
};

type HeroSkeletonProps = {
    height?: number;
};

export const CardSkeleton = ({ cardHeight }: CardSkeletonProps) => (
    <Card
        variant="outlined"
        sx={{
            position: 'relative',
            width: '100%',
            overflow: 'hidden',
            borderRadius: 3,
            boxShadow: 'none',
        }}
    >
        <Stack
            direction="row"
            spacing={1.5}
            sx={{
                minHeight: cardHeight,
                px: 2,
                py: 2,
                alignItems: 'center',
                justifyContent: 'space-between',
            }}
        >
            <Stack direction="row" spacing={1.5} alignItems="center" sx={{ minWidth: 0, flex: 1 }}>
                <MuiSkeleton variant="circular" width={56} height={56} />

                <Box sx={{ width: 2, alignSelf: 'stretch' }}>
                    <MuiSkeleton variant="rectangular" width="100%" height="100%" />
                </Box>

                <Stack spacing={0.8} sx={{ minWidth: 0, flex: 1 }}>
                    <MuiSkeleton variant="text" width="58%" height={32} />
                    <MuiSkeleton variant="text" width="86%" />
                    <Stack direction="row" spacing={2} sx={{ minWidth: 0, flexWrap: 'wrap' }}>
                        <MuiSkeleton variant="text" width="42%" />
                        <MuiSkeleton variant="text" width="34%" />
                    </Stack>
                    <MuiSkeleton variant="text" width="72%" />
                </Stack>
            </Stack>

            <Stack direction="row" spacing={1} alignItems="center">
                <MuiSkeleton variant="circular" width={36} height={36} />
                <MuiSkeleton variant="circular" width={36} height={36} />
            </Stack>
        </Stack>
    </Card>
);

export const HeroSkeleton = ({ height = 260 }: HeroSkeletonProps) => (
    <Card
        variant="outlined"
        sx={{
            position: 'relative',
            width: '100%',
            overflow: 'hidden',
            borderRadius: 0,
            boxShadow: 'none',
        }}
    >
        <Box
            sx={{
                minHeight: height,
                px: { xs: 2, sm: 3 },
                py: { xs: 2.5, sm: 3 },
                display: 'flex',
                alignItems: 'flex-end',
                background: 'linear-gradient(180deg, rgba(14, 17, 24, 0.12) 0%, rgba(14, 17, 24, 0.88) 100%)',
            }}
        >
            <Stack spacing={1.1} sx={{ width: { xs: '100%', sm: '65%' } }}>
                <MuiSkeleton variant="text" width="22%" height={24} />
                <MuiSkeleton variant="text" width="58%" height={56} />
                <MuiSkeleton variant="text" width="82%" height={32} />
            </Stack>
        </Box>
    </Card>
);

export const Skeleton = ({ variant = 'default', skeletons = 3, cardHeight = 160 }: SkeletonProps) => {
    if (variant === 'card') {
        return <CardSkeleton cardHeight={cardHeight} />;
    }

    return (
        <Stack spacing={2} sx={{ width: '100%' }}>
            {Array.from({ length: skeletons }).map((_, index) => (
                <MuiSkeleton key={`skeleton-${index}`} variant="text" width="60%" />
            ))}
        </Stack>
    );
};

