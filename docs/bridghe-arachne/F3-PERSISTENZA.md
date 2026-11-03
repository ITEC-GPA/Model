# F3 - Persistenza condivisa

3 novembre 2026; branch `codex/gpc-unification-20261102`. F1/F2 restano congelate; F4 è la prossima fase in coda.

## Architettura attuata

- **GPC.Archive 1.0.0.0**, progetto nuovo netstandard2.0: contenitore binario, blocchi immutabili con SHA-256, indice verificato, sostituzione atomica del file, salvataggio invariato, Save As e conservazione temporanea per undo. Non dipende da Model, Checker, ANTHEA, JSON, Rhino o Grasshopper.
- **GPC.Model.Persistence 3.5.0.0**: `ExternalModelArchive` prepara una revisione; `ExternalModelReference` è ciò che il documento GH deve conservare. Riferimento versionato con nome relativo dell'archivio, GUID del modello, impronta degli input e indirizzo SHA-256 del manifest. `ExternalModelRun` restituisce modello e mappa del solutore.
- ANTHEA conserva soglia inline 16 MiB, JSON per caso/combinazione, cinque copie, navigazione dei documenti e storico. Il nuovo `ModelResultArchive.Shared.cs` delega il contenitore a GPC.Archive. Il codice precedente resta disponibile come riferimento e build predefinita fino al rilascio coerente; nessuna modifica a `lib/Checker`.
- L'adattatore ANTHEA è compilato e provato con `GpcArchiveProject` impostato al nuovo progetto. Non è ancora la distribuzione predefinita di ANTHEA: attivazione ordinaria, snapshot e verifiche complete dell'applicazione appartengono a F7/F8.

## Formato e codec

Il contenitore conserva header di 64 byte e righe d'indice di 57 byte del formato ANTHEA 1. Il profilo ANTHEA mantiene esattamente la firma `ANTHEA.RISULTATI` e i kind 0/1 deflate. Il profilo GPC usa firma distinta `GPC.BLOCKS`; il bit alto del kind indica contenuto già codificato, conservato senza un secondo deflate.

Un run GPC contiene:

1. grafo `ModelDocument` senza risultati degli elementi;
2. un blocco per elemento con risultati, scritto e letto con **gli stessi `ModelImage.WriteResults/ReadResults`** del pacchetto GPC v5;
3. manifest con ordine/GUID degli elementi, riferimenti ai blocchi e indice dei dataset;
4. mappa GUID/ID modello/ID solutore, quando fornita, verificata contro input e registro del modello.

I segmenti compressi del codec esistente sono copiati come byte. Non sono trasformati in JSON per caso. I casi/combinazioni mantengono identità nel grafo e nel codec; `BlocksForDataset` seleziona gli elementi interessati senza decodificarne i campioni. ANTHEA mantiene il suo payload per caso/combinazione: si riutilizza il contenitore, non si forzano due domini diversi nello stesso codec. La presenza di uno snapshot degli input conserva la provenienza storica già prevista da GPC; non aggiunge una seconda copia dei risultati.

Il manifest identifica anche il grafo e l'eventuale mappa del solutore attraverso i digest dei blocchi. Il riferimento non è un digest isolato: `ResolvePath(documentPath)` individua il sidecar nella cartella del documento, rifiutando percorsi assoluti, separatori e risalite. Per trasferire un progetto vanno spostati documento e sidecar. Un rename esterno deve conservare il nome del sidecar citato o usare Save As per ribasare i riferimenti.

## Ciclo di vita

