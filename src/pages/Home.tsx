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
        <div className="absolute inset-0 -z-10 bg-gradient-to-b from-brand-50 to-slate-50" />
        <div className="mx-auto max-w-6xl px-4 py-20 text-center sm:py-28">
          <span className="inline-flex items-center gap-2 rounded-full border border-brand-100 bg-white px-4 py-1.5 text-sm font-medium text-brand-700">
            <Sparkles size={15} /> Ремонт з AI-оцінкою вартості
          </span>
          <h1 className="mx-auto mt-6 max-w-3xl text-4xl font-extrabold tracking-tight text-slate-900 sm:text-6xl">
            Ремонт гаджетів швидко, чесно та з гарантією
          </h1>
          <p className="mx-auto mt-5 max-w-2xl text-lg text-slate-600">
            Полагодимо смартфон, ноутбук чи планшет. Дізнайтесь приблизну ціну за
            секунди з нашим AI-помічником і запишіться онлайн.
          </p>
          <div className="mt-8 flex flex-wrap justify-center gap-3">
            <a href="#contact" className="rounded-xl bg-brand-600 px-6 py-3 font-medium text-white transition hover:bg-brand-700">
              Записатись на ремонт
            </a>
            <a href="#price" className="rounded-xl border border-slate-300 bg-white px-6 py-3 font-medium text-slate-700 transition hover:border-brand-500 hover:text-brand-600">
              Розрахувати ціну з AI
            </a>
          </div>
        </div>
      </section>

      {/* Services */}
      <section id="services" className="mx-auto max-w-6xl px-4 py-16">
        <h2 className="text-center text-3xl font-bold text-slate-900">Що ремонтуємо</h2>
        <p className="mt-2 text-center text-slate-600">Працюємо з усіма популярними брендами та моделями</p>
        <div className="mt-10 grid gap-5 sm:grid-cols-2 lg:grid-cols-4">
          {services.map((s) => (
            <div key={s.title} className="rounded-2xl border border-slate-200 bg-white p-6 transition hover:-translate-y-1 hover:shadow-lg">
              <span className="flex h-12 w-12 items-center justify-center rounded-xl bg-brand-50 text-brand-600">
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
        <div className="rounded-3xl bg-gradient-to-br from-brand-600 to-brand-700 px-6 py-12 text-center text-white sm:px-12">
          <Bot className="mx-auto" size={40} />
          <h2 className="mt-4 text-3xl font-bold">AI-калькулятор вартості ремонту</h2>
          <p className="mx-auto mt-3 max-w-xl text-brand-100">
            Вкажіть тип, модель гаджета та опишіть поломку — штучний інтелект
            миттєво порахує приблизну вартість. Скоро доступно.
          </p>
          <button
            disabled
            className="mt-6 cursor-not-allowed rounded-xl bg-white/90 px-6 py-3 font-medium text-brand-700"
          >
            Скоро 🚀
          </button>
        </div>
      </section>

      {/* Contact / booking placeholder */}
      <section id="contact" className="bg-white py-16">
        <div className="mx-auto max-w-2xl px-4 text-center">
          <h2 className="text-3xl font-bold text-slate-900">Записатись на ремонт</h2>
          <p className="mt-2 text-slate-600">Залиште заявку — ми зв’яжемось з вами найближчим часом.</p>
          <form className="mt-8 grid gap-4 text-left" onSubmit={(e) => e.preventDefault()}>
            <input className="rounded-xl border border-slate-300 px-4 py-3 outline-none focus:border-brand-500" placeholder="Ваше ім’я" />
            <input className="rounded-xl border border-slate-300 px-4 py-3 outline-none focus:border-brand-500" placeholder="Телефон" />
            <input className="rounded-xl border border-slate-300 px-4 py-3 outline-none focus:border-brand-500" placeholder="Що сталося з гаджетом?" />
            <button className="rounded-xl bg-brand-600 px-6 py-3 font-medium text-white transition hover:bg-brand-700">
              Надіслати заявку
            </button>
          </form>
        </div>
      </section>
    </>
  )
}
