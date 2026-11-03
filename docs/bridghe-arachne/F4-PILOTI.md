# F4 — Piloti GH, GPC e solutori

Avviata su richiesta esplicita dell’utente il 3 novembre 2026, nei worktree del branch `codex/gpc-unification-20261102`. Nessun merge, push, aggiornamento di `lib/GPC` o installazione Rhino. La verifica finale è in `F4-VERIFICHE.md` e `f4-evidenze.json`.

## Architettura provata

I componenti GH esistenti costruiscono gli input dei piloti. Un confine di conversione produce un grafo GPC indipendente; il grafo del solutore viene ricostruito esclusivamente da questi dati. Il calcolo restituisce risultati associati mediante `SolverIdentityMap`, con fingerprint degli input. `ExternalModelArchive` salva grafo, risultati compatti e mappa in un sidecar `.gpc-results`; il GHX conserva un riferimento XML in un pannello. La riapertura avviene anche in un nuovo processo Rhino, con confronto di fingerprint, mappa e SHA-256 di **tutto `results.bin`**, oltre alla nuova soluzione nel processo del pilota.

Gli adattatori sono deliberatamente interni ai banchi prova e compilati soltanto con `-p:GpcPilot=true`. F4 dimostra il percorso completo e i contratti minimi; non sostituisce ancora i componenti distribuiti. La gestione automatica del sidecar negli eventi di salvataggio/Autosave dei componenti di produzione appartiene a F6.

Per BridGHe il backend resta l’exporter Straus7 esistente. Per Arachne l’importatore transitorio legge l’assemblaggio esplicito del preprocessore attuale, prodotto da componenti GH reali; poi ricostruisce un nuovo modello TensileLib da GPC. Questo passaggio non migra ancora il preprocessore: F6 deve portare i componenti a produrre direttamente il dominio comune. Nessun oggetto TensileLib viene conservato nel grafo GPC o nel sidecar.

## Contratti introdotti

- **GPCModel 5.40.0.0**: `FormFindingDefinition`, obiettivi di force density, forza di cavo, forza per larghezza, lunghezza e forza assiale; direzioni Warp/Weft; penalità delle coordinate; massa superficiale. Pressione follower, gravità, piano di proiezione e opzioni numeriche esplicite. Le penalità non sono pesi gravitazionali.
- Nuovi costruttori con GUID per `NodeElement`, `BeamElement`, `AreaElement`, `LoadCaseBase`. Il chiamante fornisce l’identità prima dell’assemblaggio; non viene aggiunto un setter per riassegnarla dopo il calcolo.
- Contratti chiusi di copia, XML e fingerprint. Gli input di form-finding partecipano all’impronta d’analisi; la copia separa collezioni e vettori mutabili.
- **GPC.Model.Persistence 3.6.0.0**: conserva `FormFinding` nel documento e nell’archivio esterno. Schema Model 5 e ModelDocument 4 soltanto quando il nuovo campo è presente; gli archivi precedenti continuano a essere letti. GPC.Archive rimane 1.0.0.0.

Unità canoniche: N, mm, s, tonne; force density e forza per larghezza N/mm, forza N, lunghezza mm, massa superficiale t/mm², gravità mm/s², pressione N/mm². Nei piloti il dominio riceve già input N-mm. La conversione generalizzata di tutte le unità dei plugin resta da portare negli adattatori definitivi.

Le identità dei piloti derivano da namespace della definizione, ramo e indice topologico persistente della fixture. Non dipendono dalle coordinate e si riproducono tra esecuzioni. In F6 occorre il registro di chiavi topologiche posseduto dall’istanza del componente per le topologie editabili; gli indici di questi esempi non costituiscono quella soluzione generale.

## Ponte

