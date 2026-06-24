import React from 'react'
import {
  Card,
  CardContent,
  Box,
  Typography,
  Skeleton,
  Tooltip,
} from '@mui/material'
import TrendingUpIcon from '@mui/icons-material/TrendingUp'
import TrendingDownIcon from '@mui/icons-material/TrendingDown'
import TrendingFlatIcon from '@mui/icons-material/TrendingFlat'

type CardVariant = 'primary' | 'success' | 'warning' | 'error' | 'info' | 'secondary'

interface StatCardProps {
  title: string
  value: string | number
  subtitle?: string
  icon: React.ReactNode
  variant?: CardVariant
  trend?: number
  loading?: boolean
}

const GRADIENTS: Record<CardVariant, string> = {
  primary: 'linear-gradient(135deg, #1565C0 0%, #1976D2 100%)',
  success: 'linear-gradient(135deg, #2E7D32 0%, #388E3C 100%)',
  warning: 'linear-gradient(135deg, #E65100 0%, #F57C00 100%)',
  error: 'linear-gradient(135deg, #C62828 0%, #D32F2F 100%)',
  info: 'linear-gradient(135deg, #0277BD 0%, #0288D1 100%)',
  secondary: 'linear-gradient(135deg, #4527A0 0%, #5E35B1 100%)',
}

export default function StatCard({
  title,
  value,
  subtitle,
  icon,
  variant = 'primary',
  trend,
  loading = false,
}: StatCardProps) {
  const gradient = GRADIENTS[variant]

  const TrendIcon =
    trend == null ? null
    : trend > 0 ? TrendingUpIcon
    : trend < 0 ? TrendingDownIcon
    : TrendingFlatIcon

  const trendColor = trend == null ? undefined : trend > 0 ? '#69F0AE' : trend < 0 ? '#FF5252' : '#FFD740'

  return (
    <Card
      sx={{
        background: gradient,
        color: 'white',
        height: '100%',
        position: 'relative',
        overflow: 'hidden',
        '&::after': {
          content: '""',
          position: 'absolute',
          top: -20,
          right: -20,
          width: 100,
          height: 100,
          borderRadius: '50%',
          background: 'rgba(255,255,255,0.08)',
        },
        '&::before': {
          content: '""',
          position: 'absolute',
          bottom: -30,
          right: 20,
          width: 60,
          height: 60,
          borderRadius: '50%',
          background: 'rgba(255,255,255,0.05)',
        },
      }}
    >
      <CardContent sx={{ p: 2.5, '&:last-child': { pb: 2.5 } }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
          <Box sx={{ flex: 1, minWidth: 0 }}>
            <Typography
              variant="caption"
              sx={{ opacity: 0.85, textTransform: 'uppercase', letterSpacing: 0.8, fontSize: 11 }}
            >
              {title}
            </Typography>
            {loading ? (
              <Skeleton variant="text" width={80} height={40} sx={{ bgcolor: 'rgba(255,255,255,0.2)' }} />
            ) : (
              <Typography variant="h5" fontWeight={700} sx={{ mt: 0.5, lineHeight: 1.2 }}>
                {value}
              </Typography>
            )}
            {subtitle && (
              <Typography variant="caption" sx={{ opacity: 0.75, mt: 0.5, display: 'block' }}>
                {subtitle}
              </Typography>
            )}
          </Box>
          <Box
            sx={{
              display: 'flex',
              flexDirection: 'column',
              alignItems: 'flex-end',
              gap: 0.5,
            }}
          >
            <Box
              sx={{
                bgcolor: 'rgba(255,255,255,0.2)',
                borderRadius: 2,
                p: 1,
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
              }}
            >
              {icon}
            </Box>
            {TrendIcon && trend != null && (
              <Tooltip title={`${trend > 0 ? '+' : ''}${trend.toFixed(1)}% vs hier`}>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.3 }}>
                  <TrendIcon sx={{ fontSize: 14, color: trendColor }} />
                  <Typography variant="caption" sx={{ color: trendColor, fontWeight: 600, fontSize: 11 }}>
                    {Math.abs(trend).toFixed(1)}%
                  </Typography>
                </Box>
              </Tooltip>
            )}
          </Box>
        </Box>
      </CardContent>
    </Card>
  )
}
