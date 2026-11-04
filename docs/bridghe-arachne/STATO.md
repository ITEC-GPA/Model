# Stato della migrazione comune

Aggiornamento del 4 novembre 2026, sera (F8). Sostituisce lo stato scritto da ChatGPT il 3 novembre.

Fonti, nella cartella `C:\Users\g.pacini\Documents\Repository\gpc-unification-20261102\revisione-claude-20261103`:
- [RIPRESA.md](../../../revisione-claude-20261103/RIPRESA.md): decisioni dell'utente, commit integrati e gate. È la fonte più recente;
- [CORREZIONI.md](../../../revisione-claude-20261103/CORREZIONI.md): ogni finding della revisione del 3/11, con stato, commit e prova;
- le note delle corsie `correzioni-*.md` (L1–L7, L9, L10, P2A, P2B, F7a).

I collegamenti relativi valgono nel workspace `gpc-unification-20261102`. Il piano è in [PIANO.md](PIANO.md).

Altri documenti di F8:
- [F6-CONCLUSIONE.md](F6-CONCLUSIONE.md): che cosa ha dato F6, per corsia, e le differenze nei risultati approvate;
- [F7-PRESTAZIONI.md](F7-PRESTAZIONI.md): tempi prima e dopo, master contro F6;
- [RITORNO-VERSIONE-PRECEDENTE.md](RITORNO-VERSIONE-PRECEDENTE.md): come si torna alla versione precedente;
- [f8-mappatura-tipi.json](f8-mappatura-tipi.json): dove sta oggi ogni tipo dei vecchi modelli. Sostituisce la matrice F5.

## In breve

| Fase | Fatto | Resta |
|---|---|---|
| F0 | Workspace isolato `gpc-unification-20261102`, branch `codex/gpc-unification-20261102` | — |
| F1 | Riferimenti congelati di BridGHe (esempi 50 e 99) e di Arachne | — |
| F2 | Contratti, copie, assemblaggio | — |
| F3 | Persistenza condivisa (`GPC.Archive`, `ExternalModelArchive`) | — |
| F4 | Piloti ponte e membrana | — |
| F5 | Dominio esteso, writer Straus7 nel Converter | La matrice F5 era una classificazione (finding 0.2). Ora è superata da `f8-mappatura-tipi.json` |
| F6 | Librerie corrette (L1–L7, L9, L10). Arachne senza modelli FeMM | **BridGHe in corso**: prossimo passo C5 |
| F7 | Set candidato, ANTHEA 1.19.0, ReBear sul set, tabelle dei tempi | Va rifatto sul set ufficiale in F8 |
| F8 | Questa documentazione. Piano di rilascio deciso dall'utente | Tutto il rilascio, dopo la fine di BridGHe |

Nessun push, tag, installer o installazione è stato fatto per questo lavoro. Tutto sta sui branch.

## F1–F5 (ChatGPT, fino al 3/11)

ChatGPT ha fatto F1–F5 e quattro incrementi di F6 per Arachne. L'utente lo ha fermato il 3/11 alle 22:01 ([F6-AVANZAMENTO.md](F6-AVANZAMENTO.md), «Arresto»).

Claude ha rivisto il lavoro la sera del 3/11 (`REVISIONE.md`). L'utente ha confermato tutti i finding. CORREZIONI.md conta 50 righe: 44 corrette, 2 ritirate (A5, L4-8), 3 in corso, 1 aperta.

L'unica riga aperta era la 0.2: la matrice F5 `f5-matrice-revisione.json`. In quella matrice:
- `meaning` coincide con la destinazione in ogni campo;
- `identity` è la stessa frase per tutti i 2.309 campi;
- molte prove sono solo un rinvio al piano.

In F8 la matrice è segnata come superata (campi `supersededBy` e `supersededNote` in testa). La mappatura vera è in [f8-mappatura-tipi.json](f8-mappatura-tipi.json):
- 295 tipi, ognuno con la prova nel codice;
- 231 mappati, 4 respinti dal lettore legacy di BridGHe, 60 non risolti;
- i tipi respinti e non risolti sono tutti di BridGHe. Sono tipi che il lettore legacy rifiuta o non porta ancora in GPC (proprietà composte, stati di sezione), oppure tipi del plugin che usano ancora i modelli FeMM. P2B li deve ancora migrare.

## F6 — migrazione dei plugin

### Librerie: finite e integrate

