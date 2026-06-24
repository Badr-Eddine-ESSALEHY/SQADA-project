import React, { useState } from 'react'
import {
  Box,
  Grid,
  Card,
  CardContent,
  Typography,
  Skeleton,
  Alert,
  IconButton,
  Tooltip,
  Chip,
  useTheme,
} from '@mui/material'
import RefreshIcon from '@mui/icons-material/Refresh'
import DirectionsCarIcon from '@mui/icons-material/DirectionsCar'
import ExitToAppIcon from '@mui/icons-material/ExitToApp'
import LocalParkingIcon from '@mui/icons-material/LocalParking'
import SpaceBarIcon from '@mui/icons-material/SpaceBar'
import EuroIcon from '@mui/icons-material/Euro'
import CalendarMonthIcon from '@mui/icons-material/CalendarMonth'
import PeopleIcon from '@mui/icons-material/People'
import DonutLargeIcon from '@mui/icons-material/DonutLarge'
import ReactApexChart from 'react-apexcharts'
import type { ApexOptions } from 'apexcharts'
import StatCard from './StatCard'
import { useDashboard } from '../../hooks/useDashboard'
import { formatCurrency, formatPercent, formatNumber } from '../../utils/formatters'

export default function DashboardPage() {
  const theme = useTheme()
  const { stats, revenueData, loading, error, refresh } = useDashboard(30000)
  const [lastUpdate] = useState(new Date())

  const isDark = theme.palette.mode === 'dark'
  const textColor = theme.palette.text.primary
  const gridColor = isDark ? 'rgba(255,255,255,0.08)' : 'rgba(0,0,0,0.06)'

  // Revenue line chart
  const revenueChartOptions: ApexOptions = {
    chart: { type: 'area', toolbar: { show: false }, background: 'transparent', sparkline: { enabled: false } },
    theme: { mode: isDark ? 'dark' : 'light' },
    stroke: { curve: 'smooth', width: 2 },
    fill: { type: 'gradient', gradient: { shadeIntensity: 1, opacityFrom: 0.4, opacityTo: 0.05 } },
    xaxis: {
      categories: revenueData.map((d) => d.date.slice(5)),
      labels: { style: { colors: textColor, fontSize: '11px' }, rotate: -30 },
      axisBorder: { show: false },
      axisTicks: { show: false },
    },
    yaxis: {
      labels: {
        style: { colors: textColor, fontSize: '11px' },
        formatter: (v) => `${v.toFixed(0)} €`,
      },
    },
    grid: { borderColor: gridColor, strokeDashArray: 4 },
    tooltip: { theme: isDark ? 'dark' : 'light', y: { formatter: (v) => `${v.toFixed(2)} €` } },
    colors: ['#1565C0'],
    dataLabels: { enabled: false },
  }

  const revenueChartSeries = [
    { name: 'Revenus', data: revenueData.map((d) => parseFloat(d.value.toFixed(2))) },
  ]

  // Occupancy donut chart
  const occupancyRate = stats?.occupancyRate ?? 0
  const donutOptions: ApexOptions = {
    chart: { type: 'donut', background: 'transparent' },
    theme: { mode: isDark ? 'dark' : 'light' },
    labels: ['Occupé', 'Disponible'],
    colors: ['#1565C0', isDark ? '#1E3A5F' : '#E3F2FD'],
    legend: { position: 'bottom', labels: { colors: textColor } },
    plotOptions: {
      pie: {
        donut: {
          size: '70%',
          labels: {
            show: true,
            total: {
              show: true,
              label: 'Taux',
              color: textColor,
              formatter: () => `${occupancyRate.toFixed(1)}%`,
            },
          },
        },
      },
    },
    dataLabels: { enabled: false },
    tooltip: { theme: isDark ? 'dark' : 'light' },
  }

  const donutSeries = [
    parseFloat(occupancyRate.toFixed(1)),
    parseFloat((100 - occupancyRate).toFixed(1)),
  ]

  // Entries/Exits bar chart (mock hourly)
  const hours = Array.from({ length: 12 }, (_, i) => `${(i + 7).toString().padStart(2, '0')}h`)
  const barOptions: ApexOptions = {
    chart: { type: 'bar', toolbar: { show: false }, background: 'transparent' },
    theme: { mode: isDark ? 'dark' : 'light' },
    plotOptions: { bar: { borderRadius: 4, columnWidth: '60%' } },
    xaxis: {
      categories: hours,
      labels: { style: { colors: textColor, fontSize: '11px' } },
      axisBorder: { show: false },
    },
    yaxis: { labels: { style: { colors: textColor, fontSize: '11px' } } },
    grid: { borderColor: gridColor, strokeDashArray: 4 },
    colors: ['#1565C0', '#0288D1'],
    legend: { labels: { colors: textColor } },
    dataLabels: { enabled: false },
    tooltip: { theme: isDark ? 'dark' : 'light' },
  }

  const barSeries = [
    { name: 'Entrées', data: hours.map(() => Math.floor(Math.random() * 20) + 5) },
    { name: 'Sorties', data: hours.map(() => Math.floor(Math.random() * 18) + 4) },
  ]

  const statCards = [
    {
      title: 'Entrées du jour',
      value: loading ? '—' : formatNumber(stats?.entriesToday),
      icon: <DirectionsCarIcon sx={{ color: 'white', fontSize: 22 }} />,
      variant: 'primary' as const,
      trend: 5.2,
      subtitle: 'véhicules entrés',
    },
    {
      title: 'Sorties du jour',
      value: loading ? '—' : formatNumber(stats?.exitsToday),
      icon: <ExitToAppIcon sx={{ color: 'white', fontSize: 22 }} />,
      variant: 'info' as const,
      trend: 3.1,
      subtitle: 'véhicules sortis',
    },
    {
      title: 'Occupation actuelle',
      value: loading ? '—' : formatNumber(stats?.currentOccupancy),
      icon: <LocalParkingIcon sx={{ color: 'white', fontSize: 22 }} />,
      variant: 'warning' as const,
      trend: -2.4,
      subtitle: 'véhicules présents',
    },
    {
      title: 'Places disponibles',
      value: loading ? '—' : formatNumber(stats?.availableSpaces),
      icon: <SpaceBarIcon sx={{ color: 'white', fontSize: 22 }} />,
      variant: 'success' as const,
      trend: 2.4,
      subtitle: `sur ${stats?.totalCapacity ?? 150} places`,
    },
    {
      title: "CA du jour",
      value: loading ? '—' : formatCurrency(stats?.revenueToday),
      icon: <EuroIcon sx={{ color: 'white', fontSize: 22 }} />,
      variant: 'success' as const,
      trend: 8.7,
      subtitle: 'chiffre d\'affaires',
    },
    {
      title: 'CA du mois',
      value: loading ? '—' : formatCurrency(stats?.revenueMonth),
      icon: <CalendarMonthIcon sx={{ color: 'white', fontSize: 22 }} />,
      variant: 'secondary' as const,
      trend: 12.3,
      subtitle: 'mois en cours',
    },
    {
      title: 'Abonnés actifs',
      value: loading ? '—' : formatNumber(stats?.activeSubscribers),
      icon: <PeopleIcon sx={{ color: 'white', fontSize: 22 }} />,
      variant: 'info' as const,
      trend: 1.5,
      subtitle: 'abonnements valides',
    },
    {
      title: 'Taux de remplissage',
      value: loading ? '—' : formatPercent(stats?.occupancyRate),
      icon: <DonutLargeIcon sx={{ color: 'white', fontSize: 22 }} />,
      variant: occupancyRate > 80 ? 'error' as const : occupancyRate > 60 ? 'warning' as const : 'success' as const,
      trend: -2.4,
      subtitle: 'capacité utilisée',
    },
  ]

  return (
    <Box>
      {/* Header */}
      <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 3 }}>
        <Box>
          <Typography variant="h5" fontWeight={700}>
            Tableau de bord
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Mise à jour automatique toutes les 30 secondes
          </Typography>
        </Box>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <Chip
            label="Temps réel"
            color="success"
            size="small"
            sx={{ fontWeight: 600 }}
          />
          <Tooltip title="Actualiser">
            <IconButton onClick={refresh} size="small">
              <RefreshIcon />
            </IconButton>
          </Tooltip>
        </Box>
      </Box>

      {error && (
        <Alert severity="warning" sx={{ mb: 2 }}>
          {error} — Affichage des données de démonstration
        </Alert>
      )}

      {/* Stats cards */}
      <Grid container spacing={2} sx={{ mb: 3 }}>
        {statCards.map((card, idx) => (
          <Grid item xs={12} sm={6} md={3} key={idx}>
            <StatCard {...card} loading={loading} />
          </Grid>
        ))}
      </Grid>

      {/* Charts row 1 */}
      <Grid container spacing={2} sx={{ mb: 2 }}>
        {/* Revenue chart */}
        <Grid item xs={12} md={8}>
          <Card sx={{ height: '100%' }}>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Revenus — 30 derniers jours
              </Typography>
              {loading ? (
                <Skeleton variant="rectangular" height={260} sx={{ borderRadius: 2 }} />
              ) : (
                <ReactApexChart
                  options={revenueChartOptions}
                  series={revenueChartSeries}
                  type="area"
                  height={260}
                />
              )}
            </CardContent>
          </Card>
        </Grid>

        {/* Occupancy donut */}
        <Grid item xs={12} md={4}>
          <Card sx={{ height: '100%' }}>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Occupation actuelle
              </Typography>
              {loading ? (
                <Skeleton variant="circular" width={200} height={200} sx={{ mx: 'auto' }} />
              ) : (
                <ReactApexChart
                  options={donutOptions}
                  series={donutSeries}
                  type="donut"
                  height={260}
                />
              )}
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Charts row 2 */}
      <Grid container spacing={2}>
        {/* Entries/Exits bar */}
        <Grid item xs={12} md={8}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Flux horaire — Entrées & Sorties
              </Typography>
              <ReactApexChart
                options={barOptions}
                series={barSeries}
                type="bar"
                height={240}
              />
            </CardContent>
          </Card>
        </Grid>

        {/* Average duration */}
        <Grid item xs={12} md={4}>
          <Card sx={{ height: '100%' }}>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Durées moyennes
              </Typography>
              <Box sx={{ mt: 2 }}>
                {[
                  { label: 'Durée moyenne', value: '1h 45min', color: 'primary.main' },
                  { label: 'Durée min', value: '15min', color: 'success.main' },
                  { label: 'Durée max', value: '8h 20min', color: 'error.main' },
                  { label: 'Médiane', value: '1h 10min', color: 'info.main' },
                ].map((item) => (
                  <Box
                    key={item.label}
                    sx={{
                      display: 'flex',
                      justifyContent: 'space-between',
                      alignItems: 'center',
                      py: 1.5,
                      borderBottom: `1px solid ${theme.palette.divider}`,
                      '&:last-child': { borderBottom: 'none' },
                    }}
                  >
                    <Typography variant="body2" color="text.secondary">
                      {item.label}
                    </Typography>
                    <Typography variant="body1" fontWeight={600} color={item.color}>
                      {item.value}
                    </Typography>
                  </Box>
                ))}
              </Box>
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  )
}
