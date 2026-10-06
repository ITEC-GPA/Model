# Contratti, capacità e migrazione

## Audit e confini

Lavoro nel repository `Model`, senza modifiche ai repository delle dipendenze. La build iniziale del progetto MSTest ha eseguito 506 test, tutti superati. `Analisi_GPC_Model_per_Anthea.md` e istruzioni `AGENTS.md` applicabili a questo repository non sono stati trovati. Non sono stati aggiornati framework, linguaggio o pacchetti.

Dipendenze effettivamente lette e utilizzate:

- `../Geometry/GPCGeometry` e `../Geometry/DelaunayMesh`, assembly Release `netstandard2.0`.
- `../Utilities/GPCUtilities`, assembly Release `netstandard2.0`.
- `../Checker/GPCChecker.Concrete`: sorgenti di `SectionCheckerModelCode2010`, `SectionCheckerAttribute`, `SectionSolver` e risultato `FailureDomain`; DLL presente versione `0.0.13.0`.
- Convertitori legacy trovati in `../FeMM 30-5-2024/Test1/FeMM/FemToRhino/CSI` e `Straus7`, oltre a vecchi progetti Rhino2FEM/Rhino2Midas. Dipendono da Rhino/API proprietarie. Il nuovo adattatore SAP riutilizza il layout effettivamente letto in `SapInteractiveDatabaseModel.cs` senza portare queste dipendenze nel dominio.
- Persistenza Anthea letta in `../ANTHEA/X.Core/Archivio.cs`: archivio JSON `formato X`, versione 1, `ProjectRevisions.Pack/Unpack`. Non contiene attualmente questo grafo GPC. L'archivio XML qui implementato è utilizzabile senza UI, ma **non è ancora integrato nel salvataggio del progetto Anthea**. Nessuna modifica alla UI o all'archivio Anthea.

Flusso delle dipendenze:

```text
GPC Converter -> GPCModel <- GPCModelChecker -> GPCChecker.Concrete
                   ^
                   |
              esempio / test
```

`GPCModelChecker` e l'esempio non sono dipendenze della soluzione principale. L'esempio include l'adattatore solo se trova la DLL del checker. Nessun nuovo solutore FEM o verificatore normativo è implementato; i solver legacy di Model rimangono separati dai servizi di post-processing.

## Identità, topologia e modifiche

`NodeElement`, `BeamElement`, `AreaElement` e `VolumeElement` usano identità FEM (`Guid` e tipo concreto) per uguaglianza/hash. Due nodi coincidenti sono distinti. L'ID intero è una chiave di registro: non è una posizione nell'array. `SourceIdentity` distingue programma, modello/revisione, famiglia e ID testuale originale, senza confondere nodo 10 e beam 10.

`Model.ConnectBeam(beamId, nodeI, nodeJ)` e `ConnectShell(shellId, nodeIds)` risolvono riferimenti agli stessi oggetti del registro. `BeamElement.StartPoint/EndPoint` derivano dai nodi collegati; le proprietà restano compatibili per i beam legacy non collegati. `AreaElement.Shape/Points` derivano dai nodi quando collegati. Non si può assegnare un'altra `Shape` a una shell collegata: modificare i nodi. `AddMesh` usa ora la mappa dei vertici disponibile per collegare esplicitamente le facce.

`GetConnectedNodes`, `GetAdjacency`, `FindBySource`, i registri per ID e `Element.Groups` espongono la topologia. `GetAdjacency` costruisce un indice snapshot in O(nodi + connettività), senza unire coordinate e senza includere il nodo di orientamento fra i nodi cinematici.

`GetGroupElements(name, includeChildren, family)` seleziona elementi misti, elimina doppioni e filtra la famiglia. `AssignGroup` è idempotente e valida tutti i riferimenti prima di mutare; `UnassignGroup`, `RenameGroup`, `ReparentGroup` e `RemoveGroup` mantengono registro, chiavi dei membri e gerarchia coerenti. La rimozione non cancella elementi FEM o sottogruppi; richiede `DetachMemberships` per gruppi non vuoti. `ValidateGroups`, richiamato dalla validazione topologica, rileva anche corruzioni fatte attraverso le collezioni legacy. Le zone di armatura che usano il nome di un gruppo vengono aggiornate alla rinomina.

I gruppi organizzativi non fanno più parte di `AnalysisFingerprint`: un cambio di appartenenza non cambia il problema FEM. Le selezioni usate da una verifica hanno un proprio `ScopeFingerprint`. Questa correzione cambia l'impronta rispetto agli archivi degli incrementi precedenti: i loro risultati possono risultare obsoleti e devono essere riconciliati con la sorgente, senza riscrivere l'impronta automaticamente.

`RemoveNodeChecked` blocca nodi incidenti, riferimenti di orientamento, link e vincoli. Non elimina in cascata. Il collegamento errato di un beam viene rifiutato prima di modificarne gli estremi. La validazione rileva riferimenti non appartenenti al registro, duplicati sorgente, coordinate non finite, beam degeneri/formulazioni non supportate, shell degeneri/non planari e quadrilateri con orientamento non convesso/coerente.

**Compatibilità:** le collezioni pubbliche legacy restano mutabili. `SortedCollection.Add` conserva la precedente sostituzione per ID; `Replace` richiede lo stesso ID. Non usare queste operazioni per sostituire un nodo referenziato: modificare l'oggetto esistente oppure riconciliare esplicitamente i riferimenti. `ValidateTopology` intercetta riferimenti pendenti; `AnalysisFingerprint` osserva le modifiche anche fatte dai setter legacy. Non c'è una transazione generale per tutte le vecchie API di modifica/rimozione.

L'importatore crea sempre un candidato separato. Duplicati nodali identici producono una nota; coordinate diverse per lo stesso ID producono rifiuto. Duplicati beam/shell sono rifiutati: non esiste un aggiornamento automatico del modello attivo. Le sezioni/proprietà condivise sono oggetti GPC esistenti, non un secondo dominio.

## Assi, unità e segni

Non esiste una nuova classe di sistema di riferimento. I campi `SectionAxes`, `LayerAxes`, `OffsetAxes` e tutti i riferimenti di azioni, molle e vincoli sono `CoordinateSystem`.

`Axes.Validate` richiede una terna ortonormale destrorsa e origine finita (errore aggregato massimo `1e-8`). `Axes.Beam(I,J,direction1,rotationRadians)` costruisce `V3` da I verso J e richiede una direzione trasversale esplicita, anche per beam verticali. Una direzione assente/parallela non viene sostituita con un asse globale arbitrario. Il campo legacy `RotationAroundFirstAxis` è persistito in radianti; il riferimento completo autorevole è `Assignments.SectionAxes`. Quando si costruisce una sezione ruotata, assegnare la terna prodotta da `Axes.Beam`; lo scalare da solo non ricostruisce una convenzione sconosciuta del solutore.

Convenzione GPC dei beam:

```text
F = (V1, V2, N)     M = (M1, M2, T)
N e T lungo/attorno a V3; regola della mano destra.
```

Le stazioni verificabili dichiarano `Body = PositiveSectionFace`, riferito alla faccia di taglio canonica del beam. `OnNode`, azioni di estremità o corpo sconosciuto non sono accettati come sollecitazioni sezionali. La conversione dalla faccia/corpo di un solutore deve essere validata dal suo adattatore; il nome di una colonna non basta.

Ordine dei DOF nodali/matrici/rilasci: `DX,DY,DZ,RX,RY,RZ`. Forze nodali: `Fx,Fy,Fz,Mx,My,Mz`; il wrapper `NodeResultForces` mappa internamente `Fx=V1`, `Fy=V2`, `Fz=N`, `Mx=M1`, `My=M2`, `Mz=T`. Conserva tipo di risultato, corpo, elemento/lato e aggregazione. Non sommare totali di reazione e loro dettagli.

Unità normalizzate:

| Quantità | Unità |
|---|---|
| Forza nodale/beam | N |
| Momento nodale/beam | N mm |
| Risultante membranale/taglio shell, forza lineare | N/mm |
| Momento shell/distribuito | N mm/mm |
| Tensione | N/mm² |
| Lunghezza, area, inerzia geometrica | mm, mm², mm⁴ |
| Angolo | rad |
| Massa e inerzia di massa | kg, kg mm²; termini misti kg mm |
| Matrice di rigidezza | unità dell'azione di riga / spostamento di colonna |

`ResultUnits` usa fattori espliciti, anche distinti per colonne con unità miste. Numeratore e denominatore shell sono separati: 80 kNm beam = 80000000 Nmm; 80 kNm/m shell = 80000 Nmm/mm; 150 kN/m = 150 N/mm. La striscia larga 1000 mm ha rispettivamente 80000000 Nmm e 150000 N. Non esiste conversione automatica massa-peso.

`ResultBeamForces.ToCoordinateSystem` ruota vettori. `ToCoordinateSystemWithEccentricity` trasporta lo **stesso** sistema di azioni: `M_B = M_A + (r_A-r_B) × F`. A=(100,0,0) mm, B=0, F=(0,1000,0) N produce Mz=100000 Nmm. Questo non ricostruisce il diagramma lungo un elemento caricato. La preparazione beam ruota mantenendo il punto del campione e richiede conferma che le azioni siano già ridotte al baricentro della sezione.

