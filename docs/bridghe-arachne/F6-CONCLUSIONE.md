# F6 — conclusione

Stato al 4 novembre 2026, sera. **Arachne è finita. BridGHe è in corso** (corsia P2B): questa parte del documento va completata quando P2B finisce.

**Aggiornamento del 5 novembre** (corsia D8). Il testo qui sotto resta quello del 4/11. Nel frattempo:
- **L11** (Model `61bd8251`, `9736405e`; Checker `e7fee77c`): `BridgeCompositeSectionDefinition` nel Checker e `ModelValues.Copy(model, options, Guid)`, la copia con un'identità di modello nuova.
- **L12** (Model `30d776a7`, `f98a6e39`; Checker `e87a2ffd`, `d018ff28`): il vocabolario del ponte esce da GPCModel. Gli attributi di ponte di L11 e i metadati dei membri (`BeamMemberPart.BridgeMetadata`, la correzione B4 di L1) sono sostituiti da `BridgeModel` = `Model` + `BridgeModelData` in CompositeBridge. `RigidConnectionAttribute` resta in `GPC.Model.Attributes`, e ora scrive il punto che tiene (prima due salvataggi dello stesso modello davano byte diversi). Vedi [REGOLA-GPCMODEL.md](REGOLA-GPCMODEL.md).
- **Sessione Azioni** (Model `d52b93a3`, `92537906`, `59d2b9b5`, `fd89a333`, `81a62065`): planarità delle piastre relativa alla loro dimensione, max(1e-4 mm, 1e-5 × distanza massima fra i vertici), anche per `NonUniformPlatePressure`; ogni piastra accettata prima resta accettata. Pressioni variabili scritte in Straus7 come forze nodali equivalenti, solo in uscita. Classe EN 206/NTC del calcestruzzo da «fc = N MPa» nel lettore Straus7, con l'avviso `MaterialGradeFromStrength`.
- **P2G** (Model `75f5a370`): `ModelValues.Renumber` per la Construction Sequence di BridGHe.
- **A8** (Model `bc5a7a07`): proprietà di build che tiene in Release i riferimenti costruiti da una soluzione che non li contiene. Nessuna DLL cambia.
- **P2C–P2I** (BridGHe fino a `b579e10`): catene C e D, R3, R7, R8, migrazione di L12, catena E fino al punto (c). Le differenze elencate per il changelog 0.4.0 sono in CORREZIONI.md e in `correzioni-P2B.md`.

Fonti, nella cartella `C:\Users\g.pacini\Documents\Repository\gpc-unification-20261102\revisione-claude-20261103`: [CORREZIONI.md](../../../revisione-claude-20261103/CORREZIONI.md) («Differenze nei risultati»), [RIPRESA.md](../../../revisione-claude-20261103/RIPRESA.md) e le note `correzioni-<corsia>.md`. Lo stato generale è in [STATO.md](STATO.md).

Abbreviazioni:
- GH: Grasshopper;
- R31: Straus7 con l'API R3.1;
- BXS: file di sezione di Straus7;
- SA, SA1, SA2: aree a taglio, lungo gli assi principali 1 e 2;
- J: costante di torsione;
- F1: valori di riferimento catturati in F1 dal codice precedente;
- IPS: unità pollici, libbre, secondi; lbm: libbra-massa;
- FAF: Fixed Axial Force.

I gate di Model sono scritti come Core + UnitTest + net472, cioè le prove superate nelle tre suite.

## Che cosa ha dato F6, per corsia

### L1 — dominio GPCModel

- Finding B1–B7, B9 e la parte Model di A4. A5 è ritirato: «Linear step by step» esiste in Arachne master, quindi `SteppedLinearForceDensity` resta.
- Commit Model: `604f7067` (B1, B9), `d0ef70f5` (B2), `dd8c70fb` (B3, `MembranePlateProperty`), `64d61197` (B4), `0eecc914` (B5), `7101a045` (B6), `ea705553` (B7), `aeb2ef79` (A4), `8cab225c` (B9).
- Gate: Model `build/Verify.ps1` su `64d61197` 63 + 1.757 + 99; Checker `Verify.ps1 -Suites all` 10.382 PASS.
- Integrata in codex Model con `2a03aebc`.

