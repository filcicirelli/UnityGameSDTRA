# Percorso delle modifiche: dai suoni "creati da codice" ai suoni veri

Stesso lavoro fatto per le immagini, ma per l'audio di feedback. Spiega
**passo per passo** come ho sostituito i suoni sintetizzati da codice con file
audio veri, così è facile rispondere se il professore chiede "come / perché".

---

## 1. Punto di partenza

I 3 suoni di feedback erano **generati da codice**, nel file
`Assets/Codice/feedback paziente/FabbricaSuoni.cs`: il codice costruiva onde
sonore (sinusoidi) mettendo in fila delle note in Hz scritte in
`ParametriFeedback.cs`. Erano:

| Evento | Quando | Com'era (da codice) |
|---|---|---|
| raccolta | raccogli una stellina | 3 note che salgono (do-mi-sol) |
| vittoria | livello completato | fanfara di 4 note |
| errore | azione sbagliata | 2 note che scendono, "ruvide" |

Obiettivo: usare **suoni veri** (più ricchi) e **togliere il codice** che li
sintetizzava.

---

## 2. Da dove vengono i suoni (gratuiti, CC0)

Da pacchetti audio gratuiti **Kenney** (lo stesso autore della grafica), tutti
**CC0** (pubblico dominio, anche uso commerciale, nessuna attribuzione). Vedi
`LEGGIMI.txt` per i link. In breve:

| Evento | File usato | Pacchetto |
|---|---|---|
| raccolta | `raccolta.ogg` (confirmation_001) | Kenney – Interface Sounds |
| vittoria | `vittoria.ogg` (jingles_PIZZI12, pizzicato) | Kenney – Music Jingles |
| errore | `errore.ogg` (error_007) | Kenney – Interface Sounds |

Li ho prima raccolti e rinominati in italiano nella cartella **`suoni gioco/`**.

---

## 3. Dove ho messo i suoni nel progetto

Come per le immagini, Unity carica i file "a richiesta" solo dalla cartella
**`Assets/Resources/`**. Quindi ho copiato lì i 3 file audio:

```
Assets/Resources/raccolta.ogg
Assets/Resources/vittoria.ogg
Assets/Resources/errore.ogg
```

Unity importa i file `.ogg` come "AudioClip" senza nessuna conversione.

---

## 4. Cosa ho cambiato nel codice

### a) Rimosso `FabbricaSuoni.cs`
Era il file che disegnava le onde sonore da codice. Non serve più: eliminato.

### b) `ParametriFeedback.cs` — la "pagina parametri"
- **Tolto**: le note in Hz (`NOTE_CARAMELLA`, `NOTE_VITTORIA`, `NOTE_ERRORE`) e
  le durate delle note (non servono più).
- **Aggiunto**: i **nomi dei file audio** come costanti
  (`SUONO_RACCOLTA`, `SUONO_VITTORIA`, `SUONO_ERRORE`). Per cambiare un suono
  basta cambiare un nome qui.
- **Tenuto**: volumi, interruttori (suono/visivo on-off) e i parametri degli
  effetti visivi (Astro che si gonfia/schiaccia): non sono stati toccati.

### c) `FeedbackPaziente.cs`
Nel metodo `Awake`, dove prima creava i suoni con `FabbricaSuoni.CreaMelodia(...)`,
ora li **carica dai file** con `Resources.Load<AudioClip>(...)`. Tutto il resto
(quando suonare, volumi, effetti su Astro) è rimasto identico.

Rete di sicurezza: se un file audio mancasse, il suono resta "vuoto" e il gioco
**non si blocca** (semplicemente quel feedback non parte), con un avviso nel log.

---

## 5. Come ho scelto i suoni (senza poterli ascoltare)

Da questo ambiente non posso riprodurre l'audio. Allora ho scelto in modo
**misurato**: con un piccolo script ho calcolato, per ogni candidato, la
**durata** e l'**andamento del suono** (se l'altezza sale = allegro, se scende =
negativo). Così:
- **raccolta** = breve e in salita → conferma positiva;
- **vittoria** = jingle "pizzicato" morbido e in salita → allegro ma gentile
  (in un secondo momento ho sostituito l'8-bit, troppo stridulo, con questo
  pizzicato più piacevole; misurando il "timbro" l'8-bit aveva frequenze molto
  più alte/aspre);
- **errore** = breve e in leggera discesa → negativo ma gentile (adatto alla
  riabilitazione: deve segnalare, non spaventare).

Il giudizio "suona bene?" finale lo farà chi apre Unity. In `suoni gioco/varianti/`
ho lasciato molte alternative già pronte, così cambiare un suono è immediato.

---

## 6. Come ho verificato

Compilazione di controllo headless di Unity (come per le immagini):

```
Unity -batchmode -quit -nographics -projectPath "<progetto>" \
  -logFile /tmp/unity_compile.log -executeMethod UnityEditor.SyncVS.SyncSolution
```

Risultato: **0 errori, 0 warning, 0 eccezioni**, e i 3 `.ogg` importati come
AudioClip.

---

## 7. Domande tipiche del prof

- **Perché in `Assets/Resources`?** È l'unica cartella da cui Unity carica file
  su richiesta mentre il gioco gira (`Resources.Load`), uguale alle immagini.
- **Perché prima generavi i suoni da codice?** All'inizio non avevo file audio,
  così li sintetizzavo. Ora uso suoni veri (CC0) e ho tolto il codice superfluo.
- **I suoni sono liberi?** Sì, tutti CC0 (pubblico dominio).
- **Hai cambiato il funzionamento?** No: gli eventi e i momenti in cui parte il
  feedback sono identici. È cambiato solo *il suono* (e ora arriva da un file).
