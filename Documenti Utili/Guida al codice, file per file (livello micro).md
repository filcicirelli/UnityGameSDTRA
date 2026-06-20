# Guida al codice, file per file (livello micro) 🔬

Questo è il livello "micro" della guida: spiego il codice **file per file e
metodo per metodo**, con le tecniche vere (la matematica delle animazioni, gli
algoritmi, i piccoli trucchi) e il perché di ogni scelta. Per la **visione
d'insieme** e i **concetti di base** — a cosa serve il gioco, com'è diviso, il
flusso di una partita, e cosa sono il *fotogramma* e `Time.deltaTime` — vedi
`Guida completa al progetto (dalle basi).md`.

Tutto il codice sta in `Assets/Codice/`. I numeri regolabili sono raccolti nelle
pagine dei parametri (`Impostazioni.cs`, `ParametriFeedback.cs`,
`ParametriComandi.cs`, `ParametriWebcam.cs`); per sapere *dove cambiare cosa* vedi
`Guida per modificare il codice/Mappa del codice (dove cambiare cosa).md`. La **tabella completa delle funzioni di Unity** è in `Funzioni di Unity usate nel codice.md`; la **leggenda dei colori** in `Guida per modificare il codice/Colori del codice (oggetti, funzioni, parametri).md`.

---

### Il cervello e l'avvio — GestoreGioco.cs

Questo file contiene due classi nello stesso `.cs`: `Avvio`, che monta la scena quando premo Play, e `GestoreGioco`, che è il cervello vero e proprio. Le spiego separate.

#### Avvio

È una classe `static`: non la istanzio mai, esiste solo per il suo metodo `Inizia`. Lo marco con `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]`: questo attributo dice a Unity di chiamarlo da solo appena la scena è caricata, senza che io debba trascinare niente nell'editor. Mi piace così perché tutto il gioco nasce da codice, la scena è vuota.

Cosa fa, in ordine.

1. **Pulisce la scena.** Con `Object.FindObjectsByType<Camera>()` trovo tutte le telecamere già presenti e le distruggo una a una con `Object.Destroy`; faccio la stessa cosa con le `Light`. Lo faccio perché Unity di default può lasciare una telecamera o una luce, e io voglio partire da zero con la mia.

2. **Crea la telecamera 2D.** Costruisco un `GameObject` chiamato `"TelecameraPrincipale"`, gli metto il tag `"MainCamera"` (così Unity la riconosce come telecamera principale), poi ci aggancio un componente `Camera`. La imposto `orthographic = true`: in 2D non voglio la prospettiva, voglio che gli oggetti vicini e lontani abbiano la stessa scala. Il campo visivo lo fisso con `orthographicSize = 6f` (è il mezzo-altezza in unità di mondo), lo sfondo lo metto `Color.black` e la posiziono in `(0, 0, -10)`: la tiro indietro sull'asse Z per inquadrare tutto quello che disegno sul piano Z=0. Il nero in realtà non si vede quasi mai, perché lo sfondo vero è il wallpaper disegnato altrove; questo colore è solo una rete di sicurezza se l'immagine non venisse caricata.

3. **Crea i due oggetti che fanno girare tutto:** un `GameObject` con sopra il componente `GestoreGioco` e uno con `InterfacciaGioco`. Da qui in poi se ne occupano loro.

#### GestoreGioco

È un `MonoBehaviour`: lo posso attaccare a un `GameObject` e Unity gli chiama `Awake`, `Start` e `Update` da solo.

##### Il riferimento `Istanza` e lo stato

In `Awake` faccio `Istanza = this`. È una variabile `static`, quindi qualunque altro script può scrivere `GestoreGioco.Istanza` per parlare con il cervello senza doverselo passare in giro. È il classico singleton, tenuto al minimo.

Le variabili di stato sono tutte semplici e dicono il loro nome:
- `MenuInizialeAperto`: `true` all'avvio, diventa `false` quando premo GIOCA. Mentre è `true` il gioco è congelato.
- partita: `LivelloCorrente`, `CaramelleRaccolte`, `Punteggio`, `TotaleCaramelle`, e i tre bool `MissioneCompletata`, `PartitaFinita`, `VittoriaFinale` che mi dicono in che fase sono.
- `Vite` (le vite rimaste) e `TempoRimasto` (un `float`, è un timer).
- `MotivoSconfitta`: una stringa con il perché ho perso, la mostro nel Game Over.
- `PortaApparsa`: diventa `true` quando, raccolte tutte le caramelle, compare la porta da raggiungere.
- `TimerLampeggio`: per il lampeggio rosso quando prendo un colpo.
- `TempoIniziale`: il conto alla rovescia "PRONTI..." iniziale, in cui le bombe non fanno male.
- `cooldownDanno`: è `private`, serve a non farmi perdere tutte le vite di colpo se resto incollato a un asteroide.
- `LivelloAttuale`: i dati del livello in corso (di tipo `DatiLivello`).

##### Le proprietà calcolate

Sono tre `get` che non memorizzano niente, calcolano al volo:

| Proprietà | Cosa restituisce |
|---|---|
| `BombeAttive` | `true` solo quando `TempoIniziale <= 0`: cioè finito il "PRONTI...", da lì le bombe fanno male. |
| `Energia` | la frazione di caramelle raccolte, `CaramelleRaccolte / TotaleCaramelle`. La uso per riempire la barra "energia" dell'HUD. Controllo prima `TotaleCaramelle == 0` e ritorno `0` per non dividere per zero. |
| `TestoObiettivo` | la scritta dell'obiettivo: se la porta è apparsa ma non l'ho ancora raggiunta restituisco `"RAGGIUNGI LA PORTA"`, altrimenti l'`obiettivo` scritto in `LivelloAttuale`, e stringa vuota se non c'è ancora un livello. |

Faccio il cast esplicito a `(float)` nei due membri della divisione di `Energia`: senza, dividerei due interi e otterrei sempre 0 o 1.

##### `Start`: apro il menu

Non faccio partire subito la partita. Metto `MenuInizialeAperto = true` e chiamo `CaricatoreLivelli.MostraSoloSfondo()`, così dietro alla schermata di scelta del comando (mouse / dito / joystick) si vede già lo sfondo, ma niente caramelle o nemici.

##### `IniziaPartita` e `TornaAlMenuIniziale`

`IniziaPartita` la chiama la schermata di start quando premo GIOCA. Riceve la `Comandi.Modalita` scelta e la registra con `Comandi.Imposta(modalita)`, poi chiude il menu (`MenuInizialeAperto = false`), azzera il `Punteggio` e fa `CaricaLivello(0)` per partire dal primo livello.

`TornaAlMenuIniziale` fa il contrario, la uso dalla pausa per cambiare comando: rimette `MenuInizialeAperto = true` e richiama `MostraSoloSfondo()` per togliere la missione lasciando lo sfondo.

##### `Update`: i timer e il "livello vivo"

Prima riga: se `MenuInizialeAperto` faccio `return` subito, durante la schermata iniziale non deve scorrere nessun tempo.

Poi scalo i timer che sono ancora positivi sottraendo `Time.deltaTime` (i secondi passati dal fotogramma prima): `cooldownDanno`, `TimerLampeggio` e `TempoIniziale`. Uso `Time.deltaTime` proprio perché così il conteggio va alla stessa velocità a qualunque frame rate.

Il pezzo importante è la condizione `livelloVivo`:

`!PartitaFinita && !MissioneCompletata && !VittoriaFinale && TempoIniziale <= 0f`

cioè il livello scorre solo se non ho perso, non ho ancora finito, non è la vittoria finale e il "PRONTI..." è terminato. Solo allora scalo `TempoRimasto`; quando arriva a `0` lo blocco a `0` e chiamo `TempoScaduto()`.

##### `CaricaLivello`: gli azzeramenti

È il metodo che resetta tutto per un nuovo livello. Prima metto in sicurezza l'indice con `Mathf.Clamp(indice, 0, DefinizioneLivelli.Conteggio - 1)`, così non posso mai chiedere un livello che non esiste, poi prendo i dati con `DefinizioneLivelli.Ottieni`. Quindi azzero tutto a mano: caramelle a 0, i tre bool di fine partita a `false`, `MotivoSconfitta` vuoto, `PortaApparsa` false, i timer di lampeggio e cooldown a 0. I valori "pieni" li ricarico dalle costanti centralizzate in `Impostazioni`: `TempoIniziale = TEMPO_PRONTI`, `Vite = VITE`, `TempoRimasto = TEMPO_LIVELLO`. Infine `CaricatoreLivelli.Carica(LivelloAttuale)` crea fisicamente gli oggetti e mi restituisce quante caramelle ci sono, che salvo in `TotaleCaramelle`.

##### Navigazione tra livelli

- `ProssimoLivelloDisponibile` è una proprietà `get` che è `true` se `LivelloCorrente + 1` è ancora dentro `DefinizioneLivelli.Conteggio`.
- `ProssimoLivello`: se ce n'è un altro carica `LivelloCorrente + 1`, altrimenti chiama `RicominciaTutto`.
- `RicominciaTutto`: rimette il `Punteggio` a 0 e `CaricaLivello(0)`.
- `RipetiLivello`: ricarica lo stesso `LivelloCorrente` (lo uso dopo un Game Over per riprovare).

##### I metodi `Segnala*`: gli eventi dagli altri script

Sono i metodi che gli altri script chiamano quando succede qualcosa. Tutti partono con una guardia che blocca l'evento se la partita è già finita o vinta, così non conto cose dopo la fine.

- **`SegnalaCaramellaRaccolta`**: aumenta `CaramelleRaccolte` e `Punteggio` (di `PUNTI_CARAMELLA`), dà il feedback positivo con `FeedbackPaziente.CaramellaPresa()` — suono allegro e Astro che si gonfia. Poi controlla se ho raccolto tutto (`CaramelleRaccolte >= TotaleCaramelle`): se era l'ultimo livello chiama `AttivaVittoriaFinale()`, sennò `CreaPorta()`.
- **`SegnalaPortaRaggiunta`**: segna `MissioneCompletata = true`, fa la fanfara con `FeedbackPaziente.MissioneCompiuta()` e lancia gli effetti di festa, `GeneraPianetaAmico()` e `GeneraCoriandoli()`.
- **`SegnalaAsteroideToccato`** e **`SegnalaBombaColpita`**: prima controllano `cooldownDanno > 0`, e se sì escono — è la finestra di un attimo dopo aver preso un colpo in cui non posso essere colpito di nuovo. Altrimenti armano il cooldown (`COOLDOWN_DANNO`), accendono `TimerLampeggio = 0.30f`, danno il feedback d'errore `FeedbackPaziente.AzioneSbagliata()` (suono basso e gentile, Astro che si schiaccia) e tolgono una vita con `PerdiUnaVita`. L'asteroide toglie anche punti, `Mathf.Max(0, Punteggio - PUNTI_PERSI_HIT)` — uso `Mathf.Max` per non andare mai sotto zero.

Il feedback al paziente, scelto apposta su due canali (orecchio e occhio) e con l'errore più gentile del premio, è documentato a parte; qui mi limito a chiamare la riga giusta per ogni evento.

##### I metodi interni

- **`TempoScaduto`**: dà il feedback d'errore e chiama `PerdiUnaVita("Tempo scaduto!")`. Se dopo aver perso la vita la partita non è finita, faccio ripartire il timer con `TempoRimasto = TEMPO_LIVELLO`: il tempo scaduto costa una vita ma mi dà un'altra possibilità finché ne ho.
- **`PerdiUnaVita`**: scala le vite con `Mathf.Max(0, Vite - 1)` (mai sotto zero) e, se sono arrivato a `0`, mette `PartitaFinita = true` e salva il `motivo` ricevuto in `MotivoSconfitta`.
- **`CreaPorta`**: mette `PortaApparsa = true` e chiama `CaricatoreLivelli.GeneraPorta()` per farla comparire in scena.
- **`AttivaVittoriaFinale`**: mette `VittoriaFinale = true`, suona la fanfara con `MissioneCompiuta()` e lancia coriandoli e pianeta amico. È il finale che scatta quando finisco le caramelle dell'ultimo livello.

---

### Il personaggio — la classe Astro (in Oggetti.cs)

`Astro` è il personaggio del giocatore: un alieno su un UFO che insegue il puntatore. È un `MonoBehaviour` e tiene un riferimento statico a sé stessa in `Istanza`, così gli altri oggetti (le caramelle, la porta, le bombe) possono parlarle senza doverla cercare in scena. Una cosa importante: qui non uso la fisica di Unity (niente Rigidbody, niente collider). Le collisioni le calcolo io a mano con le distanze, perché Astro deve stare *esattamente* dove punta il paziente, e con la fisica avrei avuto rimbalzi e ritardi che non voglio.

#### Awake — preparazione

In `Awake` faccio quattro cose, una volta sola:

