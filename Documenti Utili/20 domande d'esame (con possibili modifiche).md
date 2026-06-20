# 20 domande d'esame (con possibili modifiche) 🎓

Qui raccolgo le domande che il professore potrebbe farmi — del tipo "a che serve
questo?" o "perché l'hai fatto così?" — ognuna con una **risposta** breve e
corretta e una **possibile modifica** collegata, già con il punto esatto dove
intervenire (file + metodo o costante). Le domande spaziano dalla visione
d'insieme (architettura, scelte) al dettaglio di un singolo metodo.

> Le guide di riferimento sono `Guida completa al progetto (dalle basi).md`
> (macro) e `Guida al codice, file per file (livello micro).md` (micro); la tabella completa delle API è in `Funzioni di Unity usate nel codice.md` e la leggenda colori in `Guida per modificare il codice/Colori del codice (oggetti, funzioni, parametri).md`.

---

#### GestoreGioco.cs

**Domanda 1 — A che serve il gioco e perché è pensato per la riabilitazione?**

*Risposta:* È un gioco dove muovo Astro (un alieno su un UFO) per raccogliere caramelle e raggiungere una porta. Serve a far esercitare un movimento controllato del braccio/mano: il paziente deve portare il puntatore in punti precisi evitando ostacoli. La logica di gioco sta in `GestoreGioco`, e la parte di riabilitazione vera la trovi nel `FeedbackPaziente`, che premia il gesto giusto con suono e con Astro che si gonfia (`Gonfia()`). Tutto è tarato per essere gentile: vite, tempo e raggi di presa sono larghi.

*Possibile modifica:* posso rendere l'esercizio più facile o più mirato alzando i "raggi di presa" in `Impostazioni.cs` (`RAGGIO_CARAMELLA = 0.85f`): a `1.2f` il paziente non deve essere preciso, utile a inizio terapia.

**Domanda 2 — Perché ogni livello ha due fasi?**

*Risposta:* In `GestoreGioco` la FASE 1 è "raccogli tutte le caramelle" e la FASE 2 è "raggiungi la porta". La gestisco con il flag `PortaApparsa`: in `SegnalaCaramellaRaccolta()` controllo se `CaramelleRaccolte >= TotaleCaramelle` e allora chiamo `CreaPorta()`, che mette `PortaApparsa = true`. Le due fasi servono ad allenare due gesti diversi: prima movimenti sparsi per prendere tante caramelle, poi un movimento mirato e lungo verso un singolo bersaglio.

*Possibile modifica:* per saltare la fase della porta (livello vinto appena raccolgo tutte le caramelle) in `SegnalaCaramellaRaccolta()` posso chiamare `SegnalaPortaRaggiunta()` invece di `CreaPorta()`: compare subito il pannello "PROSSIMO LIVELLO". (`AttivaVittoriaFinale()` invece chiuderebbe del tutto il gioco con la vittoria finale.)

#### Comandi.cs

**Domanda 3 — Come funzionano le tre modalità di comando e perché le hai separate così?**

*Risposta:* In `Comandi.cs` c'è l'enum `Modalita { Mouse, Dito, Joystick }`. Il cuore è `PuntatoreSchermo()`: uno `switch` che restituisce la posizione del puntatore a seconda della modalità (mouse → `Input.mousePosition`, dito → `ComandoWebcam.Posizione` ma solo se la webcam è `Pronta`, altrimenti torna al mouse; joystick → `posizioneJoystick`). Astro chiama solo questo metodo e non sa quale modo è attivo, quindi il resto del gioco non cambia mai. Le tre modalità servono perché pazienti diversi hanno capacità diverse.

*Possibile modifica:* per aggiungere un quarto comando (es. tastiera numerica) aggiungo un valore all'enum `Modalita` e un `case` in `PuntatoreSchermo()`; il resto del gioco non va toccato.

**Domanda 4 — Perché diversi script si "installano da soli" e come?**

