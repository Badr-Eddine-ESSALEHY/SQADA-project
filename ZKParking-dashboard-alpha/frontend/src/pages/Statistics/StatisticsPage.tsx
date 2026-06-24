import React, { useState, useEffect } from 'react'
import {
  Box,
  Typography,
  Card,
  CardContent,
  Tabs,
  Tab,
  Grid,
  Skeleton,
  Alert,
  useTheme,
} from '@mui/material'
import ReactApexChart from 'react-apexcharts'
import type { ApexOptions } from 'apexcharts'
import { statisticsApi } from '../../services/api'
import type { Statistics } from '../../types'

interface TabPanelProps {
  children?: React.ReactNode
  index: number
  value: number
}

function TabPanel({ children, value, index }: TabPanelProps) {
  return value === index ? <Box sx={{ pt: 2 }}>{children}</Box> : null
}

export default function StatisticsPage() {
  const theme = useTheme()
  const isDark = theme.palette.mode === 'dark'
  const [tab, setTab] = useState(0)
  const [stats, setStats] = useState<Statistics | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const textColor = theme.palette.text.primary
  const gridColor = isDark ? 'rgba(255,255,255,0.08)' : 'rgba(0,0,0,0.06)'
  const chartTheme = isDark ? 'dark' : 'light'

  useEffect(() => {
    statisticsApi.getAll().then((data) => {
      setStats(data)
      setLoading(false)
    }).catch(() => {
      setError('Erreur lors du chargement des statistiques')
      setLoading(false)
    })
  }, [])

  const commonAxis = {
    labels: { style: { colors: textColor, fontSize: '11px' } },
    axisBorder: { show: false },
    axisTicks: { show: false },
  }

  // ── Revenus ──────────────────────────────────────────────────────────────────
  const revenueLineOptions: ApexOptions = {
    chart: { type: 'line', toolbar: { show: true }, background: 'transparent' },
    theme: { mode: chartTheme },
    stroke: { curve: 'smooth', width: 2 },
    xaxis: { ...commonAxis, categories: stats?.daily.map((d) => d.date.slice(5)) ?? [] },
    yaxis: { ...commonAxis, labels: { ...commonAxis.labels, formatter: (v) => `${v.toFixed(0)} €` } },
    grid: { borderColor: gridColor, strokeDashArray: 4 },
    colors: ['#1565C0'],
    dataLabels: { enabled: false },
    tooltip: { theme: chartTheme, y: { formatter: (v) => `${v.toFixed(2)} €` } },
  }

  const revenueLineSeries = [
    { name: 'Revenus journaliers', data: stats?.daily.map((d) => parseFloat(d.revenue.toFixed(2))) ?? [] },
  ]

  // Monthly bar (group by month)
  const monthlyRevenue = (() => {
    if (!stats) return { labels: [], data: [] }
    const map: Record<string, number> = {}
    stats.daily.forEach((d) => {
      const m = d.date.slice(0, 7)
      map[m] = (map[m] || 0) + d.revenue
    })
    return { labels: Object.keys(map), data: Object.values(map).map((v) => parseFloat(v.toFixed(2))) }
  })()

  const revenueBarOptions: ApexOptions = {
    chart: { type: 'bar', toolbar: { show: false }, background: 'transparent' },
    theme: { mode: chartTheme },
    plotOptions: { bar: { borderRadius: 4, columnWidth: '50%' } },
    xaxis: { ...commonAxis, categories: monthlyRevenue.labels },
    yaxis: { ...commonAxis, labels: { ...commonAxis.labels, formatter: (v) => `${v.toFixed(0)} €` } },
    grid: { borderColor: gridColor, strokeDashArray: 4 },
    colors: ['#0288D1'],
    dataLabels: { enabled: false },
    tooltip: { theme: chartTheme, y: { formatter: (v) => `${v.toFixed(2)} €` } },
  }

  const revenueBarSeries = [{ name: 'CA mensuel', data: monthlyRevenue.data }]

  // ── Flux ─────────────────────────────────────────────────────────────────────
  const fluxBarOptions: ApexOptions = {
    chart: { type: 'bar', toolbar: { show: false }, background: 'transparent' },
    theme: { mode: chartTheme },
    plotOptions: { bar: { borderRadius: 3, columnWidth: '60%' } },
    xaxis: {
      ...commonAxis,
      categories: stats?.hourly.map((h) => `${String(h.hour).padStart(2, '0')}h`) ?? [],
    },
    yaxis: { ...commonAxis },
    grid: { borderColor: gridColor, strokeDashArray: 4 },
    colors: ['#1565C0', '#2E7D32'],
    legend: { labels: { colors: textColor } },
    dataLabels: { enabled: false },
    tooltip: { theme: chartTheme },
  }

  const fluxBarSeries = [
    { name: 'Entrées', data: stats?.hourly.map((h) => h.entries) ?? [] },
    { name: 'Sorties', data: stats?.hourly.map((h) => h.exits) ?? [] },
  ]

  const weeklyLineOptions: ApexOptions = {
    chart: { type: 'line', toolbar: { show: false }, background: 'transparent' },
    theme: { mode: chartTheme },
    stroke: { curve: 'smooth', width: 2 },
    xaxis: { ...commonAxis, categories: stats?.daily.slice(-7).map((d) => d.date.slice(5)) ?? [] },
    yaxis: { ...commonAxis },
    grid: { borderColor: gridColor, strokeDashArray: 4 },
    colors: ['#1565C0', '#2E7D32'],
    legend: { labels: { colors: textColor } },
    dataLabels: { enabled: false },
    tooltip: { theme: chartTheme },
  }

  const weeklyLineSeries = [
    { name: 'Entrées', data: stats?.daily.slice(-7).map((d) => d.entries) ?? [] },
    { name: 'Sorties', data: stats?.daily.slice(-7).map((d) => d.exits) ?? [] },
  ]

  // ── Occupation ───────────────────────────────────────────────────────────────
  const avgOccupancy = stats
    ? stats.daily.reduce((acc, d) => acc + (d.entries / 150) * 100, 0) / stats.daily.length
    : 0

  const radialOptions: ApexOptions = {
    chart: { type: 'radialBar', background: 'transparent' },
    theme: { mode: chartTheme },
    plotOptions: {
      radialBar: {
        hollow: { size: '60%' },
        dataLabels: {
          name: { color: textColor },
          value: { color: textColor, fontSize: '24px', fontWeight: 700, formatter: (v) => `${v.toFixed(1)}%` },
        },
      },
    },
    labels: ['Taux d\'occupation'],
    colors: [avgOccupancy > 80 ? '#C62828' : avgOccupancy > 60 ? '#E65100' : '#2E7D32'],
  }

  const radialSeries = [parseFloat(avgOccupancy.toFixed(1))]

  // Heatmap: occupation by hour/day
  const days = ['Lun', 'Mar', 'Mer', 'Jeu', 'Ven', 'Sam', 'Dim']
  const heatmapSeries = days.map((day) => ({
    name: day,
    data: Array.from({ length: 24 }, (_, h) => ({
      x: `${String(h).padStart(2, '0')}h`,
      y: Math.floor(Math.random() * 100),
    })),
  }))

  const heatmapOptions: ApexOptions = {
    chart: { type: 'heatmap', toolbar: { show: false }, background: 'transparent' },
    theme: { mode: chartTheme },
    dataLabels: { enabled: false },
    colors: ['#1565C0'],
    xaxis: { ...commonAxis },
    yaxis: { ...commonAxis },
    tooltip: { theme: chartTheme, y: { formatter: (v) => `${v}%` } },
  }

  // ── Durées ───────────────────────────────────────────────────────────────────
  const durationBarOptions: ApexOptions = {
    chart: { type: 'bar', toolbar: { show: false }, background: 'transparent' },
    theme: { mode: chartTheme },
    plotOptions: { bar: { borderRadius: 4, horizontal: false, columnWidth: '55%' } },
    xaxis: { ...commonAxis, categories: stats?.daily.slice(-14).map((d) => d.date.slice(5)) ?? [] },
    yaxis: { ...commonAxis, labels: { ...commonAxis.labels, formatter: (v) => `${v}min` } },
    grid: { borderColor: gridColor, strokeDashArray: 4 },
    colors: ['#4527A0'],
    dataLabels: { enabled: false },
    tooltip: { theme: chartTheme, y: { formatter: (v) => `${v} min` } },
  }

  const durationBarSeries = [
    { name: 'Durée moyenne', data: stats?.daily.slice(-14).map((d) => d.avgDuration) ?? [] },
  ]

  return (
    <Box>
      <Box sx={{ mb: 3 }}>
        <Typography variant="h5" fontWeight={700}>Statistiques</Typography>
        <Typography variant="body2" color="text.secondary">
          Analyse détaillée des données de stationnement
        </Typography>
      </Box>

      {error && <Alert severity="warning" sx={{ mb: 2 }}>{error}</Alert>}

      <Card>
        <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
          <Tabs value={tab} onChange={(_, v) => setTab(v)} variant="scrollable" scrollButtons="auto">
            <Tab label="Revenus" />
            <Tab label="Flux" />
            <Tab label="Occupation" />
            <Tab label="Durées" />
          </Tabs>
        </Box>

        <CardContent>
          {/* Revenus */}
          <TabPanel value={tab} index={0}>
            <Grid container spacing={2}>
              <Grid item xs={12}>
                <Typography variant="subtitle1" fontWeight={600} gutterBottom>
                  Revenus journaliers — 30 derniers jours
                </Typography>
                {loading ? <Skeleton variant="rectangular" height={280} sx={{ borderRadius: 2 }} /> : (
                  <ReactApexChart options={revenueLineOptions} series={revenueLineSeries} type="line" height={280} />
                )}
              </Grid>
              <Grid item xs={12} md={6}>
                <Typography variant="subtitle1" fontWeight={600} gutterBottom>
                  CA mensuel
                </Typography>
                {loading ? <Skeleton variant="rectangular" height={240} sx={{ borderRadius: 2 }} /> : (
                  <ReactApexChart options={revenueBarOptions} series={revenueBarSeries} type="bar" height={240} />
                )}
              </Grid>
              <Grid item xs={12} md={6}>
                <Typography variant="subtitle1" fontWeight={600} gutterBottom>
                  Répartition par type
                </Typography>
                {loading ? <Skeleton variant="rectangular" height={240} sx={{ borderRadius: 2 }} /> : (
                  <ReactApexChart
                    options={{
                      chart: { type: 'pie', background: 'transparent' },
                      theme: { mode: chartTheme },
                      labels: ['Horaire', 'Abonnement', 'Journalier'],
                      colors: ['#1565C0', '#2E7D32', '#E65100'],
                      legend: { labels: { colors: textColor } },
                      tooltip: { theme: chartTheme },
                    }}
                    series={[45, 35, 20]}
                    type="pie"
                    height={240}
                  />
                )}
              </Grid>
            </Grid>
          </TabPanel>

          {/* Flux */}
          <TabPanel value={tab} index={1}>
            <Grid container spacing={2}>
              <Grid item xs={12}>
                <Typography variant="subtitle1" fontWeight={600} gutterBottom>
                  Flux horaire — Entrées & Sorties
                </Typography>
                {loading ? <Skeleton variant="rectangular" height={280} sx={{ borderRadius: 2 }} /> : (
                  <ReactApexChart options={fluxBarOptions} series={fluxBarSeries} type="bar" height={280} />
                )}
              </Grid>
              <Grid item xs={12}>
                <Typography variant="subtitle1" fontWeight={600} gutterBottom>
                  Tendance hebdomadaire
                </Typography>
                {loading ? <Skeleton variant="rectangular" height={240} sx={{ borderRadius: 2 }} /> : (
                  <ReactApexChart options={weeklyLineOptions} series={weeklyLineSeries} type="line" height={240} />
                )}
              </Grid>
            </Grid>
          </TabPanel>

          {/* Occupation */}
          <TabPanel value={tab} index={2}>
            <Grid container spacing={2}>
              <Grid item xs={12} md={4}>
                <Typography variant="subtitle1" fontWeight={600} gutterBottom>
                  Taux d'occupation moyen
                </Typography>
                {loading ? <Skeleton variant="circular" width={200} height={200} sx={{ mx: 'auto' }} /> : (
                  <ReactApexChart options={radialOptions} series={radialSeries} type="radialBar" height={300} />
                )}
              </Grid>
              <Grid item xs={12} md={8}>
                <Typography variant="subtitle1" fontWeight={600} gutterBottom>
                  Carte de chaleur — Occupation par heure/jour
                </Typography>
                {loading ? <Skeleton variant="rectangular" height={280} sx={{ borderRadius: 2 }} /> : (
                  <ReactApexChart options={heatmapOptions} series={heatmapSeries} type="heatmap" height={280} />
                )}
              </Grid>
            </Grid>
          </TabPanel>

          {/* Durées */}
          <TabPanel value={tab} index={3}>
            <Grid container spacing={2}>
              <Grid item xs={12}>
                <Typography variant="subtitle1" fontWeight={600} gutterBottom>
                  Durée moyenne de stationnement — 14 derniers jours
                </Typography>
                {loading ? <Skeleton variant="rectangular" height={280} sx={{ borderRadius: 2 }} /> : (
                  <ReactApexChart options={durationBarOptions} series={durationBarSeries} type="bar" height={280} />
                )}
              </Grid>
              <Grid item xs={12} md={6}>
                <Card variant="outlined">
                  <CardContent>
                    <Typography variant="subtitle2" color="text.secondary" gutterBottom>
                      Statistiques de durée
                    </Typography>
                    {[
                      { label: 'Durée moyenne globale', value: `${Math.round((stats?.daily.reduce((a, d) => a + d.avgDuration, 0) ?? 0) / (stats?.daily.length || 1))} min` },
                      { label: 'Durée minimale', value: '8 min' },
                      { label: 'Durée maximale', value: '720 min' },
                      { label: 'Médiane', value: '65 min' },
                    ].map((item) => (
                      <Box key={item.label} sx={{ display: 'flex', justifyContent: 'space-between', py: 1, borderBottom: `1px solid ${theme.palette.divider}`, '&:last-child': { borderBottom: 'none' } }}>
                        <Typography variant="body2" color="text.secondary">{item.label}</Typography>
                        <Typography variant="body2" fontWeight={600}>{item.value}</Typography>
                      </Box>
                    ))}
                  </CardContent>
                </Card>
              </Grid>
            </Grid>
          </TabPanel>
        </CardContent>
      </Card>
    </Box>
  )
}
