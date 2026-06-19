# Feedback Paziente 🎵✨

Questa cartella aggiunge al gioco un **feedback sonoro e visivo** per ogni azione
del paziente:

| Azione del paziente | Suono | Effetto su Astro |
|---|---|---|
| Prende una **stellina** | suono breve e allegro che **sale** | si **gonfia** e brilla di verde 🟢 |
| Raggiunge la **porta** / vince | piccolo **jingle** di vittoria (pizzicato) | si **gonfia** e brilla 🟢 |
| Tocca **asteroide / bomba** | suono basso e gentile che **scende** | si **schiaccia** e lampeggia di rosso 🔴 |
| **Tempo scaduto** | come l'errore | si schiaccia 🔴 |

> **Perché serve nella riabilitazione**
> Un feedback **immediato** e su **due canali insieme** (orecchio + occhio) aiuta il
> paziente a capire *subito* se il movimento è corretto, rinforzando i gesti giusti.
> Il suono dell'errore è apposta **più basso e gentile** di quello positivo
> (`VOLUME_SBAGLIATO` < `VOLUME_GIUSTO`): deve **informare**, non spaventare.

---

## Il corpo del codice (spiegato in breve)

Due file di codice, nello stesso stile del resto del progetto:

### 1. `ParametriFeedback.cs` — la *pagina dei valori*
È l'unico file da toccare per regolare il feedback (come `Impostazioni.cs` per il
gioco). Contiene **solo numeri/valori**, nessuna logica: volumi, interruttori
(suono/visivo on-off), i **nomi dei file audio** e quanto Astro si
gonfia/schiaccia (con i colori). La tabella completa è più sotto.

### 2. `FeedbackPaziente.cs` — il *cervello* del feedback
È un oggetto che **si installa da solo** all'avvio (non serve trascinarlo in scena).
All'avvio:
- garantisce **un solo `AudioListener`** ("le orecchie"): il gioco ricrea la
  telecamera da zero e quella di default verrebbe distrutta, quindi il sistema
  rimuove eventuali orecchie e ne mette una sola su di sé (così funziona sempre,
  a prescindere dall'ordine di avvio);
- crea un **`AudioSource`** ("l'altoparlante") e **carica i tre suoni** una volta
  sola dai file audio in `Assets/Resources` (`Resources.Load<AudioClip>`).

I suoni sono **file audio veri** (`.ogg`, gratuiti e CC0): si trovano nella
cartella **`suoni gioco/`** (con il loro `LEGGIMI.txt` su fonti e licenze) e sono
copiati in `Assets/Resources/` con i nomi `raccolta`, `vittoria`, `errore`.

> Rete di sicurezza: se un file audio mancasse, il clip resta vuoto e il gioco
> **non si blocca** (semplicemente quel feedback sonoro non parte), con un avviso
> nel log.

Espone tre comandi semplici che gli altri file chiamano con **una riga**:

```csharp
FeedbackPaziente.CaramellaPresa();    // azione giusta (stellina raccolta)
FeedbackPaziente.MissioneCompiuta();  // vittoria
FeedbackPaziente.AzioneSbagliata();   // errore
```

Ogni comando fa suonare il suono giusto **e** dice ad Astro di gonfiarsi o
schiacciarsi (rispettando gli interruttori `SUONO_ATTIVO` / `VISIVO_ATTIVO`).

---

## Come è collegato al resto del gioco

Il feedback parte sempre dal "cervello" del gioco, `GestoreGioco.cs`, che già sapeva
quando un'azione era giusta o sbagliata. C'è **una sola riga** per evento:

| File / metodo | Riga aggiunta |
|---|---|
| `GestoreGioco.SegnalaCaramellaRaccolta()` | `FeedbackPaziente.CaramellaPresa();` |
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
| **File dei suoni** (in `Assets/Resources`, nome senza estensione) | | |
| `SUONO_RACCOLTA` | `"raccolta"` | suono quando si prende una stellina |
| `SUONO_VITTORIA` | `"vittoria"` | suono di livello completato |
| `SUONO_ERRORE` | `"errore"` | suono di azione sbagliata (gentile) |
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
- I **suoni** sono file `.ogg` (CC0) in `Assets/Resources`; gli **effetti visivi**
  (gonfia/schiaccia) sono fatti da codice nella classe `Astro`.
- I file `.meta` di Unity per questa cartella e per i nuovi script vengono
  generati **automaticamente** la prima volta che apri il progetto in Unity.
- Per cambiare un suono: metti un altro file in `Assets/Resources` e aggiorna la
  costante corrispondente in `ParametriFeedback.cs` (es. `SUONO_VITTORIA`).
  Vedi anche `suoni gioco/LEGGIMI.txt`.
