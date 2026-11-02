# F2 - Contratti e fondazioni

Sviluppo nel branch `codex/gpc-unification-20261102`, GPCModel **5.39.0.0**. Incremento di funzionalità: contratti di identità, assemblaggio, geometria e unità. I plugin continuano a usare il proprio modello durante F2; la sostituzione dei componenti e l'equivalenza degli adattatori appartengono a F4-F6.

## Matrice e perimetro

[f2-inventario.json](f2-inventario.json) contiene 295 dichiarazioni di tipo e 2.309 membri dichiarati: campi, proprietà, eventi, valori enum e proprietà dei record posizionali. Comprende Models, MovingLoads, Checks e Geometry di BridGHe, Models di Arachne, Geometry e FormFinding di TensileLib. I membri ereditati sono riportati sul tipo che li dichiara; statici, cache e proprietà calcolate restano visibili. Ogni dichiarazione riporta file, riga e SHA-256. Patterning/ARAP, rendering e interfaccia non sono inclusi nel dominio da unificare.

[f2-destinazioni.json](f2-destinazioni.json) assegna a ogni membro una destinazione architetturale, un significato o riferimento al sorgente, unità o incertezza dimensionale, politica di identità, serializzazione, fingerprint e utilizzatori. Le righe `existing-partial`, `extension-*` e `review-*` sono lavoro da completare e verificare in F4/F5: non attestano un adattatore già realizzato. Non viene inventato un membro GPC per coprire un campo mancante. La copertura sintattica della matrice è completa nel perimetro; la copertura semantica e implementativa completa è il gate di F5.

Rigenerazione dal workspace isolato:

```powershell
dotnet run --project tools/MigrationInventory -c Release -- .. docs/bridghe-arachne/f2-inventario.json
py -3 tools/MigrationInventory/map_inventory.py docs/bridghe-arachne/f2-inventario.json docs/bridghe-arachne/f2-destinazioni.json
```

La separazione principale è: dominio fisico in GPCModel, dati/procedure di verifica in Checker, convenzioni e interfacce software in Converter, wrapper/preview/Rhino nei plugin. TensileLib conserva il modello numerico e gli algoritmi interni. Model non acquisisce riferimenti a Checker, Converter, Rhino o ANTHEA.

## Proprietà e copie

`ModelValues.CopyValue` copia un oggetto registrato o un array di oggetti come un unico grafo; `ModelValues.Copy` copia un modello e riutilizza soltanto i blocchi immutabili dei risultati, decodificati in nuovi oggetti al primo accesso. I GUID delle entità, gli alias interni, i riferimenti ai casi e i contatori degli ID rimossi sono conservati. Nessun oggetto mutabile del grafo copiato viene condiviso con l'input.

I componenti possono quindi restituire nodi, travi, gusci, proprietà e assegnazioni autonomi. Un componente che modifica un ramo deve copiare il grafo interessato una volta prima della modifica. Copiare separatamente ogni oggetto connesso distrugge gli alias e non soddisfa questo contratto. Il vocabolario resta chiuso: una sottoclasse sconosciuta viene rifiutata, non ridotta alla classe base.

Le ottimizzazioni riproducono i contratti persistiti di NodeElement, AreaElement, Point3d, Vector3d, CoordinateSystem e delle matrici MathNet 5.0 fissate dal progetto. Il confronto con DataContractSerializer verifica il grafo completo; i test sui bit sono separati, perché XML normalizza alcuni zeri con segno e ricostruisce le stringhe immutabili. Le copie in memoria conservano i bit dei double, inclusi -0 e payload NaN; le geometrie destinate al calcolo devono comunque essere finite.

## Assemblaggio e fusione

`ModelAssembly.Assemble` accetta nodi, travi a due nodi e gusci a tre/quattro nodi con connettività esplicita. Richiede un GUID stabile del modello, posseduto dal documento/componente. Copia il grafo, ordina le entità per GUID e assegna ID positivi distinti per famiglia. Invertire l'ordine degli input non modifica il modello assemblato né le sue impronte. Copie discordanti dello stesso GUID vengono rifiutate.

`mergeToleranceMm = null` mantiene distinti i nodi coincidenti. Un valore finito >= 0 abilita la fusione; zero richiede coordinate esattamente uguali. `keepSeparate` protegge le identità indicate anche se coincidenti. La griglia spaziale cerca i rappresentanti nelle celle adiacenti e sceglie quello con GUID minimo entro la tolleranza euclidea. La fusione non è transitiva: ogni nodo deve essere entro tolleranza dal rappresentante, senza propagazione lungo catene di punti vicini. Coordinate e tolleranza sono in mm. Rapporti coordinata/tolleranza non risolvibili dalla griglia double sono rifiutati esplicitamente.

