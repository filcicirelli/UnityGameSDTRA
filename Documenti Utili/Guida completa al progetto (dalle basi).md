# Guida completa al progetto "ASTRO" — dalle basi 📚🚀

Questo è il documento dove spiego **tutto** il progetto partendo da zero: cos'è il
gioco, come si gioca, com'è fatto dentro, come l'ho diviso, le immagini e i suoni,
i tipi di funzioni, le API di Unity che ho usato e i colori del codice. È pensato
per ripassare prima dell'esame e per rispondere con sicurezza alle domande.

Questo documento è la visione **d'insieme** (macro). Gli altri documenti, più
specifici, sono:
- `Guida al codice, file per file (livello micro).md` — la spiegazione **di
  dettaglio** del codice, metodo per metodo (il livello "micro");
- `Guida per modificare il codice/Mappa del codice (dove cambiare cosa).md` — dove
  mettere le mani per ogni modifica;
- `Guida per modificare il codice/Colori del codice (oggetti, funzioni, parametri).md` — la leggenda dei colori;
- `Funzioni di Unity usate nel codice.md` — la **tabella completa** di ogni
  funzione di Unity, con cosa fa e dove la uso;
- `20 domande d'esame (con possibili modifiche).md` — le domande probabili e le
  modifiche che potrebbero chiedermi.

---

## 1. Cos'è ASTRO e a cosa serve

**ASTRO** è un piccolo gioco 2D: si guida un alieno a bordo di un UFO (Astro) che
deve **raccogliere le stelline** ("caramelle spaziali"), **evitare gli ostacoli**
(asteroidi e bombe) e infine **raggiungere una porta** per finire il livello.

> **Perché serve nella riabilitazione**
> Il gioco è uno strumento di **riabilitazione motoria**: chiede di compiere
> movimenti controllati (con il mouse, oppure muovendo il braccio davanti a una
> webcam, oppure con un joystick) e premia ogni movimento corretto con un
> **feedback immediato** (suono + animazione). Un rinforzo chiaro e tempestivo
> aiuta il paziente a capire subito se il gesto è giusto e a migliorare prima.

**A cosa serve per l'esame:** dimostra che so **programmare** un'interfaccia
interattiva. Per questo gran parte della logica (movimento, collisioni,
interfaccia, animazioni, riconoscimento del colore alla webcam) è scritta **a
mano**, invece di affidarla ai sistemi automatici di Unity.

---

## 2. Come si gioca (le regole)

- Ogni **livello** ha **due fasi**:
  1. **FASE 1** — raccogli **tutte** le caramelle;
  2. **FASE 2** — quando le hai prese tutte, appare una **porta**: basta
     raggiungerla per vincere il livello.
- Ci sono **3 livelli** di difficoltà crescente:
  1. *Primo volo*: spazio aperto, solo caramelle;
  2. *Tra gli asteroidi*: compaiono le barriere di roccia (fanno male se le tocchi);
  3. *Campo minato*: stessi asteroidi più alcune bombe.
- Si hanno **5 vite** e **60 secondi** per livello. All'inizio c'è un conto
  "**PRONTI…**" di 1,5 secondi in cui le bombe non fanno ancora male.
- Si **perde una vita** toccando un asteroide o una bomba, oppure se finisce il
  tempo. Dopo un colpo c'è circa **1 secondo** di "respiro" in cui non si può
  essere colpiti di nuovo (così non si perdono tutte le vite di colpo).
- Il **punteggio**: +10 per ogni caramella, −5 per ogni colpo.
- Finiti tutti e tre i livelli si arriva alla **schermata di vittoria finale** con
  i crediti.

Tutti questi numeri si regolano in `Impostazioni.cs` (vedi la mappa del codice).

---

## 3. Le tre modalità di comando