Gli spostamenti sono vettori, senza traslazione dell'origine. `NodalKinematics.ToGlobalEquations` esprime le prescrizioni locali mediante le `MultiPointsCostrain` esistenti. `SpringMatrix.ToCoordinateSystem` applica `K' = Q K Qᵀ`, con Q blocco diagonale di rotazioni, allo stesso punto. Conserva tutti i 36 coefficienti. Per `SymmetricElastic` controlla simmetria e semidefinitezza; matrici negative/asimmetriche non sono corrette di nascosto. `GeneralLinear` consente asimmetria dichiarata. Gap, compression-only e leggi non lineari rimangono distinguibili, senza valutazione automatica.

Le shell ruotano membrane/momenti come tensori nel piano e tagli come vettori. `ResultPlateForces.ToCoordinateSystem` rimane una rotazione con stesso punto e normale. `ResultOrientation.Shell` gestisce esplicitamente anche la normale opposta, con momenti definiti come primo momento delle tensioni e tagli trasversali coerenti; `ShellLayers` trasforma profondità e direzioni delle barre mantenendo l'identità delle facce fisiche. Per coordinate naturali il cambio di connettività deve essere esplicito. `ResultOrientation.Beam` gestisce cambio della faccia positiva e, se richiesto, stazioni I/J, distanza e lato. Trasformazioni fuori piano incompatibili sono rifiutate.

`ModelOrientation.ReverseBeam` e `ReverseShell` restituiscono una copia del Model con connettività, stazioni, risultati, offset/tratti rigidi, carichi, rilasci, sezioni e armature coerenti. Le sagome beam asimmetriche mantengono i propri assi fisici in `SectionGeometryAxes`, separati dal verso del taglio: non si specchiano impropriamente le barre. I carichi conservano il proprio riferimento fisico. Assegnazioni opache o tipi non supportati interrompono l'operazione senza toccare il modello originale. Gli input modificati invalidano le analisi precedenti: le impronte dei dataset non vengono riscritte per attribuire automaticamente risultati alla nuova revisione.

`ResultTransformations.RotateBeam/RotateShell` restituiscono nuovi campioni mantenendo punto/stazione, caso, stato e provenienza; annotano la trasformazione e richiedono tutte le componenti disponibili. `TransportBeam` richiede una scelta esplicita del nuovo punto di riduzione. Il campione originale non viene modificato. Le preparazioni usano queste stesse funzioni.

## Assegnazioni

- `NodeAssignments`: disponibilità separata dei DOF, attivazione opzionale, vincoli per caso/fase, molle a terra, link e massa completa 6x6. Un DOF non esportato non è un vincolo.
- `RestrainAssignment`: riusa `NodeRestrain`. `DofRestrain.Prescribe` e `NodeRestrain.AddImposedDisplacement` distinguono anche la prescrizione zero e conservano una molla concomitante. `ClearPrescription` la elimina. Il caso è obbligatorio per valori imposti. `AssignRestrain(Add)` rifiuta sovrapposizioni nello stesso ambito; `ReplaceScope` sostituisce esplicitamente il caso/fase. I setter legacy `IsRestrained` conservano la vecchia semantica di reset; per supporti con prescrizione e molla usare le assegnazioni separate o `Prescribe`.
- `RigidLink`/`MultiPointsCostrain`: persistono equazioni, coefficienti e nodi. I coefficienti vengono riaperti senza rigenerare i bracci dalla geometria corrente. `NodalLink` può conservare una molla tra due nodi oppure coefficienti cinematici e legge non supportata, senza creare un beam fittizio.
- `BeamReleasesAttribute`: sei connessioni distinte I/J, continuità, rilascio, rigidezza assoluta o rapporto relativo 0..1, riferimento, fase e sorgente. Nessuna conversione implicita da partial fixity a rigidezza, nessuna modifica dei risultati o dei vincoli degli altri elementi incidenti.
- `BeamAssignments`: formulazione, membro logico, nodo di orientamento, assi completi, offset, tratti rigidi, baricentro fisico, intervalli di sezione, carichi e assegnazioni preservate. `SectionAt(xi,side)` supporta `Constant`, `Tabulated` (sole stazioni esplicite, anche ai due lati) e `LinearRectangular`. Quest'ultima interpola dimensioni e percorsi delle barre solo con materiali, ID/tipi di barra e topologia compatibili; non interpola arbitrariamente precompressione o parti composite. `BeamAnalysisProfile` rappresenta rigidezze e massa lineare costanti a tratti o lineari, con modificatori separati dalle proprietà resistenti. Tutte le quantità devono essere dichiarate; valori non forniti non diventano automaticamente zeri.
- `BeamLoadAssignment`: riusa `PointLoad`/`LineLoad`, con progressiva, intensità iniziale/finale, eccentricità, convenzione di lunghezza reale/proiettata e marcatore di equivalenza nodale. Non produce risultati FEM. Peso proprio, temperatura e altre leggi non interpretate possono essere conservate come `PreservedAssignment`.
- `ShellAssignments`: spessore fisico distinto dalla proprietà elastica, offset, riferimento degli strati e zona; `ShellRebarLayer`: faccia fisica, acciaio, diametro, passo, posizione nello spessore, direzione e ordine. Le facce non cambiano al cambiare del segno del momento. Validati diametri/passi/posizioni e ingombro nello spessore; nessuna propagazione automatica di zone sovrapposte o interpolazione dello spessore.

Le barre beam riutilizzano `ReinforcedConcreteSection` e le armature esistenti. La validazione controlla l'ingombro delle barre circolari nel contorno e nei fori, senza spostarle automaticamente; non certifica il copriferro normativo o altri requisiti costruttivi.

## Risultati, revisioni e API senza UI

`AnalysisDataset` registra programma/versione, modello/revisione, analisi, unità, hash sorgenti e impronta degli input. `ResultState` conserva dataset, fase, passo, posizione mobile, semantica cumulativa, modo/normalizzazione, stato concomitante, componenti presenti/mancanti/non esportate/non applicabili e copertura. Non sono sostituiti con zeri i componenti mancanti. Le nuove API rifiutano metadati insufficienti; i risultati legacy restano consultabili.

`ResultQueries.Samples<T>(element, selection)` restituisce riferimenti tipizzati ai risultati esistenti di node/beam/shell. `ResultSelection` seleziona esattamente dataset/caso/fase/passo/posizione/modo; fase o passo null significano assenti. `ConcomitantState` null consente più stati: la query puntuale rifiuta l'ambiguità. `BeamStation` e `Verification.BeamSample` cercano stazioni esatte, senza arrotondamenti o interpolazioni. La seconda API è comoda per un caso statico senza più passi; usare `ResultSelection` per casi articolati.

`StationResultBeamForces` e `StationResultDisplacement` conservano xi, distanza fisica opzionale, dominio e lato. xi deve essere finito in [0,1] in costruttore, setter e reader. `BeamReferenceGeometry` risolve `NodeToNode`, `OffsetToOffset` e `Deformable`, controlla la distanza rispetto alla lunghezza del dominio e converte le stazioni conservando la posizione del piano di taglio. Gli offset longitudinali non sono trattati come una semplice riscalatura della coordinata. `SectionCentroidOffset` è espresso lungo V1/V2 di `SectionAxes`; quando dichiarato consente il trasporto esplicito delle azioni al baricentro fisico. Assenza del dato e baricentro nullo sono condizioni diverse.

`PointResultPlateForces` conserva otto risultanti, posizione, tipo di punto e coordinate naturali/locali/globali, eventuale nodo sorgente e regione di media. Un risultato shell al nodo dell'elemento non è un `NodeResult`. La preparazione automatica non accetta punti di tipo sconosciuto o risultati mediati.

`LinearBeamCombination.AtStation` usa i coefficienti della `Combination` esistente: stesso elemento, dataset, punto, lato, fase e passo. Richiede stati lineari completi e `IsCombined=false`; rifiuta dati già combinati, estremi indipendenti, modali/spettrali, fasi incrementali o non definite, punti mancanti e convenzioni incompatibili. Restituisce un campione derivato e una diagnostica, senza modificare il modello. Il caso derivato e la sua registrazione sono una scelta esplicita del chiamante. Non usare gli operatori aritmetici legacy sui risultati per validare automaticamente una combinazione.

`ResultAlgebra.LinearCombination(model,key,combination,template)` estende il percorso comune a forze/spostamenti nodali, beam e plate. Il template identifica punto/stazione, corpo/proprietario e dataset/fase/passo; ogni termine deve fornire un unico campione compatibile. `Attach(derived)` registra il risultato soltanto se ancora corrente, con caso/combinazione già dichiarato e senza duplicati. Le dipendenze `DerivedFrom` sono riferimenti ai campioni originari, conservati dall'archivio: modificarli/rimuoverli rende non corrente il risultato derivato anche dopo riapertura.

`ResultAlgebra.Envelope` conserva un vettore concomitante completo per ciascun minimo/massimo, con caso e stato governanti; `MinimumSources`/`MaximumSources` forniscono i campioni originali utilizzabili dalla preparazione. `IsCurrent` controlla dati, dataset e copie esposte. Non crea un vettore ottenuto accostando massimi indipendenti. `AccumulateLinearHistory` somma solo incrementi lineari consecutivi, appartenenti a una storia esplicita, con indici da zero e stato iniziale nullo dichiarato. Le fasi nonlineari richiedono risultati cumulativi dal solver; forme modali, SRSS/CQC ed estremi spettrali restano consultabili ma non vengono spacciati per stati fisici concomitanti.

`AnalysisFingerprint` calcola su richiesta SHA-256 degli input autorevoli, rilevando anche mutazioni in-place a punti, carichi, proprietà e vincoli. `VerificationFingerprint(settings)` aggiunge armature e impostazioni. Le armature beam non alterano l'impronta FEM, mentre sezione geometrica e materiale sì. `SolverBindingFingerprint` è il sottoinsieme che lega i risultati di un solutore agli elementi; esclude proprietà, materiali, spessori fisici e combinazioni (vedi «Risultati letti a modello creato e filtri»). Non viene presunta una modellazione FEM non lineare sensibile alle singole barre: tale scenario richiede estendere le dipendenze della revisione.

Le impronte sono indipendenti dalla numerazione dei riferimenti XML e dalla materializzazione lazy di `Section.Shape`; sopravvivono alla riapertura. Non sono firme di autenticità né confronti automatici fra modelli di due solutori. Si computano sull'intero grafo di input: scelta conservativa per i setter legacy, non un benchmark di prestazioni su modelli grandi. Nessuna cache nascosta; le query per elemento non copiano i valori dei dataset. L'uso concorrente con mutazioni esterne non è supportato.

Esempio essenziale:

```csharp
var beam = model.BeamElements[250];
var sample = ResultQueries.BeamStation(beam,
    new ResultSelection { Dataset = "synthetic-static", Case = "P+" },
    .5, SectionSide.Unspecified);
