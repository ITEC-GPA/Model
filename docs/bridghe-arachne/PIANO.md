# Migrazione comune BridGHe e Arachne a GPCModel

Piano concordato il 2 novembre 2026. Avvio autorizzato nella chat con richiesta di branch separato.

## Obiettivo e perimetro

Sostituire i modelli interni dei due plugin con oggetti GPC, compresi i descrittori di dominio. Conservare nei plugin wrapper Grasshopper, preview, integrazione Rhino e adattatori. TensileLib conserva il proprio solutore e il modello interno necessario al calcolo.

GPCModel contiene il dominio strutturale; Checker i dati e le procedure di verifica; Converter le convenzioni dei software e i relativi adattatori. Model non dipende da Checker, Converter o ANTHEA. L'archivio condiviso deve stare in uno strato di persistenza riutilizzabile dai runtime dei consumatori.

SAP2000 e MIDAS: i nuovi exporter sono un'estensione successiva, da stimare e verificare separatamente.

## Ambiente di lavoro

- Workspace isolato: `Repository/gpc-unification-20261102`.
- Branch comune: `codex/gpc-unification-20261102` nei repository Model, Checker, BridGHe, Arachne e tensilelib.
- Geometry e Utilities: worktree detached su commit fissati; prima di modificarli creare branch dedicati.
- Checkout principali, esempi modificati dall'utente e installazioni Rhino non sono destinazioni di build o di prove.
- Commit di partenza, hash e prove eseguite sono registrati nel manifest e nello stato; il completamento di una fase richiede evidenze, non soltanto codice compilabile.

## F0 - Avvio e isolamento

Verificare versioni pubblicate, stato dei checkout e lavori concorrenti visibili. Aggiornare i riferimenti remoti e creare i worktree separati. Fissare la provenienza delle DLL usate dalle baseline.

Il set utilizzato da un plugin per la baseline resta quello della sua versione corrente: non sostituirlo automaticamente con le librerie del branch di sviluppo.

Uscita: autorizzazione acquisita, workspace isolato e revisioni di partenza identificate. Le limitazioni della verifica dei lavori esterni vanno dichiarate nello stato.

## F1 - Baseline funzionale, numerica e prestazionale

BridGHe: audit porte/GUID, Smoke, regressioni, esempi 50 e 99, dati effettivamente esportati in Straus7, risultati e verifiche. Preparare un dump numerico completo degli input esportati, con copertura dichiarata e double senza perdita. Distinguere questo dump dalle impronte delle sole verifiche di sezione.

Arachne: test ARAP e headless, input effettivi di TensileLib, geometria iniziale e finale, forze, reazioni e convergenza. Identificare le unità degli esempi.

Prestazioni: costruzione, copia, assemblaggio, fingerprint, salvataggio, riapertura e allocazioni, a più dimensioni e sui runtime effettivi (Arachne net48, BridGHe net8.0-windows). Misurare anche le catene di componenti, non soltanto le funzioni isolate.

Misurare la non complanarità dei plate negli esempi 50 e 99. Conservare le baseline esistenti; nuove catture hanno un'identità e una provenienza proprie.

Uscita: riferimenti congelati e riproducibili, con elenco di controlli eseguiti, esclusioni, problemi preesistenti e limiti delle misure.

## F2 - Contratti e fondazioni

Preparare una matrice per ogni tipo e campo: sorgente, destinazione, significato, unità, identità, serializzazione, fingerprint e utilizzatori.

Definire e verificare:

- oggetti GPC autonomi nei componenti;
- proprietà degli oggetti, copia e isolamento delle modifiche tra rami Grasshopper;
- assemblaggio deterministico e fusione dei nodi con tolleranza dimensionale, risoluzione dei conflitti e possibilità di mantenere nodi coincidenti distinti;
- GUID persistenti, ID del modello e ID del solutore, con rimappatura esplicita;
- gruppi, membri fisici e metadati di ponte, compresi percorsi non rettilinei;
- geometria dei quad non complanari, calcolo di area/normali/assi e validazione delle formulazioni;
- conversioni comuni delle unità e delle convenzioni di segno e orientamento.

Base interna proposta: N, mm, rad, s, t, °C. Specificare separatamente grandezze derivate, gradienti termici, masse superficiali, rigidezze e proprietà delle membrane. La proposta deve essere riscontrata sui contratti esistenti prima dell'implementazione.

Target iniziale: copia di circa 10.000 nodi e 20.000 gusci entro 0,3 s sulla macchina e con il protocollo di F1. Nessun allentamento silenzioso del requisito: se non raggiungibile, registrare misure e alternative e attendere una decisione su quel punto.

Uscita: contratti verificati, copie indipendenti, assemblaggio riproducibile, limiti dei membri e dei gusci espliciti.

## F3 - Persistenza condivisa

Estrarre la parte riutilizzabile dell'archivio esterno di ANTHEA, separandola dai documenti applicativi. Definire il rapporto tra risultati GPC per elemento e blocchi per caso/combinazione: non duplicare inconsapevolmente codec, contenuti e serializzazioni.

