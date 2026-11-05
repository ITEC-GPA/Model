# Migrazione comune BridGHe e Arachne a GPCModel

## Aggiornamento del 5 novembre 2026 (F8)

Cambiamenti rispetto all'aggiornamento del 4/11 qui sotto. Lo stato reale è in [STATO.md](STATO.md).

- **Il vocabolario del ponte esce da GPCModel** (utente: «Toglilo prima», per composizione). `BridgeModel` = `Model` GPC generico + `BridgeModelData`, nel Checker CompositeBridge; i dati di ogni trave stanno in `BridgeBeamData`, per GUID. I tipi del generatore del ponte di F5 sono cancellati. Corsia L12; regola e collocazione dei dati in [REGOLA-GPCMODEL.md](REGOLA-GPCMODEL.md).
- **Regola di GPCModel** (utente): struttura, carichi, analisi e risultati. I dati delle verifiche stanno nelle librerie di dominio. Il form finding resta in GPCModel per ora.
- **Rilascio:** «Aspetta la 0.4.0». Nessun BridGHe 0.3.1 intermedio: ANTHEA 1.19.0, BridGHe 0.4.0, Arachne 0.4.0 e ReBear 0.3.4 escono insieme sul set ufficiale.
- **Nella 1.19.0** entrano la planarità relativa delle piastre, le pressioni variabili scritte in Straus7 come forze nodali equivalenti e la classe del calcestruzzo da «fc = N MPa».
- **Barre della trave composta:** la 0.4.0 tiene una barra per fila nell'analisi, come la 0.3.0 («Dopo il rilascio»). Dopo il rilascio, BridGHe 0.5.0 con l'armatura distribuita (diametro + passo) e una sola sezione per analisi e verifiche.
- **Dopo il rilascio:** un solo formato di salvataggio (solo compatto in scrittura), revisione dell'API di GPCModel con una sola API di copia, forse la separazione del form finding.

## Aggiornamento del 4 novembre 2026 (F8)

Questa sezione dice che cosa è cambiato rispetto al piano qui sotto e quali decisioni ha preso l'utente. Il testo del piano dopo questa sezione è quello concordato il 2 novembre, scritto da ChatGPT, e resta com'era.

Stato reale per fase: [STATO.md](STATO.md). Ogni finding della revisione del 3/11, con stato, commit e prova: [CORREZIONI.md](../../../revisione-claude-20261103/CORREZIONI.md), in `C:\Users\g.pacini\Documents\Repository\gpc-unification-20261102\revisione-claude-20261103`.

### Che cosa è cambiato rispetto al piano

- **Chi lavora.** ChatGPT si è fermato il 3/11 alle 22:01, su richiesta dell'utente, con F6 incompleta. Claude ha rivisto il lavoro (`REVISIONE.md`), ha corretto tutti i finding e prosegue con F6, F7 e F8.
- **F5.** La matrice `f5-matrice-revisione.json` era una classificazione, non un'implementazione (finding 0.2). È segnata come superata. La mappatura per tipo, con la prova nel codice, è in [f8-mappatura-tipi.json](f8-mappatura-tipi.json).
- **F6, modelli FeMM.** Il piano prevedeva involucri intorno agli oggetti GPC. Ora i modelli FeMM si tolgono del tutto da BridGHe e Arachne. GUID e porte dei componenti restano invariati.
- **F6, ordine in BridGHe.** Prima `GH_Model` e la fase (c), cioè analisi, risultati, verifiche e carichi mobili su GPC. Poi le fasi (a) e (b), i tipi GH. Si usa il lettore legacy già verificato su 50 e 99. Nessun convertitore temporaneo da GPC a FeMM.
- **F6, lettore NRBF.** ChatGPT aveva aggiunto un lettore NRBF per i vecchi payload. È tolto, con `System.Formats.Nrbf`. I vecchi payload sono rifiutati con un messaggio.
- **F3, salvataggio.** `GPC.Archive` vale per tutti, anche per ANTHEA, al posto della sua copia. Nel piano l'adattatore ANTHEA era opzionale.
- **Formato compatto.** L5 ha introdotto `ModelArchive` v6, `ModelValueArchive` v2 e i sidecar a parti da 16 MiB. ANTHEA passa al formato compatto con la 1.19.0.
- **Risultati nei file accanto.** Arachne: `<documento>.<InstanceGuid>.gpc-results` accanto al `.gh`. BridGHe: `<output>.bghr` (formato `BGHR2`) accanto al `.st7`, come fa master 0.3.0.
- **Copie.** Le copie dei dati GH condividono le proprietà immutabili e i sistemi di riferimento (L9). Il modello resta una copia vera, quindi Duplicate resta più lento di master.
- **Versioni.** I numeri dati sul branch in F2–F6 erano provvisori. F7a ha dato numeri nuovi; F8 li rifà sul set ufficiale. Regola del 12/10: fix → ultima componente, altro → seconda.
- **Differenze nei risultati.** Alcune differenze sono approvate dall'utente: aree a taglio, G, Ecm, travi composte e altre. L'elenco è in [F6-CONCLUSIONE.md](F6-CONCLUSIONE.md).
- **Rilascio.** «Tutto insieme», dopo la fine di BridGHe (vedi sotto e STATO.md).

