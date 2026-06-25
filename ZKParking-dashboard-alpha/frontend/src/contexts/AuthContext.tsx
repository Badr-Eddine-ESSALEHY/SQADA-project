import React, { createContext, useContext, useState, useEffect, useCallback } from 'react'
import axios from 'axios'

export interface User {
  id: string
  username: string
  email: string
  role: 'Administrateur' | 'Gestionnaire' | 'Opérateur' | 'Lecture seule'
  token: string
  refreshToken: string
}

interface AuthContextType {
  user: User | null
  loading: boolean
  login: (username: string, password: string) => Promise<void>
  logout: () => void
  refreshToken: () => Promise<void>
}

const AuthContext = createContext<AuthContextType | undefined>(undefined)

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<User | null>(() => {
    try {
      const stored = localStorage.getItem('parkingUser')
      return stored ? JSON.parse(stored) : null
    } catch {
      return null
    }
  })
  const [loading, setLoading] = useState(false)

  const login = useCallback(async (username: string, password: string) => {
    setLoading(true)
    try {
      let userData: User
      try {
        const response = await axios.post(
          `${import.meta.env.VITE_API_URL || '/api'}/auth/login`,
          { username, password },
          { timeout: 5000 },
        )
        const data = response.data
        userData = {
          id: String(data.operatorId || data.id || '1'),
          username: data.username || username,
          email: data.email || `${username}@parking.fr`,
          role: data.role === 'Admin' ? 'Administrateur' : data.role || 'Opérateur',
          token: data.accessToken || data.token,
          refreshToken: data.refreshToken,
        }
      } catch {
        // Mock authentication for demo purposes
        if (password.length < 1) throw new Error('Mot de passe requis')
        userData = {
          id: '1',
          username,
          email: `${username}@parking.fr`,
          role: username === 'admin' ? 'Administrateur' : 'Opérateur',
          token: `mock-jwt-token-${Date.now()}`,
          refreshToken: `mock-refresh-token-${Date.now()}`,
        }
      }
      setUser(userData)
      localStorage.setItem('parkingUser', JSON.stringify(userData))
    } finally {
      setLoading(false)
    }
  }, [])

  const logout = useCallback(() => {
    setUser(null)
    localStorage.removeItem('parkingUser')
  }, [])

  const refreshToken = useCallback(async () => {
    if (!user?.refreshToken) return
    try {
      const response = await axios.post(
        `${import.meta.env.VITE_API_URL || '/api'}/auth/refresh`,
        { refreshToken: user.refreshToken },
      )
      const updated = { ...user, token: response.data.token }
      setUser(updated)
      localStorage.setItem('parkingUser', JSON.stringify(updated))
    } catch {
      logout()
    }
  }, [user, logout])

  useEffect(() => {
    if (!user) return
    const interval = setInterval(refreshToken, 14 * 60 * 1000)
    return () => clearInterval(interval)
  }, [user, refreshToken])

  return (
    <AuthContext.Provider value={{ user, loading, login, logout, refreshToken }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}