# F5 — dominio ed exporter Straus7

**F5 completata il 3 novembre 2026** nel perimetro del [piano](PIANO.md): matrice chiusa, contratti registrati, exporter verificato e tutte le suite prescritte superate. [Evidenze, hash e test esclusi](f5-conclusione-evidenze.json). F6–F8 restano da eseguire.

## Perimetro e matrice

La revisione di [F2](F2-CONTRATTI.md) copre **295 tipi e 2.309 campi**, compresi **33 tipi senza campi propri**. La [matrice](f5-matrice-revisione.json) distingue 236 campi di dominio già verificati, 1.395 destinazioni riesaminate e 678 campi mantenuti nel proprietario appropriato: **nessuna voce aperta**. Le categorie indicano decisioni sui contratti, non componenti Grasshopper già migrati. L'inventario Roslyn ricalcolato coincide con quello congelato in F2, inclusi gli hash dei sorgenti.

Le destinazioni comprendono Model, Checker, Converter e i wrapper/algoritmi che il piano mantiene nei plugin o in TensileLib. I valori fisici non rappresentabili sono diagnosticati; conservarli come metadati non autorizza il solutore a ignorarli. Il writer rifiuta `PreservedSourceData` non riconosciuti, ammettendo soltanto le categorie di provenienza dei risultati prodotte da Converter.

## Completamenti rispetto al checkpoint geometria e masse

- **Spostamenti imposti:** associazione esplicita tra caso di carico e freedom case; controllo dei fattori per gli incrementi delle fasi. GPC conserva le rotazioni in rad, `St7SetNodeRestraint6` riceve gradi e i risultati nativi tornano in rad. Il caso analitico verifica traslazione, rotazione, reazioni e fasi con fattori 0,5 / 1 / 0.
- **Rilasci:** corretta la corrispondenza dei codici R3 (`Released=0`, `Fixed=1`, `Partial=2`). Verificata la somma delle cedevolezze della trave e delle molle di estremità, sia assiale sia torsionale.
- **Sezioni native:** catalogo standard/BGL e risoluzione delle librerie BSL/BGL; snapshot autonomi con dimensioni espanse o bytes BXS incorporati. Codec esplicito versionato, copie difensive, fingerprint e configurazione di integrazione conservati con i risultati. Le librerie e il file BXS originario non sono necessari per riesportare uno snapshot risolto.
- **Rastremazioni:** controllata la cedevolezza assiale contro l'integrale analitico di `1/(EA(x))`. Per RHS standard lo spessore resta costante; BGL scala l'intero contorno. Proprietà, dimensioni e convenzione dell'angolo principale in rad sono controllate mediante rilettura nativa.
- **Materiali:** `MaterialDeclarations` conserva resistenze dichiarate/attese, lega e processo produttivo dell'alluminio, senza sostituire la legge costitutiva o la resistenza di progetto. Schema materiale 8 solo quando i dati sono presenti; impronte precedenti invariate quando null.
- **Sezioni composte:** `CompositeSectionDefinition` conserva la sezione fisica e i rapporti modulari; il calcolo delle proprietà omogeneizzate opera su una copia. Archivio autonomo, fingerprint e isolamento verificati sui due runtime.
- **Checker:** trasferiti i calcoli di coefficiente di viscosità, ritiro e azioni equivalenti da ritiro. Verificati contro i valori congelati di BridGHe e un equilibrio analitico di due rettangoli. È una conservazione del comportamento del metodo esistente, non una nuova validazione normativa generale. La nuova API delle azioni da ritiro usa già **N, mm, MPa**, diversamente dai report di verifica che possono usare kN e kN m.
- **Formulazioni e assi:** conservati i tipi nativi PointContact, Pipe e Connection; gli assi locali espliciti vengono scritti anche per plate isotropi. I test di queste tre formulazioni attestano tipo/proprietà e archivio, non una validazione completa di ogni loro comportamento non lineare.

I contratti già verificati nei checkpoint precedenti restano inclusi: [exporter](F5-EXPORTER.md), [fasi](F5-FASI.md), [dominio esteso](F5-DOMINIO-ESTESO.md), [geometria e masse](F5-GEOMETRIA-MASSE.md).

## Vincoli espliciti per F6

