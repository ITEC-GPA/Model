# Stage — inventario e contratto di importazione

STG-1, 12 ottobre 2026. Lavoro richiesto dopo le chiusure F2 e prima di F3; prima cattura ANTHEA conservata al commit 71aaa60. Il rilascio T rimane congelato e collaudabile mentre si prepara il ramo stage.

## Riuso dei contratti esistenti

- `Stage` e `Model.Stages` esistono: al momento conservano nome, ID, IsCumulative e SourceStep. La grande implementazione commentata non è attiva.
- `ResultState` contiene già Phase, Step, IsCumulative, HistoryId e IncrementIndex. ResultMapper distingue la chiave caso/fase/step/posizione; ResultSelection permette la stessa selezione. Non serve duplicare il motore delle combinazioni o creare casi diversi per ogni step.
- `CivilNxTableRequest.StageSteps` scrive già OPT_CS e STAGE_STEP. CivilNxResults rifiuta invece le risposte con Stage/Step. Estendere quel lettore mantiene unità, assi, convenzioni e diagnosi in un solo posto.
- Straus7 legge il numero di stage e lo stage del caso di risultato, ma ne rifiuta l'importazione: resta capacità esplicita fino a collaudo del lettore nativo.
- La vista ANTHEA ha snapshot indipendenti dal dominio: aggiungere metadati opzionali senza cambiare le impronte degli archivi statici precedenti. Le combinazioni statiche non devono assorbire risultati staged per una coincidenza del nome.

## Schema sorgente e decisioni

