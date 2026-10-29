# GPC Model: post-processing NODE / BEAM / SHELL

## Sviluppo: assegnazioni Civil NX per il visualizzatore (9 ottobre 2026)

Branch `codex/viewer-boundary-data`, dalla base `master` a324af3f. Converter 2.0.2, non ancora rilasciato.
Le classi del dominio per svincoli, molle e relazioni cinematiche esistevano già; il convertitore ora le alimenta con:

- FRLS: sei gradi di libertà I/J, svincolo completo, fissità relativa o rigidezza assoluta, assi MIDAS espliciti;
- RIGD: equazioni dei soli gradi di libertà richiesti, nodi master/slave e bracci firmati;
- ELNK: link RIGID oppure GEN elastico diagonale con rotazione e conversione coerente N/mm/rad.

La presenza di fasi, warping negli svincoli, leggi non lineari, accoppiamento a taglio, condizioni miste rigido/elastico
o assi nodali non risolti conserva i record e produce diagnostica: non crea un equivalente meccanico arbitrario.
Svincoli su truss o altre formulazioni assiali rimangono conservati senza assegnare gradi di libertà flessionali.
Errori di connettività e coefficienti non validi rifiutano il candidato atomicamente.

Collaudo: 90 test mirati di importazione, assegnazioni, persistenza e regressione; prova reale Civil NX con
3.755 nodi, 181 beam, 3.435 plate, 381 ELNK rigidi e 1.263 relazioni master/slave da due record RIGD.
La serializzazione esplicita conserva svincoli e link, che contribuiscono all'impronta degli input di analisi.
Le evidenze private sono negli artefatti del branch visualizzatore ANTHEA. Nessun aggiornamento del bundle DLL di ANTHEA.
Restano da estendere/completare: leggi non lineari, PRLS, attivazione per fase, trasformazione MPC con SKEW,
warping, formulazioni di compressione/cavo e adattatori degli altri solutori.

Il riordino attuale usa **Model 4** e richiede la ricompilazione dei consumer. La [guida di migrazione](MIGRAZIONE_MODEL_4.txt) descrive namespace, DLL da aggiornare e compatibilita' degli archivi. Per il controllo completo: `./build/Verify.ps1`.

Il dominio FEM rimane `GPC.Model.Models.Model`. Nodi, beam, shell e solidi condividono identità e registri; gli assi sono sempre i tipi reali `Point3d`, `Vector3d` e **`GPC.Geometry.CoordinateSystem`**.

I convertitori sono nel progetto separato **`GPC Converter/GPC Converter.csproj`**, incluso in `GPCModel.sln`, con assembly/namespace `GPC.Converter`. `Model` non dipende dai convertitori né dal verificatore. L'adattatore ai motori di verifica, `GPCModelChecker`, sta nel repository **Checker** (`Checker/ModelChecker`), a valle di Model: dal 10/10/2026 Model fissa solo Geometry e Utilities e nessuna DLL di Checker (revisione di architettura, R3).

**Risultati compatti** (revisione di architettura, R1; progetto e misure in [ARCHIVIO_RISULTATI_20261010.txt](ARCHIVIO_RISULTATI_20261010.txt)):

- copie, preparazioni, snapshot e archivi non serializzano più i campioni dentro il grafo. I risultati di ogni elemento sono un blocco binario compresso e immutabile, condiviso dalle copie e decodificato al primo accesso a `Element.Results` (API invariata);
- `Model.CompactResults()` comprime i risultati e rilascia gli oggetti: dopo un import grande va chiamato a ogni dataset. `Model.ContainsResultsOf(dataset)` interroga i risultati senza decodificarli;
- `ModelArchive.Save` scrive il documento XML (versioni 1-4) per i modelli senza risultati fino a 64 MB, altrimenti il pacchetto versione 5 (ZIP con `model.xml` e `results.bin`). `SaveDocument` scrive sempre il documento XML versione 4 con i risultati in linea, adatto ai modelli piccoli. `Load` legge tutte le versioni. Le versioni precedenti della libreria rifiutano il pacchetto: non lo leggono a metà;
- `Load` legge entro tetti che seguono la dimensione del file: i documenti 1-4 entro 256 Mi caratteri e 2.000.000 di oggetti e inoltre entro i propri byte; `model.xml` del pacchetto e gli snapshot dell'analisi, senza il tetto di un documento, entro i caratteri che deflate può espandere dai loro byte (al più 1032 volte); in ognuno al più un oggetto ogni quattro caratteri. Un testo breve non può dichiarare un array grande (`z:Size`) e `results.bin` si legge entro i byte che dichiara;
- un risultato che punta a un campione di un altro elemento (DerivedFrom) non è copiabile: la copia lo rifiuta con `NotSupportedException`.

```powershell
dotnet build GPCModel.sln --no-restore
dotnet test UnitTest/UnitTest.csproj --no-restore
dotnet run --project Examples/MixedModel/MixedModel.csproj --no-restore
dotnet run --project Examples/MixedModel/MixedModel.csproj --no-restore -- --model-only
dotnet run --project Examples/MixedModel/MixedModel.csproj -- --civil-file Examples/CivilTextSynthetic.mct synthetic-revision
dotnet run --project Examples/MixedModel/MixedModel.csproj -- --straus-api-file 'C:\modelli\modello.st7' rev-1
```

