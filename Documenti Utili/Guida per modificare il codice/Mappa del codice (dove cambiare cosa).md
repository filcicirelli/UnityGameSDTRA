# Mappa del codice — dove cambiare cosa 🧭

Questa è la mia "cartina" del progetto. Serve per **trovare in fretta il punto
giusto** quando devo modificare qualcosa, soprattutto se il professore chiede una
modifica all'esame. Tutto il codice sta in `Assets/Codice/`.

> **Idea di fondo**
> Il progetto è diviso **per argomenti**: ogni cosa ha il suo file, e i numeri da
> regolare sono raccolti in poche "pagine dei parametri". Così, per cambiare molti
> comportamenti, non devo riscrivere la logica: **spesso basta cambiare un valore**
> nella pagina dei parametri giusta.

---

## 1. Mappa dei file

| File | Cosa contiene | Quando lo apro |
|---|---|---|
| `Impostazioni.cs` | I **numeri del gioco**: vite, tempo, raggi di presa, punti. | Per rendere il gioco più facile/difficile. |
| `GestoreGioco.cs` | Il **cervello**: punteggio, vite, tempo, livello, vittoria/sconfitta. | Per cambiare *le regole* (cosa succede quando…). |
| `Oggetti.cs` | Tutti gli **oggetti** del gioco: Astro, caramelle, porta, bombe, asteroidi, effetti. | Per cambiare *come si comporta* un oggetto. |
| `Livelli.cs` | I **dati dei livelli** + il **costruttore** che crea gli oggetti in scena. | Per aggiungere/cambiare un livello. |
| `FabbricaImmagini.cs` | Carica le **immagini** (PNG) da `Assets/Resources`. | Per cambiare un'immagine. |
| `InterfacciaGioco.cs` | L'**HUD** e i pannelli (vittoria, game over, menu, pannello F3). | Per cambiare ciò che si vede a schermo durante il gioco. |
| `schermata start/` | La **schermata iniziale** e la scelta del **comando** (mouse/webcam/joystick). | Per cambiare il menu di avvio o i comandi. |
| `comando webcam/` | Il comando con la **webcam** (segue un evidenziatore fluo). | Per regolare la webcam. |
| `feedback paziente/` | Il **feedback** sonoro e visivo (premio/errore) per il paziente. | Per cambiare suoni ed effetti di rinforzo. |

---

## 2. L'ordine dentro ogni file (lo "scheletro")

Quasi tutti i file seguono **lo stesso ordine di lettura**, dall'alto in basso.
Sapendo questo, so sempre **dove guardare** dentro un file:

1. **Commento iniziale** — spiega in poche righe cosa fa il file.
2. **Parametri e variabili** — prima i valori, poi `Istanza` (il riferimento
   unico), poi lo stato del gioco, infine le variabili interne (`private`).
3. **Proprietà calcolate** — valori ricavati al volo (es. `Energia`,
   `BombeAttive`).
4. **`Awake()` / `Start()`** — preparazione iniziale (una volta sola).
5. **`Update()`** — ciò che succede **ad ogni fotogramma** (movimento, controlli).
6. **Metodi pubblici** — i comandi che gli altri file possono chiamare.
7. **Metodi interni e aiutanti** — i dettagli privati (calcoli, disegno…).

> Esempio: in `GestoreGioco.cs` trovo in alto le variabili (punteggio, vite,
> tempo), poi `Start`, poi `Update` (i timer), poi i metodi `Segnala…` che gli
> altri oggetti chiamano, e in fondo i metodi privati come `PerdiUnaVita`.

---

## 3. Le "manopole" — tutti i parametri regolabili

Quasi ogni numero che ha senso cambiare è raccolto in **quattro pagine di
parametri**. Sono file **senza logica**: solo valori da regolare. (Le poche
eccezioni — sfondo, punto di partenza della navicella, effetti di festa — sono
segnalate nelle ricette della sezione 4.)

| Pagina | Regola… |
|---|---|
| `Impostazioni.cs` | il **gioco**: vite, tempo, raggi di presa, punti, caramelle del livello 1, conto "PRONTI", invulnerabilità dopo un colpo. |
| `feedback paziente/ParametriFeedback.cs` | il **feedback**: volumi, interruttori suono/visivo, nomi dei file audio, quanto Astro si gonfia/schiaccia, i colori del premio e dell'errore. |
| `schermata start/ParametriComandi.cs` | il **joystick** (velocità, zona morta, inversione su/giù) e il **dwell** dei pulsanti. |
| `comando webcam/ParametriWebcam.cs` | la **webcam**: colori da seguire, tolleranza, risoluzione, morbidezza, anteprima. |

---

## 4. Ricette: "se il prof chiede X, cambia Y"

Per ogni richiesta tipica, ecco **il file e il punto esatto** da toccare.

### 🎚️ Rendere il gioco più facile (o più difficile)
Tutto in `Impostazioni.cs`:

| Per… | Cambia | Esempio |
|---|---|---|
| Più/meno **vite** | `VITE` | da `5` a `7` (più facile) |
| Più/meno **tempo** | `TEMPO_LIVELLO` | da `60` a `90` (più facile) |
| Prendere le caramelle **da più lontano** | `RAGGIO_CARAMELLA` | da `0.85` a `1.2` |
| Toccare la porta **più facilmente** | `RAGGIO_PORTA` | alza il valore |
| Bombe **meno pericolose** | `RAGGIO_BOMBA` | abbassa il valore |
| Più "**respiro**" dopo un colpo | `COOLDOWN_DANNO` | da `1.0` a `1.5` |
| Più tempo di "**PRONTI…**" prima di iniziare | `TEMPO_PRONTI` | da `1.5` a `3.0` |

> Nota sulle bombe: la bomba esplode a `RAGGIO_BOMBA` **più** il suo
> `raggioPericolo` (un campo della classe `Bomba` in `Oggetti.cs`, di base `0.5`).
> Per la pericolosità agisci su `RAGGIO_BOMBA`; per l'alone d'allarme attorno alla
> bomba, su `raggioPericolo`.

### 🏆 Cambiare i punti
In `Impostazioni.cs`: `PUNTI_CARAMELLA` (punti guadagnati) e `PUNTI_PERSI_HIT`
(punti persi per un colpo).

### 🗺️ Aggiungere un nuovo livello
In `Livelli.cs`, nella classe `DefinizioneLivelli`:
1. aumenta `Conteggio` (es. da `3` a `4`);
2. aggiungi un `case 3: return CostruisciLivello4();` nello `switch` del metodo
   `Ottieni`;
3. scrivi un nuovo metodo `CostruisciLivello4()` copiando uno esistente (es.
   `CostruisciLivello2`) e cambiando titolo, obiettivo, asteroidi e caramelle.

### 🪨 Spostare/aggiungere asteroidi, bombe o caramelle in un livello
Sempre in `Livelli.cs`, dentro `CostruisciLivello2()` o `CostruisciLivello3()`:
- **Asteroidi**: `NuovoAsteroide(centroX, centroY, diametro)`.
- **Bombe**: `l.bombe.Add(new Vector2(x, y));`.
- **Caramelle a posizione fissa**: aggiungi una coppia `new Vector2(x, y)`
  nell'elenco `posizioni`.
- **Caramelle a caso** (come nel livello 1): si regola con
  `Impostazioni.CARAMELLE_LIV1`.

> ℹ️ **Da ricordare:** il **livello 2** e il **livello 3** hanno asteroidi
> **diversi** (ognuno con le sue righe `NuovoAsteroide`): se cambio un livello,
> l'altro non cambia.

### 🖼️ Cambiare un'immagine (la navicella, la stella, la bomba…)
1. metti il nuovo file PNG in `Assets/Resources` (es. `bomba.png`);
2. in `FabbricaImmagini.cs`, in cima, c'è una costante per ogni immagine
   (`PERSONAGGIO`, `STELLA`, `BOMBA`…): controlla che il **nome** corrisponda al
   file (senza `.png`). Tutto qui.

> ⚠️ **Eccezione — lo SFONDO** (la nebulosa, file `sfondo.jpg`) **non** passa da
> `FabbricaImmagini`. Per cambiarlo: metti il nuovo file in `Assets/Resources` e,
> in `Livelli.cs` dentro `CostruisciSfondo()`, aggiorna il nome scritto in
> `Resources.Load<Texture2D>("sfondo")`.

### 🔊 Cambiare un suono
1. metti il nuovo file audio in `Assets/Resources` (es. `vittoria.ogg`);
2. in `feedback paziente/ParametriFeedback.cs` aggiorna il nome:
   `SUONO_RACCOLTA`, `SUONO_VITTORIA` o `SUONO_ERRORE` (senza estensione).
   I **volumi** e l'interruttore `SUONO_ATTIVO` sono nello stesso file.

### 🎨 Cambiare i colori del **gioco** (non quelli del codice)
- **Caramelle**: gli elenchi `COLORI_CARAMELLE` (livelli fissi) e `COLORI_RANDOM`
  (livello 1) in `Livelli.cs`.
- **Asteroidi**: l'elenco `COLORI_ASTEROIDE` nel `CaricatoreLivelli`, in `Livelli.cs`
  (le tinte delle rocce, usate a turno).
- **Astro che brilla/lampeggia**: `COLORE_GIOIA` e `COLORE_ERRORE` in
  `ParametriFeedback.cs`.

### ✨ Rendere il feedback più evidente (o più calmo)
In `feedback paziente/ParametriFeedback.cs`:
- più evidente: alza `GONFIA_QUANTITA` (es. `0.7`) o `GIOIA_INTENSITA`;
- ambiente silenzioso: `SUONO_ATTIVO = false`;
- errore più dolce: abbassa `VOLUME_SBAGLIATO`.

