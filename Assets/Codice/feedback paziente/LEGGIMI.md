# Feedback Paziente 🎵✨

Questa cartella aggiunge al gioco un **feedback sonoro e visivo** per ogni azione
del paziente:

| Azione del paziente | Suono | Effetto su Astro |
|---|---|---|
| Prende una **caramella** | tre note che **salgono** (do‑mi‑sol), allegre | si **gonfia** e brilla di verde 🟢 |
| Prende la **chiave** | note acute e brillanti (sol‑do‑mi) | si **gonfia** e brilla 🟢 |
| Apre la **porta** / vince | piccola **fanfara** (do‑mi‑sol‑do) | si **gonfia** e brilla 🟢 |
| Tocca **asteroide / bomba** | due note **basse** che scendono | si **schiaccia** e lampeggia di rosso 🔴 |
| **Tempo scaduto** | come l'errore | si schiaccia 🔴 |

> **Perché serve nella riabilitazione**
> Un feedback **immediato** e su **due canali insieme** (orecchio + occhio) aiuta il
> paziente a capire *subito* se il movimento è corretto, rinforzando i gesti giusti.
> Il suono dell'errore è apposta **più basso e gentile** di quello positivo
> (`VOLUME_SBAGLIATO` < `VOLUME_GIUSTO`): deve **informare**, non spaventare.

---

## Il corpo del codice (spiegato in breve)

Tutto è generato **da codice**, senza file audio o immagini esterne — esattamente
come il resto del gioco (vedi `FabbricaImmagini.cs`). Tre file:

### 1. `ParametriFeedback.cs` — la *pagina dei valori*
È l'unico file da toccare per regolare il feedback (come `Impostazioni.cs` per il
gioco). Contiene **solo numeri**, nessuna logica: volumi, note, durate, quanto Astro
si gonfia/schiaccia, i colori. La tabella completa è più sotto.

### 2. `FabbricaSuoni.cs` — costruisce i suoni
Un suono è una lista di numeri fra ‑1 e +1 (i *campioni*). Il metodo
`CreaMelodia(...)` mette in fila una o più note (onde sinusoidali) e ci applica un
*inviluppo* (il volume sale in fretta e poi scende piano, così la nota non fa
"click"). Per l'errore usa l'opzione `ruvido`, che somma una seconda onda un po'
stonata: le due "battono" insieme e il suono diventa volutamente sgradevole.

