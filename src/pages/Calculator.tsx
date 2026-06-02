import { useState } from 'react'
import { Bot, Loader2, Camera } from 'lucide-react'
import { estimatePrice, analyzePhoto, type EstimateResult } from '@/lib/api'

const deviceTypes = ['Смартфон', 'Ноутбук', 'Планшет', 'Смарт-годинник']

export default function Calculator() {
  const [deviceType, setDeviceType] = useState(deviceTypes[0])
  const [model, setModel] = useState('')
  const [problem, setProblem] = useState('')
  const [result, setResult] = useState<EstimateResult | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [photoLoading, setPhotoLoading] = useState(false)
  const [photoNote, setPhotoNote] = useState<string | null>(null)

  async function onPhoto(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0]
    if (!file) return
    setPhotoNote(null)
    setPhotoLoading(true)
    try {
      const dataUrl = await new Promise<string>((res, rej) => {
        const r = new FileReader()
        r.onload = () => res(r.result as string)
        r.onerror = rej
        r.readAsDataURL(file)
      })
      const base64 = dataUrl.split(',')[1]
      const r = await analyzePhoto(base64, file.type)
      if (r.deviceType && deviceTypes.includes(r.deviceType)) setDeviceType(r.deviceType)
      if (r.model) setModel(r.model)
      setPhotoNote(r.note || 'Готово')
    } catch {
      setPhotoNote('Не вдалося розпізнати фото')
    } finally {
      setPhotoLoading(false)
    }
  }

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault()
    setError(null)
    setResult(null)
    setLoading(true)
    try {
      setResult(await estimatePrice(deviceType, model, problem))
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Сталася помилка')
    } finally {
      setLoading(false)
    }
  }

  return (
    <section className="mx-auto max-w-2xl px-4 py-16">
      <div className="text-center">
        <span className="mx-auto flex h-14 w-14 items-center justify-center rounded-2xl bg-brand-600 text-white">
          <Bot size={28} />
        </span>
        <h1 className="mt-4 text-3xl font-bold text-slate-900">AI-калькулятор вартості ремонту</h1>
        <p className="mt-2 text-slate-600">
          Вкажіть тип гаджета та опишіть поломку — отримаєте приблизну вартість.
        </p>
      </div>

      <form onSubmit={onSubmit} className="mt-8 grid gap-4">
        <label className="flex cursor-pointer items-center justify-center gap-2 rounded-xl border border-dashed border-brand-300 bg-brand-50 px-4 py-3 text-sm font-medium text-brand-700 transition hover:bg-brand-100">
          {photoLoading ? <Loader2 className="animate-spin" size={16} /> : <Camera size={16} />}
          Розпізнати гаджет за фото
          <input type="file" accept="image/*" className="hidden" onChange={onPhoto} disabled={photoLoading} />
        </label>
        {photoNote && <p className="text-xs text-slate-500">{photoNote}</p>}

        <label className="grid gap-1 text-sm font-medium text-slate-700">
          Тип гаджета
          <select
            value={deviceType}
            onChange={(e) => setDeviceType(e.target.value)}
            className="rounded-xl border border-slate-300 px-4 py-3 outline-none focus:border-brand-500"
          >
            {deviceTypes.map((d) => (
              <option key={d}>{d}</option>
            ))}
          </select>
        </label>

        <input
          value={model}
          onChange={(e) => setModel(e.target.value)}
          className="rounded-xl border border-slate-300 px-4 py-3 outline-none focus:border-brand-500"
          placeholder="Модель (напр. iPhone 12, опційно)"
        />

        <textarea
          value={problem}
          onChange={(e) => setProblem(e.target.value)}
          required
          rows={3}
          className="rounded-xl border border-slate-300 px-4 py-3 outline-none focus:border-brand-500"
          placeholder="Що сталося? Напр. розбитий екран, не тримає зарядку…"
        />

        <button
          disabled={loading}
          className="flex items-center justify-center gap-2 rounded-xl bg-brand-600 px-6 py-3 font-medium text-white transition hover:bg-brand-700 disabled:opacity-60"
        >
          {loading && <Loader2 className="animate-spin" size={18} />}
          Розрахувати вартість
        </button>
      </form>

      {error && <p className="mt-4 rounded-xl bg-red-50 p-4 text-sm text-red-700">{error}</p>}

      {result && (
        <div className="mt-6">
          <div className="rounded-2xl border border-brand-100 bg-brand-50 p-5">
            <p className="text-sm text-brand-700">Орієнтовна вартість</p>
            <p className="mt-1 text-3xl font-bold text-slate-900">
              {result.min}–{result.max} {result.currency}
            </p>
            {result.explanation && <p className="mt-2 text-sm text-slate-600">{result.explanation}</p>}
          </div>

          {result.options?.length > 0 && (
            <div className="mt-4 grid gap-3 sm:grid-cols-3">
              {result.options.map((o) => (
                <div key={o.tier} className="rounded-2xl border border-slate-200 bg-white p-4">
                  <p className="font-semibold text-slate-900">{o.tier}</p>
                  <p className="mt-1 text-lg font-bold text-brand-600">
                    {o.min}–{o.max} {result.currency}
                  </p>
                  <p className="mt-2 text-xs leading-relaxed text-slate-500">{o.description}</p>
                </div>
              ))}
            </div>
          )}

          <p className="mt-3 text-xs text-slate-400">
            Впевненість оцінки: {Math.round(result.confidence * 100)}%
          </p>
        </div>
      )}
    </section>
  )
}
