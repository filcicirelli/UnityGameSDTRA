# Schermata Iniziale e Comandi 🕹️🖱️✋

Questa cartella aggiunge due cose legate fra loro:

1. una **schermata iniziale** (start) che appare appena parte il gioco;
2. la possibilità di **scegliere come giocare**: con il **mouse**, con il **dito**
   (webcam) o con il **joystick**.

Nella schermata iniziale si vedono anche un **riepilogo delle impostazioni** del
gioco e i **parametri tecnici** (FPS, refresh rate, schermo, dispositivo…), comodi
da mostrare e spiegare.

> **Perché serve nella riabilitazione**
> Poter scegliere il comando permette di adattare il gioco al paziente e
> all'attrezzatura disponibile: il **mouse** per chi ha buon controllo fine, il
> **dito davanti alla webcam** per allenare movimenti **ampi del braccio**, il
> **joystick** per chi usa già ausili con leva. Il gioco resta identico: cambia
> solo *come* si muove Astro.

---

## Le tre modalità

| Modalità | Come si muove Astro | Note |
|---|---|---|
| **Mouse** | con il puntatore del mouse | è il modo classico, sempre disponibile |
| **Dito (webcam)** | seguendo un **oggetto colorato** sul dito davanti alla webcam | vedi cartella `comando webcam` |
| **Joystick** | con la **leva** del joystick | funziona anche con le **frecce** o i tasti **WASD** |

---

## Il corpo del codice (spiegato in breve)

Tre file di codice, nello stesso stile del resto del progetto:

### 1. `ParametriComandi.cs` — la *pagina dei valori*
Solo numeri da regolare (come `Impostazioni.cs`). Riguarda il **joystick**:
velocità del puntatore, zona morta della leva, inversione su/giù.

### 2. `Comandi.cs` — il *punto unico* che dice dove sta il puntatore
È un oggetto che **si installa da solo** all'avvio (come `FeedbackPaziente`).
Tiene la modalità scelta (`Comandi.Attuale`) ed espone **un solo metodo**:

```csharp
Comandi.PuntatoreSchermo();
// MOUSE    -> Input.mousePosition
// DITO     -> la posizione del dito (da ComandoWebcam); se manca, il mouse
// JOYSTICK -> un puntatore che si sposta a velocità con la leva
```

Astro chiama solo questo metodo e **non sa** quale comando è attivo: per questo il
resto del gioco non è cambiato. Per il **joystick** il puntatore non ha una
posizione "assoluta" come il mouse, quindi `Comandi` la calcola in `Update`
spostandola un po' ad ogni fotogramma in base alla leva (assi `Horizontal` e
`Vertical` di Unity), restando dentro lo schermo.

### 3. `SchermataStart.cs` — la *schermata iniziale*
Disegnata con `OnGUI` come l'HUD (`InterfacciaGioco`). Appare finché
`GestoreGioco.MenuInizialeAperto` è `true`. Mostra:
- il **titolo** del gioco;
- i **tre pulsanti** per scegliere il comando (quello scelto diventa verde);
- il pulsante **GIOCA**, che chiama `GestoreGioco.IniziaPartita(...)`;
- la colonna **IMPOSTAZIONI DEL GIOCO** (i valori di `Impostazioni`, del feedback
  e dei comandi);
- la colonna **PARAMETRI TECNICI** (FPS, target FPS, VSync, risoluzione, refresh
  rate, schermo intero, sistema operativo, CPU, RAM, scheda video, API grafica,
  versione di Unity…).

---

## Come è collegato al resto del gioco

L'integrazione è volutamente semplice:

| File / metodo | Cosa è cambiato |
|---|---|
| `Astro.Update()` (`Oggetti.cs`) | legge il puntatore da `Comandi.PuntatoreSchermo()` |
| `GestoreGioco.Start()` | non parte subito: apre la schermata iniziale e mostra lo sfondo |
| `GestoreGioco.IniziaPartita(...)` | imposta il comando scelto e avvia il livello 1 |
| `GestoreGioco.TornaAlMenuIniziale()` | torna alla schermata per cambiare comando |
| `CaricatoreLivelli.MostraSoloSfondo()` | mostra solo la nebulosa dietro al menu |
| `InterfacciaGioco.OnGUI()` | nasconde l'HUD durante il menu; aggiunge "CAMBIA COMANDO" |

Durante il menu il cursore del mouse torna **visibile** (serve per cliccare i
pulsanti); quando si gioca torna nascosto, perché il puntatore è Astro.

---

## 📋 Pagina dei parametri modificabili (`ParametriComandi.cs`)

| Variabile | Valore | Cosa fa |
|---|---|---|
| `JOYSTICK_VELOCITA` | `1000` | pixel al secondo del puntatore con la leva a fondo corsa |
| `JOYSTICK_ZONA_MORTA` | `0.15` | ignora i piccoli movimenti della leva a riposo |
| `JOYSTICK_INVERTI_Y` | `false` | se `true`, inverte il su/giù (stile "aereo") |

### Esempi di regolazione
- Puntatore **troppo veloce** col joystick: abbassa `JOYSTICK_VELOCITA` (es. `600`).
- La leva fa muovere il puntatore **da sola**: alza `JOYSTICK_ZONA_MORTA`.

## Note tecniche
- **Nessun file o libreria esterna**: il joystick usa gli assi standard di Unity
  (`Horizontal`/`Vertical`), la schermata usa `OnGUI`, i dati tecnici arrivano da
  `SystemInfo`, `Screen` e `Application`.
- I file `.meta` di Unity per questa cartella e per i nuovi script vengono generati
  **automaticamente** la prima volta che apri il progetto in Unity.
