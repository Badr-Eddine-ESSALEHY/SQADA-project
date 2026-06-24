import React, { useState, useContext } from 'react'
import {
  Box,
  Typography,
  Card,
  CardContent,
  Grid,
  TextField,
  Switch,
  FormControlLabel,
  Button,
  Divider,
  Alert,
  Slider,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
  Stack,
  Chip,
} from '@mui/material'
import SaveIcon from '@mui/icons-material/Save'
import CheckCircleIcon from '@mui/icons-material/CheckCircle'
import { ColorModeContext } from '../../App'
import { useTheme } from '@mui/material'

interface Settings {
  parkingName: string
  parkingAddress: string
  totalCapacity: number
  currency: string
  hourlyRate: number
  dailyMax: number
  apiUrl: string
  notifyOnAlert: boolean
  notifyOnFull: boolean
  notifyOnExpiry: boolean
  autoRefreshInterval: number
  language: string
}

const DEFAULT_SETTINGS: Settings = {
  parkingName: 'Parking Centre-Ville',
  parkingAddress: '1 Place de la République, 75001 Paris',
  totalCapacity: 150,
  currency: 'EUR',
  hourlyRate: 2.5,
  dailyMax: 20,
  apiUrl: '/api',
  notifyOnAlert: true,
  notifyOnFull: true,
  notifyOnExpiry: true,
  autoRefreshInterval: 30,
  language: 'fr',
}

