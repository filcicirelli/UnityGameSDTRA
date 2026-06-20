# Funzioni di libreria Unity usate nel gioco "ASTRO"

Questo documento elenca **tutte le funzioni, classi e proprietà di Unity** che ho
usato nel codice del gioco. Per ognuna spiego:

- **cosa fa** (a cosa serve);
- **dove la uso** (in quale file del progetto);
- **come si può modificare** (cosa cambiare per ottenere un effetto diverso, o
  con quale altra funzione Unity si potrebbe sostituire).

Tutto il codice sta in `Assets/Codice/`. I numeri di gioco regolabili (vite,
tempo, raggi, ecc.) stanno nei file `Impostazioni.cs`, `ParametriFeedback.cs`,
`ParametriComandi.cs`, `ParametriWebcam.cs`.

> Nota didattica: la maggior parte della logica del gioco (collisioni,
> interfaccia, animazioni) è scritta a mano apposta, per mostrare la
> programmazione invece di affidarla ai sistemi automatici di Unity. In fondo al
> documento, nella sezione **"Cosa NON ho sostituito e perché"**, spiego quali
> funzioni Unity avrebbero potuto rimpiazzare quel codice e perché ho scelto di
> tenerlo scritto da me.

---

## 1. Avvio automatico e ciclo di vita di un oggetto

In Unity uno script che eredita da `MonoBehaviour` riceve da solo delle
"chiamate" (i *messaggi*) in momenti precisi della vita dell'oggetto.

| Funzione / messaggio | Cosa fa | Dove la uso |
|---|---|---|
| `MonoBehaviour` | Classe base da cui ereditano tutti i miei script che vivono nella scena. | Tutti i file (Astro, Bomba, GestoreGioco, ...). |
| `Awake()` | Chiamata **una volta**, appena l'oggetto nasce: la uso per le inizializzazioni e per registrare il riferimento `Istanza`. | GestoreGioco, Astro, Comandi, FeedbackPaziente, ComandoWebcam... |
| `Start()` | Chiamata una volta, **dopo** tutti gli `Awake`: la uso quando devo essere sicuro che gli altri oggetti esistano già. | GestoreGioco, Esplosione, PezzoCoriandolo. |
| `Update()` | Chiamata **ad ogni fotogramma**: è il cuore del movimento e dei controlli. | Astro, Caramella, Bomba, GestoreGioco, Comandi... |
| `OnGUI()` | Chiamata ad ogni fotogramma per disegnare l'interfaccia "vecchio stile" (IMGUI). | InterfacciaGioco, SchermataStart, ComandoWebcam. |
| `OnEnable()` / `OnDisable()` | Chiamate quando l'oggetto si accende/spegne: le uso per aggiungere/togliere l'oggetto dalle liste (es. tutte le caramelle attive). | Caramella, Bomba, Asteroide. |
| `OnDestroy()` | Chiamata quando l'oggetto viene distrutto: la uso per azzerare il riferimento `Istanza` e spegnere la webcam. | Astro, Porta, FeedbackPaziente, ComandoWebcam. |

**Come modificarli:** la logica si cambia scrivendo dentro questi metodi. Se una
cosa deve succedere ogni frame va in `Update`; se deve succedere una volta sola
va in `Awake`/`Start`.

### Avvio del gioco senza scena preparata

| Funzione | Cosa fa | Dove la uso |
|---|---|---|
| `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]` | Attributo che fa partire un metodo **da solo** appena il gioco si avvia, senza dover trascinare nulla nella scena in Unity. | `Avvio.Inizia` (Oggetti.cs), e l'auto-installazione di Comandi, FeedbackPaziente, ComandoWebcam, SchermataStart. |

**Come modificarlo:** è ciò che mi permette di costruire tutto da codice. Se in
futuro volessi partire da una scena preparata a mano in Unity, toglierei questi
attributi e metterei gli oggetti direttamente nella scena.