var prepared = Verification.PrepareBeam(model, beam.Id, sample, settings);
var check = Verification.Run(prepared, CheckMechanism.UlsBiaxialSection, verifier);
model.CheckReports.Add(CheckReport.ForSingleResult(check));
ModelArchive.Save(model, destinationStream);
```

## Verificatore e report

`PrepareBeam` richiede topologia/assegnazioni valide, dataset normalizzato e aggiornato, sei componenti concomitanti, caso, faccia sezionale, sezione armata valida per la stazione, riferimento coerente con I→J e riduzione al baricentro confermata. Mantiene le sei azioni totali, senza larghezza di striscia. Componenti mancanti, estremi indipendenti, SRSS/CQC e dati modali non diventano input completi.

`IConcreteSectionVerifier` espone versione e capacità; `IConfiguredSectionVerifier` aggiunge la configurazione completa. L'adattatore effettivo `ConcreteSectionVerifier` chiama `SectionCheckerModelCode2010.CalculateFailureDomainPoint` e `CalculateWorkingRatio`. Accetta esplicitamente norma, criterio, trattamento del calcestruzzo teso, discretizzazione angolare e psi. Il report conserva i parametri della norma realmente passata e le opzioni numeriche. Il percorso è serializzato perché il checker usa stato condiviso; la cancellazione è verificata prima/dopo le chiamate native, che non offrono interruzione interna.

`ModelPreparation.Prepare(model, request, token)` seleziona beam/plate tramite `ElementSelection` (unione di gruppi e ID con famiglia), conserva ogni stazione/punto e usa `ResultSelection` per lo stato esatto. Un selettore senza gruppi/ID include tutte le famiglie richieste; una selezione vuota o non valida è un errore, mai un successo vuoto. Richieste sovrapposte non duplicano lo stesso oggetto risultato. Un caso senza campioni produce un elemento di lavoro mancante; la cancellazione mantiene il numero di controlli richiesti. Le azioni incrementali o di fase senza conferma cumulativa non sono verificabili direttamente.

`ShellInputPreparation` prepara il payload fisico (otto risultanti negli assi delle armature, proprietà, spessore e strati), distinto da `ShellActionPreparation`, che richiede anche un metodo strutturale esplicito. `ShellCheckInput.IsCurrent` rileva modifiche al modello, al campione o alle forze preparate. Non genera automaticamente sezioni resistenti da uno spessore e non inventa una distribuzione delle barre.

Il progetto separato `GPCModelChecker` espone `ModelChecker.Verify`. `ModelCheckRequest.Jobs` consente uno o più lavori nominati con selezione, risultati, meccanismi e `ConcreteVerificationOptions`. Il Checker nativo viene creato al primo uso e riutilizzato per sezioni/configurazioni con la stessa impronta; configurazioni diverse producono istanze distinte. `CreatedCheckers` rende osservabile questo comportamento. I report restituiti non vengono aggiunti automaticamente al modello: il chiamante può aggiungere `report.Jobs` a `model.CheckReports`.

Ogni esito riporta famiglia, ID, provenienza, lavoro, dataset/caso/fase/step/modo/posizione mobile e stazione/lato o punto shell. I controlli non eseguiti restano nel denominatore dei richiesti. `CheckReport.CurrentOutcome` confronta anche la selezione attuale del gruppo e i campioni correnti. `ModelCheckReport.CurrentOutcome(model, verifierForJob)` applica lo stesso controllo alle diverse configurazioni dei lavori. I numeri di utilizzo sono storici fino a questa convalida.

**Limite plate del motore reale:** `Checker/GPCChecker.Concrete/Checkers/PlateCheckerModelCode2010.cs` contiene una classe commentata; `PlateChecker` è astratta. `ModelChecker` restituisce `NotSupported / ShellCheckerUnavailable` per gli input shell validi. Il progetto non sostituisce questa mancanza con una verifica beam di striscia priva di metodo strutturale validato.

Esempio (namespace `GPC.Model.PostProcessing` e `GPC.Model.Checker`):

```csharp
var request = new ModelCheckRequest();
request.Jobs.Add(new ModelCheckJob {
    Name = "Spalla ULS",
    Preparation = new PreparationRequest {
        Selection = new ElementSelection { Groups = new[] { "Spalla" } },
        Results = new[] { new ResultSelection { Dataset = "analysis-01", Case = "ULS-01" } },
        Mechanisms = new[] { CheckMechanism.UlsBiaxialSection },
        Settings = "Configurazione di progetto"
    },
    Options = new ConcreteVerificationOptions {
        Standard = standardDelProgetto,
        Criterion = criterioDelProgetto,
        AngularDivisions = 64
    }
});
var report = new GPC.Model.Checker.ModelChecker().Verify(model, request, cancellationToken);
model.CheckReports.AddRange(report.Jobs);
```

La sola capacità dichiarata è **SLU sezionale N–M1–M2, pressoflessione deviata**. Taglio, torsione, SLE, stabilità, secondo ordine e verifica globale della spalla non sono certificati da questa chiamata. Il valore esemplificativo ottenuto con la DLL 0.0.13.0 è `0.014726828546570944`; è uno smoke test software, non un benchmark normativo indipendente della resistenza.

`Verification.Run` distingue esecuzione, stato dati ed esito ingegneristico; motore assente → `MissingDependency/NotEvaluated`, meccanismo assente → `NotSupported/NotEvaluated`. Controlla modifiche a modello/campione/azioni preparate prima e dopo la chiamata. Errori o indici non validi non producono esiti favorevoli.

`CheckRunner.Beam` esegue tutte le stazioni/stati importati del caso, in ordine deterministico, con avanzamento e cancellazione, quindi calcola il governante per meccanismo. `CheckReport` registra richieste/eseguite/escluse e singoli motivi: obblighi saltati impediscono un esito globale soddisfatto. `Outcome` è storico; **usare `CurrentOutcome(model, verifier)` per presentarlo come attuale**. La conservazione in `Model.CheckReports` è esplicita.

Per shell, `ShellActionPreparation.Prepare` valida otto componenti, spessore fisico, strati, stato e punto, quindi ruota e invoca il solo metodo `IShellDesignActionMethod` fornito dal chiamante. **Nessun Baumann, Wood-Armer o modello a strati è selezionato/implementato in questa modifica.** Senza metodo si ottiene `MissingShellDesignMethod/NotSupported`, con azioni originali conservate. `ShellPreparation.StripDirection1` moltiplica una terna direzionale già determinata dal metodo per la larghezza esplicita; non trasforma Fxy/Mxy in azioni di progetto, non è una verifica e non attesta completezza nelle altre direzioni. La sua mappatura è N=Fxx*b, V2=Fxz*b, M1=Mxx*b nella terna sezionale fornita dal chiamante.

## Persistenza e migrazione

`ModelArchive.Save/Load` usa XML `GpcModelArchive version="1"`, schema Model 1, `DataContractSerializer` con preservazione dei riferimenti. I tipi ammessi derivano dagli assembly/namespace compilati fissati nel codice (i solver FEM sono esclusi), non da nomi CLR scelti nel file. DTD e risoluzione di entità esterne sono disabilitati. Limiti: 256 Mi caratteri XML e 2 milioni di oggetti. Versioni future sono rifiutate.

`Save` costruisce prima l'archivio in memoria, così un errore di serializzazione non scrive metà grafo nello stream. Il chiamante gestisce stream, file temporaneo e sostituzione atomica su disco. Non è una conversione del JSON di Anthea. Per dati grandi occorre valutare un archivio separato dei risultati: qui il grafo resta nello stesso documento.

Persistiti e testati: nodi condivisi e coincidenti distinti, beam/shell/solid, coordinate/rotazioni/offset, proprietà, barre, rilasci, vincoli e MPC, molle accoppiate, carichi nodali/lineari, massa/inerzie, gruppi, casi, fasi, metadati dataset, risultati/stazioni/lati e report di verifica. Le fixture non esauriscono ogni sottotipo legacy del repository.

Correzioni e migrazione:

| Area | Comportamento / azione necessaria |
|---|---|
| `SortedCollection.Clear` | Corretta ricorsione. Writer v1 salva elementi e LastId; reader ripristina i riferimenti dopo deserializzazione. |
| Vecchie raccolte con solo LastId | Gli elementi non erano nel file e sono irrecuperabili da quel file. `MissingLegacyPayload` persiste il problema; `ValidateTopology` segnala `MissingLegacyCollectionPayload`. Reimportare dalla sorgente. |
| Writer `Model` | Corrette chiavi duplicate/incoerenti per Beams, BeamProperties, Groups. Il writer precedente poteva fallire: nessun reader può ricostruire un file mai prodotto. |
| `Element` | Ora salva attributi, carichi, risultati e provenienza. Campi legacy assenti inizializzati vuoti, senza inventare dati; una verifica richiede i metadati completi. |
| `NodeElement` | Reader accetta sia NodeElementVersion sia il vecchio BeamVersion. Uguaglianza/hash coerenti con l'identità. |
| Beam geometrici legacy | Restano leggibili con Point3d; connettività irrisolta. Collegarli con una mappa esplicita dalla sorgente: nessun nearest-node/merge automatico. |
| Rotazione beam assente | Scalar legacy 0; non significa orientamento noto. SectionAxes assente impedisce la verifica. |
| Stazioni displacement | Reader accetta DistanceFromStartPoint e ParametricDistance; writer uniforme. Xi non finito o fuori intervallo rifiutato. |
| `PointResultPlateStress` | Nuovo overload corretto ResultPlateStress. ResultStress legacy conservato tramite LegacyUnlocatedStress; non inventate facce superiore/inferiore. Getter erroneo ResultBeamForces restituisce null quando il contenuto non è beam. |
| Uguaglianza risultati | Include caso e stato analitico; campioni con stato/lato diverso non sono uguali. |
| `DofRestrain.Equals(object)` | Corretta ricorsione. AddStiffness non diventa più uno spostamento; correzioni agli aggregati per DOF. |
| `Group`, `Stage`, carichi e vincoli | Completata serializzazione dei dati usati nel percorso comune. |
| Identità elementi | Cambiamento intenzionale rispetto all'uguaglianza geometrica precedente: chiamanti che deduplicavano tramite Equals devono usare un confronto geometrico esplicito solo per segnalazione, senza fusione automatica. |

`BinaryFormatter` è usato **solo nei test legacy su oggetti creati dal test**, come nel progetto preesistente. Non è un reader di file esterni esposto dal nuovo archivio. Non è disponibile un convertitore sicuro generalizzato per vecchi file binari di provenienza ignota.

## Matrice dei convertitori

Legenda: **S** implementato e testato con fixture sintetiche; **I** interpretato ma non collaudato su export reale; **R** collaudato sui campioni nativi descritti sotto, senza attestazione generale; **P** preservato senza interpretazione; **N** non supportato. Il collaudo nativo attuale riguarda Straus7 R31/API 3.1.5; gli altri solutori usano fixture sintetiche.

| Adattatore | Geometria node/beam/shell | Proprietà | Vincoli/rilasci/carichi | Risultati node | Stazioni beam | Risultanti shell | Casi/fasi | Unità/assi |
|---|---|---|---|---|---|---|---|---|
| `ModelMapper` DTO normalizzati | S, gruppi con identità sorgente | Mapping delle proprietà passate, senza parser | S carichi nodali/vincoli permanenti espliciti | N | N | N | S casi nominati, metadati batch | CoordinateSystem e N/mm già normalizzati dal reader |
| `ResultMapper` DTO risultanti | Usa il modello esistente | Usa assegnazioni esistenti | N | S reazioni/azioni, spostamenti e rotazioni; proprietario delle azioni di elemento | S sei componenti | S otto componenti | S stato esplicito | S fattori e CoordinateSystem espliciti |
| `SapEditingTables` legacy | S; I rispetto al formato reale | N | N | N | N | N | N | Fattore lunghezza/cultura espliciti; base cartesiana globale confermata; orientamento beam non inferito |
| SAP2000 file/API generico | P bytes se manca reader | P | P | P | P | P | P | N |
| MIDAS Civil NX API | I schema UNIT/NODE/ELEM; test sintetici | P riferimenti MATL/SECT nel JSON | N mapping | N mapping | P tabella HEAD/DATA | P tabella HEAD/DATA | P colonne originali | I lunghezze; assi non inferiti |
| MIDAS Civil MCT/MGT | S node/beam/plate, gruppi; nessun export reale | P riferimenti e blocchi originali | S CONLOAD e CONSTRAINT globali semplici; P altre assegnazioni | N nel file modello | N nel file modello | N nel file modello | S nomi STLDCASE; P fasi/categorie | S N/mm e beam beta; P plate/REF/assi nodali |
| MIDAS Gen NX | P bytes se manca reader | P | P | P | P | P | P | N |
| MIDAS Gen | P bytes se manca reader | P | P | P | P | P | P | N |
| Straus7 API R3 | R node, beam strutturale, shell 3/4 nodi, gruppi gerarchici | P dati interrogati | N mapping | R reazioni, spostamenti/rotazioni, forze nodali dei singoli beam/plate | R statico lineare, stazioni esplicite | R centroide, statico lineare | R casi primari; N mapping fasi | R unità e assi nativi; S permutazioni complete |

**P per un file significa conservazione dell'intero file, non estrazione delle singole famiglie.** `SolverFileAdapter` accetta un `IModelFileReader` specifico; in sua assenza restituisce `Rejected`, `MissingValidatedReader`, hash e bytes Base64 nel report. Se un reader rifiuta il contenuto o il mapping fallisce, conserva comunque i byte originali acquisiti integralmente. Non vengono dedotti schemi da una semplice estensione. Gli ID disponibili da soli non autorizzano il matching fra modello e risultati.

Il layout SAP ispezionato ha righe da 9 campi per nodi (ID=0, X=3, Y=4, Z=6), 5 per frame (ID=0, I=1, J=2), 6 per aree (ID=0, nodi=1..4, quarto vuoto per triangoli, GUID=5). Il chiamante deve fornire cultura numerica e conferma della base globale; formato incompleto/non finito è rifiutato. I campi originali sono conservati in una codifica documentata di stringhe con lunghezza, numero di campi e flag null, poi Base64: **non è un formato file SAP**. L'hash identifica esattamente tali array ricevuti. `ModelMapper` conserva questi dati in `Model.PreservedSourceData`.

Separazione lettura → DTO → mapping → validazione. L'esito della geometria è `Partial` e `VerificationEnabled=false`: nessun dataset di risultati è stato importato. `SourceEvidence.SameAnalysis` richiede metadati uguali di programma/versione/modello-revisione/analisi; l'hash è provenienza, non uguaglianza richiesta fra file diversi. Non è stato implementato un abbinamento automatico di export privi di identificazione affidabile.

### Importazione dei risultanti nel modello comune

`ResultMapper.Import(model, batch, token)` valida e prepara l'intero dataset prima di aggiungere campioni e registro al modello. Il chiamante deve avere uso esclusivo del modello durante l'importazione. Controlla `AnalysisSource` (programma/versione/revisione/analisi), l'impronta attesa del modello, l'esistenza dei casi/combinazioni e gli ID sorgente distinti per famiglia. Il reader o il chiamante devono attestare il legame fra export e modello: non basta assegnare l'impronta del modello corrente a risultati di origine incerta.

#### Risultati letti a modello creato e filtri

L'import da API (Civil NX, Straus7) salva con `SourceBinding` due impronte: `AnalysisFingerprint` e `SolverBindingFingerprint`. Quest'ultima esclude proprietà beam/plate, materiali, spessori fisici e combinazioni. I risultati si leggono anche dopo salvataggio e riapertura, o dopo aver completato il modello per le verifiche. Sono rifiutati se cambiano la sorgente (hash), la geometria, la topologia, gli assi, gli offset, i vincoli, i carichi o i casi. Proprietà, materiali, spessori o combinazioni modificati danno solo l'avviso `ModelPropertiesChangedSinceImport`: i risultati restano quelli del modello analizzato. I legami salvati da import precedenti contengono solo `AnalysisFingerprint` e restano esatti. Il dataset si registra con l'impronta corrente; ogni lettura successiva usa un proprio `DatasetId`.

`ResultFilter.Resolve(model)` produce un `ResultReadPlan`; il lettore richiede al solutore soltanto elementi e casi del piano (Civil NX: `CivilNxResults.ReadAsync/ReadPlateNodesAsync(client, model, plan)`; Straus7: `Straus7ApiConverter.ReadResults(model, plan, modelPath, resultPath)`). Il filtro può selezionare:
- gruppi (con sottogruppi), elementi espliciti e famiglie, tramite `ElementSelection`: Beam per le forze beam, Shell per le forze plate, Node per spostamenti e reazioni dei nodi vincolati;
- nomi di proprietà (sezioni, spessori); i nodi dei beam e plate selezionati sono inclusi;
- casi statici;
- combinazioni (definizioni sorgente per id, per nome o per nome nel modello, oppure `Combination` del modello). Se ne leggono soltanto i casi statici, ricavati dai termini senza espandere gli inviluppi. ABS e SRSS sono rifiutate.

#### Ricostruzione delle combinazioni

`CombinationResults.Rebuild(model, plan, datasetId)` ricostruisce le combinazioni del piano dai risultati statici del dataset, ai beam, plate e nodi del piano. Il calcolo è in `CombinationRebuild.Rebuild` (Model).
- **Calcolo**: in ogni punto di risultato i campioni statici sono ruotati in un unico riferimento; ogni alternativa è la somma dei vettori statici moltiplicati per i coefficienti, come in `ResultAlgebra.LinearCombination`.
- **Combinazione lineare**: uno stato per punto, con `ConcomitantStateId` `linear:<nome>`.
- **Inviluppo**: per ogni componente si prendono le alternative di minimo e di massimo. Ogni alternativa governante si registra una volta sola, come stato concomitante completo (`ConcomitantEnvelopeState`, `envelope:<nome>:<scelte>`); `Coverage` elenca le componenti che governa. Non si compone mai un vettore di estremi indipendenti.
- **Tracciabilità**: gli stati derivano dai campioni statici (`DerivedFrom`), restano correnti dopo la riapertura e passano alla preparazione delle verifiche.
- **Elementi saltati**: un elemento con risultati statici incompleti, ad esempio le travi con offset di estremità in Civil NX, viene saltato con un avviso. Una seconda ricostruzione della stessa combinazione segnala i duplicati e non registra nulla.

I risultati appartengono a una `Combination` del modello dichiarata prima dell'import dei risultati statici, perché le combinazioni fanno parte di `AnalysisFingerprint`. All'import `ModelMapper` dichiara le combinazioni lineari con i coefficienti e gli inviluppi senza coefficienti. Questi ultimi danno il nome agli stati ricostruiti, mentre la definizione resta tra i dati sorgente. Per i modelli importati prima di questa versione, `CombinationResults.Declare(model, nomi)` va chiamato prima di leggere i risultati statici; se ci sono già dataset, avvisa che vanno riletti.

Misura indicativa su 10.000 plate e 10 casi: 1,2 s per una combinazione lineare; 6,2 s per un inviluppo di 25 alternative, con 117.000 stati governanti registrati.

Senza casi né combinazioni il piano comprende tutti i casi statici. Straus7 chiama un caso di risultato "‹numero del caso di carico›: ‹nome›" e numera i casi di risultato solo su quelli risolti (verificato su R31). Per questo `Straus7LinearStaticResults.CaseMap` associa i casi tramite quel numero e non tramite il numero del caso di risultato.

I DTO `BeamForceRecord` e `ShellForceRecord` dichiarano rispettivamente sei/otto componenti nell'ordine GPC, assi e unità sorgente. Il reader specifico deve già aver risolto permutazioni/segni e corpo delle azioni. Il mapper conserva `OriginalResultData`, record/hash/versione reader/convenzione, normalizza in N/mm/rad e non modifica i campioni di altri dataset. Mancanze rappresentate con `double?` e flag restano `NaN` nei contenitori numerici legacy, mai zero, e bloccano la verifica. Duplicati nello stesso punto/stato e reimport dello stesso dataset sono rifiutati. Un esito `Completed` descrive l'importazione; l'idoneità alla verifica richiede comunque la preparazione.

### Risultati nodali e preparazione comune degli assi

Il batch comune comprende `NodeForces` (`NodeForceRecord`) e `NodeDisplacements` (`NodeDisplacementRecord`) insieme a beam e shell. Le componenti nodali sono rispettivamente **Fx,Fy,Fz,Mx,My,Mz** e **Dx,Dy,Dz,Rx,Ry,Rz** negli assi dichiarati; gli indici non seguono la disposizione interna di `ResultBeamForces`. Quest'ultimo rimane il contenitore compatibile dei sei numeri di `NodeResultForces`, con accessi fisici tipizzati. I flag `State.Components` e `Original.ComponentOrder` seguono l'ordine nodale. `Original.AngleToRad` conserva anche il fattore angolare sorgente. Il punto di riduzione deve coincidere con il nodo, espresso in mm dopo la conversione.

Ogni azione distingue `Kind` e `Body`. Reazioni di appoggio, forze di molle/link e forze nodali degli elementi non diventano sollecitazioni di sezione. Per un contributo beam servono `OwnerSource` e `ElementEnd=I/J`; per una plate serve il proprietario e il nodo deve appartenere alla sua connettività. Nel Model il proprietario ha **famiglia + ID**, quindi beam e plate con lo stesso ID restano distinti. Molle/link richiedono `AggregationSet` come identificatore esplicito del contributo o insieme sorgente. `NodalActions.Validate` impedisce identità e corpi incoerenti. I vecchi archivi senza famiglia del proprietario restano leggibili ma quella provenienza incompleta non diventa automaticamente verificata.

`NodeRecord.CoordinateSystem` permette ai reader di dichiarare l'orientamento nodale. Due record dello stesso nodo con orientamenti diversi sono un conflitto. Un nodo non eredita gli assi da una trave incidente; le basi dei vincoli restano nelle relative assegnazioni. `Fx/Fy/Fz` indicano le componenti nella base del risultato, non necessariamente nel globale.

Le trasformazioni esplicite sono:

- `RotateNode(NodeResultForces, axes)` e `RotateNode(NodeResultDisplacement, axes)`: rotazione alla stessa origine; nessuna modifica del risultato sorgente.
- `TransportNode(...)`: trasporto esplicito del momento `M' = M + (O - O') × F`; il risultato trasportato non si reimporta come reazione nel nodo se l'origine è diversa. Non viene applicata questa operazione agli spostamenti.
- `RotateBeam` e `RotateShell`: rotazioni già disponibili. Per una plate con posizione `LocalPhysical`, ruotano anche le due coordinate del punto, mantenendo la posizione fisica. Le coordinate naturali della mesh non cambiano. Il cambio di normale resta un'operazione fisica distinta, non supportata da una semplice rotazione.

`ResultPreparation.Prepare` verifica appartenenza del campione, dataset, impronta del modello, completezza, concomitanza e assi; restituisce una copia nella base locale del nodo, della sezione beam o della plate. Mantiene punto di riduzione, corpo e proprietario. Modali non scalati, estremi indipendenti, componenti mancanti o stati incrementali non diventano campioni fisici completi. `IsCurrent` si invalida dopo modifica/rimozione della sorgente, della copia preparata, dei metadati del dataset o degli input FEM. Il batch calcola l'impronta globale all'inizio e alla fine, senza ripeterla per ogni campione durante la preparazione.

```csharp
var state = new ResultSelection { Dataset = datasetId, Case = modelCaseName };
var scope = new ElementSelection
{
    Families = new[] { EntityFamily.Node, EntityFamily.Beam, EntityFamily.Shell },
    Groups = new[] { groupName }, IncludeDescendants = true
};
var actions = ResultPreparation.Prepare(model, scope, new[] { state }, cancellationToken);
// Ogni elemento/stato senza risultati produce un record diagnostico, non sparisce dalla selezione.
foreach (var item in actions)
{
    if (!item.IsCurrent) { /* mostrare item.Diagnostics; nessuna verifica */ continue; }
    var localSample = item.LocalSample; // NodeResultForces/Displacement, StationResultBeamForces, PointResultPlateForces.
}
var reaction = ResultQueries.NodeForce(node, state, NodalForceKind.SupportReaction, ActionBody.OnNode);
```

`DataStatus.Ready` in questo servizio significa **dati di azione/cinematica pronti**, non resistenza verificata. `ModelPreparation` mantiene il controllo successivo su sezioni, armature, centri/offset e meccanismi prima di `ModelChecker`. Accetta solo beam/shell; i nodi non ricevono un esito generico sul calcestruzzo. Nessuna interpolazione di risultati mancanti, somma automatica di contributi o produzione di risultati tramite un nuovo solver.

### Civil NX API e versioni

Il trasporto `CivilNxApiClient` usa la Base URL configurata dal chiamante e una funzione che fornisce la MAPI-Key a runtime. Non cerca credenziali sul computer e non le archivia. Espone GET di database e POST alla sola tabella risultati; non espone analisi, modifica, salvataggio o apertura del modello. Il funzionamento e l'header seguono il [manuale MIDAS Open API](https://support.midasuser.com/hc/en-us/articles/30212837484441-How-to-work-MIDAS-CIVIL-NX-Open-API).

`ReadGeometrySnapshotAsync` legge due volte UNIT/NODE/ELEM e rifiuta cambiamenti osservati. Questo controllo non è una transazione del solutore: il modello deve restare fermo durante l'acquisizione. Il limite è 32 MiB per risposta e 60 secondi per richiesta, lettura corpo compresa; annullamento propagato. `CivilNxSnapshot` può essere creato anche da risposte JSON già acquisite per collaudi riproducibili offline.

`ICivilNxGeometryProfile` è il punto di estensione per gli schemi di versioni differenti. Il profilo comune non filtra il numero di release: interpreta campi documentati, accetta aggiunte conservandole nel JSON originale e rifiuta dati essenziali mancanti, tipi/formulazioni ignoti e connettività non supportata. Le capacità sono dichiarate separatamente dall'etichetta di versione. Non si dichiara supporto collaudato a tutte le release perché qui non sono state interrogate installazioni reali.

Il profilo geometrico attuale converte le unità di lunghezza e collega nodi/beam/plate; mantiene il JSON completo, inclusi riferimenti a materiali/sezioni/angoli. **Non interpreta ancora questi riferimenti come assegnazioni strutturali**, non ricostruisce assi locali e restituisce `Partial`. Schema tratto dai manuali [UNIT](https://support.midasuser.com/hc/en-us/articles/35802155483801-Unit-System), [NODE](https://support.midasuser.com/hc/en-us/articles/35806845654169-Node) ed [ELEM](https://support.midasuser.com/hc/en-us/articles/35806934300825-Element).

`CivilNxTableRequest` richiede elementi e casi espliciti, richiede unità N/mm ed evita l'averaging nodale shell. La selezione beam predefinita comprende solo I/J: le parti aggiuntive devono essere richieste esplicitamente e non costituiscono un massimo continuo. `CivilNxResultTableData` legge HEAD/DATA per nomi di colonna, preserva campi aggiuntivi e rifiuta accessi ambigui/mancanti. Lettura della tabella e normalizzazione ingegneristica sono stadi distinti: il raccordo automatico al `ResultMapper` attende il mapping validato di stazioni, facce, assi e stati. Riferimenti: [Beam Force](https://support.midasuser.com/hc/en-us/articles/36011262919705-Beam-Force-Analysis-Result-Table), [Plate Force per unità di lunghezza](https://support.midasuser.com/hc/en-us/articles/36012822385817-Plate-Force-Unit-Length-Analysis-Result-Table).

```csharp
using var api = new GPC.Converter.CivilNx.CivilNxApiClient(baseUrl, keyProvider);
var snapshot = await api.ReadGeometrySnapshotAsync(cancellationToken);
var imported = GPC.Converter.CivilNx.CivilNxGeometryReader.Import(snapshot,
    new AnalysisSource { Program = "MIDAS Civil NX", SolverVersion = observedVersion,
        ModelRevision = sourceRevision, AnalysisId = sourceAnalysis }, cancellationToken: cancellationToken);
