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
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Grid,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Alert,
  Skeleton,
  Stack,
  CircularProgress,
  Avatar,
} from '@mui/material'
import AddIcon from '@mui/icons-material/Add'
import EditIcon from '@mui/icons-material/Edit'
import DeleteIcon from '@mui/icons-material/Delete'
import { usersApi } from '../../services/api'
import type { AppUser, UserRole, UserStatus } from '../../types'
import { formatDate } from '../../utils/formatters'

const ROLE_COLORS: Record<UserRole, 'error' | 'warning' | 'info' | 'default'> = {
  'Administrateur': 'error',
  'Gestionnaire': 'warning',
  'Opérateur': 'info',
  'Lecture seule': 'default',
}

const ROLE_AVATARS: Record<UserRole, string> = {
  'Administrateur': '#C62828',
  'Gestionnaire': '#E65100',
  'Opérateur': '#0277BD',
  'Lecture seule': '#546E7A',
}

const EMPTY_USER: Partial<AppUser> = {
  firstName: '',
  lastName: '',
  email: '',
  role: 'Opérateur',
  status: 'Actif',
}

export default function UsersPage() {
  const [users, setUsers] = useState<AppUser[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [dialogOpen, setDialogOpen] = useState(false)
  const [editTarget, setEditTarget] = useState<AppUser | null>(null)
  const [deleteTarget, setDeleteTarget] = useState<AppUser | null>(null)
  const [deleteOpen, setDeleteOpen] = useState(false)
  const [form, setForm] = useState<Partial<AppUser>>(EMPTY_USER)
  const [saving, setSaving] = useState(false)
  const [formErrors, setFormErrors] = useState<Record<string, string>>({})

  const fetchUsers = useCallback(async () => {
    setLoading(true)
    try {
      const data = await usersApi.getAll()
      setUsers(data)
      setError(null)
    } catch {
      setError('Erreur lors du chargement des utilisateurs')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => { fetchUsers() }, [fetchUsers])

  const openDialog = (user: AppUser | null) => {
    setEditTarget(user)
    setForm(user ? { ...user } : EMPTY_USER)
    setFormErrors({})
    setDialogOpen(true)
  }

  const validate = () => {
    const e: Record<string, string> = {}
    if (!form.firstName?.trim()) e.firstName = 'Prénom requis'
    if (!form.lastName?.trim()) e.lastName = 'Nom requis'
    if (!form.email?.trim()) e.email = 'Email requis'
    else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email)) e.email = 'Email invalide'
    setFormErrors(e)
    return Object.keys(e).length === 0
  }

  const handleSave = async () => {
    if (!validate()) return
    setSaving(true)
    try {
      if (editTarget) {
        await usersApi.update(editTarget.id, form as Record<string, unknown>)
      } else {
        await usersApi.create(form as Record<string, unknown>)
      }
      await fetchUsers()
      setDialogOpen(false)
    } catch {
      // Mock: update locally
      if (editTarget) {
        setUsers((prev) => prev.map((u) => (u.id === editTarget.id ? { ...u, ...form } as AppUser : u)))
      } else {
        const newUser: AppUser = {
          id: `U${Date.now()}`,
          firstName: form.firstName || '',
          lastName: form.lastName || '',
          email: form.email || '',
          role: form.role || 'Opérateur',
          status: form.status || 'Actif',
          lastLogin: new Date().toISOString(),
        }
        setUsers((prev) => [...prev, newUser])
      }
      setDialogOpen(false)
    } finally {
      setSaving(false)
    }
  }

  const handleDelete = async () => {
    if (!deleteTarget) return
    try {
      await usersApi.delete(deleteTarget.id)
    } catch {}
    setUsers((prev) => prev.filter((u) => u.id !== deleteTarget.id))
    setDeleteOpen(false)
    setDeleteTarget(null)
  }

  return (
    <Box>
      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 3 }}>
        <Box>
          <Typography variant="h5" fontWeight={700}>Utilisateurs</Typography>
          <Typography variant="body2" color="text.secondary">
            Gestion des comptes et des accès
          </Typography>
        </Box>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={() => openDialog(null)}
          sx={{ borderRadius: 2 }}
        >
          Nouvel utilisateur
        </Button>
      </Box>

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

      <Card>
        <TableContainer>
          <Table size="small">
            <TableHead>
              <TableRow sx={{ '& th': { fontWeight: 700, bgcolor: 'action.hover' } }}>
                <TableCell>Utilisateur</TableCell>
                <TableCell>Email</TableCell>
                <TableCell>Rôle</TableCell>
                <TableCell>Statut</TableCell>
                <TableCell>Dernière connexion</TableCell>
                <TableCell align="center">Actions</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {loading
                ? Array.from({ length: 5 }).map((_, i) => (
                    <TableRow key={i}>
                      {Array.from({ length: 6 }).map((_, j) => (
                        <TableCell key={j}><Skeleton /></TableCell>
                      ))}
                    </TableRow>
                  ))
                : users.map((user) => (
                    <TableRow key={user.id} hover>
                      <TableCell>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
                          <Avatar
                            sx={{
                              width: 32,
                              height: 32,
                              fontSize: 13,
                              bgcolor: ROLE_AVATARS[user.role],
                            }}
                          >
                            {user.firstName.charAt(0)}{user.lastName.charAt(0)}
                          </Avatar>
                          <Box>
                            <Typography variant="body2" fontWeight={600}>
                              {user.firstName} {user.lastName}
                            </Typography>
                          </Box>
                        </Box>
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2" color="text.secondary">{user.email}</Typography>
                      </TableCell>
                      <TableCell>
                        <Chip
                          label={user.role}
                          color={ROLE_COLORS[user.role]}
                          size="small"
                          variant="outlined"
                        />
                      </TableCell>
                      <TableCell>
                        <Chip
                          label={user.status}
                          color={user.status === 'Actif' ? 'success' : 'default'}
                          size="small"
                        />
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2" color="text.secondary">
                          {formatDate(user.lastLogin)}
                        </Typography>
                      </TableCell>
                      <TableCell align="center">
                        <Stack direction="row" spacing={0.5} justifyContent="center">
                          <Tooltip title="Modifier">
                            <IconButton size="small" onClick={() => openDialog(user)}>
                              <EditIcon fontSize="small" />
                            </IconButton>
                          </Tooltip>
                          <Tooltip title="Supprimer">
                            <IconButton
                              size="small"
                              color="error"
                              onClick={() => { setDeleteTarget(user); setDeleteOpen(true) }}
                            >
                              <DeleteIcon fontSize="small" />
                            </IconButton>
                          </Tooltip>
                        </Stack>
                      </TableCell>
                    </TableRow>
                  ))}
            </TableBody>
          </Table>
        </TableContainer>
      </Card>

      {/* Add/Edit dialog */}
      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>{editTarget ? 'Modifier l\'utilisateur' : 'Nouvel utilisateur'}</DialogTitle>
        <DialogContent dividers>
          <Grid container spacing={2} sx={{ pt: 1 }}>
            <Grid item xs={6}>
              <TextField
                fullWidth size="small" label="Prénom"
                value={form.firstName || ''}
                onChange={(e) => setForm((p) => ({ ...p, firstName: e.target.value }))}
                error={!!formErrors.firstName} helperText={formErrors.firstName}
              />
            </Grid>
            <Grid item xs={6}>
              <TextField
                fullWidth size="small" label="Nom"
                value={form.lastName || ''}
                onChange={(e) => setForm((p) => ({ ...p, lastName: e.target.value }))}
                error={!!formErrors.lastName} helperText={formErrors.lastName}
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth size="small" label="Email" type="email"
                value={form.email || ''}
                onChange={(e) => setForm((p) => ({ ...p, email: e.target.value }))}
                error={!!formErrors.email} helperText={formErrors.email}
              />
            </Grid>
            <Grid item xs={6}>
              <FormControl fullWidth size="small">
                <InputLabel>Rôle</InputLabel>
                <Select
                  value={form.role || 'Opérateur'}
                  label="Rôle"
                  onChange={(e) => setForm((p) => ({ ...p, role: e.target.value as UserRole }))}
                >
                  <MenuItem value="Administrateur">Administrateur</MenuItem>
                  <MenuItem value="Gestionnaire">Gestionnaire</MenuItem>
                  <MenuItem value="Opérateur">Opérateur</MenuItem>
                  <MenuItem value="Lecture seule">Lecture seule</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={6}>
              <FormControl fullWidth size="small">
                <InputLabel>Statut</InputLabel>
                <Select
                  value={form.status || 'Actif'}
                  label="Statut"
                  onChange={(e) => setForm((p) => ({ ...p, status: e.target.value as UserStatus }))}
                >
                  <MenuItem value="Actif">Actif</MenuItem>
                  <MenuItem value="Inactif">Inactif</MenuItem>
                </Select>
              </FormControl>
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDialogOpen(false)} disabled={saving}>Annuler</Button>
          <Button
            variant="contained"
            onClick={handleSave}
            disabled={saving}
            startIcon={saving ? <CircularProgress size={16} /> : undefined}
          >
            {saving ? 'Enregistrement...' : 'Enregistrer'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Delete confirmation */}
      <Dialog open={deleteOpen} onClose={() => setDeleteOpen(false)} maxWidth="xs" fullWidth>
        <DialogTitle>Confirmer la suppression</DialogTitle>
        <DialogContent>
          <Typography>
            Voulez-vous vraiment supprimer l'utilisateur{' '}
            <strong>{deleteTarget?.firstName} {deleteTarget?.lastName}</strong> ?
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteOpen(false)}>Annuler</Button>
          <Button variant="contained" color="error" onClick={handleDelete}>Supprimer</Button>
        </DialogActions>
      </Dialog>
    </Box>
  )
}
