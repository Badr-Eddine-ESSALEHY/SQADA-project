import React, { useState, useEffect, useCallback } from 'react'
import {
  Box,
  Typography,
  Grid,
  Card,
  CardContent,
  CardActions,
  Button,
  TextField,
  Divider,
  List,
  ListItem,
  ListItemText,
  ListItemIcon,
  Chip,
  LinearProgress,
  Alert,
  Stack,
  CircularProgress,
} from '@mui/material'
import PictureAsPdfIcon from '@mui/icons-material/PictureAsPdf'
import TableChartIcon from '@mui/icons-material/TableChart'
import DescriptionIcon from '@mui/icons-material/Description'
import CalendarTodayIcon from '@mui/icons-material/CalendarToday'
import DateRangeIcon from '@mui/icons-material/DateRange'
import CalendarMonthIcon from '@mui/icons-material/CalendarMonth'
import EventNoteIcon from '@mui/icons-material/EventNote'
import CheckCircleIcon from '@mui/icons-material/CheckCircle'
import { reportsApi } from '../../services/api'
import { formatDate } from '../../utils/formatters'

interface ReportType {
  id: string
  title: string
  description: string
  icon: React.ReactNode
  dateType: 'date' | 'week' | 'month' | 'year'
}

interface RecentReport {
  id: string
  title: string
  date: string
  format: 'PDF' | 'EXCEL'
  size: string
}

const REPORT_TYPES: ReportType[] = [
  {
    id: 'daily',
    title: 'Rapport Journalier',
    description: 'Synthèse complète des entrées, sorties, revenus et incidents pour une journée donnée.',
    icon: <CalendarTodayIcon sx={{ fontSize: 40, color: '#1565C0' }} />,
    dateType: 'date',
  },
  {
    id: 'weekly',
    title: 'Rapport Hebdomadaire',
    description: "Analyse des performances hebdomadaires, taux d'occupation et rapports de revenus.",
    icon: <DateRangeIcon sx={{ fontSize: 40, color: '#2E7D32' }} />,
    dateType: 'week',
  },
  {
    id: 'monthly',
    title: 'Rapport Mensuel',
    description: 'Évolution à long terme, abonnements mensuels et statistiques de fréquentation globale.',
    icon: <CalendarMonthIcon sx={{ fontSize: 40, color: '#E65100' }} />,
    dateType: 'month',
  },
  {
    id: 'custom',
    title: 'Rapport Personnalisé',
    description: "Extraction sur mesure basée sur des filtres avancés d'intervalles et critères spécifiques.",
    icon: <EventNoteIcon sx={{ fontSize: 40, color: '#0277BD' }} />,
    dateType: 'date',
  },
]