### 3. `FeedbackPaziente.cs` — il *cervello* del feedback
È un oggetto che **si installa da solo** all'avvio (non serve trascinarlo in scena).
All'avvio:
- garantisce **un solo `AudioListener`** ("le orecchie"): il gioco ricrea la
  telecamera da zero e quella di default verrebbe distrutta, quindi il sistema
  rimuove eventuali orecchie e ne mette una sola su di sé (così funziona sempre,
  a prescindere dall'ordine di avvio);
- crea un **`AudioSource`** ("l'altoparlante") e i quattro suoni una volta sola.

Espone quattro comandi semplici che gli altri file chiamano con **una riga**:

```csharp
FeedbackPaziente.CaramellaPresa();    // azione giusta
FeedbackPaziente.ChiavePresa();       // azione giusta
FeedbackPaziente.MissioneCompiuta();  // vittoria
FeedbackPaziente.AzioneSbagliata();   // errore
```

Ogni comando fa suonare il suono giusto **e** dice ad Astro di gonfiarsi o
schiacciarsi (rispettando gli interruttori `SUONO_ATTIVO` / `VISIVO_ATTIVO`).

---

## Come è collegato al resto del gioco

Il feedback parte sempre dal "cervello" del gioco, `GestoreGioco.cs`, che già sapeva
quando un'azione era giusta o sbagliata. Ho aggiunto **una sola riga** per evento:

| File / metodo | Riga aggiunta |
|---|---|
| `GestoreGioco.SegnalaCaramellaRaccolta()` | `FeedbackPaziente.CaramellaPresa();` |
| `GestoreGioco.SegnalaChiaveRaccolta()` | `FeedbackPaziente.ChiavePresa();` |
| `GestoreGioco.SegnalaPortaRaggiunta()` | `FeedbackPaziente.MissioneCompiuta();` |
| `GestoreGioco.AttivaVittoriaFinale()` | `FeedbackPaziente.MissioneCompiuta();` |
| `GestoreGioco.SegnalaAsteroideToccato()` | `FeedbackPaziente.AzioneSbagliata();` |
| `GestoreGioco.SegnalaBombaColpita()` | `FeedbackPaziente.AzioneSbagliata();` |
| `GestoreGioco.TempoScaduto()` | `FeedbackPaziente.AzioneSbagliata();` |

La parte **visiva** (Astro che si gonfia/schiaccia e cambia colore) vive dentro
`Oggetti.cs`, nella classe `Astro`, perché è Astro a ridisegnarsi ogni fotogramma.
I metodi sono `Astro.Gonfia()` e `Astro.Schiaccia()`, e leggono i numeri da
`ParametriFeedback`.

---

## 📋 Pagina dei parametri modificabili (`ParametriFeedback.cs`)

| Variabile | Valore | Cosa fa |
|---|---|---|
| **Interruttori** | | |
| `SUONO_ATTIVO` | `true` | accende/spegne tutti i suoni (es. paziente sensibile ai rumori) |
| `VISIVO_ATTIVO` | `true` | accende/spegne tutti gli effetti visivi |
| **Volumi (0–1)** | | |
| `VOLUME_GENERALE` | `0.90` | volume di tutto il feedback |
| `VOLUME_GIUSTO` | `0.80` | volume delle azioni corrette |
| `VOLUME_SBAGLIATO` | `0.45` | volume degli errori (più basso = più gentile) |
| **Note dei suoni (Hz)** | | |
| `NOTE_CARAMELLA` | do‑mi‑sol | melodia della caramella |
| `NOTE_CHIAVE` | sol‑do‑mi acuti | melodia della chiave |
| `NOTE_VITTORIA` | do‑mi‑sol‑do | fanfara di vittoria |
| `NOTE_ERRORE` | due note basse | suono dell'errore (scendono) |
| `DURATA_NOTA_GIUSTO` | `0.10` s | durata di ogni nota positiva |
| `DURATA_NOTA_ERRORE` | `0.18` s | durata di ogni nota di errore |
| **Astro si gonfia (positivo)** | | |
| `GONFIA_QUANTITA` | `0.45` | quanto cresce (0.45 = +45%) |
| `GONFIA_DURATA` | `0.45` s | quanto dura il gonfiamento |
| `COLORE_GIOIA` | verde chiaro | colore del bagliore positivo |
| `GIOIA_INTENSITA` | `0.80` | forza del bagliore (0–1) |
| **Astro si schiaccia (negativo)** | | |
| `SCHIACCIA_QUANTITA` | `0.30` | quanto si appiattisce (+30% largo, −30% alto) |
| `SCHIACCIA_DURATA` | `0.30` s | durata dello schiacciamento |
| `COLORE_ERRORE` | rosso | colore del lampeggio di errore |

### Esempi di regolazione
- Feedback **più evidente** per chi fatica a notarlo: alza `GONFIA_QUANTITA` a `0.7`.
- Ambiente **silenzioso** (es. ospedale): metti `SUONO_ATTIVO = false`.
- Errore **ancora più dolce**: abbassa `VOLUME_SBAGLIATO` a `0.25`.

---

## Note tecniche
- **Nessun file esterno**: suoni e immagini sono creati da codice.
- Le immagini `.meta` di Unity per questa cartella e per i nuovi script vengono
  generate **automaticamente** la prima volta che apri il progetto in Unity.
- Per aggiungere un nuovo suono: aggiungi le note in `ParametriFeedback`, crealo
  in `FeedbackPaziente.Awake()` con `FabbricaSuoni.CreaMelodia(...)` e richiamalo.
