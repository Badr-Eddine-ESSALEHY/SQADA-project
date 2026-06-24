import React, { useState, useContext } from 'react'
import { Outlet } from 'react-router-dom'
import {
  AppBar,
  Box,
  Toolbar,
  IconButton,
  Typography,
  Avatar,
  Menu,
  MenuItem,
  Tooltip,
  Select,
  FormControl,
  InputLabel,
  Chip,
  useTheme,
  useMediaQuery,
  Divider,
} from '@mui/material'
import MenuIcon from '@mui/icons-material/Menu'
import Brightness4Icon from '@mui/icons-material/Brightness4'
import Brightness7Icon from '@mui/icons-material/Brightness7'
import AccountCircleIcon from '@mui/icons-material/AccountCircle'
import LogoutIcon from '@mui/icons-material/Logout'
import NotificationsIcon from '@mui/icons-material/Notifications'
import LocalParkingIcon from '@mui/icons-material/LocalParking'
import { useAuth } from '../../contexts/AuthContext'
import { ColorModeContext } from '../../App'
import Sidebar, { DRAWER_WIDTH } from './Sidebar'

const PARKINGS = [
  { id: 'P1', name: 'Parking Centre-Ville' },
  { id: 'P2', name: 'Parking Gare' },
  { id: 'P3', name: 'Parking Aéroport' },
]

interface LayoutProps {
  onToggleMode: () => void
}

export default function Layout({ onToggleMode }: LayoutProps) {
  const theme = useTheme()
  const isMobile = useMediaQuery(theme.breakpoints.down('md'))
  const { user, logout } = useAuth()
  const colorMode = useContext(ColorModeContext)
  const [mobileOpen, setMobileOpen] = useState(false)
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null)
  const [selectedParking, setSelectedParking] = useState('P1')

  const handleUserMenuOpen = (e: React.MouseEvent<HTMLElement>) => setAnchorEl(e.currentTarget)
  const handleUserMenuClose = () => setAnchorEl(null)

  return (
    <Box sx={{ display: 'flex', minHeight: '100vh' }}>
      {/* Sidebar */}
      {isMobile ? (
        <Sidebar
          open={mobileOpen}
          variant="temporary"
          onClose={() => setMobileOpen(false)}
        />
      ) : (
        <Sidebar open variant="permanent" />
      )}

      {/* Main area */}
      <Box
        component="main"
        sx={{
          flexGrow: 1,
          display: 'flex',
          flexDirection: 'column',
          minWidth: 0,
          ml: isMobile ? 0 : 0,
        }}
      >
        {/* AppBar */}
        <AppBar
          position="sticky"
          elevation={0}
          sx={{
            bgcolor: 'background.paper',
            borderBottom: `1px solid ${theme.palette.divider}`,
            color: 'text.primary',
            zIndex: theme.zIndex.drawer - 1,
          }}
        >
          <Toolbar sx={{ gap: 1 }}>
            {isMobile && (
              <IconButton edge="start" onClick={() => setMobileOpen(true)}>
                <MenuIcon />
              </IconButton>
            )}

            {/* Parking selector */}
            <FormControl size="small" sx={{ minWidth: 200, display: { xs: 'none', sm: 'flex' } }}>
              <InputLabel>Parking</InputLabel>
              <Select
                value={selectedParking}
                label="Parking"
                onChange={(e) => setSelectedParking(e.target.value)}
                startAdornment={<LocalParkingIcon sx={{ mr: 1, color: 'primary.main', fontSize: 18 }} />}
              >
                {PARKINGS.map((p) => (
                  <MenuItem key={p.id} value={p.id}>
                    {p.name}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>

            <Box sx={{ flexGrow: 1 }} />

            {/* Dark mode toggle */}
            <Tooltip title={theme.palette.mode === 'dark' ? 'Mode clair' : 'Mode sombre'}>
              <IconButton onClick={colorMode.toggleColorMode}>
                {theme.palette.mode === 'dark' ? <Brightness7Icon /> : <Brightness4Icon />}
              </IconButton>
            </Tooltip>

            {/* Notifications */}
            <Tooltip title="Notifications">
              <IconButton>
                <NotificationsIcon />
              </IconButton>
            </Tooltip>

            {/* User menu */}
            <Tooltip title="Mon compte">
              <IconButton onClick={handleUserMenuOpen} sx={{ p: 0.5 }}>
                <Avatar sx={{ width: 34, height: 34, bgcolor: 'primary.main', fontSize: 14 }}>
                  {user?.username?.charAt(0).toUpperCase()}
                </Avatar>
              </IconButton>
            </Tooltip>

            <Menu
              anchorEl={anchorEl}
              open={Boolean(anchorEl)}
              onClose={handleUserMenuClose}
              transformOrigin={{ horizontal: 'right', vertical: 'top' }}
              anchorOrigin={{ horizontal: 'right', vertical: 'bottom' }}
              PaperProps={{ sx: { mt: 1, minWidth: 200 } }}
            >
              <Box sx={{ px: 2, py: 1.5 }}>
                <Typography variant="subtitle2" fontWeight={600}>
                  {user?.username}
                </Typography>
                <Chip
                  label={user?.role}
                  size="small"
                  color="primary"
                  variant="outlined"
                  sx={{ mt: 0.5, fontSize: 11 }}
                />
              </Box>
              <Divider />
              <MenuItem onClick={handleUserMenuClose}>
                <AccountCircleIcon sx={{ mr: 1.5, fontSize: 20 }} />
                Mon profil
              </MenuItem>
              <MenuItem
                onClick={() => {
                  handleUserMenuClose()
                  logout()
                }}
                sx={{ color: 'error.main' }}
              >
                <LogoutIcon sx={{ mr: 1.5, fontSize: 20 }} />
                Déconnexion
              </MenuItem>
            </Menu>
          </Toolbar>
        </AppBar>

        {/* Page content */}
        <Box
          sx={{
            flexGrow: 1,
            p: { xs: 2, md: 3 },
            bgcolor: 'background.default',
            overflow: 'auto',
          }}
        >
          <Outlet />
        </Box>
      </Box>
    </Box>
  )
}
