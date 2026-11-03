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
- Il salvataggio dei sidecar nel ciclo reale del documento GH resta da integrare. La prova SDK dimostra che `GH_DocumentIO.SaveQuiet(path)` non rende necessariamente disponibile `path` al componente durante `Write`; non si assume che `OnPingDocument().FilePath` identifichi il destinatario del Save As.
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

## Lavoro ancora necessario per chiudere F6

1. Completare F6B: sostituire lo storage fisico legacy nei componenti e nei generatori; completare la separazione fra input, obiettivi e output; collegare sidecar, riapertura, copie del documento e Save As; verificare tutte le conversioni dei tipi F5 e le diagnosi dei dati non supportati.
2. Eseguire F6A: migrare componenti, generatori del ponte, carichi mobili, fasi e verifiche BridGHe; collegare Converter alla produzione e rimuovere l'exporter locale solo dopo confronto equivalente.
3. Verificare GUID/porte, esempi 50/99 e membrane, input/output nativi, archivi e prestazioni sul set candidato completo. Conservare le baseline congelate e documentare le differenze, senza aggiornarle per ottenere test verdi.

I gate generali F7/F8, integrazione degli altri consumatori e rilascio restano successivi. F6 rimane attiva finché i punti sopra non sono completati.

La lettura NRBF segue il decoder ufficiale e la ricostruzione esplicita di un insieme chiuso di tipi: [guida Microsoft](https://learn.microsoft.com/en-us/dotnet/standard/serialization/binaryformatter-migration-guide/read-nrbf-payloads), [pacchetto fissato 9.0.16](https://www.nuget.org/packages/System.Formats.Nrbf/9.0.16).