### 🕹️ Regolare i comandi
- **Joystick** (anche frecce/WASD): `schermata start/ParametriComandi.cs`
  (`JOYSTICK_VELOCITA`, `JOYSTICK_ZONA_MORTA`, `JOYSTICK_INVERTI_Y`).
- **Pulsanti senza mouse** (webcam/joystick): `DWELL_SECONDI` nello stesso file.
- **Webcam**: `comando webcam/ParametriWebcam.cs` (i `COLORI_EVIDENZIATORE` da
  seguire, `TOLLERANZA_TINTA`, `MORBIDEZZA`, l'anteprima…).

> **"Fai muovere la navicella più veloce/lenta":** attenzione, con **mouse** e
> **webcam** Astro segue il puntatore **1:1** (subito dov'è il puntatore), quindi
> **non** c'è una velocità da regolare: dipende dalla mano. Solo col **joystick**
> la velocità si imposta con `JOYSTICK_VELOCITA`. Per rendere la webcam più
> **calma** (se la mano trema) si alza `MORBIDEZZA` in `ParametriWebcam.cs`.

### 🚀 Cambiare il punto di partenza della navicella
In `Livelli.cs`, nel metodo `CostruisciAstro()`: la riga
`a.transform.position = new Vector3(0f, 0f, 0f)` (0,0 = centro dello schermo).

> Nota: la **porta** nasce sempre **lontana** da dov'è Astro, quindi spostare il
> punto di partenza cambia un po' anche dove apparirà la porta.

### 🎉 Esplosione, coriandoli e pianeta amico (gli effetti di festa)
Questi numeri **non** stanno nelle pagine dei parametri: sono in cima alle
rispettive classi in `Oggetti.cs`:
- `Esplosione`: `durata` e `scalaMax` (quanto dura e quanto diventa grande);
- `Coriandoli`: `quantita` e `durata` (quanti coriandoli e per quanto tempo);
- `PianetaAmico`: `scalaFinale` (quanto è grande il pianeta finale).

### ➕ Aggiungere una nuova modalità di comando (es. tastiera a parte)
Tocca due file della cartella `schermata start/`:
1. in `Comandi.cs`: aggiungi un valore all'`enum Modalita` e un `case` nello
   `switch` di `PuntatoreSchermo()`;
2. in `SchermataStart.cs`: aggiungi il pulsante (come `BottoneModalita`) e il
   testo in `DescrizioneComando()`.

### 🔍 Cambiare lo "zoom" della telecamera
In `GestoreGioco.cs`, nel metodo `Avvio.Inizia()`: `c.orthographicSize = 6f`
(più alto = si vede di più / più lontano).

### 🧱 Aggiungere un **nuovo tipo di oggetto** (es. un nuovo ostacolo)
È il cambiamento che richiede più lavoro, ma copio lo **stesso schema** che ho già
usato per bombe e caramelle. Tre passi:
1. in `Oggetti.cs`: una nuova classe (es. `class Meteora : MonoBehaviour`) con
   una lista statica `public static List<Meteora> Tutte`, i metodi `OnEnable`/
   `OnDisable` che si aggiungono/tolgono dalla lista, e un metodo `Inizializza`;
2. in `Livelli.cs`: un metodo `CostruisciMeteore(dati)` che crea gli oggetti in
   scena (copiando `CostruisciBombe`);
3. in `Oggetti.cs`, dentro `Astro.Update()`: un controllo `ControllaMeteore()`
   per gestire il contatto (copiando `ControllaBombe`).

### 🖥️ Cambiare ciò che si legge a schermo (HUD)
In `InterfacciaGioco.cs`: il metodo `OnGUI()` decide cosa disegnare; i singoli
pezzi sono nei metodi `Disegna…` (contatori, barra energia, pannelli). Il
**pannello informazioni F3** è in `DisegnaPannelloDebug`.

### 🌀 Cambiare un'animazione (respiro, pulsazioni…)
Le animazioni usano `Mathf.Sin`. Nel codice di un oggetto (in `Oggetti.cs`):
- il numero che **moltiplica** `Mathf.Sin(...)` è l'**ampiezza** (quanto si muove);
- il numero che moltiplica `Time.time` **dentro** il seno è la **velocità**.

---

## 5. Da ricordare (piccole trappole)

- Quando **aggiungo un livello** devo aggiornare **`Conteggio`** *e* lo `switch`
  in `Ottieni`: se mi dimentico uno dei due, il livello non parte.
- Il **nome** scritto nelle costanti (`"bomba"`, `"vittoria"`…) deve essere
  **identico** al file in `Assets/Resources` (senza estensione).
- Gli **asteroidi del livello 2 e 3** sono uguali: vanno cambiati in entrambi.
- I file `.meta` di Unity si creano **da soli** alla prima apertura: non li tocco.

---

*Materiale di supporto per l'esame "Sistemi per la Riabilitazione e la Terapia
Assistita". Autore: Filippo Cicirelli.*