> **Due cose diverse da non confondere:** nel gioco ci sono le **modalità di
> comando** (come muovo Astro: mouse, webcam, joystick) e i **livelli** di
> difficoltà (cosa c'è in scena). Sono i due "assi" del gioco; non esistono altre
> modalità tipo storia o arcade.

Astro si muove sempre verso un **puntatore**. Cambia solo **da dove arriva** la
posizione del puntatore. La modalità si sceglie nella **schermata iniziale**.

| Modalità | Come si muove Astro | A chi serve |
|---|---|---|
| **Mouse** | segue il puntatore del mouse (1:1) | controllo fine della mano |
| **Webcam (dito)** | segue un **evidenziatore fluo** (verde/giallo/fucsia) mosso davanti alla webcam | allenare movimenti **ampi del braccio** |
| **Joystick** | un puntatore che si sposta "a velocità" con la leva (vanno anche frecce/WASD) | chi usa ausili con leva |

Il punto chiave dell'architettura è che **Astro non sa** quale modalità è attiva:
chiede solo "dove sta il puntatore?" al file `Comandi.cs`, con il metodo
`Comandi.PuntatoreSchermo()`. Così il resto del gioco non cambia mai, qualunque sia
il comando scelto.

- **Mouse**: restituisce `Input.mousePosition`.
- **Webcam**: il file `ComandoWebcam.cs` analizza ogni fotogramma della webcam,
  cerca i pixel del colore dell'evidenziatore e ne calcola il "centro". Non
  riconosce davvero "il dito" (servirebbe l'intelligenza artificiale): segue un
  **colore acceso**, molto più saturo della pelle, perciò non confonde la mano col
  viso. Il riconoscimento usa il modello **HSV** (tinta/saturazione/luminosità),
  che resiste ai cambi di luce.
- **Joystick**: il puntatore non ha una posizione "assoluta", quindi `Comandi`
  parte dal centro e lo sposta un po' ad ogni fotogramma in base alla leva.

---

## 4. Le basi di Unity (il minimo per capire il resto)

Poche idee che tornano dappertutto nel codice:

| Termine | Cos'è |
|---|---|
| **GameObject** | un "oggetto" in scena (Astro, una caramella, la telecamera…). Da solo è vuoto: prende vita dai componenti. |
| **Component** | qualcosa che si "attacca" a un GameObject e gli dà una capacità (uno `SpriteRenderer` per mostrarsi, un `AudioSource` per suonare, uno script…). |
| **Transform** | la **posizione**, la **rotazione** e la **scala** di ogni GameObject. |
| **MonoBehaviour** | la classe base dei miei script che vivono in scena. Ereditandola, lo script riceve i "messaggi" di Unity (vedi sotto). |
| **Sprite** | un'immagine 2D disegnabile in scena. Lo mostra lo `SpriteRenderer`. |
| **Scena** | il "contenitore" del gioco. Qui la scena parte **vuota**: costruisco tutto da codice (vedi sezione 6). |

I **messaggi di Unity** sono funzioni che Unity chiama **da sola** al momento
giusto: `Awake` e `Start` all'inizio, `Update` ad ogni fotogramma, `OnGUI` per
disegnare l'interfaccia, `OnDestroy` alla fine. Non li chiamo io: li riempio di
codice e Unity li esegue.

### Il fotogramma (frame), il tempo e l'indipendenza dagli FPS

Questa è l'idea più importante da capire, perché ritorna in **ogni** animazione e
in **ogni** timer del gioco.

- Un **fotogramma** (in inglese *frame*) è una **singola immagine** del gioco. Il
  gioco funziona come un cartone animato: disegna tante immagini al secondo e
  l'occhio le vede come movimento continuo.
- Gli **FPS** (*frame per second*, fotogrammi al secondo) dicono **quante immagini
  al secondo** vengono disegnate. 60 FPS = 60 fotogrammi ogni secondo. Più sono,
  più il movimento è fluido.
- **`Update()` viene chiamato una volta per ogni fotogramma**: è il "battito" del
  gioco. Dentro `Update` muovo Astro, controllo i contatti, faccio le animazioni.
  Quindi a 60 FPS il mio `Update` gira 60 volte al secondo.

Il problema è che **gli FPS non sono fissi**: un PC potente può fare 120 fotogrammi
al secondo, uno lento 30. Se in `Update` scrivessi "sposta Astro di 1 ogni
fotogramma", su un PC veloce Astro andrebbe il **doppio** più veloce. Per evitarlo
uso `Time.deltaTime`:

| Cosa | Cos'è | A cosa mi serve |
|---|---|---|
| **`Time.deltaTime`** | i **secondi passati dall'ultimo fotogramma** (a 60 FPS ≈ `0,016`; a 30 FPS ≈ `0,033`) | moltiplicando i movimenti per `deltaTime` ottengo "tanto **al secondo**" invece di "tanto **al fotogramma**": il gioco va **uguale a qualsiasi FPS** |
| **`Time.unscaledDeltaTime`** | come sopra, ma **non** risente di pause/rallentamenti | il conteggio FPS e la barra del dwell |
| **`Time.time`** | i **secondi totali** da quando il gioco è partito | è il mio "orologio" per le onde delle animazioni (`Mathf.Sin(Time.time * velocità)`) |

Esempio concreto: in `GestoreGioco.Update` faccio `TempoRimasto -= Time.deltaTime`.
Così tolgo **esattamente un secondo ogni secondo**, sia a 30 sia a 120 FPS. Stessa
logica per il joystick (`JOYSTICK_VELOCITA * Time.deltaTime` = pixel al secondo) e
per lo smoothing della webcam.

### I tre sistemi di coordinate (mondo, schermo, GUI)

Nel codice convivono **tre** modi di indicare "dove sta un punto", e una parte del
codice serve proprio a **tradurre** da uno all'altro:

| Sistema | Origine (0,0) | Unità | Chi lo usa |
|---|---|---|---|
| **Mondo** (*world*) | al centro | "unità di Unity" | gli oggetti in scena: `transform.position` (es. Astro, le caramelle) |
| **Schermo** (pixel) | in **basso** a sinistra | pixel | il mouse (`Input.mousePosition`) e la webcam |
| **GUI** | in **alto** a sinistra (y ribaltata) | pixel | l'interfaccia disegnata con `OnGUI` |

Per questo nel codice traduco:
- **pixel → mondo** con `telecamera.ScreenToWorldPoint(...)`: è così che Astro va
  dove sta il puntatore del mouse;
- **mondo → GUI** con `telecamera.WorldToScreenPoint(...)` e poi `Screen.height - y`
  (per ribaltare la y): serve a capire se Astro è sopra un pulsante, nel "dwell".

---

## 5. Come ho diviso il progetto

Tutto il codice è in `Assets/Codice/`. L'ho diviso **per argomenti**: ogni cosa ha
il suo file, e le **feature** principali stanno in **sottocartelle dedicate**, ognuna
con una sua "pagina dei parametri" e un suo `LEGGIMI`.

```
Assets/Codice/
├─ GestoreGioco.cs      → l'avvio automatico + il "cervello" del gioco
├─ Oggetti.cs           → tutti gli oggetti (Astro, caramelle, bombe, asteroidi, effetti)
├─ Livelli.cs           → i dati dei livelli + il costruttore della scena
├─ Impostazioni.cs      → i numeri del gioco (vite, tempo, punti, raggi)
├─ FabbricaImmagini.cs  → carica le immagini dai file
├─ InterfacciaGioco.cs  → l'HUD e i pannelli (OnGUI)
├─ schermata start/     → la schermata iniziale e la scelta del comando
├─ comando webcam/      → il comando con la webcam
└─ feedback paziente/   → il feedback sonoro e visivo
```

**Chi comanda chi** (l'architettura, in breve):

- Il **cervello** è `GestoreGioco`: tiene punteggio, vite, tempo e livello, e
  **decide le regole** (cosa succede quando prendo una caramella, quando vengo
  colpito, quando finisce il tempo…). Gli altri oggetti gli **mandano segnali**
  (i metodi `Segnala…`), lui aggiorna lo stato.
- Gli **oggetti** (in `Oggetti.cs`) si occupano solo di sé stessi: Astro si muove
  e controlla i contatti, la caramella sta ferma e si fa raccogliere, la bomba
  pulsa ed esplode. Quando succede qualcosa avvisano il `GestoreGioco`.
- Il **costruttore** `CaricatoreLivelli` (in `Livelli.cs`) crea in scena gli
  oggetti del livello a partire dai **dati** (`DefinizioneLivelli`).
- L'**interfaccia** (`InterfacciaGioco`) legge lo stato dal `GestoreGioco` e lo
  disegna a schermo; non decide nulla del gioco.
- Le tre **feature in cartella** (comandi, webcam, feedback) sono "agganciate" in
  modo che il resto del gioco le usi con **una riga**:
  `Comandi.PuntatoreSchermo()`, `FeedbackPaziente.CaramellaPresa()`.

> **Il trucco "si installa da solo"**
> Diversi oggetti — `Comandi`, `FeedbackPaziente`, `ComandoWebcam`, `SchermataStart`
> — **si creano da soli** all'avvio, grazie all'attributo
> `[RuntimeInitializeOnLoadMethod]`. Il `GestoreGioco` e l'`InterfacciaGioco`,
> invece, li crea la classe `Avvio` (anch'essa all'avvio). Per questo la scena è
> vuota e non devo trascinare niente: il gioco si monta da codice. È una scelta che
> rende ogni passo esplicito e il progetto facile da spostare su un altro computer.

---

## 6. Il flusso di una partita, passo passo

Seguire questo percorso aiuta a capire come si "incastra" tutto:

1. **Avvio.** Il metodo `Avvio.Inizia()` (in `GestoreGioco.cs`) parte da solo:
   toglie telecamere e luci di default, crea una **telecamera 2D** (ortografica) e
   crea i due oggetti che fanno funzionare tutto, `GestoreGioco` e
   `InterfacciaGioco`. In parallelo si installano da soli anche `Comandi`,
   `FeedbackPaziente`, `ComandoWebcam` e `SchermataStart`.
2. **Schermata iniziale.** `GestoreGioco.Start()` non fa partire subito il gioco:
   apre il menu (`MenuInizialeAperto = true`) e mostra solo lo sfondo. La
   `SchermataStart` disegna i tre pannelli (scelta comando, impostazioni,
   parametri tecnici) e il pulsante **GIOCA**.
3. **Inizio partita.** Premendo GIOCA, `GestoreGioco.IniziaPartita(modalità)`
   imposta il comando scelto e chiama `CaricaLivello(0)`.
4. **Costruzione del livello.** `CaricaLivello` azzera lo stato e chiede a
   `CaricatoreLivelli.Carica(...)` di creare in scena sfondo, Astro, asteroidi,
   bombe e caramelle, leggendo i **dati** del livello.
5. **Si gioca (il ciclo `Update`).** Ad ogni fotogramma: Astro si sposta verso il
   puntatore, controlla se tocca una caramella/asteroide/bomba/porta, e si anima
   (inclinazione, feedback). Il `GestoreGioco` fa scorrere i timer.
6. **Eventi.** Quando Astro prende una caramella chiama
   `GestoreGioco.SegnalaCaramellaRaccolta()`, che aggiorna punteggio ed energia e,
   se le caramelle sono finite, fa comparire la **porta**. Toccando un asteroide o
   una bomba si chiama `SegnalaAsteroideToccato()` / `SegnalaBombaColpita()`, che
   tolgono una vita. Ogni evento accende il **feedback** per il paziente.
7. **Fine livello / partita.** Raggiunta la porta si passa al livello successivo;
   finite le vite o il tempo è **game over**; finiti tutti i livelli si arriva alla
   **vittoria finale** con i crediti.

> **Come si fa partire il gioco:** apro il progetto in **Unity 6000.4.7f1** e premo
> **Play (▶)** — non serve preparare la scena, si monta da sé. C'è anche una build
> **WebGL** già pronta (gira a 960×600). Per spostarlo su un altro PC (es. Windows)
> vedi `Come spostare il progetto su Windows.md`.

---

## 7. Le immagini e i suoni (gli asset)

- **Immagini e suoni veri, gratuiti e CC0.** Le immagini (PNG) e i suoni (.ogg)
  sono asset liberi (licenza CC0). I file che il gioco **carica davvero** stanno in
  `Assets/Resources/`; le **fonti e le licenze** sono nelle cartelle `oggetti gioco/`
  e `suoni gioco/` (nella **radice del progetto**, fuori da `Assets`), con il loro
  `LEGGIMI`.
- **Come si caricano.** Tutto passa da `Resources.Load`, che cerca un file in
  `Assets/Resources` **per nome** (senza estensione). Le **immagini** le carica
  `FabbricaImmagini.cs`, che in cima ha una costante per ogni file (`PERSONAGGIO`,
  `STELLA`, `BOMBA`…); i **suoni** li carica `FeedbackPaziente.cs` con i nomi
  scritti in `ParametriFeedback.cs` (`raccolta`, `vittoria`, `errore`).
- **Lo sfondo** (la nebulosa, `sfondo.jpg`) fa eccezione: lo carica direttamente
  `Livelli.cs` nel metodo `CostruisciSfondo()`.
- **Sprite e colori.** Un'immagine diventa uno `Sprite` con `Sprite.Create`. Lo
  `SpriteRenderer` la disegna e può **tingerla** con `.color` (così la stessa
  stellina chiara diventa gialla, azzurra, fucsia…) e decidere chi sta davanti con
  `.sortingOrder` (Astro è in alto, lo sfondo dietro a tutto).
- **Rete di sicurezza.** Se un file manca, il gioco **non si blocca**: l'immagine
  diventa un quadrato magenta e il suono semplicemente non parte, con un avviso nel
  log.

---

## 8. I tipi di funzioni nel codice

Nel progetto le funzioni non sono tutte uguali: ognuna ha un "ruolo". Distinguerle
aiuta a spiegare il codice.

| Tipo di funzione | Chi la chiama | Esempi |
|---|---|---|
| **Messaggi di Unity** (ciclo di vita) | Unity, **da sola**, al momento giusto | `Awake`, `Start`, `Update`, `OnGUI`, `OnEnable`/`OnDisable`, `OnDestroy` |
| **Comandi / eventi pubblici** | gli **altri file** del gioco | `GestoreGioco.SegnalaCaramellaRaccolta()`, `FeedbackPaziente.CaramellaPresa()`, `Comandi.PuntatoreSchermo()` |
| **Proprietà calcolate** (`get`) | si leggono come un dato, ma fanno un piccolo calcolo | `GestoreGioco.Energia`, `BombeAttive`, `TestoObiettivo`, `ComandoWebcam.Pronta` |
| **Inizializzatori** | chi crea l'oggetto, per "prepararlo" | `Caramella.Inizializza(...)`, `Asteroide.Inizializza(...)` |
| **Metodi privati / aiutanti** | solo dentro lo stesso file (dettagli) | `PerdiUnaVita`, `Riquadro`, `ColoreGiusto`, `DisegnaContatori` |
| **Coroutine** | si avvia con `StartCoroutine`, può **aspettare** nel tempo | `ComandoWebcam.AvviaWebcam()` (aspetta il permesso webcam) |
| **Auto-installazione** | Unity all'avvio, grazie all'attributo speciale | `Installa()` con `[RuntimeInitializeOnLoadMethod]` |

Tre dettagli utili da sapere:

- **`static`** vuol dire "una cosa sola per tutto il gioco". Lo uso per i metodi
  comodi (`FeedbackPaziente.CaramellaPresa()` si chiama senza avere l'oggetto in
  mano) e per il riferimento `Istanza`, con cui ogni oggetto trova facilmente il
  suo "unico esemplare" (es. `GestoreGioco.Istanza`).
- **Le liste statiche** come `Caramella.Attive`, `Bomba.Tutte`, `Asteroide.Tutti`
  tengono l'elenco di tutti gli oggetti di quel tipo presenti nel livello: così
  Astro può controllarli tutti con un semplice ciclo `for`.
- **Le classi statiche-libreria**: alcune classi non si attaccano a un oggetto e non
  hanno `Istanza`; sono solo **contenitori di funzioni e valori** che chiamo col loro
  nome, come `FabbricaImmagini.CreaAstro()`, `CaricatoreLivelli.Carica(...)`,
  `DefinizioneLivelli.Ottieni(...)` e le pagine dei parametri (`Impostazioni`,
  `ParametriFeedback`…). Sono diverse dai `MonoBehaviour` con `Istanza`: non vivono
  in scena, sono solo "scatole di strumenti".

---

## 9. Le API di Unity che ho usato (panoramica)

Qui riassumo **per area** le funzioni di libreria di Unity. La **tabella completa**,
voce per voce con "cosa fa / dove la uso / come si modifica", è nel documento
`Funzioni di Unity usate nel codice.md`.

| Area | Funzioni/strumenti principali | Dove |
|---|---|---|
| **Ciclo di vita** | `MonoBehaviour`, `Awake`, `Start`, `Update`, `OnGUI`, `OnEnable`/`OnDisable`, `OnDestroy`, attributo `[RuntimeInitializeOnLoadMethod]`, coroutine (`IEnumerator`/`yield`) | tutti i file |
| **Oggetti in scena** | `new GameObject`, `AddComponent`, `GetComponent`, `Destroy` (anche a tempo), `FindObjectsByType`, `transform` (`position`, `localScale`, `rotation`, `SetParent`) | Livelli, Oggetti, GestoreGioco |
| **Telecamera** | `Camera.main`, `orthographic`/`orthographicSize`, `ScreenToWorldPoint`, `WorldToScreenPoint` | Avvio, Astro, Interfaccia |
| **Immagini/sprite** | `Resources.Load`, `Sprite.Create`, `SpriteRenderer` (`sprite`, `color`, `sortingOrder`), `Texture2D`, `Texture2D.whiteTexture` | FabbricaImmagini, Livelli, Interfaccia |
| **Matematica e tipi** | `Mathf` (`Sin`, `Cos`, `Lerp`, `Clamp`, `Max`/`Min`…), `Vector2`/`Vector3`, `Vector2.Distance`, `Color`/`Color32`, `Color.Lerp`, `Color.RGBToHSV`, `Rect.Contains`, `Random.Range` | ovunque |
| **Tempo** | `Time.deltaTime`, `Time.unscaledDeltaTime`, `Time.time` | timer e animazioni |
| **Comandi (vecchio Input Manager)** | `Input.mousePosition`, `Input.GetAxisRaw`, `Input.GetKeyDown`, `Cursor.visible` | Comandi, Interfaccia |
| **Interfaccia (IMGUI)** | `OnGUI`, `GUI.Label`/`Button`/`Box`, `GUI.DrawTexture`, `GUIStyle`, `Event.current` | Interfaccia, SchermataStart, Webcam |
| **Audio** | `AudioSource`, `PlayOneShot`, `AudioListener`, `AudioClip` | FeedbackPaziente |
| **Webcam** | `WebCamTexture` (`Play`/`Pause`/`Stop`, `GetPixels32`…), `WebCamTexture.devices`, `Application.RequestUserAuthorization` | ComandoWebcam |
| **Info di sistema** | `SystemInfo`, `Screen`, `Application`, `QualitySettings`, `Debug.LogWarning` | pannelli tecnici (F3 e schermata iniziale) |

> Uso il **vecchio sistema di Input** (Input Manager) e **non** il nuovo Input
> System: è più semplice, non richiede pacchetti aggiuntivi e funziona ovunque,
> anche su Windows e nella build WebGL.

---

## 10. I colori del codice

Per leggere e spiegare meglio il codice, **ogni tipo di cosa ha un colore** (la
leggenda completa è in `Guida per modificare il codice/Colori del codice (oggetti, funzioni, parametri).md`):

- 🟩 **verde acqua** → gli **oggetti** (classi/enum): `Astro`, `GestoreGioco`;
- 🟨 **giallo** → le **funzioni** (metodi): `IniziaPartita()`, `Gonfia()`;
- 🟦 **azzurro** → lo **stato** della partita (campi/proprietà): `Punteggio`, `Vite`;
- 🟧 **arancione** → i **parametri/costanti** da regolare: `VITE`, `TEMPO_LIVELLO`.

I nomi sono scritti in modo coerente — Maiuscola per gli oggetti, `()` per le
funzioni, MAIUSCOLO per i parametri — quindi la leggenda funziona anche su carta o
su un altro editor.

---

## 11. Perché certe scelte (e cosa avrei potuto usare al posto)

Alcune cose le faccio **a mano** anche se Unity avrebbe un sistema pronto: per un
esame in cui devo dimostrare di **saper programmare**, scrivere io la logica vale
più che affidarla a un sistema automatico. (Il confronto completo è in
`Funzioni di Unity usate nel codice.md`, sezione *"Cosa NON ho sostituito e perché"*.)

| Faccio a mano | Alternativa di Unity | Perché a mano |
|---|---|---|
| **Collisioni** (distanze e "punto dentro cerchio") | fisica 2D (`Collider2D` + `Rigidbody2D`) | preciso, prevedibile, mostra la matematica |
| **Interfaccia** con `OnGUI` | UI moderna (Canvas, Button) | tiene tutta l'interfaccia nel codice, senza prefab |
| **Animazioni** con `Mathf.Sin` | `Animator` + clip | poche righe danno pulsazioni e dondolii |
| **Oggetti costruiti da codice** | Prefab + `Instantiate` | ogni passo è esplicito, non dipende da file di scena |
| **Coriandoli ed esplosione** | Particle System | mostra come gestisco tanti oggetti e la loro durata |
| **Riconoscimento del colore** alla webcam | librerie esterne / IA | nessuna dipendenza, e capisco/spiego ogni riga |

---

## 12. Glossario rapido

- **Fotogramma (frame)**: una singola immagine del gioco; il gioco ne disegna tante al secondo.
- **FPS**: fotogrammi al secondo (quante immagini disegna ogni secondo).
- **`Time.deltaTime`**: i secondi passati dall'ultimo fotogramma; moltiplico i movimenti per questo così il gioco va uguale a qualsiasi FPS.
- **`Time.time`**: i secondi totali dall'avvio; lo uso come orologio per le animazioni.
- **`Update()`**: il metodo chiamato da Unity una volta per ogni fotogramma.
- **`Lerp`**: prende un valore intermedio fra due (movimenti morbidi invece che a scatti).
- **`Vector2` / `Vector3`**: coppie/terne di numeri (x,y) o (x,y,z): posizioni, dimensioni, velocità.
- **Sprite**: immagine 2D in scena.
- **Renderer (`SpriteRenderer`)**: il componente che disegna lo sprite.
- **`sortingOrder`**: chi viene disegnato davanti (numero più alto = più avanti).
- **`Istanza`**: il riferimento all'unico esemplare di un oggetto (singleton).
- **`static`**: "uno solo per tutto il gioco".
- **`enum`**: un elenco di valori con un nome (es. `Modalita { Mouse, Dito, Joystick }`).
- **HSV**: modo di descrivere un colore con tinta/saturazione/luminosità.
- **Dwell**: "premere" un pulsante tenendoci sopra Astro per un po' (senza clic).
- **HUD**: le scritte e le barre a schermo durante il gioco.
- **IMGUI / `OnGUI`**: il sistema di interfaccia "scritto a codice" che uso.
- **CC0**: licenza che rende un'opera libera da usare, anche senza citare l'autore.

---

*Materiale di supporto per l'esame "Sistemi per la Riabilitazione e la Terapia
Assistita". Autore: Filippo Cicirelli.*