- `Prepare` si esegue una volta per la revisione, su un modello stabile durante la lettura. Può operare prima del primo salvataggio; i blocchi rimangono nella sessione.
- `Save` riceve le revisioni correnti e quelle storiche da mantenere. Il callback riceve riferimenti con il nuovo nome del sidecar; `BlockArchive.WriteFile` consente la scrittura atomica del documento. I wrapper GH effettivi vengono collegati nei piloti F4 e nella migrazione F6.
- Un autosave invariato legge/verifica metadati e manifest, ma non riscrive né ricomprime i payload. Non richiamare `Prepare` a ogni autosave.
- L'archivio viene sostituito prima del documento. I blocchi del documento precedente restano nel nuovo file fino al successivo salvataggio riuscito. Se il chiamante non conosce i vecchi riferimenti, il primo salvataggio conserva l'intero indice.
- Save As conserva anche i blocchi di una destinazione già esistente: se il callback fallisce, il suo vecchio documento resta leggibile. La sessione mantiene il percorso originario fino al successo.
- I blocchi rimossi restano in memoria per l'undo della sessione. La durata dello storico persistito è una scelta del chiamante. Per liberare anche i dati di undo si chiude/rinnova la sessione dopo aver deciso la politica applicativa.
- `IsCurrent(model)` confronta GUID e impronta degli input. La lettura di una revisione storica resta possibile; non equivale ad attestare la validità dei risultati per un modello modificato. `Prepare` rifiuta snapshot di analisi e mappe solutore incoerenti. In assenza di provenienza nel modello originale, l'archivio non la inventa.
- Mancanza/corruzione è esplicita per blocco. L'API GPC rifiuta il run incompleto e il salvataggio con blocchi richiesti assenti; ANTHEA conserva la propria gestione dei risultati parzialmente indisponibili.
- Le operazioni sul contenitore sono serializzate per sessione. Rimane un solo proprietario applicativo per documento aperto; non è un sistema di scrittura concorrente da più processi né un database distribuito. Atomicità verificata su filesystem locale Windows; callback documento obbligatoriamente atomico. Non viene promessa una transazione distribuita su cartelle sincronizzate o dischi remoti.

## Compatibilità e correzioni

Gli archivi GPC XML v1-v4 e ZIP v5 conservano i lettori esistenti. Il `ModelDocument` usa schema 3 solo quando sono presenti AssemblyMap o metadati di ponte F2; gli schemi 1/2 restano identici per i modelli precedenti. Questo corregge la perdita di AssemblyMap nel percorso ZIP/SaveDocument rilevata durante F3 e fa rifiutare i nuovi metadati ai lettori precedenti, invece di perderli. Nessuna modifica al dominio GPCModel 5.39.0.0.

L'estrazione irrigidisce i controlli di indice: overflow/limiti, indirizzi duplicati, intervalli sovrapposti, lunghezze e rapporto di espansione; non cambia la lettura degli archivi validi. Limite esplicito: 256 MiB per blocco e per indice, ereditato dal perimetro ANTHEA. Il grafo senza risultati è un singolo blocco: modelli con quel grafo oltre 256 MiB richiedono un'estensione a streaming/chunking, non sono dichiarati supportati da questa prima API.

## Prove riproducibili

`tests/ExternalArchiveTest.cs` è compilato nelle suite .NET 6 e .NET Framework. Copre: archivio ANTHEA originale e riscrittura identica, copie indipendenti, deduplicazione, codifiche raw/deflate, autosave, interruzione, undo, Save As sopra destinazione esistente, primo salvataggio senza riferimenti precedenti, file assenti/corrotti/futuri, indici invalidi, mappa F2 nei pacchetti GPC, run/mappa/dataset, riferimenti portabili, trasferimento, storico, rifiuto di percorsi non validi e recupero di un archivio GPC storico.

Fixture nuova `tests/Fixtures/anthea-blocks-v1-20261103.bin`: catturata prima della modifica con ANTHEA c4f68c5e, provenienza/hash nel JSON accanto. Nessuna fixture o baseline preesistente è stata sostituita.

`tools/ExternalArchiveProbe`: costruisce 1.000 nodi e 20 casi, 20.000 campioni e 120.000 componenti di spostamento/rotazione. Scrive riferimento, archivio, impronta degli input, SHA dei valori binari e impronta della mappa. Ogni archivio viene letto da un processo dell'altro runtime. Include -0, subnormal e frazioni non esatte.

Comandi (dal checkout Model):

```powershell
dotnet build tools/ExternalArchiveProbe -c Release
dotnet tools/ExternalArchiveProbe/bin/Release/net8.0/ExternalArchiveProbe.dll write <nuova-cartella-net8>
./tools/ExternalArchiveProbe/bin/Release/net48/ExternalArchiveProbe.exe read <cartella-net8>
./tools/ExternalArchiveProbe/bin/Release/net48/ExternalArchiveProbe.exe write <nuova-cartella-net48>
dotnet tools/ExternalArchiveProbe/bin/Release/net8.0/ExternalArchiveProbe.dll read <cartella-net48>
./build/Verify.ps1 -Results <nuova-cartella-test-model>
../Checker/build/Verify.ps1 -ModelRoot . -Results <nuova-cartella-test-checker>
```

I dati del probe sono sintetici e verificano persistenza e compatibilità; non sostituiscono i piloti nativi F4. Misure ed esiti finali nel [rapporto F3](F3-VERIFICHE.md). Nessun rilascio, merge o push.