- `Istanza = this`: registro me stessa come istanza globale.
- `telecamera = Camera.main`: mi salvo la telecamera, mi serve a ogni frame per convertire i pixel dello schermo in coordinate del mondo.
- `distanzaZ = Mathf.Abs(telecamera.transform.position.z)`: salvo quanto è lontana la telecamera lungo Z. Il gioco è 2D ma `ScreenToWorldPoint` lavora in 3D, quindi devo dirgli a che profondità voglio il punto; uso il valore assoluto perché la telecamera sta a Z negativa.
- `scalaBase = transform.localScale`: memorizzo la scala iniziale di Astro. Tutte le animazioni di gonfiamento e schiacciamento partono da qui, moltiplicando questa scala per dei fattori — così se domani cambio la dimensione di Astro nella scena, le animazioni restano proporzionate.

Poi prendo lo `SpriteRenderer` con `GetComponent` e, se c'è, salvo il suo colore in `coloreBase`. Faccio il controllo `if (sr != null)` perché se manca lo sprite non voglio che il gioco vada in errore: semplicemente non farò i cambi di colore. In `OnDestroy` pulisco `Istanza` (la rimetto a null solo se ero davvero io quella registrata), così non resta un riferimento morto.

#### Update — ordine delle operazioni

`Update` gira ogni fotogramma. La prima riga utile è la guardia `if (gm == null) return`: se il `GestoreGioco` non c'è ancora, non faccio niente. Poi seguo questo ordine: muovo Astro, calcolo la velocità, applico le animazioni, controllo le interazioni, aggiorno il colore.

**1) Seguire il puntatore.** Prima salvo `posPrecedente = transform.position` (mi servirà per la velocità). Poi chiedo dove sta puntando il giocatore con `Comandi.PuntatoreSchermo()`: è quella funzione che astrae il comando scelto nella schermata iniziale (mouse, dito o joystick) e mi restituisce un punto in pixel. A quel punto gli imposto `z = distanzaZ` e lo passo a `telecamera.ScreenToWorldPoint(...)` per ottenere le coordinate nel mondo. Infine scrivo `transform.position` forzando `z = 0f`: Astro vive sul piano del gioco, la Z mi interessava solo per la conversione. Il risultato è che Astro è *incollato* al puntatore, non lo insegue con un ritardo: è una scelta voluta per la riabilitazione, il paziente deve sentire che comanda lui.

**2) Velocità.** Calcolo `Velocita = (posizione_nuova − posizione_vecchia) / dt`. Per `dt` uso `Mathf.Max(Time.deltaTime, 0.000001f)`: divido per il tempo trascorso così la velocità è in unità al secondo e non dipende dal frame rate, e quel valore minimo serve solo a non dividere mai per zero. La velocità non la uso per muovere Astro (lo muove già il puntatore), mi serve solo per inclinare lo sprite, vedi più sotto.

**3) Animazioni.** Tutte le animazioni lavorano su due fattori, `fattoreX` e `fattoreY`, che alla fine moltiplicano `scalaBase`.

- *A riposo*: inizializzo sia `fattoreX` sia `fattoreY` a `1`. Astro da fermo resta fermo: si deforma solo quando c'è un feedback (gonfiamento o schiacciamento).

- *Gonfiamento (azione giusta)*: quando `timerGonfia > 0` lo decremento di `Time.deltaTime` e calcolo `avanz`, cioè quanto sono avanti nell'animazione da 0 a 1, come `1 − Clamp01(timerGonfia / GONFIA_DURATA)`. Il trucco è la curva *a campana* `campana = Mathf.Sin(avanz * Mathf.PI)`: quando `avanz` va da 0 a 1, l'argomento del seno va da 0 a π, quindi il seno fa 0 → 1 → 0. Così Astro cresce e torna da solo, in modo morbido, senza scatti né un salto finale. Poi `gonfia = 1 + GONFIA_QUANTITA * campana` e moltiplico entrambi i fattori per `gonfia` (gonfiamento uniforme, cresce in tutte le direzioni).

- *Schiacciamento (azione sbagliata)*: quando `timerSchiaccia > 0` calcolo `quanto = SCHIACCIA_QUANTITA * Clamp01(timerSchiaccia / SCHIACCIA_DURATA)`. Qui la forma è diversa, voglio l'effetto *squash*: moltiplico `fattoreX` per `(1 + quanto)` e `fattoreY` per `(1 − quanto)`, cioè Astro si fa più largo e più basso allo stesso tempo. Da notare che qui non uso la campana: `quanto` parte massimo (timer pieno) e cala verso zero col timer, quindi lo schiacciamento è forte subito e si rilassa — è la sensazione giusta per un "colpo" ricevuto.

  Alla fine applico tutto in un colpo solo: `transform.localScale = scalaBase * (fattoreX, fattoreY, 1)`. Gonfiamento e respiro si sommano in modo naturale perché sono fattori moltiplicativi sullo stesso fattore.

- *Inclinazione*: calcolo un'inclinazione obiettivo dalla velocità orizzontale, `inclinObiettivo = Clamp(−Velocita.x * 3f, −18f, 18f)`. Il segno meno fa inclinare l'UFO *contro* la direzione del moto (come una moto in curva); il `Clamp` a ±18 gradi evita rotazioni esagerate. Ma non applico subito questo valore: faccio `inclinazione = Mathf.Lerp(inclinazione, inclinObiettivo, Time.deltaTime * 8f)`, cioè ammorbidisco l'inclinazione verso l'obiettivo invece di scattare. È il classico smorzamento esponenziale con `Lerp` nel tempo: il movimento risulta fluido anche se il mouse va a scatti. Poi imposto la rotazione su Z con `Quaternion.Euler(0, 0, inclinazione)`.

#### Controlli di contatto

Eseguo i controlli solo se il gioco è "vivo", cioè `giocoVivo = !MissioneCompletata && !PartitaFinita && !VittoriaFinale`: a partita finita Astro non deve più prendere caramelle né colpi. Tutti questi controlli sono geometrici, fatti da me, niente fisica.

| Metodo | Cosa controllo | Tecnica |
|---|---|---|
| `ControllaCaramelle` | Astro vicino a una caramella | `Vector2.Distance(pos, caramella) <= raggioCaramella` |
| `ControllaAsteroidi` | Astro dentro la barriera | `a.Contiene(pos)`, cioè `Rect.Contains` |
| `ControllaBombe` | Astro nell'alone di una bomba | distanza `<= raggioBomba + b.raggioPericolo` |
| `ControllaPorta` | Astro arriva alla porta | `Vector2.Distance(...) <= raggioPorta` |

- `ControllaCaramelle`: scorro la lista `Caramella.Attive` **all'indietro** (`for (int i = Count-1; i >= 0; i--)`). Lo faccio apposta perché `Raccogli()` distrugge la caramella e quindi modifica la lista: andando dall'ultima alla prima, togliere un elemento non sballa gli indici di quelli che devo ancora controllare. Salto i `null` per sicurezza e, se la distanza è entro `raggioCaramella`, chiamo `c.Raccogli()`.

- `ControllaAsteroidi`: gli asteroidi sono rocce tonde, quindi uso `a.Contiene(pos)`, che dentro controlla se la posizione di Astro è **dentro il cerchio** della roccia (centro e raggio). Appena ne tocco uno chiamo `SegnalaAsteroideToccato()`, accendo `timerLampeggio = LAMPEGGIO_ASTRO_DURATA` e faccio `return`: basta un asteroide per frame, non ha senso contarne due insieme.

- `ControllaBombe`: lo eseguo solo se `gm.BombeAttive`. Per ogni bomba sommo il mio `raggioBomba` al suo `raggioPericolo`: così la zona di pericolo è la somma dei due raggi, ed è la bomba stessa a dire quanto è "larga" la sua zona. Se sono dentro, `b.Detona()` e `return`.

- `ControllaPorta`: lo faccio solo se `Porta.Istanza != null`, cioè dopo che ho raccolto tutte le caramelle e la porta è comparsa. Se sono entro `raggioPorta`, chiamo `p.Sblocca()`.

I tre raggi (`raggioCaramella`, `raggioPorta`, `raggioBomba`) non sono numeri sparsi nel codice: li inizializzo dalle costanti `Impostazioni.RAGGIO_*`, così la difficoltà si regola da un solo posto.

#### AggiornaColore — la priorità del rosso

`AggiornaColore` decide il colore di Astro a ogni frame, ed è una catena `if / else if / else` con una priorità precisa:

1. **Rosso (errore)** per primo: se `timerLampeggio > 0`, lo decremento e calcolo `k = Clamp01(timerLampeggio / LAMPEGGIO_ASTRO_DURATA)`, poi `sr.color = Color.Lerp(coloreBase, COLORE_ERRORE, k)`. Con `k` che parte da 1 e cala, il rosso è acceso subito dopo il colpo e svanisce verso il colore normale.
2. **Oro/gioia** solo *altrimenti*: se `timerGonfia > 0`, uso la stessa campana `Mathf.Sin(avanz * Mathf.PI)` (moltiplicata per `GIOIA_INTENSITA`) per far brillare Astro in `COLORE_GIOIA` in sincrono col gonfiamento.
3. **Colore base** se non c'è né errore né gioia.

L'ordine è la cosa importante: il rosso ha la precedenza sul bagliore positivo. Non voglio che, per un caso di timer sovrapposti, Astro mostri il colore "giusto" mentre ha appena preso un colpo: l'errore deve sempre vincere visivamente.

#### Gonfia() e Schiaccia() — i comandi del feedback

Questi due metodi sono pubblici e li chiama il sistema di feedback (`FeedbackPaziente.cs`) quando il paziente fa bene o male. Sono semplici, ma curo un dettaglio: si annullano a vicenda.

- `Gonfia()`: imposta `timerGonfia = GONFIA_DURATA` e azzera sia `timerSchiaccia` sia `timerLampeggio`.
- `Schiaccia()`: imposta `timerSchiaccia` *e* `timerLampeggio` a `SCHIACCIA_DURATA` (così il rosso dura esattamente quanto lo schiacciamento) e azzera `timerGonfia`.

Azzero sempre il feedback opposto perché il segnale al paziente deve restare pulito: non deve mai capitare "forma sbagliata con colore giusto" o un gonfiamento che continua mentre Astro ha appena sbagliato. Un evento nuovo cancella il precedente, e il messaggio resta uno solo.

---

### Gli altri oggetti ed effetti (in Oggetti.cs)

In `Oggetti.cs` tengo, oltre ad Astro, tutte le altre cose che si muovono o con cui Astro interagisce: gli oggetti veri (caramelle, porta, bombe, asteroidi) e gli effetti puramente decorativi (esplosione, pianeta amico, coriandoli). Quasi tutte le animazioni le faccio "a mano" con un seno o un coseno: niente sistema di particelle, niente fisica di Unity, solo un po' di trigonometria in `Update`. Qui spiego classe per classe cosa fa e con che tecnica.

#### Caramella

È la stellina da raccogliere passando vicino con Astro.

- **La lista statica `Attive`**: ogni caramella, quando si accende, si aggiunge da sola alla lista `Caramella.Attive` in `OnEnable` e si toglie in `OnDisable`. Così Astro non deve cercarle nella scena: in `ControllaCaramelle` scorre direttamente questa lista. La scorro **all'indietro** (`for` da `Attive.Count - 1` a `0`) apposta, perché `Raccogli()` fa `Destroy` e quindi modifica la lista mentre la sto leggendo: partendo dal fondo, togliere un elemento non sballa gli indici di quelli che devo ancora controllare.
- **Sta ferma**: la caramella non si muove. In `Inizializza` le do solo la posizione e resta lì; non ha un `Update`. Così il campo di gioco è calmo e la stellina è più facile da puntare (utile in riabilitazione).
- **`Raccogli`**: la chiama Astro quando la distanza scende sotto `raggioCaramella`. La prima riga è una guardia `if (raccolta) return` con il flag `raccolta`, così non scatta due volte nello stesso frame. Poi avviso il `GestoreGioco` con `SegnalaCaramellaRaccolta()` (è lui che fa partire suono e gonfiamento di Astro) e infine `Destroy(gameObject)`.

#### Porta

Compare quando ho raccolto tutte le caramelle; per finire il livello basta raggiungerla.

- **`Istanza` statica**: tengo un riferimento singolo perché di porte ce n'è una sola alla volta. La imposto in `Awake` e la rimetto a `null` in `OnDestroy` (controllando `if (Istanza == this)`, per non azzerare per sbaglio una porta nuova). Astro controlla `Porta.Istanza != null` per sapere se siamo nella fase finale.
- **Wobble**: la porta sta ferma (non si muove, basta raggiungerla), però la faccio "respirare" con `wobble = 1 + Sin(Time.time * 3f) * 0.05f` applicato alla scala base `1.8`. È solo un dettaglio per renderla viva e attirare l'occhio.
- **`Sblocca`**: come per la caramella, parte con la guardia `if (aperta) return` sul flag `aperta`, poi chiama `SegnalaPortaRaggiunta()`. Da quel punto in poi `Update` esce subito (`if (aperta) return`), così la porta smette di animarsi.

#### Bomba

È l'oggetto da non toccare. La rendo riconoscibile con un alone rosso che pulsa.

