import React, { useState, useEffect, useCallback } from 'react'
import {
  Box,
  Typography,
  Card,
  CardContent,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TablePagination,
  Chip,
  IconButton,
  Tooltip,
  TextField,
  InputAdornment,
  Button,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Grid,
  Skeleton,
  Alert,
  Stack,
} from '@mui/material'
import SearchIcon from '@mui/icons-material/Search'
import VisibilityIcon from '@mui/icons-material/Visibility'
import DownloadIcon from '@mui/icons-material/Download'
import FilterListIcon from '@mui/icons-material/FilterList'
import ClearIcon from '@mui/icons-material/Clear'
import { ticketsApi } from '../../services/api'
import type { ParkingTicket, TicketStatus } from '../../types'
import { formatDate, formatDuration, formatCurrency, formatPlate } from '../../utils/formatters'

const STATUS_COLORS: Record<TicketStatus, 'success' | 'info' | 'error' | 'warning'> = {
  'Payé': 'success',
  'En cours': 'info',
  'Perdu': 'error',
  'Illisible': 'warning',
}

export default function TicketsPage() {
  const [tickets, setTickets] = useState<ParkingTicket[]>([])
  const [total, setTotal] = useState(0)
  const [page, setPage] = useState(0)
  const [rowsPerPage, setRowsPerPage] = useState(20)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [selected, setSelected] = useState<ParkingTicket | null>(null)
  const [detailOpen, setDetailOpen] = useState(false)

  // Filters
  const [search, setSearch] = useState('')
  const [statusFilter, setStatusFilter] = useState('')
  const [typeFilter, setTypeFilter] = useState('')

  const fetchTickets = useCallback(async () => {
    setLoading(true)
    try {
      const data = await ticketsApi.getAll({
        page: page + 1,
        pageSize: rowsPerPage,
        plate: search || undefined,
        status: statusFilter || undefined,
        type: typeFilter || undefined,
      })
      setTickets(data.items)
      setTotal(data.total)
      setError(null)
    } catch {
      setError('Erreur lors du chargement des tickets')
    } finally {
      setLoading(false)
    }
  }, [page, rowsPerPage, search, statusFilter, typeFilter])

  useEffect(() => { fetchTickets() }, [fetchTickets])

  const handleExport = () => {
    const csv = [
      ['N° Ticket', 'Date entrée', 'Date sortie', 'Durée', 'Plaque', 'Type', 'Montant', 'Statut'],
      ...tickets.map((t) => [
        t.ticketNumber,
        formatDate(t.entryDate),
        formatDate(t.exitDate),
        formatDuration(t.duration),
        formatPlate(t.plate),
        t.type,
        formatCurrency(t.amount),
        t.status,
      ]),
    ]
      .map((row) => row.join(';'))
      .join('\n')
    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' })
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `tickets_${new Date().toISOString().split('T')[0]}.csv`
    a.click()
    URL.revokeObjectURL(url)
  }

  const clearFilters = () => {
    setSearch('')
    setStatusFilter('')
    setTypeFilter('')
    setPage(0)
  }

  return (
    <Box>
      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 3 }}>
        <Box>
          <Typography variant="h5" fontWeight={700}>Tickets de stationnement</Typography>
          <Typography variant="body2" color="text.secondary">
            Gestion et suivi des tickets
          </Typography>
        </Box>
        <Button
          variant="contained"
          startIcon={<DownloadIcon />}
          onClick={handleExport}
          sx={{ borderRadius: 2 }}
        >
          Exporter CSV
        </Button>
      </Box>

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

      {/* Filters */}
      <Card sx={{ mb: 2 }}>
        <CardContent sx={{ py: 2 }}>
          <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} alignItems="center">
            <FilterListIcon color="action" />
            <TextField
              size="small"
              placeholder="Rechercher par plaque..."
              value={search}
              onChange={(e) => { setSearch(e.target.value); setPage(0) }}
              InputProps={{
                startAdornment: <InputAdornment position="start"><SearchIcon fontSize="small" /></InputAdornment>,
              }}
              sx={{ minWidth: 220 }}
            />
            <FormControl size="small" sx={{ minWidth: 150 }}>
              <InputLabel>Statut</InputLabel>
              <Select value={statusFilter} label="Statut" onChange={(e) => { setStatusFilter(e.target.value); setPage(0) }}>
                <MenuItem value="">Tous</MenuItem>
                <MenuItem value="Payé">Payé</MenuItem>
                <MenuItem value="En cours">En cours</MenuItem>
                <MenuItem value="Perdu">Perdu</MenuItem>
                <MenuItem value="Illisible">Illisible</MenuItem>
              </Select>
            </FormControl>
            <FormControl size="small" sx={{ minWidth: 150 }}>
              <InputLabel>Type</InputLabel>
              <Select value={typeFilter} label="Type" onChange={(e) => { setTypeFilter(e.target.value); setPage(0) }}>
                <MenuItem value="">Tous</MenuItem>
                <MenuItem value="Horaire">Horaire</MenuItem>
                <MenuItem value="Abonnement">Abonnement</MenuItem>
                <MenuItem value="Journalier">Journalier</MenuItem>
              </Select>
            </FormControl>
            {(search || statusFilter || typeFilter) && (
              <Tooltip title="Effacer les filtres">
                <IconButton size="small" onClick={clearFilters}><ClearIcon /></IconButton>
              </Tooltip>
            )}
            <Box sx={{ flexGrow: 1 }} />
            <Typography variant="body2" color="text.secondary">
              {total} ticket{total > 1 ? 's' : ''}
            </Typography>
          </Stack>
        </CardContent>
      </Card>

      {/* Table */}
      <Card>
        <TableContainer>
          <Table size="small">
            <TableHead>
              <TableRow sx={{ '& th': { fontWeight: 700, bgcolor: 'action.hover' } }}>
                <TableCell>N° Ticket</TableCell>
                <TableCell>Date entrée</TableCell>
                <TableCell>Date sortie</TableCell>
                <TableCell>Durée</TableCell>
                <TableCell>Plaque</TableCell>
                <TableCell>Type</TableCell>
                <TableCell align="right">Montant</TableCell>
                <TableCell>Statut</TableCell>
                <TableCell align="center">Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {loading
                ? Array.from({ length: 8 }).map((_, i) => (
                    <TableRow key={i}>
                      {Array.from({ length: 9 }).map((_, j) => (
                        <TableCell key={j}><Skeleton /></TableCell>
                      ))}
                    </TableRow>
                  ))
                : tickets.map((ticket) => (
                    <TableRow
                      key={ticket.id}
                      hover
                      sx={{ cursor: 'pointer' }}
                      onClick={() => { setSelected(ticket); setDetailOpen(true) }}
                    >
                      <TableCell>
                        <Typography variant="body2" fontWeight={600} color="primary.main">
                          {ticket.ticketNumber}
                        </Typography>
                      </TableCell>
                      <TableCell>{formatDate(ticket.entryDate)}</TableCell>
                      <TableCell>{formatDate(ticket.exitDate)}</TableCell>
                      <TableCell>{formatDuration(ticket.duration)}</TableCell>
                      <TableCell>
                        <Typography variant="body2" fontFamily="monospace" fontWeight={600}>
                          {formatPlate(ticket.plate)}
                        </Typography>
                      </TableCell>
                      <TableCell>{ticket.type}</TableCell>
                      <TableCell align="right">
                        <Typography variant="body2" fontWeight={600}>
                          {formatCurrency(ticket.amount)}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Chip
                          label={ticket.status}
                          color={STATUS_COLORS[ticket.status]}
                          size="small"
                        />
                      </TableCell>
                      <TableCell align="center" onClick={(e) => e.stopPropagation()}>
                        <Tooltip title="Voir détails">
                          <IconButton
                            size="small"
                            onClick={() => { setSelected(ticket); setDetailOpen(true) }}
                          >
                            <VisibilityIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                      </TableCell>
                    </TableRow>
                  ))}
            </TableBody>
          </Table>
        </TableContainer>
        <TablePagination
          component="div"
          count={total}
          page={page}
          onPageChange={(_, p) => setPage(p)}
          rowsPerPage={rowsPerPage}
          onRowsPerPageChange={(e) => { setRowsPerPage(parseInt(e.target.value)); setPage(0) }}
          rowsPerPageOptions={[10, 20, 50]}
          labelRowsPerPage="Lignes par page :"
          labelDisplayedRows={({ from, to, count }) => `${from}–${to} sur ${count}`}
        />
      </Card>

      {/* Detail dialog */}
      <Dialog open={detailOpen} onClose={() => setDetailOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>
          Détail du ticket — {selected?.ticketNumber}
        </DialogTitle>
        <DialogContent dividers>
          {selected && (
            <Grid container spacing={2}>
              {[
                { label: 'N° Ticket', value: selected.ticketNumber },
                { label: 'Plaque', value: formatPlate(selected.plate) },
                { label: 'Type', value: selected.type },
                { label: 'Statut', value: selected.status },
                { label: 'Date entrée', value: formatDate(selected.entryDate) },
                { label: 'Date sortie', value: formatDate(selected.exitDate) },
                { label: 'Durée', value: formatDuration(selected.duration) },
                { label: 'Montant', value: formatCurrency(selected.amount) },
              ].map((item) => (
                <Grid item xs={6} key={item.label}>
                  <Typography variant="caption" color="text.secondary">{item.label}</Typography>
                  <Typography variant="body1" fontWeight={600}>{item.value}</Typography>
                </Grid>
              ))}
            </Grid>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDetailOpen(false)}>Fermer</Button>
        </DialogActions>
      </Dialog>
    </Box>
  )
}