1. Il vecchio input GH delle rotazioni imposte non indicava l'unità e veniva passato direttamente all'API in gradi. L'adattatore deve convertire questa convenzione effettiva in rad, registrando la provenienza. Più casi indipendenti richiedono freedom case distinti e fattori di fase espliciti; il writer rifiuta la combinazione ambigua.
2. Il getter/setter pubblico legacy `ShearUltimateStrength` di BridGHe accede al campo della resistenza a trazione, mentre l'archivio conserva un campo separato per il taglio. F6 deve preservare il valore archiviato e diagnosticare la discordanza; nessuna correzione silenziosa del sorgente o delle fixture è stata fatta in F5.
3. Il writer legacy trattava ogni opzione di mirror diversa da zero come `mtLeft`. Le geometrie GPC espanse possono essere esportate via BXS; la cattura di una sezione nativa con mirror non espanso viene rifiutata. F6 deve distinguere geometria richiesta ed effetto realmente prodotto dalla sorgente.
4. Le formulazioni legacy SpringDamper/UserDefined prive di legge fisica, i supporti LinkProperty/piani custom non supportati dalla sorgente e le topologie Tri6/Quad8/Quad9 non vengono degradati a un elemento diverso. Restano diagnostiche esplicite secondo la matrice. I profili variabili richiedono la suddivisione prevista dai contratti; non viene inventata una legge di interpolazione.
5. I campi legacy inattivi o ambigui (ad esempio `TemperatureOnTime`, cache di sezione e alcuni dati di load case) restano dati di compatibilità. F6 non deve attivarli automaticamente. Gli offset verticali delle sezioni composte già rifiutati dalla factory sorgente non vengono reinterpretati come eccentricità orizzontali.

## Verifiche e versioni

| Gate eseguito sul set finale | Superati | Fallimenti | Esclusi |
|---|---:|---:|---:|
| Model.Core.Tests (.NET 6) | 63 | 0 | 0 |
| Model UnitTest (.NET 6, incluse prove Straus7 R31) | 1.793 | 0 | 7 |
| Model.NetFramework.Tests (.NET Framework 4.7.2) | 96 | 0 | 0 |
| Checker, tutte le 11 suite | 10.382 | 0 | 1 |

Totale Model: **1.952 PASS**. I sette test esclusi richiedono campioni esterni o l'abilitazione di una misura temporale; l'esclusione Checker è una misura temporale opt-in. Non sono conteggiati come superati. Nomi e motivazioni sono riportati nelle evidenze. I nuovi test nativi F5 risultano eseguiti e superati, comprese le due varianti di spostamento imposto, le due leggi di rastremazione, le due librerie e il BXS autonomo.

`Model/build/Verify.ps1` e `Checker/build/Verify.ps1 -Suites all` sono terminati con **exit code 0**. API: Model 9.942 firme, Persistence 38; Checker 622 / 2.633 / 3.431 / 9, senza rimozioni inattese. EqualityAudit superato su 491 file (9 usi esaminati, 8 voci ammesse). Le 26 rimozioni Model già approvate rimangono quelle della baseline esistente.

Per riprodurre i gate usare un nuovo percorso di risultati sotto `artifacts/f5`, senza sovrascrivere le evidenze. Model richiede `GPC_STRAUS7_WRITE` verso tale cartella, `GPC_STRAUS7_BRIDGE_PILOT` verso il pilota F4 `BridGHe/artifacts/f4/bridge-20261103-092142`, `GPC_STRAUS7_ELEMENTS=versioned` e `GPC_STRAUS7_STAGES=versioned`, con Straus7 R31 e licenza disponibili. Checker riceve `-ModelRoot` verso questo checkout e `-Suites all`; lo script cancella la cartella passata a `-Results`, che deve quindi essere nuova e risolta esplicitamente nel workspace.

La matrice si ricontrolla con `py -3 -X utf8 tools/MigrationInventory/review_f5_matrix.py --check`. Log e TRX finali: `artifacts/f5/model-f5-final-gate` e `artifacts/f5/checker-f5-final-gate`, con i rispettivi log nella cartella superiore. Gli hash SHA-256 dei risultati, dei log e dei sorgenti modificati verificati sono nel rapporto JSON.

Versioni: **GPCModel 5.46.0.0**, **GPC.Model.Persistence 3.12.0.0**, **Converter 2.18.0.0**, **Checker.CompositeBridge 1.26.0.0**. Nuove funzionalità: incremento del secondo componente secondo gli AGENTS.md. GPC.Archive resta 1.0.0.0.

Branch isolato `codex/gpc-unification-20261102`. F6 conserva la migrazione dei componenti; F7 l'integrazione estesa e F8 il rilascio coordinato. Le baseline API e le fixture restano invariate; nessun merge, push o aggiornamento dell'installazione Rhino in questa fase.