// imported.Status è Partial: completare assegnazioni e risultato prima della verifica.
```

### MIDAS Civil: file MCT/MGT

`GPC.Converter.MidasCivil.MidasCivilTextReader` è il secondo lettore nell'ordine richiesto. Lo schema si basa sul [MCT File Quick Reference Civil](https://manual.midasuser.com/EN_Common/Civil/870/Start/14_Appendix/MCT_File_Quick_Reference.htm) e sul writer locale `../Rhino2Midas Backup/Rhino2Midas20251123/Rhino2Midas.Core/Helper/HelperMidasExport.cs`. Il [manuale Export Civil](https://manual.midasuser.com/EN_Common/Civil/870/Start/01_File/Export.htm) denomina questi file MCT; il writer locale usa anche MGT. Questo lettore interpreta lo schema Civil dichiarato dal chiamante, non identifica automaticamente il prodotto e non costituisce il futuro lettore MIDAS GEN.

```csharp
using var source = File.OpenRead(path);
var reader = new GPC.Converter.MidasCivil.MidasCivilTextReader(modelRevision: sourceRevision);
var imported = reader.Import(source, cancellationToken);
if (imported.Model != null)
{
    var members = imported.Model.GetGroupElements("Deck");
    // imported.Status == Partial; nessuna verifica è abilitata da questa importazione.
}
```

L'esempio completo si esegue dalla radice con `dotnet run --project Examples/MixedModel/MixedModel.csproj -- --civil-file Examples/CivilTextSynthetic.mct synthetic-revision`. Il file incluso è **sintetico**, senza risultati calcolati da un solutore. Il programma stampa diagnostica, conteggi, gruppi e disponibilità degli assi, poi controlla il round-trip dell'archivio in memoria.

Il reader separa tokenizzazione, DTO comuni e `ModelMapper`. I DTO aggiunti sono `GroupRecord`, `LoadCaseRecord`, `NodeLoadRecord` e `NodeRestrainRecord`; le estensioni sono additive. I membri dei gruppi si risolvono per `SourceIdentity`, mai usando l'ID sorgente come posizione nel registro interno. I carichi coincidenti restano carichi distinti. I vincoli permanenti ripetuti sullo stesso nodo accumulano i DOF fissati, mantenendo i riferimenti alle righe.

| Comando | Interpretazione attuale |
|---|---|
| `VERSION`, `UNIT` | Versione osservata, senza whitelist; unità esplicite obbligatorie prima di dati dimensionali, applicate nell'ordine del file. |
| `NODE` | ID interi positivi conservati come stringhe canoniche; coordinate GCS convertite in mm. Duplicato identico segnalato, duplicato discordante rifiutato. |
| `ELEMENT BEAM` | Travi dritte a due nodi, subtype/EXVAL zero; record con beta numerico oppure REF. Per REF si conserva il riferimento, senza assegnare `SectionAxes`. |
| `ELEMENT PLATE` | Triangoli (quarto nodo zero) e quadrilateri, subtype 1/2. Connettività condivisa, planarità e winding validati dal dominio. Materiale, spessore e suffisso di orientamento/larghezza conservati senza interpretazione. |
| `GROUP` | Gruppi strutturali anche vuoti e sovrapposti, nodi/elementi, liste e intervalli `10to40by10` oppure `10 to 40 by 10`. Campo grafico opzionale conservato. |
| `STLDCASE`, `USE-STLD` | Casi nominati e selezione del caso dei carichi successivi; categoria/descrizione conservate nel record. Riferimenti a casi non dichiarati rifiutati. |
| `CONLOAD` | Sei componenti globali Fx/Fy/Fz/Mx/My/Mz, normalizzate in N e Nmm; gruppo di carico conservato come dato sorgente, non trasformato in gruppo strutturale. |
| `CONSTRAINT` | Sei flag 0/1, vincoli permanenti globali; label del boundary group conservata. Se esistono `LOCALAXIS` o comandi `STAGE*`, tutti questi vincoli restano preservati con avviso, senza appiattire assi/fasi. |
| `MATERIAL`, `SECTION`, `THICKNESS`, rilasci, molle, carichi distribuiti, combinazioni/fasi e altri comandi | Blocchi conservati con avviso; nessuna legge meccanica, resistenza, spessore fisico o attivazione dedotta. Tipi di elemento diversi da BEAM/PLATE sono rifiutati conservando il file, senza ometterli da un candidato apparentemente completo. |

Le unità supportate sono N, KN, KGF, TONF, LBF, KIPS e MM, CM, M, IN, FT; i momenti derivano da forza × lunghezza. Le tre/sei componenti non vengono completate con zeri se mancanti. Cultura numerica invariante, esponenti `E`, nessun separatore delle migliaia. UTF-8 rigoroso con BOM opzionale; per ANSI il chiamante deve passare `textEncoding` esplicito. Decodifiche invalide sono rifiutate, senza sostituzione silenziosa dei caratteri.

Gli assi beam seguono il [manuale Create Elements](https://manual.midasuser.com/EN_Common/Civil/940/Start/04_Model/04_Elements/Create_Elements.htm): x MIDAS va da I a J, z a beta zero è la proiezione di Z globale sul piano trasversale; per una trave esattamente verticale z parte da X globale. y completa la terna destrorsa. La corrispondenza è **GPC (V1,V2,V3) = MIDAS (y,z,x)**; beta è in gradi e ruota attorno all'asse longitudinale con regola della mano destra. Geometrie quasi verticali con proiezione inferiore alla soglia numerica del lettore sono rifiutate e richiedono assi esportati esplicitamente. Questo mapping non attesta né gli offset né la posizione del centro sezionale; `ActionsAtSectionCentroidConfirmed` resta falso. Gli assi plate e nodali non vengono dedotti da quelli beam.

Il parser gestisce commenti `;`, nomi CSV tra virgolette, virgolette raddoppiate e continuazioni finali `\`. Richiede `ENDDATA`, rifiuta dati successivi, campi dimensionali non finiti, colonne incomplete, gruppi/casi ambigui e connettività pendenti. Limiti: 128 MiB di sorgente nell'adattatore e un milione di riferimenti espansi complessivi. Hash sui byte originali; blocchi testuali conservati con righe normalizzate LF e riga sorgente iniziale. `ModelFileReadException.Record` alimenta la diagnostica strutturata; ogni rifiuto lascia `Model=null`.

**Il file modello non fornisce le sollecitazioni di analisi.** L'importazione resta `Partial` e `VerificationEnabled=false`. Materiali/sezioni, spessore fisico, assi plate, REF/assi nodali, rilasci/molle, fasi e risultati richiedono ulteriori mapping e confronto con export effettivi. Una versione nuova con lo stesso schema è leggibile, ma i 49 test di `MidasCivilTextWorkflowTest` e l'esempio verificano solo fixture sintetiche: nessuna release è dichiarata collaudata su file reale. I formati diversi possono implementare `IModelFileReader` mantenendo lo stesso mapper comune.

Riferimenti ufficiali consultati, da verificare contro la versione e il campione effettivo prima di scrivere altri mapping:

- [CSI: Frame Element Internal Forces Output Conventions](https://docs.csiamerica.com/help-files/sap/Output/Frame_Element_Internal_Forces_Output_Conventions.htm): asse longitudinale SAP, stazioni e facce in presenza di offset. Non adottare questi indici come quelli GPC.
- [Strand7: Beam Elements Local and Principal Axes](https://www.strand7.com/straus7r3help/Content/Topics/Elements/ElementsBeamsPrincipalAxes.htm): asse longitudinale 3; momento nel piano e momento attorno all'asse non sono sinonimi.
- Il collegamento MIDAS Beam End Release fornito nella richiesta non era accessibile durante l'audit; nessuna regola di conversione di partial fixity è stata dedotta da quel collegamento.

### Straus7 API R3

`GPC.Converter.Straus7` usa il runtime nativo installato, senza riferimenti a Rhino e senza distribuire DLL del produttore. `Straus7NativeApi` implementa le chiamate del manuale **Straus7 R3 API**, verificato contro `C:\Program Files\Straus7 R31\API Includes\Visual C#\St7API.cs` e `Documentation\Straus7 R3 API Manual.pdf`. Il binding è Windows x64, ABI R3; major diverse sono rifiutate. Le funzioni richieste vengono risolte per nome, una funzione mancante produce diagnostica. `IStraus7ReadApi` consente binding alternativi senza cambiare il Model. La versione registrata come `AnalysisSource.SolverVersion` è, per questo reader, la **versione API** restituita da `St7Version`, non una build del solver dedotta.

