import React, { useState, useEffect, useCallback } from 'react'
import {
  Box,
  Typography,
  Grid,
  Card,
  CardContent,
  Chip,
  IconButton,
  Tooltip,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
  Divider,
  Alert,
  Badge,
  Button,
  Skeleton,
  Stack,
} from '@mui/material'
import RefreshIcon from '@mui/icons-material/Refresh'
import ErrorIcon from '@mui/icons-material/Error'
import WarningIcon from '@mui/icons-material/Warning'
import InfoIcon from '@mui/icons-material/Info'
import CheckCircleIcon from '@mui/icons-material/CheckCircle'
import WifiIcon from '@mui/icons-material/Wifi'
import WifiOffIcon from '@mui/icons-material/WifiOff'
import BuildIcon from '@mui/icons-material/Build'
import DoneIcon from '@mui/icons-material/Done'
import { maintenanceApi } from '../../services/api'
import type { MaintenanceStatus, DeviceStatus, AlertSeverity } from '../../types'
import { formatRelativeTime } from '../../utils/formatters'

const STATUS_CONFIG: Record<DeviceStatus, { color: 'success' | 'error' | 'warning' | 'default'; icon: React.ReactNode; label: string }> = {
  Online: { color: 'success', icon: <WifiIcon fontSize="small" />, label: 'En ligne' },
  Offline: { color: 'error', icon: <WifiOffIcon fontSize="small" />, label: 'Hors ligne' },
  Error: { color: 'error', icon: <ErrorIcon fontSize="small" />, label: 'Erreur' },
  Maintenance: { color: 'warning', icon: <BuildIcon fontSize="small" />, label: 'Maintenance' },
}

const ALERT_ICONS: Record<AlertSeverity, React.ReactNode> = {
  error: <ErrorIcon color="error" />,
  warning: <WarningIcon color="warning" />,
  info: <InfoIcon color="info" />,
}