### Coroutine (azioni che aspettano nel tempo)

| Funzione | Cosa fa | Dove la uso |
|---|---|---|
| `StartCoroutine(...)` + `IEnumerator` + `yield return` | Avvia una funzione che può **mettersi in pausa** e riprendere più tardi, senza bloccare il gioco. | `ComandoWebcam.AvviaWebcam`: aspetta il permesso di usare la webcam. |
| `yield break` | Esce subito dalla coroutine. | Stessa funzione, se il permesso è negato. |

**Come modificarla:** per far aspettare *un tot di secondi* dentro una coroutine
si usa `yield return new WaitForSeconds(secondi)`. Io aspetto invece la risposta
di `RequestUserAuthorization`.

---

## 2. GameObject, Component, Transform

Sono i mattoni di Unity: ogni cosa in scena è un `GameObject`, a cui si
"attaccano" dei componenti (`Component`), e ognuno ha una posizione (`Transform`).

| Funzione | Cosa fa | Dove la uso |
|---|---|---|
| `new GameObject("nome")` | Crea un oggetto vuoto in scena con quel nome. | Ovunque costruisco oggetti da codice (Livelli.cs soprattutto). |
| `AddComponent<T>()` | Attacca un componente (uno script, uno SpriteRenderer, un AudioSource...) all'oggetto. | Livelli.cs (sprite e script), FeedbackPaziente (AudioSource), ecc. |
| `GetComponent<T>()` | Recupera un componente già presente sull'oggetto. | Astro, Bomba, Esplosione (per prendere lo SpriteRenderer). |
| `Object.Destroy(oggetto)` | Distrugge un oggetto/componente. | Pulizia livello, raccolta caramella, esplosione bomba. |
| `Destroy(gameObject, tempo)` | Distrugge l'oggetto **dopo "tempo" secondi**, da solo. | Coriandoli, Esplosione, PezzoCoriandolo. |
| `Object.FindObjectsByType<T>()` | Trova tutti gli oggetti di un certo tipo presenti in scena. | `Avvio` (per togliere telecamere e luci di default), FeedbackPaziente (per togliere gli AudioListener doppi). |
| `gameObject`, `.tag`, `.name` | Riferimento all'oggetto a cui è attaccato lo script, la sua etichetta e il suo nome. | Vari. |

**Transform (posizione, scala, rotazione):**

| Funzione | Cosa fa | Dove la uso |
|---|---|---|
| `transform.position` | Posizione nel mondo. La leggo e la scrivo per **muovere** Astro e le bombe, e per posizionare gli oggetti del livello. | Astro.Update, Bomba.Update, Livelli... |
| `transform.localScale` | Dimensione dell'oggetto. La cambio per gli effetti gonfia/schiaccia e per le pulsazioni, e la imposto per dare la misura a rocce e oggetti. | Astro (gonfia/schiaccia), asteroidi, Porta, Esplosione, PianetaAmico. |
| `transform.localRotation` / `transform.rotation` / `transform.Rotate(...)` | Rotazione dell'oggetto. | Astro (inclinazione), PezzoCoriandolo (giravolte). |
| `transform.SetParent(genitore)` | Mette l'oggetto "dentro" un altro, per tenere ordinata la gerarchia. | Livelli.cs (contenitori Caramelle/Asteroidi/Bombe), Coriandoli, alone della bomba. |
| `transform.localPosition` | Posizione **rispetto al genitore**. | Alone della bomba, coriandoli. |

**Come modificarli:** sono i comandi base. Per spostare qualcosa cambio
`position`; per ingrandirlo cambio `localScale`; per ruotarlo `Rotate` o
`localRotation = Quaternion.Euler(...)`.

---

## 3. Telecamera

