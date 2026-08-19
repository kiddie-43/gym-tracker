import { type ReactNode, type UIEvent, useEffect, useRef, useState } from 'react';
import Box from '@mui/material/Box';
import IconButton from '@mui/material/IconButton';
import Typography from '@mui/material/Typography';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import MoreVertRoundedIcon from '@mui/icons-material/MoreVertRounded';

interface CollapsibleHeaderProps {
  title: string;
  imageUrl: string;
  height?: number;
  onBack: () => void;
  onMenu: () => void;
  showTopActions?: boolean;
  children?: ReactNode;
}

export function CollapsibleHeader({
  title,
  imageUrl,
  height = 260,
  onBack,
  onMenu,
  showTopActions = true,
  children,
}: CollapsibleHeaderProps) {
  const frameRef = useRef<number | null>(null);
  const [scrollY, setScrollY] = useState(0);

  // Hero shrinks from expanded height down to a compact bar.
  const minHeight = 56;
  const collapseRange = Math.max(height - minHeight, 1);
  const heroHeight = Math.max(height - scrollY, minHeight);
  const progress = Math.min(scrollY / collapseRange, 1);

  useEffect(() => {
    return () => {
      if (frameRef.current !== null) {
        cancelAnimationFrame(frameRef.current);
      }
    };
  }, []);

  const handleScroll = (e: UIEvent<HTMLDivElement>) => {
    const nextScrollTop = e.currentTarget.scrollTop;

    if (frameRef.current !== null) {
      cancelAnimationFrame(frameRef.current);
    }

    frameRef.current = requestAnimationFrame(() => {
      setScrollY(nextScrollTop);
      frameRef.current = null;
    });
  };

  return (
    <Box sx={{ flex: 1, minHeight: 0, position: 'relative', overflow: 'hidden' }}>
      <Box
        sx={{
          position: 'absolute',
          top: 0,
          left: 0,
          right: 0,
          zIndex: 1,
          height: heroHeight,
          minHeight,
          overflow: 'hidden',
          willChange: 'height',
          pointerEvents: 'none',
          backgroundImage: `url(${imageUrl})`,
          backgroundSize: 'cover',
          backgroundPosition: 'center',
          '&::after': {
            content: '""',
            position: 'absolute',
            inset: 0,
            background: 'linear-gradient(to bottom, rgba(0,0,0,0.5) 0%, rgba(0,0,0,0.2) 50%, rgba(0,0,0,0.6) 100%)',
          },
        }}
      >
        {showTopActions ? (
          <Box
            sx={{
              position: 'absolute',
              top: 0,
              left: 0,
              right: 0,
              zIndex: 1,
              display: 'flex',
              justifyContent: 'space-between',
              alignItems: 'center',
              px: 1,
              pt: 1,
              pointerEvents: 'auto',
            }}
          >
            <IconButton onClick={onBack} sx={{ color: '#fff' }}>
              <ArrowBackIcon />
            </IconButton>
            <IconButton onClick={onMenu} sx={{ color: '#fff' }}>
              <MoreVertRoundedIcon />
            </IconButton>
          </Box>
        ) : null}

        <Typography
          variant="h6"
          noWrap
          sx={{
            position: 'absolute',
            left: 0,
            right: 0,
            zIndex: 1,
            textAlign: 'center',
            fontWeight: 700,
            color: '#fff',
            fontSize: '1rem',
            px: 6,
            top: `${12 + progress * 10}px`,
          }}
        >
          {title}
        </Typography>
      </Box>

      <Box
        onScroll={handleScroll}
        sx={{ height: '100%', overflowY: 'auto', bgcolor: 'background.default' }}
      >
        <Box sx={{ height }} />
        <Box>
          {children}
        </Box>
      </Box>
    </Box>
  );
}

