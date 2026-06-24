import axios from 'axios'

const BASE_URL = import.meta.env.VITE_API_URL || '/api'

const api = axios.create({
  baseURL: BASE_URL,
  timeout: 10000,
  headers: { 'Content-Type': 'application/json' },
})

// Request interceptor: attach JWT
api.interceptors.request.use((config) => {
  try {
    const stored = localStorage.getItem('parkingUser')
    if (stored) {
      const user = JSON.parse(stored)
      if (user?.token) {
        config.headers.Authorization = `Bearer ${user.token}`
      }
    }
  } catch {}
  return config
})

// Response interceptor: handle 401
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const original = error.config
    if (error.response?.status === 401 && !original._retry) {
      original._retry = true
      try {
        const stored = localStorage.getItem('parkingUser')
        if (stored) {
          const user = JSON.parse(stored)
          const res = await axios.post(`${BASE_URL}/auth/refresh`, {
            refreshToken: user.refreshToken,
          })
          const updated = { ...user, token: res.data.token }
          localStorage.setItem('parkingUser', JSON.stringify(updated))
          original.headers.Authorization = `Bearer ${res.data.token}`
          return api(original)
        }
      } catch {
        localStorage.removeItem('parkingUser')
        window.location.href = '/login'
      }
    }
    return Promise.reject(error)
  },
)

// ─── Mock data generators ────────────────────────────────────────────────────

function mockDashboardStats() {
  return {
    entriesToday: Math.floor(Math.random() * 200) + 150,
    exitsToday: Math.floor(Math.random() * 180) + 130,
    currentOccupancy: Math.floor(Math.random() * 80) + 40,
    availableSpaces: Math.floor(Math.random() * 60) + 20,
    revenueToday: Math.random() * 2000 + 800,
    revenueMonth: Math.random() * 40000 + 20000,
    activeSubscribers: Math.floor(Math.random() * 50) + 80,
    occupancyRate: Math.random() * 30 + 60,
    totalCapacity: 150,
  }
}

function mockTickets(page = 1, pageSize = 20) {
  const statuses = ['Payé', 'En cours', 'Perdu', 'Illisible']
  const types = ['Horaire', 'Abonnement', 'Journalier']
  const plates = ['AB-123-CD', 'EF-456-GH', 'IJ-789-KL', 'MN-012-OP', 'QR-345-ST']
  const items = Array.from({ length: pageSize }, (_, i) => {
    const entry = new Date(Date.now() - Math.random() * 86400000 * 7)
    const exit = new Date(entry.getTime() + Math.random() * 14400000)
    const duration = Math.floor((exit.getTime() - entry.getTime()) / 60000)
    const status = statuses[Math.floor(Math.random() * statuses.length)]
    return {
      id: `T${(page - 1) * pageSize + i + 1}`,
      ticketNumber: `PKG-${String((page - 1) * pageSize + i + 1).padStart(6, '0')}`,
      entryDate: entry.toISOString(),
      exitDate: status === 'En cours' ? null : exit.toISOString(),
      duration: status === 'En cours' ? null : duration,
      plate: plates[Math.floor(Math.random() * plates.length)],
      type: types[Math.floor(Math.random() * types.length)],
      amount: status === 'En cours' ? null : Math.round(duration * 0.05 * 100) / 100,
      status,
    }
  })
  return { items, total: 500, page, pageSize }
}

function mockSubscribers() {
  const types = ['Mensuel', 'Trimestriel', 'Annuel']
  const statuses = ['Actif', 'Expiré', 'En attente']
  return Array.from({ length: 30 }, (_, i) => {
    const start = new Date(Date.now() - Math.random() * 86400000 * 90)
    const months = [1, 3, 12][Math.floor(Math.random() * 3)]
    const end = new Date(start.getTime() + months * 30 * 86400000)
    return {
      id: `S${i + 1}`,
      firstName: ['Jean', 'Marie', 'Pierre', 'Sophie', 'Lucas'][i % 5],
      lastName: ['Dupont', 'Martin', 'Bernard', 'Petit', 'Robert'][i % 5],
      phone: `06${String(Math.floor(Math.random() * 100000000)).padStart(8, '0')}`,
      plate: `AB-${String(i + 1).padStart(3, '0')}-CD`,
      subscriptionType: types[Math.floor(Math.random() * types.length)],
      startDate: start.toISOString(),
      endDate: end.toISOString(),
      status: statuses[Math.floor(Math.random() * statuses.length)],
      email: `user${i + 1}@example.com`,
    }
  })
}

function mockStatistics() {
  const days = Array.from({ length: 30 }, (_, i) => {
    const d = new Date()
    d.setDate(d.getDate() - (29 - i))
    return {
      date: d.toISOString().split('T')[0],
      revenue: Math.random() * 1500 + 500,
      entries: Math.floor(Math.random() * 150) + 50,
      exits: Math.floor(Math.random() * 140) + 45,
      avgDuration: Math.floor(Math.random() * 120) + 30,
    }
  })
  const hourly = Array.from({ length: 24 }, (_, h) => ({
    hour: h,
    entries: Math.floor(Math.random() * 20) + (h >= 7 && h <= 19 ? 10 : 0),
    exits: Math.floor(Math.random() * 18) + (h >= 7 && h <= 19 ? 8 : 0),
  }))
  return { daily: days, hourly }
}

