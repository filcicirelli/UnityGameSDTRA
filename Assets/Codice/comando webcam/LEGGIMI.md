# Comando con la Webcam 📷✋

Questa cartella aggiunge una **modalità di gioco**: invece di muovere Astro
con il **mouse**, lo si muove **muovendo il dito davanti alla webcam**.

Si sceglie dalla **schermata iniziale** (pulsante "DITO (WEBCAM)") — vedi la
cartella `schermata start`.

| | Modalità mouse | Modalità dito (webcam) |
|---|---|---|
| Come muovo Astro | con il puntatore del mouse | muovendo il dito davanti alla webcam |
| Cosa serve | niente | un **oggetto colorato** sulla punta del dito |

> **Perché serve nella riabilitazione**
> Comandare il gioco **con il movimento del braccio/dito nello spazio** (invece che
> con piccoli movimenti del polso sul mouse) allena gesti **ampi e controllati**.
> È regolabile: il parametro `MORBIDEZZA` calma il puntatore se la mano **trema**,
> e l'**anteprima con il mirino** dà al paziente un riscontro immediato di dove sta
> puntando. Resta sempre disponibile anche il mouse, così il gioco non si blocca mai.

---

## Come funziona davvero (in modo onesto)

Riconoscere *davvero* un dito o una mano richiede l'**intelligenza artificiale**
(librerie pesanti e non "scolastiche"). Qui usiamo una tecnica più semplice,
robusta e completamente **fatta da codice**, in linea con il resto del progetto
(nessun file o libreria esterna): **seguiamo un colore acceso**.

Il paziente mette sulla punta del dito un **oggetto colorato** — un ditale, un
adesivo, il cappuccio di un pennarello, di default **arancione**. Per ogni
fotogramma della webcam il programma:

1. **guarda i pixel** dell'immagine (uno ogni `PASSO_ANALISI`, per andare veloce);
2. tiene quelli del **colore giusto** — controlla la *tinta*, così funziona anche
   se cambia la luce, scartando i grigi e le zone troppo scure;
3. ne calcola il **centro** (la media delle posizioni): quello è il **dito**;
4. trasforma quel centro nella **posizione del puntatore** sullo schermo, con un
   effetto **specchio** (muovi a destra → punti a destra) e un movimento **morbido**.

Da lì in poi Astro segue quel punto **esattamente come seguiva il mouse**: tutto il
resto del gioco (caramelle, chiave, porta, bombe, feedback) funziona identico.

---

## Il corpo del codice (spiegato in breve)

Due file di codice, come per il feedback paziente:

### 1. `ParametriWebcam.cs` — la *pagina dei valori*
È l'unico file da toccare per regolare la modalità (come `Impostazioni.cs` per il
gioco). Contiene **solo numeri/valori**, nessuna logica: il **colore da seguire** e
quanto può variare, la risoluzione della webcam, l'effetto specchio, la morbidezza
del movimento e l'anteprima. Tabella più sotto.

### 2. `ComandoWebcam.cs` — il *cervello* della modalità
È un oggetto che **si installa da solo** all'avvio (non va trascinato in scena),
esattamente come `FeedbackPaziente`. Si occupa di:
- **accendere la webcam** quando la modalità "dito" è attiva (chiedendo prima il
  permesso, obbligatorio su alcuni sistemi/browser) e di **metterla in pausa**
  quando si gioca col mouse o col joystick;
- **analizzare ogni fotogramma** per trovare il centro del colore (vedi sopra);
- **esporre la posizione del dito** (`ComandoWebcam.Posizione`) e se lo vede
  (`ComandoWebcam.DitoVisto`): è `Comandi` a passarla ad Astro;
- **disegnare l'anteprima** della webcam in un angolo, con un **mirino** che diventa
  verde quando vede il dito e rosso quando non lo vede.

---

## Come è collegato al resto del gioco