| Funzione | Cosa fa | Dove la uso |
|---|---|---|
| `Camera`, `Camera.main` | La telecamera principale (quella con tag "MainCamera"). | Astro, InterfacciaGioco, Livelli. |
| `.orthographic`, `.orthographicSize` | Rendono la vista 2D (senza prospettiva) e ne regolano lo "zoom". | `Avvio.Inizia`. |
| `.backgroundColor` | Colore di sfondo della telecamera (qui nero, si vede solo se manca lo sfondo). | `Avvio.Inizia`. |
| `.aspect` | Rapporto larghezza/altezza della finestra. | Livelli.cs, per ingrandire lo sfondo finché copre lo schermo. |
| `ScreenToWorldPoint(punto)` | Converte una posizione in **pixel dello schermo** (es. il mouse) in una posizione nel **mondo del gioco**. | Astro.Update: è così che Astro segue il puntatore. |
| `WorldToScreenPoint(punto)` | Il contrario: dal mondo ai pixel dello schermo. | InterfacciaGioco: per capire se Astro sta sopra un pulsante (dwell). |

**Come modificarla:** `orthographicSize = 6` significa che si vedono 6 unità
sopra e 6 sotto il centro. Alzandolo si "allontana" la telecamera (si vede di
più), abbassandolo si "avvicina".

---

## 4. Immagini, sprite e texture

| Funzione | Cosa fa | Dove la uso |
|---|---|---|
| `Resources.Load<T>("nome")` | Carica un file (immagine o suono) dalla cartella `Assets/Resources`, usando il nome **senza estensione**. | FabbricaImmagini (PNG), FeedbackPaziente (audio), Livelli (sfondo). |
| `Sprite.Create(texture, rect, pivot, ppu, ...)` | Trasforma una texture in uno **sprite** disegnabile in scena. | FabbricaImmagini, Livelli (sfondo). |
| `SpriteRenderer` | Il componente che **disegna** uno sprite. Proprietà che uso: `.sprite`, `.color` (tinta), `.sortingOrder` (chi sta davanti). | Astro, caramelle, bombe, asteroidi, porta, ecc. |
| `Texture2D` | Un'immagine in memoria. Metodi che uso: `new Texture2D(...)`, `.SetPixel`, `.SetPixels`, `.Apply`, `.filterMode`, `.wrapMode`. | FabbricaImmagini (quadrato pieno per l'alone della bomba). |
| `Texture2D.whiteTexture` | Una texture bianca 1×1 **già pronta** di Unity. La coloro con `GUI.color` per disegnare rettangoli pieni senza crearne una nuova ogni volta. | InterfacciaGioco, SchermataStart, ComandoWebcam (metodo `Riquadro`/mirino). |
| `TextureFormat.RGBA32`, `FilterMode.Point`, `TextureWrapMode.Clamp` | Impostazioni della texture: formato colore con trasparenza, niente sfocatura, niente ripetizione ai bordi. | FabbricaImmagini.CreaQuadratoPieno. |

**Come modificarli:**
- per **cambiare un'immagine** basta mettere un altro PNG in `Assets/Resources`
  e scrivere il suo nome nelle costanti in cima a `FabbricaImmagini.cs`;
- `ppu` (pixel per unità) in `Sprite.Create` decide quanto grande appare lo
  sprite: io uso il lato più lungo, così immagini di misure diverse risultano
  più o meno grandi uguali;
- `sortingOrder` più alto = disegnato **davanti** (Astro è a 10, lo sfondo a -10).

---

## 5. Matematica e tipi di base

### Mathf (funzioni matematiche)

