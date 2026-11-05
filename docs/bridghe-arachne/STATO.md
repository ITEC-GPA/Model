# Stato della migrazione comune

Aggiornamento del 5 novembre 2026, pomeriggio (F8, corsia D8). Sostituisce lo stato del 4 novembre sera.

Fonti, nella cartella `C:\Users\g.pacini\Documents\Repository\gpc-unification-20261102\revisione-claude-20261103`:
- [RIPRESA.md](../../../revisione-claude-20261103/RIPRESA.md): decisioni dell'utente, commit integrati e gate. È la fonte più recente;
- [CORREZIONI.md](../../../revisione-claude-20261103/CORREZIONI.md): ogni finding della revisione del 3/11, con stato, commit e prova, più le decisioni e le differenze nei risultati;
- le note delle corsie `correzioni-*.md`: L1–L7, L9–L12, P2A, P2B (con i passi di P2C–P2I), P2T, PROBE-030, A8, R8, F7a, F7b.

I collegamenti relativi valgono nel workspace `gpc-unification-20261102`. Il piano è in [PIANO.md](PIANO.md).

Altri documenti di F8:
- [REGOLA-GPCMODEL.md](REGOLA-GPCMODEL.md): che cosa contiene GPCModel e dove sta ogni altro dato (nuovo, 5/11);
- [F6-CONCLUSIONE.md](F6-CONCLUSIONE.md): che cosa ha dato F6, per corsia, e le differenze nei risultati approvate;
- [F7-PRESTAZIONI.md](F7-PRESTAZIONI.md): tempi prima e dopo, master contro F6;
- [RITORNO-VERSIONE-PRECEDENTE.md](RITORNO-VERSIONE-PRECEDENTE.md): come si torna alle versioni precedenti e che cosa succede ai file;
- [f8-mappatura-tipi.json](f8-mappatura-tipi.json): dove sta oggi ogni tipo dei vecchi modelli FeMM. Sostituisce la matrice F5.

## In breve

| Fase | Fatto | Resta |
|---|---|---|
| F0–F5 | Workspace isolato, riferimenti F1, contratti, persistenza, piloti, writer Straus7 nel Converter | La matrice F5 è superata da `f8-mappatura-tipi.json` |
| F6 | Librerie corrette e integrate (L1–L7, L9–L12, più le correzioni della sessione Azioni). Arachne senza modelli FeMM, pronta per il rilascio (A8) | **BridGHe in corso** (P2I): catena E, tipi GH su GPC, rimozione dei FeMM, merge di master |
| F7 | Set candidato F7b dalle teste codex (GPCModel 5.50.0.0); ANTHEA 1.19.0 standard, nativa e UI verdi su quel set; ReBear identico bit per bit (R8) | Va rifatto sul set ufficiale in F8 |
| F8 | Documentazione (questo aggiornamento), piano di rilascio deciso dall'utente | Tutto il rilascio, dopo la fine di BridGHe |

**Stima data all'utente (5/11 pomeriggio):** 9–13 giorni lavorativi fino al rilascio comune, cioè fra il 18 e il 24 novembre. BridGHe pesa per la maggior parte; F7 1 giorno, F8 2–3 giorni.

Nessun push, tag, installer o installazione è stato fatto per questo lavoro. Tutto sta sui branch.

## Decisioni del 5/11 che cambiano l'architettura

- **Il vocabolario del ponte esce da GPCModel** («Toglilo prima»), **per composizione** («Composizione»). La corsia L12 l'ha fatto:
  - `BridgeModel` = `Model` GPC generico + `BridgeModelData`, nel Checker CompositeBridge. Non è una classe derivata da `Model`;
  - `BridgeModelData` tiene un `BridgeBeamData` per GUID di trave (membro, superelemento, stato di sezione, sezione composta) e la ricetta D3;
  - `Layout.BridgeMemberMetadata`/`BridgeMemberFamily` e `CompositeSectionDefinition` passano da GPCModel a CompositeBridge;
  - `RigidConnectionAttribute` resta in GPCModel, in `GPC.Model.Attributes`;
  - i tipi del generatore del ponte di F5 sono cancellati (nessuno li usava).