export default function MaintenancePage() {
  const [status, setStatus] = useState<MaintenanceStatus | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [lastRefresh, setLastRefresh] = useState(new Date())

  const fetchStatus = useCallback(async () => {
    try {
      const data = await maintenanceApi.getStatus()
      setStatus(data)
      setLastRefresh(new Date())
      setError(null)
    } catch {
      setError('Erreur lors du chargement du statut')
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    fetchStatus()
    const interval = setInterval(fetchStatus, 30000)
    return () => clearInterval(interval)
  }, [fetchStatus])

  const handleMarkRead = async (alertId: string) => {
    try {
      await maintenanceApi.markAlertRead(alertId)
    } catch {}
    setStatus((prev) =>
      prev
        ? {
            ...prev,
            alerts: prev.alerts.map((a) => (a.id === alertId ? { ...a, read: true } : a)),
          }
        : prev,
    )
  }

  const unreadCount = status?.alerts.filter((a) => !a.read).length ?? 0

  return (
    <Box>
      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 3 }}>
        <Box>
          <Typography variant="h5" fontWeight={700}>Maintenance</Typography>
          <Typography variant="body2" color="text.secondary">
            Statut des équipements — Actualisation automatique toutes les 30s
          </Typography>
        </Box>
        <Stack direction="row" spacing={1} alignItems="center">
          <Typography variant="caption" color="text.secondary">
            Dernière mise à jour : {formatRelativeTime(lastRefresh.toISOString())}
          </Typography>
          <Tooltip title="Actualiser">
            <IconButton onClick={fetchStatus} size="small">
              <RefreshIcon />
            </IconButton>
          </Tooltip>
        </Stack>
      </Box>

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

      {/* Gates */}
      <Typography variant="h6" fontWeight={600} sx={{ mb: 1.5 }}>
        Barrières
      </Typography>
      <Grid container spacing={2} sx={{ mb: 3 }}>
        {loading
          ? Array.from({ length: 4 }).map((_, i) => (
              <Grid item xs={12} sm={6} md={3} key={i}>
                <Skeleton variant="rectangular" height={140} sx={{ borderRadius: 2 }} />
              </Grid>
            ))
          : status?.gates.map((gate) => {
              const cfg = STATUS_CONFIG[gate.status]
              return (
                <Grid item xs={12} sm={6} md={3} key={gate.id}>
                  <Card
                    sx={{
                      borderLeft: 4,
                      borderColor:
                        gate.status === 'Online'
                          ? 'success.main'
                          : gate.status === 'Offline'
                          ? 'error.main'
                          : gate.status === 'Error'
                          ? 'error.main'
                          : 'warning.main',
                    }}
                  >
                    <CardContent sx={{ pb: '16px !important' }}>
                      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 1 }}>
                        <Typography variant="subtitle2" fontWeight={700}>
                          {gate.name}
                        </Typography>
                        <Chip
                          icon={cfg.icon}
                          label={cfg.label}
                          color={cfg.color}
                          size="small"
                        />
                      </Box>
                      <Chip
                        label={gate.type}
                        size="small"
                        variant="outlined"
                        color={gate.type === 'Entrée' ? 'primary' : 'secondary'}
                        sx={{ mb: 1 }}
                      />
                      <Typography variant="caption" color="text.secondary" display="block">
                        IP : {gate.ip}
                      </Typography>
                      <Typography variant="caption" color="text.secondary" display="block">
                        Dernier ping : {formatRelativeTime(gate.lastPing)}
                      </Typography>
                    </CardContent>
                  </Card>
                </Grid>
              )
            })}
      </Grid>

      {/* Terminals */}
      <Typography variant="h6" fontWeight={600} sx={{ mb: 1.5 }}>
        Terminaux
      </Typography>
      <Grid container spacing={2} sx={{ mb: 3 }}>
        {loading
          ? Array.from({ length: 3 }).map((_, i) => (
              <Grid item xs={12} sm={6} md={4} key={i}>
                <Skeleton variant="rectangular" height={130} sx={{ borderRadius: 2 }} />
              </Grid>
            ))
          : status?.terminals.map((terminal) => {
              const cfg = STATUS_CONFIG[terminal.status]
              return (
                <Grid item xs={12} sm={6} md={4} key={terminal.id}>
                  <Card
                    sx={{
                      borderLeft: 4,
                      borderColor:
                        terminal.status === 'Online'
                          ? 'success.main'
                          : terminal.status === 'Maintenance'
                          ? 'warning.main'
                          : 'error.main',
                    }}
                  >
                    <CardContent sx={{ pb: '16px !important' }}>
                      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 1 }}>
                        <Typography variant="subtitle2" fontWeight={700}>
                          {terminal.name}
                        </Typography>
                        <Chip
                          icon={cfg.icon}
                          label={cfg.label}
                          color={cfg.color}
                          size="small"
                        />
                      </Box>
                      <Typography variant="caption" color="text.secondary" display="block">
                        IP : {terminal.ip}
                      </Typography>
                      <Typography variant="caption" color="text.secondary" display="block">
                        Dernier ping : {formatRelativeTime(terminal.lastPing)}
                      </Typography>
                    </CardContent>
                  </Card>
                </Grid>
              )
            })}
      </Grid>

      {/* Alerts */}
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1.5 }}>
        <Typography variant="h6" fontWeight={600}>
          Alertes
        </Typography>
        {unreadCount > 0 && (
          <Badge badgeContent={unreadCount} color="error">
            <Box />
          </Badge>
        )}
      </Box>
      <Card>
        <List disablePadding>
          {loading
            ? Array.from({ length: 3 }).map((_, i) => (
                <React.Fragment key={i}>
                  {i > 0 && <Divider />}
                  <ListItem>
                    <Skeleton variant="circular" width={24} height={24} sx={{ mr: 2 }} />
                    <Skeleton variant="text" width="80%" />
                  </ListItem>
                </React.Fragment>
              ))
            : status?.alerts.length === 0
            ? (
                <ListItem>
                  <ListItemIcon><CheckCircleIcon color="success" /></ListItemIcon>
                  <ListItemText primary="Aucune alerte active" secondary="Tous les équipements fonctionnent normalement" />
                </ListItem>
              )
            : status?.alerts.map((alert, idx) => (
                <React.Fragment key={alert.id}>
                  {idx > 0 && <Divider />}
                  <ListItem
                    sx={{ opacity: alert.read ? 0.6 : 1, bgcolor: alert.read ? 'transparent' : 'action.hover' }}
                    secondaryAction={
                      !alert.read && (
                        <Tooltip title="Marquer comme lu">
                          <IconButton size="small" onClick={() => handleMarkRead(alert.id)}>
                            <DoneIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                      )
                    }
                  >
                    <ListItemIcon>{ALERT_ICONS[alert.severity]}</ListItemIcon>
                    <ListItemText
                      primary={alert.message}
                      secondary={formatRelativeTime(alert.time)}
                      primaryTypographyProps={{ fontWeight: alert.read ? 400 : 600 }}
                    />
                  </ListItem>
                </React.Fragment>
              ))}
        </List>
        {unreadCount > 0 && (
          <Box sx={{ p: 1.5, borderTop: 1, borderColor: 'divider', textAlign: 'right' }}>
            <Button
              size="small"
              onClick={() => status?.alerts.filter((a) => !a.read).forEach((a) => handleMarkRead(a.id))}
            >
              Tout marquer comme lu
            </Button>
          </Box>
        )}
      </Card>
    </Box>
  )
}