| Funzione | Cosa fa | Dove la uso |
|---|---|---|
| `Mathf.Clamp(v, min, max)` / `Mathf.Clamp01(v)` | Tiene un numero dentro un intervallo (Clamp01 = fra 0 e 1). | Indice livello, barra energia, inclinazione di Astro, avanzamento animazioni. |
| `Mathf.Max(...)` / `Mathf.Min(...)` | Il più grande / il più piccolo fra due numeri. | Punteggio (non sotto 0), vite, dimensione sfondo, guardia su deltaTime. |
| `Mathf.Abs(v)` | Valore assoluto (toglie il segno). | Distanza dalla telecamera, zona morta del joystick, distanza fra tinte. |
| `Mathf.Sin(x)` / `Mathf.Cos(x)` | Seno e coseno: danno onde che salgono e scendono, perfette per **pulsazioni e dondolii**. | Le animazioni (Astro gonfia/schiaccia, bombe, porta, coriandoli). |
| `Mathf.Lerp(a, b, t)` | Valore intermedio fra `a` e `b` (t da 0 a 1): movimenti **morbidi** invece che a scatti. | Inclinazione di Astro, esplosione, media FPS, colore del tempo. |
| `Mathf.Pow(base, esp)` | Elevamento a potenza. | Smoothing della webcam indipendente dagli FPS. |
| `Mathf.PI` | Il pi greco (3,14...). | Fasi casuali delle animazioni, "campana" del gonfiamento. |
| `Mathf.CeilToInt(v)` / `Mathf.RoundToInt(v)` | Arrotonda per eccesso / al più vicino, restituendo un intero. | Secondi del timer, FPS mostrati. |

### Vettori, rotazioni, colori, rettangoli

| Funzione | Cosa fa | Dove la uso |
|---|---|---|
| `Vector2` / `Vector3` | Coppie/terne di numeri (x,y) o (x,y,z): posizioni, dimensioni, velocità. | Ovunque. |
| `Vector2.Distance(a, b)` | Distanza fra due punti. | **Collisioni** caramella/bomba/porta, scelta posizioni. |
| `Vector2.Lerp(a, b, t)` | Punto intermedio fra due posizioni (movimento morbido). | Smoothing del puntatore webcam. |
| `.magnitude` / `.sqrMagnitude` | Lunghezza di un vettore (al quadrato per la versione veloce, senza radice). | Velocità di Astro, distanza dal centro, **collisione con gli asteroidi**. |
| `Vector3.zero` / `Vector3.one` | Le scorciatoie (0,0,0) e (1,1,1). | Inizializzazioni di scala/posizione. |
| `Quaternion.Euler(x, y, z)` | Costruisce una rotazione a partire dai gradi. | Inclinazione di Astro. |
| `Color` / `Color32` | Un colore (rosso, verde, blu, trasparenza). `Color32` usa numeri 0–255 per i pixel della webcam. | Tinte, feedback, analisi webcam. |
| `Color.Lerp(a, b, t)` | Colore intermedio fra due colori. | Lampeggio rosso di Astro, bagliore di gioia, tempo che diventa rosso. |
| `Color.RGBToHSV(...)` | Converte un colore da RGB a **tinta/saturazione/luminosità**: serve per riconoscere l'evidenziatore alla webcam anche se cambia la luce. | ComandoWebcam. |
| `Rect` + `.Contains(punto)` | Un rettangolo e il test "questo punto è dentro?". | Aree dei pulsanti (il dwell). |
| `Random.Range(min, max)` | Numero a caso (intero o decimale). | Posizioni casuali di caramelle/porta, fasi e colori delle animazioni, coriandoli. |

**Come modificarli:** sono strumenti generali. Esempi pratici:
- l'ampiezza di un'oscillazione è il numero che moltiplica `Mathf.Sin` (es.
  `* 0.05f` = oscillazione leggera della porta; alzandolo si muove di più);
- la velocità di un'oscillazione è il numero che moltiplica `Time.time` dentro
  il seno (più alto = più veloce);
- la "morbidezza" di un movimento con `Lerp` dipende dal terzo valore `t`: più
  piccolo = più lento/morbido.

---

## 6. Tempo