*Risposta:* `Comandi`, `ComandoWebcam`, `FeedbackPaziente` e `SchermataStart` hanno un metodo statico con l'attributo `[RuntimeInitializeOnLoadMethod(...AfterSceneLoad)]`: Unity lo chiama da solo appena la scena è pronta, e lì faccio `new GameObject(...).AddComponent<...>()`. Così non devo trascinare nessun oggetto in scena e non rischio di dimenticarmene uno. È lo stesso trucco di `Avvio.Inizia()`, che crea telecamera, `GestoreGioco` e `InterfacciaGioco`.

*Possibile modifica:* se volessi che un servizio sopravviva al cambio scena, in quel metodo `Installa()` aggiungo `Object.DontDestroyOnLoad(go)` dopo aver creato il GameObject.

#### Oggetti.cs (Astro)

**Domanda 5 — Come fa Astro a seguire il puntatore?**

*Risposta:* In `Astro.Update()` chiedo a `Comandi.PuntatoreSchermo()` un punto in pixel dello schermo, gli metto la `z` giusta (`distanzaZ`) e lo converto in coordinate del mondo con `telecamera.ScreenToWorldPoint(...)`. Poi assegno quella posizione ad Astro. Non c'è inseguimento o velocità massima: Astro è esattamente dove punti, fotogramma per fotogramma. La velocità la calcolo a parte (`Velocita`) come differenza di posizione diviso `Time.deltaTime`, e mi serve solo per inclinare lo sprite.

*Possibile modifica:* per un movimento più morbido (mano che trema) posso interpolare la posizione con `Vector3.Lerp(transform.position, mouseMondo, Time.deltaTime * k)` invece di assegnarla secca.

**Domanda 6 — Perché le collisioni le fai a mano, senza la fisica di Unity?**

*Risposta:* Non uso Rigidbody né Collider: in `Astro` i metodi `ControllaCaramelle()`, `ControllaBombe()` e `ControllaPorta()` misurano la distanza con `Vector2.Distance(...)` e la confrontano con un raggio (`raggioCaramella`, ecc.). Per gli asteroidi uso `Asteroide.Contiene(punto)`, che è un `Rect.Contains`. Lo faccio così perché Astro non si muove con la fisica (è incollato al puntatore), quindi i collider darebbero problemi; e perché controllare una distanza è semplice, prevedibile e mi fa regolare la difficoltà cambiando un solo numero.

*Possibile modifica:* per rendere la presa delle caramelle più generosa alzo `RAGGIO_CARAMELLA` in `Impostazioni.cs`; il confronto in `ControllaCaramelle()` non cambia.

**Domanda 7 — Come riconosci se Astro tocca un asteroide?**

*Risposta:* L'asteroide è una roccia tonda. In `Asteroide.Inizializza()` calcolo un raggio di collisione dal diametro (`diametro * RAGGIO_ASTEROIDE_FATTORE`), e in `Contiene(punto)` controllo se la posizione di Astro è dentro il cerchio con `(punto - centro).sqrMagnitude <= raggio * raggio`. In `Astro.ControllaAsteroidi()` scorro la lista statica `Asteroide.Tutti` e, appena uno contiene la posizione di Astro, chiamo `SegnalaAsteroideToccato()` e faccio `return` (basta un asteroide per volta). Uso il cerchio perché la roccia è tonda.

*Possibile modifica:* per regolare la tolleranza cambio `RAGGIO_ASTEROIDE_FATTORE` in `Impostazioni.cs`: più alto = il cerchio "fa male" un po' prima, più basso = più permissivo.

#### ComandoWebcam.cs

**Domanda 8 — Come riconosci il colore alla webcam? Perché HSV e non RGB?**

*Risposta:* In `ColoreGiusto()` converto ogni pixel da RGB a HSV con `Color.RGBToHSV` e guardo soprattutto la TINTA (hue). Confrontare la tinta funziona anche se la luce cambia: un verde resta verde in ombra o al sole, mentre in RGB i tre numeri cambiano tutti. Scarto i pixel poco saturi (`SATURAZIONE_MINIMA = 0.55`, così la pelle non passa) e troppo scuri (`LUMINOSITA_MINIMA`). Calcolo le tinte bersaglio una volta sola in `Awake()` da `COLORI_EVIDENZIATORE`.

