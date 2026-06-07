// Базовий URL API Gateway. У розробці задається через VITE_API_URL,
// напр. http://localhost:5000 (порт gateway з Aspire-дашборду).
const BASE = import.meta.env.VITE_API_URL ?? 'http://localhost:5000'

export const TOKEN_KEY = 'gadgetfix_token'

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const token = localStorage.getItem(TOKEN_KEY)
  const res = await fetch(`${BASE}${path}`, {
    ...init,
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...init?.headers,
    },
  })
  if (!res.ok) {
    const body = await res.json().catch(() => null)
    throw new Error(body?.error ?? `Помилка запиту (${res.status})`)
  }
  return res.status === 204 ? (undefined as T) : res.json()
}

// ---- Auth ----
export interface User { id: string; fullName: string; phone: string; email?: string; telegramChatId?: string; role: number; createdAt: string }
export interface AuthResponse { token: string; user: User }
export interface UpdateProfile { fullName: string; email?: string; telegramChatId?: string }
export const UserRole = { Client: 0, Admin: 1 } as const

export const register = (fullName: string, phone: string, password: string, email?: string) =>
  request<AuthResponse>('/api/users/register', {
    method: 'POST',
    body: JSON.stringify({ fullName, phone, email, password }),
  })

export const login = (login: string, password: string) =>
  request<AuthResponse>('/api/users/login', {
    method: 'POST',
    body: JSON.stringify({ login, password }),
  })

export const getMe = () => request<User>('/api/users/me')

export const updateProfile = (data: UpdateProfile) =>
  request<User>('/api/users/me', { method: 'PUT', body: JSON.stringify(data) })

export const generateTelegramCode = () =>
  request<{ code: string }>('/api/users/me/telegram-code', { method: 'POST' })

// ---- Catalog ----
export interface DeviceType { id: number; name: string; slug: string; icon?: string }
export interface RepairService { id: number; deviceTypeId: number; name: string; basePrice: number; estimatedDays: number }

export const getDeviceTypes = () => request<DeviceType[]>('/api/catalog/device-types')
export const getServices = (deviceTypeId?: number) =>
  request<RepairService[]>(`/api/catalog/services${deviceTypeId ? `?deviceTypeId=${deviceTypeId}` : ''}`)

// ---- AI ----
export interface PartOption { tier: string; min: number; max: number; description: string }
export interface EstimateResult {
  min: number
  max: number
  currency: string
  explanation: string
  confidence: number
  options: PartOption[]
}
export interface ChatMsg { role: 'user' | 'assistant'; content: string }
export const chat = (messages: ChatMsg[]) =>
  request<{ reply: string }>('/api/ai/chat', { method: 'POST', body: JSON.stringify({ messages }) })

export interface PhotoResult { deviceType?: string; model?: string; note: string; damage?: string }
export const analyzePhoto = (imageBase64: string, mimeType: string) =>
  request<PhotoResult>('/api/ai/analyze-photo', {
    method: 'POST',
    body: JSON.stringify({ imageBase64, mimeType }),
  })

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
  history?: { status: number; changedAt: string }[]
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

export const getMyOrders = () => request<Order[]>('/api/orders/my')

export const cancelOrder = (id: string) =>
  request<Order>(`/api/orders/${id}/cancel`, { method: 'PATCH' })

// ---- Reviews ----
export interface Review { id: number; authorName: string; rating: number; comment: string; createdAt: string }
export interface ReviewStats { average: number; count: number }
export const getReviews = () => request<Review[]>('/api/reviews')
export const getReviewStats = () => request<ReviewStats>('/api/reviews/stats')
export const createReview = (rating: number, comment: string) =>
  request<Review>('/api/reviews', { method: 'POST', body: JSON.stringify({ rating, comment }) })

export const updateOrderStatus = (id: string, status: number) =>
  request<Order>(`/api/orders/${id}/status`, { method: 'PATCH', body: JSON.stringify({ status }) })

export const updateOrderPrice = (id: string, estimatedPrice: number | null) =>
  request<Order>(`/api/orders/${id}/price`, { method: 'PATCH', body: JSON.stringify({ estimatedPrice }) })
