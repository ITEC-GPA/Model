# F1 - Baseline congelata

Baseline del 2 novembre 2026, nei worktree `codex/gpc-unification-20261102`. I riferimenti iniziali sono in `baseline-manifest.json`; misure ed esiti conclusivi in `f1-evidenze.json`. Nessun algoritmo di produzione e nessun atteso sono stati sostituiti. F4 e' in coda dopo F3.

## Copertura funzionale e numerica

- Model: 63 Core, 1.628 UnitTest e 4 NetFramework superati; 12 test ignorati, gia' elencati in `verifiche-iniziali.json`. Audit API e uguaglianza superati.
- Checker: 10.361 test superati nelle 11 esecuzioni del runner completo, zero fallimenti ammessi o nuovi; audit API superati.
- BridGHe: audit di 152 contratti e 229 GUID; 378 controlli core/nativi; 83 gruppi di regressione e altri 3 in host separati; 10 esempi nativi analitici e benchmark dei carichi mobili superati.
- Esempio 50 REV003: 5 analisi, 31 posizioni mobili, 36 combinazioni, 32.832 verifiche di sezione, 3 dettagli e 6 grafici. L'impronta delle 66.960 righe Checker e' identica nelle corse ordinaria e strumentata: `C1CF95A069DF8E9D7F44FD1F54A3CB045CE0B9DDC82B62A0F1E3C7BFE3378E02`.
- Esempio 99 REV004: 4 analisi, 6 combinazioni, 1.224 verifiche di sezione, 6 dettagli e 10 grafici. Impronta di 2.736 righe: `8E9756B76EC3EFFE68D1175CFAA666139358DC794689E007CE9522D03075C330`. Controllati equilibrio verticale, assenza di errori del solutore e integrita' dei GHX.
- Arachne: 54 ARAP e 153 headless superati. Acquisite 40 coppie input/output TensileLib in 24 casi: lineare, iterativo, non lineare, obiettivi impossibili, pressione, peso proprio, unita', vincoli orientati, reti disconnesse e conservazione degli input.
- ANTHEA, commit `c4f68c5e69a88b4b0a13891fe2bd02db6afe7537`: `ModelWorkspace.Checks` e `Application.Tests` superati con il runner ufficiale e lo snapshot di librerie proprio del consumatore. Sono la baseline pertinente a modello, documenti e archivio esterno, non la certificazione dell'intera applicazione dopo la futura migrazione.

## Input Straus7 e quad

`BridGHe/tools/capture_straus_api.py` genera una variante diagnostica delle dichiarazioni native, conservando firme e marshalling. Si abilita soltanto con `-p:BridgheTraceApi=true`; `BRIDGHE_STRAUS_TRACE` indica un nuovo file `.jsonl.gz`. Registra gli argomenti prima della chiamata, il codice restituito e gli output scalari `ref`. I double sono rappresentati con 16 cifre esadecimali IEEE-754, inclusi zero con segno e NaN.

La copertura dichiarata comprende 105 funzioni di modifica, anche quelle non percorse dagli esempi; le 38 funzioni di lettura/controllo escluse sono elencate nel manifest. Le corse 50 e 99 contengono rispettivamente 19.869 e 3.157 chiamate, tutte accettate. `summarize_straus_capture.py` controlla la sequenza, ricostruisce coordinate/connettivita', verifica i riferimenti ai nodi e genera impronte degli input per modello. Gli archivi `.bgmodel` conservano separatamente modello prima e dopo l'analisi; sono conservati anche file nativi e risultati Straus7.

Questo e' un dump degli input effettivamente inviati e accettati dall'API, **non** la rilettura indipendente di ogni attributo dal file `.st7`. I controlli nativi analitici e di equilibrio completano la prova senza sostituire tale distinzione. Il transcript mantiene ordine delle chiamate e ID nativi; una diversa numerazione richiedera' il confronto con la mappa esplicita di F2.

In tutte le fasi di 50 e 99 gli elementi esportati sono beam: **zero plate**, quindi nessun quad da misurare. L'assenza di non complanarita' in questi esempi non prova la compatibilita' dei gusci. Il tool misura comunque, per eventuali quad, la distanza dal piano del triangolo di area massima e il rapporto rispetto al lato maggiore. F2 richiede casi sintetici dedicati a quad deformati, degeneri e ripiegati.

## Input/output TensileLib

`Arachne/tools/Diagnostics/MigrationTensileCapture.cs`, incluso solo con `-p:ArachneMigrationCapture=true`, scrive in `ARACHNE_MIGRATION_CAPTURE`. Il grafo catturato e' quello assemblato immediatamente prima del solutore e quello restituito prima della conversione ai risultati GH. Contiene campi, riferimenti condivisi, coordinate, vincoli, pesi, obiettivi, parametri del solutore, unita', stato di convergenza, forze assiali e reazioni. I floating point sono salvati come bit; i getter del grafo non vengono invocati dalla visita riflessiva. Il nome del test associa ogni coppia al relativo controllo analitico.

