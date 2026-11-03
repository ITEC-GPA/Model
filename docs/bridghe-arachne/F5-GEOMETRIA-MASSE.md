# F5: geometria autonoma, sezioni e partecipazione delle masse

Checkpoint del 3 novembre 2026. **F5 resta aperta**: la revisione completa dei campi e le lacune di implementazione individuate dalla revisione sono ancora da chiudere. Tutto rimane nel branch `codex/gpc-unification-20261102`.

## Contratti verificati

- `BridgeGeometry` rappresenta il ponte campionato prima dell'assegnazione FEM: nodi, assi dei membri, identità persistenti, appoggi, giunti, segmenti, terne, progressive, bordi e mesh dell'impalcato. Gli indici dei nodi sono locali alla geometria, da 1; quelli della mesh sono da 0. I quad conservano le coordinate non complanari. Connettività incoerente, identità duplicate e quad ripiegati sono rifiutati.
- `ModelValueArchive.Save/Load<T>` salva un valore autonomo del vocabolario chiuso GPC, senza creare un contenitore FEM artificiale. Usa il codec XML numerico condiviso. Il serializzatore XML standard di .NET Framework perdeva il segno di uno zero nelle terne: il codec di produzione lo conserva e mantiene l'impronta. Versione futura, tipo non registrato, root errata e DTD vengono rifiutati; gli stream del chiamante rimangono aperti.
- Le sezioni generiche possono conservare i moduli elastici minimi dichiarati, anche senza contorno geometrico. Non viene dedotta una classificazione resistente né un modulo plastico.
- `LoadCaseBase.InertiaMasses` rende esplicita la partecipazione di massa strutturale e non strutturale alle azioni inerziali. Il valore nullo mantiene la rappresentazione storica; le nuove selezioni entrano nella copia, nell'archivio e nell'impronta. Model schema 10 e ModelDocument schema 9 vengono scritti soltanto quando servono questi dati.

## Export e prove native

Il writer costruisce anche sezioni BXS da regioni geometriche GPC, incluse cavità e parti specchiate separate. Il modello temporaneo della sezione condivide il proprietario del runtime Straus7; i file temporanei creati dal writer vengono rimossi dopo l'assegnazione. Le proprietà meccaniche GPC sono riapplicate dopo la generazione geometrica.

Quattro prove native verificano il centro di taglio di un canale e le sezioni ruotata, cava e composta da due parti specchiate. Per queste ultime la sezione viene scalata integralmente nelle due direzioni da 1 a 2: `A(t)=A0(1+t)^2`, quindi `u=PL/(2EA0)`. Il confronto usa dieci punti di integrazione longitudinale e tolleranza relativa `5e-5`.

La selezione delle masse è verificata per tutte le quattro combinazioni dei flag. Il plate ha area `10 000 mm²`, spessore `10 mm`, densità `1e-9 t/mm³`, massa aggiunta `2e-8 t/mm²` e accelerazione `10 000 mm/s²`. I contributi alle reazioni sono rispettivamente `1 N` e `2 N`; risultano `0`, `1`, `2` e `3 N`, con tolleranza assoluta `1e-8 N`. Il test controlla anche la rilettura dei flag e la persistenza XML/package.

Il piano rifiuta l'associazione di una legge cutoff con una curva tensione–deformazione materiale: l'interazione non è definita dal writer.

## Gate del checkpoint

GPCModel **5.45.0.0**, Persistence **3.11.0.0**, Converter **2.17.0.0**. Incrementi per nuove funzionalità; baseline API e fixture storiche invariate.

- Model: **1.933 PASS**, zero fallimenti, 7 esclusioni; API e audit superati.
- Checker: **10.377 PASS** nelle 11 suite, zero fallimenti, una esclusione; API superate.
- [Evidenze e hash](f5-geometria-masse-evidenze.json).

La [matrice di revisione](f5-matrice-revisione.json) contiene tutti i 2.309 campi F2 e conserva esplicitamente gli stati aperti. Il generatore `tools/MigrationInventory/review_f5_matrix.py --check` fallisce finché rimangono campi aperti; [elenco operativo](f5-campi-da-risolvere.json). Questo checkpoint ne lascia **632** aperti. Le righe revisionate attestano il contratto di destinazione o una permanenza motivata nel plugin/solutore, senza attestare la migrazione dei componenti di produzione.

Restano in particolare l'associazione dei cedimenti imposti ai fattori dei freedom case, il completamento delle sorgenti di sezione native e la revisione delle leggi di rastremazione. Una sezione BXS scala tutte le coordinate; una forma standard Straus7 conserva invece gli spessori delle pareti: l'equivalenza non può essere assunta cambiando automaticamente rappresentazione. F6 deve inoltre rendere esplicite le discrepanze già rilevate nel vecchio exporter.
