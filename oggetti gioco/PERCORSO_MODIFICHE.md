# Percorso delle modifiche: dalle immagini "disegnate da codice" alle immagini vere

Questo documento spiega **passo per passo** come ho sostituito le immagini
generate via codice con immagini vere (file PNG), così è facile rispondere
se il professore chiede "come hai fatto / perché".

---

## 1. Punto di partenza

Prima, quasi tutti gli oggetti del gioco erano **disegnati pixel per pixel
dentro il codice**, nel file `Assets/Codice/FabbricaImmagini.cs`: ogni
immagine era una griglia 16×16 fatta di lettere (una lettera = un colore).
Erano disegnati così: Astro, caramella, porta, bomba, pianeta, asteroide,
esplosione.

Obiettivo: usare **immagini vere** (più belle e professionali) e **togliere
il codice che le disegnava** (che non serve più).

---

## 2. Da dove vengono le immagini (tutte gratuite, licenza CC0)

Le immagini arrivano da pacchetti gratuiti e liberi anche per uso commerciale
(CC0 = pubblico dominio, nessuna attribuzione obbligatoria). Dettaglio completo
e link nel file **LEGGIMI.txt**. In breve:

| Oggetto | File usato | Pacchetto / Fonte |
|---|---|---|
| personaggio | `personaggio.png` | Kenney – Space Shooter Redux |
| stella (collezionabile) | `stella.png` (stella argento) | Kenney – Space Shooter Redux |
| pianeta | `pianeta.png` | Kenney – Planets |
| asteroide | `asteroide.png` | Kenney – Space Shooter Redux |
| bomba | `bomba.png` | "Bomb sprite" (OpenGameArt) |
| esplosione | `esplosione.png` | Kenney – Particle Pack (lavorata, vedi §5b) |
| coriandolo | `coriandolo.png` (stessa stella) | Kenney – Space Shooter Redux |
| porta | `porta.png` | "Portals" (OpenGameArt) |

Le ho prima raccolte e rinominate in italiano nella cartella **`oggetti gioco/`**.

---

## 3. Dove ho messo le immagini nel progetto

Unity carica le immagini "a runtime" solo se stanno in una cartella speciale
chiamata **`Assets/Resources/`**. Quindi ho **copiato lì** gli 8 PNG usati dal
gioco, con i nomi italiani:

```
Assets/Resources/personaggio.png
Assets/Resources/stella.png
Assets/Resources/pianeta.png
Assets/Resources/asteroide.png
Assets/Resources/bomba.png
Assets/Resources/esplosione.png
Assets/Resources/porta.png
Assets/Resources/coriandolo.png
Assets/Resources/sfondo.jpg   (lo sfondo, c'era già)
```

Ho anche **rimosso** le due vecchie immagini non più usate
(`shipBlue_manned.png` e `laserBeige_burst.png`).

---

## 4. Cosa ho cambiato nel codice (file per file)

### a) `Assets/Codice/FabbricaImmagini.cs` — riscritto
- **Tolto** tutto il codice che disegnava le immagini: la "legenda colori",
  i 7 disegni a lettere e le funzioni `Disegna` / `DisegnaColorato`
  (e le funzioni per schiarire/scurire i colori). Era il grosso del file.
- **Tenuto/aggiunto** un unico metodo `Carica(nome)` che legge il PNG da
  `Assets/Resources` con `Resources.Load` e lo trasforma in immagine
  (`Sprite.Create`).
- In cima al file ci sono ora **i nomi dei file come "costanti"**
  (`PERSONAGGIO`, `STELLA`, `PIANETA`, ...): è la **pagina parametri**
  centralizzata. Per cambiare un'immagine basta cambiare un nome qui.
- I metodi che il resto del gioco chiama (`CreaAstro`, `CreaBomba`, ...)
  sono rimasti **con lo stesso nome**, così non ho dovuto cambiare tutto il
  resto del codice: dentro, ora, caricano il file invece di disegnare.

