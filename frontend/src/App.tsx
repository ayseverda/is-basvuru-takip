import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import './App.css'

// ---------- Tipler ----------

// Backend'deki BasvuruDto'nun ön yüzdeki karşılığı
type Basvuru = {
  id: number
  sirketAdi: string
  pozisyon: string
  basvuruTarihi: string
  maas: number | null
  durum: number
  sekil: number
  kaynak: number
  ilanLinki: string | null
  notlar: string | null
  sehir: number | null
}

// Backend'deki SayimDto ve OzetDto'nun karşılıkları
type Sayim = { deger: number; sayi: number }
type Ozet = { toplam: number; durumaGore: Sayim[]; kaynagaGore: Sayim[] }

// Backend'deki IceAktarSonucDto'nun karşılığı
type IceAktarSonuc = { eklenen: number; atlanan: number; hatali: number; hatalar: string[] }

// Backend'deki BasvuruIstekDto'nun karşılığı (Id yok)
type BasvuruIstek = Omit<Basvuru, 'id'>

// ---------- Enum etiketleri ----------
// Sıra, backend'deki enum sırasıyla birebir aynı olmalı (0, 1, 2...)

const DURUMLAR = [
  'Başvuruldu',
  'Reddedildi',
  'Mülakat',
  'Test aşamasında',
  'Teklif alındı',
  'Teklif kabul edildi',
  'Cevap yok',
  'Teklif reddedildi',
]
const SEKILLER = ['Ofisten', 'Uzaktan', 'Hibrit', 'Belirtilmemiş']
const KAYNAKLAR = ['Kariyer.net', 'LinkedIn', 'Indeed', 'Şirket sitesi', 'Mail', 'Diğer']
const SEHIRLER = ['Ankara', 'İstanbul', 'Sakarya', 'Bursa', 'Kocaeli', 'Diğer']

// Durumlara göre rozet rengi (CSS sınıf adı)
const DURUM_RENGI = ['mavi', 'kirmizi', 'yesil', 'mor', 'altin', 'yesil', 'gri', 'gri']

// Bugünün tarihi "YYYY-MM-DD" biçiminde (form varsayılanı için)
function bugun(): string {
  const d = new Date()
  const ay = String(d.getMonth() + 1).padStart(2, '0')
  const gun = String(d.getDate()).padStart(2, '0')
  return `${d.getFullYear()}-${ay}-${gun}`
}

function bosForm(): BasvuruIstek {
  return {
    sirketAdi: '',
    pozisyon: '',
    basvuruTarihi: bugun(),
    maas: null,
    durum: 0,
    sekil: 0,
    kaynak: 1,
    ilanLinki: null,
    notlar: null,
    sehir: 0,
  }
}

// Backend'in 400 cevabındaki doğrulama hatalarını tek listeye çevirir
async function hataMesajlari(cevap: Response): Promise<string[]> {
  try {
    const veri = await cevap.json()
    if (veri.errors) {
      return Object.values(veri.errors as Record<string, string[]>).flat()
    }
  } catch {
    // Gövde JSON değilse aşağıdaki genel mesaj kullanılır
  }
  return [`Bir hata oluştu (kod ${cevap.status}).`]
}

// ---------- Ana bileşen ----------

function App() {
  // null: henüz kontrol edilmedi, false: giriş yok, true: giriş var
  const [girisVar, setGirisVar] = useState<boolean | null>(null)
  const [basvurular, setBasvurular] = useState<Basvuru[]>([])

  // Backend'den başvuruları çeker; 401 gelirse giriş ekranına döner
  async function basvurulariYukle() {
    const cevap = await fetch('/api/Basvurular')
    if (cevap.status === 401) {
      setGirisVar(false)
      return
    }
    setBasvurular(await cevap.json())
    setGirisVar(true)
  }

  // Sayfa ilk açıldığında bir kez çalışır
  useEffect(() => {
    basvurulariYukle()
  }, [])

  if (girisVar === null) {
    return <div className="ortala">Yükleniyor…</div>
  }

  if (!girisVar) {
    return <GirisEkrani girisYapildi={basvurulariYukle} />
  }

  return <Panel basvurular={basvurular} yenile={basvurulariYukle} />
}

// ---------- Giriş / kayıt ekranı ----------