### Decisioni dell'utente

3/11 sera:
- tutti i finding di `REVISIONE.md` sono giusti e vanno corretti;
- togliere del tutto i modelli FeMM da BridGHe e Arachne, mantenendo GUID e porte dei componenti;
- writer Straus7: pannelli a taglio con G = E/2(1+ν); rigidezze delle travi dai valori GPC, con le aree a taglio calcolate da GPC;
- togliere il lettore NRBF, con `System.Formats.Nrbf`;
- `GPC.Archive` per tutti, ANTHEA compreso, ottimizzato;
- tutto resta sul branch; a fine lavori si prova il merge su master; prima di push o installazioni si chiede.

4/11 mattina:
- aree a taglio di De Saint Venant con ν = 0 («seguirei la scienza delle costruzioni»);
- parti separate che si inflettono insieme: la regola va bene;
- travi composte: aree a taglio dalla sezione omogeneizzata, ν = 0, con l'n della proprietà;
- Arachne: massa in IPS a scelta di Claude (libbra-massa coerente); via i vecchi test (da 171 a 162); Cable e Fixed Axial Force possono convivere, come in master;
- BridGHe: M2, Ecm di GPC al posto degli E tabellati; M3, G = E/2(1+ν) anche nella torsione della griglia di soletta; M1, M4 e O1 a scelta di Claude.

4/11 giorno:
- ANTHEA passa al formato compatto dalla prossima versione minore; legge sempre i file vecchi; le versioni vecchie danno un messaggio chiaro;
- proprietà immutabili condivise tra le copie GH; il modello resta una copia vera;
- risultati nel file accanto.

4/11 pomeriggio:
- «A fine fase di correzioni unisci i vari rami. Riportiamo tutto il lavoro ad un repo pulito»;
- sistemi di riferimento condivisi nelle copie: sì, con una prova che restino invariati;
- cattura congelata `cattura-mainwindow` di ANTHEA: aggiornarla;
- sezioni di ANTHEA 1.18 convertite al primo salvataggio: tenerla così;
- ANTHEA con un archivio in uso da un altro programma: lasciarlo com'è (il salvataggio si ferma);
- sezioni composte non saldate a contatto: separare i nodi nel writer;
- offset centrifugo: GPC resta in N per veicolo e il writer converte;
- leggi centrifughe che dipendono dal raggio: restano `AdapterDefined`;
- `LineLoad.GetGlobalForces`: correggerlo;
- D3 preparato una volta: «falli».

4/11 sera, rilascio:
- «Tutto insieme»: si aspetta la fine di BridGHe, poi il set ufficiale da master, ANTHEA 1.19.0 con la sua full, BridGHe 0.4.0, Arachne e ReBear sullo stesso set;
- «Sì, push e pubblica»: se tutte le prove passano, push, tag, installer, bin dei plugin ricostruiti con Rhino chiuso e copia sul server X:, di notte se possibile, con il contenuto vecchio in una cartella da eliminare;
- installer di ANTHEA anche in `\\STORAGESARZANA\Varie\20 - AGGIORNAMENTI_SOFTWARE\Anthea`, con il precedente spostato in `_SUPERATI`;
- da ricostruire solo BridGHe, Arachne e ReBear.

Il ritorno alla versione precedente è descritto in [RITORNO-VERSIONE-PRECEDENTE.md](RITORNO-VERSIONE-PRECEDENTE.md).

---

Piano concordato il 2 novembre 2026. Avvio autorizzato nella chat con richiesta di branch separato.

Ordine operativo aggiornato su richiesta dell'utente: F1, F2, F3 e F4 in coda. F4 è stata poi avviata e completata il 3 novembre 2026 nel perimetro documentato in [F4-PILOTI.md](F4-PILOTI.md), con [evidenze di verifica](F4-VERIFICHE.md).

Il 3 novembre l’utente ha autorizzato la prosecuzione F5–F8. **F5 è completata**: [perimetro e verifiche finali](F5-CONCLUSIONE.md), [evidenze](f5-conclusione-evidenze.json). Matrice chiusa su 295 tipi e 2.309 campi; suite Model e Checker superate. I gate di uscita restano quelli sotto; F6–F8 non sono dichiarate completate.

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

La precedente stima 29-39 giorni è preliminare e non copre separatamente tutti i dettagli di persistenza e integrazione. La stima residua dopo F4 è 25–38 giornate equivalenti sequenziali, con ripartizione e limiti in [F4-PILOTI.md](F4-PILOTI.md). Il percorso più breve presuppone lavoro parallelo su F6A/F6B e assenza di revisioni sostanziali dei contratti.

Le decisioni che richiedono l'utente sono registrate con problema, evidenza, effetto e alternative; in attesa della risposta proseguono soltanto le attività indipendenti.
