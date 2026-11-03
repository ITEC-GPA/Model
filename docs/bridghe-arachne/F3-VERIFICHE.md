# F3 - Esito delle verifiche

3 novembre 2026. Implementazione e prove concluse sul branch separato; F4 è la prossima fase in coda. [Contratto e limiti](F3-PERSISTENZA.md), [manifest congelato](f3-evidenze.json).

| Verifica finale | Esito effettivo |
|---|---|
| Model.Core.Tests | 63 PASS |
| UnitTest (.NET 6) | 1.663 PASS, 12 ignorati preesistenti |
| Model.NetFramework.Tests | 39 PASS |
| Totale Model | 1.765 PASS, zero fallimenti |
| API Model/Persistence e audit uguaglianza | PASS, zero rimozioni inattese |
| Checker completo | 10.361 PASS, 11 esecuzioni, zero fallimenti |
| API Checker/Concrete/Design/Masonry | PASS |
| ANTHEA prima della modifica | ModelWorkspace.Checks e Application.Tests PASS |
| ANTHEA con contenitore condiviso, finale | Stesse due suite PASS |
| ANTHEA configurazione predefinita, finale | Stesse due suite PASS, snapshot invariato |
| Archivio ANTHEA storico catturato prima di F3 | Ogni blocco letto; riscrittura identica byte per byte su entrambi i runtime |
| Probe .NET 8 → Framework e Framework → .NET 8 | PASS in processi distinti: SHA dei valori binari, impronta input e mappa del solutore identici |

I 14 test F3 nuovi sono eseguiti in entrambe le suite .NET e Framework (28 esecuzioni). La fixture di compatibilità è nuova; quelle precedenti e le baseline API sono invariate. Il test del percorso ZIP/SaveDocument riproduce e chiude la perdita di AssemblyMap trovata dopo F2.

Le suite complete producono `artifacts/f3/model-tests-final`, `model-verify-final.log`, `checker-tests-final/summary.json`, `checker-verify-final.log`. La corsa ANTHEA finale condivisa è `20261103-005533-gpc-f3-shared-final`, quella predefinita `20261103-005637-gpc-f3-default-final`; entrambe `-Keep -NoPrune`, sotto `ANTHEA/supporto/artefatti/ci`.

## Misura della persistenza

Probe sintetico: 1.000 nodi, 20 casi, 20.000 campioni, 120.000 componenti. Una cattura distinta per runtime; ogni cattura è poi letta dall'altro runtime. Si confrontano i contenuti di ciascun archivio con i suoi digest originali, non si pretende che due modelli generati separatamente abbiano lo stesso file.

| Runtime scrittore | Prepare [s] | Primo Save [s] | Mediana 5 autosave invariati [s] | Riapertura locale [s] | Archivio [byte] |
|---|---:|---:|---:|---:|---:|
| net8 | 0.2099102 | 0.045326 | 0.014707 | 0.8292333 | 4341528 |
| net48 | 0.1834315 | 0.0429796 | 0.013895 | 0.81283149999999993 | 4542178 |

Il riferimento nel documento misura **457 byte** in entrambe le catture. I cinque autosave conservano SHA e timestamp del sidecar. Si tratta di misure della singola macchina nel protocollo dichiarato; non di un limite garantito, né delle prestazioni dei componenti GH ancora da migrare.

## Evidenze e limiti

`f3-evidenze.json` congela hash e dimensioni di 91 file, incluse DLL, sorgenti, fixture e risultati; gli esiti prima/dopo di ANTHEA hanno hash separati nel manifest. I tentativi intermedi rimangono separati: primo test mirato con un confronto insufficiente dell'identità Model, poi sostituito nel nuovo test con confronto di input e byte effettivi di `results.bin`; nessun atteso storico cambiato. Il build iniziale del probe aveva due errori di uso dell'API, corretti prima delle catture finali.

Le DLL GPC.Archive prodotte dal contesto ANTHEA e dal probe hanno MVID/SHA diversi, pur con la stessa versione, stessa revisione dichiarata e IL coincidente per i 50 metodi analizzati. Sono entrambe registrate e provate, senza presentarle come un unico set pubblicabile. Riscontro supplementare: `artifacts/f3/archive-build-identity.json`, SHA-256 `17DD9C4569B03CF04A7B37F9D1358D61D4A1173EBBCC1CF11229B53709A90E87`. La causa esatta della differenza dei metadati di build non è stata attestata; la build unica e il controllo del set coerente restano obbligatori in F8.

Non eseguiti in F3: piloti GH/solutori nativi F4, migrazione completa dei plugin, profilo full WPF ANTHEA, nuovo installer, rilascio. Lo snapshot ANTHEA non è stato modificato. GPC.Archive supporta blocchi/indice fino a 256 MiB; il grafo del modello è ancora un singolo blocco. F3 fornisce la libreria e l'adattatore ANTHEA verificato in configurazione esplicita, non l'adozione ordinaria di tutti i consumatori.

F4: piccolo ponte e membrana, confronto con F1, rami GH indipendenti, archivio/riapertura, inclusa la scelta del percorso temporaneo per autosave di documenti GH non ancora salvati. Senza una scrittura esplicita del sidecar, la sessione non salvata conserva i dati soltanto in memoria.