| Corsia | Che cosa | Commit principali |
|---|---|---|
| L1 | Dominio GPCModel: B1–B7, B9, A4 | Model `604f7067`, `d0ef70f5`, `dd8c70fb`, `64d61197`, `0eecc914`, `7101a045`, `ea705553`, `aeb2ef79` |
| L2 | Writer Straus7 del Converter: C1–C7, B4, B8 | Model `3997b165`, `bada6d7b`, `1933f3e6`, `681805b9`, `0e499aad`, `a8574694`, `3ae686ab` |
| L3 | `GPC.Archive` per blocchi, D3 del Checker, ANTHEA su `GPC.Archive` | Model `62508fef`; Checker `66bb2441`; ANTHEA `8be298a6` |
| L4 | Revisione indipendente di P1: 10 finding | Model `3901d338`, `67dfe57b`, `7b472811`; Checker `9a4c3f85`; ANTHEA `a0c13df4` |
| L5 | Formato compatto e prestazioni di Arachne | Model `e31af9e1`, `c12ef42f`, `aebb23dd`; Arachne `4c942a6`, `94a31f6`, `34f8d65` |
| L6 | API pubblica delle aree a taglio a ν = 0, BXS conforme delle sezioni saldate | Model `fed32f3c`, `405b73e2` |
| L7 | Blocco di `Super Element Check` | BridGHe `442733c` |
| L9 | Proprietà e sistemi di riferimento condivisi nelle copie, `LineLoad.GetGlobalForces`, writer riusati | Model `9735e5ab`, `1ab5ac64`, `df660262`, `3312b802`, `9b7f26aa`, `9a91a9b9`; Arachne `6a2deef`, `57f0fd7`, `611a3d8` |
| L10 | Decisioni 5, 6 e 9 dell'utente | Model `f3ee8807`, `ec99b709`; Checker `d85a9fda` |

Il dettaglio per corsia è in [F6-CONCLUSIONE.md](F6-CONCLUSIONE.md).

### Arachne: finita

- Corsia P2A (Arachne `321b6d0`), poi L5 e L9.
- `Arachne.Common/Models` è tolto: 32 file FeMM. I data type GH portano oggetti GPC.
- GUID e porte dei componenti sono invariati: inventario 66/66.
- Il lettore NRBF è tolto. I vecchi payload sono rifiutati con un messaggio.
- I risultati del Form Find stanno nel file accanto `<documento>.<InstanceGuid>.gpc-results`.
- `Arachne.Compare` contro master `39c54a2`: 31 scenari identici bit per bit, 1 diverso per decisione (massa in IPS).
- Il master di Arachne (`39c54a2`) è già unito nel branch (`b50cddb`). Il branch è 26 commit avanti e 0 indietro.

### BridGHe: in corso

Corsia P2B, cartella `gpc-f6-claude/P2B`, branch `claude/f6-bridghe`, testa `f142705`. Note in `correzioni-P2B.md`.

Fatto:
- passi 0–5: analisi, export e sezioni native passano dal writer del Converter;
- passo 7: tolti il binding St7API e il vecchio exporter. Nel plugin non c'è più nessun `DllImport`;
- passi 8–9: aree a taglio a ν = 0 e travi composte omogeneizzate (E13);
- passo 10: C5 nel confronto offline. D3 riproduce le verifiche di BridGHe: 99 con 2.808/2.808 righe, 50 con 66.960/66.960 righe identiche bit per bit;
- passo 13: C1a, C2 e C3 primo passo (`GH_Model.Gpc`);
- passi 16–17: **C4 e C3 finiti** (`029a40f`, `f142705`):
  - i componenti Results leggono i dati GPC, uguali bit per bit ai risultati FEM su 01–10, 99 e 50;
  - la giuntura copia `GpcInput` invece di riconvertire il modello FEM;
  - corretto un difetto di unità introdotto in F6 per le definizioni non Nmm;
  - risultati delle analisi in `<output>.bghr` (formato `BGHR2`) accanto al `.st7`, come master 0.3.0.

Resta:
1. **C5** (prossimo): `BridgeWorkflowExecutor.Prepare` in `Super Element Check`, un piano per workflow;
2. C6: carichi mobili e utilità sui risultati GPC;
3. fasi (a) e (b): tipi GH su GPC, con E10 (Ecm) ed E11 (G nella griglia di soletta);
4. rimozione dei modelli FeMM (`BridGHe.Common/Models`, 55 file) e della giuntura `FemmAnalysisSeam`;
5. merge di master (`87175b7`, `d7e4092`, `95ab7c3`), con i conflitti elencati al passo 12 di P2B;
6. versione almeno 0.4.0 (master è 0.3.0).

I tipi GH portano ancora i modelli FeMM. La conversione verso GPC la fa il lettore legacy (`src/BridGHe.Common/Gpc/Legacy`).

## F7 — integrazione

Corsia F7a, cartella `gpc-f6-claude/F7`. Note in `correzioni-F7a.md`.

### Set candidato

