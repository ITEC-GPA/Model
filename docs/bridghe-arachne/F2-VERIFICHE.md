# F2 - Verifiche del checkpoint

3 novembre 2026. GPCModel 5.39.0.0, branch `codex/gpc-unification-20261102`. Contratti in [F2-CONTRATTI.md](F2-CONTRATTI.md); risultati, misure complete e SHA-256 in [f2-evidenze.json](f2-evidenze.json).

## Esito funzionale

| Verifica eseguita | Risultato |
|---|---:|
| Model.Core.Tests | 63 superati |
| Model UnitTest | 1.649 superati; 12 ignorati |
| Model.NetFramework.Tests, runtime 4.8 | 25 superati |
| Totale Model | 1.737 superati; zero fallimenti |
| Checker, tutte le 11 esecuzioni | 10.361 superati; zero fallimenti |
| Compatibilità API Model/Persistence/Checker/Design/Masonry | Superata; zero rimozioni nuove inattese |
| Audit uguaglianza Model | Superato |

I 21 nuovi test condivisi sono eseguiti sia dal runner UnitTest sia su .NET Framework. Le fixture degli archivi storici, gli attesi e le baseline API esistenti sono rimasti invariati. I 12 test ignorati sono gli stessi della baseline: non vengono contati come superati. Il build Checker riporta avvisi nullable nei test già presenti; nessun errore di compilazione.

Comandi validi conclusivi:

```powershell
# Dal worktree Model
./build/Verify.ps1 -Results ../artifacts/f2/model-tests-final
# Dal worktree Checker
./build/Verify.ps1 -ModelRoot ../Model -Results ../artifacts/f2/checker-tests-final
```

Il runner Checker ricrea la cartella dei risultati: in questa esecuzione è stata verificata come nuova e contenuta nel workspace isolato. I log conclusivi sono `artifacts/f2/model-verify-final.log` e `checker-verify-final.log`. Tutti i tentativi intermedi restano conservati e distinti: comprendono errori di compilazione durante lo sviluppo, affinamento del confronto XML, correzione del namespace persistito e del confronto di identità rilevato dall'audit. Non vengono presentati come esecuzioni verdi.

## Prestazioni

Stessa macchina e protocollo F1: griglie 100/162, 900/1.682, 10.000/19.602 nodi/gusci, stesso sorgente del benchmark, x64 Release, workstation GC, warm-up piccolo, prima copia separata, cinque copie a caldo, GC fuori dai timer. Le due versioni del runner caricano la medesima GPCModel.dll, verificata per SHA-256. Nessuna suite è stata avviata in parallelo alla misura finale.

Caso maggiore:

| Misura | .NET 8.0.33 | .NET Framework 4.8.10008 |
|---|---:|---:|
| Mediana copia F1 [s] | 0,448615 | 0,859215 |
| Mediana copia F2 [s] | **0,124534** | **0,288961** |
| Prima copia F2 [s] | 0,206397 | 0,360284 |
| Intervallo cinque copie a caldo [s] | 0,119827-0,193644 | 0,284198-0,304166 |
| Allocazioni mediane del thread per copia [byte] | 68.679.360 | 78.657.904 |
| Fingerprint analisi [s] | 0,1871 | 0,3984 |
| Salvataggio in memoria [s] | 4,5442 | 6,7420 |
| Riapertura in memoria [s] | 2,6305 | 3,3649 |

Il target **0,3 s è raggiunto sulla mediana delle cinque copie a caldo, la statistica usata nel confronto F1**. Non è un limite massimo garantito per ogni chiamata: su .NET Framework la prima copia richiede 0,360 s e una ripetizione a caldo 0,304 s. Il criterio, le singole misure e queste eccezioni sono espliciti; non si dichiara raggiunto un requisito di latenza massima o a freddo di 0,3 s. Se il requisito operativo riguarda quel limite più severo, occorre un ulteriore gate prestazionale prima della migrazione dei componenti.

Tutte le griglie hanno superato i controlli di distacco, alias di connettività, mutazione isolata e impronta dopo salvataggio/lettura. L'archivio usa ancora il codec precedente; i suoi tempi non sono una misura di F3. Il benchmark costruisce la griglia con i registri GPC come in F1: non misura l'intera nuova procedura `ModelAssembly` né la futura catena GH migrata. Queste misure integrative appartengono ai piloti.

## Stato di uscita

F2 completata per le fondazioni descritte e il criterio prestazionale statistico F1. Sono disponibili copia autonoma, assemblaggio deterministico con conflitti espliciti, mappe di identità, metadati di ponte, percorsi non rettilinei, geometria dei Q4 non planari e conversioni dimensionali comuni.

La matrice completa di routing individua anche i contratti ancora da implementare in F4/F5. Non sono stati convertiti i componenti di produzione, né gli exporter. Le nuove geometrie non attestano da sole l'ammissibilità di una formulazione shell nativa. Le baseline native 50/99 e TensileLib restano quelle congelate in F1.

F3 e F4 restano in coda, in quest'ordine. Per F3 sono stati individuati i sorgenti ANTHEA `ModelWorkspace/ModelResultArchive.cs` e `Application/Documents/ResultArchiveDocumentPersistence.cs`; l'estrazione del codec e la gestione condivisa dei sidecar non sono ancora implementate. Nessun merge, push o rilascio del set coordinato è stato eseguito.
