import { Link } from 'react-router-dom'
import { Wrench } from 'lucide-react'

const links = [
  { href: '#services', label: 'Послуги' },
  { href: '#how', label: 'Як це працює' },
  { href: '#price', label: 'AI-калькулятор' },
  { href: '#contact', label: 'Контакти' },
]

export default function Header() {
  return (
    <header className="sticky top-0 z-50 border-b border-slate-200 bg-white/80 backdrop-blur">
      <div className="mx-auto flex max-w-6xl items-center justify-between px-4 py-3">
        <Link to="/" className="flex items-center gap-2 font-bold text-slate-900">
          <span className="flex h-9 w-9 items-center justify-center rounded-xl bg-brand-600 text-white">
            <Wrench size={18} />
          </span>
          GadgetFix
        </Link>

        <nav className="hidden items-center gap-6 md:flex">
          {links.map((l) => (
            <a key={l.href} href={l.href} className="text-sm text-slate-600 transition hover:text-brand-600">
              {l.label}
            </a>
          ))}
        </nav>

        <a
          href="#contact"
          className="rounded-xl bg-brand-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-brand-700"
        >
          Записатись
        </a>
      </div>
    </header>
  )
}