- **Regola di GPCModel** (utente): GPCModel contiene struttura, carichi, analisi e risultati; i dati delle verifiche e del dominio stanno nelle librerie di dominio. Dettagli in [REGOLA-GPCMODEL.md](REGOLA-GPCMODEL.md).
- **Form finding:** resta in GPCModel. «per ora teniamo il form finding dentro model. forse lo separeremo. ma non oggi».
- **Converter:** «diventerà complesso, ma è normale».
- **«Aspetta la 0.4.0»:** nessun rilascio intermedio con BridGHe 0.3.1. ANTHEA 1.19.0, Arachne, ReBear e BridGHe 0.4.0 escono insieme, sul set ufficiale. La sonda PROBE-030 (0.3.0 sulle librerie nuove: risultati identici) resta come prova di riserva.
- **Nella 1.19.0 entrano** tre correzioni di libreria chieste dalla sessione Azioni: planarità relativa delle piastre, pressioni variabili scritte in Straus7 come forze nodali equivalenti («Sì, forze nodali») e classe del calcestruzzo da «fc = N MPa» («Sì, nella 1.19.0»).
- **Barre della trave composta: «Dopo il rilascio».** La 0.4.0 tiene nell'analisi una barra per fila, come la 0.3.0. Le verifiche usano tre barre per fila: è un'incoerenza che esiste già nella 0.3.0 ed è nel changelog.
- **Dopo il rilascio** (decisioni dell'utente, da non fare ora):
  - un solo formato di salvataggio: si scrive solo il compatto, i vecchi formati restano in sola lettura («con la prossima versione ne imponiamo 1 e togliamo le altre»);
  - revisione dell'API di GPCModel (circa 10.000 firme): una sola API di copia con un oggetto di opzioni, e i tipi «ponte» già su master;
  - forse la separazione del form finding;
  - BridGHe 0.5.0 con l'armatura della soletta distribuita: diametro + passo per la fila superiore e per quella inferiore, una sola sezione per analisi e verifiche, il componente vecchio nascosto. L'utente accetta che I22 cambi.

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
| L9 | Proprietà e sistemi di riferimento condivisi nelle copie, `LineLoad`, writer riusati | Model `9735e5ab`, `1ab5ac64`, `df660262`, `3312b802`, `9b7f26aa`, `9a91a9b9`; Arachne `6a2deef`, `57f0fd7`, `611a3d8` |
| L10 | Decisioni 5, 6 e 9 dell'utente | Model `f3ee8807`, `ec99b709`; Checker `d85a9fda` |
| L11 | Contratti per i dati del ponte; copia con un'identità di modello nuova | Model `61bd8251`, `9736405e` (`ModelValues.Copy(model, options, Guid)`); Checker `e7fee77c` (`BridgeCompositeSectionDefinition`) |
| L12 | Vocabolario del ponte fuori da GPCModel, `BridgeModel` per composizione | Model `30d776a7`, `f98a6e39`; Checker `e87a2ffd`, `d018ff28` |
| Sessione Azioni | Planarità relativa delle piastre (`ShellPlanarity`), pressioni variabili come forze nodali equivalenti, classe del calcestruzzo da «fc = N MPa» | Model `d52b93a3`, `92537906`, `59d2b9b5`, `fd89a333`, `81a62065` |
| P2G | `ModelValues.Renumber(Model, Func<Element,long>)` per Construction Sequence | Model `75f5a370` |
| P2H | La definizione composta porta le file di barre e l'acciaio d'analisi | Checker `28d8b967` (`BridgeCompositeRebarRows`, `AnalysisSection()`), `e90e9162` (`AnalysisSteel`) |
| A8 | `Directory.Build.props` di Model: `ShouldUnsetParentConfigurationAndPlatform=false` | Model `bc5a7a07` |

Il dettaglio per corsia è in [F6-CONCLUSIONE.md](F6-CONCLUSIONE.md) e nelle note delle corsie.

### Arachne: finita, pronta per il rilascio

- Corsia P2A (Arachne `321b6d0`), poi L5, L9 e A8.
- `Arachne.Common/Models` è tolto: 32 file FeMM. I data type GH portano oggetti GPC. GUID e porte dei componenti sono invariati: inventario 66/66.
- Il lettore NRBF è tolto. I vecchi payload sono rifiutati con un messaggio.
- I risultati del Form Find stanno nel file accanto `<documento>.<InstanceGuid>.gpc-results`.
- **A8** (branch `claude/f8-arachne`, ora in codex `bf2d6bc`):
  - i 6 esempi distribuiti (01–03, `.gh` e `.ghx`) non contengono dati internalizzati di Arachne. Aperti, risalvati e ripristinati dal file accanto danno gli stessi numeri del bin 0.3.0-rc.8 del server, bit per bit. Restano come sono;
  - corretto un difetto del bin: `build.ps1` metteva nel plug-in Release le DLL GPC compilate in Debug (`64e4616`). Ora lo script controlla le 12 DLL del bin, blocca Gmsh e NRBF, e con `-ReleaseSet` confronta gli SHA-256 con il set;
  - README, CHANGELOG (sezione 0.4.0), ARCHITECTURE, PORTING ed `examples/README.md` aggiornati (`abe93ba`, `bf2d6bc`);
  - versione proposta **0.4.0**, senza rc. Non è ancora alzata.
- `ShellPlanarity` non arriva ad Arachne: Arachne legge solo le posizioni dei nodi.

### BridGHe: in corso

Corsia P2B, poi P2C–P2I, cartella `gpc-f6-claude/P2B`, branch `claude/f6-bridghe`, testa `b579e10`. Note in `correzioni-P2B.md`. In parallelo la corsia P2U riscrive i test Smoke su GPC (branch `claude/f6-bridghe-tests`).

Fatto, ogni passo con un confronto bit per bit contro il percorso FEM:
- analisi ed export Straus7 dal writer del Converter; binding St7API e vecchio exporter tolti (passi 0–7);
- aree a taglio a ν = 0, travi composte omogeneizzate (E13), Ecm di GPC (E10), G = E/2(1+ν) nel graticcio (E11);
- risultati delle analisi in `<output>.bghr` (formato `BGHR2`) accanto al `.st7`; i componenti Results leggono i dati GPC e non ripiegano più sui FEM (R7);
- Super Element Check su D3 con un piano `Prepare` per workflow; modello di ponte costruito solo da dati GPC (catena C, `FemmBridgeSeam` tolta);
- catena D: Attach Moving Paths, Phase Model e Construction Sequence su GPC (rinumerata 1..n con `ModelValues.Renumber`);
- R3, R8 (inviluppi, Result Cases, fatica, controfreccia, reazioni, Girder Results) su GPC;
- migrazione di L12: i dati del ponte vanno in `BridgeModelData`, chiavi = GUID delle travi; `GpcGooArchive` ha `Kind.BridgeModel` nella busta `BGHGPC1`; Build Model dà errore per una trave in due superelementi;
- catena E, punti 1–2 e (c): `GH_CompositeBeamProperty` porta la `BridgeCompositeSectionDefinition`; ritiro, proprietà geometrica e Self Tension la usano; `GH_CompositeBeam` porta le definizioni d'inizio e fine, lette da Super Element Check, Linear Tension e anteprima.

Resta:
1. catena E (d), **in corso e non committato**: Bridge State Override su GPC (`GH_Beam` con `BeamElement` + `BridgeBeamData`);
2. catena E (e): `GH_Beam`, `GH_Node` e `GH_Plate` su GPC, Build Model che raccoglie i `BridgeBeamData` in `BridgeModelData`, materiali M1, persistenza `BGHGPC1` di tutti i tipi GH, punto di controllo prima della rimozione;
3. rimozione dei modelli FeMM (`BridGHe.Common/Models`, 55 file, ancora tutti presenti a `b579e10`) e della giuntura `FemmAnalysisSeam`, con R6;
4. merge di master (`87175b7`, `d7e4092`, `95ab7c3`), con i conflitti elencati al passo 12 di P2B;
5. versione 0.4.0 (master è 0.3.0).

Impronte del percorso D3, invariate a ogni passo dal passo 18: 99 `CB903536`, 50 `99FA70F1`. Le impronte del percorso FEM (`EB5A1500`, `F0B6964B`) restano di riferimento. Per il 50 il riferimento della 0.3.0 di master è `BBF53DEB…` (PROBE-030), non più F1: `95ab7c3` ha sostituito il REV003.

Le voci del changelog 0.4.0 raccolte finora sono in `correzioni-P2B.md`, «Voci del changelog 0.4.0 raccolte (P2G)».

### ReBear: pronto per il set ufficiale

- Corsia R8, branch `claude/f8-rebear` da master `f2df907` (0.3.3), commit `9ce8d1c`: README e `docs/MIGRATION.md` per la **0.3.4**, con i segnaposto `[F8: …]`.
- ReBear 0.3.3 si compila sulle teste codex (Model `e1fd8042`, Checker `d018ff28`) senza modifiche al sorgente. Tutto è identico bit per bit al set del 2/11: Verify 41/41, i 20 esempi, le proprietà di sezione e i dati internalizzati, letti e scritti nei due sensi.
- Cambia solo il set GPC in `lib/GPC`. Il branch F7a `claude/f7-rebear` (`63a78f4`) faceva lo stesso sul set F7a; R8 è più recente e riparte da master.

## F7 — integrazione

Corsie F7a (4/11) e F7b (5/11), cartella `gpc-f6-claude/F7`. Note in `correzioni-F7a.md` e `correzioni-F7b.md`.

### Set candidato F7b

- Cartella `gpc-f6-claude\F7\sets\gpc-candidate-20261105-f7b`, 15 DLL uguali al manifest, nessuna DLL di gmsh.
- Prodotto con `Release-Set.ps1 -Candidate` da Model `7a06643b` e Checker `ef50a212` (branch `claude/f7b-set`), cioè le teste codex `bc5a7a07` e `d018ff28` più le versioni.
- Versioni: GPCModel 5.50.0.0, GPCModelData 0.3.0.28, GPC.Model.Persistence 3.15.0.0, GPC.Archive 1.2.0.1, GPC.Converter 2.20.0.0, GPCChecker.CompositeBridge 1.28.0.0, GPCModelChecker 2.39.0.1; Concrete, Steel, Geotechnics, Masonry e Design all'ultima componente.
- Le 26 rimozioni di API di L12 contano come «altro»: seconda componente, nessun salto della prima.
- **Il set è già indietro**: il codex Checker è andato avanti a `e90e9162` (P2H, solo aggiunte a CompositeBridge), e BridGHe non è finito. F8 rifà versioni e set.

### ANTHEA 1.19.0

- Branch codex di ANTHEA, testa `94f48d20`: contiene origin/main fino a `8ef58180` (Rev70 delle guide compresa) e una correzione di compilazione: `BridgeModel` era ambiguo fra `GPC.Design.Bridges` e il nuovo `GPC.Checkers.CompositeBridge`, risolto con un alias di file.
- Commit F7a: `31c5c5cb` (sezioni c.a. nel formato compatto, documento versione 5), `18d96c67` (numero 1.19.0), `566b2983` (cattura `cattura-mainwindow` aggiornata, con l'ok dell'utente).
- Sul set F7b (`claude/f7b-anthea`): profilo standard 56/57 (solo la prova F4 attesa), `Straus7Native.Checks` nativa 113 controlli PASS, UI 73/73 al primo colpo.
- Sezioni 5–27 volte più piccole; la 1.18.0 vera apre un file della 1.19 con sezioni compatte e dice «Formato o versione del file non supportati.».
- Prima della full va riunito origin/main, se si è mosso.

### Stesso processo Rhino

In uno stesso Rhino la prima DLL GPC caricata vale per tutti i plugin. Oggi i bin di BridGHe (P2B), Arachne (A8) e ReBear (R8) hanno GPCModel 5.48.0.0 con SHA-256 diversi. F8 mette i tre plugin sullo stesso set; la prova in Rhino 8 con i tre plugin caricati, nei due ordini, si fa con il set ufficiale.

### Tempi

Le tabelle sono in [F7-PRESTAZIONI.md](F7-PRESTAZIONI.md). In sintesi:
- Arachne: costruzione, salvataggio e apertura più veloci di master; Form Find a 60.000 nodi alla pari con master (55,2 s contro 54 s);
- Arachne: restano più lenti Duplicate, Force Density, il Form Find a 10.000 nodi (+1–2 s) e il salvataggio subito dopo un Form Find, su .NET Framework;
- BridGHe, esempio 50: D3 con il piano preparato una volta passa, nel ciclo C5 in Rhino, da 1.202 a 223 s;
- ANTHEA: salvataggio e apertura invariati, entro il rumore.

## F8 — documentazione e rilascio

### Piano di rilascio deciso dall'utente (4/11 sera, confermato il 5/11)

**«Tutto insieme».** Si aspetta la fine di BridGHe. Poi, in quest'ordine:
1. set ufficiale delle librerie da master:
   - versioni nuove sopra quelle di F7b, con la regola del 12/10 (fix → ultima componente, altro → seconda, resto a zero);
   - `Release-Set.ps1` senza `-Candidate`, da un checkout pulito, con `-Previous` uguale al `release-set.json` di AU;
2. ANTHEA 1.19.0 con il set ufficiale e il profilo full;
3. BridGHe 0.4.0, Arachne 0.4.0 e ReBear 0.3.4 sullo stesso set;
4. se tutte le prove passano («Sì, push e pubblica»):
   - push e tag;
   - installer di ANTHEA in `\\STORAGESARZANA\Varie\20 - AGGIORNAMENTI_SOFTWARE\Anthea`, con la 1.18.0 spostata in `_SUPERATI`;
   - bin dei plugin ricostruiti con Rhino chiuso;
   - il file `gpc-f6-claude/INT/rilascio-plugin-pronto.json`, scritto solo a push fatto e bin ricostruiti.
5. La copia dei plugin sul server `X:\ITEC-MODELLI\5-RHINO\SGBIM-GR_PLUGIN\<plugin>` la fa il compito programmato delle 02:00 (`bridghe-030-copia-server`), che legge `rilascio-plugin-pronto.json`. Il contenuto vecchio va in una cartella da eliminare.

Plugin da ricostruire: BridGHe, Arachne e ReBear. Rhino2FEM, Pawper, Ant e Meerkat non usano Model né Checker, e Geometry e Utilities non cambiano: per loro non serve una versione nuova.

### Altri passi di F8

- Costruire il set da un checkout pulito: nel worktree codex di Model 22 file hanno a capo LF dove `.gitattributes` chiede CRLF (R8), e gli a capo entrano nelle DLL attraverso i PDB.
- Ricostruire `GPC.Archive` dal commit finale (SourceLink) prima del set.
- Registrare il set ufficiale in ANTHEA (`tests/fixtures/f4/library-release.json`): la prova F4 di `Architecture.Tests` passa solo con un set ufficiale.
- Un worktree nuovo di ANTHEA ha bisogno delle cartelle d'ingresso delle guide in `supporto/artefatti` (`guide_anthea_itec_rev03`, `-rev04`, `-rev47`, `globale-guidata-20260930`, `muri-materiali-distinta-20261005`), altrimenti `wiki/indice` e `wiki/manuale` falliscono.
- Arachne: rilascio con `scripts/build.ps1 -RhinoTests -ReleaseSet <set ufficiale>\release-set.json`; alzare la versione e cambiare il commento di `Directory.Build.props` («F6 development candidate… not a release»).
- ReBear: `lib/GPC` e `release-set.json` completo dal set ufficiale, `<Version>0.3.4</Version>`, segnaposto `[F8: …]`, `results.json` degli esempi.
- BridGHe: uscire da un ramo con `PluginBuildCheck.cs` = blob `bad6968` e con master unito. Il codex BridGHe `a0dbd4d` ha ancora la copia vecchia.
- Provare il merge su master in ogni repository e provare i pacchetti in un ambiente pulito.
- Dopo i merge, ricostruire i bin che Rhino carica.
- Togliere i rami e i worktree uniti, e le cartelle di prova (PROBE, `P2A/mb`, `R8`, `F7`): le cancellazioni definitive le fa l'utente.

### Che cosa solo F8 può scrivere nei documenti

- versioni finali delle DLL e dei prodotti, SHA-256 del set ufficiale e del suo `release-set.json`, nome del set (`gpc-release-2026MMDD-xxxxxxxx`);
- commit e tag finali di Model, Checker, ANTHEA, BridGHe, Arachne e ReBear;
- data del rilascio;
- nome, dimensione e SHA-256 dell'installer di ANTHEA 1.19.0;
- esiti delle suite sul set ufficiale, compresa la full di ANTHEA e la prova dei tre plugin in Rhino;
- il changelog finale di BridGHe 0.4.0.

## Commit integrati, per repository

Branch `codex/gpc-unification-20261102`, worktree in `gpc-unification-20261102/<repo>`. I conteggi sono fatti il 5/11 contro i riferimenti locali di origin.

| Repository | Testa | Contro master | Integrazioni dopo il 4/11 sera |
|---|---|---|---|
| Model | `bc5a7a07` | 107 avanti, 0 indietro rispetto a origin/master `2f934f9f` | `61bd8251`, `9736405e` L11; `d52b93a3`, `92537906` planarità; `59d2b9b5`, `fd89a333` forze nodali; `78aa14da` merge del ramo Model di P2G (L12 `f98a6e39` + `Renumber` `75f5a370`); `e1fd8042` merge delle classi fc (`81a62065`); `bc5a7a07` proprietà di build |
| Checker | `e90e9162` | 20 avanti, 0 indietro rispetto a origin/master `6156e4ce` | `e7fee77c` L11; `e87a2ffd`, `d018ff28` L12; `28d8b967`, `e90e9162` P2H |
| ANTHEA | `94f48d20` | 19 avanti, 0 indietro rispetto a origin/main `8ef58180` | `264173aa` Rev70 delle guide, `187ea6f5` merge di origin/main, `94f48d20` alias di `BridgeModel` |
| Arachne | `bf2d6bc` | 31 avanti, 0 indietro rispetto a master `39c54a2` | `6fa88f4`, `64e4616`, `ca16932`, `abe93ba`, `bf2d6bc` (A8) |
| tensilelib | `5e25765` | invariato | — |
| BridGHe | `a0dbd4d` (WIP di ChatGPT) | — | Nessuna: il lavoro è su `claude/f6-bridghe` (`b579e10`), da integrare a fine BridGHe |
| ReBear | — | — | `9ce8d1c` su `claude/f8-rebear`, nel workspace R8 |
| Versioni F7b | — | — | Model `7a06643b`, Checker `ef50a212` su `claude/f7b-set`, non in codex |

## Gate più recenti

| Dove | Commit | Esito |
|---|---|---|
| Model, `build/Verify.ps1` nativo (`int-14`) | codex `e1fd8042` | 63 + 1.950 + 141 PASS, 7 ignorati come sempre; GPCModel 26 rimozioni di API approvate (L12), nessuna inattesa |
| Model, `build/Verify.ps1` non nativo (A8) | `bc5a7a07` | 63 + 1.862 + 141 PASS; API 0 rimozioni inattese |
| Checker, `Verify.ps1 -Suites all` (`int-14`) | codex `d018ff28` su Model `e1fd8042` | tutte le suite PASS: 10.411 (CompositeBridge 469, ModelChecker 674, Steel.Members 7.132) |
| Checker, `Verify.ps1 -Suites all` (P2H) | `e90e9162` | 10.415 PASS, API con sole aggiunte |
| Set candidato F7b | Model `7a06643b`, Checker `ef50a212` | Model nativo 63 + 1.950 + 141; Checker 10.411; 15 DLL uguali al manifest |
| ANTHEA (F7b) | `36b45c11` = codex `94f48d20` + `lib/Checker` F7b | standard 56/57 (solo F4); `Straus7Native.Checks` 113 controlli PASS; UI 73/73 |
| Arachne (A8) | `abe93ba` su Model `e1fd8042` | headless 164, ARAP 54, `Arachne.Compare` 31 + 1 (IPS), esempi 6/6 |
| BridGHe (P2I, `p2i1`/`p2i2`) | `b579e10`, Checker `e90e9162`, Model `bc5a7a07` | 99 `CB903536`, 50 `99FA70F1`, `--core-only` 269, `--core-only --native` 392, `--bridge-native` PASS, `--regression all` 89 (fallisce solo `gpc-release-set`, atteso con GpcPilot) |
| ReBear (R8) | `9ce8d1c` sulle teste codex | Verify 41/41, esempi 20/20 identici; dati internalizzati 73/73 nei due sensi |
| BridGHe 0.3.0 sulle librerie nuove (PROBE-030) | `f5b9bc4` | audit 152/229, core 269, regression 84, esempi 10/10, core nativo 378, 99 e 50 identici foglia per foglia (2.736 e 66.960 righe) |

I log sono in `gpc-f6-claude/INT/results` e nelle cartelle delle corsie.

## Che cosa non è ancora verificato

- BridGHe dopo la catena E e la rimozione dei FeMM: numeri, `.gh` con i contratti GPC (`BGHGPC1`), prestazioni, salvataggio dell'esempio 50 nel formato finale.
- I tre plugin nello stesso Rhino 8, sul set ufficiale, nei due ordini di caricamento.
- I tempi di Arachne e BridGHe in Rhino 8 su .NET 8: le misure di Arachne sono tutte su .NET Framework.
- Il set ufficiale, il profilo full di ANTHEA, la prova in un ambiente pulito e il ritorno alla versione precedente con i pacchetti veri.
- Le domande ancora aperte sono in CORREZIONI.md, «Domande aperte».
