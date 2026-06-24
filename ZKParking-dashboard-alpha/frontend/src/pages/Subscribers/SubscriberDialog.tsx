import React, { useState, useEffect } from 'react'
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  Grid,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  CircularProgress,
} from '@mui/material'
import type { Subscriber, SubscriptionType } from '../../types'

interface SubscriberDialogProps {
  open: boolean
  subscriber: Subscriber | null
  onClose: () => void
  onSave: (data: Partial<Subscriber>) => Promise<void>
}

const EMPTY: Partial<Subscriber> = {
  firstName: '',
  lastName: '',
  email: '',
  phone: '',
  plate: '',
  subscriptionType: 'Mensuel',
  startDate: new Date().toISOString().split('T')[0],
  endDate: '',
  status: 'Actif',
}

export default function SubscriberDialog({ open, subscriber, onClose, onSave }: SubscriberDialogProps) {
  const [form, setForm] = useState<Partial<Subscriber>>(EMPTY)
  const [saving, setSaving] = useState(false)
  const [errors, setErrors] = useState<Record<string, string>>({})

  useEffect(() => {
    if (subscriber) {
      setForm({
        ...subscriber,
        startDate: subscriber.startDate?.split('T')[0],
        endDate: subscriber.endDate?.split('T')[0],
      })
    } else {
      setForm(EMPTY)
    }
    setErrors({})
  }, [subscriber, open])

  const validate = () => {
    const e: Record<string, string> = {}
    if (!form.firstName?.trim()) e.firstName = 'Prénom requis'
    if (!form.lastName?.trim()) e.lastName = 'Nom requis'
    if (!form.phone?.trim()) e.phone = 'Téléphone requis'
    if (!form.plate?.trim()) e.plate = 'Plaque requise'
    if (!form.startDate) e.startDate = 'Date de début requise'
    if (!form.endDate) e.endDate = 'Date de fin requise'
    setErrors(e)
    return Object.keys(e).length === 0
  }

  const handleChange = (field: keyof Subscriber, value: string) => {
    setForm((prev) => ({ ...prev, [field]: value }))
    if (errors[field]) setErrors((prev) => ({ ...prev, [field]: '' }))
  }

  const handleSave = async () => {
    if (!validate()) return
    setSaving(true)
    try {
      await onSave(form)
      onClose()
    } finally {
      setSaving(false)
    }
  }

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>
        {subscriber ? 'Modifier l\'abonné' : 'Nouvel abonné'}
      </DialogTitle>
      <DialogContent dividers>
        <Grid container spacing={2} sx={{ pt: 1 }}>
          <Grid item xs={6}>
            <TextField
              fullWidth
              label="Prénom"
              value={form.firstName || ''}
              onChange={(e) => handleChange('firstName', e.target.value)}
              error={!!errors.firstName}
              helperText={errors.firstName}
              size="small"
            />
          </Grid>
          <Grid item xs={6}>
            <TextField
              fullWidth
              label="Nom"
              value={form.lastName || ''}
              onChange={(e) => handleChange('lastName', e.target.value)}
              error={!!errors.lastName}
              helperText={errors.lastName}
              size="small"
            />
          </Grid>
          <Grid item xs={6}>
            <TextField
              fullWidth
              label="Email"
              type="email"
              value={form.email || ''}
              onChange={(e) => handleChange('email', e.target.value)}
              size="small"
            />
          </Grid>
          <Grid item xs={6}>
            <TextField
              fullWidth
              label="Téléphone"
              value={form.phone || ''}
              onChange={(e) => handleChange('phone', e.target.value)}
              error={!!errors.phone}
              helperText={errors.phone}
              size="small"
            />
          </Grid>
          <Grid item xs={6}>
            <TextField
              fullWidth
              label="Plaque d'immatriculation"
              value={form.plate || ''}
              onChange={(e) => handleChange('plate', e.target.value.toUpperCase())}
              error={!!errors.plate}
              helperText={errors.plate}
              size="small"
              inputProps={{ style: { textTransform: 'uppercase', fontFamily: 'monospace' } }}
            />
          </Grid>
          <Grid item xs={6}>
            <FormControl fullWidth size="small">
              <InputLabel>Type d'abonnement</InputLabel>
              <Select
                value={form.subscriptionType || 'Mensuel'}
                label="Type d'abonnement"
                onChange={(e) => handleChange('subscriptionType', e.target.value as SubscriptionType)}
              >
                <MenuItem value="Mensuel">Mensuel</MenuItem>
                <MenuItem value="Trimestriel">Trimestriel</MenuItem>
                <MenuItem value="Annuel">Annuel</MenuItem>
              </Select>
            </FormControl>
          </Grid>
          <Grid item xs={6}>
            <TextField
              fullWidth
              label="Date de début"
              type="date"
              value={form.startDate || ''}
              onChange={(e) => handleChange('startDate', e.target.value)}
              error={!!errors.startDate}
              helperText={errors.startDate}
              size="small"
              InputLabelProps={{ shrink: true }}
            />
          </Grid>
          <Grid item xs={6}>
            <TextField
              fullWidth
              label="Date de fin"
              type="date"
              value={form.endDate || ''}
              onChange={(e) => handleChange('endDate', e.target.value)}
              error={!!errors.endDate}
              helperText={errors.endDate}
              size="small"
              InputLabelProps={{ shrink: true }}
            />
          </Grid>
          <Grid item xs={6}>
            <FormControl fullWidth size="small">
              <InputLabel>Statut</InputLabel>
              <Select
                value={form.status || 'Actif'}
                label="Statut"
                onChange={(e) => handleChange('status', e.target.value)}
              >
                <MenuItem value="Actif">Actif</MenuItem>
                <MenuItem value="Expiré">Expiré</MenuItem>
                <MenuItem value="En attente">En attente</MenuItem>
              </Select>
            </FormControl>
          </Grid>
        </Grid>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} disabled={saving}>Annuler</Button>
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
  )
}