### L2 — writer Straus7 del Converter

- Finding C1–C7, B4 (opzioni dei carichi mobili) e B8 (legge centrifuga in N-mm). Prove native Straus7.
- Commit Model: `3997b165` (C1), `bada6d7b`, `e18441dd` (C2), `1933f3e6` (C3), `681805b9`, `117e2921`, `a8574694`, `0e499aad` (C5), `a8574694` (C6, C7), `fd0bf7f5`, `bfb9c1fe` (B4), `3ae686ab`, `f5ebe943` (B8).
- Gate: Model nativo su `f5ebe943` 63 + 1.831 + 99 = 1.993 PASS, con 35 prove nuove.
- Integrata con `270b9a4c` e, secondo giro, `8c5cf519`.

### L3 — `GPC.Archive`, D3 del Checker, ANTHEA

- D1: ANTHEA unito con origin/main 1.18.0 e passato a `GPC.Archive` per l'archivio dei risultati.
- D2: errori per blocco, sidecar mancante o corrotto che non blocca il salvataggio, manifest in cache per l'autosave.
- D3: il workflow `CompositeBridge` riproduce le ipotesi di BridGHe (azioni fuori piano, ψL per azione, ritiro primario).
- Commit: Model `62508fef`; Checker `66bb2441`; ANTHEA `77f42f35`, `8be298a6`, `4a20ca06`, `9d53cb1d`, `eb28bf2a`.
- Gate: `ExternalArchiveTest` 25/25 su net6.0 e net472; Checker all PASS (CompositeBridge 447); ANTHEA `ModelWorkspace.Checks` 722, profilo standard 81 PASS + 3 NEW-FAIL spiegati, UI 11.
- Integrata con Model `e54300b9`, Checker `a1e93191`, ANTHEA `f31f5672`.

### L4 — revisione indipendente di P1

- 10 finding. 9 corretti; L4-8 non è riproducibile ed è stato verificato in nativo, con una prova di guardia.
- Commit: Model `3901d338`, `67dfe57b`, `7b472811`, `fe2c6d89`, `198b21e0` (solo prova); Checker `9a4c3f85`; ANTHEA `a0c13df4`.
- Gate: Model nativo su `fe2c6d89` 63 + 1.877 + 113; Checker 10.390; ANTHEA `ModelWorkspace.Checks` 726, `Presentation.Tests` 4.991, UI 11.
- Integrata con Model `d9c4db78`, Checker `c8cc0468`, ANTHEA `6fb72d56`.

### L5 — formato compatto e prestazioni di Arachne

- Codec binario dei contratti, `ModelArchive` v6, `ModelValueArchive` v2, manifest dei sidecar v2 a parti da 16 MiB. Nessun limite di 256 MB per modello.
- ANTHEA continua a scrivere XML: il passaggio al compatto lo ha fatto F7a per le sezioni.
- Decisioni dell'utente: Cable e FAF insieme come master; massa IPS in lbm.
- Commit: Model `e31af9e1`, `c12ef42f`, `aebb23dd`; Arachne `4c942a6`, `48fdde3`, `34f8d65`, `94a31f6`, `013ac42`, `77b73da`, `07ef5e6`, `5fd34ef`.
- Gate: Model non nativo su `aebb23dd` 63 + 1.797 + 123; Arachne headless 163, ARAP 54; `Arachne.Compare` 31 identici + 1 diverso per decisione.
- Integrata con Model `b19dbe9d`, Arachne `40e074d`.

### L6 — aree a taglio a ν = 0, BXS conforme, centrifuga