- Cartella `gpc-f6-claude\F7\sets\gpc-candidate-20261104-f7a`. 15 DLL uguali al manifest. Nessuna DLL di gmsh.
- Prodotto con `Release-Set.ps1 -Candidate` da Model `4375e7a5` e Checker `5f6e4e44` (branch `claude/f7-set`).
- Versioni: GPCModel 5.49.0.0, GPC.Model.Persistence 3.14.0.0, GPC.Archive 1.2.0.0, GPC.Converter 2.19.0.0, GPCChecker.CompositeBridge 1.27.0.0, GPCModelChecker 2.39.0.0.
- **Il set è vecchio**: non contiene L9, L10 e il lavoro di libreria di P2B. Le versioni di F7a non sono in codex. F8 le rifà (almeno GPCModel 5.50.0.0).

### ANTHEA 1.19.0

- Branch codex di ANTHEA, testa `86a72f1b`. Commit: `31c5c5cb` (sezioni c.a. nel formato compatto, documento versione 5), `18d96c67` (numero 1.19.0), `566b2983` (cattura `cattura-mainwindow` aggiornata, con l'ok dell'utente).
- origin/main è unito fino a `131f3738` (merge `e5ee8553`, registro `86a72f1b`). Restano da unire 3 commit di origin/main: `0c82e812`, `b222c9dc` (solo prove) e `4f38cffb`.
- Sezioni 5–27 volte più piccole. Esempio: `progetto-1.18.0.programma` da 531 a 65 kB.
- La 1.18.0 vera apre un file della 1.19 con sezioni compatte e dice «Formato o versione del file non supportati.».
- `lib/Checker` contiene ancora il set candidato F7a (`ae62a4f3`).

### ReBear

- Branch `claude/f7-rebear`, commit `63a78f4`, sopra master `f2df907`. Solo `lib/GPC` dal set candidato, motore negli esempi, README.
- `build/Verify.ps1 -AllowCandidate` 41/41; `build/Examples.ps1` 20/20.

### Stesso processo Rhino

I tre plugin caricano oggi build diverse. BridGHe (P2B) e Arachne caricano due GPCModel pilota 5.48.0.0 con SHA-256 diversi. Nello stesso Rhino la prima DLL caricata vale per tutti. F8 deve mettere i tre plugin sullo stesso set.

### Tempi

Le tabelle sono in [F7-PRESTAZIONI.md](F7-PRESTAZIONI.md). In sintesi:
- Arachne: costruzione, salvataggio e apertura sono più veloci di master. Form Find a 60.000 nodi è alla pari con master.
- Arachne: restano più lenti Duplicate, Force Density e Form Find a 10.000 nodi (+1–2 s), su .NET Framework.
- BridGHe, esempio 50: D3 con il piano preparato una volta passa, nel ciclo C5 in Rhino, da 1.202 a 223 s.

## F8 — documentazione e rilascio

### Piano di rilascio deciso dall'utente (4/11 sera)

**«Tutto insieme».** Si aspetta la fine di BridGHe. Poi, in quest'ordine:
1. set ufficiale delle librerie da master:
   - versioni nuove sopra quelle di F7a (almeno GPCModel 5.50.0.0);
   - `Release-Set.ps1` senza `-Candidate`, con `-Previous` uguale al `release-set.json` di AU;
2. ANTHEA 1.19.0 con il set ufficiale e il profilo full;
3. BridGHe 0.4.0, Arachne e ReBear sullo stesso set;
4. se tutte le prove passano («Sì, push e pubblica»):
   - push e tag;
   - installer di ANTHEA;
   - bin dei plugin ricostruiti con Rhino chiuso;
   - copia sul server `X:\ITEC-MODELLI\5-RHINO\SGBIM-GR_PLUGIN\<plugin>`, di notte se possibile, con il contenuto vecchio in una cartella da eliminare;
5. installer di ANTHEA anche in `\\STORAGESARZANA\Varie\20 - AGGIORNAMENTI_SOFTWARE\Anthea`, con la 1.18.0 spostata in `_SUPERATI`.

Plugin da ricostruire: BridGHe, Arachne e ReBear. Rhino2FEM, Pawper, Ant e Meerkat non usano Model né Checker, e Geometry e Utilities non cambiano: per loro non serve una versione nuova.

### Altri passi di F8

- Costruire il set da un checkout pulito. Gli a capo LF dei file di codex Model sono solo nel working tree; l'indice è normale.
- Ricostruire `GPC.Archive` dal commit finale (SourceLink) prima del set.
- Registrare il set ufficiale in ANTHEA (`tests/fixtures/f4/library-release.json`): la prova di provenienza F4 di `Architecture.Tests` passa solo con un set ufficiale.
- Provare il merge su master in ogni repository e provare i pacchetti in un ambiente pulito.
- Dopo i merge, ricostruire i bin che Rhino carica.
- Chiedere all'utente prima di push o installazioni, salvo quanto già autorizzato sopra.

