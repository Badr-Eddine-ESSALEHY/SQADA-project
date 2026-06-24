// ─── Dashboard ────────────────────────────────────────────────────────────────
export interface DashboardStats {
  entriesToday: number
  exitsToday: number
  currentOccupancy: number
  availableSpaces: number
  revenueToday: number
  revenueMonth: number
  activeSubscribers: number
  occupancyRate: number
  totalCapacity: number
}

// ─── Tickets ──────────────────────────────────────────────────────────────────
export type TicketStatus = 'Payé' | 'En cours' | 'Perdu' | 'Illisible'
export type TicketType = 'Horaire' | 'Abonnement' | 'Journalier'

export interface ParkingTicket {
  id: string
  ticketNumber: string
  entryDate: string
  exitDate: string | null
  duration: number | null
  plate: string
  type: TicketType
  amount: number | null
  status: TicketStatus
}

export interface TicketsResponse {
  items: ParkingTicket[]
  total: number
  page: number
  pageSize: number
}

// ─── Subscribers ──────────────────────────────────────────────────────────────
export type SubscriptionType = 'Mensuel' | 'Trimestriel' | 'Annuel'
export type SubscriberStatus = 'Actif' | 'Expiré' | 'En attente'

export interface Subscriber {
  id: string
  firstName: string
  lastName: string
  phone: string
  plate: string
  subscriptionType: SubscriptionType
  startDate: string
  endDate: string
  status: SubscriberStatus
  email: string
}

// ─── Statistics ───────────────────────────────────────────────────────────────
export interface DailyStats {
  date: string
  revenue: number
  entries: number
  exits: number
  avgDuration: number
}

export interface HourlyStats {
  hour: number
  entries: number
  exits: number
}

export interface Statistics {
  daily: DailyStats[]
  hourly: HourlyStats[]
}

// ─── Maintenance ──────────────────────────────────────────────────────────────
export type DeviceStatus = 'Online' | 'Offline' | 'Error' | 'Maintenance'

export interface Gate {
  id: string
  name: string
  type: 'Entrée' | 'Sortie'
  ip: string
  status: DeviceStatus
  lastPing: string
}

export interface Terminal {
  id: string
  name: string
  ip: string
  status: DeviceStatus
  lastPing: string
}

export type AlertSeverity = 'error' | 'warning' | 'info'

export interface MaintenanceAlert {
  id: string
  severity: AlertSeverity
  message: string
  time: string
  read: boolean
}

export interface MaintenanceStatus {
  gates: Gate[]
  terminals: Terminal[]
  alerts: MaintenanceAlert[]
}

// ─── Users ────────────────────────────────────────────────────────────────────
export type UserRole = 'Administrateur' | 'Gestionnaire' | 'Opérateur' | 'Lecture seule'
export type UserStatus = 'Actif' | 'Inactif'

export interface AppUser {
  id: string
  firstName: string
  lastName: string
  email: string
  role: UserRole
  status: UserStatus
  lastLogin: string
}

// ─── Charts ───────────────────────────────────────────────────────────────────
export interface ChartDataPoint {
  x: string | number
  y: number
}

export interface ChartSeries {
  name: string
  data: ChartDataPoint[] | number[]
}

// ─── Parking ──────────────────────────────────────────────────────────────────
export interface Parking {
  id: string
  name: string
  address: string
  totalCapacity: number
}
