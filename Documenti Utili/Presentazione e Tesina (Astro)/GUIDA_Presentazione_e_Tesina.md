# Guida — Presentazione e Tesina del progetto «Astro» 🎓

Questa cartella contiene il materiale per **esporre il progetto al professore**.

| File | Cos'è | Come si usa |
|---|---|---|
| **Presentazione_Astro.pptx** | Le **slide** da proiettare all'esame (schemi a blocchi, niente codice) | Apri con PowerPoint / Keynote / Google Slides |
| **Tesina_Astro.docx** | La **relazione scritta** (testo + immagini + bibliografia) | Apri con Word; aggiorna l'indice con **F9** |
| **GUIDA_…​.md** | Questo file | Istruzioni per presentare |

> Il file `Base PP Politecnico.pptx` di partenza **non è stato modificato**: la presentazione è un file nuovo, così non perdi l'originale.

---

## 1) Prima cosa da fare: riempire i campi vuoti

Sia nella **copertina del PP** sia nel **frontespizio della tesina** ci sono spazi da completare:
- **Studente**, **Relatore**, **Corso**, **Anno Accademico**.

Nella tesina, dopo aver scritto, **clicca sull'indice e premi F9** (→ «Aggiorna intero sommario») per generarlo in automatico.

---

## 2) Le immagini (importante)

La tesina contiene dei **segnaposto tratteggiati** del tipo *«🖼 Figura 2.1 — …»*.
Sostituiscili con **schermate vere del gioco**. Schermate consigliate da catturare:

1. Schermata iniziale con la scelta del comando (mouse / dito / joystick).
2. Una partita: Astro, le stelline, lo sfondo.
3. I tre livelli (anche tre catture separate).
4. La modalità **webcam** con l'anteprima e il mirino.
5. Il **feedback positivo** (Astro che si gonfia) e la **vittoria** (pianeta + coriandoli).
6. Lo **schema a blocchi** dell'architettura: puoi esportarlo dal PP (slide «Schema a blocchi — 2»: tasto destro → salva come immagine).

Su Windows: **Win+Shift+S**. Su Mac: **Cmd+Shift+4**.

---

## 3) Come esporre il PP (filo del discorso)

Ogni slide ha già delle **note del relatore** (in PowerPoint: menu *Visualizza → Note*) con cosa dire. In sintesi:

1. **Copertina** – «Ho costruito da zero un videogioco che aiuta un paziente a fare riabilitazione motoria.»
2. **Indice** – prima *come l'ho fatto*, poi *a cosa serve*.
3. **Il progetto** – il problema (esercizi ripetitivi e noiosi) e l'idea (renderli un gioco).
4. **Come funziona** – il concept in 4 passi.
5. **Schema a blocchi 1 — flusso di gioco** – le due fasi (stelline → porta), i tre livelli, la gestione degli errori.
6. **Schema a blocchi 2 — architettura** – ogni modulo fa una cosa sola; «Comandi» è il punto unico.
7. **Modalità di comando** – mouse / webcam / joystick e perché contano per la terapia.
8. **Webcam** – il punto forte: *inseguo un colore*, senza IA né librerie esterne.
9. **Feedback** – risposta immediata su due sensi: rinforza il gesto corretto.
10. **Livelli e parametri** – difficoltà e accessibilità regolabili senza programmare.
11. **Il lavoro svolto** – riepilogo di cosa hai realizzato (senza codice).
12. **La riabilitazione** – di che terapia si tratta e come il gioco la supporta.
13. **Perché i giochi aiutano** – vantaggi ed evidenze dalla letteratura.
14. **La tesina** – come è strutturata la relazione (collega PP e tesina).
15. **Conclusioni** – cosa funziona e dove si può arrivare.
16. **Chiusura** – grazie + domande.

**Durata**: con ~16 slide punta a **10–12 minuti** (≈40 secondi a slide; più tempo sui due schemi a blocchi).

---

## 4) Domande probabili del professore (preparati una risposta)

- **«Perché non usi l'intelligenza artificiale per la mano?»** → Perché serve un riconoscimento robusto e fatto da codice: inseguire un colore acceso è semplice, affidabile e senza dipendenze esterne. La pelle è meno satura dell'evidenziatore, quindi il viso non viene confuso.
- **«Come misureresti se funziona davvero?»** → Salvando metriche oggettive (tempi, errori, ampiezza dei movimenti) e confrontando le sessioni; poi uno studio pilota con i terapisti.
- **«Che ruolo ha il terapista?»** → Sceglie la modalità di comando e tara i parametri (difficoltà, anti-tremore, volumi) sul singolo paziente.
- **«Da dove vengono grafica e suoni?»** → Sono asset gratuiti **CC0** (pubblico dominio), inclusi nel progetto in `Assets/Resources`; la **logica** (livelli, tracciamento della webcam, feedback) è invece tutta scritta da me in C#. Nessuna dipendenza o libreria esterna: applicazione leggera e portabile (Windows/Mac).

---

## 5) Rigenerare i file (per chi vuole modificarli da codice)

I due documenti sono generati da script Python (`build_pptx.py`, `build_docx.py`) con le librerie
`python-pptx` e `python-docx`. Per rigenerarli basta rieseguire gli script; i contenuti scientifici
stanno nei file `lit_ppt.json` (slide) e `lit_docx.json` (tesina), nella cartella `sorgenti/`.