function GirisEkrani({ girisYapildi }: { girisYapildi: () => void }) {
  const [email, setEmail] = useState('')
  const [sifre, setSifre] = useState('')
  const [kayitModu, setKayitModu] = useState(false)
  const [hatalar, setHatalar] = useState<string[]>([])
  const [bilgi, setBilgi] = useState('')

  async function gonder(e: FormEvent) {
    e.preventDefault()
    setHatalar([])
    setBilgi('')

    const govde = JSON.stringify({ email, password: sifre })
    const basliklar = { 'Content-Type': 'application/json' }

    if (kayitModu) {
      const cevap = await fetch('/register', { method: 'POST', headers: basliklar, body: govde })
      if (!cevap.ok) {
        setHatalar(await hataMesajlari(cevap))
        return
      }
      setBilgi('Kayıt başarılı, şimdi giriş yapabilirsin.')
      setKayitModu(false)
      return
    }

    // useCookies=true: giriş bilgisi tarayıcıda çerez olarak saklanır
    const cevap = await fetch('/login?useCookies=true', { method: 'POST', headers: basliklar, body: govde })
    if (!cevap.ok) {
      setHatalar(['E-posta ya da şifre hatalı.'])
      return
    }
    girisYapildi()
  }

  return (
    <div className="ortala">
      <form className="kart giris" onSubmit={gonder}>
        <h1>Başvuru Takip</h1>
        <p className="soluk">{kayitModu ? 'Yeni hesap oluştur' : 'Hesabına giriş yap'}</p>

        <label>
          E-posta
          <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
        </label>
        <label>
          Şifre
          <input type="password" value={sifre} onChange={(e) => setSifre(e.target.value)} required />
        </label>

        {hatalar.length > 0 && (
          <ul className="hatalar">
            {hatalar.map((h) => <li key={h}>{h}</li>)}
          </ul>
        )}
        {bilgi && <p className="bilgi">{bilgi}</p>}

        <button type="submit" className="ana">{kayitModu ? 'Kayıt ol' : 'Giriş yap'}</button>
        <button type="button" className="metin" onClick={() => setKayitModu(!kayitModu)}>
          {kayitModu ? 'Hesabın var mı? Giriş yap' : 'Hesabın yok mu? Kayıt ol'}
        </button>
      </form>
    </div>
  )
}

// ---------- Başvuru tablosu ----------

// Tablodaki sıralanabilir sütunlar
type Sutun = 'basvuruTarihi' | 'sirketAdi' | 'pozisyon' | 'durum' | 'kaynak' | 'sekil' | 'sehir' | 'maas'

const SUTUNLAR: { ad: Sutun; baslik: string }[] = [
  { ad: 'basvuruTarihi', baslik: 'Tarih' },
  { ad: 'sirketAdi', baslik: 'Şirket' },
  { ad: 'pozisyon', baslik: 'Pozisyon' },
  { ad: 'durum', baslik: 'Durum' },
  { ad: 'kaynak', baslik: 'Kaynak' },
  { ad: 'sekil', baslik: 'Şekil' },
  { ad: 'sehir', baslik: 'Şehir' },
  { ad: 'maas', baslik: 'Maaş' },
]

