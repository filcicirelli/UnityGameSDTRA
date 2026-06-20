# Colori del codice — oggetti, funzioni, parametri 🎨

Ho scritto il codice in modo che **ogni tipo di cosa abbia un colore diverso**: gli
oggetti, le funzioni, i dati e i parametri si riconoscono **a colpo d'occhio**.
Questo non cambia il programma: è solo un aiuto per **leggere** e per **spiegare**
il codice all'esame con sicurezza.

> **Perché aiuta all'esame**
> Quando il professore chiede una modifica, il colore mi dice subito dove
> guardare: cerco una **manopola** da regolare? guardo l'**arancione**. Voglio
> cambiare un **comportamento**? cerco una **funzione gialla**. Devo spiegare un
> **oggetto**? è in **verde**. E all'esame posso dire "la classe verde `Astro`" o
> "il metodo giallo `Gonfia`", e si capisce al volo di cosa sto parlando.

---

## La leggenda

| Colore | Categoria (in C#) | Cos'è nel gioco | Esempi dal codice | Come si scrive |
|---|---|---|---|---|
| 🟩 **Verde acqua** | **OGGETTI** — classi ed enum | I "tipi" di cose del gioco | `Astro`, `Caramella`, `Bomba`, `GestoreGioco`, `DatiLivello` | Iniziale **Maiuscola** (`PascalCase`) |
| 🟨 **Giallo** | **FUNZIONI** — metodi | Le **azioni** che il gioco sa fare | `IniziaPartita()`, `CaricaLivello()`, `Gonfia()`, `SegnalaBombaColpita()` | Maiuscola + **parentesi** `()` |
| 🟦 **Azzurro** | **STATO** — campi e proprietà | I dati che **cambiano** durante la partita | `Punteggio`, `Vite`, `LivelloCorrente`, `TempoRimasto` | Iniziale Maiuscola (pubblici) o minuscola (privati) |
| 🟧 **Arancione** | **PARAMETRI** — costanti | Le **manopole** da regolare | `VITE`, `TEMPO_LIVELLO`, `RAGGIO_BOMBA`, `GONFIA_QUANTITA` | Tutto **MAIUSCOLO** con underscore |
| ⬜ **Grigio** | dettagli — parametri e variabili locali | Valori "di passaggio" dentro una funzione | `indice`, `modalita`, `posizione`, `motivo` | minuscola (`camelCase`) |

> **Nota sull'arancione:** l'arancione segna le cose **`static`**, cioè condivise
> da tutto il gioco. Le **manopole** vere e proprie sono quelle in MAIUSCOLO (in
> `Impostazioni.cs` e nelle pagine dei parametri); in arancione finiscono anche i
> **riferimenti unici** come `Istanza`, che sono anch'essi `static`. Sono facili da
> distinguere: le manopole si scrivono in MAIUSCOLO, gli altri no.

A questi si aggiungono i colori che **ogni editor** dà da solo: le **parole chiave**
di C# (`public`, `void`, `if`, `return`…) in blu/viola, il **testo tra virgolette**
(`"vittoria"`, `"GIOCA"`) in un colore caldo, i **numeri** a parte e i **commenti**
(le righe che iniziano con `//`) in verde/grigio spento.

> **Come riconoscere le quattro categorie anche senza colori**
> Il colore segue il **modo di scrivere i nomi**, che nel progetto è sempre lo
> stesso: gli **oggetti** iniziano in Maiuscola, le **funzioni** hanno le `()`, i
> **parametri** regolabili sono in MAIUSCOLO, le variabili "di passaggio" in
> minuscolo. Quindi la leggenda funziona anche su carta o su un editor diverso.

---

## Come sono attivati (VS Code)

I colori sono già impostati nel file `.vscode/settings.json` del progetto, nella
sezione `editor.semanticTokenColorCustomizations`. VS Code li applica da solo
appena apro la cartella (servono l'estensione C#/Unity, già consigliata in
`.vscode/extensions.json`, e un tema **scuro** come *Dark+*).

Se dovessi rimetterli a mano (per esempio su un altro computer), basta incollare
questo dentro `.vscode/settings.json`:

```jsonc
"editor.semanticHighlighting.enabled": true,
"csharp.semanticHighlighting.enabled": true,
"editor.semanticTokenColorCustomizations": {
    "enabled": true,
    "rules": {
        "class": "#4EC9B0", "struct": "#4EC9B0", "enum": "#4EC9B0", "interface": "#4EC9B0",
        "method": "#DCDCAA",
        "property": "#9CDCFE", "field": "#9CDCFE",
        "field.static": "#FFB454", "property.static": "#FFB454", "field.readonly": "#FFB454",
        "parameter": "#C8C8C8"
    }
}
```

| Colore | Codice | Categoria |
|---|---|---|
| 🟩 verde acqua | `#4EC9B0` | oggetti (classi/enum) |
| 🟨 giallo | `#DCDCAA` | funzioni (metodi) |
| 🟦 azzurro | `#9CDCFE` | stato (campi/proprietà) |
| 🟧 arancione | `#FFB454` | parametri/costanti e cose `static` (condivise) |
| ⬜ grigio | `#C8C8C8` | parametri delle funzioni |

---

## Sugli altri editor

- **Visual Studio** (Windows, quello che probabilmente userà il professore)
  distingue **già da solo** queste categorie con colori propri: classi, metodi,
  campi e costanti hanno tinte diverse. Si possono cambiare da
  *Strumenti → Opzioni → Ambiente → Tipi di carattere e colori*, ma non serve:
  la **leggenda qui sopra vale lo stesso**, perché i nomi sono scritti in modo
  coerente (Maiuscola per gli oggetti, `()` per le funzioni, MAIUSCOLO per i
  parametri).
- **Su carta / nella tesina**: uso gli stessi simboli colorati
  (🟩 oggetti, 🟨 funzioni, 🟦 stato, 🟧 parametri) così la leggenda è la stessa
  ovunque.

---

## Come la uso all'esame (riassunto)

1. **Devo regolare il gioco?** → cerco l'**arancione** (le costanti), quasi sempre
   in `Impostazioni.cs` o in una pagina dei parametri.
2. **Devo cambiare un comportamento?** → cerco la **funzione gialla** giusta
   (es. `SegnalaBombaColpita()`).
3. **Devo spiegare un oggetto?** → parto dal suo nome in **verde** (es. `Astro`).
4. **Devo seguire un dato?** → lo riconosco in **azzurro** (es. `Punteggio`).

---

*Materiale di supporto per l'esame "Sistemi per la Riabilitazione e la Terapia
Assistita". Autore: Filippo Cicirelli.*
