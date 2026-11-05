# Che cosa sta in GPCModel e dove stanno gli altri dati

Scritto il 5 novembre 2026 (F8, corsia D8). Fonti: [RIPRESA.md](../../../revisione-claude-20261103/RIPRESA.md), sezioni del 5/11 «Decisioni dell'utente, 5/11: struttura di GPCModel» e «Forze nodali equivalenti per `NonUniformPlatePressure`»; `correzioni-L12.md` §1–§2 nella stessa cartella.

## La regola

Decisione dell'utente del 5/11: **GPCModel contiene struttura, carichi, analisi e risultati. I dati delle verifiche e del dominio stanno nelle librerie di dominio** (Checker e simili).

Con questa regola l'utente ha deciso anche:
- **«Toglilo prima»:** il vocabolario del ponte esce da GPCModel prima del rilascio;
- **«Composizione»:** un modello di ponte è un `Model` GPC generico più i dati del ponte, nella libreria del ponte (Checker CompositeBridge). Non è una classe derivata da `Model`;
- **form finding:** «per ora teniamo il form finding dentro model. forse lo separeremo. ma non oggi». È la regola attuale e un punto aperto per dopo il rilascio.

La corsia L12 ha applicato la regola al vocabolario del ponte: Model `30d776a7` e `f98a6e39`, Checker `e87a2ffd` e `d018ff28`, integrati in codex.

## Dove sta ogni dato

| Dato | Dove | Tipi principali |
|---|---|---|
| Struttura: nodi, travi, piastre, solidi, materiali, sezioni, proprietà, gruppi, sistemi di riferimento | GPCModel | `GPC.Model.Models.Model`, `GPC.Model.Elements`, `GPC.Model.Sections`, `GPC.Model.Materials`, `GPC.Model.ElementProperties` |
| Vincoli, molle, spostamenti imposti, collegamenti rigidi | GPCModel | `RestrainAssignment` con `DofRestrain.Prescribe`, `GPC.Model.Constraints.RigidLink`; in bozza, prima del modello, `GPC.Model.Attributes.RigidConnectionAttribute` |
| Membri fisici generali (sequenze ordinate di parti FE) | GPCModel | `PhysicalMemberDefinition`, `BeamMemberPart`, `Model.PhysicalMembers`, `PhysicalMemberPath`. Senza metadati del ponte: `PhysicalMembers.cs` è tornato quello di master |
| Etichette degli elementi | GPCModel | `ElementLabelAttribute` (il writer Straus7 non le scrive) |
| Carichi, casi, combinazioni, fasi, carichi mobili | GPCModel | `GPC.Model.Loads`, `GPC.Model.LoadCases`, `GPC.Model.Stages`, `GPC.Model.Actions.Moving` (`MovingLoadDefinition`) |
| Analisi e risultati, compreso il form finding | GPCModel | `GPC.Model.Analysis` (`AnalysisSemantics.FormFinding`, `FormFindingDefinition`, `FormFindingGoalAttribute`, `FormFindingCoordinateAttribute`), `GPC.Model.Results` |
| Meccanica della sezione composta | GPCModel | `CompositeSectionProperties`, `SectionShearAreas` (anche `ComputeHomogenised`) |
| Regole generali del modello | GPCModel | `ModelPreflight` (`ValidateDomain`, `RequireExportable`), `ShellPlanarity`, `ModelValues.Copy`, `ModelValues.Renumber` |
| Salvataggio dei modelli e dei valori | GPC.Model.Persistence, GPC.Archive | `ModelArchive` (XML o compatto v6), `ModelValueArchive` (v2), `ExternalModelArchive` e i blocchi di GPC.Archive per i risultati accanto al documento |
| Scrittura e lettura Straus7 | GPC.Converter | writer e lettore Straus7, dialetto R3, classi del calcestruzzo da «fc = N MPa» |
| **Dati del ponte** | **Checker CompositeBridge** | `BridgeModel` = `Model` + `BridgeModelData`; un `BridgeBeamData` per GUID di trave con `Member` (`Layout.BridgeMemberMetadata`), `SuperElement`, `SectionState` (`BridgeLocalSectionState`) e `CompositeSection` (`BridgeBeamCompositeSection`); la ricetta D3 `BridgeWorkflowDefinition` in `BridgeModelData.Workflow` |
| Sezioni composte del ponte | Checker CompositeBridge | `BridgeCompositeSectionDefinition` (con `BridgeCompositeRebarRows`, `AnalysisSection()` e `AnalysisSteel`); `CompositeSectionDefinition` per il ritiro (`CompositeShrinkageActions`) |
| Verifiche | Checker | `GPCChecker.Concrete`, `GPCChecker.Steel`, `GPCChecker.CompositeBridge`, `GPCChecker.Geotechnics`, `GPCChecker.Masonry`, `GPC.Design`; il cancello dei modelli con errori di dominio è in `GPCModelChecker` (`InvalidModelForVerification`) |
| Dati dei plugin | nel plugin | unità della definizione, anteprime, dati solo per la visualizzazione (in BridGHe `PreservedSourceData`), interfaccia |

## Come si salva un modello di ponte

- Il `Model` va nei formati esistenti di `ModelArchive` (XML o compatto). Nessun formato nuovo.
- I dati del ponte hanno il loro XML dei contratti (`BridgeModelData.Save`/`Read`), validato in lettura.
- Il pacchetto `BridgeModel.Save(Stream)` / `BridgeModel.Load(Stream)` è un solo flusso: intestazione `GPCBRG` versione 1, lunghezza e byte dei dati del ponte, poi l'archivio del modello come lo scrive `ModelArchive.Save`.
- In BridGHe il pacchetto sta nella busta `BGHGPC1` dei dati internalizzati (`GpcGooArchive`, `Kind.BridgeModel`).
- I dati del ponte non arrivano al writer Straus7 né all'impronta d'analisi: il writer riceve `bridge.Model`.

## Che cosa resta in GPCModel senza seguire del tutto la regola

Sono tipi già su master. Toglierli romperebbe l'API di master, quindi L12 non li ha toccati. Si guardano nella revisione dell'API di GPCModel dopo il rilascio (decisione dell'utente); che cosa farne non è deciso.
- Tipi «ponte» di master: `ReinforcedConcreteSection.CreateBridgeSection`, `RoadBridgeCombinations`, `StandardAASHTO`, `TransverseDistribution`, `ClimateActions` con `BridgeDeck`/`BridgeSurfacing`, `SectionBoxGirder`.
- Su master GPCModel ha anche i namespace `GPC.Model.Checking` (contratti, preparazione, scenari, esecuzione e rapporti delle verifiche), `GPC.Model.Design`, `GPC.Model.Geotechnics` e `GPC.Model.Standards`. L12 non li ha esaminati.
- Il form finding resta per decisione dell'utente (sopra).

## Dopo il rilascio

Decisioni dell'utente, da non fare in questo rilascio:
- revisione dell'API di GPCModel (circa 10.000 firme), con una sola API di copia e un oggetto di opzioni, e i tipi «ponte» di master;
- un solo formato di salvataggio: si scrive solo il compatto, i vecchi formati restano in sola lettura. Il pacchetto `GPCBRG` segue `ModelArchive` senza cambiare; per i dati del ponte basta un codec nuovo dietro la versione 2 dell'intestazione;
- forse la separazione del form finding;
- l'unificazione delle due definizioni di sezione composta di CompositeBridge (`CompositeSectionDefinition` e `BridgeCompositeSectionDefinition`), insieme alla revisione dell'API (decisione del coordinatore, L12 §7).