| Funzione | Cosa fa | Dove la uso |
|---|---|---|
| `Time.deltaTime` | Secondi passati dall'**ultimo fotogramma**. Moltiplicando per `deltaTime` i movimenti vanno uguali su PC lenti e veloci. | Tutti i timer e i movimenti. |
| `Time.unscaledDeltaTime` | Come sopra, ma **non** risente di un'eventuale pausa/rallentamento del gioco. | Conteggio FPS, barra dwell. |
| `Time.time` | Secondi totali dall'avvio. La uso come "orologio" per le onde delle animazioni. | Animazioni (pulsazioni, dondolii, lampeggi). |
| `Time.timeScale` | Velocità del tempo di gioco (1 = normale). La **leggo** soltanto, per mostrarla nel pannello F3. | InterfacciaGioco (debug). |
| `Time.realtimeSinceStartup` | Tempo reale dall'avvio, in secondi. | Pannello F3 ("tempo di gioco"). |

**Come modificarli:** per fare un conto alla rovescia faccio
`tempo = tempo - Time.deltaTime` ogni frame (vedi `GestoreGioco`). Se volessi
una pausa "vera" potrei mettere `Time.timeScale = 0`.

---

## 7. Comandi del giocatore (vecchio Input Manager)

> Il progetto usa il **vecchio** sistema di input di Unity (Input Manager),
> perché è semplice e funziona ovunque, anche su Windows del professore e in
> WebGL. NON uso il nuovo Input System (che è un pacchetto separato).

| Funzione | Cosa fa | Dove la uso |
|---|---|---|
| `Input.mousePosition` | Posizione del mouse in pixel dello schermo. | Comandi (puntatore mouse e dito), pannello F3. |
| `Input.GetAxisRaw("Horizontal"/"Vertical")` | Legge la leva del joystick / le frecce / WASD, da -1 a +1, senza smussatura. | Comandi.MuoviConJoystick. |
| `Input.GetKeyDown(KeyCode.F3 / BackQuote)` | "È stato appena premuto questo tasto?" | InterfacciaGioco (apre/chiude il pannello informazioni). |
| `Cursor.visible` | Mostra/nasconde il cursore del mouse. | InterfacciaGioco: visibile nei menù, nascosto mentre si gioca (il puntatore è Astro). |

**Come modificarli:** i nomi `"Horizontal"`/`"Vertical"` e i tasti collegati si
configurano in Unity sotto *Project Settings → Input Manager*. La velocità e la
zona morta del joystick stanno in `ParametriComandi.cs`.

---

## 8. Interfaccia a schermo (IMGUI / OnGUI)

> Tutta l'interfaccia (HUD, schermata iniziale, pannelli) è disegnata "a mano"
> dentro `OnGUI` con le funzioni `GUI`. È il sistema **IMGUI**: ho scelto questo
> perché tiene tutto nel codice, senza dover costruire Canvas o prefab in Unity.

| Funzione | Cosa fa | Dove la uso |
|---|---|---|
| `GUI.Label(rect, testo, stile)` | Scrive del testo. | Contatori, titoli, timer, pannelli. |
| `GUI.Button(rect, testo, stile)` | Disegna un pulsante e dice se è stato cliccato. | MENU, RIAVVIA, GIOCA, scelta comando. |
| `GUI.Box(rect, testo, stile)` | Riquadro con eventuale testo (lo uso per i "badge"). | Obiettivo, descrizioni. |
| `GUI.DrawTexture(rect, texture)` | Disegna un'immagine/un rettangolo in una zona. | Sfondi, barre, bordi (tramite il metodo `Riquadro`). |
| `GUI.DrawTextureWithTexCoords(...)` | Disegna una texture potendo **specchiarla**. | ComandoWebcam (anteprima specchiata). |
| `GUI.color` / `GUI.backgroundColor` | Tinta con cui vengono disegnate le cose seguenti. | `Riquadro` (rettangoli colorati), pulsante del comando scelto (verde). |
| `GUI.skin.label / .box / .button` | Gli stili di partenza da cui creo i miei. | CostruisciStili. |
| `GUIStyle` | Lo "stile" di un testo/pulsante: `.fontSize`, `.fontStyle`, `.alignment`, `.normal.textColor`, `.normal.background`, `.richText`, `.wordWrap`. | CostruisciStili (HUD e schermata iniziale). |
| `FontStyle.Bold`, `TextAnchor.*` | Grassetto e allineamento del testo. | Stili vari. |
| `Event.current` + `EventType.Repaint` | Capisce in che "fase" del disegno siamo: faccio scorrere il tempo della barra dwell una sola volta per frame. | InterfacciaGioco.BottoneAccessibile. |
| `Screen.width` / `.height` / `.fullScreen` / `.dpi` / `.currentResolution` | Dimensioni e dati della finestra: tutto l'HUD si posiziona in base a questi. | InterfacciaGioco, SchermataStart, ComandoWebcam. |

