import { useState, useEffect, useCallback, useRef } from 'react'
import { dashboardApi } from '../services/api'
import type { DashboardStats } from '../types'

interface UseDashboardReturn {
  stats: DashboardStats | null
  revenueData: { date: string; value: number }[]
  loading: boolean
  error: string | null
  refresh: () => void
}

const DEFAULT_STATS: DashboardStats = {
  entriesToday: 0,
  exitsToday: 0,
  currentOccupancy: 0,
  availableSpaces: 0,
  revenueToday: 0,
  revenueMonth: 0,
  activeSubscribers: 0,
  occupancyRate: 0,
  totalCapacity: 150,
}

export function useDashboard(refreshInterval = 30000): UseDashboardReturn {
  const [stats, setStats] = useState<DashboardStats | null>(null)
  const [revenueData, setRevenueData] = useState<{ date: string; value: number }[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const mountedRef = useRef(true)

  useEffect(() => {
    mountedRef.current = true
    return () => { mountedRef.current = false }
  }, [])

  const fetchData = useCallback(async () => {
    try {
      setError(null)
      const [statsData, revenue] = await Promise.all([
        dashboardApi.getStats(),
        dashboardApi.getRevenueChart(30),
      ])
      if (mountedRef.current) {
        setStats({ ...DEFAULT_STATS, ...statsData })
        setRevenueData(Array.isArray(revenue) ? revenue : [])
      }
    } catch (err) {
      if (mountedRef.current) {
        setError('Erreur lors du chargement des données')
        console.error('Dashboard fetch error:', err)
      }
    } finally {
      if (mountedRef.current) setLoading(false)
    }
  }, [])

  useEffect(() => {
    fetchData()
    const interval = setInterval(fetchData, refreshInterval)
    return () => clearInterval(interval)
  }, [fetchData, refreshInterval])

  return { stats, revenueData, loading, error, refresh: fetchData }
}