*Possibile modifica:* per seguire un evidenziatore arancione aggiungo il suo colore alla lista `COLORI_EVIDENZIATORE` in `ParametriWebcam.cs`; le tinte si ricalcolano da sole all'avvio.

**Domanda 9 — La tinta è un cerchio: come gestisci che 0 e 1 sono lo stesso colore (rosso)?**

*Risposta:* In `ColoreGiusto()` calcolo la differenza di tinta `dh = Mathf.Abs(h - tintaBersaglio)`, e se `dh > 0.5f` faccio `dh = 1f - dh`. Così misuro sempre la distanza "dal lato più corto" della ruota dei colori: un rosso a hue 0.98 e uno a hue 0.02 risultano vicini, non lontanissimi. Poi accetto il pixel se `dh <= TOLLERANZA_TINTA` (0.08).

*Possibile modifica:* se l'evidenziatore viene perso troppo facilmente alzo `TOLLERANZA_TINTA` a `0.12` in `ParametriWebcam.cs`; se invece cattura colori sbagliati la abbasso.

**Domanda 10 — Dove sta "il dito" nell'immagine? Come trovi il punto?**

*Risposta:* In `AnalizzaFrame()` scorro i pixel (saltandone alcuni con `PASSO_ANALISI` per andare veloce) e per ogni pixel del colore giusto sommo le coordinate `x` e `y` e conto quanti sono. Alla fine faccio la media: è il "centro di massa" dei pixel colorati, cioè dove sta l'evidenziatore. Se i pixel trovati sono almeno `PIXEL_MINIMI` (12) dico `DitoVisto = true`; altrimenti lascio il puntatore dov'era invece di spararlo a caso.

*Possibile modifica:* su PC lenti rendo l'analisi più rapida alzando `PASSO_ANALISI` a `3` in `ParametriWebcam.cs` (analizzo meno pixel, perdo un po' di precisione).

#### InterfacciaGioco.cs

**Domanda 11 — Cos'è il dwell dei pulsanti e perché serve?**

*Risposta:* In `BottoneAccessibile()` un pulsante si preme col clic del mouse OPPURE tenendoci sopra Astro per un po'. Se `AstroSopra(r)` è vero, faccio scorrere `dwellTimer` con `Time.unscaledDeltaTime` e disegno una barra verde che si riempie; quando `dwellTimer >= DWELL_SECONDI` il pulsante scatta da solo. Serve a chi gioca con webcam o joystick, che non ha il clic: deve poter premere "RIPROVA" o "MENU" solo muovendo Astro.

*Possibile modifica:* per pazienti più lenti alzo `DWELL_SECONDI` (ora `1.2f`) a `2.0f` in `ParametriComandi.cs`; il pulsante richiede di restare fermi più a lungo.

**Domanda 12 — Come fa `AstroSopra` a sapere se Astro è sul pulsante, se uno è nel mondo e l'altro nei pixel della GUI?**

*Risposta:* In `AstroSopra(r)` prendo la posizione di Astro nel mondo e la converto in pixel schermo con `cam.WorldToScreenPoint(...)`. Poi ribalto la `y` (`Screen.height - schermo.y`) perché la GUI ha la y verso il basso mentre lo schermo di Unity ce l'ha verso l'alto. Infine controllo `r.Contains(puntoGui)`. Senza il ribaltamento della y il dwell scatterebbe sul pulsante sbagliato.

*Possibile modifica:* per allargare l'area "sensibile" del pulsante posso passare a `r.Contains` un rettangolo un po' più grande (es. gonfiando `r` di qualche pixel) dentro `AstroSopra`.

**Domanda 13 — Perché disegni l'HUD con OnGUI invece dei Canvas di Unity?**

