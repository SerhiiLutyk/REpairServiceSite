import { useEffect, useState } from 'react'
import { Star } from 'lucide-react'
import { getReviews, getReviewStats, type Review, type ReviewStats } from '@/lib/api'

function Stars({ value }: { value: number }) {
  return (
    <span className="inline-flex">
      {[1, 2, 3, 4, 5].map((i) => (
        <Star key={i} size={16} className={i <= value ? 'fill-amber-400 text-amber-400' : 'text-slate-300'} />
      ))}
    </span>
  )
}

export default function ReviewsSection() {
  const [reviews, setReviews] = useState<Review[]>([])
  const [stats, setStats] = useState<ReviewStats | null>(null)

  useEffect(() => {
    getReviews().then(setReviews).catch(() => {})
    getReviewStats().then(setStats).catch(() => {})
  }, [])

  if (reviews.length === 0) return null

  return (
    <section className="mx-auto max-w-6xl px-4 py-16">
      <h2 className="text-center text-3xl font-bold text-slate-900">Відгуки клієнтів</h2>
      {stats && stats.count > 0 && (
        <p className="mt-2 flex items-center justify-center gap-2 text-slate-600">
          <Stars value={Math.round(stats.average)} />
          <span className="font-semibold text-slate-900">{stats.average}</span>
          <span className="text-sm">· {stats.count} відгуків</span>
        </p>
      )}
      <div className="mt-10 grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
        {reviews.slice(0, 6).map((r) => (
          <div key={r.id} className="card p-6">
            <Stars value={r.rating} />
            <p className="mt-3 text-sm text-slate-600">{r.comment}</p>
            <p className="mt-3 text-sm font-semibold text-slate-900">{r.authorName}</p>
          </div>
        ))}
      </div>
    </section>
  )
}
