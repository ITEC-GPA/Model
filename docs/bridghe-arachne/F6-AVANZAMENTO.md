# F6 — migrazione dei plugin, in corso

Avvio richiesto dall'utente il 3 novembre 2026. Branch isolato `codex/gpc-unification-20261102`; nessun merge, push o rilascio. F5 resta conclusa sul suo set di commit. Questo documento registra incrementi di F6, **non il superamento del gate di uscita della fase**.

## Primo incremento: percorso di calcolo Arachne

- Il componente reale `FormFindComponent` costruisce un grafo GPC normalizzato tramite `FormFindingInputBuilder`, lo esporta con `TensileModelAdapter` ed esegue i metodi lineare, non lineare e lineare a passi tramite l'adattatore condiviso. Non costruisce più direttamente il modello fisico TensileLib.
- Gli input GPC restano immutati dal calcolo. Spostamenti, reazioni e sforzi assiali sono associati al grafo mediante `SolverIdentityMap`, dataset e impronta dell'input. Ogni iterazione conserva in memoria un riferimento preparato con `ExternalModelArchive`; `ReadAnalysis` riapre un grafo indipendente con i risultati.
- La precedente funzione di assemblaggio è conservata **solo nel progetto di collaudo** (`LegacyFormFindingBaseline.cs`), estratta dal commit Arachne `5ddbe6f8a78de788a120c23cd6015c350cd2c320`. L'entry point pubblico di compatibilità nel plugin delega al nuovo percorso GPC.
- Le opzioni di calcolo e gli obiettivi di lunghezza, forza assiale e densità di forza hanno backing GPC. Le impostazioni inattive dei controlli legacy restano nei wrapper; non entrano come obiettivi fisici incompatibili.
- Identità dei componenti Node, Beam, Plate e Build Model derivate da `InstanceGuid` e indirizzo di output Grasshopper; i nodi generati dipendono dall'identità dell'elemento e dalla posizione topologica I/J o del vertice. Copie e ricalcoli mantengono le identità. Il modello wrapper v6 persiste proprietario e registro delle identità.
- Archivi wrapper con intestazione `ARCGPC2`, vocabolario chiuso e codec numerico condiviso `ContractXml`. La lettura legacy decodifica record NRBF con `System.Formats.Nrbf` 9.0.16, senza `BinaryFormatter.Deserialize`. I tipi non ammessi, gli schemi futuri e gli archivi fuori limite sono rifiutati. Le fixture NRBF sono nuove catture dichiarate, non documenti dell'utente e non rigenerazioni delle baseline F1/F2.
- Corrette copie superficiali di modelli, elementi, obiettivi e wrapper geometrici/proprietà/risultati; la direzione Warp/Weft ora sopravvive alla copia di un obiettivo. Ripristinati il salvataggio completo della proprietà di trave e la persistenza del plate con GUID, geometria e proprietà.

## Scostamenti e limiti specifici

- Le classi fisiche legacy nei plugin **non sono ancora eliminate**. Materiali, sezioni, assegnazioni e molte operazioni di costruzione/output usano ancora i wrapper precedenti come sorgente. Il grafo GPC descritto sopra è il grafo dell'analisi di form finding, non ancora l'intero modello applicativo sostitutivo.
- Il secondo incremento collega i sidecar al writer GH usando `ArchiveLocation`, verificato sullo SDK installato. `OnPingDocument().FilePath` resta escluso dalla scelta della destinazione. Restano da collaudare il ciclo interattivo completo e i documenti di grande scala; il terzo incremento verifica il ripristino degli output GH dai risultati persistiti.
- Nel vecchio archivio di `GH_BeamProperty` mancava la sezione: quei dati non possono essere ricostruiti dal solo payload. Il nuovo reader rifiuta il contenuto incompleto invece di introdurre una sezione di default. I componenti upstream possono ricostruire la proprietà dagli input della definizione.
- I carichi lineari sono integrati dalle **intensità**, non da `LineLoad.GetGlobalLoadVector()` che restituisce già una risultante. Per le distribuzioni collineari a risultante non nulla si conserva la rappresentazione al baricentro. Una distribuzione a risultante nulla conserva il primo momento mediante due forze equivalenti; il vecchio assemblaggio perdeva questo contributo.
- In IPS la massa inerziale del solver deriva coerentemente da forza e accelerazione. La precedente interpretazione di lbm come massa coerente con lbf/in era dimensionalmente errata. Il nuovo percorso segnala la correzione; la verifica dedicata usa l'equilibrio analitico di due cavi simmetrici, senza cambiare una baseline esistente.
- Le masse lineari e superficiali del grafo di form finding sono esplicite. Area unitaria e spessore unitario del modello interno TensileLib sono riferimenti per riprodurre queste masse; non costituiscono una sezione o uno spessore fisico aggiuntivo.

## Verifiche dell'incremento

Log nella directory di workspace `artifacts/f6/`; evidenze dell'incremento in [f6-incremento1-evidenze.json](f6-incremento1-evidenze.json). Commit Arachne `7360f3cb2289694809f6fdb66cd57dc1a9408ac0`.