La scelta del comando passa tutta dall'hub `Comandi` (cartella `schermata start`),
così Astro non è cambiato per ogni modalità: chiede solo *"dove sta il puntatore?"*.

| File | Cosa fa |
|---|---|
| `Comandi.PuntatoreSchermo()` | se la modalità è "dito" usa `ComandoWebcam.Posizione` |
| `ComandoWebcam` | calcola quella posizione seguendo il colore |

Se la webcam manca o il permesso viene negato, `Comandi` torna automaticamente al
**mouse** e il gioco continua senza bloccarsi.

---

## 📋 Pagina dei parametri modificabili (`ParametriWebcam.cs`)

| Variabile | Valore | Cosa fa |
|---|---|---|
| **Colore da seguire** | | |
| `COLORE_DA_SEGUIRE` | arancione | il colore dell'oggetto sulla punta del dito |
| `TOLLERANZA_TINTA` | `0.08` | quanto la tinta può variare ed essere accettata (0–0.5) |
| `SATURAZIONE_MINIMA` | `0.35` | quanto dev'essere acceso il colore (scarta i grigi) |
| `LUMINOSITA_MINIMA` | `0.25` | quanto dev'essere luminoso (scarta le ombre) |
| **Webcam** | | |
| `LARGHEZZA_RICHIESTA` | `320` | larghezza dell'immagine (bassa = veloce) |
| `ALTEZZA_RICHIESTA` | `240` | altezza dell'immagine |
| `FPS_RICHIESTI` | `30` | fotogrammi al secondo della webcam |
| `SPECCHIA` | `true` | effetto specchio (destra → destra) |
| `PASSO_ANALISI` | `2` | analizzo 1 pixel ogni N (più alto = più veloce) |
| `PIXEL_MINIMI` | `12` | quanti pixel colorati servono per "vedere" il dito |
| **Movimento** | | |
| `MORBIDEZZA` | `0.5` | 0 = scattoso e reattivo, verso 1 = morbido e calmo |
| **Anteprima** | | |
| `MOSTRA_ANTEPRIMA` | `true` | mostra in un angolo cosa vede la webcam |
| `ANTEPRIMA_LARGHEZZA` | `240` | larghezza dell'anteprima in pixel |
| `ANTEPRIMA_MARGINE` | `20` | distanza dell'anteprima dal bordo |

### Esempi di regolazione
- Il paziente ha un oggetto **rosso/verde/blu**: cambia `COLORE_DA_SEGUIRE`.
- Il colore **non viene riconosciuto bene**: alza un po' `TOLLERANZA_TINTA` (es. `0.12`)
  o abbassa `SATURAZIONE_MINIMA`.
- Vengono riconosciuti **troppi puntini** sbagliati: alza `PIXEL_MINIMI` o
  abbassa `TOLLERANZA_TINTA`.
- La mano **trema**: alza `MORBIDEZZA` verso `0.8`.
- PC **lento**: alza `PASSO_ANALISI` a `3`–`4`.

---

## Consigli pratici per usarla
- Usa un oggetto colorato **acceso e uniforme** (un ditale, un adesivo, un tappo).
- Evita di indossare/avere alle spalle **oggetti dello stesso colore** del marker.
- Una **luce buona** aiuta molto il riconoscimento.
- Guarda l'**anteprima**: quando il mirino è **verde** il dito è tracciato bene.

## Note tecniche
- **Nessun file o libreria esterna**: la webcam è gestita con `WebCamTexture`, di serie
  in Unity; l'analisi del colore è pochi cicli `for` su `Color.RGBToHSV`.
- I file `.meta` di Unity per questa cartella e per i nuovi script vengono generati
  **automaticamente** la prima volta che apri il progetto in Unity.
- Se sul tuo computer l'immagine dell'anteprima risultasse **capovolta o ruotata**
  (capita con alcune webcam), è una questione di orientamento del dispositivo:
  il tracciamento del colore continua comunque a funzionare.
