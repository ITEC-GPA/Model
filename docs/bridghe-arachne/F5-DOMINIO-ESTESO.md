# F5 — completamento del dominio e collaudi meccanici

Prosecuzione del 3 novembre 2026 nel branch `codex/gpc-unification-20261102`. Versioni in lavorazione: GPCModel **5.44.0.0**, Persistence **3.10.0.0**, Converter **2.16.0.0**. Il checkpoint precedente resta descritto in [F5-EXPORTER.md](F5-EXPORTER.md). **F5 resta aperta finché la matrice semantica F2 e tutti i gate richiesti non sono chiusi.** Non è un rilascio e non certifica la migrazione dei componenti F6.

## Contratti aggiunti

- `BridgeDefinition`: allineamento GPC, appoggi e obliquità, travi, impalcato e pendenze, suddivisione di officina, diaframmi e mesh. `BridgeSectionRule` e `BridgeFamilyProperty` conservano selettori, proprietà e scale dimensionali. I descrittori sono autonomi; la generazione Rhino e la sostituzione dei componenti appartengono a F6.
- `MaterialAnalysisData`: modulo G esplicito per le travi, smorzamento viscoso e modale, conducibilità e calore specifico, legge uniaxiale con risposta e incrudimento dichiarati. L'energia del dominio N mm è convertita in J per Straus7. Una curva uniaxiale non viene usata per inventare plasticità ortotropa.
- Membrane per unità di larghezza: export delle rigidezze N/mm e della massa t/mm² con spessore numerico di riferimento di 1 mm, senza attribuire uno spessore fisico al tessuto. Restano espliciti i rifiuti di dati termici o precompressioni in MPa senza una corrispondente legge per unità di larghezza.
- Carichi mobili: sorgenti termiche, legge centrifuga con dialetto e unità espliciti, flag per singola forza, selezione degli elementi tramite GUID. `Straus7MovingLibrary.Resolve` espande le voci installate in dati autonomi e registra la provenienza.
- `ShellSurfacePressure`: pressioni globali e normali sulle due facce esterne, con proiezione per risultante o componenti. La pressione normale è positiva verso l'interno della faccia e ne segue la direzione. La pressione globale mantiene la direzione globale.
- `BeamCutoffLaw`: limiti assiali in N, risposta fragile/duttile e conservazione della massa. Formulazione `CutoffBar` esplicita; aggiunta la formulazione `ShearPanel` per i plate.

I nuovi dati di pressione e i carichi mobili estesi usano schema Model 9 / ModelDocument 8. Il materiale con `AnalysisData` usa MaterialVersion 7. Le proprietà elastiche con cutoff o shear panel usano versione 2; le proprietà precedenti continuano a scrivere versione 1. Copia, archivio e fingerprint sono dichiarati; gli schemi futuri sono rifiutati.

## Export e risultati

L'export delle rastremazioni ora trasferisce anche la geometria dimensionale della sezione: il solo attributo di taper non consentiva il calcolo. Sono riconosciute le forme standard implementate nel writer; le altre vengono rifiutate durante `Prepare`. Il numero di punti di integrazione longitudinale è un'opzione numerica per GUID di trave. I centri di taglio sono trasformati dalle coordinate della sezione agli assi principali, conservandone l'eccentricità.

I collegamenti rigidi trasferiscono le equazioni di corpo rigido. Equazioni modificate o bracci non più coerenti con la geometria sono rifiutati prima della scrittura. I vincoli generali MPC e i link elastici non sono assimilati a collegamenti rigidi.

`Straus7ModelRun` importa ora anche casi mobili generati e risultati delle fasi non lineari a piccoli spostamenti. Per più casi mobili genitori la corrispondenza deve essere esplicita. Le fasi conservano incrementi, storia, cumulatività e semantica dei totali; non vengono convertite in casi lineari combinabili. Evidenze e mappe sono preparate prima di modificare i risultati GPC.

## Riscontri numerici eseguiti

| Controllo | Riscontro |
|---|---|
| Rastremazione rettangolare assiale 1→2 | `u = PL/(EA0) ln(2) = 6,9314718056 mm`; 10 punti di integrazione, tolleranza relativa `1e-5` |
| Materiale bilineare, E=1000 MPa, Et=100 MPa | A=100 mm², L=1000 mm, P=1000 N: `u=55 mm`, tolleranza `1e-5 mm`; tolleranze iterative `1e-8` |
| Barra cutoff in parallelo a barra elastica | Limite 500 N, carico finale 2000 N: `u=15 mm` duttile, `u=20 mm` fragile |
| Pannello a solo taglio, bordo caricato rigido | G=100000 MPa, t=10 mm, lato=100 mm, P=1000 N: `u=0,001 mm` |
| Sezione a C e centro di taglio eccentrico | Offset riletto e torsione `θ=PeL/(GJ)` verificati |
| Collegamento rigido eccentrico | Equazioni `u_slave=u_master+θ×r`; reazione assiale e momento verificati |
| Pressioni su plate inclinato | Equilibrio globale e rilettura di entrambe le facce; tre modalità di proiezione |
| Membrane | Due orientamenti degli assi, con proprietà ordinarie e per unità di larghezza |
| Librerie mobili e risultati | Espansione e riesportazione di due voci; casi generati e fasi normalizzati e riaperti da archivio |

Gli attesi analitici dei collaudi meccanici sono mantenuti. Nessuna fixture o baseline preesistente è rigenerata. Gate completi superati: **Model 1.916 PASS, 7 esclusioni** (63 Core, 1.764 UnitTest, 89 .NET Framework); **Checker 10.377 PASS, 1 esclusione**, tutte le 11 suite. Compatibilità API e audit superati. Il precedente tentativo aveva una sola omissione nel contratto fingerprint di `BridgeDefinition.schema`, corretta e ricollaudata. [Evidenze e SHA-256 dei log](f5-dominio-esteso-evidenze.json).

## Difetti legacy identificati

- Il vecchio writer passava un solo double a `St7SetPlateNormalPressure2`, che richiede due valori ordinati -z/+z. Il nuovo writer trasferisce entrambi; F6 deve diagnosticare questa differenza e non riprodurre una lettura fuori array.
- La API R31 non imposta effettivamente G dello shear panel tramite il solo setter isotropo. Il trasferimento passa da una proprietà shell ortotropa e poi cambia formulazione. Il percorso diretto e quello corretto sono stati confrontati sullo stesso modello vincolato: il primo produce una matrice singolare, il secondo la soluzione analitica.
- Il writer legacy non applicava `MaterialModel.Mode/Hardening`. L'adattatore F6 deve distinguere la legge effettivamente esportata dalle opzioni memorizzate; non attivare implicitamente una diversa risposta costitutiva.

Le fonti primarie consultate sono il manuale API e la guida R31 installati. Estratti e log sono in `../artifacts/f5` rispetto alla radice del repository Model, con directory separate per ogni esecuzione. [L'overlay F5](f5-destinazioni-parziali.json) contiene ora 278 righe di delta esplicitamente riscontrate su 2309: il resto richiede revisione semantica, non necessariamente nuovo codice (comprende anche cache, preview, alias e interni del solutore).