function mockMaintenance() {
  const gates = [
    { id: 'G1', name: 'Barrière Entrée 1', type: 'Entrée', ip: '192.168.1.10', status: 'Online', lastPing: new Date().toISOString() },
    { id: 'G2', name: 'Barrière Entrée 2', type: 'Entrée', ip: '192.168.1.11', status: 'Online', lastPing: new Date().toISOString() },
    { id: 'G3', name: 'Barrière Sortie 1', type: 'Sortie', ip: '192.168.1.20', status: 'Offline', lastPing: new Date(Date.now() - 300000).toISOString() },
    { id: 'G4', name: 'Barrière Sortie 2', type: 'Sortie', ip: '192.168.1.21', status: 'Error', lastPing: new Date(Date.now() - 60000).toISOString() },
  ]
  const terminals = [
    { id: 'T1', name: 'Terminal Caisse 1', ip: '192.168.1.30', status: 'Online', lastPing: new Date().toISOString() },
    { id: 'T2', name: 'Terminal Caisse 2', ip: '192.168.1.31', status: 'Maintenance', lastPing: new Date(Date.now() - 120000).toISOString() },
    { id: 'T3', name: 'Terminal Entrée 1', ip: '192.168.1.32', status: 'Online', lastPing: new Date().toISOString() },
  ]
  const alerts = [
    { id: 'A1', severity: 'error', message: 'Barrière Sortie 1 hors ligne depuis 5 minutes', time: new Date(Date.now() - 300000).toISOString(), read: false },
    { id: 'A2', severity: 'warning', message: 'Terminal Caisse 2 en maintenance programmée', time: new Date(Date.now() - 120000).toISOString(), read: false },
    { id: 'A3', severity: 'error', message: 'Erreur de communication Barrière Sortie 2', time: new Date(Date.now() - 60000).toISOString(), read: false },
    { id: 'A4', severity: 'info', message: 'Mise à jour firmware disponible pour Terminal Entrée 1', time: new Date(Date.now() - 3600000).toISOString(), read: true },
  ]
  return { gates, terminals, alerts }
}

function mockUsers() {
  const roles = ['Administrateur', 'Gestionnaire', 'Opérateur', 'Lecture seule']
  return Array.from({ length: 10 }, (_, i) => ({
    id: `U${i + 1}`,
    firstName: ['Admin', 'Jean', 'Marie', 'Pierre', 'Sophie', 'Lucas', 'Emma', 'Paul', 'Claire', 'Marc'][i],
    lastName: ['Système', 'Dupont', 'Martin', 'Bernard', 'Petit', 'Robert', 'Leroy', 'Simon', 'Michel', 'Laurent'][i],
    email: `user${i + 1}@parking.fr`,
    role: roles[i % roles.length],
    status: i < 8 ? 'Actif' : 'Inactif',
    lastLogin: new Date(Date.now() - Math.random() * 86400000 * 7).toISOString(),
  }))
}

// ─── API functions ────────────────────────────────────────────────────────────

async function tryApi<T>(apiCall: () => Promise<T>, mockFn: () => T): Promise<T> {
  try {
    return await apiCall()
  } catch {
    return mockFn()
  }
}

export const dashboardApi = {
  getStats: () =>
    tryApi(
      async () => (await api.get('/dashboard/stats')).data,
      mockDashboardStats,
    ),
  getRevenueChart: (days = 30) =>
    tryApi(
      async () => (await api.get(`/dashboard/revenue?days=${days}`)).data,
      () => mockStatistics().daily.map((d) => ({ date: d.date, value: d.revenue })),
    ),
}

export const ticketsApi = {
  getAll: (params?: Record<string, unknown>) =>
    tryApi(
      async () => (await api.get('/tickets', { params })).data,
      () => mockTickets(Number(params?.page) || 1, Number(params?.pageSize) || 20),
    ),
  getById: (id: string) =>
    tryApi(
      async () => (await api.get(`/tickets/${id}`)).data,
      () => mockTickets(1, 1).items[0],
    ),
  export: (params?: Record<string, unknown>) =>
    api.get('/tickets/export', { params, responseType: 'blob' }),
}

export const subscribersApi = {
  getAll: () =>
    tryApi(
      async () => (await api.get('/subscribers')).data,
      mockSubscribers,
    ),
  create: (data: Record<string, unknown>) => api.post('/subscribers', data),
  update: (id: string, data: Record<string, unknown>) => api.put(`/subscribers/${id}`, data),
  delete: (id: string) => api.delete(`/subscribers/${id}`),
  renew: (id: string, months: number) => api.post(`/subscribers/${id}/renew`, { months }),
}

export const statisticsApi = {
  getAll: (params?: Record<string, unknown>) =>
    tryApi(
      async () => (await api.get('/statistics', { params })).data,
      mockStatistics,
    ),
}

export const reportsApi = {
  generate: (type: string, params: Record<string, unknown>) =>
    api.post(`/reports/${type}`, params, { responseType: 'blob' }),
}

export const maintenanceApi = {
  getStatus: () =>
    tryApi(
      async () => (await api.get('/maintenance/status')).data,
      mockMaintenance,
    ),
  markAlertRead: (id: string) => api.put(`/maintenance/alerts/${id}/read`),
}

export const usersApi = {
  getAll: () =>
    tryApi(
      async () => (await api.get('/users')).data,
      mockUsers,
    ),
  create: (data: Record<string, unknown>) => api.post('/users', data),
  update: (id: string, data: Record<string, unknown>) => api.put(`/users/${id}`, data),
  delete: (id: string) => api.delete(`/users/${id}`),
}

export default api