- API pubblica `GPC.Model.Sections.SectionShearAreas`, a ν = 0, con densità della mesh esplicita.
- BXS conforme per le sezioni saldate.
- La centrifuga è ammessa solo per forze in assi del percorso; in assi globali è rifiutata a `Prepare`.
- Commit Model: `fed32f3c`, `af2877d9`, `405b73e2`, `1688815a`, `317366b4`, `99f07e28`.
- Gate: Model non nativo su `405b73e2` 63 + 1.808 + 119; Checker 10.390. Dopo l'integrazione, nativo su `938508b2` 63 + 1.892 + 119.
- Integrata con `938508b2`.

### L7 — blocco di `Super Element Check`

- Causa: `GpcReleaseGuard.CheckLoaded` di BridGHe chiamava `RhinoApp.WriteLine` dentro `AppDomain.AssemblyLoad`, mentre il thread di Rhino aspettava lo stesso caricamento. Il difetto non era nel Checker né in GPCModel.
- Correzione: `InvokeOnUiThread`. BridGHe `442733c` sul branch `claude/f6-L7`, poi nel branch di P2B.
- Lo stesso difetto c'era nei plugin in produzione. Una sessione separata lo ha corretto sui master e lo ha pubblicato il 4/11: BridGHe `87175b7`, Arachne `39c54a2`, ReBear `f2df907`, più i plugin Rhino2*.
- Gate: le guardie `GpcReleaseSetTests.Guard` e `PluginBuildCheckTests.Guard` sono rosse senza la correzione e verdi dopo; `--regression plugin-build-check` PASS; `--core-only` 269. Poi 99 e 50 nativi PASS con il Checker pilota (P2B, passo 8 bis).

### L9 — copie condivise, `LineLoad`, writer riusati

- `ModelCopyOptions.ShareProperties` e `ModelCopyOptions.ShareCoordinateSystems`, con due overload di `ModelValues.CopyValue` e `ModelValues.Copy`. Le copie a un argomento restano profonde: ANTHEA e Checker non cambiano.
- Arachne usa le due opzioni insieme in tutte le copie dei dati GH.
- Corretta in Arachne la Plate Property di membrana, che teneva il GUID della membrana.
- `LineLoad.GetGlobalForces` e `LineLoad.GetLocalForces` corretti (vedi le differenze, sotto).
- Writer del codec dei risultati riusati dentro una compattazione, con gli stessi byte; sidecar del Form Find preparato durante la proiezione delle uscite.
- Commit: Model `9735e5ab`, `1ab5ac64`, `df660262`, `3312b802`, `9b7f26aa`, `9a91a9b9`; Arachne `6a2deef`, `57f0fd7`, `6f3931d`, `b50cddb` (merge di master `39c54a2`), `611a3d8`.
- Gate (ultimo giro): Model non nativo 63 + 1.842 + 135; Checker 10.390; Arachne headless 164, ARAP 54, `Arachne.Compare` 31 + 1. Nativo su `3312b802`: 63 + 1.910 + 134.
- Integrata in codex con avanzamenti rapidi; l'ultimo è Model `9a91a9b9`, Arachne `611a3d8`.

### L10 — decisioni 5, 6 e 9

- Decisione 5: le parti a contatto delle sezioni composte non saldate hanno nodi separati nel BXS (Model `f3ee8807`).
- Decisione 6: offset centrifugo in N per veicolo (Model `ec99b709`).
- Decisione 9: D3 preparato una volta, `BridgeWorkflowExecutor.Prepare` → `BridgeWorkflowPlan` (Checker `d85a9fda`). La vecchia API resta.
- Gate: Checker all 10.397 PASS (CompositeBridge 455, K0 compreso); Model nativo su `ec99b709` 63 + 1.923 + 131; prova nativa mirata 44/44.
- Integrata con Model `f0fc9cb9`, Checker `cc9dbb16`. Gate `int-10`: Model nativo 63 + 1.928 + 135.

### P2A — Arachne senza FeMM

