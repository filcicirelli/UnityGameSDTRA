# Sorgenti — generazione di Presentazione e Tesina

Questi script rigenerano **da codice** la presentazione e la tesina. Sono utili se vuoi
modificare i contenuti in modo riproducibile (stesso stile, stessi colori del Politecnico di Bari).

## File
| File | Cosa fa |
|---|---|
| `build_pptx.py` | genera `Presentazione_Astro.pptx` (16 slide, schemi a blocchi nativi) |
| `build_docx.py` | genera `Tesina_Astro.docx` (relazione + bibliografia) |
| `lit_ppt.json` | testi scientifici **brevi** usati nelle slide |
| `lit_docx.json` | testi scientifici **estesi** + bibliografia usati nella tesina |
| `logo_polibari.png` | logo usato nei documenti |

## Come si usa
Serve Python 3 con due librerie:
```bash
pip3 install python-pptx python-docx
python3 build_pptx.py   # crea Presentazione_Astro.pptx nella cartella superiore? no: accanto allo script
python3 build_docx.py   # crea Tesina_Astro.docx accanto allo script
```
I percorsi sono **relativi** alla cartella dello script: copiando questa cartella altrove, continua a funzionare.

## La bibliografia
I 29 riferimenti in `lit_docx.json` sono stati **cercati e verificati** (esistenza confermata su
PubMed / DOI). Si raccomanda comunque di **ricontrollare ogni citazione** sulla fonte originale
(autori, anno, rivista, DOI) prima della consegna.

> Per cambiare un testo scientifico modifica i file `lit_*.json` e rilancia gli script.
> Per cambiare layout/colori delle slide intervieni su `build_pptx.py` (sezione *Palette* e le funzioni `slide_*`).