STAG descrive durata, salvataggio degli output, tempi aggiuntivi e attivazioni/disattivazioni per gruppi strutturali, vincoli e carichi. L'età degli elementi all'attivazione è distinta dal tempo della fase. STCT separa analisi indipendente/accumulata, non linearità, effetti differiti e casi di erezione. Conservare i dati non ancora interpretati, senza trasformarli in assegnazioni permanenti. Fonti: [STAG](https://support.midasuser.com/hc/en-us/articles/35987578396697-Define-Construction-Stage), [STCT](https://support.midasuser.com/hc/en-us/articles/35990281053465-Construction-Stage-Analysis-Control-Data).

La richiesta di risultati deve specificare OPT_CS, fase/step e casi con suffisso CS. La risposta ha colonne Load, Stage e Step; gli spostamenti distinguono Accumulative, Current e Real. Questi riferimenti vanno conservati e non ricostruiti per deduzione dai numeri. L'analisi accumulata non implica che ogni tabella sia un incremento sommabile. Fonti: [richieste delle tabelle](https://support.midasuser.com/hc/ko/articles/45171987915929-Designing-with-Intent-The-Vision-Behind-POST-TABLE), [spostamenti](https://support.midasuser.com/hc/en-us/articles/36009638400281-Displacements-Analysis-Result-Table).

Scelte implementative: metadati di fase nel dominio Stage; identità dei risultati in ResultState; nome del caso sorgente conservato con ambito CS esplicito, separato da un eventuale ST omonimo. La combinazione automatica degli effetti differiti/non lineari non è abilitata da questa estensione. Nessuna conversione da risultato cumulato a incremento senza richiesta e storia completa.

## Piano delle prove

1. Contratto stage: lettura di archivi precedenti, copia e fingerprint; durata/ordine/step/attivazioni, dati non validi rifiutati.
2. Lettore: due fasi e più step, nome del caso riutilizzato, N/mm e kN/m, segni beam/plate, spostamenti e rotazioni, dati incompleti/duplicati, cancellazione e provenienza. Fixture nostre con valori indipendenti, marcate sintetiche.
3. ANTHEA: importazione, filtro fase/step, persistenza, aggiornamento e rifiuto del collegamento; casi statici invariati. Capacità degli altri software dichiarate.
4. Confronto reale: il 12 ottobre alle 23:01 il servizio MIDAS risponde, ma STAG e STCT restituiscono database vuoti nel modello aperto. Il campione non può validare risultati per fase. Evidenza ANTHEA `supporto/artefatti/refactoring/stages/study-20261012`. Il confronto reale resta distinto dalle prove sintetiche.

## Stato

- [x] STG-1: inventario, documentazione del protocollo e controllo del campione disponibile.
- [x] STG-2: metadati comuni e lettore dei risultati, collaudati su risposte sintetiche; il confronto reale resta STG-4.
- [ ] STG-3: interfaccia e capacità degli adattatori.
- [ ] STG-4: collaudo integrato; confronto reale quando disponibile un modello con stage.

### STG-2a — contratto del dominio (completato)

Model 5.2.0.0: estensione funzionale di Stage con StageDefinition e StageGroupChange immutabili; età degli elementi distinta dal giorno della fase, redistribuzione normalizzata come rapporto, posizione di attivazione dei vincoli esplicita. StageResultCase distingue un caso di output CS da uno statico omonimo, senza creare un caso per ogni step. ResultState conserva opzionalmente il riferimento degli spostamenti della sorgente; Phase/Step/HistoryId esistenti sono riusati.

Prima: archivio `tests/Fixtures/stage-legacy.xml` catturato con Model 5.1.0.0 rilasciato (94efc9bc), evidenza esterna ANTHEA `supporto/artefatti/refactoring/stages/baseline-20261012`. Dopo: stesse impronte dei due stage e del modello, nessun cambiamento agli archivi statici; dati nuovi entrano nell'impronta e le versioni future vengono rifiutate. Differenza prevista: nuovi metadati quando presenti; nessuna assegnazione permanente modificata. Test mirati: 28 PASS, 0 FAIL (`TestResults/Stages-20261012/stage-domain.trx`). Le versioni dei binari dipendenti saranno aggiornate nel set coerente della fase.

### STG-2b — lettura MIDAS (completato su protocollo sintetico)

CivilNxModelProfile riusa il parser e il mapper esistenti per importare STAG; conserva STCT, BNGR e LDGR. Le operazioni sui gruppi sono metadati consultabili, senza simulare l'analisi né applicare vincoli di fase come permanenti. I nomi delle fasi sono sempre conservati; una definizione senza durata resta non interpretata e segnalata. Il catalogo CS contiene Summation, i casi di erezione STCT e gli ulteriori nomi esplicitamente richiesti, in ambito distinto dai casi statici. Le collisioni di ambito sono rifiutate.

ReadStagesAsync richiede fase/passo/caso e riferimento degli spostamenti, limita ogni richiesta a 2000 elementi, e usa solo la lettura post/TABLE. ImportStages riusa Beam/Plate/Node del lettore statico per unità e segni; nessuna seconda implementazione. Phase/Step, provenienza e Accumulative/Current/Real sopravvivono all'archivio compatto. I risultati stage hanno semantica Unknown finché non è validata l'ipotesi di analisi: la presenza di una fase non autorizza la sovrapposizione lineare o la somma di Current/Real come incrementi. Cumulatività delle forze sconosciuta salvo dichiarazione esplicita. Nessuno zero sostituisce risultati assenti; una coppia fase/passo/caso richiesta e completamente mancante produce un errore. Il contratto delle vecchie richieste di basso livello resta compatibile.

Prove: 52 mirate su MIDAS/stage/filtri; Verify completo 12 Model.Core + 1212 UnitTest PASS, due benchmark nativi già esclusi, API 6517 firme e nessuna rimozione inattesa, audit uguaglianza superato. Evidenze `TestResults/Stages-20261012/full-compatible`. Copertura: tre stati in due fasi, beam/plate/reazioni/deformate, segni e unità, archivio, filtri statici, cancellazione, stato mancante/duplicato/estraneo, colonne mancanti, NaN, unità miste, modifica della fase dopo l'importazione. La prima corsa completa ha individuato una restrizione nuova nel costruttore delle richieste: rimossa la restrizione per mantenere il contratto precedente; test e attesi storici invariati.

Versioni: Converter 2.1.0.0 per la nuova lettura a fasi; ModelData 0.0.2.10 e Persistence 3.0.0.3 per il ricollegamento tecnico a Model 5.2.0.0. La distribuzione attende il set coerente e il collaudo ANTHEA. Il campione MIDAS reale attualmente aperto non ha stage: nessuna pretesa di verifica dei risultati stage sul software reale.