La DLL si specifica nel costruttore o tramite `STRAUS7_API_PATH` (directory o percorso assoluto); in assenza vengono cercate le installazioni R31/R3 in Program Files. Occorre una licenza API valida. `St7SetLicenceOptions(lmAbort)` evita finestre interattive. Il converter serializza le proprie sessioni e rifiuta una DLL già caricata: se un altro plugin usa St7API serve un processo separato. Chiude file risultati/modello e rilascia runtime/licenza anche in caso di errore. La cancellazione viene osservata fra le chiamate native; una chiamata già in corso non viene interrotta forzatamente.

```csharp
var converter = new GPC.Converter.Straus7.Straus7ApiConverter();
var geometry = converter.Import(new GPC.Converter.Straus7.Straus7ImportRequest
{
    ModelPath = st7Path, ModelRevision = revision, AnalysisId = analysisId
}, cancellationToken);
if (geometry.Model == null) return; // Esporre geometry.Diagnostics nell'interfaccia.

// Con selezioni vuote ReadResults restituisce solo l'elenco dei casi primari.
var native = converter.ReadResults(new GPC.Converter.Straus7.Straus7ResultsRequest
{
    ModelPath = st7Path, ResultPath = resultPath,
    CaseNumbers = new[] { 1 }, BeamNumbers = new[] { 1 }, PlateNumbers = new[] { 1 },
    NodeNumbers = new[] { 1 }, MinimumBeamStations = 11,
    IncludeElementNodeForces = true // Richiede l'output solver Element Node Force nel file risultati.
}, cancellationToken);

// Esempio: il chiamante ha verificato che il caso risultato 1 corrisponde al caso G.
// Gli indici dei casi risultati non vengono equiparati automaticamente agli indici dei load case.
var caseMap = new Dictionary<int, string>
{
    [1] = GPC.Converter.Straus7.Straus7ApiConverter.CaseName(1, "G")
};
var results = GPC.Converter.Straus7.Straus7LinearStaticResults.Import(
    geometry.Model, native, datasetId, caseMap, cancellationToken: cancellationToken);
```

