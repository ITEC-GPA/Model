# Stato della migrazione comune

Aggiornamento del 2 novembre 2026. Attività avviata su richiesta dell'utente; piano in [PIANO.md](PIANO.md).

## Stato effettivo

Preparazione tecnica F0 completata. F1 avviata e parzialmente verificata. F2-F8 non avviate. Nessuna modifica al dominio, ai solutori o ai risultati numerici è stata introdotta.

Le sessioni Codex visibili pertinenti erano inattive; la conclusione della precedente sessione Claude non è attestabile da questi strumenti. La nuova autorizzazione della chat consente la preparazione e le verifiche nei worktree separati. Non assumere che una versione compilata equivalga a una release coordinata completata.

## Branch e provenienza

Branch `codex/gpc-unification-20261102` in cinque repository indipendenti:

| Repository | Commit iniziale | Note |
|---|---|---|
| Model | `2f934f9fe48ebfff0b06a98de5ccff7bdea43ea4` | GPCModel 5.38.0.0; ultima origin/master dopo fetch |
| Checker | `6156e4ce00975b0774d172ea681e24377bfb3c9f` | Ricompilazione per GPCModel 5.38.0.0 |
| BridGHe | `e7f650df3f8f052f80e930175898e72d1480b081` | Baseline con set pubblicato GPCModel 5.37.0.2 |
| Arachne | `7b1108dbe083a55af41dc5d2f2f710d9cfcab3ef` | Baseline net48, 0.3.0-rc.7 |
| tensilelib | `5e2576534f62ecca17d120430e3c29110a5e3de2` | Sorgenti separati; baseline usa DLL 2.5.0.1 catturata |

Geometry e Utilities sono worktree detached su commit fissati. Le DLL corrispondono agli SHA-256 richiesti da Model. I branch di lavoro non hanno upstream sul master remoto. Non è stato eseguito alcun merge o push.

Il manifest [baseline-manifest.json](baseline-manifest.json) registra sette revisioni, dodici input e sei dipendenze. Il file `workspace.json` nella radice del workspace collega piano, stato, manifest e artefatti.

Il GHX REV003 modificato nel checkout principale di BridGHe è stato lasciato intatto. La baseline catturata proviene dal file versionato del worktree (4.904.830 byte), non dal file dell'utente contenente risultati salvati.

Durante i controlli finali il master principale di BridGHe è risultato avanzato, per attività esterna a questa chat, al commit locale `be9bb0b` (messaggio `minor`, 2 novembre 22:11). La sola differenza rispetto a `e7f650d` è il GHX REV003, cresciuto da 4.904.830 a 886.961.214 byte; il master risulta un commit avanti al remoto. Il branch isolato resta fissato alla versione pubblicata `e7f650d`. Prima delle catture native distinguere eventuali differenze degli input da quelle dovute ai risultati memorizzati; non incorporare automaticamente il file grande né modificare il commit dell'utente.

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

Dettagli, inclusi i test ignorati, in [verifiche-iniziali.json](verifiche-iniziali.json). I test ignorati non sono considerati superati né copertura delle analisi native.

Log e TRX: `../../../artifacts/f1` rispetto a questa cartella. Principali file: `bridghe-build.log`, `bridghe-smoke-build.log`, `bridghe-contracts.json`, `bridghe-core.log`, `arachne-build-arap-verified.log`, `arachne-headless.log`, `model-verify-complete.log`, cartella `model-tests-complete`.

## Problemi dell'ambiente risolti

- Il primo build record di Arachne non conteneva il commit a causa della diversa proprietà Windows dei worktree. Risolto con `safe.directory` limitato al processo; build e test ripetuti, commit del record verificato. Non modificata la configurazione Git globale.
- La prima esecuzione di Model Verify si è fermata alla compilazione UnitTest per il wrapper GMsh mancante nel checkout fratello. Aggiunta una copia della sola DLL managed, con hash verificato e destinazione riservata ai test; la suite completa successiva è riuscita. Gmsh non è stato aggiunto ai riferimenti di produzione o al set rilasciabile.

## Attività necessarie per chiudere F1

1. Definire la copertura e implementare il dump degli input effettivamente scritti in Straus7; un fingerprint di Checker non lo sostituisce.
2. Catturare nuovamente 50/99 e regressioni native nell'ambiente isolato, distinguendo input esportati, risultati FEM e risultati delle verifiche.
3. Misurare i quad non complanari dei ponti e conservarne ID, deviazione e scala geometrica.
4. Catturare input/output di TensileLib per i casi rappresentativi di Arachne, comprese convergenza, unità e obiettivi.
5. Eseguire le misure a più dimensioni nei runtime effettivi: costruzione, copie, assemblaggio, revisioni, salvataggio/riapertura e memoria.
6. Completare la baseline pertinente di Checker e dei consumatori. Le suite ANTHEA/ReBear non sono state rieseguite in questa attività.

La migrazione dei tipi non può essere dichiarata pronta sulla sola base di questi controlli iniziali. Le decisioni sulle geometrie, sulle copie e sull'archivio condiviso appartengono a F2-F4, dopo la cattura dei riferimenti.