- **Suite headless Arachne: 166 PASS, zero fallimenti**, build Release senza flag `GpcPilot`: 66 controlli GUID/porte rispetto a FeMM, 38 di form finding, 43 di patterning e 6 di reti, oltre a 13 nuovi contratti F6. Il confronto numerico diretto con l'assemblaggio precedente copre N/mm, SI e kN/m.
- Caso analitico di carico lineare da +1 a −1 N/mm su 1.200 mm: risultante nulla e primo momento −240.000 N mm, equivalenti a +200/−200 N agli estremi.
- Lettura delle fixture NRBF, alias, indipendenza delle copie, schema futuro, zero negativo, identità al ricalcolo, proprietà/plate internalizzati, associazione dei risultati all'impronta.
- `SharedXmlBoundaryPreservesForeignWrapperDoubleBits`: eseguito e superato su net6.0 e net472. SHA-256 di Geometry e Utilities coincidono con il set fissato da Model.
- Versioni di sviluppo: GPCModel **5.47.0.0** (nuovo confine XML riutilizzabile; allineata anche AssemblyVersion), Arachne **0.4.0-f6.1**, Assembly/FileVersion **0.4.0.0**. Persistence, Converter, Checker e TensileLib non modificati in questo incremento.

## Secondo incremento: storage dei nodi e persistenza dei risultati

- `NodeModel` conserva posizione, nome e identità in `NodeElement`; `CaseModel` usa `LoadCaseBase`; `NodeForceLoadModel` conserva le componenti in `PointLoad`. I valori Rhino sono viste per l'interfaccia. Le quantità dei wrapper seguono ancora le unità selezionate nel documento e vengono normalizzate dal builder; questi oggetti draft non vanno esportati direttamente come grafo N/mm.
- Schemi wrapper Node/Case/NodeForce v2, con lettura delle versioni precedenti. Le identità legacy dipendono da namespace/proprietario persistente e non da un nuovo GUID a ogni lettura. Il wrapper GH dei casi ora salva anche ID e identità; l'archivio storico con solo nome è ancora letto. Le copie possiedono i rispettivi carichi e casi.
- `AnalysisDocumentState` salva i riferimenti dei run nel componente Form Find e i blocchi nel file `<documento>.<InstanceGuid>.gpc-results`. Il percorso deriva dal writer dell'operazione corrente, anche nel Save As. Riapertura e spostamento del progetto risolvono riferimenti relativi; la lettura del run non invoca il solver.
- `BlockArchive.SaveSnapshot` / `ExternalModelArchive.SaveSnapshot` conservano tutti i blocchi già presenti quando il commit del documento è controllato dall'host e non osservabile dal componente. Non si presume riuscita la scrittura GH dopo il callback `Write`. Lo storico non viene compattato automaticamente in questa modalità: è il compromesso esplicito per preservare i documenti precedenti anche dopo più tentativi falliti.
- Clipboard/undo senza percorso contengono un pacchetto GPC portabile e la mappa del solver, con limite cumulativo di 256 MiB; i file GH contengono solo i riferimenti. L'input GH resta apribile se manca il sidecar; apertura dei risultati e tentativo di copiarli/salvarli segnalano i dati mancanti. Un nuovo calcolo può ricostruirli.
- Corretto l'allineamento delle iterazioni dopo la riapertura: il nuovo calcolo sostituisce il run salvato senza aggiungere slot duplicati. Le vecchie cache di output vengono svuotate alla lettura del componente.
- Versioni di sviluppo: Arachne **0.4.0-f6.2**, GPC.Archive **1.1.0.0**, GPC.Model.Persistence **3.13.0.0**. GPCModel resta **5.47.0.0**; nessun cambiamento ai suoi contratti fisici in questo incremento.

Verifiche eseguite: suite Arachne **168 PASS**, 0 fallimenti (66 inventario, 38 form finding, 43 patterning, 6 reti, 15 contratti F6); suite `ExternalArchiveTest` **16 PASS** su ciascuno dei runtime net6.0/net472. I controlli includono bit dello zero negativo, identità/copie, file GHX e GH, salvataggio invariato, Save As, clipboard, trasferimento, sidecar mancante e ricalcolo dopo riapertura. Log: `artifacts/f6/checkpoint2-*.log`, `external-net6.log`, `external-net472.log`. Gate completo `Model/build/Verify.ps1`: **1.897 PASS**, 66 esclusi/ignorati per opzioni native/campioni non configurati in questa prima esecuzione; la ripetizione con ambiente nativo F5 è fallita per `St7Init -8`, chiave hardware non disponibile; collaudo nativo da ripetere quando ripristinata; nessuna rimozione API inattesa, equality audit superato. [Evidenze del secondo incremento](f6-incremento2-evidenze.json). Commit Arachne `62fedfa7c29828cc5329dbf8f769bf83b82d4270`.

Nel secondo incremento gli output GH richiedevano ancora il calcolo. Il terzo incremento rimuove questa dipendenza per le analisi convergenti e corregge il collaudo di riapertura: `GH_DocumentIO.Open` restituisce un documento disabilitato, che il test ora abilita esplicitamente prima di ricalcolare e controllare gli output.