Le DLL TensileLib/alglib usate sono quelle congelate nel manifest iniziale: non si attribuisce questa cattura a una ricompilazione dei sorgenti TensileLib.

## Protocollo prestazionale

Tre griglie triangolari: 100/162, 900/1.682 e 10.000/19.602 nodi/gusci, passo 1.000 mm, spessore 100 mm, senza risultati. Stesso numero di entita', ma rappresentazioni interne diverse: GPC mantiene connettivita' esplicita; i plugin mantengono anche geometria Rhino. Il confronto misura i percorsi correnti, non dimostra da solo equivalenza semantica.

`Model/tools/MigrationScale` usa lo stesso sorgente e le stesse DLL GPC in net8.0 e net48, x64, Release, workstation GC. Piccolo warm-up, prima copia distinta, cinque copie a caldo; GC completo prima di ogni timer. Controlli fuori dal timer: distacco degli oggetti, conservazione dei collegamenti interni, isolamento della mutazione e impronta di analisi dopo salvataggio/lettura. Allocazioni del thread, variazione heap e picco working set sono grandezze separate. Save/load operano in memoria e non misurano la latenza del disco.

I runner dei plugin espongono `--migration-scale`, con un nuovo file `MIGRATION_SCALE_OUTPUT`. Misurano costruzione/assemblaggio, costruttore di copia effettivamente usato da GH, archivio e SHA-256 dei byte. L'hash dell'archivio non equivale a un fingerprint semantico. Arachne usa il proprio percorso di deserializzazione che ricostruisce gli indici.

Mediane delle cinque copie e singole misure di archivio sul caso maggiore:

| Percorso | Copia [s] | Save [s] | Load [s] |
|---|---:|---:|---:|
| GPC .NET 8 | 0,449 | 4,399 | 2,477 |
| GPC .NET Framework 4.8 | 0,859 | 6,824 | 3,266 |
| BridGHe attuale | 5,353 | 1,262 | 3,749 |
| Arachne attuale | 1,023 | 5,393 | 12,095 |

GPC alloca circa 508-536 MB per copia del caso maggiore. La dimensione degli archivi dipende dal codec, quindi non e' una misura diretta della memoria del dominio. Questi risultati non raggiungono il target F2 di 0,3 s; il requisito resta invariato.

Nei due CSV GPC iniziali la colonna `retainedHeapDeltaBytes` e' il delta di `GC.GetTotalMemory(false)`, quindi puo' includere garbage non ancora raccolta: non va interpretata come memoria trattenuta. Il runner successivo usa il nome corretto `heapDeltaBytes`; le catture originali restano immutate.

Le catene GH Arachne reali (costruzione, force density, assemblaggio e form-finding) sono misurate con ricomputazione forzata a 25, 169 e 1.089 nodi. Sul caso maggiore le tre corse richiedono 0,692/0,728/0,741 s. Per BridGHe le corse native registrano i confini temporali dell'intero grafo, inclusi lettura Excel, analisi, merge e Checker; la corsa ordinaria di 50 raggiunge gli input in 27,446 s e conclude verifiche/dettagli/grafici a 178,088 s. Non confrontare questi tempi con il solo costruttore di copia.

## Limiti, tentativi scartati e riproducibilita'

- Una corsa Arachne simultanea al processo BridGHe ha fallito per `Rhino.Runtime.NotLicensedException`; la ripetizione sequenziale ha superato tutti i test. I runtime Rhino vanno eseguiti in sequenza su questa macchina.
- La prima misura del caricamento Arachne usava il deserializzatore generico senza ricostruzione degli indici. Il runner e' stato corretto per chiamare il percorso reale di `GH_Model.Read` e la misura completa e' in `arachne-scale-complete.csv`; il tentativo precedente non e' un difetto del prodotto.
- Il primo avvio del runner ANTHEA senza lo stage `build` non aveva gli eseguibili nel nuovo worktree. La corsa `gpc-f1-complete`, con build e fast, e' quella valida.
- ReBear non e' individuabile come repository/progetto tra i checkout disponibili. Nessuna sua verifica viene dichiarata eseguita; la copertura completa dei consumatori resta nel gate d'integrazione F7.
- La baseline non comprende prove WPF complete di ANTHEA, ne' prestazioni di modelli con molti dataset di risultati. Queste ultime appartengono anche al gate di persistenza F3 e devono usare il codec definitivo.
- Log, CSV, transcript, archivi e TRX restano in `artifacts/f1` del workspace; il manifest delle evidenze ne conserva percorsi e SHA-256. Le nuove catture richiedono destinazioni nuove: i runner diagnostici rifiutano di sovrascriverle.

F1 chiusa per il perimetro dichiarato. F2 deve ora risolvere prestazioni/ownership, assemblaggio e identita', geometrie non piane e contratti di unita' prima di procedere con F3 e F4.