Servono le dipendenze GPC locali indicate nei progetti. Su un checkout senza `obj/project.assets.json`, eseguire prima `dotnet restore` con le sorgenti NuGet aziendali configurate. Target e versioni dei pacchetti esistenti sono invariati: librerie `netstandard2.0`, test/esempio `net6.0`.

L'esempio è **sintetico**: non esegue un'analisi FEM. Esercita topologia condivisa, risultati e armature, preparazione, verificatore reale quando presente, archivio e riapertura. Le risultanti shell assegnate non costituiscono una soluzione in equilibrio del modello misto.

Il flusso comune comprende gruppi gerarchici e importazione atomica di **reazioni/azioni nodali, spostamenti/rotazioni e risultanti beam/plate**, con provenienza e unità. Le azioni nodali dei singoli elementi mantengono famiglia, elemento proprietario, estremo I/J e corpo dell'azione. `ResultPreparation.Prepare` seleziona nodi/elementi per gruppi e stati, valida i dati e prepara copie negli assi locali tramite `CoordinateSystem`; le copie diventano non correnti dopo modifiche agli input o ai risultati. `ModelPreparation` controlla invece l'idoneità strutturale prima di `ModelChecker.Verify(model, request, cancellationToken)`. Ogni lavoro può scegliere gruppi/elementi, dataset/casi/fasi e impostazioni diverse. Il servizio crea uno o più Checker reali, riutilizzando sezioni e configurazioni compatibili, e mantiene nel report anche verifiche mancanti o non supportate. Ai nodi non viene attribuito un esito generico di verifica del calcestruzzo.

Il primo connettore è **MIDAS Civil NX API**: trasporto in sola lettura, schema comune unità/nodi/connettività, richiesta tabelle beam/plate e lettura delle colonne per nome. È predisposto per profili di schema di più versioni, senza whitelist di release. Campi aggiuntivi conservati; schemi incompatibili rifiutati con diagnostica. Il collaudo attuale usa risposte sintetiche: **non è una dichiarazione di compatibilità verificata con tutte le versioni**.

Il secondo lettore è **MIDAS Civil MCT e Gen MGT** (`MidasCivilTextReader`, `MidasGenTextReader`, scelta dall'intestazione o dall'estensione con `MidasTextFiles`): traduce i comandi nei record dei database Civil NX e li mappa con il lettore dell'API, quindi con le stesse convenzioni. Importa nodi, travi, truss, elementi solo trazione/compressione e cavi, piastre con assi e spessori, materiali, sezioni, gruppi, casi, combinazioni, fasi (metadati), vincoli anche in assi nodali, molle puntuali, rilasci, offset, link rigidi ed elastici lineari e carichi nodali, beam, di pressione e di peso proprio. Pareti, elementi piani e solidi, piani e diaframmi di Gen restano conservati con avviso senza rifiutare il file. L'MCT degli export nativi Civil NX 9.5.5 dà lo stesso modello GPC dell'API. [Comandi e limiti](POST_PROCESSING.md#midas-civil-file-mctmgt).

Il terzo connettore è **Straus7 API R3** (`Straus7ApiConverter`): apre i file ST7 in sola lettura tramite la DLL installata, importa nodi, beam/plate, assi nativi `CoordinateSystem`, gruppi gerarchici e casi. Normalizza risultati statici lineari beam/plate, reazioni nodali, spostamenti in mm e rotazioni in radianti. Con `IncludeElementNodeForces=true` acquisisce anche le azioni nodali dei singoli beam/plate, se memorizzate dal solver. Importa anche truss, cavi, molle-smorzatori (link elastici), rilasci di estremità, molle nodali, link rigidi/master-slave/pinned e combinazioni LSA; i brick e gli altri link sono conservati con avviso. Con `IncludePlateNodeResults=true` legge le risultanti plate ai nodi di ogni elemento, senza media. Collaudato con **Straus7 R31 / API 3.1.5**: un modello plate del produttore, benchmark risolti dal solver reale (uno versionato) e quattro modelli BridGHe con i loro risultati. Richiede Windows x64 e licenza API; altre release R3 con lo stesso ABI sono predisposte ma non dichiarate collaudate. [Uso e limiti](POST_PROCESSING.md#straus7-api-r3).

Ordine dei connettori richiesto: Civil NX API → Civil tramite MGT/MCT → Straus7 → SAP2000 → GEN NX → GEN. Il prossimo convertitore è SAP2000.

Il nucleo Model comprende ora anche riferimenti fisici beam con offset e tratti rigidi, trasporto al baricentro, inversioni I/J e normali su copie del modello, sezioni tabulate/rettangolari variabili, profili di rigidezza separati dalla resistenza, combinazioni nodali/beam/plate, inviluppi di stati concomitanti, accumulo esplicito di incrementi lineari e bilanci di equilibrio con copertura e identità dei contributi. Le inversioni conservano i dati sorgente e rendono non correnti le analisi della revisione precedente. `--model-only` esercita combinazioni, preparazione locale, equilibrio e archivio senza istanziare Checker.

La [documentazione tecnica](POST_PROCESSING.md) contiene contratti, esempi, limiti e il catalogo delle sezioni presenti e da estendere. Restano separati dal nucleo comune i mapping specifici dei converter, SAP2000/GEN, i metodi di verifica da sviluppare in Checker, l'interfaccia e il collaudo su archivi reali estesi. Geometrie/materiali specialistici sono esplicitati nel catalogo: il supporto a una sagoma non implica un algoritmo di verifica resistente per quella famiglia.