## Terzo incremento: ripristino degli output senza solver

- `FormFindingOutputProjection` ricostruisce nodi, mesh, pressioni, reazioni e precarichi dal modello GPC e dai dataset. Le coordinate finali native sono conservate in `OriginalResultData` per evitare perdita di bit nella somma posizione iniziale + spostamento. La proiezione non modifica input e risultati archiviati.
- Alla riapertura, con Auto Run disattivato, il componente confronta l'impronta dell'input: ripristina gli output se coincide; li svuota e richiede un nuovo calcolo se cambia. Tornando all'input precedente recupera il run salvato. Un vecchio archivio pressurizzato privo delle informazioni necessarie segnala che deve essere ricalcolato.
- `NodalForceKind.AppliedPressure` distingue le forze nodali di pressione dalle reazioni e impone la convenzione `BodyOnNode`. Il caso di pressione appartiene all'input, quindi il nome del caso entra nell'impronta. Versioni: GPCModel **5.48.0.0**, Arachne **0.4.0-f6.3**.
- L'output di un calcolo non convergente conserva ancora la proiezione transitoria precedente; viene segnalato e non è archiviato come analisi completata.
- Suite Arachne **169 PASS, zero fallimenti** (66 inventario, 38 form finding, 43 patterning, 6 reti, 16 contratti F6). Dopo il ripristino della chiave, gate completo Model con ambiente nativo F5: **1.958 PASS, 7 esclusioni previste, zero fallimenti** (63 Core, 1.796 UnitTest, 99 net472). API: 9.946/39 firme, zero rimozioni inattese; equality audit superato. Questo collaudo include i cambiamenti degli incrementi precedenti e supera il blocco di licenza registrato al secondo incremento.
- Commit Arachne `bd79ae28d3e42f82669d9c3d88582f72c9e636e8`; [evidenze del terzo incremento](f6-incremento3-evidenze.json). Nessuna baseline rigenerata.

## Quarto incremento: vincoli, obiettivi nodali e carichi concentrati

- I vincoli traslazionali sono conservati come `DofRestrain`; gli obiettivi sulle coordinate come `FormFindingCoordinateGoal`. I controlli booleani/vettoriali del plugin sono viste. Il builder clona i dati e associa l'identità del nodo senza modificare il wrapper. Assi, copie, zero negativo e lettura dei precedenti schemi sono verificati.
- `BeamPointLoadModel` conserva intensità e posizione in `BeamLoadAssignment.Concentrated`; il cambio forza/momento trasferisce le componenti nella famiglia corretta e la copia possiede anche il caso. Lo schema v2 salva il sistema di riferimento e il builder normalizza una copia del carico. Un caso con assi ruotati verifica il vettore globale rispetto a un risultato analitico indipendente.
- **Dati legacy incompleti:** lo schema v1 del carico concentrato non scriveva `CoordinateSystem`. Il reader conserva componenti e posizione, ma marca l'asse mancante; il calcolo richiede il ricalcolo del componente upstream oppure un'assegnazione esplicita degli assi. Il flag sopravvive a un nuovo salvataggio. Non è possibile ricostruire un sistema perso dal solo payload.
- Versione Arachne **0.4.0-f6.4**, commit `5f5170cd14267081f138f25fe1e8d70b97ad211a`. Suite completa **171 PASS, zero fallimenti** (66 inventario, 38 form finding, 43 patterning, 6 reti, 18 contratti F6). Tutte le 11 suite Checker sul Model 5.48.0.0: **10.382 PASS, zero fallimenti**, un controllo temporale escluso; API senza rimozioni inattese. [Evidenze del quarto incremento](f6-incremento4-evidenze.json).

## Lavoro ancora necessario per chiudere F6

1. Completare F6B: sostituire lo storage fisico legacy nei componenti e nei generatori; completare la separazione fra input, obiettivi e output; completare i collaudi interattivi/di scala dei sidecar; verificare tutte le conversioni dei tipi F5 e le diagnosi dei dati non supportati.
2. Eseguire F6A: migrare componenti, generatori del ponte, carichi mobili, fasi e verifiche BridGHe; collegare Converter alla produzione e rimuovere l'exporter locale solo dopo confronto equivalente.
3. Verificare GUID/porte, esempi 50/99 e membrane, input/output nativi, archivi e prestazioni sul set candidato completo. Conservare le baseline congelate e documentare le differenze, senza aggiornarle per ottenere test verdi.

I gate generali F7/F8, integrazione degli altri consumatori e rilascio restano successivi. F6 rimane attiva finché i punti sopra non sono completati.

La lettura NRBF segue il decoder ufficiale e la ricostruzione esplicita di un insieme chiuso di tipi: [guida Microsoft](https://learn.microsoft.com/en-us/dotnet/standard/serialization/binaryformatter-migration-guide/read-nrbf-payloads), [pacchetto fissato 9.0.16](https://www.nuget.org/packages/System.Formats.Nrbf/9.0.16).
