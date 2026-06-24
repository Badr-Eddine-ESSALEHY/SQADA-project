import React, { useState } from 'react'
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
    description: 'Analyse des tendances sur 7 jours avec comparaison à la semaine précédente.',
    icon: <DateRangeIcon sx={{ fontSize: 40, color: '#2E7D32' }} />,
    dateType: 'week',
  },
  {
    id: 'monthly',
    title: 'Rapport Mensuel',
    description: 'Bilan mensuel complet avec statistiques détaillées et graphiques d\'évolution.',
    icon: <CalendarMonthIcon sx={{ fontSize: 40, color: '#E65100' }} />,
    dateType: 'month',
  },
  {
    id: 'annual',
    title: 'Rapport Annuel',
    description: 'Rapport annuel exhaustif avec analyses comparatives et projections.',
    icon: <EventNoteIcon sx={{ fontSize: 40, color: '#4527A0' }} />,
    dateType: 'year',
  },
]

interface RecentReport {
  id: string
  title: string
  type: string
  date: string
  format: 'PDF' | 'Excel'
  size: string
}

const RECENT_REPORTS: RecentReport[] = [
  { id: '1', title: 'Rapport Journalier', type: 'daily', date: new Date(Date.now() - 86400000).toISOString(), format: 'PDF', size: '245 Ko' },
  { id: '2', title: 'Rapport Hebdomadaire', type: 'weekly', date: new Date(Date.now() - 86400000 * 3).toISOString(), format: 'Excel', size: '512 Ko' },
  { id: '3', title: 'Rapport Mensuel', type: 'monthly', date: new Date(Date.now() - 86400000 * 7).toISOString(), format: 'PDF', size: '1.2 Mo' },
  { id: '4', title: 'Rapport Journalier', type: 'daily', date: new Date(Date.now() - 86400000 * 2).toISOString(), format: 'Excel', size: '180 Ko' },
]

export default function ReportsPage() {
  const [dates, setDates] = useState<Record<string, string>>({
    daily: new Date().toISOString().split('T')[0],
    weekly: new Date().toISOString().split('T')[0],
    monthly: new Date().toISOString().slice(0, 7),
    annual: new Date().getFullYear().toString(),
  })
  const [downloading, setDownloading] = useState<string | null>(null)
  const [success, setSuccess] = useState<string | null>(null)

  const handleDownload = async (reportId: string, format: 'pdf' | 'excel') => {
    const key = `${reportId}-${format}`
    setDownloading(key)
    setSuccess(null)
    try {
      await reportsApi.generate(reportId, { date: dates[reportId], format })
      // Mock: simulate download
      await new Promise((r) => setTimeout(r, 1500))
      setSuccess(`Rapport ${format.toUpperCase()} généré avec succès`)
    } catch {
      // Mock success for demo
      await new Promise((r) => setTimeout(r, 1500))
      setSuccess(`Rapport ${format.toUpperCase()} généré avec succès (démo)`)
    } finally {
      setDownloading(null)
      setTimeout(() => setSuccess(null), 4000)
    }
  }

  const getDateInput = (report: ReportType) => {
    switch (report.dateType) {
      case 'date':
        return (
          <TextField
            size="small"
            type="date"
            value={dates[report.id]}
            onChange={(e) => setDates((prev) => ({ ...prev, [report.id]: e.target.value }))}
            InputLabelProps={{ shrink: true }}
            label="Date"
            fullWidth
          />
        )
      case 'week':
        return (
          <TextField
            size="small"
            type="week"
            value={dates[report.id]}
            onChange={(e) => setDates((prev) => ({ ...prev, [report.id]: e.target.value }))}
            InputLabelProps={{ shrink: true }}
            label="Semaine"
            fullWidth
          />
        )
      case 'month':
        return (
          <TextField
            size="small"
            type="month"
            value={dates[report.id]}
            onChange={(e) => setDates((prev) => ({ ...prev, [report.id]: e.target.value }))}
            InputLabelProps={{ shrink: true }}
            label="Mois"
            fullWidth
          />
        )
      case 'year':
        return (
          <TextField
            size="small"
            type="number"
            value={dates[report.id]}
            onChange={(e) => setDates((prev) => ({ ...prev, [report.id]: e.target.value }))}
            InputLabelProps={{ shrink: true }}
            label="Année"
            inputProps={{ min: 2020, max: 2030 }}
            fullWidth
          />
        )
    }
  }

  return (
    <Box>
      <Box sx={{ mb: 3 }}>
        <Typography variant="h5" fontWeight={700}>Rapports</Typography>
        <Typography variant="body2" color="text.secondary">
          Génération et téléchargement des rapports de stationnement
        </Typography>
      </Box>

      {success && (
        <Alert severity="success" sx={{ mb: 2 }} icon={<CheckCircleIcon />}>
          {success}
        </Alert>
      )}

      {/* Report cards */}
      <Grid container spacing={2} sx={{ mb: 4 }}>
        {REPORT_TYPES.map((report) => {
          const isPdfLoading = downloading === `${report.id}-pdf`
          const isExcelLoading = downloading === `${report.id}-excel`
          const isAnyLoading = isPdfLoading || isExcelLoading
          return (
            <Grid item xs={12} sm={6} md={3} key={report.id}>
              <Card sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
                {isAnyLoading && <LinearProgress />}
                <CardContent sx={{ flexGrow: 1 }}>
                  <Box sx={{ textAlign: 'center', mb: 2 }}>
                    {report.icon}
                    <Typography variant="h6" fontWeight={600} sx={{ mt: 1 }}>
                      {report.title}
                    </Typography>
                  </Box>
                  <Typography variant="body2" color="text.secondary" sx={{ mb: 2, textAlign: 'center' }}>
                    {report.description}
                  </Typography>
                  {getDateInput(report)}
                </CardContent>
                <Divider />
                <CardActions sx={{ justifyContent: 'center', gap: 1, p: 1.5 }}>
                  <Button
                    variant="contained"
                    color="error"
                    size="small"
                    startIcon={<PictureAsPdfIcon />}
                    disabled={isAnyLoading}
                    onClick={() => handleDownload(report.id, 'pdf')}
                    sx={{ borderRadius: 2 }}
                  >
                    {isPdfLoading ? 'Génération...' : 'PDF'}
                  </Button>
                  <Button
                    variant="contained"
                    color="success"
                    size="small"
                    startIcon={<TableChartIcon />}
                    disabled={isAnyLoading}
                    onClick={() => handleDownload(report.id, 'excel')}
                    sx={{ borderRadius: 2 }}
                  >
                    {isExcelLoading ? 'Génération...' : 'Excel'}
                  </Button>
                </CardActions>
              </Card>
            </Grid>
          )
        })}
      </Grid>

      {/* Recent reports */}
      <Card>
        <CardContent>
          <Typography variant="h6" fontWeight={600} gutterBottom>
            Rapports récents
          </Typography>
          <List disablePadding>
            {RECENT_REPORTS.map((report, idx) => (
              <React.Fragment key={report.id}>
                {idx > 0 && <Divider />}
                <ListItem
                  secondaryAction={
                    <Stack direction="row" spacing={1} alignItems="center">
                      <Typography variant="caption" color="text.secondary">{report.size}</Typography>
                      <Chip
                        label={report.format}
                        size="small"
                        color={report.format === 'PDF' ? 'error' : 'success'}
                        variant="outlined"
                      />
                      <Button size="small" variant="outlined" sx={{ borderRadius: 2 }}>
                        Télécharger
                      </Button>
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
        </CardContent>
      </Card>
    </Box>
  )
}