- I data type GH portano oggetti GPC. `Arachne.Common/Models` è tolto. L'adattatore verso TensileLib è lineare nel numero di elementi (A1).
- Lettore NRBF tolto (A3). Nel bin non ci sono `System.Formats.Nrbf`, `System.Reflection.Metadata` e `System.Collections.Immutable`.
- Vocabolario aggiunto a Model: `FormFindingGoalAttribute`, `FormFindingCoordinateAttribute`, `ElementLabelAttribute`, `Section` con contorno e proprietà dichiarate, `Group.Create`, `ModelAssembly.RefreshIdentityMap`.
- Commit: Model `2d080f0d`, `bbbccc95`, `7efe1282`, `0c908d76`; Arachne `d7ecb46` (`tools/Arachne.Compare`), `321b6d0`, `188044d`, `b3e7e77`.
- Gate: Model su `0c908d76` 63 + 1.783 + 114; `Arachne.Compare` 30/30 scenari identici bit per bit a master `7b1108d`; inventario GUID e porte 66/66; headless 162; ARAP 54.
- Integrata con Model `cc050bec`, Arachne `3992879`.

### P2B — BridGHe senza FeMM (in corso)

Branch `claude/f6-bridghe`, testa `f142705`. Fatto finora:

| Passo | Che cosa | Commit BridGHe |
|---|---|---|
| 1–3 | Livello GPC (`BridGHe.Common/Gpc`): identità, unità, assi, sezioni, materiali, carichi; lettore BGHA1 senza le classi FeMM; import del modello FEM in GPC | `59e2d02`, `72d7ad0`, `293c405` |
| 4–5 | Analisi Straus7 via GPC e Converter; risultati con le convenzioni di prima | `83b9482`, `d44a724` |
| 6 | Archivio dei dati GH su contratti GPC (`GpcGooArchive`, `BGHGPC1`), non ancora usato dai tipi GH | `7b21b7e` |
| 7 | Tolti il binding St7API e il vecchio exporter | `554eab1` |
| 8–9 | Aree a taglio a ν = 0 (E12); travi composte omogeneizzate (E13) | `167008d`, `bc8aa54`, merge `10ed3f0` |
| 10 | C5 offline: D3 riproduce BridGHe su 99 e 50 | `dd9eebc`, `4df2374` |
| 11 | L9 nel branch; audit copy-on-modify | `65701c5` |
| 13 | C1a (etichette, membri di ponte, superelementi), C2 e C3 primo passo | `fbdd9c8`, `3727489`, `8d50fa9` |
| 14–15 | Offset centrifugo con il template; prova nativa su percorso diritto e ad arco; file `BGHR2` | `307ff68`, `e089f4a`, `579844d` |
| 16–17 | C4 (risultati da GPC, `BGHR2` accanto al `.st7`) e C3 finiti | `5c30ad4`, `ede1fc7`, `1266ddc`, `029a40f`, `f142705` |

Commit di libreria di P2B, già in codex:
- Model: `dc56b137`, `b9a3fdee`, `6bf3e1a5`, `90c4739e` (E13), `173ac4a6`, `f5f9b9db`, `97654d85` (stazioni), `91bdbadb` (etichette non scritte in Straus7);
- Checker: `31670ac8`, `109d3b71` (correttivi D3 trovati da C5).

Gate su `f142705`, con il Checker pilota `cc9dbb16`:
- `--core-only` 269; `--regression all` 85 (fallisce solo `gpc-release-set`, atteso con `GpcPilot`);
- `--core-only --native` 392; `--examples all` 10/10; `--moving-load-tests --native` PASS; `--gpc-results-native` 16;
- 99 PASS, 2.808 righe, impronta `EB5A1500`; 50 PASS, 66.960 righe, impronta `F0B6964B`.

Resta: C5 con `Prepare` in `Super Element Check`, C6, fasi (a) e (b), rimozione dei modelli FeMM, merge di master, versione 0.4.0.

## Differenze nei risultati approvate

Solo i numeri riportati nelle note. Dove una differenza non è ancora nel codice, lo dico.

### 1. Aree a taglio di De Saint Venant a ν = 0