*Risposta:* In `InterfacciaGioco`, `SchermataStart` e `ComandoWebcam` disegno tutto con `OnGUI()`. Lo faccio perché è semplice: scrivo riga per riga (`GUI.Label`, `GUI.Button`) senza dover creare Canvas, prefab o ancorare oggetti nell'editor. Tutto il codice dell'interfaccia sta in un file solo e si legge in ordine. Per un progetto piccolo come questo è più chiaro da spiegare; per un gioco grande converrebbe il Canvas perché OnGUI è meno performante.

*Possibile modifica:* per ingrandire tutto l'HUD su schermi piccoli posso scalare la matrice in cima a `OnGUI` con `GUI.matrix = Matrix4x4.Scale(...)`, senza toccare i singoli rettangoli.

**Domanda 14 — A che serve il pannello F3 e come calcoli gli FPS?**

*Risposta:* Il pannello F3 (`DisegnaPannelloDebug`) è come la schermata di debug di Minecraft: lo apro con F3 (in `Update`) e mostra stato del gioco, posizione di Astro, FPS e info sul dispositivo. Gli FPS li calcolo come `1f / Time.unscaledDeltaTime` e li ammorbidisco con `fps = Mathf.Lerp(fps, fpsOra, 0.1f)` così il numero non balla. Tengo anche `fpsMinimo`. Uso `unscaledDeltaTime` per misurare il tempo reale anche se mettessi il gioco in pausa.