La geometria usa numeri API per la connettività e `SourceIdentity` distinta per famiglia. Gli ID utente di nodi/elementi sono metadati separati, possono ripetersi e non vengono usati per unire entità. I gruppi mantengono gerarchia e appartenenza degli elementi; il nome comune include l'ID nativo per distinguere etichette uguali. L'API gruppi non assegna nodi. Le coordinate diventano mm; gli assi iniziali beam/plate provengono direttamente dall'API e sono validati come terne destrorse. V3 beam deve seguire I→J. Il centroide geometrico plate viene letto dall'API, senza supporre che coincida con una coordinata UV costante.

Sono accettati beam strutturali `btBeam` a due nodi e shell `ptPlateShell` a tre/quattro nodi. Truss, plane stress/strain, brick, link, connettività superiori e assi discordanti sono rifiutati atomicamente. Materiali elastici/sezioni beam standard e spessori/materiale isotropo plate sono conservati come dati nativi interrogati, senza inventare resistenze, armature o spessore di progetto. **Carichi, vincoli, rilasci, molle, offset e fasi non sono ancora mappati.** Non si dichiara conservato ogni attributo del file: rimangono percorso e SHA256 dell'originale, più i record acquisiti; il binario ST7 non viene incorporato. Il chiamante deve conservare l'originale. La geometria restituisce `Partial`.

La lettura risultati verifica prima `St7ValidateResultFile` e richiede flag di validazione nulli; apre senza generare combinazioni, seleziona numeri espliciti di casi/entità e conserva i dati grezzi. File sorgente mantenuti in sola lettura, hash verificati prima/dopo. Il raccordo al Model richiede stesso hash geometrico, versione API, identità analisi esplicita e impronta degli input immutata rispetto all'import. Non basta riscrivere l'impronta corrente per attribuire risultati a un modello modificato. I casi devono essere abbinati esplicitamente a load case già esistenti.

Il profilo di normalizzazione attuale accetta **solo statico lineare, casi primari, senza fasi costruttive**:

