# Task di esercizio delle aste fisiche (spostamenti)

29 ottobre 2026, branch claude/model-sle-aste. Contratti additivi di GPCModel per le verifiche di esercizio (SLE) delle aste: categoria di combinazione dichiarata e ingresso deformativo dell'asta. Il motore che li consuma e Steel.Serviceability.Member in GPCModelChecker (Checker/src/Steel/SERVICEABILITY_MEMBERS.txt).

## Dichiarazione del task

`MemberCheckSpecification` ha due campi facoltativi, assenti dalle impronte quando nulli:

- `Category` (`CombinationCategory?`): il task si pianifica solo per le `ResultSelection` dichiarate con la stessa categoria, mai dedotta dal nome del caso. Obbligatoria per i task deformativi (Characteristic, Frequent, QuasiPermanent); una categoria ultima (Ultimate, UltimateSeismic, UltimateAccidental) limita un task di stabilita. Senza categoria un task resta pianificato per ogni selezione, come prima.
- `Deformation` (`MemberDeformationSpecification`): criterio (`MemberDeformationCriterion`: VerticalDeflection, HorizontalDisplacement, Vibration), direzione negli assi dell'asta (`MemberDeformationDirection`: Axis1, Axis2, Transverse solo per lo spostamento orizzontale) e base della freccia dei carichi variabili (`MemberVariableDeflectionBasis` con `VariableSelection`):
  - `None`: nessuna;
  - `Direct`: la selezione dichiarata e lo stato dei soli variabili, ammessa con un solo stato della categoria nella richiesta (`AmbiguousVariableDeflectionPairing` altrimenti);
  - `Difference`: stato del task meno la selezione dichiarata (per esempio i soli permanenti), per sovrapposizione lineare: tutti gli stati devono essere `LinearStatic` (`VariableDifferenceRequiresLinearStates`).

Il meccanismo e `CheckMechanism.Serviceability`. Combinazioni incoerenti sono rifiutate dalla risoluzione del piano: `MemberServiceabilityRequiresDeformation`, `MemberDeformationRequiresServiceability`, `ServiceabilityCategoryRequired`, `UltimateCategoryRequired`, `InvalidMemberCheckCategory`, `NoResultSelectionForCategory`, `TransverseDirectionOnlyForHorizontalDisplacement`, `VariableDeflectionOnlyForVerticalDeflection`, `VariableSelectionRequiresBasis`, `InvalidVariableSelection`. Un task Serviceability senza categoria ne deformazione resta il task storico basato sulle forze.

## Ingresso deformativo

Per un task con `Deformation`, `BeamCheckPlan` non prepara tagli di forza: raccoglie gli spostamenti dei nodi del percorso dell'asta (`NodeResultDisplacement` dei nodi I/J di ogni elemento, nell'ordine dell'asta) nel tratto Start/End, per lo stato del task e, se dichiarato, per lo stato variabile. Ogni campione e ruotato negli assi dell'asta (`PhysicalMemberGeometry.Axes`: V3 lungo l'asta, V1 e V2 trasversali) con le rotazioni e diventa un `MemberDisplacementSample` (posizione, nodo, stato, D1-D3, R1-R3 in mm e rad, impronta del campione sorgente). `MemberInputSnapshot.Deformation` (`MemberDeformationSnapshot`) contiene specifica, categoria, `Total` e `Variable`; `Samples` resta vuoto.

Controlli, con i codici di BeamActionPreparation dove esistono: dataset e unita N,mm,rad (`DatasetMismatchOrUnknownUnits`), componenti complete e stato concomitante (`IncompleteState`), semantica (`NonConcomitantState`), stati incrementali (`IncrementalOrUnknownPhaseState`), analisi corrente (`StaleAnalysis`, dato Stale), spostamenti riferiti al nodo (`InvalidNodalDisplacement`/`NodalDisplacementNotAtNode`), campioni mancanti o ambigui (`MissingMemberNodeDisplacement`, `AmbiguousMemberNodeDisplacement`), estremi del tratto (`MissingMemberDisplacementBoundary`), stati coerenti (`IncoherentMemberStates`), stato globale confermato (`GlobalMemberStateNotConfirmed`, `VariableMemberStateNotConfirmed`), stato variabile diverso da quello del task (`VariableSelectionIsTheTaskState`). La copertura conta i nodi richiesti e disponibili; un nodo oltre una zona rigida di un percorso OffsetToOffset non e un punto dell'asta e non e usato. Nessun valore e interpolato fra i nodi; `StationResultDisplacement` non e usato (nessun convertitore lo produce).

## Currency e compatibilita

- Lo snapshot entra nell'impronta preparata e nell'identita del resoconto come quello delle forze.
- Le sorgenti nodali si rivalidano in `BeamCheckPlan.IsCurrent` (nodo, campione registrato, dataset, analisi, derivazione).
- Lo scopo (`FingerprintScope`) aggiunge, solo se la richiesta ha task deformativi, i campioni nodali di ogni asta con tali task per tutte le selezioni della richiesta e tutte le selezioni variabili dichiarate: una variazione di soli spostamenti rende non corrente il piano e NotEvaluated il resoconto, esattamente come una variazione di forze. Uno spostamento di un caso estraneo alla richiesta non cambia nulla.
- Campi nuovi con `omitWhenNull` nelle impronte, `OptionalField` negli archivi: richieste, specifiche, task, snapshot e sigilli esistenti hanno le stesse impronte (valori congelati da master 853b56ef in `HistoricalMemberRequestsKeepTheirFingerprints`; archivio di modello, configurazione e resoconto scritto da master in Checker `MemberServiceabilityLegacyArchiveTest`).

## API pubblica aggiunta

Tipi: `MemberDeformationCriterion`, `MemberDeformationDirection`, `MemberVariableDeflectionBasis`, `MemberDeformationSpecification`, `MemberDisplacementSample`, `MemberDeformationSnapshot`. Membri: `MemberCheckSpecification.Category`, `MemberCheckSpecification.Deformation`, `MemberInputSnapshot.Deformation`. Nessuna rimozione o modifica.

## Test

`Model.Core.Tests/MemberDeformationPreparationTest`: assi dell'asta, selezione per categoria (anche dei task di stabilita), stati Direct/Difference, dichiarazioni rifiutate, nodi mancanti e stati non qualificati, cambi di soli spostamenti (stato del task, stato dedotto, rotazione) su piano e scopo, copie dei contratti e impronte storiche.