*Possibile modifica:* per un numero più reattivo alzo il fattore di smussamento da `0.1f` a `0.3f` nella riga `Mathf.Lerp(fps, fpsOra, ...)` in `InterfacciaGioco.Update` (la stessa riga c'è anche in `SchermataStart.Update`, per gli FPS della schermata iniziale).

**Domanda 15 — Perché crei i rettangoli colorati con `Texture2D.whiteTexture` invece di una texture nuova ogni frame?**

*Risposta:* Il metodo `Riquadro(r, colore)` imposta `GUI.color = colore` e disegna `Texture2D.whiteTexture` (la texture bianca già pronta di Unity), poi ripristina il colore. Prima creavo una `Texture2D` nuova per ogni rettangolo a ogni frame: era uno spreco di memoria che produceva garbage. Una `Texture2D` nuova la creo solo in `TexturaPiena`, una volta sola in `CostruisciStili`, perché lo sfondo di uno stile GUI vuole una texture vera e `GUI.color` non basta.

*Possibile modifica:* potrei tingere lo sfondo del flash rosso con una sfumatura cambiando il colore passato a `Riquadro` nella parte del `TimerLampeggio` in `OnGUI`.

#### Oggetti.cs (feedback visivo)

**Domanda 16 — Come funziona l'effetto "gonfia" di Astro e perché usi Mathf.Sin?**

*Risposta:* In `Astro.Update`, quando `timerGonfia > 0`, calcolo l'avanzamento `avanz` da 0 a 1 mentre il timer scende, poi `campana = Mathf.Sin(avanz * Mathf.PI)`. Il seno tra 0 e π va 0 → 1 → 0, quindi Astro cresce fino al massimo (`GONFIA_QUANTITA`) e torna alla forma normale con una curva morbida "a campana", senza scatti. Lo schiacciamento è simile ma fa Astro più largo e più basso (`1 + quanto` su X, `1 - quanto` su Y). A riposo Astro resta fermo: i fattori di scala partono da `1` e si muovono solo quando c'è un feedback.

*Possibile modifica:* per un gonfiamento più evidente alzo `GONFIA_QUANTITA` (ora `0.45`) a `0.7` in `ParametriFeedback.cs`; per renderlo più lento alzo `GONFIA_DURATA`.

**Domanda 17 — Perché l'errore è "gentile" e perché il rosso vince sul verde?**

*Risposta:* Nella riabilitazione il feedback negativo deve informare, non spaventare: per questo in `ParametriFeedback.cs` ho `VOLUME_SBAGLIATO = 0.45` contro `VOLUME_GIUSTO = 0.80`, e il suono dell'errore è basso. Inoltre in `Astro.Schiaccia()` azzero `timerGonfia` e in `AggiornaColore()` il rosso (`COLORE_ERRORE`) ha la precedenza sul bagliore verde: così non capita mai "forma sbagliata con colore giusto" e il segnale resta chiaro.

*Possibile modifica:* per un errore ancora più dolce abbasso `VOLUME_SBAGLIATO` a `0.25`, o spengo del tutto il suono con `SUONO_ATTIVO = false`, sempre in `ParametriFeedback.cs`.

#### GestoreGioco.cs / Comandi.cs (indipendenza dagli FPS)

**Domanda 18 — Perché moltiplichi tutto per `Time.deltaTime`?**

*Risposta:* Per far andare il gioco uguale su PC lenti e veloci. In `GestoreGioco.Update` scalo il tempo (`TempoRimasto -= Time.deltaTime`); il joystick in `Comandi.MuoviConJoystick` si muove di `JOYSTICK_VELOCITA * Time.deltaTime` pixel; la webcam in `AnalizzaFrame` usa `t = 1 - Mathf.Pow(morbidezza, Time.deltaTime * 60f)` apposta per rendere il `Lerp` indipendente dagli FPS. Se non moltiplicassi per `deltaTime`, su un PC a 120 fps tutto andrebbe il doppio più veloce che a 60.

*Possibile modifica:* per velocizzare il puntatore del joystick alzo `JOYSTICK_VELOCITA` (ora `1000f` px/s) in `ParametriComandi.cs`; la formula con `Time.deltaTime` resta giusta a qualsiasi frame rate.

#### Livelli.cs / Caricamento

**Domanda 19 — Come si carica un'immagine o un suono e cos'è il sortingOrder?**

*Risposta:* Carico l'immagine di sfondo con `Resources.Load<Texture2D>("sfondo")` in `CaricatoreLivelli.CostruisciSfondo()` (nome senza estensione, file in `Assets/Resources`), e i tre suoni con `Resources.Load<AudioClip>(...)` in `FeedbackPaziente.Awake()`. Se un file manca metto un `Debug.LogWarning` e il gioco va avanti lo stesso. Il `sortingOrder` decide chi sta davanti: lo sfondo ha `-10` (dietro a tutto), gli asteroidi `0`, le caramelle `2`, Astro `10` (sempre davanti). Numero più alto = più in primo piano.

*Possibile modifica:* per mettere la porta davanti alle caramelle ma dietro ad Astro cambio `sr.sortingOrder` in `GeneraPorta()` (ora `5`); per un nuovo sfondo metto un file in `Assets/Resources` e cambio il nome `"sfondo"` in `CostruisciSfondo`.

**Domanda 20 — Come si aggiunge un livello nuovo?**

*Risposta:* In `Livelli.cs`, dentro `DefinizioneLivelli`: aumento la costante `Conteggio` (ora `3`), aggiungo un `case 3:` nello `switch` di `Ottieni()` e scrivo un metodo `CostruisciLivello4()` come gli altri, dove riempio `titolo`, `obiettivo`, gli asteroidi con `NuovoAsteroide(...)`, le bombe (`l.bombe.Add`) e le caramelle (a posizioni fisse con `AggiungiCaramelle` o casuali con `caramelleCasuali`). Non devo toccare altro: `CaricatoreLivelli.Carica` costruisce tutto dai dati, e il `GestoreGioco` passa già da un livello al successivo con `ProssimoLivello()`.

*Possibile modifica:* per un livello solo-raccolta senza ostacoli basta impostare `l.caramelleCasuali = 15` e non aggiungere asteroidi né bombe, come fa `CostruisciLivello1`.

---

*Materiale di supporto per l'esame "Sistemi per la Riabilitazione e la Terapia
Assistita". Autore: Filippo Cicirelli.*