**Come modificarli:** per spostare o ridimensionare un elemento si cambiano i
numeri del `Rect` (x, y, larghezza, altezza). Per cambiare aspetto del testo si
modifica il `GUIStyle` (es. `fontSize`). Tutte le posizioni sono legate a
`Screen.width`/`Screen.height`, così l'interfaccia si adatta alla finestra.

---

## 9. Audio

| Funzione | Cosa fa | Dove la uso |
|---|---|---|
| `AudioSource` | L'"altoparlante" del gioco. Proprietà: `.playOnAwake`, `.spatialBlend` (0 = suono 2D). | FeedbackPaziente. |
| `AudioSource.PlayOneShot(clip, volume)` | Fa partire un suono, permettendo a più suoni di **sovrapporsi** senza tagliarsi. | FeedbackPaziente.Suona. |
| `AudioListener` | Le "orecchie": senza, non si sente nulla. Ne tengo esattamente una. | FeedbackPaziente.Awake. |
| `AudioClip` | Un file audio caricato in memoria. | I tre suoni: raccolta, vittoria, errore. |

**Come modificarli:** per **cambiare un suono** si mette un altro file in
`Assets/Resources` e si aggiorna il nome in `ParametriFeedback.cs`. I volumi
(`VOLUME_GIUSTO`, `VOLUME_SBAGLIATO`, `VOLUME_GENERALE`) e l'accensione del suono
stanno sempre in `ParametriFeedback.cs`.

---

## 10. Webcam

> Serve alla modalità "dito": si muove Astro muovendo un evidenziatore fluo
> davanti alla webcam. Unity dà gli strumenti per **leggere** la webcam; il
> riconoscimento del colore è codice mio (Unity non ha una funzione pronta).

| Funzione | Cosa fa | Dove la uso |
|---|---|---|
| `WebCamTexture` | L'immagine in diretta della webcam. Metodi: `new WebCamTexture(...)`, `.Play()`, `.Pause()`, `.Stop()`, `.isPlaying`, `.width/.height`, `.didUpdateThisFrame`. | ComandoWebcam. |
| `WebCamTexture.devices` | Elenco delle webcam disponibili (ne uso la prima). | ComandoWebcam, schermata iniziale (le conta). |
| `.GetPixels32(array)` | Copia tutti i pixel del fotogramma in un array, per analizzarli. | ComandoWebcam.AnalizzaFrame. |
| `Application.RequestUserAuthorization(UserAuthorization.WebCam)` + `HasUserAuthorization(...)` | Chiede e verifica il **permesso** di usare la webcam (serve su alcuni sistemi/browser). | ComandoWebcam.AvviaWebcam. |

**Come modificarla:** la risoluzione e gli FPS richiesti, la tolleranza sul
colore, i colori dell'evidenziatore e la morbidezza del movimento stanno tutti
in `ParametriWebcam.cs`.

---

## 11. Informazioni di sistema (pannelli tecnici)