- **Lista `Tutte`**: stessa idea della caramella, si registra in `OnEnable` e si toglie in `OnDisable`; Astro la scorre in `ControllaBombe`.
- **`raggioPericolo`**: oltre allo sprite, la bomba ha una zona di pericolo di `0.5`. Astro la considera toccata quando la distanza è `<= raggioBomba + b.raggioPericolo`: sommo i due raggi così è più "perdonante" e non serve centrare il pixel esatto.
- **L'alone rosso pulsante**: in `Inizializza` creo a runtime un `GameObject` figlio chiamato "Alone", gli attacco uno `SpriteRenderer` con un quadrato pieno rosso semitrasparente (`FabbricaImmagini.CreaQuadratoPieno`) e lo metto a scala `SCALA_ALONE = 2.4` (è una costante). Gli do `sortingOrder = ordine - 1`, cioè un livello dietro la bomba, così l'alone fa da sfondo e non la copre. In `Update` lo faccio pulsare su **due** parametri insieme: la scala con `pulse = 0.85 + Sin(Time.time * 5 + fase) * 0.25`, e la trasparenza con `col.a = 0.20 + 0.20 * Sin(...)`. Pulsare scala e alpha insieme dà l'effetto "battito" molto più di quanto farebbe la sola dimensione. Anche qui la `fase` casuale evita che tutte le bombe pulsino in sincrono. La bomba stessa fa pure un piccolo movimento ovale come la caramella (seno e coseno con frequenze diverse).
- **`Detona`**: guardia `if (esplosa) return`, poi chiama `CaricatoreLivelli.GeneraEsplosione` sulla sua posizione (l'anello che vedremo tra poco), avvisa il gestore con `SegnalaBombaColpita()` e si distrugge.

#### Asteroide

È la barriera che fa male. La cosa interessante è che la collisione la calcolo **io**, senza la fisica di Unity.

- **Il cerchio "a mano"**: in `Inizializza` ricevo `centro` e `diametro` e calcolo il raggio di collisione come `diametro * RAGGIO_ASTEROIDE_FATTORE` (il fattore sta in `Impostazioni`). Tengo il raggio un po' **più piccolo** della roccia disegnata, così i "quasi tocchi" non tolgono una vita: in riabilitazione è meglio essere indulgenti. La grandezza che si **vede** la dà lo `SpriteRenderer` con `localScale` (in `Livelli.cs`), mentre questo cerchio serve **solo** per la collisione: tengo le due cose separate apposta.
- **Il test `Contiene`**: confronto la distanza al quadrato dal centro col raggio al quadrato, `(punto - centro).sqrMagnitude <= raggio * raggio`. Uso i quadrati per evitare la radice quadrata di `Distance`: stesso risultato, un filo più veloce. Astro in `ControllaAsteroidi` mi passa la sua posizione e io dico se è dentro il cerchio. Per le caramelle e le bombe uso la distanza in modo simile; per l'asteroide il cerchio è la forma giusta perché la roccia è tonda.

#### Esplosione

È l'anello luminoso che spunta quando una bomba viene toccata.

- **Autodistruzione a tempo**: in `Start` faccio partire `Destroy(gameObject, durata)` con `durata = 0.7`. Uso l'overload di Unity `Destroy(oggetto, tempo)` che distrugge da solo dopo N secondi: prima me lo contavo a mano in `Update` con un timer, questa versione è più pulita e una riga in meno.
- **L'anello che cresce e svanisce**: parto da scala `0.2` e in `Update` calcolo l'avanzamento `k = eta / durata` (da `0` a `1`, con `Clamp01`). Per la crescita uso `velocita = 1 - (1 - k)²`: è una curva ease-out, cioè cresce in fretta all'inizio e rallenta verso la fine, che è esattamente come si comporta un'onda d'urto. Con quel valore faccio `Lerp(0.2, scalaMax, velocita)` (con `scalaMax = 4.5`). In parallelo dissolvo l'anello mettendo l'alpha a `1 - k`, così a scala massima è ormai quasi invisibile. Crescere e sparire insieme è quello che vende l'idea dell'esplosione.

#### PianetaAmico

È il pianeta sorridente che appare a fine missione. Voglio che entri con un effetto "pop", non che appaia di colpo.

- **Il pop-in**: in `Awake` lo metto a scala `0` (invisibile). Poi in `Update`, per il primo mezzo secondo (`if (vita < 0.5f)`), normalizzo il tempo in `t = vita / 0.5` e calcolo `scala = scalaFinale * rimbalzo * t`, dove `rimbalzo = 1 + 0.2 * Sin(t * π)`. Il `t` lineare lo fa crescere da zero alla dimensione piena; il termine `Sin(t * π)` vale `0` agli estremi e `1` a metà, quindi aggiunge un picco del 20% a metà animazione e poi rientra: è quello che dà la sensazione di "rimbalzo" tipica del pop-in. Passati i `0.5` secondi passo a un respiro tranquillo, `scalaFinale + 0.1 * Sin(Time.time * 3)`. Aggiungo anche un dondolio orizzontale (`dx = Sin(Time.time * 1.5) * 0.15`) come se salutasse.

#### Coriandoli e PezzoCoriandolo

Sono la pioggia di coriandoli che festeggia la missione completata. Ho diviso il lavoro in due classi: una che li **crea in massa** e una che gestisce il **singolo** pezzo.

- **`Coriandoli` (creazione in massa)**: in `Start` faccio un ciclo che chiama `CreaUno()` per `quantita = 80` volte, generando tutti i coriandoli in un colpo solo. Ogni `CreaUno` costruisce un `GameObject` figlio, lo posiziona in alto (`y = 6.5`) con una `x` casuale tra `-8` e `8`, gli mette uno `SpriteRenderer` con la stellina (`FabbricaImmagini.CreaCoriandolo`) tinta di un colore pescato a caso dall'array `COLORI` (cinque tinte vivaci). La scala è casuale e schiacciata (`scala * 0.6` sulla Y) per dare l'idea di un pezzetto di carta, non di un quadrato. Poi attacca un `PezzoCoriandolo` e gli passa velocità, rotazione e durata. Alla fine `Coriandoli` si autodistrugge con `Destroy(gameObject, durata + 1.5f)`: l'`1.5` in più è un margine per essere sicuro che tutti i pezzi siano spariti prima di togliere il contenitore.
- **`PezzoCoriandolo` (il singolo)**: ognuno parte con una velocità casuale (orizzontale tra `-1.5` e `1.5`, verticale verso il basso) e una `velocitaAngolare` casuale tra `-360` e `360` gradi al secondo. In `Update` applico una **gravità leggera** abbassando `velocita.y` di `1.2 * Time.deltaTime` a ogni frame: è una vera integrazione, l'accelerazione modifica la velocità che modifica la posizione, così i coriandoli accelerano cadendo invece di scendere a velocità fissa. Aggiungo un piccolo ondeggio orizzontale con un seno (ampiezza minima, `0.01`) perché la carta in aria non cade dritta. La **rotazione** la faccio con `transform.Rotate(0, 0, velocitaAngolare * Time.deltaTime)`, e ogni pezzo ha la sua `fase` casuale così l'ondeggio non è sincronizzato. Anche qui l'**autodistruzione a tempo** è l'overload `Destroy(gameObject, vita)` chiamato in `Start`: con 80 pezzi è importante che si ripuliscano da soli e non restino in scena.

Una nota trasversale: il trucco che uso ovunque è la **fase casuale** (`Random.Range(0f, Mathf.PI * 2f)`) aggiunta dentro i seni. Le animazioni sono deterministiche e identiche, ma sfasandole ogni oggetto sembra avere vita propria e l'insieme non appare "a macchina".

---

### I livelli — dati e costruzione (Livelli.cs)

Questo file è il punto centrale dei livelli e l'ho diviso in due parti che non si mescolano mai: una scrive solo i **dati** (i numeri di ogni livello), l'altra prende quei dati e **costruisce davvero gli oggetti in scena**. Tengo separate le due cose apposta: se voglio cambiare dove va una caramella tocco i dati, se voglio cambiare *come* viene disegnata tocco il caricatore. In fondo ci sono tre classette che sono solo contenitori di valori.

#### DefinizioneLivelli (i dati)

È una `static class`: non la istanzio mai, le chiedo solo i dati. La costante `Conteggio = 3` dice quanti livelli ci sono, e il commento accanto ricorda di aggiornarla se ne aggiungo o tolgo uno.

Il metodo `Ottieni(int indice)` è uno `switch`: a `0` risponde con `CostruisciLivello1()`, a `1` con il `2`, a `2` con il `3`, e a un indice fuori range torna `null`. È volutamente banale: chi chiama passa un numero e riceve il `DatiLivello` giusto, senza sapere com'è fatto dentro.

I tre metodi `CostruisciLivelloN()` riempiono un `DatiLivello` e me lo restituiscono. La differenza tra i livelli sta tutta qui:

- **`CostruisciLivello1`** è il più scarno: imposto `titolo`, `obiettivo` e una sola cosa importante, `l.caramelleCasuali = Impostazioni.CARAMELLE_LIV1`. Niente asteroidi, niente bombe: spazio aperto. Quel campo `caramelleCasuali > 0` è il segnale che dirà al caricatore di piazzare le stelline **a caso**. L'obiettivo lo compongo concatenando il numero preso da `Impostazioni`, così testo e gioco restano coerenti se cambio quel valore.
- **`CostruisciLivello2`** aggiunge sei rocce tonde con `l.asteroidi.Add(NuovoAsteroide(...))`, sparse nel campo e di diametri diversi, tutte **dentro lo schermo** (così nessuna viene tagliata). Le caramelle qui **non** sono casuali: le metto in un array di dieci `Vector2` infilati nei "vuoti" tra le rocce e li passo a `AggiungiCaramelle`.
- **`CostruisciLivello3`** ha sei rocce tonde con una disposizione **diversa** dal livello 2 (lascia dei "vicoli" liberi) e in più aggiunge quattro bombe in `l.bombe`, messe lontane dal centro `(0,0)` dove parte Astro. Anche qui le dieci caramelle hanno posizioni fisse, scelte nei vicoli tra rocce e bombe.

Gli aiutanti servono a non ripetere codice:
- `NuovoAsteroide(cx, cy, diametro)` impacchetta i tre numeri in un `DatiAsteroide` (`centro` e `diametro`). Lo uso perché scrivere una riga per asteroide è molto più leggibile che costruire l'oggetto a mano ogni volta. Il colore non lo metto qui: lo decide il caricatore, che fa variare le tinte a turno (vedi `CostruisciAsteroidi`).
- `AggiungiCaramelle(livello, posizioni)` scorre l'array e per ogni posizione crea un `DatiCaramella`, assegnando il colore con `COLORI_CARAMELLE[i % COLORI_CARAMELLE.Length]`. Quel `modulo` è il trucco: ci sono cinque colori e dieci caramelle, quindi i colori si **riciclano** in ciclo senza mai uscire dall'array.

Gli array di colori sono due e stanno in punti diversi del file: `COLORI_CARAMELLE` (cinque tinte) qui dentro, per i livelli a posizioni fisse; e più sotto, nel caricatore, `COLORI_RANDOM` (sei tinte) per il livello 1. Sono separati apposta perché il livello 1 pesca un colore a caso, gli altri seguono l'ordine fisso dell'array.

#### CaricatoreLivelli (costruisce la scena)

Anche questa è `static`: tiene dei riferimenti privati al `contenitore` (un `GameObject` radice) e a tre `Transform` figli (`contenitoreCaramelle`, `contenitoreAsteroidi`, `contenitoreBombe`). Quei sotto-contenitori servono solo a tenere ordinata la gerarchia in Unity: tutte le caramelle finiscono sotto "Caramelle", e così via.

**`Carica(dati)`** è il regista. Prima chiama `Pulisci()` per cancellare la missione precedente, poi crea il `GameObject` "Missione" come radice, costruisce sfondo e Astro, crea i tre sotto-contenitori con `CreaFiglio`, e infine costruisce asteroidi, bombe e caramelle. Restituisce il numero totale di caramelle create, che serve al `GestoreGioco` per sapere quante raccoglierne.

**`MostraSoloSfondo()`** è una versione ridotta: pulisce, crea un contenitore "SoloSfondo" e disegna **solo** la nebulosa. La uso dietro la schermata iniziale, dove non si gioca ancora ma voglio comunque vedere lo sfondo invece del nero.

**`Pulisci()`** distrugge il contenitore con `Object.Destroy` e rimette a `null` tutti i riferimenti. Distruggere il padre butta via anche tutti i figli in un colpo solo: non devo cancellare ogni oggetto a mano.

`CreaFiglio(nome)` è l'aiutante che crea un `GameObject`, lo aggancia al contenitore con `SetParent` e ne torna il `Transform`.

**`CostruisciSfondo`** è la parte con un po' di matematica. Carico l'immagine con `Resources.Load<Texture2D>("sfondo")` (nome **senza estensione**); se manca, scrivo un `LogWarning` ed esco senza bloccare il gioco, resta il nero della telecamera. Se c'è, la trasformo in `Sprite` con `Sprite.Create` e la metto a `z = 10`, ben dietro a tutto, con `sortingOrder = -10` così viene disegnata per prima e sta sotto a ogni altra cosa. Poi la **ingrandisco fino a coprire la visuale**: dalla `Camera.main` leggo `orthographicSize`, che è la metà dell'altezza vista, quindi l'altezza piena è `orthographicSize * 2`; la larghezza la ricavo moltiplicando per `cam.aspect`. Confronto queste misure con quelle dello sprite (`sprite.bounds.size`) e prendo `Mathf.Max` tra le due scale (larghezza e altezza): scelgo la **più grande** così non restano bordi vuoti, anche se vuol dire tagliare un po' i lati. Il `* 1.02f` è un margine del 2% per sicurezza, e se la telecamera mancasse uso valori di ripiego (`6f` e `16f/9f`).

**`CostruisciAstro`** crea il `GameObject` "Astro" al centro `(0,0)`, scala `1.5`, sprite da `FabbricaImmagini.CreaAstro()` e `sortingOrder = 10` (il più alto, così Astro sta sempre davanti). Poi gli attacca il componente `Astro` che ne gestisce il comportamento.

**`CostruisciAsteroidi`** scorre la lista dei dati e per ognuno crea un `GameObject`. La parte interessante è come disegno la roccia: la lascio **intera** e la scalo in modo **uniforme** con `localScale = (d.diametro, d.diametro, 1)`. Scalando uguale in X e Y la roccia resta tonda e intera, senza deformazioni né tagli. Il colore lo prendo a turno da una piccola palette `COLORI_ASTEROIDE` (`i % lunghezza`), così le rocce sembrano pietre diverse. Poi chiamo `a.Inizializza(d.centro, d.diametro)` per dargli posizione e collisione.

**`CostruisciBombe`** è più semplice: per ogni `Vector2` nella lista crea una bomba scalata `1.2`, con lo sprite della bomba e `sortingOrder = 3`, poi `Inizializza(pos)`.

**`CostruisciCaramelle`** è il metodo che decide tra i due casi e torna il totale:
- se `dati.caramelleCasuali > 0` (livello 1) genera quel numero di caramelle pescando ogni volta una posizione da `PosizioneCaramellaCasuale` e un colore a caso da `COLORI_RANDOM` con `Random.Range`;
- altrimenti (livelli 2 e 3) usa le posizioni e i colori fissi già nella lista `dati.caramelle`.

`CreaCaramella` costruisce lo sprite e lo tinge: lo sprite della stellina è chiaro, e il `sr.color` lo colora (giallo, azzurro...), così ogni livello tiene le sue caramelle colorate.

**`PosizioneCaramellaCasuale`** è il punto dove evito di piazzare male le caramelle del livello 1. Provo **fino a 30 volte** a estrarre un punto in `x ∈ [-7, 7]`, `y ∈ [-3.5, 4]` e lo scarto se non va bene: con `p.magnitude < 1.2f` rifiuto i punti troppo vicini al centro (dove parte Astro, altrimenti le raccoglierebbe subito) e con `DentroAsteroide` rifiuto i punti dentro un ostacolo. Il primo punto buono lo torno; se in 30 tentativi non trovo niente (quasi impossibile) c'è un fallback `(3,3)`. Uso un numero massimo di tentativi invece di un `while` infinito apposta, per non rischiare un ciclo che non finisce mai.

**`DentroAsteroide(punto, dati, margine)`** è il controllo che dà supporto a sopra: per ogni asteroide guardo se il punto cade dentro il suo cerchio, allargato di `margine` per lasciare un po' di spazio libero attorno alla roccia (lo uso per non piazzare caramelle o la porta troppo a ridosso di un ostacolo). Se il punto è dentro torno `true`.

**`GeneraPorta`** non parte all'inizio: la chiama il `GestoreGioco` *dopo* che tutte le caramelle sono prese. Crea la porta, poi legge dove si trova Astro **adesso** da `Astro.Istanza.transform.position` e passa quel punto a `ScegliPosizionePortaCasuale`. La ragione è scritta nel commento e mi piace: la porta deve nascere **lontana** da Astro, altrimenti il livello finirebbe subito senza far fare il movimento, che è proprio il punto del gioco per la riabilitazione.

**`ScegliPosizionePortaCasuale(daEvitare)`** ha la stessa logica a tentativi (fino a 40 questa volta) ma con tre filtri: scarto i punti con `Vector2.Distance(p, daEvitare) < 3f` (troppo vicini ad Astro), quelli dentro un asteroide, e quelli `TroppoVicinoBomba`. Il range è più stretto (`x ∈ [-5.5, 5.5]`, `y ∈ [-2.5, 2.8]`) per non far nascere la porta sui bordi. `TroppoVicinoBomba` scorre `Bomba.Tutte` e torna `true` se il punto è a meno di `1.2f` da una bomba: così non costringo il paziente a passare incollato a una bomba per vincere.

I tre metodi finali generano gli effetti di festa e di errore, sempre agganciati al contenitore (e con il controllo `if (contenitore == null) return;` per non crashare se la missione è già stata pulita): `GeneraPianetaAmico` mette il pianeta amico in alto a `z = -2`, `GeneraCoriandoli` aggiunge solo il componente `Coriandoli`, e `GeneraEsplosione(posizione)` crea l'esplosione nel punto dato a `z = -3` con `sortingOrder = 8` (davanti a quasi tutto).

#### Le classi dati

In fondo ci sono tre classi che contengono **solo dati, niente logica**. Le tengo separate perché sono il "ponte" tra le due metà del file: `DefinizioneLivelli` le riempie, `CaricatoreLivelli` le legge.

| Classe | Campi | A cosa serve |
|---|---|---|
| `DatiLivello` | `titolo`, `obiettivo`, `caramelleCasuali`, e le liste `caramelle`, `asteroidi`, `bombe` | descrive un intero livello |
| `DatiCaramella` | `posizione`, `colore` | una caramella a posizione fissa |
| `DatiAsteroide` | `centro`, `diametro` | un asteroide (roccia tonda) |

Il campo chiave è `caramelleCasuali` in `DatiLivello`: parte da `0`, e quel valore è proprio l'interruttore che fa scegliere al caricatore tra caramelle casuali (livello 1, valore `> 0`) e caramelle dalla lista `caramelle` (livelli 2 e 3, valore `0`).

---

### L'interfaccia di gioco — InterfacciaGioco.cs

Questa è la classe che disegna tutto l'HUD del gioco: i contatori in alto, la barra dell'energia, i banner, i pannelli di vittoria e game over, il menu e il pannello tecnico in stile F3. Ho scelto la strada di `OnGUI`, l'immediate-mode GUI di Unity, perché mi permette di scrivere tutta l'interfaccia riga per riga in un solo file, senza dovere costruire una Canvas con i prefab e collegare i riferimenti dall'editor. Il prezzo da pagare è che `OnGUI` viene richiamato più volte per fotogramma (un evento per il `Layout`, uno per il `Repaint`, altri per gli input), e questo dettaglio ha conseguenze concrete sul dwell: lo spiego più avanti.

#### Update — il tasto F3, gli FPS e il cursore

`Update` qui non muove niente, fa tre cose di servizio.
- **Apre/chiude il pannello F3** quando premo `F3` (o il tasto `` ` ``), con `Input.GetKeyDown`.
- **Conta gli FPS**: `fps = Mathf.Lerp(fps, 1f / Time.unscaledDeltaTime, 0.1f)`, una media morbida così il numero non "balla", e tengo anche `fpsMinimo`.
- **Mostro o nascondo il cursore** del mouse con `Cursor.visible = pannelloAperto`: lo **nascondo mentre si gioca**, perché lì il vero puntatore è Astro e il cursore di sistema darebbe solo fastidio; lo **rimostro** solo quando c'è un pannello da cliccare (schermata iniziale, menu, fine livello, game over).

Il pannello F3 (`DisegnaPannelloDebug`) mostra anche valori come `Application.targetFrameRate`, `QualitySettings.vSyncCount` e `Time.timeScale`: questi li **leggo soltanto** per mostrarli — è un pannello di diagnostica, come quello di Minecraft — non li imposto, quindi non cambiano il gioco.

La classe è un singleton: in `Awake` salvo `Istanza = this`, così gli altri script possono raggiungerla senza cercarla in scena.

#### OnGUI — l'ordine di disegno

`OnGUI` è il metodo principale: lo chiama Unity ad ogni evento e io ridisegno l'intera schermata da capo ogni volta. La prima cosa che faccio è una guardia: se `stileGrande == null` chiamo `CostruisciStili()`. In questo modo gli stili si creano la prima volta che servono, dentro `OnGUI`, e non in `Awake` o `Start` (dove `GUI.skin` non è ancora affidabile).

Poi prendo `GestoreGioco.Istanza`: se è nullo esco subito, e se `MenuInizialeAperto` è vero esco comunque, perché in quel caso l'HUD non deve apparire — ci pensa la schermata iniziale a disegnarsi.

L'ordine in cui disegno conta, perché in `OnGUI` chi disegna dopo sta **sopra** a chi ha disegnato prima. Quindi seguo una piramide:

1. **Flash rosso a schermo intero**: se `gm.TimerLampeggio > 0` (Astro ha appena preso un colpo) coloro tutto lo schermo di rosso. L'intensità è `0.35f * Clamp01(TimerLampeggio / 0.25f)`: parte da circa 0.35 e svanisce verso zero man mano che il timer scende, così il flash sfuma da solo invece di sparire di colpo.
2. **HUD principale**: `DisegnaContatori`, `DisegnaTitoloLivello`, `DisegnaBarraEnergia`.
3. **Riga in basso** (solo se non siamo nella vittoria finale): il badge dell'obiettivo (`gm.TestoObiettivo`) e i pulsanti `MENU`, `RIAVVIA` e `INFO (F3)`. Il pulsante INFO esiste perché nella build WebGL il browser può intercettare il tasto F3 (la ricerca nella pagina), quindi do un modo alternativo per aprire il pannello tecnico.
4. **Banner PRONTI**: se `TempoIniziale > 0` e la partita non è già finita o vinta.
5. **Pannelli centrali**: vittoria finale, oppure missione completata; poi game over; poi il menu (se aperto e non in vittoria finale).
6. **Pannello tecnico F3**: per ultimo, così sta sopra a tutto il resto.

#### CostruisciStili — gli stili creati una volta sola

In `OnGUI` non voglio allocare un `GUIStyle` nuovo ad ogni frame per gli stili fissi, quindi li costruisco tutti qui dentro la prima volta. Parto sempre da uno stile di base di `GUI.skin` (`label`, `box` o `button`) e poi cambio solo quello che mi serve:

| Stile | Base | Uso |
|---|---|---|
| `stileGrande` | label | testo bianco bold 24, è il mio testo HUD standard |
| `stileEnorme` | label | 48 bold centrato, giallo caldo — titoloni dei pannelli |
| `stileTitolo` | label | 24 bold centrato, azzurrino — titolo livello |
| `stileBadge` | box | sfondo blu (texture vera via `TexturaPiena`) per l'obiettivo |
| `stileBottone` | button | 22 bold, i pulsanti |
| `stileDebug` | label | 15, verdino, niente word wrap — il pannello F3 |

Solo `stileBadge` ha bisogno di un `normal.background` con una texture vera, perché uno stile GUI non si colora con `GUI.color`: vuole proprio una `Texture2D`. Per questo chiamo `TexturaPiena`, ma una volta sola, qui dentro. Per gli stili "locali" e momentanei (per esempio `stilePunti`, `stileCuori`, `stileTempo`) li derivo invece al volo dentro i singoli metodi, perché cambiano poco e non vale la pena tenerli come campi.

#### DisegnaContatori — caramelle, punti e vite coi cuori

Tre righe in alto a sinistra:

- **Caramelle**: una `GUI.Label` con `CaramelleRaccolte / TotaleCaramelle`.
- **Punti**: derivo `stilePunti` da `stileGrande` portando il font a 34, e formatto il punteggio con `ToString("0000")` così ho sempre quattro cifre con gli zeri davanti (stile arcade).
- **Vite**: qui c'è il trucco. Derivo `stileCuori` e gli attivo `richText = true`, che permette di mettere tag di colore dentro la stringa. Costruisco la stringa cuore per cuore in un ciclo da 0 a `Impostazioni.VITE`: se l'indice `i < gm.Vite` aggiungo un cuore acceso `<color=#FF7388>♥</color>`, altrimenti un cuore grigio `<color=#555562>♥</color>`. Uso **sempre lo stesso simbolo** "♥" (il cuore pieno) e distinguo vita presente/persa solo col colore. La ragione è pratica: il simbolo del cuore "vuoto" su certi Windows non esiste e diventa un quadratino, mentre il cuore pieno c'è ovunque, quindi su qualsiasi PC l'HUD si vede uguale. È una scelta importante perché il professore gira su Windows.

#### DisegnaTitoloLivello

Centrato in alto. Leggo `gm.LivelloAttuale.titolo` (con guardia su null) e lo metto in una label larga centrata sullo schermo (`Screen.width / 2f - 320` per 640 di larghezza). Sotto, con uno stile derivato più piccolo (font 18), scrivo "Livello N di M" usando `gm.LivelloCorrente + 1` (il `+1` perché l'indice parte da zero ma all'utente mostro da 1) e `DefinizioneLivelli.Conteggio` per il totale.

#### DisegnaBarraEnergia — col tempo che lampeggia

In alto a destra disegno la barra dell'energia di Astro e il tempo rimasto. La barra è fatta a mano con due rettangoli pieni:

- prima lo **sfondo** scuro (`Riquadro` largo `larghezza`),
- poi il **riempimento** verde, la cui larghezza è `(larghezza - 6) * Clamp01(gm.Energia)`. Il `Clamp01` tiene il valore tra 0 e 1 anche se l'energia uscisse dal range, e i `-6 / +3` sono il bordino che lascia lo sfondo visibile attorno.

Per il tempo calcolo i secondi con `CeilToInt(gm.TempoRimasto)` (arrotondo per eccesso, così non vedo "0s" mentre c'è ancora un pezzo di secondo). Poi decido se è **urgente**: lo è quando `TempoRimasto <= 10` e la partita non è finita né completata. Se è urgente, il colore lampeggia: calcolo `k = 0.5 + 0.5 * Sin(Time.time * 8f)`, che è un'oscillazione tra 0 e 1 a frequenza alta, e con quel `k` faccio un `Color.Lerp` tra il giallo caldo e il rosso. Il seno mappato in `[0,1]` è il modo classico per ottenere un lampeggio morbido invece di un on/off secco. Sopra i 10 secondi il colore resta bianco fisso.

#### DisegnaBannerPronti — il banner PRONTI

Durante il conto alla rovescia iniziale disegno una striscia scura a circa metà schermo (`Screen.height * 0.45f`) e sopra la scritta "PRONTI... N", dove N è `CeilToInt(gm.TempoIniziale)`. Uso uno stile derivato da `stileEnorme` con font 40. Lo mostro solo finché `TempoIniziale > 0` e la partita non è già finita o vinta.

#### I pannelli centrali

Sono i sovra-pannelli che compaiono al centro nei vari stati. Tutti sono costruiti con gli stessi mattoni: `Riquadro` per gli sfondi e i bordi, label per i testi, e `BottoneAccessibile` per i pulsanti.

- **DisegnaPannelloVittoriaFinale**: è il più ricco. Prima copro tutto lo schermo con un velo blu-viola, poi disegno il pannello centrale (largo al massimo 820 ma che si restringe su schermi piccoli con `Min(820, Screen.width - 60)`). Il **bordo giallo** non è un rettangolo bordato: sono **quattro `Riquadro` sottili** (alto/basso/sinistra/destra), perché `OnGUI` non ha un primitivo "bordo" e disegnare quattro strisce è la via più semplice. Sotto al titolo metto il messaggio e i **crediti** organizzati a righe (etichetta + valore alternati, spaziati di 30/46 pixel): autore Filippo Cicirelli, esame "Sistemi per la Riabilitazione e la Terapia Assistita", professore Vitantonio Bevilacqua. In fondo il pulsante "GIOCA DI NUOVO" che chiama `gm.RicominciaTutto()`, con sotto `DisegnaSuggerimentoPulsante`.
- **DisegnaPannelloMissioneCompletata**: striscia scura, titolone "MISSIONE COMPLETATA!", e un pulsante la cui etichetta cambia: se `gm.ProssimoLivelloDisponibile` mostro "PROSSIMO LIVELLO" (e chiamo `ProssimoLivello`), altrimenti "RICOMINCIA" (e chiamo `RicominciaTutto`).
- **DisegnaPannelloGameOver**: striscia rossa scura e "BOOM! GAME OVER" con stile derivato in rosato. Il messaggio del motivo si adatta: se `MotivoSconfitta` è vuoto scrivo "Hai esaurito le vite.", altrimenti uso il motivo seguito da "Vite finite." e chiudo con un invito gentile, "Riprova con più calma!". Due pulsanti: "RIPROVA LIVELLO" (`gm.RipetiLivello()`) e "DA CAPO" (`gm.RicominciaTutto()`).
- **DisegnaPannelloMenu**: il pannello di pausa. Tre pulsanti, "RIPRENDI" (chiude il menu), "RIAVVIA", e "CAMBIA COMANDO" che chiama `gm.TornaAlMenuIniziale()` per riportare alla schermata iniziale e scegliere un altro controllo (mouse/dito/joystick).

#### DisegnaPannelloDebug — il pannello tecnico F3

È la mia versione della schermata F3 di Minecraft. Lo apro/chiudo dal tasto `F3` (o il backtick) in `Update`, dove tra l'altro tengo aggiornati gli FPS. Il calcolo è `fpsOra = 1 / Max(unscaledDeltaTime, 0.000001)`: uso `unscaledDeltaTime` così il valore non cambia se metto in pausa il tempo, e il `Max` evita la divisione per zero. Poi faccio una media morbida con `fps = Lerp(fps, fpsOra, 0.1f)`, così il numero non sfarfalla, e tengo traccia del minimo in `fpsMinimo` (che riazzero a `FPS_INIZIALE` ogni volta che riapro il pannello).

Il pannello disegna un velo nero semitrasparente e poi due colonne di testo (un'unica grande stringa multi-riga ciascuna):

- **Colonna sinistra — gioco**: livello, stato (vittoria finale / completata / game over / in gioco), obiettivo, punti, caramelle, vite, tempo, posizione e velocità di Astro (`Astro.Istanza.Velocita.magnitude`), posizione del mouse in pixel, se ci sono bombe attive, e il conteggio degli oggetti in scena letto dalle liste statiche `Caramella.Attive.Count`, `Asteroide.Tutti.Count`, `Bomba.Tutte.Count`.
- **Colonna destra — sistema**: prestazioni (FPS, FPS minimo, lag in ms per frame, target FPS, VSync, `timeScale`), dispositivo (tutto da `SystemInfo`: OS, modello, CPU e core, RAM, scheda e memoria video, API grafica), schermo (risoluzione, refresh, fullscreen) e altro (versione di Unity, piattaforma, tempo di gioco).

Un dettaglio sulla disposizione: la colonna destra parte a `xDestra = Max(510, Screen.width - 520)`. Così su uno schermo largo la metto vicino al bordo destro, ma su uno schermo stretto (la build WebGL gira a 960x600) la blocco a 510 perché non finisca sopra la colonna sinistra.

#### BottoneAccessibile — clic OPPURE dwell

Questo è il metodo più interessante, perché è il punto chiave dell'accessibilità: un pulsante si può premere col **clic del mouse** oppure tenendoci **sopra Astro** per qualche secondo (il *dwell*). Il dwell serve a chi gioca con webcam o joystick e non ha un vero clic.

La prima riga è semplice: `bool premuto = GUI.Button(...)`, che gestisce il clic normale. Poi, se `AstroSopra(r)`, gestisco il dwell. La logica del timer è delicata perché `OnGUI` viene chiamato più volte per frame, e se facessi avanzare il timer ad ogni chiamata conterei lo stesso fotogramma più volte. La soluzione è far scorrere il tempo **solo durante l'evento di Repaint**: controllo `Event.current.type == EventType.Repaint`, che capita una volta sola per frame. Dentro quel controllo:

- se non stavo già caricando questo pulsante (`!dwellAttivo || dwellRect != r`), vuol dire che ho appena puntato qui: attivo il dwell, salvo `dwellRect = r` e azzero `dwellTimer`;
- altrimenti incremento `dwellTimer += Time.unscaledDeltaTime`, e quando supera `ParametriComandi.DWELL_SECONDI` spengo il dwell (così non scatta due volte) e metto `premuto = true`.

Uso `dwellRect` come "identità" del pulsante: dato che i `Rect` sono uguali frame dopo frame, confrontarli mi dice se sto ancora puntando lo stesso pulsante o se sono passato a un altro. Se invece Astro **esce** dal pulsante mentre lo stavo caricando, nel ramo `else` (sempre nel Repaint) azzero `dwellAttivo` e `dwellTimer`: il caricamento si annulla.

Per il riscontro visivo, fuori dal controllo del Repaint chiamo `DisegnaBarraDwell(r, k)` con `k = Clamp01(dwellTimer / DWELL_SECONDI)`: è la frazione di riempimento, da 0 (appena entrato) a 1 (sta per scattare). La barra (vedi `DisegnaBarraDwell`) è una strisciolina verde alta 6 pixel in fondo al pulsante, larga `r.width * k`: si riempie da sinistra a destra man mano che tengo Astro fermo. Se non sto puntando quel pulsante, `k` è 0 e la barra resta vuota.

#### AstroSopra — dal mondo allo schermo

Mi serve sapere se Astro è dentro il rettangolo di un pulsante, ma Astro vive nel mondo di gioco e i `Rect` della GUI sono in pixel di schermo, con un dettaglio: **gli assi y sono invertiti**. In Unity lo schermo ha y che cresce verso l'alto, ma in `OnGUI` la y cresce verso il **basso**. Quindi prendo la posizione di Astro nel mondo, la converto in pixel con `cam.WorldToScreenPoint`, e poi costruisco il punto GUI come `(schermo.x, Screen.height - schermo.y)`: quel `Screen.height -` è proprio l'inversione della y. Alla fine basta `r.Contains(puntoGui)`. Se manca Astro o la camera, ritorno `false` senza fare danni.

#### Riquadro e TexturaPiena — perché riusano la texture bianca

Questi due sono gli aiutanti per disegnare rettangoli colorati, ed è dove ho sistemato uno spreco.

`Riquadro(Rect, Color)` disegna un rettangolo pieno di un colore qualsiasi **senza creare nessuna texture nuova**: salvo il `GUI.color` corrente, lo imposto al colore voluto, disegno la `Texture2D.whiteTexture` (la texture bianca già pronta di Unity) con `GUI.DrawTexture`, e poi rimetto il `GUI.color` di prima. Siccome la texture è bianca, moltiplicarla per `GUI.color` mi dà esattamente il colore che voglio. Prima creavo una `Texture2D` nuova per ogni rettangolo ad ogni frame: era uno spreco inutile, dato che `OnGUI` gira in continuazione. Per questo `Riquadro` è il metodo che uso ovunque per sfondi, barre, bordi e veli.

`TexturaPiena(Color)` invece crea davvero una texture 1x1 di un colore (`SetPixel` + `Apply`), ma serve solo nel caso in cui uno **stile GUI** voglia un `normal.background`: lì `GUI.color` non basta, lo stile pretende una texture vera. La chiamo una sola volta, dentro `CostruisciStili` per lo sfondo di `stileBadge`, quindi qui non c'è spreco perché non si rifà ad ogni frame.

---

### Schermata iniziale e comandi (cartella schermata start)

Questa cartella decide come si comanda Astro e cosa vedo appena parte il gioco. L'idea che tengo fissa è una sola: il resto del gioco non deve sapere se sto usando il mouse, la webcam o il joystick. Chiede solo "dove sta il puntatore?" e basta. I tre file dividono i compiti così: `Comandi.cs` è la logica, `SchermataStart.cs` è il menu che disegno a schermo, `ParametriComandi.cs` è la pagina dei numeri da regolare.

#### Comandi.cs

Questo è il file che fa da "centralino" del puntatore. Tutta la sua ragione di esistere è in una frase: Astro non deve sapere quale comando è attivo.

**L'enum `Modalita`.** Definisco i tre modi possibili con `enum Modalita { Mouse, Dito, Joystick }`. Uso un enum e non tre booleani o tre stringhe perché così il modo attivo è uno e uno solo (`Attuale`), non posso mai finire in uno stato assurdo tipo "mouse e joystick insieme". Parto da `Modalita.Mouse` come default.

**Perché disaccoppio Astro dal comando.** Il punto centrale è il metodo `PuntatoreSchermo()`, che è `static` così chiunque lo chiama con `Comandi.PuntatoreSchermo()` senza avere un riferimento all'oggetto. Restituisce sempre un `Vector3` in pixel dello schermo, lo stesso sistema di coordinate di `Input.mousePosition`. Dentro c'è uno `switch` su `Attuale`:

| Modalità | Cosa restituisco |
|---|---|
| `Dito` | la posizione del dito vista dalla webcam (`ComandoWebcam.Posizione`), ma solo se `ComandoWebcam.Pronta`; altrimenti ripiego sul mouse |
| `Joystick` | la posizione che ho accumulato io, `posizioneJoystick`, impacchettata in un `Vector3` |
| default (`Mouse`) | direttamente `Input.mousePosition` |

Il vantaggio è concreto: Astro chiama una riga sola e non gli importa da dove arriva il numero. Se domani aggiungo un quarto comando, tocco solo questo `switch` e il resto del gioco resta identico. Sul caso `Dito` ho messo una rete di sicurezza: se la webcam non è ancora partita, invece di dare una posizione sballata torno al mouse, così il gioco è sempre comandabile.

**Il problema della posizione "assoluta".** Mouse e dito hanno una posizione assoluta: in ogni istante so dove sono sullo schermo. Il joystick no: una leva mi dà una *direzione*, non un punto. Per questo tengo una variabile mia, `posizioneJoystick`, che parto dal centro dello schermo (`CentraPuntatore()` la mette a `Screen.width / 2f`, `Screen.height / 2f`) e poi sposto fotogramma per fotogramma. Ricentro anche dentro `Imposta()`, cioè quando premo GIOCA scegliendo il joystick, così ogni partita riparte sempre dal centro.

**`MuoviConJoystick()`** gira solo se il comando attivo è il joystick (lo controllo in `Update()` per non sprecare lavoro negli altri modi). Qui c'è la matematica vera:

- Leggo i due assi standard di Unity con `Input.GetAxisRaw("Horizontal")` e `"Vertical"`. Uso la versione `Raw` (valori netti, senza la levigatura automatica di Unity) perché la morbidezza me la gestisco da solo. Gli assi vanno da `-1` a `+1`, e le frecce e i tasti WASD pilotano gli stessi assi, quindi il joystick e la tastiera funzionano senza scrivere codice in più.
- **Zona morta:** se `Mathf.Abs(ax)` è sotto `JOYSTICK_ZONA_MORTA` lo azzero, idem per `ay`. Serve perché una leva fisica a riposo non sta mai esattamente a zero: senza questo filtro il puntatore "scivolerebbe" da solo. Confronto il valore assoluto così copro sia il verso positivo che quello negativo con un controllo solo.
- **Inversione Y:** se `JOYSTICK_INVERTI_Y` è vero faccio `ay = -ay`, per chi preferisce il comando "stile aereo" (leva avanti = giù).
- **Movimento a velocità:** calcolo `passo = JOYSTICK_VELOCITA * Time.deltaTime` e sommo `ax * passo` e `ay * passo` alla posizione. Moltiplicare per `Time.deltaTime` è il trucco fondamentale: rende lo spostamento indipendente dagli FPS, così il puntatore va alla stessa velocità sia su un PC veloce che su uno lento. "A velocità" vuol dire che più tengo la leva, più continuo ad andare nella stessa direzione: non è uno spostamento istantaneo ma un accumulo.
- **Clamp:** alla fine blocco la posizione dentro lo schermo con `Mathf.Clamp(..., 0f, Screen.width)` e `Screen.height`, così il puntatore non può uscire dai bordi.

Infine l'auto-installazione: con `[RuntimeInitializeOnLoadMethod]` l'oggetto `Comandi` si crea da solo all'avvio, non devo trascinarlo in scena. Tengo un riferimento `Istanza` e in `Installa()` controllo che non ne esista già uno, per non averne due.

#### SchermataStart.cs

È la prima cosa che si vede. La disegno tutta con `OnGUI`, lo stesso sistema dell'HUD di gioco: niente Canvas, niente prefab, il codice si legge dall'alto in basso. Anche questa classe si installa da sola con `[RuntimeInitializeOnLoadMethod]`.

**Quando si mostra.** In `OnGUI` per prima cosa prendo `GestoreGioco.Istanza` e, se il menu non è aperto (`!gm.MenuInizialeAperto`), esco subito senza disegnare niente. Così la schermata compare solo al momento giusto. Sopra lo sfondo stendo un velo scuro semitrasparente con `Riquadro(...)` così i testi si leggono bene.

**Gli stili creati una volta sola.** I `GUIStyle` (titolo, sezioni, bottoni, info...) li costruisco in `CostruisciStili()`, chiamata solo la prima volta (`if (stileTitolo == null)`). `OnGUI` gira tanti fotogrammi al secondo: ricreare gli stili ogni volta sarebbe spreco inutile.

**Le tre colonne.** Sotto il titolo "ASTRO" calcolo a mano la larghezza di tre colonne uguali: `(Screen.width - margine * 4f) / 3f`, cioè tolgo i quattro margini (esterni più i due tra le colonne) e divido per tre. Poi piazzo le tre x e disegno:

1. **Scelta del comando** (`DisegnaColonnaComandi`): i tre pulsanti più il GIOCA.
2. **Impostazioni del gioco** (`PannelloInfo` con `TestoImpostazioni()`).
3. **Parametri tecnici** (`PannelloInfo` con `TestoParametriTecnici()`).

`PannelloInfo` è un aiutante che disegna riquadro + titolo + testo su più righe, così non ripeto lo stesso codice per le due colonne di testo.

**`BottoneModalita` (il pulsante selezionato diventa verde).** Ogni comando è un pulsante. Il trucco visivo sta qui: confronto la modalità del pulsante con `scelta` e, se coincidono, prima di disegnarlo metto `GUI.backgroundColor` su un verde (`0.30f, 0.85f, 0.40f`); subito dopo rimetto il colore vecchio che avevo salvato, così coloro solo *quel* pulsante e non quelli dopo. In più antepongo `"> "` all'etichetta del selezionato come segnale extra. Ho scelto apposta il carattere `>`, un carattere normale di tastiera, perché si veda uguale su Windows e su Mac. Il metodo ritorna `true` se l'ho premuto, e nella colonna aggiorno `scelta`. Da notare che premere un pulsante cambia solo la selezione: il comando parte davvero solo col GIOCA, che chiama `gm.IniziaPartita(scelta)`.

Sotto i tre pulsanti mostro una `DescrizioneComando(scelta)` che spiega in due righe il comando evidenziato (per la webcam dice di muovere un evidenziatore fluo verde/giallo/fucsia, per il joystick dice che vanno bene anche frecce o WASD).

**`TestoImpostazioni()`.** Costruisco una stringa unica concatenando i valori reali del gioco, raggruppati con intestazioni tipo `=== PARTITA ===`. Non scrivo numeri a mano: leggo le costanti vere (`DefinizioneLivelli.Conteggio`, `Impostazioni.VITE`, `Impostazioni.TEMPO_LIVELLO`, i punti, i raggi di presa, gli interruttori del feedback `ParametriFeedback.SUONO_ATTIVO`/`VISIVO_ATTIVO` mostrati come "acceso"/"spento", e i parametri dei comandi come `ParametriComandi.JOYSTICK_VELOCITA` e `JOYSTICK_ZONA_MORTA`). Così questa colonna è sempre sincronizzata con le impostazioni vere: se cambio un valore nel suo file, qui si aggiorna da solo.

**`TestoParametriTecnici()`.** Stessa idea, ma per le info di sistema, utili da mostrare all'esame. Ci sono le prestazioni (FPS calcolati con la media morbida, target FPS, VSync, lag in ms), lo schermo (risoluzione, refresh rate, schermo intero, DPI), il dispositivo (sistema operativo, modello, CPU e core, RAM, scheda e memoria video, API grafica, numero di webcam trovate con `WebCamTexture.devices.Length`) e il software (versione Unity, piattaforma). Sono quasi tutte chiamate a `SystemInfo`, `Screen` e `Application`: leggo lo stato reale della macchina, non invento niente.

Gli FPS li calcolo in `Update()` con una media morbida: `fps = Mathf.Lerp(fps, fpsOra, 0.1f)`. Il `Lerp` con peso `0.1` fa avvicinare il valore mostrato a quello istantaneo solo di un decimo per fotogramma, così il numero non "balla" di continuo ed è leggibile. `fpsOra` è `1f / Time.unscaledDeltaTime`, con un `Mathf.Max` minuscolo per non rischiare la divisione per zero.

Un dettaglio sul disegno: `Riquadro(...)` colora un rettangolo pieno senza creare texture nuove ogni frame; uso la `Texture2D.whiteTexture` già pronta di Unity e la tingo con `GUI.color`, salvando e ripristinando il colore precedente.

#### ParametriComandi.cs

È la "pagina dei valori" dei comandi, gemella di `Impostazioni.cs` per il gioco: dentro non c'è nessuna logica, solo i numeri che posso voler cambiare. Tengo i parametri separati dalla logica così, per ritarare il joystick su un paziente diverso, tocco un file solo e non rischio di rompere il codice in `Comandi.cs`.

Riguarda soprattutto il joystick (mouse e webcam non hanno bisogno di questi numeri):

| Parametro | Valore | Cosa regola |
|---|---|---|
| `JOYSTICK_VELOCITA` | `1000f` | pixel al secondo con la leva a fondo corsa; più alto = puntatore più veloce |
| `JOYSTICK_ZONA_MORTA` | `0.15f` | sotto questa soglia ignoro la leva, così non scivola da sola |
| `JOYSTICK_INVERTI_Y` | `false` | se vero inverte su/giù (comando "stile aereo") |
| `DWELL_SECONDI` | `1.2f` | quanti secondi tenere il puntatore fermo su un pulsante per premerlo senza clic |

Le prime tre sono esattamente i valori che `MuoviConJoystick()` legge. `DWELL_SECONDI` invece serve ai pulsanti dei pannelli (fine livello, game over, menu): chi gioca con webcam o joystick non ha il clic del mouse, quindi può "premere" tenendo il puntatore fermo sopra il pulsante per quel tempo. Le tre soglie del joystick sono `const` perché non cambiano a runtime; `JOYSTICK_INVERTI_Y` è un `readonly bool` perché è un interruttore di preferenza.

---

### Il comando con la webcam (cartella comando webcam)

In questa modalità Astro si muove davanti alla webcam invece che col mouse. Importante: non riconosco "il dito" davvero (servirebbe l'intelligenza artificiale), ma seguo un **evidenziatore fluo** tenuto in mano (verde, giallo o fucsia). Per ogni fotogramma della webcam cerco i pixel di quel colore e ne calcolo il centro: quel centro diventa la posizione del puntatore. Gli evidenziatori sono molto più saturi della pelle, quindi non rischio di scambiare il colore col viso.

#### ComandoWebcam.cs

##### Auto-installazione

Come per `FeedbackPaziente`, questo oggetto **si installa da solo**: non devo trascinarlo in nessuna scena. Il metodo `Installa()` ha l'attributo `[RuntimeInitializeOnLoadMethod(AfterSceneLoad)]`, quindi Unity lo chiama in automatico subito dopo aver caricato la scena. Lì creo un `GameObject` nuovo e gli attacco il componente `ComandoWebcam`. Il controllo `if (Istanza != null) return;` mi assicura che ce ne sia uno solo. Gli altri file non vanno a cercare il componente: leggono direttamente le variabili statiche `Posizione` (dove sta il dito, in pixel dello schermo, stesso sistema di `Input.mousePosition`) e `DitoVisto` (se in questo fotogramma ho trovato il colore). Ci pensa poi `Comandi` a passare `Posizione` ad Astro.

##### Awake: precalcolo le tinte

La tecnica che uso per riconoscere un colore non lavora in RGB ma in **HSV** (tinta, saturazione, luminosità). La ragione è semplice: la **tinta** (`hue`) di un colore resta la stessa anche se cambia la luce — un verde resta verde sia in ombra che al sole — mentre i valori RGB cambiano parecchio. Quindi in `Awake` ciclo sui `COLORI_EVIDENZIATORE` e, con `Color.RGBToHSV`, mi salvo nell'array `tinteBersaglio` solo la tinta di ognuno. Lo faccio **una volta sola** all'avvio, non a ogni fotogramma, perché è un valore fisso. Sempre qui inizializzo `Posizione` al centro dello schermo, così se la webcam non è ancora pronta il puntatore parte dal mezzo.

##### ModalitaDitoAttiva

La webcam non deve stare accesa sempre: è invadente e consuma. `ModalitaDitoAttiva()` mi dice quando serve davvero, e torna `true` solo se ricorrono **tre** condizioni insieme: la modalità scelta è `Comandi.Modalita.Dito`, esiste il `GestoreGioco`, e il menù iniziale **non** è aperto (`!MenuInizialeAperto`). Quindi durante la schermata iniziale la webcam resta spenta e si accende solo quando la partita parte davvero. In `Update`, se la modalità non è attiva, metto la webcam in pausa con `webcam.Pause()` per liberarla.

##### Accensione: la coroutine AvviaWebcam

L'accensione passa da `AccendiWebcam()`: se la `webcam` esiste già ma è in pausa la riavvio con `Play()`; se invece non esiste ancora e non sto già avviando (`avvioInCorso`), faccio partire la coroutine `AvviaWebcam()`. Uso una **coroutine** perché chiedere il permesso non è istantaneo e non voglio bloccare il gioco mentre aspetto.

Dentro `AvviaWebcam` faccio i passi in ordine:
1. `yield return Application.RequestUserAuthorization(UserAuthorization.WebCam)` — chiedo il permesso (serve su alcuni sistemi e nel browser) e aspetto la risposta dell'utente. Se viene negato scrivo `messaggio = "permesso negato"` ed esco.
2. Controllo `WebCamTexture.devices.Length`: se è 0 non c'è nessuna webcam e lo segnalo.
3. Altrimenti prendo la **prima** webcam disponibile (`devices[0].name`) e creo la `WebCamTexture` chiedendo larghezza, altezza e FPS dai `ParametriWebcam`, poi `Play()`.

La proprietà `Pronta` mi serve da semaforo: è vera solo se la webcam esiste, sta suonando e `width > 16`. Quel controllo sul 16 è un trucco: finché la texture è ancora 16x16 vuol dire che il **primo vero fotogramma non è ancora arrivato**, quindi non analizzo nulla.

##### AnalizzaFrame: il "centro di massa" del colore

È il cuore della modalità, e lo eseguo solo quando `Pronta && webcam.didUpdateThisFrame` (cioè quando c'è davvero un fotogramma nuovo, inutile rianalizzare lo stesso).

Prima copio i pixel della webcam in un array `Color32[] pixel` con `GetPixels32`. Quell'array lo **riuso** tra un fotogramma e l'altro (lo ricreo solo se cambia la dimensione `w*h`), così non alloco memoria nuova 30 volte al secondo.

Poi la matematica vera. Scorro tutti i pixel e per ognuno chiamo `ColoreGiusto`: se è del colore giusto **sommo la sua posizione** in `sommaX`/`sommaY` e incremento `conta`. Alla fine il "centro" è semplicemente la **media** delle posizioni: `sommaX / conta` e `sommaY / conta`. È il **centro di massa** dei pixel colorati, cioè il punto medio dove si trova l'evidenziatore. Uso `long` per le somme perché su tante posizioni un `int` rischierebbe di traboccare.

Due dettagli che contano:
- **`PASSO_ANALISI`**: nei due cicli `for` non vado di pixel in pixel ma a salti di `passo` (`x += passo`, `y += passo`). Analizzare 1 pixel ogni 2 mi dà circa **un quarto** del lavoro a parità di immagine: per trovare il centro di una macchia di colore non serve guardare ogni singolo pixel, basta un campione. Più alto è il passo, più vado veloce ma meno preciso.
- **`PIXEL_MINIMI`**: dichiaro `DitoVisto = true` solo se `conta >= PIXEL_MINIMI`. Così, se becco due o tre puntini sparsi di colore simile (rumore), li ignoro e non sparo il puntatore a caso. Se non raggiungo il minimo, metto `DitoVisto = false` e **lascio il puntatore dov'era** — non lo resetto.

Quando il dito è visto, calcolo la posizione normalizzata `nx`, `ny` dentro l'immagine (valori 0..1, dividendo per `w` e `h`). Qui applico l'**effetto specchio**: se `SPECCHIA` è attivo faccio `nx = 1f - nx`. Senza, muovendo il dito a destra il puntatore andrebbe a sinistra, perché la webcam mi vede "di fronte"; con lo specchio l'immagine si comporta come uno specchio vero ed è molto più naturale. Salvo `ultimaNx`/`ultimaNy` (mi servono per disegnare il mirino) e converto in pixel di schermo moltiplicando per `Screen.width`/`Screen.height`.

##### Lo smoothing indipendente dagli FPS

Se mettessi il puntatore di colpo sul centro di massa, tremerebbe a ogni fotogramma (la mano non è ferma e il rilevamento ha rumore). Quindi faccio inseguire la posizione al bersaglio con un `Vector2.Lerp(Posizione, bersaglio, t)`: il puntatore si avvicina al bersaglio di una frazione `t` per fotogramma, in modo morbido.

Il problema è che un `Lerp` con `t` fisso **dipende dagli FPS**: su un PC veloce viene chiamato più spesso e quindi smorza di più, su uno lento di meno, e il gioco "si sente" diverso. Per renderlo **indipendente dal frame rate** uso la formula:

`t = 1f - Mathf.Pow(morbidezza, Time.deltaTime * 60f)`

L'idea è che la `MORBIDEZZA` esprime "quanta distanza resta dopo 1/60 di secondo". Elevando quel fattore a `Time.deltaTime * 60` lo riscalo sul tempo realmente trascorso: se un fotogramma dura il doppio, l'esponente raddoppia e la potenza tiene conto esattamente della frazione che sarebbe rimasta con due passi piccoli. Risultato: lo smorzamento è lo stesso a 30 o a 120 FPS. Uso `Mathf.Clamp01` sulla morbidezza per stare nel range valido della potenza.

##### ColoreGiusto: tinta sul cerchio dei colori

`ColoreGiusto` decide se un singolo pixel "vale". Prima converto il `Color32` in HSV con `RGBToHSV`, poi faccio tre test:
1. `s < SATURAZIONE_MINIMA` → scarto. Questo è il filtro chiave: la pelle e il viso sono **poco saturi**, un evidenziatore fluo è molto saturo. Tenendo la soglia alta butto via il viso.
2. `v < LUMINOSITA_MINIMA` → scarto: troppo scuro, è un'ombra nera.
3. **Distanza di tinta**: confronto la tinta `h` del pixel con ognuna delle `tinteBersaglio`; basta che combaci **uno solo** dei colori.

Il punto interessante è il calcolo della distanza di tinta, perché la tinta è un **cerchio**: 0 e 1 sono lo stesso colore (il rosso). Se mi limitassi a `Mathf.Abs(h - tinta)`, due tinte vicinissime ma a cavallo dello zero (es. 0.98 e 0.02) risulterebbero lontanissime. Quindi calcolo `dh = Abs(h - tinta)` e, se `dh > 0.5`, lo correggo in `dh = 1 - dh`: in pratica misuro la distanza "girando dalla parte più corta" del cerchio. Se questa distanza è entro `TOLLERANZA_TINTA` il colore va bene.

##### OnGUI: l'anteprima col mirino

`OnGUI` disegna solo se `ModalitaDitoAttiva()`. Se la webcam non è ancora `Pronta`, con `DisegnaSuggerimento` scrivo in basso il motivo (es. "permesso negato (uso il mouse)" oppure "Avvio webcam..."): intanto il gioco resta giocabile col mouse.

Quando è pronta e `MOSTRA_ANTEPRIMA` è attivo, `DisegnaAnteprima` mette in alto a destra un riquadro con quello che vede la webcam. Calcolo l'altezza `ph` dalla larghezza mantenendo le **proporzioni** (`pw * height / width`). Per disegnare l'immagine specchiata uso `GUI.DrawTextureWithTexCoords` con coordinate `Rect(1, 0, -1, 1)` quando `SPECCHIA` è attivo: la larghezza negativa ribalta la texture in orizzontale, così combacia con il mirino. Il **mirino** è una crocetta (`DisegnaMirino`) fatta con due rettangolini bianchi colorati via `GUI.color`, posizionata in `ultimaNx`/`ultimaNy`: nota che per la y faccio `1f - ultimaNy` perché la GUI ha la y verso il basso, mentre l'immagine la conta verso l'alto. Il mirino è **verde** se vedo il dito, **rosso** se non lo vedo, così il paziente capisce subito se l'evidenziatore è inquadrato. Sopra, un'etichetta dice "evidenziatore: OK" oppure "mostra l'evidenziatore".

#### ParametriWebcam.cs

È la **pagina dei valori** della modalità, sullo stesso modello di `Impostazioni.cs`: nessuna logica, solo i numeri che un terapista può regolare senza toccare il resto del codice.

| Variabile | Valore | Cosa regola |
|---|---|---|
| `COLORI_EVIDENZIATORE` | verde, giallo, fucsia fluo | la lista dei colori da seguire; basta che il dito combaci con uno qualsiasi. Per cambiarli si modifica solo questa lista |
| `TOLLERANZA_TINTA` | `0.08` | quanto la tinta del pixel può discostarsi da un colore della lista (0 = identico, 0.5 = mezza ruota) |
| `SATURAZIONE_MINIMA` | `0.55` | soglia di saturazione: tenuta **alta** apposta per scartare pelle e viso |
| `LUMINOSITA_MINIMA` | `0.25` | scarta i pixel troppo scuri (ombre) |
| `LARGHEZZA_RICHIESTA` / `ALTEZZA_RICHIESTA` | `320` / `240` | risoluzione richiesta alla webcam: bassa = analisi veloce, e basta e avanza per seguire un colore |
| `FPS_RICHIESTI` | `30` | fotogrammi al secondo richiesti |
| `SPECCHIA` | `true` | effetto specchio: dito a destra → puntatore a destra |
| `PASSO_ANALISI` | `2` | analizzo 1 pixel ogni 2: più alto = più veloce ma meno preciso |
| `PIXEL_MINIMI` | `12` | quanti pixel del colore servono per dire "ho visto il dito" (ignora i puntini sparsi) |
| `MORBIDEZZA` | `0.5` | morbidezza del puntatore: 0 = scattoso e reattivo, vicino a 1 = molto calmo (utile se la mano trema) |
| `MOSTRA_ANTEPRIMA` | `true` | accende/spegne l'anteprima della webcam |
| `ANTEPRIMA_LARGHEZZA` | `240` | larghezza del riquadro in pixel di schermo |
| `ANTEPRIMA_MARGINE` | `20` | distanza dell'anteprima dal bordo |

In pratica, i tre parametri su cui si interviene di più sono `MORBIDEZZA` (se la mano del paziente trema, lo alzo), `SATURAZIONE_MINIMA` e `TOLLERANZA_TINTA` (se la stanza ha una luce difficile e il colore fatica a essere visto, li allargo un po').

---

### Il feedback per il paziente (cartella feedback paziente)

In questa cartella ci sono due file. Uno è il "cervello" che fa partire suoni e animazioni (`FeedbackPaziente.cs`), l'altro è la pagina dove tengo tutti i numeri regolabili (`ParametriFeedback.cs`). Tengo separate logica e valori esattamente come faccio nel resto del progetto: chi vuole tarare il feedback tocca solo i numeri, non il codice.

#### FeedbackPaziente.cs

Questo è l'oggetto che dà al paziente il "premio" quando fa la cosa giusta e il "no" gentile quando sbaglia, su due canali insieme: orecchio (un suono) e occhio (Astro che si gonfia o si schiaccia). L'idea di riabilitazione dietro è semplice: un feedback immediato e su due sensi aiuta a capire subito se il movimento è corretto, così i gesti giusti si rinforzano.

**Auto-installazione.** Non voglio dover trascinare questo oggetto nella scena dentro Unity: è una cosa in più che si può dimenticare. Quindi uso il metodo `Installa`, marcato con `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]`: Unity lo chiama da solo all'avvio, dopo che la scena è caricata. Dentro controllo `if (Istanza == null)` e, solo se non c'è già, creo `new GameObject("FeedbackPaziente").AddComponent<FeedbackPaziente>()`. Il controllo su `Istanza` evita di crearne due copie. `Istanza` è il riferimento statico unico (lo riempio in `Awake`) che permette agli altri script di trovarmi senza cercarmi nella scena.

**Awake.** Qui faccio tre cose, una volta sola all'avvio.

1. *Garantisco UN SOLO `AudioListener`.* L'`AudioListener` sono le "orecchie" del gioco: senza, i suoni non si sentono, e Unity si lamenta se ce ne sono due. Il problema è che il mio gioco ricrea la telecamera da zero, quindi quella di default (con le sue orecchie) viene distrutta e l'ordine in cui succedono le cose non è garantito. Per non dipendere da quell'ordine faccio la cosa sicura: con `Object.FindObjectsByType<AudioListener>()` trovo tutte le orecchie esistenti, le distruggo in un ciclo, e poi ne metto una sola con `gameObject.AddComponent<AudioListener>()` su questo oggetto, che resta vivo per tutta la partita. Così so per certo che ce n'è esattamente una.

2. *Creo l'`AudioSource` 2D.* È l'"altoparlante". Lo aggiungo con `AddComponent<AudioSource>()` e lo metto `playOnAwake = false` (non deve partire da solo all'avvio) e `spatialBlend = 0f`, cioè suono completamente 2D. Lo metto 2D apposta: il suono del feedback non deve cambiare a seconda di dove si trova Astro nello spazio, deve sentirsi sempre uguale.

3. *Carico i tre suoni una volta sola.* Con `Resources.Load<AudioClip>(...)` carico `suonoCaramella`, `suonoVittoria` e `suonoErrore` dai file in `Assets/Resources`, usando i nomi che leggo da `ParametriFeedback` (`SUONO_RACCOLTA`, `SUONO_VITTORIA`, `SUONO_ERRORE`). Li carico qui in `Awake` e li tengo nei campi, così non ricarico il file da disco ogni volta che serve un suono. Se un file manca, `Resources.Load` torna `null`: non blocco il gioco, stampo solo un `Debug.LogWarning`. La rete di sicurezza vera è nel metodo `Suona`, che vedo dopo.

In `OnDestroy` rimetto `Istanza = null` (solo se sono davvero io l'istanza corrente), per non lasciare un riferimento a un oggetto distrutto.

**I metodi statici pubblici.** Sono i tre comandi che gli altri file del gioco chiamano con una riga sola:

- `CaramellaPresa()` — stellina raccolta
- `MissioneCompiuta()` — livello vinto
- `AzioneSbagliata()` — errore (asteroide, bomba, tempo scaduto)

Sono `static` apposta: chi li chiama scrive `FeedbackPaziente.CaramellaPresa();` senza doversi procurare un riferimento all'oggetto. Dentro fanno tutti la stessa guardia `if (Istanza != null)` prima di agire, così se per qualche motivo il feedback non c'è la chiamata non dà errore, semplicemente non fa niente. `CaramellaPresa` e `MissioneCompiuta` chiamano lo stesso metodo interno `Giusto(...)` passandogli il clip diverso (la caramella o la vittoria), perché la *logica* del premio è identica e cambia solo il suono. `AzioneSbagliata` chiama `Sbagliato()`.

**Giusto e Sbagliato.** Sono il cuore della logica e sono speculari. `Giusto(suono)`:
- se `ParametriFeedback.SUONO_ATTIVO` è vero, chiama `Suona(suono, VOLUME_GIUSTO)`;
- se `VISIVO_ATTIVO` è vero **e** `Astro.Istanza != null`, chiama `Astro.Istanza.Gonfia()`.

`Sbagliato()` fa la stessa cosa al contrario: suona `suonoErrore` a `VOLUME_SBAGLIATO` e chiama `Astro.Istanza.Schiaccia()`. I due interruttori `SUONO_ATTIVO` e `VISIVO_ATTIVO` sono indipendenti: posso spegnere solo il suono (paziente sensibile ai rumori) e tenere il visivo, o viceversa. Il controllo `Astro.Istanza != null` serve perché la parte visiva la fa Astro, e potrei essere in un momento in cui Astro non esiste: in quel caso il suono parte lo stesso e l'animazione viene saltata senza errori.

**Suona.** Per fare partire un clip uso `sorgente.PlayOneShot(clip, volume)`. Scelgo `PlayOneShot` invece del normale `Play` perché permette ai suoni di sovrapporsi: se il paziente prende due stelline una dietro l'altra, il secondo suono non taglia il primo, partono sopra. All'inizio del metodo c'è la guardia `if (clip == null || sorgente == null) return;`: è qui che la rete di sicurezza dei file mancanti diventa concreta, se un clip non è stato caricato non succede niente. Il volume che passo non è quello secco: lo moltiplico per `VOLUME_GENERALE`, così quel parametro funziona da volume "master" che scala tutti gli altri.

#### ParametriFeedback.cs

È una `static class` senza logica, solo costanti: la pagina dei valori da regolare. Un terapista può cambiarli senza toccare il resto.

| Gruppo | Variabile | Valore | A cosa serve |
|---|---|---|---|
| Interruttori | `SUONO_ATTIVO` | `true` | accende/spegne tutti i suoni |
| | `VISIVO_ATTIVO` | `true` | accende/spegne tutti gli effetti visivi |
| Volumi (0–1) | `VOLUME_GENERALE` | `0.90` | volume master, moltiplica gli altri |
| | `VOLUME_GIUSTO` | `0.80` | volume delle azioni corrette |
| | `VOLUME_SBAGLIATO` | `0.45` | volume degli errori (più basso) |
| Nomi suoni | `SUONO_RACCOLTA` | `"raccolta"` | file della stellina raccolta |
| | `SUONO_VITTORIA` | `"vittoria"` | file del livello completato |
| | `SUONO_ERRORE` | `"errore"` | file dell'azione sbagliata |
| Gonfia (positivo) | `GONFIA_QUANTITA` | `0.45` | cresce fino a +45% |
| | `GONFIA_DURATA` | `0.45` s | quanto dura il gonfiamento |
| | `COLORE_GIOIA` | verde chiaro `(0.70, 1, 0.55)` | bagliore del premio |
| | `GIOIA_INTENSITA` | `0.80` | forza del bagliore (0–1) |
| Schiaccia (negativo) | `SCHIACCIA_QUANTITA` | `0.30` | +30% largo, −30% alto |
| | `SCHIACCIA_DURATA` | `0.30` s | quanto dura lo schiacciamento |
| | `COLORE_ERRORE` | rosso `(1, 0.30, 0.30)` | lampeggio dell'errore |
| | `LAMPEGGIO_ASTRO_DURATA` | `0.20` s | durata del lampeggio rosso quando Astro viene colpito |

Qualche scelta sui numeri. I nomi dei suoni sono **senza estensione** perché è così che li vuole `Resources.Load`. I colori li tengo come `static readonly Color` (non `const`, perché un `Color` non può essere costante in C#): `COLORE_GIOIA` è un verde acceso per il premio, `COLORE_ERRORE` un rosso per lo sbaglio. Le quantità di gonfia e schiaccia sono frazioni: `0.45` vuol dire "+45% di dimensione", `0.30` per lo schiacciamento vuol dire che Astro diventa il 30% più largo e il 30% più basso, l'effetto "pancake".

**Perché l'errore è più gentile del premio.** È la scelta più importante di questo file ed è voluta. `VOLUME_SBAGLIATO` (`0.45`) è circa la metà di `VOLUME_GIUSTO` (`0.80`); anche `SCHIACCIA_QUANTITA` e `SCHIACCIA_DURATA` (`0.30`) sono più piccole di `GONFIA_QUANTITA` e `GONFIA_DURATA` (`0.45`). Nella riabilitazione il feedback deve incoraggiare: l'errore serve a *informare* il paziente che il gesto non era giusto, non a spaventarlo o a punirlo. Quindi lo sbaglio è un segnale più basso, più breve e più piccolo del premio, mentre l'azione giusta è quella che "si sente" di più. È esattamente la logica che ho scritto nel LEGGIMI: il feedback positivo deve pesare più di quello negativo.

---

### Immagini e impostazioni (FabbricaImmagini.cs e Impostazioni.cs)

Questi due file sono la parte "amministrativa" del progetto: uno fabbrica gli sprite a partire dai PNG, l'altro raccoglie in un solo posto tutti i numeri che regolano il gioco. Li tengo separati apposta, così quando voglio cambiare un'immagine o tarare la difficoltà so esattamente dove mettere le mani senza toccare la logica.

#### FabbricaImmagini.cs

Questa è una classe `static`: non la istanzio mai, è solo un contenitore di funzioni che mi restituiscono uno `Sprite`. Il suo compito è uno solo: prendere i file PNG che stanno in `Assets/Resources` e trasformarli in sprite pronti da disegnare.

**Le costanti coi nomi dei file.** In cima ho otto costanti `private const string` (`PERSONAGGIO`, `STELLA`, `PIANETA`, `ASTEROIDE`, `BOMBA`, `ESPLOSIONE`, `PORTA`, `CORIANDOLO`). Ognuna è il nome di un file PNG **senza estensione**, perché è quello che `Resources.Load` si aspetta. Le tengo tutte qui in alto come una piccola "pagina parametri": se voglio cambiare l'immagine della navicella mi basta mettere un altro PNG in `Resources` e aggiornare la stringa `PERSONAGGIO`, senza cercare il nome del file sparso nel codice. Nei commenti segno anche il vecchio nome di ogni cosa (la stella era la caramella, il personaggio era "Astro"): è rimasto da quando il gioco aveva un tema diverso, e serve a non perdersi tra i due nomi.

**I metodi `Crea*`.** Sono le funzioni che il resto del gioco chiama davvero: `CreaAstro`, `CreaCaramella`, `CreaPianetaAmico`, `CreaBomba`, `CreaEsplosione`, `CreaPorta`, `CreaCoriandolo`. Sono tutti uguali: una riga che fa `return Carica(...)` passando la costante giusta. Faccio così per esporre verso l'esterno un nome chiaro e "di gioco" (`CreaBomba`) e nascondere il dettaglio di quale file viene letto: chi mi chiama non deve sapere che dietro c'è `Resources.Load`.

`CreaAsteroide` restituisce la roccia come **immagine intera**: in `Livelli.cs` viene disegnata tutta e scalata in modo uniforme (così resta tonda, non si taglia), e il colore lo decide direttamente lo SpriteRenderer. Per questo qui non passo nessun colore: restituisco solo l'immagine, ci pensa poi chi la usa a scalarla e colorarla.

**Il metodo `Carica` (il motore del file).** È la funzione di supporto che tutti i `Crea*` usano. Fa tre cose:

- **Carica il PNG.** Con `Resources.Load<Texture2D>(nome)` leggo la texture dalla cartella `Resources`. Ricordo che il nome va passato senza `.png`, ed è il motivo per cui le costanti in alto non hanno l'estensione.
- **Normalizza la dimensione.** Questo è il trucco più utile. Calcolo `latoLungo = Mathf.Max(tex.width, tex.height)` e poi passo quel valore come **PPU** (pixel-per-unit) a `Sprite.Create`. Il PPU dice "quanti pixel valgono 1 unità di Unity": se imposto il PPU uguale al lato più lungo, allora il lato più lungo dell'immagine diventa esattamente 1 unità. Il risultato è che immagini di risoluzioni diverse (una da 64px, una da 256px) compaiono in scena grandi più o meno uguali, alte circa 1 unità. Così posso scaricare asset di misure qualsiasi e poi ridimensionarli tutti allo stesso modo con `localScale`, senza dovermi preoccupare di quanti pixel ha davvero ogni file.
- **Fallback magenta.** Se `Resources.Load` torna `null` (file mancante o nome sbagliato) **non blocco il gioco**: scrivo un `Debug.LogWarning` con il nome che cercavo e restituisco un quadrato magenta chiamando `CreaQuadratoPieno(new Color(1f, 0f, 1f))`. Il magenta è il segnale classico di "texture mancante": appena vedo un quadrato fucsia in scena so subito quale immagine non è stata trovata, invece di ritrovarmi con un crash o uno sprite invisibile.

Un dettaglio nella `Sprite.Create`: passo `SpriteMeshType.FullRect`. Di default Unity ritaglia lo sprite seguendo i pixel non trasparenti (mesh "Tight") per risparmiare; con `FullRect` lo sprite resta invece un rettangolo pieno, comportamento semplice e prevedibile quando lo scalo. Il pivot lo metto sempre a `(0.5, 0.5)`, cioè al centro, così quando ruoto o scalo un oggetto lo fa attorno al suo centro.

**Il metodo `CreaQuadratoPieno`.** Questo è l'unico sprite che non viene da un PNG: lo costruisco a mano via codice. Creo un array di `Color` di `16 * 16` pixel, lo riempio tutto con lo stesso colore passato, poi creo una `Texture2D` 16x16 in formato `RGBA32`, ci scrivo i pixel con `SetPixels`, chiamo `Apply()` per "stampare" le modifiche e infine ne faccio uno sprite. Imposto `filterMode = Point` (niente sfocatura tra i pixel) e `wrapMode = Clamp`. Lo uso per due cose: l'alone rosso che pulsa attorno alla bomba (è solo un rettangolo colorato che ingrandisco e faccio variare di trasparenza) e, come visto sopra, il quadrato di ripiego magenta. Non è un disegno, è letteralmente una macchia di colore: per un alone che comunque va scalato e reso semitrasparente non serve di più, quindi evito di sprecare un file PNG.

#### Impostazioni.cs

Anche questa è una classe `static` di sole costanti: nessuna logica, solo i numeri del gioco raccolti in un posto. È la pagina che apro quando il gioco risulta troppo difficile o troppo facile, e tornerà utile per adattarlo a un paziente in riabilitazione. Spiego ogni costante con cosa succede ad alzarla o abbassarla.

| Costante | Valore | Cosa fa | Se la alzo / abbasso |
|---|---|---|---|
| `VITE` | `5` | vite all'inizio di ogni livello | più vite = più margine d'errore, partita più tollerante |
| `TEMPO_LIVELLO` | `60` s | secondi a disposizione per finire il livello | più tempo = meno fretta, utile per chi ha movimenti lenti |
| `RAGGIO_CARAMELLA` | `0.85` | distanza entro cui Astro raccoglie una stellina | più grande = più facile prenderla (basta avvicinarsi) |
| `RAGGIO_PORTA` | `1.80` | distanza entro cui la porta conta come raggiunta | più grande = non serve centrarla con precisione |
| `RAGGIO_BOMBA` | `0.55` | distanza entro cui la bomba ti colpisce | più piccolo = più facile schivarla (la tengo bassa apposta) |
| `PUNTI_CARAMELLA` | `10` | punti guadagnati per ogni stellina | cambia solo il peso del punteggio, non la difficoltà del movimento |
| `PUNTI_PERSI_HIT` | `5` | punti tolti quando vieni colpito da asteroide o bomba | più alto = l'errore "pesa" di più sul punteggio |
| `CARAMELLE_LIV1` | `10` | quante stelline genera a caso il livello 1 | più stelline = più cose da raccogliere, livello più lungo |
| `TEMPO_PRONTI` | `1.5` s | durata della schermata iniziale "PRONTI..." prima del via | più lungo = più tempo per prepararsi al comando prima che parta |
| `COOLDOWN_DANNO` | `1.0` s | invulnerabilità dopo un colpo | più alto = più facile: dopo un errore hai un attimo di respiro in cui un secondo ostacolo non toglie un'altra vita |

I tre raggi (`RAGGIO_CARAMELLA`, `RAGGIO_PORTA`, `RAGGIO_BOMBA`) sono la leva più importante per la difficoltà, perché decidono quanto deve essere preciso il movimento del paziente: allargo quelli "buoni" (caramella e porta) e tengo stretto quello "cattivo" (bomba) per rendere il gioco più gentile senza cambiare nient'altro. Il `COOLDOWN_DANNO` serve lo stesso scopo dal lato degli errori: evita che un singolo momento di difficoltà costi due o tre vite di fila, cosa che in riabilitazione sarebbe frustrante.

---

*Materiale di supporto per l'esame "Sistemi per la Riabilitazione e la Terapia
Assistita". Autore: Filippo Cicirelli.*