- **Decisione:** 3/11 (aree a taglio da GPC) e 4/11 mattina (ν = 0, «seguirei la scienza delle costruzioni»).
- **Dove:** L2 `bada6d7b`, `e18441dd`; L6 `405b73e2`; API `GPC.Model.Sections.SectionShearAreas`. BridGHe le usa dal passo 8 (`167008d`).
- **Prima:** Straus7 calcolava le aree a taglio delle sezioni standard. La prima versione di L2 usava il ν del materiale.
- **Ora:** De Saint Venant a ν = 0, calcolate per elementi finiti. Entro lo 0,3% da R31; scarto massimo 0,274% (CHS 500×5: −0,27%).
- **Rispetto a L2 (ν del materiale):** rettangolo 100×200 SA1 +6,24%; quadrato 100 +0,62%; cerchio Ø100 +0,76%; I 400 con piatti SA1 +0,42%; trave da ponte I 1800 SA1 +0,16%.
- **BridGHe (E12)**, contro il passo 7: 50 utilizzo ≤ 2,7e-6, tensioni ≤ 1,0e-3 MPa; 99 ≤ 7,9e-7 e ≤ 3,9e-4 MPa. Contro F1 lo scarto scende: 50 utilizzo 6,8e-7, tensioni ≤ 3,0e-4 MPa; 99 4,8e-7 e 2,4e-4 MPa. Nessun esito cambiato.

### 2. Travi composte con la sezione omogeneizzata (E13)

- **Decisione:** 4/11 mattina. Aree a taglio dalla sezione omogeneizzata in unità acciaio, De Saint Venant a ν = 0, con l'n della proprietà.
- **Dove:** Model `90c4739e`, `SectionShearAreas.ComputeHomogenised` (in codex); BridGHe `bc8aa54`, merge `10ed3f0` (solo sul branch di P2B).
- **Prima:** SA = 0 su tutte le 57 proprietà composte dell'esempio 50. Straus7 trascurava la deformazione a taglio delle travi composte.
- **Trave G1-C1 del 50:** solo acciaio SA1 30.190 e SA2 38.685 mm²; con n0 = 6,2: 137.234 e 42.956 mm²; con n∞ = 18,3: 51.148 e 40.855 mm².
- **Esempio 99:** utilizzo fino a −0,0236 su 0,166 (trave 13: da 0,166 a 0,143, −14%); tensioni nell'acciaio fino a 8,0 MPa (da 56,3 a 48,3); rapporti fino a 0,022.
- **Esempio 50:** tensioni fino a 3,5 MPa su 125; rapporti fino a 0,043 su 1,53. Utilizzo: 46.978 righe sotto l'1%, 18.020 fra 1 e 5%, 660 fra 5 e 12%, 216 fra 12 e 30%, 44 oltre il 30%. Le 44 sono la combinazione di prova TEST_M03_N0 alla fase 40, con tensioni sotto 0,1 MPa (da 0,341 a 0,0002). Momenti di M03 vicino agli appoggi −14% (trave 36: da −94,0e6 a −80,6e6 N·mm).
- Nessun esito di verifica cambiato.

### 3. G = E/2(1+ν)

- **Pannelli a taglio** (finding C4, decisione del 3/11). Prima G12 = E; ora G = E/2(1+ν), circa 2,6 volte più morbido. Era già nel writer: nessun commit. Prova nativa `NativeShearPanelMatchesUniformShearSolution`.
- **Proprietà di trave** (E7 di P2B): prima Straus7 ricavava G da ν, ora il writer scrive G esplicito = E/2(1+ν). G è identico: cambia solo la rappresentazione.
- **Torsione della griglia di soletta di BridGHe** (M3, E11, decisione del 4/11 mattina). Prima la griglia usava il G tabellato di BridGHe. Effetto atteso circa 2,5e-4. **Deciso, non ancora nel codice:** entra con la fase (a) di P2B.

### 4. Ecm di GPC

