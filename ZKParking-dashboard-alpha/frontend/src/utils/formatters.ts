import { format, parseISO, formatDistanceToNow } from 'date-fns'
import { fr } from 'date-fns/locale'

/**
 * Format a number as French currency: 1 234,56 €
 */
export function formatCurrency(amount: number | null | undefined): string {
  if (amount == null) return '—'
  return new Intl.NumberFormat('fr-FR', {
    style: 'currency',
    currency: 'EUR',
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(amount)
}

/**
 * Format duration in minutes to "2h 30min" or "45min"
 */
export function formatDuration(minutes: number | null | undefined): string {
  if (minutes == null) return '—'
  if (minutes < 60) return `${minutes}min`
  const h = Math.floor(minutes / 60)
  const m = minutes % 60
  return m > 0 ? `${h}h ${m}min` : `${h}h`
}

/**
 * Format ISO date string to localized French date: "15 janv. 2024 14:30"
 */
export function formatDate(date: string | Date | null | undefined): string {
  if (!date) return '—'
  try {
    const d = typeof date === 'string' ? parseISO(date) : date
    return format(d, 'dd MMM yyyy HH:mm', { locale: fr })
  } catch {
    return '—'
  }
}

/**
 * Format ISO date string to short date: "15/01/2024"
 */
export function formatShortDate(date: string | Date | null | undefined): string {
  if (!date) return '—'
  try {
    const d = typeof date === 'string' ? parseISO(date) : date
    return format(d, 'dd/MM/yyyy', { locale: fr })
  } catch {
    return '—'
  }
}

/**
 * Format ISO date string to time only: "14:30"
 */
export function formatTime(date: string | Date | null | undefined): string {
  if (!date) return '—'
  try {
    const d = typeof date === 'string' ? parseISO(date) : date
    return format(d, 'HH:mm', { locale: fr })
  } catch {
    return '—'
  }
}

/**
 * Format relative time: "il y a 5 minutes"
 */
export function formatRelativeTime(date: string | Date | null | undefined): string {
  if (!date) return '—'
  try {
    const d = typeof date === 'string' ? parseISO(date) : date
    return formatDistanceToNow(d, { addSuffix: true, locale: fr })
  } catch {
    return '—'
  }
}

/**
 * Format plate to uppercase with dashes: "AB-123-CD"
 */
export function formatPlate(plate: string | null | undefined): string {
  if (!plate) return '—'
  return plate.toUpperCase().replace(/\s+/g, '-')
}

/**
 * Format a percentage: "75,4 %"
 */
export function formatPercent(value: number | null | undefined): string {
  if (value == null) return '—'
  return new Intl.NumberFormat('fr-FR', {
    style: 'percent',
    minimumFractionDigits: 1,
    maximumFractionDigits: 1,
  }).format(value / 100)
}

/**
 * Format a large number with French thousands separator: "1 234"
 */
export function formatNumber(value: number | null | undefined): string {
  if (value == null) return '—'
  return new Intl.NumberFormat('fr-FR').format(value)
}
