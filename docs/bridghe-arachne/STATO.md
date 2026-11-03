# Stato della migrazione comune

Aggiornamento del 3 novembre 2026. AttivitÃ  avviata su richiesta dell'utente; piano in [PIANO.md](PIANO.md).

## Stato effettivo

**F5 in corso su richiesta dell’utente; F6–F8 autorizzate e in coda.** Checkpoint verificato in [F5-AVANZAMENTO.md](F5-AVANZAMENTO.md), [matrice parziale](f5-destinazioni-parziali.json) ed [evidenze](f5-evidenze.json). Model 1.827 PASS; Checker 10.369 PASS; entrambi i nuovi writer nativi superati dopo il ripristino della licenza. Contratti e adapter pilota estesi; exporter completo e migrazione produttiva ancora da terminare. Nessun rilascio.

F0–F4 completate nei rispettivi perimetri. F4: [piloti e contratti](F4-PILOTI.md), [verifiche](F4-VERIFICHE.md), [evidenze](f4-evidenze.json). GPCModel 5.40.0.0 e GPC.Model.Persistence 3.6.0.0; GPC.Archive resta 1.0.0.0. Due varianti ponte e quattro casi membrana, con calcolo, sidecar e riapertura anche in processi distinti. Model 1.779 PASS e 12 ignorati preesistenti; Checker 10.361 PASS. Le build predefinite dei plugin sono state ripristinate e verificate. Nessuna migrazione estesa dei componenti o release: prossima fase F5, poi F6–F8.

F1: [baseline](F1-BASELINE.md), [evidenze](f1-evidenze.json). F2: [contratti](F2-CONTRATTI.md), [verifiche](F2-VERIFICHE.md). F3: [persistenza](F3-PERSISTENZA.md), [verifiche](F3-VERIFICHE.md). L’adattatore ANTHEA rimane opt-in; adozione predefinita e snapshot restano F7/F8.

Target copia F2: mediane 0,125 s su .NET 8 e 0,289 s su .NET Framework nel protocollo F1. Non Ã¨ un limite massimo per ogni chiamata: su Framework la prima copia Ã¨ 0,360 s e il massimo a caldo 0,304 s. Model: 1.737 test superati, 12 ignorati preesistenti. Checker: 10.361 superati. API e audit superati, fixture e attesi invariati.

Le sessioni Codex visibili pertinenti erano inattive; la conclusione della precedente sessione Claude non Ã¨ attestabile da questi strumenti. La nuova autorizzazione della chat consente la preparazione e le verifiche nei worktree separati. Non assumere che una versione compilata equivalga a una release coordinata completata.

## Branch e provenienza

Branch `codex/gpc-unification-20261102` in sei repository indipendenti:

| Repository | Commit iniziale | Note |
|---|---|---|
| Model | `2f934f9fe48ebfff0b06a98de5ccff7bdea43ea4` | GPCModel 5.38.0.0; ultima origin/master dopo fetch |
| Checker | `6156e4ce00975b0774d172ea681e24377bfb3c9f` | Ricompilazione per GPCModel 5.38.0.0 |
| BridGHe | `e7f650df3f8f052f80e930175898e72d1480b081` | Baseline con set pubblicato GPCModel 5.37.0.2 |
| Arachne | `7b1108dbe083a55af41dc5d2f2f710d9cfcab3ef` | Baseline net48, 0.3.0-rc.7 |
| tensilelib | `5e2576534f62ecca17d120430e3c29110a5e3de2` | Sorgenti separati; baseline usa DLL 2.5.0.1 catturata |
| ANTHEA | `c4f68c5e69a88b4b0a13891fe2bd02db6afe7537` | Baseline selettiva con il suo set di librerie fissate; sorgente della persistenza F3 |

Geometry e Utilities sono worktree detached su commit fissati. Le DLL corrispondono agli SHA-256 richiesti da Model. I branch di lavoro non hanno upstream sul master remoto. Non Ã¨ stato eseguito alcun merge o push.

Il manifest [baseline-manifest.json](baseline-manifest.json) registra sette revisioni, dodici input e sei dipendenze. Il file `workspace.json` nella radice del workspace collega piano, stato, manifest e artefatti.

Il GHX REV003 modificato nel checkout principale di BridGHe Ã¨ stato lasciato intatto. La baseline catturata proviene dal file versionato del worktree (4.904.830 byte), non dal file dell'utente contenente risultati salvati.

