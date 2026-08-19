import { useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import { CollapsibleHeader } from '../../components/CollapsibleHeader/CollapsibleHeader';

const SCROLL_THRESHOLD = 60;

export function CollapsibleHeaderExamplePage() {
  const navigate = useNavigate();
  const scrollRef = useRef<HTMLDivElement>(null);
  const [collapsed, setCollapsed] = useState(false);

  function handleScroll() {
    const el = scrollRef.current;
    if (!el) return;
    setCollapsed(el.scrollTop > SCROLL_THRESHOLD);
  }

  return (
    <Box
      ref={scrollRef}
      onScroll={handleScroll}
      sx={{ height: '100dvh', overflowY: 'auto', display: 'flex', flexDirection: 'column' }}
    >
      <CollapsibleHeader
        title="Rutina de fuerza"
        imageUrl="https://images.unsplash.com/photo-1534438327276-14e5300c3a48?w=800"
        height={260}
        collapsed={collapsed}
        onBack={() => navigate(-1)}
        onMenu={() => console.log('menu')}
      >
        <Box sx={{ p: 2 }}>
          {Array.from({ length: 20 }, (_, i) => (
            <Typography key={i} sx={{ py: 1.5, borderBottom: '1px solid', borderColor: 'divider' }}>
              Ejercicio {i + 1}
            </Typography>
          ))}
        </Box>
      </CollapsibleHeader>
    </Box>
  );
}