## Commit integrati, per repository

Branch `codex/gpc-unification-20261102`, worktree in `gpc-unification-20261102/<repo>`.

| Repository | Testa | Contro master | Integrazioni (prima linea del branch) |
|---|---|---|---|
| Model | `9a91a9b9` | 92 avanti, 0 indietro rispetto a origin/master | `2a03aebc` L1, `270b9a4c` L2, `e54300b9` L3, `8c5cf519` L2 secondo giro, `8234b7d1`, `cc050bec` P2A, `d9c4db78` L4, `938508b2` L6, `b19dbe9d` L5, `9735e5ab` … `3312b802` L9, `e031e161` P2B, `91bdbadb`, `b2bf3d54` (con `f0fc9cb9`: L9 secondo giro e L10), `9a91a9b9` L9 quinto giro |
| Checker | `cc9dbb16` | 15 avanti, 0 indietro | `a1e93191` L3, `c8cc0468` L4, `b829ff40` P2B (D3 `31670ac8`, `109d3b71`), `cc9dbb16` L10 |
| ANTHEA | `86a72f1b` | 15 avanti, 3 indietro rispetto a origin/main | `f31f5672` L3, `6fb72d56` L4, `ae62a4f3`, `31c5c5cb`, `18d96c67` F7a, `566b2983`, `e5ee8553` merge di origin/main, `86a72f1b` |
| Arachne | `611a3d8` | 26 avanti, 0 indietro rispetto a master `39c54a2` | `3992879` P2A, `40e074d` L5, `6a2deef` … `6f3931d` L9, `b50cddb` merge di master, `611a3d8` |
| tensilelib | `5e25765` | invariato | — |
| BridGHe | `a0dbd4d` (WIP di ChatGPT) | — | Nessuna: il lavoro è su `claude/f6-bridghe` (`f142705`, 28 commit sopra `a0dbd4d`), da integrare a fine P2B |
| ReBear | — | — | `63a78f4` su `claude/f7-rebear`, nel workspace F7 |
| Model e Checker, versioni F7a | — | — | `4375e7a5` e `5f6e4e44` su `claude/f7-set`, non in codex |

## Gate più recenti

| Dove | Commit | Esito |
|---|---|---|
| Model, `build/Verify.ps1` nativo (`int-13`) | codex `9a91a9b9` | 63 + 1.929 + 135 PASS, 7 ignorati come sempre |
| Checker, `Verify.ps1 -Suites all` (`int-10` … `int-13`) | codex `cc9dbb16` su Model codex | 10.397 PASS (CompositeBridge 455), API senza rimozioni |
| Arachne | `611a3d8` | headless 164, ARAP 54, `Arachne.Compare` 31 + 1 |
| ANTHEA | `86a72f1b` | `ANTHEA.Projects.Checks` 22, `Application.Tests` 3.517, `ModelWorkspace.Checks` 731; profilo standard 56/57 (solo il NEW-FAIL atteso F4); UI 72/73, poi `ui/check-contrast` da sola PASS |
| BridGHe | `f142705` (Checker pilota `cc9dbb16`) | `--core-only` 269, `--regression all` 85 (fallisce solo `gpc-release-set`, atteso), `--core-only --native` 392, `--examples all` 10/10, `--moving-load-tests --native` PASS, `--gpc-results-native` 16, 99 PASS (2.808 righe, impronta `EB5A1500`), 50 PASS (66.960 righe, impronta `F0B6964B`) |
| ReBear | `63a78f4` | 41/41, esempi 20/20 (set candidato F7a) |
| Set candidato F7a | Model `4375e7a5`, Checker `5f6e4e44` | Model nativo 63 + 1.901 + 128; Checker 10.390 |

Note sui gate:
- `int-12` ha avuto 3 fallimenti `St7Init -8`: la chiave USB era stata tolta. Con la chiave (`int-13`) tutto passa.
- In `int-13` la prova `L6_NativePathAxesFollowTheReferencePoint` ha impiegato 4.943 s invece di 18–20 s, poi è passata. La causa probabile è un blocco di tutta la macchina dalle 18:54. Se si ripete va indagato.
- I log sono in `gpc-f6-claude/INT/results`.

## Che cosa non è ancora verificato

- BridGHe dopo C5, C6 e le fasi (a)/(b): numeri, `.gh` con i contratti GPC, prestazioni.
- Il salvataggio del `.gh` dell'esempio 50 con il formato finale: è misurato solo il formato attuale (23,06 MB).
- I tempi di Arachne in Rhino 8 su .NET 8. Tutte le misure di Arachne sono su .NET Framework.
- Il set ufficiale, il profilo full di ANTHEA e la prova in un ambiente pulito: si fanno in F8.
- Le domande ancora aperte sono in CORREZIONI.md, «Domande aperte».