- **Decisione:** M2, 4/11 mattina (E10).
- **Prima:** i componenti di BridGHe danno E tabellati (C25/30: 31.480 MPa). **Ora:** Ecm calcolato da GPC a partire da fck (31.476 MPa).
- Effetto atteso circa 1e-4 su 50 e 99. **Deciso, non ancora nel codice:** entra con la fase (a) di P2B.

### 5. Sezioni composte non saldate: nodi del BXS separati

- **Decisione:** 4/11 pomeriggio, «separare i nodi nel writer». L10, Model `f3ee8807`.
- **Prima:** R31 unisce i vertici della BXS più vicini di circa 2,2e-5 della diagonale della sezione. Due parti a contatto diventavano un corpo solo.
- **Ora:** i vertici a contatto si spostano verso l'interno della loro parte, fino ad almeno 5e-5 della diagonale.
- **Effetto su R31:**
  - 2 L 100×10 a contatto: J da +135% a −2,49%, SA +0,42/+0,36%;
  - 2 C punta contro punta: J da +19.148% a −2,71%;
  - 4 L a croce: J da +314% a −2,78%, SA +0,50%.
- Le sezioni saldate e quelle con le parti staccate non cambiano. Le proprietà GPC scritte non cambiano.
- **Costo:** la BXS perde da 6e-4 a 1,3e-3 dell'area. Conta solo nel recupero delle tensioni di R31. Questo costo l'ha accettato Claude (RIPRESA).
- Nessun esempio (01–10, 50, 99) ha sezioni composte di più parti: nessun risultato degli esempi cambia.

### 6. Offset centrifugo per veicolo

- **Decisione:** 4/11 pomeriggio, «GPC resta in N per veicolo e il writer converte». L10, Model `ec99b709`; BridGHe `307ff68`.
- **Misura su R31:** l'offset si applica a ogni forza con il flag della centrifuga: in N per una forza concentrata, per mm di lunghezza per una forza distribuita.
- **Ora:** il writer scrive «O/misura», dove la misura è il numero di forze concentrate più la lunghezza delle distribuite. Il lettore torna a N per veicolo.
- **Prova nativa** (legge 50 N per veicolo + 0,1 × verticale): 1, 2 e 3 forze concentrate, distribuita e mista danno tutti circa 150 N. Prima, con l'offset scritto com'era, 2 forze davano 200 N e una distribuita da 100 mm 5.100 N.
- **Prova nativa in BridGHe:** offset GPC di 100 N per veicolo in 7 configurazioni di corsie e adiacenza. I fattori di corsia non scalano i casi statici generati. R31 applica l'offset solo su un percorso curvo: sull'arco la reazione laterale è 100,000 N.
- Atteso cambiato: `B8_Straus7DialectStaysInTheConverter`.
- Gli archivi di testo 5.43–5.48 esistono solo sul branch, quindi nessuna conversione.

### 7. `LineLoad.GetGlobalForces` corretto

- **Decisione:** 4/11 pomeriggio, «correggilo». L9, Model `df660262`; `GetLocalForces` in `9b7f26aa`.
- **Prima:** il metodo toglieva l'origine del sistema di riferimento dalle componenti ruotate. Un carico in assi d'asta con origine nel nodo I era sbagliato di −(coordinate del nodo I).
- **Ora:** il metodo ruota le componenti e basta, come `PointLoad.GetGlobalForces`.
- **Chi cambia:** solo il nuovo writer Straus7 del branch, per i carichi distribuiti con assi fuori dall'origine.
- Su origin/master il metodo non ha chiamanti: nessuna conversione in produzione da rifare. Checker, ANTHEA, Arachne, ReBear e BridGHe non cambiano.
- Prove `LineLoadGlobalForcesTest` e `LineLoadStraus7PlanTest`: falliscono sul codice di prima.

### 8. Massa in IPS in libbre-massa

