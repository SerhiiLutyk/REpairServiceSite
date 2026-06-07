import { useEffect, useMemo, useState } from 'react'
import { Loader2, RefreshCw, Search } from 'lucide-react'
import { getOrders, updateOrderStatus, updateOrderPrice, OrderStatusLabels, type Order } from '@/lib/api'

const statuses = [0, 1, 2, 3, 4, 5]

export default function Admin() {
  const [orders, setOrders] = useState<Order[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [statusFilter, setStatusFilter] = useState<number | 'all'>('all')
  const [search, setSearch] = useState('')

  async function load() {
    setError(null)
    try {
      setOrders(await getOrders())
    } catch {
      setError('Не вдалося завантажити замовлення. Перевірте, чи запущено бекенд.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    load()
  }, [])

  async function changeStatus(id: string, status: number) {
    const updated = await updateOrderStatus(id, status)
    setOrders((prev) => prev.map((o) => (o.id === id ? updated : o)))
  }

  async function changePrice(id: string, value: string) {
    const price = value.trim() === '' ? null : Number(value)
    if (price !== null && Number.isNaN(price)) return
    const updated = await updateOrderPrice(id, price)
    setOrders((prev) => prev.map((o) => (o.id === id ? updated : o)))
  }

  // Аналітика
  const stats = useMemo(() => {
    const byStatus = statuses.map((s) => orders.filter((o) => o.status === s).length)
    const revenue = orders
      .filter((o) => o.status === 4)
      .reduce((sum, o) => sum + (o.estimatedPrice ?? 0), 0)
    const active = orders.filter((o) => o.status >= 1 && o.status <= 3).length
    return { total: orders.length, active, revenue, byStatus }
  }, [orders])

  const filtered = useMemo(
    () =>
      orders.filter(
        (o) =>
          (statusFilter === 'all' || o.status === statusFilter) &&
          (search === '' ||
            o.phone.includes(search) ||
            o.customerName.toLowerCase().includes(search.toLowerCase())),
      ),
    [orders, statusFilter, search],
  )

  return (
    <section className="mx-auto max-w-5xl px-4 py-12">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-slate-900">Адмін-панель · Замовлення</h1>
        <button onClick={load} className="flex items-center gap-2 rounded-lg border border-slate-300 px-3 py-1.5 text-sm hover:border-brand-500">
          <RefreshCw size={15} /> Оновити
        </button>
      </div>

      {loading && <Loader2 className="mt-8 animate-spin text-brand-600" />}
      {error && <p className="mt-6 rounded-xl bg-red-50 p-4 text-sm text-red-700">{error}</p>}

      {!loading && !error && (
        <>
          {/* Аналітика */}
          <div className="mt-6 grid gap-4 sm:grid-cols-3">
            <div className="card p-5">
              <p className="text-sm text-slate-500">Усього замовлень</p>
              <p className="mt-1 text-3xl font-bold text-slate-900">{stats.total}</p>
            </div>
            <div className="card p-5">
              <p className="text-sm text-slate-500">В роботі</p>
              <p className="mt-1 text-3xl font-bold text-brand-600">{stats.active}</p>
            </div>
            <div className="card p-5">
              <p className="text-sm text-slate-500">Дохід (видані)</p>
              <p className="mt-1 text-3xl font-bold text-green-600">{stats.revenue} грн</p>
            </div>
          </div>

          {/* Розподіл за статусами */}
          <div className="card mt-4 p-5">
            <p className="text-sm font-medium text-slate-700">Розподіл за статусами</p>
            <div className="mt-3 space-y-2">
              {statuses.map((s) => {
                const count = stats.byStatus[s]
                const pct = stats.total ? (count / stats.total) * 100 : 0
                return (
                  <div key={s} className="flex items-center gap-3 text-sm">
                    <span className="w-28 shrink-0 text-slate-500">{OrderStatusLabels[s]}</span>
                    <div className="h-2.5 flex-1 overflow-hidden rounded-full bg-slate-100">
                      <div className="h-full rounded-full bg-brand-500" style={{ width: `${pct}%` }} />
                    </div>
                    <span className="w-6 text-right font-medium text-slate-700">{count}</span>
                  </div>
                )
              })}
            </div>
          </div>

          {/* Фільтри */}
          <div className="mt-6 flex flex-wrap gap-3">
            <div className="relative flex-1">
              <Search size={16} className="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
              <input
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                placeholder="Пошук за ім'ям або телефоном"
                className="w-full rounded-lg border border-slate-300 py-2 pl-9 pr-3 text-sm outline-none focus:border-brand-500"
              />
            </div>
            <select
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value === 'all' ? 'all' : Number(e.target.value))}
              className="rounded-lg border border-slate-300 px-3 py-2 text-sm outline-none focus:border-brand-500"
            >
              <option value="all">Усі статуси</option>
              {statuses.map((s) => (
                <option key={s} value={s}>{OrderStatusLabels[s]}</option>
              ))}
            </select>
          </div>

          <div className="mt-4 overflow-x-auto rounded-2xl border border-slate-200">
            <table className="w-full text-left text-sm">
              <thead className="bg-slate-50 text-slate-500">
                <tr>
                  <th className="px-4 py-3">Клієнт</th>
                  <th className="px-4 py-3">Телефон</th>
                  <th className="px-4 py-3">Несправність</th>
                  <th className="px-4 py-3">Ціна, грн</th>
                  <th className="px-4 py-3">Статус</th>
                </tr>
              </thead>
              <tbody>
                {filtered.length === 0 && (
                  <tr><td colSpan={5} className="px-4 py-6 text-center text-slate-400">Нічого не знайдено</td></tr>
                )}
                {filtered.map((o) => (
                  <tr key={o.id} className="border-t border-slate-100">
                    <td className="px-4 py-3 font-medium text-slate-900">{o.customerName}</td>
                    <td className="px-4 py-3 text-slate-600">{o.phone}</td>
                    <td className="px-4 py-3 text-slate-600">{o.problemDescription}</td>
                    <td className="px-4 py-3">
                      <input
                        type="number"
                        min="0"
                        defaultValue={o.estimatedPrice ?? ''}
                        onBlur={(e) => {
                          const v = e.target.value
                          if (v !== String(o.estimatedPrice ?? '')) changePrice(o.id, v)
                        }}
                        onKeyDown={(e) => { if (e.key === 'Enter') (e.target as HTMLInputElement).blur() }}
                        placeholder="—"
                        className="w-24 rounded-lg border border-slate-300 px-2 py-1 outline-none focus:border-brand-500"
                      />
                    </td>
                    <td className="px-4 py-3">
                      <select
                        value={o.status}
                        onChange={(e) => changeStatus(o.id, Number(e.target.value))}
                        className="rounded-lg border border-slate-300 px-2 py-1 outline-none focus:border-brand-500"
                      >
                        {statuses.map((s) => (
                          <option key={s} value={s}>{OrderStatusLabels[s]}</option>
                        ))}
                      </select>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </>
      )}
      <p className="mt-4 text-xs text-slate-400">
        Зміна статусу на «Готово» надсилає Telegram-повідомлення (якщо налаштовано бота).
      </p>
    </section>
  )
}
