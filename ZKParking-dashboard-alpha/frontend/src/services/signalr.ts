import * as signalR from '@microsoft/signalr'

type EventCallback = (data: unknown) => void

class ParkingSignalRService {
  private connection: signalR.HubConnection | null = null
  private callbacks: Map<string, EventCallback[]> = new Map()

  async connect(token?: string): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) return

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/parking', {
        accessTokenFactory: () => token || '',
        skipNegotiation: false,
        transport: signalR.HttpTransportType.WebSockets,
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(signalR.LogLevel.Warning)
      .build()

    this.connection.on('EntryEvent', (data) => this.emit('entry', data))
    this.connection.on('ExitEvent', (data) => this.emit('exit', data))
    this.connection.on('PaymentEvent', (data) => this.emit('payment', data))
    this.connection.on('AlertEvent', (data) => this.emit('alert', data))
    this.connection.on('StatsUpdate', (data) => this.emit('stats', data))

    this.connection.onreconnecting(() => this.emit('reconnecting', null))
    this.connection.onreconnected(() => this.emit('reconnected', null))
    this.connection.onclose(() => this.emit('disconnected', null))

    try {
      await this.connection.start()
      this.emit('connected', null)
    } catch (err) {
      console.warn('SignalR connection failed (backend may not be running):', err)
    }
  }

  async disconnect(): Promise<void> {
    if (this.connection) {
      await this.connection.stop()
      this.connection = null
    }
  }

  async joinParking(parkingId: string): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      await this.connection.invoke('JoinParking', parkingId)
    }
  }

  async leaveParking(parkingId: string): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      await this.connection.invoke('LeaveParking', parkingId)
    }
  }

  on(event: string, callback: EventCallback): void {
    if (!this.callbacks.has(event)) this.callbacks.set(event, [])
    this.callbacks.get(event)!.push(callback)
  }

  off(event: string, callback: EventCallback): void {
    const cbs = this.callbacks.get(event)
    if (cbs) {
      this.callbacks.set(event, cbs.filter((cb) => cb !== callback))
    }
  }

  private emit(event: string, data: unknown): void {
    this.callbacks.get(event)?.forEach((cb) => cb(data))
  }

  onEntryEvent(cb: EventCallback) { this.on('entry', cb) }
  onExitEvent(cb: EventCallback) { this.on('exit', cb) }
  onPaymentEvent(cb: EventCallback) { this.on('payment', cb) }
  onAlertEvent(cb: EventCallback) { this.on('alert', cb) }
  onStatsUpdate(cb: EventCallback) { this.on('stats', cb) }

  get state(): signalR.HubConnectionState {
    return this.connection?.state ?? signalR.HubConnectionState.Disconnected
  }
}

export const parkingSignalR = new ParkingSignalRService()
export default parkingSignalR
