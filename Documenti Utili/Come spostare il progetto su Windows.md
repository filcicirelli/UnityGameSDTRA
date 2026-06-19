# Come spostare il progetto su un altro computer (es. Windows)

Questa guida spiega come portare il gioco "ASTRO" da questo Mac a un altro
computer (per esempio Windows) e farlo funzionare **senza problemi**, usando
sempre il programma **Unity**.

Il progetto e' stato scritto apposta per essere facile da spostare: tutti i file
che servono (immagini, suoni, sfondo) stanno dentro la cartella `Assets`, in
`Assets/Resources/`, quindi non ci sono file sparsi che si possono perdere. I
livelli, invece, sono definiti dal codice.


## 1) Cosa serve sul computer nuovo

1. **Unity Hub** (gratuito, dal sito ufficiale unity.com).
2. **La stessa identica versione di Unity** usata qui:

   ```
   Unity 6000.4.7f1
   ```

   > Importante: usa **proprio** questa versione. Aprendo il progetto con una
   > versione diversa, Unity potrebbe "aggiornarlo" e cambiare qualche file.
   > In Unity Hub si possono installare piu' versioni affiancate.

   (La versione e' scritta anche nel file `ProjectSettings/ProjectVersion.txt`.)


## 2) Quali cartelle copiare

Copia la cartella del progetto **intera**. Se vuoi farla piu' leggera, bastano
queste cartelle/file (sono quelli che contano davvero):

| Copiare SEMPRE              | A cosa serve                                  |
|-----------------------------|-----------------------------------------------|
| `Assets/`                   | Tutto il codice, lo sfondo e la scena         |
| `Packages/`                 | L'elenco dei moduli di Unity usati            |
| `ProjectSettings/`          | Le impostazioni del progetto                  |

Puoi anche copiare `UserSettings/` (preferenze personali dell'editor), ma non e'
indispensabile.

### Cartelle che NON serve copiare (si rigenerano da sole)

Queste le ricrea Unity da solo alla prima apertura. Se le copi non e' un
problema, ma rendono la cartella enorme e inutilmente pesante:

- `Library/`  (la piu' grande: cache di Unity)
- `Temp/`
- `obj/`
- `Logs/`
- i file `*.csproj`, `*.sln`, `*.slnx` (progetti per l'editor di codice)

> Consiglio: per una copia pulita e leggera, cancella `Library`, `Temp` e `obj`
> prima di copiare. Unity li ricostruisce automaticamente.


## 3) Come aprire e avviare il gioco

1. Apri **Unity Hub** sul computer nuovo.
2. Premi **Add** (o **Apri**) e scegli la cartella del progetto.
3. Apri il progetto: la prima volta Unity impiega qualche minuto a
   ricostruire la cache (`Library`). E' normale.
4. In alto, premi il tasto **Play (▶)**: il gioco parte dalla schermata
   iniziale, dove si sceglie il comando (mouse, dito o joystick).

> Il gioco si "installa da solo" all'avvio (telecamera, oggetti, suoni): non
> serve trascinare niente nella scena. Funziona anche se apri una scena diversa.


## 4) La webcam su Windows (modalita' "dito")

La modalita' che segue l'evidenziatore fluo con la webcam funziona anche su
Windows. Solo due cose da sapere:

- La **prima volta** Windows puo' chiedere il permesso di usare la fotocamera:
  rispondi **Si / Consenti**. (Su Windows il permesso si controlla anche da
  *Impostazioni → Privacy → Fotocamera*.)
- Se non c'e' nessuna webcam, o il permesso viene negato, **il gioco non si
  blocca**: continua a funzionare con il **mouse** e mostra un messaggio.


## 5) Perche' funziona senza problemi (in breve)

- **Niente percorsi fissi del computer.** Nel codice non ci sono percorsi tipo
  `C:\...` o `/Users/...`: Unity trova i file da solo, su qualsiasi sistema.
- **Tutti i file stanno dentro `Assets/`.** Immagini e suoni (CC0) sono in
  `Assets/Resources/`; non ci sono file sparsi da perdere o da ritrovare.
- **I nomi combaciano.** Ogni nome scritto nel codice (es. `"sfondo"`, `"asteroide"`)
  corrisponde esattamente al file (`sfondo.jpg`, `asteroide.png`), quindi tutto si
  carica anche sui sistemi che distinguono maiuscole/minuscole.
- **Nessun carattere "strano" nel codice.** Il testo a schermo usa solo simboli
  comuni a tutti i sistemi (per esempio il cuore "♥" delle vite), cosi' si vede
  uguale su Windows e su Mac.
- **Fine-riga uniformi.** Il file `.gitattributes` tiene i file di testo coerenti
  tra Windows e Mac, se sposti il progetto con git.


## 6) Se qualcosa non va

| Problema                                  | Soluzione                                              |
|-------------------------------------------|-------------------------------------------------------|
| Unity chiede di "aggiornare" il progetto  | Hai una versione diversa: installa la **6000.4.7f1**. |
| Lo sfondo e' nero                          | Controlla che ci sia `Assets/Resources/sfondo.jpg`.   |
| La webcam non parte                        | Dai il permesso fotocamera; intanto si gioca col mouse. |
| Errori strani alla prima apertura          | Chiudi Unity, cancella `Library` e riapri il progetto. |