export default function ReportsPage() {
  const [dates, setDates] = useState<Record<string, string>>(() => {
    const today = new Date().toISOString().split('T')[0]
    return { daily: today, weekly: today, monthly: today.substring(0, 7), custom: today }
  })

  const [recentReports, setRecentReports] = useState<RecentReport[]>([])
  const [loadingHistory, setLoadingHistory] = useState(true)
  const [downloading, setDownloading] = useState<string | null>(null)
  const [success, setSuccess] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)

  const loadReportHistory = useCallback(async () => {
    try {
      setLoadingHistory(true)
      const res = await reportsApi.getRecent()
      setRecentReports(res.data || [])
    } catch (err) {
      console.error("Failed loading database generation profiles", err)
      setError("Impossible de charger l'historique des rapports.")
    } finally {
      setLoadingHistory(false)
    }
  }, [])

  useEffect(() => {
    loadReportHistory()
  }, [loadReportHistory])

  const handleDateChange = (id: string, val: string) => {
    setDates((prev) => ({ ...prev, [id]: val }))
  }

  const handleGenerate = async (reportId: string, format: 'pdf' | 'excel') => {
    const operationKey = `${reportId}-${format}`
    setDownloading(operationKey)
    setSuccess(null)
    setError(null)

    try {
      const response = await reportsApi.generate(reportId, { date: dates[reportId], format })
      
      const url = window.URL.createObjectURL(new Blob([response.data]))
      const link = document.createElement('a')
      link.href = url
      link.setAttribute('download', `Rapport_${reportId}_${dates[reportId]}.${format === 'pdf' ? 'pdf' : 'xlsx'}`)
      document.body.appendChild(link)
      link.click()
      link.remove()

      setSuccess(`Rapport ${format.toUpperCase()} généré et téléchargé avec succès !`)
      await loadReportHistory()
    } catch (err) {
      console.error("Engine generation endpoint fault", err)
      setError("Échec de la communication avec le serveur lors de la génération du rapport.")
    } finally {
      setDownloading(null)
      setTimeout(() => setSuccess(null), 5000)
    }
  }

  return (
    <Box sx={{ p: 3 }}>
      <Box sx={{ mb: 4 }}>
        <Typography variant="h4" fontWeight={700} color="text.primary" gutterBottom>
          Gestion des Rapports
        </Typography>
        <Typography variant="body1" color="text.secondary">
          Configurez, planifiez et générez des rapports analytiques directement issus de la base PostgreSQL.
        </Typography>
      </Box>

      {success && <Alert severity="success" icon={<CheckCircleIcon />} sx={{ mb: 3, borderRadius: 2 }}>{success}</Alert>}
      {error && <Alert severity="error" sx={{ mb: 3, borderRadius: 2 }}>{error}</Alert>}

      <Grid container spacing={3}>
        {REPORT_TYPES.map((type) => {
          const isPdfLoading = downloading === `${type.id}-pdf`
          const isExcelLoading = downloading === `${type.id}-excel`

          return (
            <Grid item xs={12} md={6} key={type.id}>
              <Card sx={{ height: '100%', display: 'flex', flexDirection: 'column', borderRadius: 3, boxShadow: '0 4px 12px rgba(0,0,0,0.05)' }}>
                <CardContent sx={{ flexGrow: 1, p: 3 }}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 2 }}>
                    {type.icon}
                    <Box>
                      <Typography variant="h6" fontWeight={600}>{type.title}</Typography>
                      <Typography variant="body2" color="text.secondary">{type.description}</Typography>
                    </Box>
                  </Box>
                  <Divider sx={{ my: 2 }} />
                  <TextField
                    fullWidth
                    label="Sélectionner la période"
                    type={type.dateType === 'month' ? 'month' : 'date'}
                    value={dates[type.id]}
                    onChange={(e) => handleDateChange(type.id, e.target.value)}
                    InputLabelProps={{ shrink: true }}
                    size="small"
                  />
                </CardContent>
                <CardActions sx={{ p: 3, pt: 0, gap: 1 }}>
                  <Button
                    fullWidth
                    variant="contained"
                    color="error"
                    startIcon={isPdfLoading ? <CircularProgress size={16} color="inherit" /> : <PictureAsPdfIcon />}
                    disabled={downloading !== null}
                    onClick={() => handleGenerate(type.id, 'pdf')}
                    sx={{ borderRadius: 2, py: 1 }}
                  >
                    PDF
                  </Button>
                  <Button
                    fullWidth
                    variant="contained"
                    color="success"
                    startIcon={isExcelLoading ? <CircularProgress size={16} color="inherit" /> : <TableChartIcon />}
                    disabled={downloading !== null}
                    onClick={() => handleGenerate(type.id, 'excel')}
                    sx={{ borderRadius: 2, py: 1 }}
                  >
                    Excel
                  </Button>
                </CardActions>
                {downloading?.startsWith(type.id) && <LinearProgress color="primary" />}
              </Card>
            </Grid>
          )
        })}
      </Grid>

      <Card sx={{ mt: 4, borderRadius: 3, boxShadow: '0 4px 12px rgba(0,0,0,0.05)' }}>
        <CardContent sx={{ p: 3 }}>
          <Typography variant="h6" fontWeight={600} gutterBottom>
            Rapports récents
          </Typography>
          {loadingHistory ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}><CircularProgress /></Box>
          ) : recentReports.length === 0 ? (
            <Typography variant="body2" color="text.disabled" sx={{ py: 2, textAlign: 'center' }}>
              Aucun rapport n'a été généré récemment dans la base de données.
            </Typography>
          ) : (
            <List disablePadding>
              {recentReports.map((report, idx) => (
                <React.Fragment key={report.id || idx}>
                  {idx > 0 && <Divider />}
                  <ListItem
                    secondaryAction={
                      <Stack direction="row" spacing={1} alignItems="center">
                        <Typography variant="caption" color="text.secondary">{report.size || 'N/A'}</Typography>
                        <Chip
                          label={report.format}
                          size="small"
                          color={report.format?.toUpperCase() === 'PDF' ? 'error' : 'success'}
                          variant="outlined"
                        />
                      </Stack>
                    }
                  >
                    <ListItemIcon>
                      <DescriptionIcon color="action" />
                    </ListItemIcon>
                    <ListItemText
                      primary={report.title}
                      secondary={formatDate(report.date)}
                      primaryTypographyProps={{ fontWeight: 600 }}
                    />
                  </ListItem>
                </React.Fragment>
              ))}
            </List>
          )}
        </CardContent>
      </Card>
    </Box>
  )
}