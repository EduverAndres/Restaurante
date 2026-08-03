import { Injectable, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../environments/environment';
import { Order, normalizeOrder } from './order.service';

export interface RiderLocation {
  orderId: string;
  latitude: number;
  longitude: number;
}

@Injectable({ providedIn: 'root' })
export class SignalrService {
  private hubConnection!: signalR.HubConnection;
  isConnected = signal(false);

  constructor() {
    this.buildConnection();
  }

  private buildConnection(): void {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(environment.signalrUrl, {
        accessTokenFactory: () => localStorage.getItem('restaurante_token') || '',
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    this.hubConnection.onreconnecting(() => this.isConnected.set(false));
    this.hubConnection.onreconnected(() => this.isConnected.set(true));
    this.hubConnection.onclose(() => this.isConnected.set(false));
  }

  private connectionAttempt: Promise<void> | null = null;

  async start(): Promise<void> {
    const state = this.hubConnection.state;
    if (state === signalR.HubConnectionState.Connected) return;
    if (state === signalR.HubConnectionState.Connecting && this.connectionAttempt) {
      return this.connectionAttempt;
    }
    this.connectionAttempt = this.connect();
    return this.connectionAttempt;
  }

  private async connect(): Promise<void> {
    try {
      await this.hubConnection.start();
      this.isConnected.set(true);
    } catch (err) {
      console.error('SignalR connection error:', err);
      setTimeout(() => this.start(), 5000);
    }
  }

  async stop(): Promise<void> {
    await this.hubConnection.stop();
    this.isConnected.set(false);
  }

  onOrderUpdated(callback: (order: Order) => void): void {
    this.hubConnection.off('OrderUpdated');
    this.hubConnection.on('OrderUpdated', (raw: any) => callback(normalizeOrder(raw)));
  }

  onNewOrder(callback: (order: Order) => void): void {
    this.hubConnection.off('NewOrder');
    this.hubConnection.on('NewOrder', (raw: any) => callback(normalizeOrder(raw)));
  }

  onRiderLocationUpdated(callback: (location: RiderLocation) => void): void {
    this.hubConnection.off('RiderLocationUpdated');
    this.hubConnection.on('RiderLocationUpdated', callback);
  }

  async joinRestaurantGroup(restaurantId: string): Promise<void> {
    if (this.hubConnection.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('JoinRestaurantGroup', restaurantId);
    }
  }

  async leaveRestaurantGroup(restaurantId: string): Promise<void> {
    if (this.hubConnection.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('LeaveRestaurantGroup', restaurantId);
    }
  }

  async joinOrderGroup(orderId: string): Promise<void> {
    if (this.hubConnection.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('JoinOrderGroup', orderId);
    }
  }

  async leaveOrderGroup(orderId: string): Promise<void> {
    if (this.hubConnection.state === signalR.HubConnectionState.Connected) {
      await this.hubConnection.invoke('LeaveOrderGroup', orderId);
    }
  }
}
