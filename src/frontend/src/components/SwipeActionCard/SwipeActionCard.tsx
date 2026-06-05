import { type ReactNode, useRef, useState } from 'react';

import Button from '@mui/material/Button';
import Box from '@mui/material/Box';
import type { ButtonProps } from '@mui/material/Button';
import type { Theme } from '@mui/material/styles';
import type { SystemStyleObject } from '@mui/system';

type SwipeActionStyleOverride = SystemStyleObject<Theme> | ((theme: Theme) => SystemStyleObject<Theme>);

interface SwipeActionCardProps {
  children: ReactNode;
  onAction: () => void;
  actionLabel: string;
  actionIcon?: ReactNode;
  onContentClick?: () => void;
  disabled?: boolean;
  actionWidth?: number;
  swipeTrigger?: number;
  actionColor?: ButtonProps['color'];
  actionLaneColor?: 'primary' | 'secondary' | 'success' | 'warning' | 'error' | 'info';
  actionButtonVariant?: ButtonProps['variant'];
  actionButtonSx?: SwipeActionStyleOverride;
  actionLaneSx?: SwipeActionStyleOverride;
}

export function SwipeActionCard({
  children,
  onAction,
  actionLabel,
  actionIcon,
  onContentClick,
  disabled = false,
  actionWidth = 120,
  swipeTrigger = 52,
  actionColor = 'warning',
  actionLaneColor = 'warning',
  actionButtonVariant = 'contained',
  actionButtonSx,
  actionLaneSx,
}: SwipeActionCardProps) {
  const resolveSx = (theme: Theme, sxOverride?: SwipeActionStyleOverride): SystemStyleObject<Theme> => {
    if (!sxOverride) {
      return {};
    }

    return typeof sxOverride === 'function' ? sxOverride(theme) : sxOverride;
  };

  const [swipeOffset, setSwipeOffset] = useState(0);
  const [isDragging, setIsDragging] = useState(false);

  const touchStartXRef = useRef<number | null>(null);
  const touchStartOffsetRef = useRef<number>(0);
  const justSwipedRef = useRef(false);

  const swipeProgress = Math.min(1, Math.abs(swipeOffset) / actionWidth);

  const handleTouchStart = (event: React.TouchEvent) => {
    const startX = event.touches[0]?.clientX;
    touchStartXRef.current = typeof startX === 'number' ? startX : null;
    touchStartOffsetRef.current = swipeOffset;
    setIsDragging(true);
  };

  const handleTouchMove = (event: React.TouchEvent) => {
    if (touchStartXRef.current === null) {
      return;
    }

    const currentX = event.touches[0]?.clientX;
    if (typeof currentX !== 'number') {
      return;
    }

    const deltaX = currentX - touchStartXRef.current;
    const nextOffset = Math.max(-actionWidth, Math.min(0, touchStartOffsetRef.current + deltaX));
    setSwipeOffset(nextOffset);
  };

  const handleTouchEnd = () => {
    const shouldTrigger = Math.abs(swipeOffset) >= swipeTrigger;

    if (shouldTrigger && !disabled) {
      justSwipedRef.current = true;
      onAction();
    }

    setSwipeOffset(0);
    setIsDragging(false);
    touchStartXRef.current = null;
    touchStartOffsetRef.current = 0;
  };

  const handleContentClick = () => {
    if (justSwipedRef.current) {
      justSwipedRef.current = false;
      return;
    }

    onContentClick?.();
  };

  return (
    <Box
      sx={{
        position: 'relative',
        overflow: 'hidden',
        borderRadius: 1,
      }}
    >
      <Box
        sx={(theme) => ({
            position: 'absolute',
            top: 0,
            right: 0,
            bottom: 0,
            width: actionWidth,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            px: 1,
            background: `linear-gradient(160deg, ${theme.palette[actionLaneColor].dark} 0%, ${theme.palette[actionLaneColor].main} 100%)`,
            ...resolveSx(theme, actionLaneSx),
          })}
      >
        <Button
          variant={actionButtonVariant}
          color={actionColor}
          startIcon={actionIcon}
          disabled={disabled}
          onClick={(event) => {
            event.stopPropagation();
            if (!disabled) {
              onAction();
            }
          }}
          sx={(theme) => ({
              minWidth: 0,
              px: 1.5,
              py: 0.85,
              borderRadius: 999,
              fontWeight: 700,
              boxShadow: '0 6px 16px rgba(0,0,0,0.22)',
              transform: `scale(${0.92 + swipeProgress * 0.08})`,
              opacity: 0.75 + swipeProgress * 0.25,
              transition: 'transform 140ms ease, opacity 140ms ease',
              ...resolveSx(theme, actionButtonSx),
            })}
        >
          {actionLabel}
        </Button>
      </Box>

      <Box
        sx={{
          transform: `translateX(${swipeOffset}px)`,
          transition: isDragging ? 'none' : 'transform 220ms cubic-bezier(0.2, 0.75, 0.2, 1)',
          willChange: 'transform',
        }}
        onClick={handleContentClick}
        onTouchStart={handleTouchStart}
        onTouchMove={handleTouchMove}
        onTouchEnd={handleTouchEnd}
        onTouchCancel={handleTouchEnd}
      >
        {children}
      </Box>
    </Box>
  );
}
