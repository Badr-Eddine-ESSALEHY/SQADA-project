import axios from 'axios'

const BASE_URL = import.meta.env.VITE_API_URL || '/api'

const api = axios.create({
  baseURL: BASE_URL,
  timeout: 10000,
  headers: { 'Content-Type': 'application/json' },
})

// Request interceptor: automatically attach JWT Bearer Token
api.interceptors.request.use((config) => {
  try {
    const stored = localStorage.getItem('parkingUser')
    if (stored) {
      const user = JSON.parse(stored)
      if (user?.token) {
        config.headers.Authorization = `Bearer ${user.token}`
      }
    }
  } catch (err) {
    console.error("Error attaching auth header", err)
  }
  return config
})

// Response interceptor: automatically refresh expired sessions
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
          const updated = { ...user, token: res.data.accessToken || res.data.token }
          localStorage.setItem('parkingUser', JSON.stringify(updated))
          original.headers.Authorization = `Bearer ${updated.token}`
          return api(original)
        }
      } catch {
        localStorage.removeItem('parkingUser')
        window.location.href = '/login'
      }
    }
    return Promise.reject(error)
  }
)

export const authApi = {
  login: (credentials: Record<string, unknown>) => api.post('/auth/login', credentials),
  refresh: (data: Record<string, unknown>) => api.post('/auth/refresh', data),
}

export const dashboardApi = {
  getStats: (rangeDays = 30) => api.get(`/dashboard/stats?rangeDays=${rangeDays}`),
  getRevenue: (rangeDays = 30) => api.get(`/dashboard/revenue?rangeDays=${rangeDays}`),
  getRevenueChart: (days = 30) => api.get(`/dashboard/revenue?days=${days}`),
}

export const ticketsApi = {
  getAll: (params?: Record<string, unknown>) => api.get('/tickets', { params }),
  getById: (id: string) => api.get(`/tickets/${id}`),
  entry: (data: Record<string, unknown>) => api.post('/tickets/entry', data),
  exit: (id: string, data: Record<string, unknown>) => api.post(`/tickets/${id}/exit`, data),
  pay: (id: string, data: Record<string, unknown>) => api.post(`/tickets/${id}/pay`, data),
  export: (params?: Record<string, unknown>) => api.get('/tickets/export', { params, responseType: 'blob' }),
}

export const subscribersApi = {
  getAll: () => api.get('/subscribers'),
  create: (data: Record<string, unknown>) => api.post('/subscribers', data),
  update: (id: string, data: Record<string, unknown>) => api.put(`/subscribers/${id}`, data),
  delete: (id: string) => api.delete(`/subscribers/${id}`),
  renew: (id: string, months: number) => api.post(`/subscribers/${id}/renew`, { months }),
}

export const statisticsApi = {
  getAll: (params?: Record<string, unknown>) => api.get('/statistics', { params }),
}

export const reportsApi = {
  getRecent: () => api.get('/reports/recent'),
  generate: (type: string, params: Record<string, unknown>) =>
    api.post(`/reports/${type}`, params, { responseType: 'blob' }),
}

export const maintenanceApi = {
  getStatus: () => api.get('/maintenance/status'),
  markAlertRead: (id: string) => api.put(`/maintenance/alerts/${id}/read`),
}

export const usersApi = {
  getAll: () => api.get('/users'),
  getProfile: () => api.get('/users/profile'),
  updateProfile: (data: Record<string, unknown>) => api.put('/users/profile', data),
}

export default api