function Panel({ basvurular, yenile }: { basvurular: Basvuru[]; yenile: () => void }) {
  const [formAcik, setFormAcik] = useState(false)
  // null ise yeni başvuru, dolu ise o başvuru düzenleniyor
  const [duzenlenen, setDuzenlenen] = useState<Basvuru | null>(null)

  function yeniAc() {
    setDuzenlenen(null)
    setFormAcik(true)
  }

  function duzenleAc(b: Basvuru) {
    setDuzenlenen(b)
    setFormAcik(true)
  }
  const [filtre, setFiltre] = useState<number | 'hepsi'>('hepsi')
  const [arama, setArama] = useState('')
  const [ozet, setOzet] = useState<Ozet | null>(null)
  const [iceAktariliyor, setIceAktariliyor] = useState(false)
  const [iceAktarSonuc, setIceAktarSonuc] = useState<IceAktarSonuc | null>(null)
  const [iceAktarHata, setIceAktarHata] = useState('')

  // Seçilen CSV dosyalarını sırayla backend'e yükler ve sonuçları toplar
  async function linkedInIceAktar(dosyalar: FileList | null) {
    if (!dosyalar || dosyalar.length === 0) return
    setIceAktariliyor(true)
    setIceAktarSonuc(null)
    setIceAktarHata('')

    const toplam: IceAktarSonuc = { eklenen: 0, atlanan: 0, hatali: 0, hatalar: [] }
    for (const dosya of Array.from(dosyalar)) {
      const form = new FormData()
      form.append('dosya', dosya) // Backend'deki parametre adıyla aynı olmalı
      const cevap = await fetch('/api/Basvurular/linkedin-ice-aktar', { method: 'POST', body: form })
      if (!cevap.ok) {
        const mesaj = (await cevap.text()).replace(/^"|"$/g, '')
        setIceAktarHata(`${dosya.name}: ${mesaj || `yüklenemedi (kod ${cevap.status})`}`)
        continue
      }
      const sonuc: IceAktarSonuc = await cevap.json()
      toplam.eklenen += sonuc.eklenen
      toplam.atlanan += sonuc.atlanan
      toplam.hatali += sonuc.hatali
      toplam.hatalar.push(...sonuc.hatalar.map((h) => `${dosya.name} · ${h}`))
    }

    setIceAktarSonuc(toplam)
    setIceAktariliyor(false)
    yenile()
  }

  // Başvurular her değiştiğinde (ekleme, silme, durum değişikliği) özeti backend'den yeniden çek
  useEffect(() => {
    fetch('/api/Basvurular/ozet')
      .then((cevap) => (cevap.ok ? cevap.json() : null))
      .then((veri) => setOzet(veri))
      .catch(() => setOzet(null))
  }, [basvurular])
  const [siralama, setSiralama] = useState<{ sutun: Sutun; artan: boolean }>({ sutun: 'basvuruTarihi', artan: false })

  // Önce durum filtresi ve arama, sonra sıralama
  const aranan = arama.trim().toLocaleLowerCase('tr-TR')
  const gosterilen = basvurular
    .filter((b) => filtre === 'hepsi' || b.durum === filtre)
    .filter((b) =>
      aranan === '' ||
      b.sirketAdi.toLocaleLowerCase('tr-TR').includes(aranan) ||
      b.pozisyon.toLocaleLowerCase('tr-TR').includes(aranan) ||
      (b.notlar ?? '').toLocaleLowerCase('tr-TR').includes(aranan))
    .sort((a, b) => {
      const x = a[siralama.sutun]
      const y = b[siralama.sutun]
      // Boş değerler her zaman en alta
      if (x === null) return 1
      if (y === null) return -1
      const fark = typeof x === 'string' ? x.localeCompare(y as string, 'tr') : x - (y as number)
      return siralama.artan ? fark : -fark
    })

  // Başlığa tıklanınca o sütuna göre sırala; aynı başlığa tekrar tıklanınca yön değişir
  function sirala(sutun: Sutun) {
    setSiralama(siralama.sutun === sutun ? { sutun, artan: !siralama.artan } : { sutun, artan: true })
  }

  // Özet sayıları
  const mulakatSayisi = basvurular.filter((b) => b.durum === 2 || b.durum === 3).length
  const bekleyenSayisi = basvurular.filter((b) => b.durum === 0).length
  const redSayisi = basvurular.filter((b) => b.durum === 1).length

  // Hücredeki açılır listeden durum değiştirildiğinde PUT gönderir
  async function durumDegistir(b: Basvuru, yeniDurum: number) {
    const { id, ...geriKalan } = b
    await fetch(`/api/Basvurular/${id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ ...geriKalan, durum: yeniDurum }),
    })
    yenile()
  }

  async function sil(b: Basvuru) {
    if (!confirm(`${b.sirketAdi} başvurusu silinsin mi?`)) return
    await fetch(`/api/Basvurular/${b.id}`, { method: 'DELETE' })
    yenile()
  }

  return (
    <div className="sayfa">
      <header className="ust">
        <div>
          <h1>Başvurularım</h1>
          <p className="soluk kucuk ozet-satir">
            {basvurular.length} toplam · {bekleyenSayisi} cevap bekliyor · {mulakatSayisi} mülakat/test · {redSayisi} ret
          </p>
        </div>
        <div className="ust-butonlar">
          {/* Gerçek dosya seçici gizli; görünen düğme onu tetikliyor */}
          <label className={`ikincil${iceAktariliyor ? ' pasif' : ''}`}>
            {iceAktariliyor ? 'İçe aktarılıyor…' : "LinkedIn'den içe aktar"}
            <input
              type="file"
              accept=".csv"
              multiple
              hidden
              disabled={iceAktariliyor}
              onChange={(e) => {
                linkedInIceAktar(e.target.files)
                e.target.value = '' // Aynı dosya tekrar seçilebilsin
              }}
            />
          </label>
          <button className="ana" onClick={yeniAc}>+ Yeni başvuru</button>
        </div>
      </header>

      {(iceAktarSonuc || iceAktarHata) && (
        <div className="ice-aktar-sonuc" role="status">
          <div className="ice-aktar-ust">
            {iceAktarSonuc && (
              <span>
                <strong>{iceAktarSonuc.eklenen}</strong> başvuru eklendi ·{' '}
                <strong>{iceAktarSonuc.atlanan}</strong> tanesi zaten vardı ·{' '}
                <strong>{iceAktarSonuc.hatali}</strong> satır okunamadı
              </span>
            )}
            {iceAktarHata && <span className="tehlike-yazi">{iceAktarHata}</span>}
            <button
              type="button"
              className="metin"
              onClick={() => { setIceAktarSonuc(null); setIceAktarHata('') }}
            >
              Kapat
            </button>
          </div>
          {iceAktarSonuc && iceAktarSonuc.hatalar.length > 0 && (
            <details>
              <summary>Okunamayan satırlar</summary>
              <ul>
                {iceAktarSonuc.hatalar.map((h) => <li key={h}>{h}</li>)}
              </ul>
            </details>
          )}
        </div>
      )}

      {ozet && ozet.toplam > 0 && (
        <section className="grafikler">
          <PastaGrafik
            baslik="Durumlara göre"
            satirlar={ozet.durumaGore}
            etiketler={DURUMLAR}
            renkler={DURUM_PASTA_RENK}
          />
          <PastaGrafik
            baslik="Kaynaklara göre"
            satirlar={ozet.kaynagaGore}
            etiketler={KAYNAKLAR}
            renkler={KAYNAK_PASTA_RENK}
          />
        </section>
      )}

      <div className="arac-cubugu">
        <input
          type="search"
          placeholder="Şirket, pozisyon veya notta ara…"
          value={arama}
          onChange={(e) => setArama(e.target.value)}
        />
        <select value={filtre} onChange={(e) => setFiltre(e.target.value === 'hepsi' ? 'hepsi' : Number(e.target.value))}>
          <option value="hepsi">Tüm durumlar</option>
          {DURUMLAR.map((d, i) => <option key={d} value={i}>{d}</option>)}
        </select>
        <span className="soluk kucuk">{gosterilen.length} satır</span>
      </div>

      <div className="tablo-kutu">
        <table className="tablo">
          {/* Sabit genişlikler: filtreleyince sütunlar yer değiştirmesin */}
          <colgroup>
            <col style={{ width: 44 }} />
            <col style={{ width: 104 }} />
            <col style={{ width: '15%' }} />
            <col style={{ width: '17%' }} />
            <col style={{ width: 176 }} />
            <col style={{ width: 112 }} />
            <col style={{ width: 84 }} />
            <col style={{ width: 96 }} />
            <col style={{ width: 104 }} />
            <col />
            <col style={{ width: 60 }} />
            <col style={{ width: 40 }} />
            <col style={{ width: 40 }} />
          </colgroup>
          <thead>
            <tr>
              <th className="no">#</th>
              {SUTUNLAR.map((s) => (
                <th key={s.ad} onClick={() => sirala(s.ad)} className="siralanir">
                  {s.baslik}
                  <span className="ok">{siralama.sutun === s.ad ? (siralama.artan ? '▲' : '▼') : ''}</span>
                </th>
              ))}
              <th>Not</th>
              <th>İlan</th>
              <th></th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {gosterilen.length === 0 && (
              <tr><td colSpan={13} className="bos">Gösterilecek başvuru yok.</td></tr>
            )}
            {gosterilen.map((b, sira) => (
              <tr key={b.id} onDoubleClick={() => duzenleAc(b)} title="Düzenlemek için çift tıkla">
                <td className="no">{sira + 1}</td>
                <td className="tarih">{b.basvuruTarihi}</td>
                <td className="kalin sarar">{b.sirketAdi}</td>
                <td className="sarar">{b.pozisyon}</td>
                <td>
                  <select
                    className={`rozet ${DURUM_RENGI[b.durum]}`}
                    value={b.durum}
                    onChange={(e) => durumDegistir(b, Number(e.target.value))}
                  >
                    {DURUMLAR.map((d, i) => <option key={d} value={i}>{d}</option>)}
                  </select>
                </td>
                <td>{KAYNAKLAR[b.kaynak]}</td>
                <td>{SEKILLER[b.sekil]}</td>
                <td>{b.sehir !== null ? SEHIRLER[b.sehir] : <span className="soluk">–</span>}</td>
                <td className="sayi-hucre">{b.maas !== null ? `${b.maas.toLocaleString('tr-TR')} ₺` : <span className="soluk">–</span>}</td>
                <td className="not-hucre" title={b.notlar ?? ''}><span>{b.notlar ?? ''}</span></td>
                <td>{b.ilanLinki && <a href={b.ilanLinki} target="_blank" rel="noreferrer">Aç ↗</a>}</td>
                <td><button className="metin" onClick={() => duzenleAc(b)} title="Düzenle">✎</button></td>
                <td><button className="metin tehlike" onClick={() => sil(b)} title="Sil">✕</button></td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {formAcik && <BasvuruFormu duzenlenen={duzenlenen} kapat={() => setFormAcik(false)} kaydedildi={yenile} />}
    </div>
  )
}

// ---------- Özet grafiği (pasta) ----------

// Her durum ve kaynak için sabit bir renk (enum sırasıyla aynı)
const DURUM_PASTA_RENK = ['#6d8cff', '#ff7b7b', '#5fcf86', '#b292ff', '#f0c04f', '#2fbfa0', '#7b8494', '#c98a5e']
const KAYNAK_PASTA_RENK = ['#6d8cff', '#4fb3e8', '#f0a637', '#5fcf86', '#e07bd0', '#7b8494']

type PastaProps = {
  baslik: string
  satirlar: Sayim[]
  etiketler: string[]
  renkler: string[]
}

function PastaGrafik({ baslik, satirlar, etiketler, renkler }: PastaProps) {
  const toplam = satirlar.reduce((t, s) => t + s.sayi, 0)
  const sirali = [...satirlar].sort((a, b) => b.sayi - a.sayi)

  // Her dilimin başladığı ve bittiği yüzdeyi hesaplayıp tek bir conic-gradient oluştur
  let baslangic = 0
  const dilimler = sirali.map((s) => {
    const bitis = baslangic + (s.sayi / toplam) * 100
    const parca = `${renkler[s.deger] ?? '#7b8494'} ${baslangic}% ${bitis}%`
    baslangic = bitis
    return parca
  })

  return (
    <div className="grafik">
      <h2>{baslik}</h2>
      <div className="pasta-govde">
        <div
          className="pasta"
          style={{ background: `conic-gradient(${dilimler.join(', ')})` }}
          role="img"
          aria-label={`${baslik}: ` + sirali.map((s) => `${etiketler[s.deger]} ${s.sayi}`).join(', ')}
        >
          <div className="pasta-orta">
            <span className="pasta-toplam">{toplam}</span>
            <span className="pasta-alt">başvuru</span>
          </div>
        </div>
        <ul className="lejant">
          {sirali.map((s) => (
            <li key={s.deger}>
              <span className="lejant-renk" style={{ background: renkler[s.deger] ?? '#7b8494' }} />
              <span className="lejant-ad">{etiketler[s.deger] ?? `Değer ${s.deger}`}</span>
              <span className="lejant-sayi">{s.sayi}</span>
              <span className="lejant-yuzde">%{Math.round((s.sayi / toplam) * 100)}</span>
            </li>
          ))}
        </ul>
      </div>
    </div>
  )
}

// ---------- Başvuru formu (ekleme ve düzenleme) ----------

type FormProps = {
  duzenlenen: Basvuru | null
  kapat: () => void
  kaydedildi: () => void
}

function BasvuruFormu({ duzenlenen, kapat, kaydedildi }: FormProps) {
  // Düzenlemede form mevcut değerlerle, eklemede boş açılır
  const [form, setForm] = useState<BasvuruIstek>(() => {
    if (!duzenlenen) return bosForm()
    const { id: _id, ...degerler } = duzenlenen
    return degerler
  })
  const [hatalar, setHatalar] = useState<string[]>([])

  // Formdaki tek bir alanı günceller
  function alan<K extends keyof BasvuruIstek>(ad: K, deger: BasvuruIstek[K]) {
    setForm({ ...form, [ad]: deger })
  }

  async function gonder(e: FormEvent) {
    e.preventDefault()
    // Boş metinleri null'a çevir ([Url] boş metni kabul etmiyor)
    const govde = {
      ...form,
      ilanLinki: form.ilanLinki?.trim() || null,
      notlar: form.notlar?.trim() || null,
    }
    // Düzenlemede PUT, eklemede POST
    const adres = duzenlenen ? `/api/Basvurular/${duzenlenen.id}` : '/api/Basvurular'
    const cevap = await fetch(adres, {
      method: duzenlenen ? 'PUT' : 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(govde),
    })
    if (!cevap.ok) {
      setHatalar(await hataMesajlari(cevap))
      return
    }
    kaydedildi()
    kapat()
  }

  return (
    <div className="perde" onClick={kapat}>
      <form className="kart modal" onSubmit={gonder} onClick={(e) => e.stopPropagation()}>
        <h2>{duzenlenen ? 'Başvuruyu düzenle' : 'Yeni başvuru'}</h2>

        <div className="ikili">
          <label>Şirket*<input value={form.sirketAdi} onChange={(e) => alan('sirketAdi', e.target.value)} /></label>
          <label>Pozisyon*<input value={form.pozisyon} onChange={(e) => alan('pozisyon', e.target.value)} /></label>
        </div>

        <div className="ikili">
          <label>Başvuru tarihi*<input type="date" value={form.basvuruTarihi} onChange={(e) => alan('basvuruTarihi', e.target.value)} /></label>
          <label>Maaş (₺)
            <input type="number" value={form.maas ?? ''} onChange={(e) => alan('maas', e.target.value === '' ? null : Number(e.target.value))} />
          </label>
        </div>

        <div className="ikili">
          <label>Durum
            <select value={form.durum} onChange={(e) => alan('durum', Number(e.target.value))}>
              {DURUMLAR.map((d, i) => <option key={d} value={i}>{d}</option>)}
            </select>
          </label>
          <label>Nereden başvurdum
            <select value={form.kaynak} onChange={(e) => alan('kaynak', Number(e.target.value))}>
              {KAYNAKLAR.map((k, i) => <option key={k} value={i}>{k}</option>)}
            </select>
          </label>
        </div>

        <div className="ikili">
          <label>Çalışma şekli
            <select value={form.sekil} onChange={(e) => alan('sekil', Number(e.target.value))}>
              {SEKILLER.map((s, i) => <option key={s} value={i}>{s}</option>)}
            </select>
          </label>
          <label>Şehir
            <select value={form.sehir ?? ''} onChange={(e) => alan('sehir', e.target.value === '' ? null : Number(e.target.value))}>
              <option value="">Belirtilmemiş</option>
              {SEHIRLER.map((s, i) => <option key={s} value={i}>{s}</option>)}
            </select>
          </label>
        </div>

        <label>İlan linki<input value={form.ilanLinki ?? ''} placeholder="https://..." onChange={(e) => alan('ilanLinki', e.target.value)} /></label>
        <label>Notlar<textarea rows={3} value={form.notlar ?? ''} onChange={(e) => alan('notlar', e.target.value)} /></label>

        {hatalar.length > 0 && (
          <ul className="hatalar">
            {hatalar.map((h) => <li key={h}>{h}</li>)}
          </ul>
        )}

        <div className="butonlar">
          <button type="button" className="metin" onClick={kapat}>Vazgeç</button>
          <button type="submit" className="ana">Kaydet</button>
        </div>
      </form>
    </div>
  )
}

export default App