- Beam: `St7GetBeamResultArray(rtBeamForce, stBeamGlobal)` restituisce `FX,MX,FY,MY,FZ,MZ`. Sono azioni equilibranti sulla faccia del pezzo lato End 2: si cambiano di segno forza e momento, poi si proiettano in V1/V2/V3 per ottenere `N,V1,V2,T,M1,M2` sulla faccia positiva. La distinzione fra momento nel piano e momento intorno all'asse è descritta nelle [convenzioni ufficiali beam](https://www.strand7.com/strand7r3help/Content/Topics/ResultsInterpretation/ResultsInterpretationBeamInternalForceConvention.htm).
- Stazioni: coordinate parametriche `bpParam` in [0,1], tutte le righe restituite dall'API, comprese eventuali stazioni aggiuntive. Righe duplicate alla stessa ascissa sono mantenute nel rapporto grezzo, ma rifiutate dalla normalizzazione finché non è noto il lato della discontinuità. Nessuna media/interpolazione e nessuna attestazione di copertura dei massimi. Offset non risolti: `ActionsAtSectionCentroidConfirmed` resta falso.
- Plate: coppia force/moment al centroide, assi locali iniziali, senza media nodale. L'ordine nativo `xx,yy,zz,xy,yz,xz` diventa `Fxx,Fyy,Fxy,Fxz,Fyz,Mxx,Myy,Mxy`. Le quantità sono per unità di larghezza; Mxx/Myy positivi corrispondono a trazione sulla faccia +z, secondo le [convenzioni ufficiali plate](https://www.strand7.com/strand7r3help/Content/Topics/ResultsInterpretation/ResultsInterpretationPlateShearForceAndMomentConventions.htm). Termini fuori piano non rappresentabili e non nulli sono rifiutati. Origine = centroide nativo, posizione locale fisica (0,0).
- Nodi: reazioni globali `Fx,Fy,Fz,Mx,My,Mz` applicate al nodo (`SupportReaction`, `OnNode`), traslazioni `Dx,Dy,Dz` e rotazioni `Rx,Ry,Rz`. Il converter richiede esplicitamente spostamenti assoluti e radianti tramite API; normalizza in N/mm/rad e colloca il punto di riduzione nel nodo. Non cambia il segno delle reazioni come invece avviene per la faccia di taglio beam.
- Azioni nodali dei singoli elementi, opzionali: `rtBeamNodeReact` alle estremità e `rtPlateNodeReact` ai nodi senza media. Entrambi gli array seguono `Fx,Fy,Fz,Mx,My,Mz`, diversamente da `rtBeamForce`. Il corpo è `OnElement`; proprietario e connettività ordinata vengono verificati, con I/J per i beam. Sono le azioni che equilibrano il singolo elemento, secondo le [definizioni ufficiali delle quantità](https://www.strand7.com/strand7r3help/Content/Topics/ResultsInterpretation/ResultsInterpretationResultQuantities.htm). Il file deve contenere l'output **Element Node Force** (`srElementNodeForce`); in assenza, la richiesta esplicita viene rifiutata. Il converter non ricalcola o modifica il file risultati.

Unità, casi, validazione e array originali restano conservati insieme al dataset. Non si sommano automaticamente reazioni, contributi di molle/link e azioni degli elementi: rappresentano corpi o sottoinsiemi diversi e potrebbero duplicare la stessa azione fisica. Gli assi nativi sono controllati **prima** che `CoordinateSystem` normalizzi i vettori del costruttore. La base locale del nodo, quando non dichiarata dalla sorgente, resta globale; una base di vincolo relativa a un caso non viene assunta come orientamento unico del nodo.

Le unità ammesse sono m/cm/mm/ft/in e N/kN/MN/kgf/lbf/tf/kip; risultanti convertite in N/mm, momenti in Nmm, shell per mm. Elementi inattivi o risultati mancanti vengono rifiutati. `birth stage=1` è il valore osservato in API 3.1.5 anche senza fasi; l'assenza delle fasi è controllata separatamente sul modello e sul caso risultato. L'importazione canonica è atomica. `Completed` indica dati importati, **non idoneità a Checker**: proprietà, armature, offset e altre condizioni richieste dalla preparazione devono essere completati e il legame con l'analisi rivalidato.

**Collaudo nativo del 30/09/2026, Straus7 R31/API 3.1.5:**

- File del produttore `C:\ProgramData\Straus7 R31\API Examples\Matlab\Plates.st7`: 5.175 nodi, 4.928 shell, un gruppo/caso, archivio con impronta invariata. SHA256 `E349F7B6039F3594D440C106ED411529E35997AF19E08EB54BE4F1C422D26A40`.
- `Straus7NativeIntegrationTest`: modello nuovo a sei nodi, beam a mensola lungo Z (L=2000 mm) e plate rettangolare separata. Calcolo reale con il solver installato. Beam: F=(100,200,300) N, M=(10,20,30) Nmm; confronto a ogni stazione con F costante e M=Mtip+(0,0,L-z)×F. Plate con ν=0: due forze da 500 N su bordo largo 1000 mm danno Fxx=1 N/mm; secondo caso con due momenti My da 500 Nmm dà Mxx=+1 Nmm/mm. Verifica anche lettura nodale e normalizzazione canonica.
- 51 test deterministici Straus7 coprono componenti distinte, unità, gerarchia, identità condivisa, archivio, cancellazione, errori API/chiusura, associazioni errate, proprietà/assi invalidi, fasi, azioni nodali degli elementi e rifiuti senza mutazioni. Questi test usano un sostituto API esclusivamente nel progetto test. Altri 35 test comuni (`NodalResultWorkflowTest`) controllano assi locali, gradi/radianti, proprietari, molle/link distinti, rotazione del punto plate, selezioni, dati incompleti e snapshot modificati.

Per ripetere il benchmark nativo, scegliere una directory **nuova**; lo script rifiuta di sovrascrivere una directory esistente:

```powershell
python UnitTest/Fixtures/Straus7/generate_native_fixture.py 'C:\Program Files\Straus7 R31\Bin64\St7api.dll' C:\temp\straus-benchmark-new
$env:GPC_STRAUS7_BENCHMARK = 'C:\temp\straus-benchmark-new'
dotnet test UnitTest/UnitTest.csproj --filter FullyQualifiedName~Straus7NativeIntegrationTest
```

Lo script di test crea e calcola soltanto quel nuovo modello, abilitando `srElementNodeForce`; il converter di produzione espone esclusivamente letture. Il benchmark controlla anche reazioni, rotazioni della mensola, azioni su I/J, forze membranali dei quattro nodi plate e preparazione locale. Senza la variabile il test nativo viene indicato come inconclusivo; gli altri 707 test non richiedono Straus7. Il benchmark copre queste configurazioni, non tutte le formulazioni, release, offset, torsioni plate o analisi dinamiche/non lineari. Il prossimo connettore nell'ordine richiesto è SAP2000.

Il percorso eseguibile completo usa un abbinamento caso esplicito:

```powershell
dotnet run --project Examples/MixedModel/MixedModel.csproj -- --straus-api-results C:\temp\straus-benchmark-new\benchmark.st7 C:\temp\straus-benchmark-new\benchmark.LSA rev-1 lsa-1 1 'Case 1: Benchmark' --element-node-forces
```

Nel benchmark questo comando importa 30 campioni (11 stazioni beam, un centroide plate, 12 campioni nodali e 6 azioni nodali degli elementi), li prepara negli assi locali e verifica la riapertura dell'archivio. L'opzione finale è facoltativa. Questa preparazione delle azioni precede e non sostituisce la preparazione strutturale per Checker.

## Collaudo e tracciabilità

Suite MSTest mantenuta; nessun test preesistente disabilitato. I due test esistenti modificati correggono rispettivamente l'attesa della precedente uguaglianza geometrica degli elementi e il test plate che istanziava erroneamente `ResultBrickForces`. Baseline 506 test; suite del nucleo 707 test deterministici più un test Straus7 nativo opzionale, più 8 test nel progetto `ModelChecker.Tests`. I 49 casi del lettore Civil coprono assi e segni, unità variabili/imperiali, cultura, gruppi/connettività, carichi cumulativi, vincoli, preservazione, errori atomici, encoding, cancellazione e round-trip. Straus7 comprende 51 test con API sostitutiva confinata ai test e un benchmark nativo. I risultati nodali e la preparazione comune aggiungono 35 test. Le altre fixture di import e azioni sono sintetiche. I test del Checker includono esecuzione reale, confronto riuso/istanza nuova, sezioni in cache mutate, più configurazioni, report persistiti, gruppi modificati, cancellazione e errori di creazione.

Classi di test nuove: `PostProcessingTest`, `MixedWorkflowTest`, `LegacyModelRegressionTest`, `AssignmentWorkflowTest`. Gli attesi sono identità/riferimenti espliciti, numeri analitici assegnati e trasformazioni note, senza usare un nuovo solver. Le tolleranze assolute sono nell'unità del campo (circa 1e-10 per forze/assi, 1e-7 per momenti della mensola, 1e-6 per 80 milioni Nmm). Non sono utilizzati solo confronti trasformazione-inversa.

| Criterio | Evidenza / limite |
|---|---|
| T01–T04 | ID sparsi e uguali tra famiglie, coincidenti distinti, nodo condiviso, modifica osservata, rimozione bloccata e riferimenti persistiti. |
| T05 | Prescrizione nodale a 45° come equazione globale; spostamenti e forze ruotati confrontati con componenti analitiche. |
| T06 | Matrice accoppiata, termini misti/unità, rotazione a 90°, risposta numerica; negative/asimmetriche conservate e diagnosticate. |
| T07 | Rilascio di un solo beam, altro beam e vincolo nodale invariati. |
| T08 | Beam verticale con sezione ruotata di 90°, N/T asse 3 e segni trasversali. |
| T09 | **Non implementato** il rovesciamento fisico I/J completo. Modifica topologica invalida i risultati; non equivale al test richiesto. |
| T10–T12 | Unità totali/per lunghezza e striscia1000; trasporto eccentrico Mz=100000; xi invalidi in tutti i percorsi. |
| T13 | Stessa xi, lati opposti distinti, query e round-trip. |
| T14 | Mensola analitica P=1000 N, L=2000 mm: V2=+1000, M1=-2000000 all'incastro, reazione Fy=-1000 e Mx=+2000000. |
| T15 | Diagramma assegnato di trave appoggiata q=2, L=4000: tagli ±4000 e massimo interno 4000000. Non è stato aggiunto un solutore o un controllo globale di equilibrio. |
| T16 | Due tagli +1000/-1000 alla stessa ascissa, senza interpolazione implicita. |
| T17 | Otto componenti distinte, attesi analitici a 90° e 30°; normale opposta trasformata con API esplicita, punti e facce/armature coerenti. Una normale sorgente incoerente non viene corretta silenziosamente dalla preparazione. |
| T18 | Estremi indipendenti rifiutati; governante ottenuto dagli esiti di stati completi; combinazioni lineari rifiutano dati non compatibili. |
| T19 | Intervalli sezionali e scelta ai lati della transizione, ambiguità esplicita. |
| T20 | Barre asimmetriche immutate e momento firmato opposto negli input. **Non è un benchmark indipendente delle capacità resistenti positive/negative del checker.** |
| T21 | Componenti mancanti, dataset modificati, input preparati mutati e report storici non diventano Pass attuale. |
| T22 | Round-trip ricco, vincoli/MPC/massa/offset/carichi/solidi/report e migrazione chiavi legacy; payload mai scritto segnalato irrecuperabile. Non collaudato su archivi storici reali. |
| T23 | Revisioni incompatibili, collisioni e cancellazione non modificano il modello attivo. Matching reale solver da collaudare. |
| T24 | Motore/method assente, meccanismi esclusi e cancellazione danno stato non valutato. Fake confinati ai test. |

L'esempio `Examples/MixedModel` verifica anche l'integrazione reale con il checker quando disponibile, stampa le versioni/esiti e riapre il grafo. `--without-checker` esercita il percorso senza dipendenza. L'SDK segnala `NETSDK1138` per l'esempio net6.0; il target è stato mantenuto come richiesto.

## Completamento del nucleo Model

I quattro blocchi comuni sono implementati: riferimenti fisici/inversioni, assegnazioni variabili/rigidezze, algebra dei risultati e bilanci di equilibrio. `Model` rimane indipendente da Converter, SDK dei solver e Checker. Il supporto è esplicito per beam rettilinei a due nodi e plate piane triangolari/quadrangolari; geometrie/materiali specialistici sono elencati sotto, non approssimati automaticamente.

`Equilibrium` costruisce risultanti da `PointLoad`, carichi beam uniformi/lineari con coppie distribuite, eccentricità e lunghezza reale/proiettata, pressioni uniformi sulle plate e azioni nodali registrate. Molle e link usano le azioni effettivamente esportate con corpo identificato; contributi esterni già risolti dal solver possono essere forniti con `Explicit`. Un carico proiettato richiede la normale unitaria al piano di proiezione. Forze in N e momenti in Nmm sono sommati globalmente rispetto al polo richiesto.

Il chiamante dichiara corpo libero, stato concomitante e manifest delle azioni fisiche attese tramite `EquilibriumScope`. Copertura incompleta, dati scaduti, stati incompatibili, parti mancanti o rappresentazioni duplicate danno `IsBalanced=null`, mai un esito positivo. `PhysicalActionId`, `Representation`, `Part` ed `ExpectedParts` consentono di usare le forze nodali equivalenti senza contare anche il carico distribuito originario. Il servizio non può dedurre da soli record incompleti la presenza di tutti i carichi del modello: `CompleteCoverageConfirmed` deve derivare da evidenza della sorgente, indicata in `CoverageEvidence`.

Esempio completo del solo Model:

```powershell
dotnet run --project Examples/MixedModel/MixedModel.csproj --no-restore -- --model-only
```

L'esempio dichiara la combinazione prima dei risultati, combina nodi/beam/plate, prepara input locali, seleziona stati governanti, controlla un corpo libero analitico e riapre l'archivio con dipendenze dei risultati integre. Non contiene una riscrittura delle impronte per rendere correnti risultati vecchi e non istanzia Checker.

Le nuove prove sono in `ModelPhysicalCompletionTest`, `ModelResultAlgebraTest` e `ModelEquilibriumTest`: formule analitiche, inversioni avanti/indietro, assegnazioni/carichi, rifiuti atomici, perdita di concomitanza, provenienza mutata, archivi senza campi opzionali e round-trip di combinazioni/carichi di area. Rimangono necessari i collaudi su archivi storici reali e dataset di produzione estesi.

Verifica del completamento: build `GPCModel.sln` con 0 errori/avvisi; **748 test UnitTest passati** (41 nuovi), un test nativo Straus7 opzionale non attivato; **8 test di regressione dell'adattatore ModelChecker passati**, senza modificare il progetto Checker. Esempio `--model-only` concluso con input Node/Beam/Shell Ready, equilibrio del corpo libero analitico e archivio riaperto Ready. Rimane l'avviso SDK preesistente dell'esempio net6.0.

I lavori **esterni al Model** restano distinti: nei converter occorre interpretare materiali/sezioni, vincoli, rilasci, carichi, offset e fasi dei solver e completare SAP2000/GEN; in Checker occorrono il metodo shell e i meccanismi resistenti ulteriori; nell'applicazione serve l'interfaccia di gestione. Per Straus7 le proprietà elastiche conservate non forniscono da sole resistenze e armature. Aggiungere proprietà fisiche dopo l'importazione cambia l'impronta di analisi: prima di usare i risultati serve un abbinamento rivalidato alla sorgente, non una riscrittura automatica dell'impronta.

## Lavoro ancora necessario per il flusso Anthea completo

1. Completare/validare i connettori nell'ordine richiesto: **Civil NX API, Civil tramite MGT, Straus7, SAP2000, GEN NX, GEN**. Per Civil NX il trasporto e lo schema geometrico comune sono implementati; mancano mapping delle assegnazioni, convenzioni dei risultati e benchmark reali di segni/assi/offset. Nessun importatore completo dei sei solutori è dichiarato finito.
2. Scelta e validazione ingegneristica del metodo shell (accoppiamento Nxy/Mxy, facce, armature, strisce e verifiche), con benchmark indipendenti. Il contratto è pronto, l'algoritmo no.
3. Estensioni specialistiche del catalogo sezionale/materiali qui sotto e formulazioni oltre beam rettilinei/plate piane. Il nucleo comune non esegue analisi FEM o ricostruzioni arbitrarie di contributi non esportati.
4. Integrazione dei risultati e dell'archivio nel percorso effettivo di Anthea; convalida su archivi legacy reali, compresi sottotipi non presenti nelle fixture.
5. Convalida ingegneristica del motore reale per casi di progetto, copriferri/requisiti costruttivi e capacità ulteriori effettivamente disponibili. I test software non costituiscono certificazione del prodotto.

Il nucleo e l'esempio sono eseguibili; questi punti restano limiti espliciti, non verifiche o importazioni simulate in produzione.

## Catalogo sezioni: presenti ed estensioni per copertura generale

Inventario dei sorgenti del 30 settembre 2026. Una geometria rappresentabile non implica che torsione, ingobbamento, rigidezza composta o verifica normativa siano già disponibili per ogni uso. Le sagome possibili sono illimitate: la copertura generale richiede sia primitive parametriche sia sezioni arbitrarie e dati meccanici importati.

### Già presenti

| Famiglia | Tipi esistenti / copertura |
|---|---|
| Piene elementari | `SectionRectangular` (anche quadrata/piatto), `SectionCircular` |
| Cave | `SectionCHS`, `SectionRHS` (anche SHS; spessori delle quattro pareti distinti) |
| Aperte | `SectionH` (ali anche diverse), `SectionT`, `SectionL`, `SectionC` |
| Travi da ponte in acciaio | `SectionHInclinedWeb`, `SectionHDoubleBottomFlange`, `SectionSteelBox` |
| Generiche | `Section(Shape2d)` per contorni/fori e `Section(...)` per proprietà assegnate |
| Materiali beam | `SteelSection`, `ReinforcedConcreteSection`, barre circolari e posizionamento di profili metallici nel calcestruzzo |
| Plate | `ConcretePlateProperty`, `SteelPlateProperty`, `GlassPlateProperty` con componenti vetrati |
| Variazione lungo il beam | Intervalli costanti, sezioni esplicite tabulate, legge rettangolare lineare compatibile; profili separati di rigidezza |

`SectionSteelBox` è un **cassoncino aperto superiormente**: non rappresenta già la rigidezza torsionale di un cassone chiuso dalla soletta. Nella `Section` geometrica di base alcuni metodi specializzati restituiscono valori di base o non sono implementati: non assumere che un contorno poligonale fornisca automaticamente un calcolo generale di torsione/ingobbamento. L'importazione dei valori del solver deve conservarne provenienza e disponibilità.

### Da implementare o completare, senza duplicare le classi presenti

| Priorità | Famiglia | Casi da coprire e lavoro effettivo |
|---|---|---|
| P1 | Poligonali arbitrarie complete | Piene, concave, con più fori e più regioni; riconoscimento contorni, disponibilità esplicita di A/I/J/Cw/aree di taglio/centri e proprietà importate. Riutilizzare `Section`/`Shape2d`; completare i dati mancanti senza inventare zeri. |
| P1 | Cassoni chiusi | Rettangolari/trapezoidali, mono e multicella, ali a sbalzo, anime inclinate, spessori diversi; geometria parametrica e descrizione delle celle. RHS e cassoncino aperto non bastano per tutti questi casi. |
| P1 | Sezioni civili/PSC da ponte | I, T, doppia T, U/vasca, cassone, bulbo, travi prefabbricate, lastre alveolari/piene/alleggerite e impalcati a più anime. Sagome generiche sono già possibili; mancano generatori/dati specializzati e mapping dei solver. |
| P1 | Calcestruzzo precompresso | Cavi/trefoli/barre, posizione e area, materiali, aderenza, stato e fase della precompressione, guaine e perdite fornite dalla sorgente. La deformazione iniziale del singolo tondino già presente non è un modello completo del cavo. |
| P1 | Composite acciaio–calcestruzzo | Trave I o cassoncino + soletta, cassone chiuso dalla soletta, più travi/regioni, larghezza efficace, calcestruzzo gettato in fasi diverse, stato fessurato/omogeneizzato e connessione. Estendere le parti metalliche nel c.a. esistenti. |
| P1 | Sezioni variabili generali | Rastremature di H/I, cassoni, PSC, cerchi/cave e composite; interpolazione esplicita dei parametri, transizioni di topologia, orientazione/offset variabili e sezioni per fase. Oggi i casi generali richiedono sezioni tabulate. |
| P2 | Profili aperti sottili | Z, C/U con labbri, Z con labbri, omega/cappello, sigma, angolari irrigiditi e altri sagomati a freddo; raggi e spessori reali, regioni efficaci distinte dalla geometria lorda. |
| P2 | Profili composti metallici | Doppi L, doppi C, cruciformi, H accoppiati, sezioni saldate da piatti, irrigidimenti longitudinali; componenti con traslazione/rotazione e ipotesi di collegamento esplicite. |
| P2 | Tubolari e forme piene aggiuntive | Ellissi piene/cave, ovali, poligoni regolari, triangoli/trapezi e profili asimmetrici; come generatori parametrici sul supporto generico. |
| P2 | SRC e tubi riempiti | Profili inglobati, tubi circolari/rettangolari riempiti, doppia pelle; regioni/materiali e interazione. Il posizionamento di acciaio nel c.a. copre parte della geometria, non tutte le ipotesi meccaniche. |
| P2 | Sezioni a fibre/multimateriale | Regioni con materiali/leggi distinti, fibre esplicite, barre, predeformazioni, parti attive per fase; conservare sia la rappresentazione geometrica sia quella di analisi. |
| P2 | Plate/shell stratificate | Strati di materiali con quote, spessori e orientazioni, matrici membrana/flessione/accoppiamento/taglio e disponibilità; ortotropia, sandwich e laminati. Le barre delle shell e il vetro stratificato presenti non costituiscono un modello generale di laminato. |
| P3 | Legno | Massiccio, lamellare, LVL, sezioni composte e pannelli CLT; materiali ortotropi e stratigrafia. Rettangoli/I sono già geometrie riutilizzabili. |
| P3 | Alluminio e FRP | Estrusi aperti/chiusi/multicella, profili pultrusi e laminati; riutilizzare contorni generici, aggiungendo composizione e orientazione del materiale dove necessarie. Materiali base presenti non equivalgono a un modello sezionale completo. |
| P3 | Rinforzi e sezioni per fasi | Incamiciature, piatti/profili aggiunti, FRP, ripristini di calcestruzzo e sezioni residue/corrose; geometria e attivazione delle singole parti, con stati resistenti da valutare in Checker. |

IPE/IPN/HEA/HEB/HEM, UPN/UPE, W/S/HP e analoghe sigle di catalogo richiedono **database e mapping dei parametri**, non necessariamente nuove classi geometriche. Raccordi, inclinazione delle ali e tolleranze devono però corrispondere al profilo reale. Cavi, tiranti, molle, link, aste tension-only e contatti sono **formulazioni di elemento**, non nuove sagome di sezione.

Le famiglie proposte sono coerenti con la distinzione fra sezioni standard, generiche, composte e rastremate nei manuali dei solver: [MIDAS CIVIL NX – Section Properties/SPC](https://support.midasuser.com/hc/en-us/articles/45996449125401--CIVIL-NX-Understanding-the-SPC-Feature-in-MIDAS-CAD), [Strand7 – beam sections standard e arbitrarie](https://www.strand7.com/html/buildingmodels/bxs.htm), [SAP2000 – Frame Sections](https://docs.csiamerica.com/help-files/sap/Menus/Define/Section_Properties/Frame_Sections/Frame_Section.htm). Priorità e suddivisione in classi sono una proposta architetturale per questo repository, non requisiti imposti dai produttori.