Conflitti di assegnazioni, vincoli o gruppi non vengono risolti automaticamente. La fusione di nodi con carichi viene rifiutata: il chiamante deve stabilire esplicitamente se sommare, sostituire o conservare separati. Sono rifiutati anche elementi o link collassati dopo la fusione. Le identità sorgente dei nodi assorbiti restano nella mappa persistita.

Orientamenti e link espressi tramite vecchi ID numerici vengono risolti prima della rinumerazione. Se un ID identifica più nodi di rami indipendenti, l'assemblaggio fallisce con una diagnosi. Casi e proprietà omonimi devono avere contenuti equivalenti. I gruppi conservano la gerarchia e non creano membri fisici implicitamente.

Limiti deliberati dell'API F2: niente importazione implicita da geometrie senza nodi, niente solidi/elementi di ordine superiore, risultati preesistenti o generici `Attributes` con rimappatura sconosciuta. Questi input richiedono un adattatore esplicito; vengono rifiutati anziché omessi. Combinazioni, fasi, vincoli globali e membri fisici si aggiungono esplicitamente al modello assemblato. Il contratto non certifica un problema FEM completo e ben vincolato.

## Identità e archivi

| Identificatore | Responsabilità | Persistenza |
|---|---|---|
| GUID entità | Identità stabile tra copie, rami e riaperture | Contratto dell'entità |
| SourceIdentity | Programma, revisione, famiglia, identificatore originale | Entità e voci di AssemblyIdentityMap |
| ID modello | Chiave intera della famiglia nel modello assemblato | Registri e AssemblyIdentityMap |
| ID solutore | Chiave dell'esportazione per famiglia e specifica analisi | SolverIdentityMap associata al run |

`AssemblyIdentityMap` conserva input GUID, GUID canonico, ID originale, ID finale e provenienza, compresi gli alias fusi. È salvata nel modello. `ToSolver(model, solver, ids)` verifica la corrispondenza con il modello corrente e richiede un ID positivo univoco per ogni entità canonica; produce una mappa legata all'impronta degli input di analisi. Una mappa non aggiornata rispetto alle entità del modello viene rifiutata. Il salvataggio della mappa del solutore insieme al dataset sarà responsabilità dell'archivio F3/adattatore F4; F2 introduce il contratto serializzabile e verificato, senza dichiarare già modificati gli exporter.

I nuovi contratti sono registrati nel vocabolario degli archivi e delle impronte. `ModelSchemaVersion` sale a **4 soltanto se sono presenti AssemblyMap o i nuovi metadati di ponte**; i modelli privi di questi dati continuano a scrivere gli schemi precedenti. Il reader accetta fino a 4 e rifiuta versioni future. La versione dello schema del modello è distinta dalla versione del contenitore `ModelArchive`. Nessuna fixture o baseline API preesistente viene rigenerata.

I modelli attuali dei plugin non espongono già un GUID persistente per ogni entità. Gli adattatori F4/F6 dovranno assegnarlo e conservarlo nel wrapper/documento, usando identità di componente, ramo e chiave dell'oggetto; non basta generare un nuovo GUID a ogni soluzione, né ricavarlo soltanto dalle coordinate.

## Membri fisici e ponte

`PhysicalMemberDefinition` resta una sequenza esplicita di parti e versi I/J. `PhysicalMemberPath` ne verifica connettività, assenza di cicli, segmenti non degeneri ed eventuali discontinuità delle linee eccentriche. Ammette percorsi non rettilinei, parametrizzati per ascissa curvilinea in mm. Ai nodi comuni conserva entrambe le sezioni se il lato non è specificato; per una parte percorsa J->I inverte stazione e lato. È una fotografia geometrica indipendente dagli edit successivi del modello.

Non viene definito un asse locale unico attraverso uno spigolo e non vengono trasportate automaticamente le sollecitazioni. `PhysicalMemberGeometry` conserva il requisito di rettilineità per le procedure che lo richiedono. La validazione dell'edit usa il percorso generale; i verificatori mantengono i propri limiti di formulazione.

`BridgeMemberMetadata` associa esplicitamente PhysicalId, famiglia, gruppo geometrico, segmento, campata, progressive, direzione SectionUp e GeometryId al membro. Il gruppo geometrico non è un `Group` organizzativo. Il metadato opzionale entra nel fingerprint del membro e nell'archivio; se assente, le impronte storiche restano uguali. Stati di verifica, casi di classificazione e coefficienti del workflow composito non sono trasferiti automaticamente in questo record fisico.

## Gusci non complanari

`AreaElement.Nodes` conserva già i vertici spaziali; `ShellGeometry` fornisce la geometria senza costruire un Polygon3d planare. `Shape` resta l'interfaccia planare preesistente. Per i Q4 si usa la mappa bilineare:

`x(r,s) = sum_i N_i(r,s) x_i`, con angoli naturali (-1,-1), (1,-1), (1,1), (-1,1).

