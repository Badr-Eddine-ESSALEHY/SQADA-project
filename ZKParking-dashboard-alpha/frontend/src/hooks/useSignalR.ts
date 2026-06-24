import { useEffect, useRef, useState, useCallback } from 'react'
import { HubConnectionState } from '@microsoft/signalr'
import { parkingSignalR } from '../services/signalr'
import { useAuth } from '../contexts/AuthContext'

interface UseSignalRReturn {
  connectionState: HubConnectionState
  isConnected: boolean
  joinParking: (parkingId: string) => Promise<void>
  leaveParking: (parkingId: string) => Promise<void>
}

export function useSignalR(
  onEntry?: (data: unknown) => void,
  onExit?: (data: unknown) => void,
  onPayment?: (data: unknown) => void,
  onAlert?: (data: unknown) => void,
  onStats?: (data: unknown) => void,
): UseSignalRReturn {
  const { user } = useAuth()
  const [connectionState, setConnectionState] = useState<HubConnectionState>(
    HubConnectionState.Disconnected,
  )
  const mountedRef = useRef(true)

  useEffect(() => {
    mountedRef.current = true
    return () => { mountedRef.current = false }
  }, [])

  useEffect(() => {
    if (!user) return

    const updateState = () => {
      if (mountedRef.current) setConnectionState(parkingSignalR.state)
    }

    parkingSignalR.on('connected', updateState)
    parkingSignalR.on('disconnected', updateState)
    parkingSignalR.on('reconnecting', updateState)
    parkingSignalR.on('reconnected', updateState)

    if (onEntry) parkingSignalR.onEntryEvent(onEntry)
    if (onExit) parkingSignalR.onExitEvent(onExit)
    if (onPayment) parkingSignalR.onPaymentEvent(onPayment)
    if (onAlert) parkingSignalR.onAlertEvent(onAlert)
    if (onStats) parkingSignalR.onStatsUpdate(onStats)

    parkingSignalR.connect(user.token).then(updateState)

    return () => {
      parkingSignalR.off('connected', updateState)
      parkingSignalR.off('disconnected', updateState)
      parkingSignalR.off('reconnecting', updateState)
      parkingSignalR.off('reconnected', updateState)
      if (onEntry) parkingSignalR.off('entry', onEntry)
      if (onExit) parkingSignalR.off('exit', onExit)
      if (onPayment) parkingSignalR.off('payment', onPayment)
      if (onAlert) parkingSignalR.off('alert', onAlert)
      if (onStats) parkingSignalR.off('stats', onStats)
    }
  }, [user]) // eslint-disable-line react-hooks/exhaustive-deps

  const joinParking = useCallback((parkingId: string) => parkingSignalR.joinParking(parkingId), [])
  const leaveParking = useCallback((parkingId: string) => parkingSignalR.leaveParking(parkingId), [])

  return {
    connectionState,
    isConnected: connectionState === HubConnectionState.Connected,
    joinParking,
    leaveParking,
  }
}
