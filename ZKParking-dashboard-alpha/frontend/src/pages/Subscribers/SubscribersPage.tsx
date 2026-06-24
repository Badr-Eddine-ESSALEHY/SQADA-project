import React, { useState, useEffect, useCallback } from 'react'
import {
  Box,
  Typography,
  Card,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Chip,
  IconButton,
  Tooltip,
  TextField,
  InputAdornment,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Alert,
  Badge,
  Skeleton,
  Stack,
} from '@mui/material'
import AddIcon from '@mui/icons-material/Add'
import EditIcon from '@mui/icons-material/Edit'
import DeleteIcon from '@mui/icons-material/Delete'
import SearchIcon from '@mui/icons-material/Search'
import AutorenewIcon from '@mui/icons-material/Autorenew'
import WarningAmberIcon from '@mui/icons-material/WarningAmber'
import { subscribersApi } from '../../services/api'
import type { Subscriber, SubscriberStatus } from '../../types'
import { formatShortDate, formatPlate } from '../../utils/formatters'
import SubscriberDialog from './SubscriberDialog'
import { differenceInDays, parseISO } from 'date-fns'

const STATUS_COLORS: Record<SubscriberStatus, 'success' | 'error' | 'warning'> = {
  'Actif': 'success',
  'Expiré': 'error',
  'En attente': 'warning',
}

