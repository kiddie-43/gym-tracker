import type { ChangeEvent, ReactNode } from 'react';

import Paper from '@mui/material/Paper';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TablePagination from '@mui/material/TablePagination';

type DataTableProps = {
  ariaLabel: string;
  columnWidths: string[];
  head: ReactNode;
  body: ReactNode;
  pagination: {
    count: number;
    page: number;
    rowsPerPage: number;
    onPageChange: (_event: unknown, newPage: number) => void;
    onRowsPerPageChange: (event: ChangeEvent<HTMLInputElement>) => void;
    rowsPerPageOptions?: number[];
  };
  size?: 'small' | 'medium';
};

export function DataTable({
  ariaLabel,
  columnWidths,
  head,
  body,
  pagination,
  size = 'medium',
}: DataTableProps) {
  return (
    <Paper sx={{ overflow: 'hidden', display: 'flex', flexDirection: 'column', flex: 1 }}>
      <Table aria-label={ariaLabel} size={size}>
        <colgroup>
          {columnWidths.map((width) => (
            <col key={width} style={{ width }} />
          ))}
        </colgroup>
        <TableHead>{head}</TableHead>
      </Table>

      <TableContainer sx={{ flex: 1, overflow: 'auto' }}>
        <Table aria-label={ariaLabel} size={size}>
          <colgroup>
            {columnWidths.map((width) => (
              <col key={width} style={{ width }} />
            ))}
          </colgroup>
          <TableBody>{body}</TableBody>
        </Table>
      </TableContainer>

      <TablePagination
        component="div"
        count={pagination.count}
        page={pagination.page}
        rowsPerPage={pagination.rowsPerPage}
        onPageChange={pagination.onPageChange}
        onRowsPerPageChange={pagination.onRowsPerPageChange}
        rowsPerPageOptions={pagination.rowsPerPageOptions ?? [10, 25, 50]}
        sx={{
          borderTop: 1,
          borderColor: 'divider',
          bgcolor: 'background.paper',
          '& .MuiTablePagination-toolbar': {
            minHeight: 56,
            px: { xs: 1, sm: 2 },
          },
          '& .MuiTablePagination-selectLabel, & .MuiTablePagination-displayedRows': {
            mb: 0,
          },
        }}
      />
    </Paper>
  );
}