export default function SettingsPage() {
  const theme = useTheme()
  const colorMode = useContext(ColorModeContext)
  const [settings, setSettings] = useState<Settings>(() => {
    try {
      const stored = localStorage.getItem('parkingSettings')
      return stored ? { ...DEFAULT_SETTINGS, ...JSON.parse(stored) } : DEFAULT_SETTINGS
    } catch {
      return DEFAULT_SETTINGS
    }
  })
  const [saved, setSaved] = useState(false)

  const update = <K extends keyof Settings>(key: K, value: Settings[K]) => {
    setSettings((prev) => ({ ...prev, [key]: value }))
    setSaved(false)
  }

  const handleSave = () => {
    localStorage.setItem('parkingSettings', JSON.stringify(settings))
    setSaved(true)
    setTimeout(() => setSaved(false), 3000)
  }

  return (
    <Box>
      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 3 }}>
        <Box>
          <Typography variant="h5" fontWeight={700}>Paramètres</Typography>
          <Typography variant="body2" color="text.secondary">
            Configuration du système de gestion de parking
          </Typography>
        </Box>
        <Button
          variant="contained"
          startIcon={<SaveIcon />}
          onClick={handleSave}
          sx={{ borderRadius: 2 }}
        >
          Enregistrer
        </Button>
      </Box>

      {saved && (
        <Alert severity="success" icon={<CheckCircleIcon />} sx={{ mb: 2 }}>
          Paramètres enregistrés avec succès
        </Alert>
      )}

      <Grid container spacing={2}>
        {/* Parking configuration */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" fontWeight={600} gutterBottom>
                Configuration du parking
              </Typography>
              <Divider sx={{ mb: 2 }} />
              <Stack spacing={2}>
                <TextField
                  fullWidth size="small" label="Nom du parking"
                  value={settings.parkingName}
                  onChange={(e) => update('parkingName', e.target.value)}
                />
                <TextField
                  fullWidth size="small" label="Adresse"
                  value={settings.parkingAddress}
                  onChange={(e) => update('parkingAddress', e.target.value)}
                />
                <TextField
                  fullWidth size="small" label="Capacité totale" type="number"
                  value={settings.totalCapacity}
                  onChange={(e) => update('totalCapacity', parseInt(e.target.value) || 0)}
                  inputProps={{ min: 1, max: 10000 }}
                />
                <FormControl fullWidth size="small">
                  <InputLabel>Devise</InputLabel>
                  <Select
                    value={settings.currency}
                    label="Devise"
                    onChange={(e) => update('currency', e.target.value)}
                  >
                    <MenuItem value="EUR">Euro (€)</MenuItem>
                    <MenuItem value="USD">Dollar ($)</MenuItem>
                    <MenuItem value="GBP">Livre (£)</MenuItem>
                    <MenuItem value="MAD">Dirham (MAD)</MenuItem>
                  </Select>
                </FormControl>
              </Stack>
            </CardContent>
          </Card>
        </Grid>

        {/* Tarification */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" fontWeight={600} gutterBottom>
                Tarification
              </Typography>
              <Divider sx={{ mb: 2 }} />
              <Stack spacing={2}>
                <TextField
                  fullWidth size="small" label="Tarif horaire (€/h)" type="number"
                  value={settings.hourlyRate}
                  onChange={(e) => update('hourlyRate', parseFloat(e.target.value) || 0)}
                  inputProps={{ min: 0, step: 0.1 }}
                />
                <TextField
                  fullWidth size="small" label="Tarif journalier maximum (€)" type="number"
                  value={settings.dailyMax}
                  onChange={(e) => update('dailyMax', parseFloat(e.target.value) || 0)}
                  inputProps={{ min: 0, step: 0.5 }}
                />
                <Box>
                  <Typography variant="body2" color="text.secondary" gutterBottom>
                    Intervalle d'actualisation : {settings.autoRefreshInterval}s
                  </Typography>
                  <Slider
                    value={settings.autoRefreshInterval}
                    onChange={(_, v) => update('autoRefreshInterval', v as number)}
                    min={10}
                    max={120}
                    step={10}
                    marks={[
                      { value: 10, label: '10s' },
                      { value: 30, label: '30s' },
                      { value: 60, label: '1min' },
                      { value: 120, label: '2min' },
                    ]}
                    valueLabelDisplay="auto"
                    valueLabelFormat={(v) => `${v}s`}
                  />
                </Box>
              </Stack>
            </CardContent>
          </Card>
        </Grid>

        {/* Apparence */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" fontWeight={600} gutterBottom>
                Apparence
              </Typography>
              <Divider sx={{ mb: 2 }} />
              <Stack spacing={2}>
                <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                  <Box>
                    <Typography variant="body1">Mode sombre</Typography>
                    <Typography variant="caption" color="text.secondary">
                      Activer le thème sombre de l'interface
                    </Typography>
                  </Box>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <Chip
                      label={theme.palette.mode === 'dark' ? 'Sombre' : 'Clair'}
                      size="small"
                      color={theme.palette.mode === 'dark' ? 'primary' : 'default'}
                    />
                    <Switch
                      checked={theme.palette.mode === 'dark'}
                      onChange={colorMode.toggleColorMode}
                    />
                  </Box>
                </Box>
                <FormControl fullWidth size="small">
                  <InputLabel>Langue</InputLabel>
                  <Select
                    value={settings.language}
                    label="Langue"
                    onChange={(e) => update('language', e.target.value)}
                  >
                    <MenuItem value="fr">Français</MenuItem>
                    <MenuItem value="en">English</MenuItem>
                    <MenuItem value="ar">العربية</MenuItem>
                  </Select>
                </FormControl>
              </Stack>
            </CardContent>
          </Card>
        </Grid>

        {/* Notifications */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" fontWeight={600} gutterBottom>
                Notifications
              </Typography>
              <Divider sx={{ mb: 2 }} />
              <Stack spacing={1}>
                <FormControlLabel
                  control={
                    <Switch
                      checked={settings.notifyOnAlert}
                      onChange={(e) => update('notifyOnAlert', e.target.checked)}
                    />
                  }
                  label={
                    <Box>
                      <Typography variant="body2">Alertes équipements</Typography>
                      <Typography variant="caption" color="text.secondary">
                        Notifier lors d'une panne ou erreur
                      </Typography>
                    </Box>
                  }
                />
                <FormControlLabel
                  control={
                    <Switch
                      checked={settings.notifyOnFull}
                      onChange={(e) => update('notifyOnFull', e.target.checked)}
                    />
                  }
                  label={
                    <Box>
                      <Typography variant="body2">Parking complet</Typography>
                      <Typography variant="caption" color="text.secondary">
                        Notifier quand le parking est plein
                      </Typography>
                    </Box>
                  }
                />
                <FormControlLabel
                  control={
                    <Switch
                      checked={settings.notifyOnExpiry}
                      onChange={(e) => update('notifyOnExpiry', e.target.checked)}
                    />
                  }
                  label={
                    <Box>
                      <Typography variant="body2">Expiration abonnements</Typography>
                      <Typography variant="caption" color="text.secondary">
                        Notifier 30 jours avant expiration
                      </Typography>
                    </Box>
                  }
                />
              </Stack>
            </CardContent>
          </Card>
        </Grid>

        {/* Connection */}
        <Grid item xs={12}>
          <Card>
            <CardContent>
              <Typography variant="h6" fontWeight={600} gutterBottom>
                Connexion API
              </Typography>
              <Divider sx={{ mb: 2 }} />
              <Grid container spacing={2}>
                <Grid item xs={12} md={6}>
                  <TextField
                    fullWidth size="small" label="URL de l'API"
                    value={settings.apiUrl}
                    onChange={(e) => update('apiUrl', e.target.value)}
                    helperText="URL de base pour les appels API (ex: /api ou http://localhost:5000/api)"
                  />
                </Grid>
                <Grid item xs={12} md={6}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, height: '100%' }}>
                    <Chip label="SignalR" color="primary" size="small" />
                    <Typography variant="body2" color="text.secondary">
                      Hub : /hubs/parking — Reconnexion automatique activée
                    </Typography>
                  </Box>
                </Grid>
              </Grid>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  )
}
