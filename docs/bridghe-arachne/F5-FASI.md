# F5 — fasi costruttive e incrementi Straus7

Prosecuzione del 3 novembre 2026, dopo il checkpoint Model `47c9b4df`. **F5 resta aperta; F6–F8 restano autorizzate e in coda.** Nessuna integrazione nei checkout principali o nelle installazioni Rhino.

## Dominio e persistenza

`Stage.AnalysisState` descrive la configurazione fisica: geometria di nascita originale, deformata oppure azzeramento della storia; movimento dei nodi vincolati; rotazione dei gruppi di elementi entranti; livello del fluido in mm. Gruppi e freedom case attivi sono insiemi completi di GUID, non differenze rispetto alla fase precedente. L'attivazione non include implicitamente i discendenti dei gruppi.

`StageLoadIncrement` conserva ordine, nome, fattori dei load case, fattori dei freedom case e pseudo-tempo facoltativo in secondi. I fattori sono **totali al passo**, anche negativi o nulli: non si sommano agli incrementi precedenti. Un fattore assente equivale a zero. Un freedom case attivo con fattore zero conserva i vincoli e azzera gli spostamenti imposti; la rimozione dei vincoli richiede la disattivazione del freedom case.

Le validazioni rifiutano riferimenti sconosciuti o duplicati, valori non finiti, tempi incompleti o decrescenti e opzioni incompatibili con la geometria di nascita. La sequenza appartiene a `Stage.Definition`; il writer rifiuta sequenze duplicate, matrici legacy di combinazioni e calendari con durata/attivazioni temporali non convertiti esplicitamente.

I nuovi record sono inclusi nei contratti chiusi di copia, archivio e fingerprint. Le appartenenze ai gruppi diventano input delle impronte di analisi e associazione ai risultati **soltanto in presenza del nuovo stato fisico delle fasi**. I gruppi organizzativi dei modelli storici mantengono il comportamento precedente.

Versioni: GPCModel **5.42.0.0**, Persistence **3.8.0.0**, Converter **2.14.0.0**. `StageVersion` 2, Model schema 7 e ModelDocument schema 6 vengono scritti solo in presenza dei nuovi dati. Nel checkpoint precedente il numero nominale nel csproj era 5.41.0.0 ma `AssemblyInfo.cs`, con generazione automatica disabilitata, riportava ancora 5.39.0.0: anche AssemblyVersion e AssemblyFileVersion sono ora allineati a 5.42.0.0. Nessuna fixture, baseline API o risultato atteso rigenerato.

## Writer e collaudo nativo

`Straus7StageWritePlan` prepara operazioni distaccate dagli input e conserva la mappa GUID fase → numero nativo. Richiede mappe complete e biunivoche di gruppi, load case e freedom case; controlla la revisione e le identità delle fasi prima di scrivere, anche quando una nuova fase ha gli stessi dati fisici della precedente. `IStraus7StageWriteApi` consente il collaudo senza licenza; `Straus7NativeWriter` esegue le chiamate R31 su un file nuovo o su una copia con **nessuna fase preesistente**.

- Scrittura di tutti i cinque flag di `St7AddStage`, inclusi livello del fluido e Reset. Il vecchio exporter BridGHe passa un array di tre elementi e non è stato ancora sostituito in produzione.
- Attivazione/disattivazione esplicita di ogni gruppo e caso; creazione ordinata degli incrementi; scrittura dell'intera matrice dei fattori, compresi gli zeri di scarico.
- Salvataggio e rilettura in una nuova sessione: geometria di nascita, Morph/Rotate/Reset, livello del fluido, gruppi, casi, fattori, nomi e pseudo-tempi.
- Rifiuto di una seconda scrittura sulle fasi appena create e controllo SHA-256 del sorgente invariato.

Un test ha trovato un comportamento effettivo di R31: aggiungere il **primo incremento** reimposta come attivi i load case e i freedom case precedentemente disabilitati. La diagnosi isolata ha distinto questo comportamento da quello dei setter dei fattori. Il piano applica ora le selezioni dopo la creazione degli incrementi. Il collaudo mantiene gli stessi attesi e verifica anche i casi disattivati dopo la riapertura. Il tentativo fallito è conservato in `stage-all-native-final.log`; il risultato valido è `stage-writers-verified.log`, confermato dal gate completo `model-stage-native-gate.log` con i writer abilitati.

Fonti locali verificate: SDK `St7API.cs` R31; manuale API, pagine stampate 622–626; help `GlobalStages.htm` e `SolversIncrements.htm`. Estratti conservati in `artifacts/f5/straus-help` e `artifacts/f5/stage-followup` nel workspace isolato.

## Regressioni e limiti delle prove

- Model: **1.844 PASS** (63 Core, 1.709 UnitTest, 72 Framework), API e audit superati. Il gate conclusivo abilita i tre writer nativi opt-in; restano 12 test ignorati preesistenti. Il precedente gate ordinario riportava 1.841 PASS e 15 ignorati.
- Writer: **13 PASS**, inclusi i tre test nativi di attributi fisici, carichi mobili e fasi, su copie nuove dei modelli fissati.
- Checker: **10.369 PASS** nelle 11 suite; un test temporale preesistente ignorato, nessun nuovo fallimento.
- BridGHe: **515 asserzioni** nei due piloti ponte; **11 asserzioni** nella riapertura dei due GHX in un processo separato, con byte numerici, input e mappa delle identità invariati. Build predefinita ripristinata; **269 controlli PASS**.

Il pilota F4 ora rifiuta esplicitamente i nuovi dati fisici e i carichi mobili che il suo adapter limitato non trasferisce. Non può più perderli durante l'export. Il percorso produttivo dei componenti non è stato migrato.

La prova delle nuove fasi verifica dati e impostazioni nativi salvati, **non esegue una nuova analisi NLA per confrontare gli effetti fisici di Morph/Rotate/Reset**. Le prove numeriche del ponte riguardano i due casi lineari F4. Resta da provare l'equivalenza delle analisi per fasi prodotte dal nuovo exporter completo, incluse attivazioni di gruppi gerarchici, geometria di nascita e risultati per incremento.

## Lavoro residuo

La matrice parziale riscontra **103 campi su 2.309**; nessuno è dichiarato migrato nei componenti produttivi. Il numero dei campi non è una misura percentuale del lavoro residuo.

F5 richiede ancora exporter di topologia/proprietà/carichi completo, collegamento delle fasi al percorso produttivo, load patch, string group, output stations nel lettore, materiali/supporti e moving load avanzati, conversione statica dei percorsi con confronto numerico e workflow ponte eseguibile. Seguono F6 (componenti e rimozione dei vecchi grafi/BinaryFormatter), F7 (set condiviso e consumatori ANTHEA/ReBear) e F8 (pacchetti, documentazione, installazione pulita e rollback).

Log e TRX sono nel workspace `artifacts/f5`; [f5-fasi-evidenze.json](f5-fasi-evidenze.json) identifica le prove e i sorgenti di questo checkpoint. Il manifest F5 precedente resta storico.