- **Decisione:** 4/11 mattina, «metti quello che pensi meglio». L5, Arachne `94a31f6`.
- **Prima:** in IPS TensileLib usava la libbra-massa come massa del solutore. Il peso risultava 386 volte troppo grande. Lo scenario del peso proprio in IPS di master diverge (−3,3E+43).
- **Ora:** unità di massa del solutore lbf·s²/in: le densità in libbre-massa si dividono per 386,0886. Lo scenario dà −0,464 in.
- Nmm, SI e kNm restano identici bit per bit. `Arachne.Compare`: 31 identici, 1 diverso per decisione.

### 9. Menu Units del parametro Section di BridGHe

- **Dove:** P2B, audit di L9 (`65701c5`) e decisioni del coordinatore (`fbdd9c8`).
- **Prima:** ogni clic dei menu Mirror, Gap e Units aggiungeva un elemento al dato memorizzato. Units convertiva anche ogni dato a monte. Un clic da mm a m dava due sezioni da 6e-7 m e riscalava sul posto le sezioni a monte.
- **Ora:** il menu converte solo il dato memorizzato, una volta. Un clic da mm a m dà una sezione da 0,6 m. I dati a monte non cambiano.
- Nessun esempio usa questo parametro (GUID `0f79b0ba` assente da `example/` e `tests/`): nessun modello di esempio cambia.

### Altre differenze già approvate

| Differenza | Entità | Stato |
|---|---|---|
| E1: J dalla geometria GPC invece che da Straus7 | I del 50 −2,1%; I del 99 −1,3% | approvata (C2) |
| E3: centro di taglio della I monosimmetrica del 99, fase 1 (48 travi) | da −296,316 a −296,389 mm | approvata |
| BXS conforme delle sezioni saldate (L6) | valori di R31 dalla BXS: J del coprigiunto da −64,72% a −2,61%; croce SA da −12,91% a +0,33%. Proprietà GPC invariate | approvata |
| Sezioni in più parti (L4-1) | prima l'export falliva; ora entro lo 0,3% da R31 | corretta |
| SA1 e SA2 scambiate con inerzie quasi uguali (L4-9) | prima scambiate | corretta |
| Centrifuga su forze in assi globali (L4, L6) | R31 applica 0 N; ora il writer la rifiuta a `Prepare` | corretta |
| E4–E9 di P2B: etichette degli assi, ID utente, gruppo radice, modo del modulo di taglio, numerazione dei casi, casi delle fasi | nessun effetto meccanico | solo rappresentazione |
| Righe del Checker contro F1 (P2B, passo 5) | 99: utilizzo 3,2e-7 su 0,17, tensioni 1,6e-4 MPa. 50: utilizzo 2,8e-6 su 0,37, tensioni 1,2e-3 MPa su 125. Nessun esito cambiato | approvate |

### Arachne: comportamenti diversi senza numeri diversi

Dalla nota P2A (§3). Sono tutti su input non validi, messaggi o persistenza:
- diagnostiche GPC in più, come Remark accanto ai testi di master;
- rifiuti nuovi: lunghezza fissata ≤ 0, pesi negativi, gravità negativa, tolleranza 0, curvatura con corda 0;
- due elementi distinti con lo stesso Element Id: errore di Build Model;
- piastre con più di 4 vertici tolte da Build Model, con il messaggio di prima;
- un modello scomposto e ricostruito perde i risultati del form finding precedente;
- la Generic Section non conserva più le forze di taglio, che nessuna uscita mostrava;
- materiali: il tipo (acciaio, calcestruzzo, alluminio) e il G esplicito non sono conservati: non hanno effetto sul form finding.

Cable e FAF sulla stessa asta: 216/216 numeri e 80/80 testi uguali a master (L5).

## Che cosa resta di F6

- BridGHe: C5, C6, fasi (a) e (b), rimozione dei modelli FeMM, merge di master, 0.4.0.
- Applicare E10 (Ecm) ed E11 (G nella griglia di soletta) con la fase (a), e misurarne l'effetto su 50 e 99.
- Completare questo documento con l'esito finale di P2B.
