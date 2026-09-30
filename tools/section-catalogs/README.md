# Estrazione dei cataloghi delle sezioni

Script Python che trasformano le fonti (norme riprodotte e programmi dei produttori) nei CSV di
`ModelData/Sections/Catalogs`, incorporati in GPCModelData. Documentazione dei cataloghi, della mappatura
e della fedeltà: [ModelData/Sections/README.md](../../ModelData/Sections/README.md).

| Script | Fonte | Cataloghi |
| --- | --- | --- |
| `extract_arcelormittal.py` | ArcelorMittal, Sections and Merchant Bars, Sales Programme V2026-1 (Excel) | EN 10365 (I/H, IPN/J, UPE/PFC/UPN), EN 10056-1 (angolari) |
| `extract_promozioneacciaio.py` | Fondazione Promozione Acciaio, tabelle dei profili cavi EN 10210 / EN 10219 (PDF) | CHS, SHS, RHS a caldo; CHS a freddo |
| `extract_aisc.py` | AISC Shapes Database v16.0 (Excel, agosto 2023) | W, M, S, HP, C, MC, L, WT, MT, ST, 2L, HSS, PIPE |

Requisiti: Python 3 con `openpyxl` e `pdfplumber`. Le fonti non sono nel repository: il loro SHA256 è
scritto nell'intestazione di ogni CSV.