export default function SubscribersPage() {
  const [subscribers, setSubscribers] = useState<Subscriber[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [search, setSearch] = useState('')
  const [dialogOpen, setDialogOpen] = useState(false)
  const [editTarget, setEditTarget] = useState<Subscriber | null>(null)
  const [deleteTarget, setDeleteTarget] = useState<Subscriber | null>(null)
  const [deleteOpen, setDeleteOpen] = useState(false)

  const fetchSubscribers = useCallback(async () => {
    setLoading(true)
    try {
      const data = await subscribersApi.getAll()
      setSubscribers(data)
      setError(null)
    } catch {
      setError('Erreur lors du chargement des abonnés')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => { fetchSubscribers() }, [fetchSubscribers])

  const filtered = subscribers.filter((s) => {
    const q = search.toLowerCase()
    return (
      s.firstName.toLowerCase().includes(q) ||
      s.lastName.toLowerCase().includes(q) ||
      s.plate.toLowerCase().includes(q) ||
      s.phone.includes(q)
    )
  })

  const expiringSoon = subscribers.filter((s) => {
    if (s.status !== 'Actif') return false
    const days = differenceInDays(parseISO(s.endDate), new Date())
    return days >= 0 && days <= 30
  }).length

  const handleSave = async (data: Partial<Subscriber>) => {
    if (editTarget) {
      await subscribersApi.update(editTarget.id, data as Record<string, unknown>)
    } else {
      await subscribersApi.create(data as Record<string, unknown>)
    }
    await fetchSubscribers()
  }

  const handleDelete = async () => {
    if (!deleteTarget) return
    try {
      await subscribersApi.delete(deleteTarget.id)
      await fetchSubscribers()
    } catch {
      // Mock: just remove locally
      setSubscribers((prev) => prev.filter((s) => s.id !== deleteTarget.id))
    }
    setDeleteOpen(false)
    setDeleteTarget(null)
  }

  const handleRenew = async (sub: Subscriber) => {
    try {
      await subscribersApi.renew(sub.id, 1)
      await fetchSubscribers()
    } catch {
      // Mock: update locally
      setSubscribers((prev) =>
        prev.map((s) =>
          s.id === sub.id
            ? {
                ...s,
                endDate: new Date(
                  new Date(s.endDate).getTime() + 30 * 86400000,
                ).toISOString(),
                status: 'Actif',
              }
            : s,
        ),
      )
    }
  }

  return (
    <Box>
      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 3 }}>
        <Box>
          <Typography variant="h5" fontWeight={700}>Abonnés</Typography>
          <Typography variant="body2" color="text.secondary">
            Gestion des abonnements de stationnement
          </Typography>
        </Box>
        <Stack direction="row" spacing={1}>
          {expiringSoon > 0 && (
            <Chip
              icon={<WarningAmberIcon />}
              label={`${expiringSoon} expirant bientôt`}
              color="warning"
              variant="outlined"
            />
          )}
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={() => { setEditTarget(null); setDialogOpen(true) }}
            sx={{ borderRadius: 2 }}
          >
            Nouvel abonné
          </Button>
        </Stack>
      </Box>

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

      {/* Search */}
      <Card sx={{ mb: 2, p: 2 }}>
        <TextField
          size="small"
          placeholder="Rechercher par nom, plaque, téléphone..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          InputProps={{
            startAdornment: <InputAdornment position="start"><SearchIcon fontSize="small" /></InputAdornment>,
          }}
          sx={{ width: { xs: '100%', sm: 360 } }}
        />
        <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
          {filtered.length} abonné{filtered.length > 1 ? 's' : ''}
        </Typography>
      </Card>

      <Card>
        <TableContainer>
          <Table size="small">
            <TableHead>
              <TableRow sx={{ '& th': { fontWeight: 700, bgcolor: 'action.hover' } }}>
                <TableCell>Nom</TableCell>
                <TableCell>Téléphone</TableCell>
                <TableCell>Plaque</TableCell>
                <TableCell>Type</TableCell>
                <TableCell>Début</TableCell>
                <TableCell>Fin</TableCell>
                <TableCell>Statut</TableCell>
                <TableCell align="center">Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {loading
                ? Array.from({ length: 6 }).map((_, i) => (
                    <TableRow key={i}>
                      {Array.from({ length: 8 }).map((_, j) => (
                        <TableCell key={j}><Skeleton /></TableCell>
                      ))}
                    </TableRow>
                  ))
                : filtered.map((sub) => {
                    const daysLeft = differenceInDays(parseISO(sub.endDate), new Date())
                    const expiring = sub.status === 'Actif' && daysLeft >= 0 && daysLeft <= 30
                    return (
                      <TableRow key={sub.id} hover>
                        <TableCell>
                          <Typography variant="body2" fontWeight={600}>
                            {sub.firstName} {sub.lastName}
                          </Typography>
                          <Typography variant="caption" color="text.secondary">{sub.email}</Typography>
                        </TableCell>
                        <TableCell>{sub.phone}</TableCell>
                        <TableCell>
                          <Typography variant="body2" fontFamily="monospace" fontWeight={600}>
                            {formatPlate(sub.plate)}
                          </Typography>
                        </TableCell>
                        <TableCell>{sub.subscriptionType}</TableCell>
                        <TableCell>{formatShortDate(sub.startDate)}</TableCell>
                        <TableCell>
                          <Badge
                            badgeContent={expiring ? `J-${daysLeft}` : undefined}
                            color="warning"
                          >
                            <Typography variant="body2">{formatShortDate(sub.endDate)}</Typography>
                          </Badge>
                        </TableCell>
                        <TableCell>
                          <Chip
                            label={sub.status}
                            color={STATUS_COLORS[sub.status]}
                            size="small"
                          />
                        </TableCell>
                        <TableCell align="center">
                          <Stack direction="row" spacing={0.5} justifyContent="center">
                            <Tooltip title="Renouveler">
                              <IconButton size="small" color="success" onClick={() => handleRenew(sub)}>
                                <AutorenewIcon fontSize="small" />
                              </IconButton>
                            </Tooltip>
                            <Tooltip title="Modifier">
                              <IconButton size="small" onClick={() => { setEditTarget(sub); setDialogOpen(true) }}>
                                <EditIcon fontSize="small" />
                              </IconButton>
                            </Tooltip>
                            <Tooltip title="Supprimer">
                              <IconButton
                                size="small"
                                color="error"
                                onClick={() => { setDeleteTarget(sub); setDeleteOpen(true) }}
                              >
                                <DeleteIcon fontSize="small" />
                              </IconButton>
                            </Tooltip>
                          </Stack>
                        </TableCell>
                      </TableRow>
                    )
                  })}
            </TableBody>
          </Table>
        </TableContainer>
      </Card>

      {/* Add/Edit dialog */}
      <SubscriberDialog
        open={dialogOpen}
        subscriber={editTarget}
        onClose={() => setDialogOpen(false)}
        onSave={handleSave}
      />

      {/* Delete confirmation */}
      <Dialog open={deleteOpen} onClose={() => setDeleteOpen(false)} maxWidth="xs" fullWidth>
        <DialogTitle>Confirmer la suppression</DialogTitle>
        <DialogContent>
          <Typography>
            Voulez-vous vraiment supprimer l'abonné{' '}
            <strong>{deleteTarget?.firstName} {deleteTarget?.lastName}</strong> ?
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteOpen(false)}>Annuler</Button>
          <Button variant="contained" color="error" onClick={handleDelete}>
            Supprimer
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  )
}