### b) `Assets/Codice/Livelli.cs` — barriere di asteroidi
La barriera-asteroide è lunga e stretta. Se "stiracchiassi" una roccia tonda
verrebbe un ovale brutto. Allora **ripeto la stessa roccia a mosaico** usando
una funzione di Unity (`SpriteRenderer.drawMode = Tiled` con `size` = grandezza
della barriera). Il **colore** della roccia (che cambia da un livello all'altro)
ora lo metto con `SpriteRenderer.color`.

### c) `Assets/Codice/Oggetti.cs` — due ritocchi
- **Asteroide**: siccome ora la grandezza la decide lo SpriteRenderer
  (mosaico + `size`), la scala dell'oggetto resta 1. Il "rettangolo" che serve
  a capire se il giocatore tocca la barriera (la collisione) è rimasto identico.
- **Coriandoli**: i coriandoli della festa ora usano la stellina
  (`coriandolo.png`) colorata, invece dei quadratini di prima.

---

## 5. Due scelte tecniche (utili da spiegare)

1. **Asteroide "a mosaico"**: una sola immagine di roccia ripetuta tante volte
   fa una barriera bella di qualsiasi lunghezza, senza deformare la roccia.

2. **Stella argento (chiara)**: la stellina da raccogliere viene **colorata**
   dal gioco a ogni livello. Una stella *dorata* moltiplicata per un colore
   (es. blu) diventerebbe scura/sporca; una stella *chiara* invece prende bene
   ogni colore. Per questo come stella principale uso quella argento.

3. **Rete di sicurezza**: se per errore mancasse un file immagine, il gioco
   **non si blocca**: mostra un quadrato magenta (il classico segnale
   "immagine non trovata") e scrive un avviso. Così l'errore è evidente ma il
   gioco gira lo stesso.

---

## 5b. Scelta delle grafiche più semplici e leggibili (controllo visivo)

Non ho scelto le immagini solo dal nome: le ho **aperte e guardate una per una**,
tenendo il criterio "una forma chiara e singola per ogni oggetto, su sfondo
trasparente". Così ho corretto 4 cose:

- **porta**: il file scaricato (`portalRings`) era un *foglio di animazione*
  (tanti fotogrammi in griglia): come immagine singola avrebbe mostrato una
  griglia di porticine. Ho **ritagliato un solo fotogramma** (un anello pulito).
- **esplosione**: gli sprite del Particle Pack hanno lo **sfondo nero pieno**
  (sono pensati per i "sistemi di particelle", non come immagini normali): da
  sprite normale si vedrebbe un quadrato nero. Ho **reso trasparente il nero e
  l'ho colorata a fiammata** (arancio/giallo), così si capisce subito che è uno
  scoppio.
- **pianeta**: tra i 10 pianeti del pacchetto ho scelto quello **tipo Terra**,
  il più immediato da riconoscere come "pianeta".
- **coriandoli**: usano la stessa **stella chiara trasparente** della stellina
  (colorata a runtime), per evitare lo sfondo nero degli sprite del Particle
  Pack.

In breve: personaggio = navicella semplice, stella = stella a 5 punte, pianeta =
Terra, asteroide = sasso grigio, bomba = bomba con miccia, esplosione = fiammata,
porta = anello luminoso, coriandoli = stelline colorate. Tutte forme nette.

---

## 6. Come ho verificato che funziona

Ho fatto una **compilazione di controllo** di Unity senza aprire l'editor
(modalità "batch"), che controlla che il codice C# sia corretto:

```
Unity -batchmode -quit -nographics \
  -projectPath "<cartella del progetto>" \
  -logFile /tmp/unity_compile.log \
  -executeMethod UnityEditor.SyncVS.SyncSolution
```

Risultato: **0 errori, 0 warning, 0 eccezioni**. La compilazione C# è uguale
su Mac e Windows con la stessa versione di Unity (6000.4.7f1), quindi compila
pulito anche sul computer del professore.

---

## 7. Come cambiare un'immagine in futuro (semplicissimo)

1. Metti il nuovo PNG in `Assets/Resources/` (es. `pianeta.png`).
2. Se vuoi un nome diverso, aggiorna la costante in cima a
   `FabbricaImmagini.cs` (es. `PIANETA = "pianeta"`).

Fine: niente codice da riscrivere.

---

## 8. Riepilogo "domande tipiche del prof"

- **Perché in `Assets/Resources`?** Perché è l'unica cartella da cui Unity sa
  caricare file su richiesta mentre il gioco gira (`Resources.Load`).
- **Perché avevi disegni nel codice e ora no?** All'inizio non avevo immagini
  pronte, così le disegnavo da codice. Ora uso immagini vere (CC0) e ho tolto
  il codice dei disegni perché era superfluo.
- **Le immagini sono libere?** Sì, tutte CC0 (pubblico dominio): usabili anche
  per scopi commerciali senza dover citare nessuno (vedi LEGGIMI.txt).
- **Hai cambiato il funzionamento del gioco?** No: livelli, movimenti,
  collisioni e regole sono identici. È cambiato solo *l'aspetto* degli oggetti.