Durante i controlli finali il master principale di BridGHe Ã¨ risultato avanzato, per attivitÃ  esterna a questa chat, al commit locale `be9bb0b` (messaggio `minor`, 2 novembre 22:11). La sola differenza rispetto a `e7f650d` Ã¨ il GHX REV003, cresciuto da 4.904.830 a 886.961.214 byte; il master risulta un commit avanti al remoto. Il branch isolato resta fissato alla versione pubblicata `e7f650d`. Prima delle catture native distinguere eventuali differenze degli input da quelle dovute ai risultati memorizzati; non incorporare automaticamente il file grande nÃ© modificare il commit dell'utente.

## Verifiche effettivamente eseguite

| Verifica | Esito |
|---|---|
| Build BridGHe Release e Smoke | PASS, zero errori e zero avvisi |
| Audit contratti BridGHe | PASS, 152 file, 229 GUID, zero anomalie |
| BridGHe Smoke `--core-only` | 269 PASS |
| Build Arachne e ARAP | PASS; 54 test ARAP, zero fallimenti |
| Arachne headless | 153 PASS: inventario 66, form-finding 38, patterning 43, reti 6 |
| Model.Core.Tests | 63 PASS |
| Model UnitTest | 1.628 PASS, 12 ignorati, zero fallimenti |
| Model.NetFramework.Tests | 4 PASS |
| Model API e audit uguaglianza | PASS; `build/Verify.ps1` completato con exit code 0 |

Dettagli, inclusi i test ignorati, in [verifiche-iniziali.json](verifiche-iniziali.json). I test ignorati non sono considerati superati nÃ© copertura delle analisi native.

Log e TRX: `../../../artifacts/f1` rispetto a questa cartella. Principali file: `bridghe-build.log`, `bridghe-smoke-build.log`, `bridghe-contracts.json`, `bridghe-core.log`, `arachne-build-arap-verified.log`, `arachne-headless.log`, `model-verify-complete.log`, cartella `model-tests-complete`.

## Problemi dell'ambiente risolti

- Il primo build record di Arachne non conteneva il commit a causa della diversa proprietÃ  Windows dei worktree. Risolto con `safe.directory` limitato al processo; build e test ripetuti, commit del record verificato. Non modificata la configurazione Git globale.
- La prima esecuzione di Model Verify si Ã¨ fermata alla compilazione UnitTest per il wrapper GMsh mancante nel checkout fratello. Aggiunta una copia della sola DLL managed, con hash verificato e destinazione riservata ai test; la suite completa successiva Ã¨ riuscita. Gmsh non Ã¨ stato aggiunto ai riferimenti di produzione o al set rilasciabile.

## Gate F4 e seguito

Baseline conclusa: vedere il rapporto F1 per copertura, misure, esclusioni e tentativi scartati. I dati iniziali sopra restano il resoconto storico della preparazione.

F3 conclusa nello strato di persistenza: 1.765 test Model PASS, 12 ignorati preesistenti; 10.361 Checker PASS; ANTHEA prima/dopo e configurazione predefinita PASS per le due suite selezionate. Recupero incrociato .NET 8/Framework con valori binari identici; riferimento 457 byte, autosave invariato circa 14-15 ms di mediana nel probe (20.000 campioni).

F4 avviata e conclusa il 3 novembre 2026 su richiesta esplicita dell’utente. Il perimetro transitorio e le esclusioni sono dichiarati nel rapporto F4: exporter Straus7 ancora nel backend BridGHe, preprocessore Arachne riutilizzato all’ingresso, componenti di produzione da migrare in F6. F5 è stata successivamente avviata; vedere il checkpoint sopra.

Checkpoint F1: Model `4134bd1a`, BridGHe `2b2c4b3`, Arachne `63b9516`. Le modifiche F2 appartengono al solo repository Model; Checker Ã¨ stato ricompilato e verificato senza modifiche ai suoi sorgenti. Nessuna integrazione nei checkout principali.

ANTHEA aggiunta al workspace al commit c4f68c5e, sullo stesso branch separato; ModelWorkspace.Checks e Application.Tests superati con le sue librerie fissate. ReBear è stato successivamente individuato in `Repository/ConcreteSectionChecker`; il checkout principale non è stato modificato.

Checkpoint F2: Model `7e08ddc8`. F3 comprende Model (contenitore/API e test), ANTHEA (adattatore opt-in e registro differenze), Checker (inclusione della nuova DLL nel futuro set di rilascio). Tutte le modifiche restano nei worktree isolati; nessun merge o push.