Le uso solo per **mostrare** dati nel pannello F3 e nella schermata iniziale
(utili da spiegare all'esame). Non cambiano il gioco.

| Funzione | Cosa fa |
|---|---|
| `SystemInfo.operatingSystem`, `.deviceModel`, `.processorType`, `.processorCount`, `.systemMemorySize`, `.graphicsDeviceName`, `.graphicsMemorySize`, `.graphicsDeviceType` | Sistema operativo, modello del PC, CPU, numero di core, RAM, scheda video, memoria video, API grafica. |
| `Application.targetFrameRate`, `.unityVersion`, `.platform` | FPS desiderati, versione di Unity, piattaforma su cui gira. |
| `QualitySettings.vSyncCount` | Se la sincronia verticale è attiva. |
| `Screen.currentResolution.refreshRateRatio.value` | Frequenza di aggiornamento dello schermo (Hz). |
| `Debug.LogWarning("...")` | Scrive un avviso nella Console di Unity (es. se manca un file). | 

**Come modificarle:** sono di sola lettura (tranne `Debug.LogWarning`). Si
aggiungono/tolgono righe nei testi del pannello (`InterfacciaGioco.DisegnaPannelloDebug`
e `SchermataStart.TestoParametriTecnici`).

---

## 12. Cosa NON ho sostituito e perché

Alcune cose le faccio "a mano" anche se Unity avrebbe un sistema automatico. È
una scelta voluta: per un esame in cui devo dimostrare di **saper programmare**,
scrivere io la logica vale più che affidarla a un sistema pronto. Per
completezza, ecco le alternative Unity e il motivo della scelta.

| Cosa faccio a mano | Funzione/sistema Unity che potrebbe sostituirlo | Perché l'ho tenuto a mano |
|---|---|---|
| **Collisioni** (distanze e "punto dentro cerchio") | Fisica 2D: `CircleCollider2D`/`BoxCollider2D` + `Rigidbody2D` + `OnTriggerEnter2D`, oppure `Physics2D.OverlapCircle`. | Il controllo a mano è preciso, prevedibile e mostra la matematica. La fisica completa qui sarebbe sovradimensionata. |
| **Interfaccia con OnGUI** | UI moderna `uGUI` (Canvas, Button, Text/Image, Slider) o UI Toolkit. | OnGUI tiene tutta l'interfaccia nel codice, senza costruire Canvas/prefab: si legge tutto in un file. |
| **Animazioni con `Mathf.Sin`** | `Animator` + clip di animazione, o `AnimationCurve`. | Poche righe di codice danno pulsazioni e dondolii; l'Animator richiederebbe lavoro in editor e mostrerebbe meno codice. |
| **Oggetti costruiti da codice** (`new GameObject` + `AddComponent`) | **Prefab** + `Instantiate`. | Costruire da codice rende esplicito ogni passo e non dipende da file di scena. |
| **Coriandoli ed esplosione** | **Particle System** (Shuriken). | La versione a mano funziona e fa vedere come gestisco tanti oggetti e la loro durata di vita. |

### Cosa invece HO semplificato con funzioni Unity

Queste sostituzioni rendono il codice più pulito **senza** nascondere il lavoro,
quindi le ho fatte:

- **Rettangoli colorati dell'interfaccia** → prima creavo una `Texture2D` nuova
  ad **ogni fotogramma** (uno spreco di memoria); ora uso `Texture2D.whiteTexture`
  (la texture bianca già pronta di Unity) colorata con `GUI.color`, tramite un
  unico metodo `Riquadro(...)`. (InterfacciaGioco.cs, SchermataStart.cs)
- **Texture bianca della webcam** → era un'immagine 1×1 costruita a mano,
  sostituita anch'essa con `Texture2D.whiteTexture`. (ComandoWebcam.cs)
- **Distruzione a tempo** di esplosione e coriandoli → prima contavo i secondi a
  mano in `Update`; ora uso l'overload `Destroy(oggetto, tempo)` di Unity, che fa
  da solo la stessa cosa. (Oggetti.cs)

---

*Documento di riferimento per l'esame "Sistemi per la Riabilitazione e la Terapia
Assistita". Autore: Filippo Cicirelli.*
