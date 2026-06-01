// Базовий URL API Gateway. У розробці задається через VITE_API_URL,
// напр. http://localhost:5000 (порт gateway з Aspire-дашборду).
const BASE = import.meta.env.VITE_API_URL ?? 'http://localhost:5000'

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const res = await fetch(`${BASE}${path}`, {
    headers: { 'Content-Type': 'application/json' },
    ...init,
  })
  if (!res.ok) {
    const body = await res.json().catch(() => null)
    throw new Error(body?.error ?? `Помилка запиту (${res.status})`)
  }
  return res.status === 204 ? (undefined as T) : res.json()
}

// ---- Catalog ----
export interface DeviceType { id: number; name: string; slug: string; icon?: string }
export interface RepairService { id: number; deviceTypeId: number; name: string; basePrice: number; estimatedDays: number }

export const getDeviceTypes = () => request<DeviceType[]>('/api/catalog/device-types')
export const getServices = (deviceTypeId?: number) =>
  request<RepairService[]>(`/api/catalog/services${deviceTypeId ? `?deviceTypeId=${deviceTypeId}` : ''}`)

// ---- AI ----
export interface EstimateResult { min: number; max: number; currency: string; explanation: string; confidence: number }
export const estimatePrice = (deviceType: string, model: string, problem: string) =>
  request<EstimateResult>('/api/ai/estimate', {
    method: 'POST',
    body: JSON.stringify({ deviceType, model, problem }),
  })

// ---- Orders ----
export const OrderStatusLabels: Record<number, string> = {
  0: 'Нова заявка',
  1: 'Діагностика',
  2: 'В ремонті',
  3: 'Готово',
  4: 'Видано',
  5: 'Скасовано',
}

export interface Order {
  id: string
  customerName: string
  phone: string
  deviceTypeId: number
  serviceId?: number
  problemDescription: string
  estimatedPrice?: number
  status: number
  createdAt: string
  updatedAt: string
}

export interface CreateOrder {
  customerName: string
  phone: string
  deviceTypeId: number
  serviceId?: number
  problemDescription: string
  estimatedPrice?: number
}

export const createOrder = (order: CreateOrder) =>
  request<Order>('/api/orders', { method: 'POST', body: JSON.stringify(order) })

export const getOrders = () => request<Order[]>('/api/orders')

export const updateOrderStatus = (id: string, status: number) =>
  request<Order>(`/api/orders/${id}/status`, { method: 'PATCH', body: JSON.stringify({ status }) })
