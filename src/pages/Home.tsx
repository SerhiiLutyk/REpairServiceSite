import { Link } from 'react-router-dom'
import {
  Smartphone,
  Laptop,
  Tablet,
  Watch,
  Sparkles,
  CalendarCheck,
  Bot,
  ShieldCheck,
} from 'lucide-react'

const services = [
  { icon: Smartphone, title: 'Смартфони', desc: 'Заміна екранів, акумуляторів, роз’ємів, ремонт після води.' },
  { icon: Laptop, title: 'Ноутбуки', desc: 'Чистка, заміна термопасти, апгрейд SSD/RAM, ремонт плат.' },
  { icon: Tablet, title: 'Планшети', desc: 'Тачскрін, дисплеї, зарядка, відновлення ПЗ.' },
  { icon: Watch, title: 'Гаджети', desc: 'Смарт-годинники, навушники, консолі та інша електроніка.' },
]

const steps = [
  { icon: CalendarCheck, title: 'Запис онлайн', desc: 'Обираєте час та залишаєте заявку за хвилину.' },
  { icon: Bot, title: 'AI-оцінка', desc: 'Помічник одразу підкаже приблизну вартість ремонту.' },
  { icon: Sparkles, title: 'Діагностика', desc: 'Майстер уточнює несправність і фінальну ціну.' },
  { icon: ShieldCheck, title: 'Ремонт + гарантія', desc: 'Повертаємо гаджет як новий. Гарантія до 6 міс.' },
]

export default function Home() {
  return (
    <>
      {/* Hero */}
      <section className="relative overflow-hidden">
        <div className="absolute inset-0 -z-10 bg-gradient-to-b from-brand-50 via-white to-slate-50" />
        <div className="blob -left-20 -top-20 h-72 w-72 bg-brand-300" />
        <div className="blob -right-16 top-10 h-80 w-80 bg-violet-300" />
        <div className="mx-auto max-w-6xl px-4 py-24 text-center sm:py-32">
          <span className="inline-flex items-center gap-2 rounded-full border border-brand-100 bg-white/80 px-4 py-1.5 text-sm font-medium text-brand-700 shadow-sm backdrop-blur">
            <Sparkles size={15} /> Ремонт з AI-оцінкою вартості
          </span>
          <h1 className="mx-auto mt-6 max-w-3xl text-5xl font-extrabold tracking-tight text-slate-900 sm:text-6xl">
            Ремонт гаджетів <span className="text-gradient">швидко й чесно</span>
          </h1>
          <p className="mx-auto mt-6 max-w-2xl text-lg text-slate-600">
            Полагодимо смартфон, ноутбук чи планшет. Дізнайтесь приблизну ціну за
            секунди з нашим AI-помічником і запишіться онлайн.
          </p>
          <div className="mt-9 flex flex-wrap justify-center gap-3">
            <Link to="/booking" className="rounded-xl bg-brand-600 px-7 py-3.5 font-medium text-white shadow-lg shadow-brand-600/25 transition hover:bg-brand-700 hover:shadow-brand-600/40">
              Записатись на ремонт
            </Link>
            <Link to="/calculator" className="rounded-xl border border-slate-300 bg-white px-7 py-3.5 font-medium text-slate-700 transition hover:border-brand-500 hover:text-brand-600">
              Розрахувати ціну з AI
            </Link>
          </div>

          {/* Статистика */}
          <div className="mx-auto mt-14 grid max-w-2xl grid-cols-3 gap-4">
            {[
              { v: '10 000+', l: 'ремонтів' },
              { v: '24 год', l: 'середній термін' },
              { v: 'до 6 міс', l: 'гарантія' },
            ].map((s) => (
              <div key={s.l}>
                <p className="text-2xl font-extrabold text-slate-900 sm:text-3xl">{s.v}</p>
                <p className="mt-1 text-sm text-slate-500">{s.l}</p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Services */}
      <section id="services" className="mx-auto max-w-6xl px-4 py-16">
        <h2 className="text-center text-3xl font-bold text-slate-900">Що ремонтуємо</h2>
        <p className="mt-2 text-center text-slate-600">Працюємо з усіма популярними брендами та моделями</p>
        <div className="mt-10 grid gap-5 sm:grid-cols-2 lg:grid-cols-4">
          {services.map((s) => (
            <div key={s.title} className="card p-6">
              <span className="flex h-12 w-12 items-center justify-center rounded-xl bg-gradient-to-br from-brand-500 to-brand-700 text-white shadow-md shadow-brand-600/20">
                <s.icon size={24} />
              </span>
              <h3 className="mt-4 text-lg font-semibold text-slate-900">{s.title}</h3>
              <p className="mt-1 text-sm text-slate-600">{s.desc}</p>
            </div>
          ))}
        </div>
      </section>

      {/* How it works */}
      <section id="how" className="bg-white py-16">
        <div className="mx-auto max-w-6xl px-4">
          <h2 className="text-center text-3xl font-bold text-slate-900">Як це працює</h2>
          <div className="mt-10 grid gap-5 sm:grid-cols-2 lg:grid-cols-4">
            {steps.map((s, i) => (
              <div key={s.title} className="relative rounded-2xl bg-slate-50 p-6">
                <span className="absolute right-5 top-4 text-4xl font-bold text-slate-200">{i + 1}</span>
                <span className="flex h-11 w-11 items-center justify-center rounded-xl bg-brand-600 text-white">
                  <s.icon size={22} />
                </span>
                <h3 className="mt-4 font-semibold text-slate-900">{s.title}</h3>
                <p className="mt-1 text-sm text-slate-600">{s.desc}</p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* AI price teaser */}
      <section id="price" className="mx-auto max-w-6xl px-4 py-16">
        <div className="relative overflow-hidden rounded-3xl bg-gradient-to-br from-brand-600 via-brand-700 to-violet-700 px-6 py-14 text-center text-white sm:px-12">
          <div className="blob -right-10 -top-10 h-56 w-56 bg-white/30" />
          <div className="blob -bottom-16 left-0 h-56 w-56 bg-violet-400/40" />
          <span className="mx-auto flex h-16 w-16 items-center justify-center rounded-2xl bg-white/15 backdrop-blur">
            <Bot size={34} />
          </span>
          <h2 className="mt-5 text-3xl font-bold">AI-калькулятор вартості ремонту</h2>
          <p className="mx-auto mt-3 max-w-xl text-brand-100">
            Вкажіть тип, модель гаджета та опишіть поломку — штучний інтелект
            запропонує 3 варіанти запчастин і миттєво порахує вартість.
          </p>
          <Link
            to="/calculator"
            className="mt-7 inline-block rounded-xl bg-white px-7 py-3.5 font-medium text-brand-700 shadow-lg transition hover:bg-brand-50"
          >
            Розрахувати вартість
          </Link>
        </div>
      </section>

      {/* Contact / booking CTA */}
      <section id="contact" className="bg-white py-16">
        <div className="mx-auto max-w-2xl px-4 text-center">
          <h2 className="text-3xl font-bold text-slate-900">Готові полагодити свій гаджет?</h2>
          <p className="mt-2 text-slate-600">Залиште заявку — ми зв’яжемось з вами найближчим часом.</p>
          <Link
            to="/booking"
            className="mt-6 inline-block rounded-xl bg-brand-600 px-6 py-3 font-medium text-white transition hover:bg-brand-700"
          >
            Записатись на ремонт
          </Link>
        </div>
      </section>
    </>
  )
}