La normale è `normalize(dx/dr × dx/ds)` e segue l'ordine della connettività; asse 1 tangente a r, asse 3 normale, asse 2 = asse 3 × asse 1. I triangoli usano coordinate baricentriche. Si rifiutano coordinate non finite, mappe degeneri, avvolgimenti incrociati e Jacobiani con proiezione incoerente. Per Q4 la proiezione del Jacobiano è affine: il controllo ai quattro angoli ne garantisce il segno sull'intero dominio naturale.

L'area è l'integrale del modulo del Jacobiano, in mm², con Gauss 2x2 composto e raffinamento 1,2,4,...,64 per direzione; tolleranza relativa predefinita 1e-8 tra raffinamenti successivi, errore esplicito se non converge. Il test del caso a sella confronta l'integrale con una triangolazione indipendente di 80.000 triangoli. La non complanarità è la massima distanza dal piano del triangolo di vertici di area maggiore, in mm; il rapporto con la massima distanza tra vertici è adimensionale.

`ValidateTopology()` mantiene il rifiuto dei quad non planari. L'overload con `supportsWarpedQuads = true` abilita la validazione geometrica bilineare; l'adattatore deve dichiarare il supporto della formulazione prima di usarlo. Non prova che un elemento shell di uno specifico solutore sia accurato per un dato livello di distorsione. Gli esempi 50/99 sono beam-only, quindi non sostituiscono questi test dedicati.

## Unità e convenzioni

La base proposta corrisponde alla base già esistente: N, mm, rad, s, tonne, °C. La massa coerente è la tonnellata: `1 t * 1 mm/s² = 1 N`. `DomainUnits` riusa UnitsConvert, separa pressione dichiarata da forza/lunghezza² e distingue temperatura assoluta, differenza e gradiente.

| Grandezza | Base GPC | Esempio verificato |
|---|---|---|
| Momento | N mm | 1 kN m = 1e6 N mm |
| Force density q=N/L | N/mm | 1 kN/m = 1 N/mm |
| Rigidezza/tensione di membrana per larghezza | N/mm | Et in kN/m mantiene il valore numerico |
| Massa superficiale | t/mm² | 2 kg/m² = 2e-9 t/mm² |
| Densità | t/mm³ | massa/volume, distinta dal peso specifico |
| Accelerazione | mm/s² | 9,81 m/s² = 9.810 mm/s²; nessuna gravità implicita |
| Temperatura assoluta | °C | 68 °F = 20 °C |
| Differenza/gradiente | Δ°C; Δ°C/mm | 18 Δ°F = 10 Δ°C, senza offset 32 |
| Molla 6x6 | righe [N,N mm], colonne [mm,rad] | conversione di ogni blocco traslazionale/rotazionale |

L'adattatore deve convertire una sola volta. Arachne oggi mantiene coordinate e carichi nelle unità del ModelModel e riconverte le proprietà memorizzate in unità di default verso TensileLib. Il componente MaterialMembrane converte già g/m² in t/mm² (1e-12), N/5cm in N/mm (/50) e kN/m in N/mm (fattore 1). Questi percorsi vanno riscontrati con i dump F1, non applicando un fattore globale a tutti i double.

`NodeWeightAttributeModel` rappresenta pesi di penalizzazione delle coordinate, non peso gravitazionale. `NodePressureLoadModel` contiene forze nodali derivate dalla pressione sulla forma trovata; il successivo form-finding le esclude dagli input e ricalcola la pressione. Questa distinzione è conservata nella matrice. I modi Cable/ForceDensity/Membrane/Curvature non sono intercambiabili; pesi e residui richiedono una normalizzazione esplicita nell'adattatore.

Le convenzioni geometriche restano quelle di `Axes`, `ModelOrientation`, `ResultOrientation` e `ActionTransformations`: asse 3 della trave I->J, terna destra, lati delle sezioni espliciti. Le conversioni dei segni del solutore restano nel Converter. Il codice corrente di BridGHe mappa i momenti Straus `(BM1,BM2)` nei componenti GPC `(BM2,-BM1)` prima della trasformazione al riferimento delle verifiche: questo è un comportamento del codice congelato, non una nuova prescrizione normativa. Il pilota deve dimostrarne l'equivalenza anche con assi ruotati, eccentricità e inversione I/J.

## Verifiche e passaggio successivo

Esiti e misure finali sono registrati in `F2-VERIFICHE.md` e `f2-evidenze.json`. I test aggiunti comprendono copie isolate, contratti XML indipendenti, bit dei double, connettività condivisa, conflitti e rimappature, gerarchie di gruppi, mappe persistite, unità, geometria e metadati di ponte. Le suite complete e gli audit devono risultare verdi sul commit conclusivo prima del passaggio a F3. Nessuna riduzione del target prestazionale di F2 è implicita nell'esistenza del nuovo codice.