Il banco prova alimenta il componente Build Model reale con GH_Node/GH_Beam; geometrie e proprietà della fixture sono costruite dal test. Tre nodi, due travi, coordinate `(0,0,0)`, `(3000,600,100)`, `(6000,1200,200)` mm. Modello lineare elastico, senza peso proprio; appoggi e vincoli torsionali espliciti. Carico centrale `(120,80,-1000)` N e momento `(5000,7000,9000)` N mm. Rotazione della sezione 23°, eccentricità locali `(40,75)` mm. Seconda variante: inversione I/J della seconda asta con orientamento ed eccentricità coerenti.

Il dominio conserva le rigidezze EA, EI1, EI2, GJ e GA1/GA2 in `BeamAnalysisProfile`, con assi e offset espliciti. L’esportazione usa una sezione generica equivalente con modulo di riferimento 1 N/mm² e ν=0. È una rappresentazione per l’analisi: **non è la sezione fisica per una verifica di resistenza**. Il materiale/sezione fisico completo rientra in F5.

Sono confrontati spostamenti, rotazioni, reazioni e sei componenti degli sforzi in ogni stazione esportata. Le tolleranze sono `max(1e-7, 2e-8 × |riferimento|)` nelle unità della rispettiva grandezza. Controlli indipendenti: equilibrio verticale e invarianza della risposta nodale globale dopo inversione I/J. I momenti nativi BM1/BM2 entrano nelle componenti GPC come M1=BM2, M2=−BM1, nel sistema della sezione.

## Membrana

Rete a sella 4×4: 25 nodi, 40 aste, 16 superfici; luce 1000 mm, quota del bordo ±200 mm, forza centrale −10 N lungo Z. Obiettivi per unità di larghezza 2 N/mm lungo Warp e 3 N/mm lungo Weft, costruiti con i componenti dedicati.

Quattro casi: equilibrio di base; pressione normale 0.0001 N/mm²; metodo non lineare e vincolo parziale centrale ruotato di 45°; gravità 9806.65 mm/s² e tessuto da 1050 g/m², cioè 1.05e-9 t/mm². Confronto con il percorso attuale su tutti gli spostamenti, forze assiali e reazioni, tolleranza `1e-8 × max(1, |riferimento|)` nelle unità della grandezza. Tutti i casi devono convergere.

L’archivio conserva spostamenti, reazioni, sforzi assiali, stato del dataset e opzioni. Le rotazioni nodali e le componenti non applicabili alle aste assiali sono marcate `NotApplicable`. La massa di linea e di superficie è preservata indipendentemente da area/spessore fittizi usati internamente da TensileLib. Le direzioni Warp/Weft identificano gli obiettivi della rete; non costituiscono una legge elastica ortotropa verificata.

## Perimetro e prossime fasi

I due piloti rifiutano assegnazioni non gestite; includono una prova di rifiuto per un’assegnazione nodale fuori perimetro. Non certificano la copertura dei 2.309 membri inventariati in F2. Restano in F5: exporter indipendente dal backend BridGHe, sezioni/materiali fisici completi, carichi distribuiti e mobili, fasi, rilasci/molle, proprietà ortotrope e gli altri tipi della matrice. Gli obiettivi lunghezza/forza imposta sono contrattualizzati, ma non sono coperti dal pilota membrana con lo stesso grado delle force density Warp/Weft.

F1 resta immutata. F4 usa confronti accoppiati fra percorso attuale e percorso GPC per i nuovi piloti, con il solutore TensileLib congelato in F1. Non è una ripetizione completa degli esempi ponte 50/99 e di tutti gli 80 dump TensileLib: quella regressione resta necessaria quando F5/F6 cambieranno i percorsi di produzione.

La stima residua è indicativa: F5 8–12 giornate equivalenti, F6 BridGHe 6–9, F6 Arachne 5–8, F7 3–5, F8 3–4; totale sequenziale 25–38. Sono quantità di lavoro, non una previsione di calendario. Il pilota riduce l’incertezza su copie/identità/archivio, ma non elimina quella del vocabolario completo e delle convenzioni dei solutori.
