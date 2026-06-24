import React from 'react'
import { useNavigate, useLocation } from 'react-router-dom'
import {
  Drawer,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Toolbar,
  Divider,
  Typography,
  Box,
  useTheme,
} from '@mui/material'
import DashboardIcon from '@mui/icons-material/Dashboard'
import BarChartIcon from '@mui/icons-material/BarChart'
import ConfirmationNumberIcon from '@mui/icons-material/ConfirmationNumber'
import PeopleIcon from '@mui/icons-material/People'
import AssessmentIcon from '@mui/icons-material/Assessment'
import BuildIcon from '@mui/icons-material/Build'
import ManageAccountsIcon from '@mui/icons-material/ManageAccounts'
import SettingsIcon from '@mui/icons-material/Settings'
import LocalParkingIcon from '@mui/icons-material/LocalParking'

const DRAWER_WIDTH = 240

const navItems = [
  { label: 'Tableau de bord', icon: <DashboardIcon />, path: '/dashboard' },
  { label: 'Statistiques', icon: <BarChartIcon />, path: '/statistics' },
  { label: 'Tickets', icon: <ConfirmationNumberIcon />, path: '/tickets' },
  { label: 'Abonnés', icon: <PeopleIcon />, path: '/subscribers' },
  { label: 'Rapports', icon: <AssessmentIcon />, path: '/reports' },
  { label: 'Maintenance', icon: <BuildIcon />, path: '/maintenance' },
  { label: 'Utilisateurs', icon: <ManageAccountsIcon />, path: '/users' },
  { label: 'Paramètres', icon: <SettingsIcon />, path: '/settings' },
]

interface SidebarProps {
  open: boolean
  variant?: 'permanent' | 'temporary'
  onClose?: () => void
}

export default function Sidebar({ open, variant = 'permanent', onClose }: SidebarProps) {
  const navigate = useNavigate()
  const location = useLocation()
  const theme = useTheme()

  const drawerContent = (
    <>
      <Toolbar sx={{ gap: 1 }}>
        <LocalParkingIcon sx={{ color: 'primary.main', fontSize: 32 }} />
        <Box>
          <Typography variant="subtitle1" fontWeight={700} lineHeight={1.2}>
            ZKTeco
          </Typography>
          <Typography variant="caption" color="text.secondary" lineHeight={1}>
            Parking Dashboard
          </Typography>
        </Box>
      </Toolbar>
      <Divider />
      <List sx={{ px: 1, pt: 1 }}>
        {navItems.map((item) => {
          const active = location.pathname === item.path
          return (
            <ListItem key={item.path} disablePadding sx={{ mb: 0.5 }}>
              <ListItemButton
                onClick={() => {
                  navigate(item.path)
                  onClose?.()
                }}
                selected={active}
                sx={{
                  borderRadius: 2,
                  '&.Mui-selected': {
                    bgcolor: 'primary.main',
                    color: 'white',
                    '& .MuiListItemIcon-root': { color: 'white' },
                    '&:hover': { bgcolor: 'primary.dark' },
                  },
                }}
              >
                <ListItemIcon
                  sx={{
                    minWidth: 40,
                    color: active ? 'white' : 'text.secondary',
                  }}
                >
                  {item.icon}
                </ListItemIcon>
                <ListItemText
                  primary={item.label}
                  primaryTypographyProps={{ fontSize: 14, fontWeight: active ? 600 : 400 }}
                />
              </ListItemButton>
            </ListItem>
          )
        })}
      </List>
      <Box sx={{ flexGrow: 1 }} />
      <Box sx={{ p: 2 }}>
        <Typography variant="caption" color="text.disabled">
          v1.0.0 — {new Date().getFullYear()}
        </Typography>
      </Box>
    </>
  )

  return (
    <Drawer
      variant={variant}
      open={open}
      onClose={onClose}
      sx={{
        width: open || variant === 'permanent' ? DRAWER_WIDTH : 0,
        flexShrink: 0,
        '& .MuiDrawer-paper': {
          width: DRAWER_WIDTH,
          boxSizing: 'border-box',
          borderRight: `1px solid ${theme.palette.divider}`,
          display: 'flex',
          flexDirection: 'column',
        },
      }}
    >
      {drawerContent}
    </Drawer>
  )
}

export { DRAWER_WIDTH }