Gli output pesanti delle analisi non vengono riscritti dentro il documento Grasshopper a ogni autosave. I riferimenti includono versione, impronte e un percorso o un meccanismo di risoluzione; un digest da solo non è un archivio.

Gestire salvataggi atomici, Save As, trasferimento dei progetti, documenti non ancora salvati, archivi mancanti o corrotti, invalidazione e conservazione delle revisioni. Mantenere la compatibilità richiesta dagli archivi GPC già esistenti.

Uscita: recupero riproducibile dopo riapertura, errori espliciti per dati indisponibili, salvataggio invariato senza ricodifica completa.

## F4 - Pilota end-to-end

Un piccolo ponte: componenti GH -> GPC -> Straus7 -> risultati -> archivio -> riapertura. Comprende orientamenti, eccentricità, vincoli e carichi rappresentativi.

Una membrana: componenti GH -> GPC -> TensileLib -> form-finding -> risultati -> archivio -> riapertura. Comprende obiettivi, direzioni del tessuto e vincoli rappresentativi.

Introdurre i tipi necessari ai piloti prima di convertire tutti i componenti. Correggere i contratti emersi come inadeguati e aggiornare la stima delle fasi successive.

Uscita: equivalenza con le baseline, prestazioni accettabili, nessuna contaminazione tra rami GH.

## F5 - Dominio completo ed exporter Straus7

Completare la matrice di F2.

GPCModel: temperatura nodale, gradienti, precarichi, precompressione dei plate, masse non strutturali, supporti elastici, rastremazioni, carichi mobili, fasi e dati fisici delle membrane; materiali ortotropi, proprietà per unità di larghezza, force density, lunghezze/forze imposte, pesi nodali, pressioni follower, obiettivi e risultati di form-finding.

Checker.CompositeBridge: workflow di verifica, associazione delle fasi ai casi, stati delle sezioni e travi composte, con riferimenti stabili agli elementi. La collocazione dei dati fisici e dei dati di verifica segue la matrice, senza dipendenze inverse.

Converter: trasferire l'exporter Straus7 dietro un'interfaccia verificabile (proposta `IStraus7WriteApi`), coprendo load patch, string group, Morph/Rotate, output stations e rilettura dei risultati effettivamente prodotti.

Registrare ogni tipo nei contratti necessari di copia, archivio e fingerprint. Nessuna conversione deve omettere dati non supportati senza diagnosi.

Uscita: copertura completa della matrice e suite pertinenti delle librerie superate.

## F6 - Migrazione dei plugin

F6A BridGHe: componenti, generatori, carichi mobili, fasi e verifiche sul nuovo dominio; rimuovere l'exporter locale soltanto dopo l'equivalenza.

F6B Arachne: componenti e adattatore TensileLib sul nuovo dominio; separare input, obiettivi e risultati; eliminare BinaryFormatter. Il patterning indipendente dal modello conserva il comportamento esistente.

F6A e F6B possono procedere in parallelo soltanto dopo la stabilizzazione dei contratti comuni. I wrapper GH, la preview e gli adattatori restano nei rispettivi plugin. Preservare o migrare consapevolmente GUID, parametri ed esempi.

Uscita: vecchi modelli rimossi dalla produzione, percorsi funzionali coperti e regressioni equivalenti.

## F7 - Integrazione

Verificare il set completo di librerie e i consumatori, inclusi ReBear e ANTHEA. Controllare caricamento delle DLL nello stesso processo, archivi, risultati numerici, memoria, autosave, riapertura e scala dei modelli.

Bit-identità dove richiesta e applicabile. Negli altri confronti: unità normalizzate, corrispondenze degli ID e tolleranze dimensionali assolute/relative motivate. Ogni differenza è registrata e classificata; mai cambiare gli attesi per nascondere regressioni.

Uscita: nessuna regressione nuova non approvata, controlli prescritti dai repository superati, prestazioni confermate.

## F8 - Documentazione e rilascio

Aggiornare esempi, guide, convenzioni e limiti; produrre un set coerente con commit, versioni, SHA-256 ed esiti delle suite. Provare pacchetti ed esempi in ambiente pulito e documentare il ritorno alla versione precedente.

Uscita: rilascio identificabile e riproducibile. I branch di questa attività restano separati durante lo sviluppo; integrazione e pubblicazione seguono le autorizzazioni applicabili al momento del rilascio.

## Stime e scostamenti

La precedente stima 29-39 giorni è preliminare e non copre separatamente tutti i dettagli di persistenza e integrazione. Ristimare dopo F4. Il percorso più breve presuppone lavoro parallelo su F6A/F6B e assenza di revisioni sostanziali dei contratti.

Le decisioni che richiedono l'utente sono registrate con problema, evidenza, effetto e alternative; in attesa della risposta proseguono soltanto le attività indipendenti.
