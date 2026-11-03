# F5 — contratti fisici, workflow e primi writer nativi

Checkpoint del 3 novembre 2026. **F5 in corso; F6, F7 e F8 non completate.**
L'utente ha autorizzato la prosecuzione delle quattro fasi, mantenendo i branch separati.
Questa è una consegna intermedia verificata, non un rilascio del nuovo modello dei plugin.

## Prosecuzione: fasi costruttive

Il checkpoint successivo è descritto in [F5-FASI.md](F5-FASI.md): contratti di attivazione e incrementi, writer nativo delle fasi, controlli delle revisioni e del pilota ponte. Questo documento conserva i risultati del checkpoint `47c9b4df`; i relativi hash in `f5-evidenze.json` si riferiscono a quella revisione, non ai sorgenti aggiornati. La matrice corrente riscontra 103 campi su 2.309.

## Implementato

| Ambito | Risultato verificato | Limite attuale |
|---|---|---|
| Assegnazioni fisiche | Temperature nodali con ruolo, gradienti beam, forza/deformazione iniziale, tensioni normali iniziali plate, massa aggiunta con offset globale, supporti distribuiti, taper e richiesta di stazioni di output | Contratti e writer degli attributi; i componenti produttivi usano ancora il dominio precedente |
| Materiali | Elasticità ortotropa 3D con reciprocità e controllo di definitezza positiva; membrana con rigidezze e resistenze per unità di larghezza | Nessun valore di spessore fittizio dedotto; esportazione costitutiva ortotropa completa ancora da integrare |
| Form-finding | Curvatura con corda esplicita, metodo lineare iterativo, target con segno, obiettivi indipendenti e diagnostica dei conflitti | Adattatore Arachne opt-in; preprocessore e componenti produttivi da migrare |
| Workflow ponte | Ricetta Checker con GUID di elementi/casi, revisione delle sorgenti, stati locali e controllo di duplicazione degli incrementi | Le procedure di assemblaggio delle fasi e di verifica del vecchio workflow non sono ancora trasferite |
| Carichi mobili meccanici | Percorsi, veicoli, forze puntuali/distribuite, corsie, fattori di contemporaneità, identità e archivio GPC; writer nativo e rilettura | Sorgenti termiche, centrifuga, template di libreria, set nativi e collaudo dei casi statici generati ancora da completare |

`IStraus7WriteApi` e `IStraus7MovingWriteApi` sono confini verificabili per queste porzioni di export.
I relativi piani validano gli input prima delle scritture, distaccano i dati mutabili e rifiutano revisioni superate.
Il writer opera su un nuovo file o su una copia, non sovrascrive output esistenti e non modifica il file sorgente.
Non è ancora l'exporter completo di topologia, proprietà, carichi, fasi e risultati.

## Convenzioni riscontrate

- Base del dominio: N, mm, s, t, °C. Temperature assolute in °C; gradienti in Δ°C/mm.
- Supporti distribuiti beam in N/mm² e plate in N/mm³; la selezione nativa è un **freedom case**, non un load case.
- Le tre componenti del precarico plate sono `sigma11`, `sigma22`, `sigma33`, in N/mm². La terza non è `tau12`; il writer dei gusci rifiuta un valore non nullo.
- Masse plate in t/mm²; offset in assi globali. Rigidezze e resistenze di membrana in N/mm.
- Output stations è una richiesta al lettore dei risultati, non un attributo scritto nel modello Straus7.
- Il carico mobile distribuito è in N/mm lungo x; la larghezza dell'impronta ripartisce la medesima risultante, non la moltiplica.
- I fattori multicorsia si applicano per numero di corsie attive oppure per ordine di gravosità; non identificano la posizione fisica della corsia.
- La conversione statica Straus7 esclude i carichi floating: il nuovo piano la rifiuta esplicitamente quando li contiene.

Fonti: API R3 installata (`St7API.cs`, manuale API), help locale `St7RefManR3.chm`,
[Layouts: Paths](https://www.strand7.com/straus7r3help/Content/Topics/LAYOUTS/LayoutsPaths.htm),
[conversione dei percorsi in casi](https://www.strand7.com/strand7r3help/Content/Topics/VISUALTools/ToolsConvertLoadPathsToLoadCases.htm),
[Plate Pre-Load](https://www.strand7.com/straus7r3help/Content/Topics/VISUALAttributes/PlateAttributesPreLoad.htm).
Gli estratti locali consultati sono negli artefatti F5; non sono redistribuiti nel repository.

Il vecchio exporter locale non impostava il ruolo della temperatura nodale, forzava una faccia per il supporto plate e trattava tale supporto nel ciclo dei casi di carico.
I nuovi writer rendono espliciti questi dati. La correzione del percorso produttivo richiede ancora il trasferimento e il confronto con le baseline.

## Persistenza e compatibilità

GPCModel `5.41.0.0`, GPC.Model.Persistence `3.7.0.0`, Converter `2.13.0.0`, Checker.CompositeBridge `1.24.0.0`.
Incrementi di funzionalità secondo AGENTS.md; GPC.Archive resta `1.0.0.0`; TensileLib resta il binario F1 `2.5.0.1`.

I nuovi tipi sono registrati nei vocabolari chiusi di archivio e fingerprint.
Model schema 6 e ModelDocument schema 5 sono usati solo quando necessari ai nuovi dati/obiettivi.
Material conserva la versione 5 per gli isotropi e usa 6 in presenza dell'ortotropia.
Gli archivi storici mantengono la loro rappresentazione; non sono state rigenerate fixture, attesi o baseline API.

Due tentativi intermedi del gate hanno evidenziato prima un disallineamento reader/writer della versione Material,
poi una riscrittura non necessaria dei materiali isotropi. Entrambi corretti nel codice; gate finale verde.

La [matrice parziale F5](f5-destinazioni-parziali.json) riscontra 93 campi dei 2.309 inventariati in F2.
È un overlay con evidenze e limiti: non dichiara quei campi già migrati nei componenti né chiude i restanti 2.216 campi.

## Collaudi

- Model: **1.827 PASS**, zero fallimenti; 14 ignorati nella suite ordinaria (12 già presenti e 2 nuovi test nativi opt-in).
- Writer Straus7: **8 PASS** nell'esecuzione dedicata, inclusi entrambi i test nativi ignorati nella suite ordinaria.
  Salvataggio, nuova sessione di lettura, confronto dei valori e SHA-256 del sorgente invariato.
- Checker: **10.369 PASS** nelle 11 suite, zero fallimenti; controlli API superati.
- Form-finding: due casi analitici con lunghezza/forza imposta, curvatura e caso a densità di forza di segno misto;
  limite di iterazione, conservazione degli input, copia/archivio e rifiuto dei risultati non convergenti.
- I quattro piloti membrana F4 restano equivalenti e salvano/riaprono i risultati senza variazione dei byte numerici.
- Arachne: 899 asserzioni nei piloti (36 nuove F5), riapertura dei 4 documenti in un processo separato;
  build predefinita ripristinata e **153 controlli PASS**, zero fallimenti.

La licenza inizialmente restituiva `St7Init -8`. Dopo l'intervento dell'utente, inizializzazione e test nativi riusciti.
Nessuna correzione al loader è stata necessaria per questo problema.

Il caso con tutte le densità negative produce una matrice ridotta non definita positiva: il backend CG fissato alla baseline fallisce esplicitamente.
Il contratto conserva i segni; il test risolvibile usa segni misti con rigidezza ridotta positiva.
Non viene affermato che TensileLib gestisca ogni sistema indefinito, né è stato modificato il solutore per questa migrazione.

Log, TRX e file nativi: `../../../artifacts/f5`. Identità delle prove in [f5-evidenze.json](f5-evidenze.json).

## Lavoro necessario per chiudere le fasi

1. **F5:** completare la matrice; coprire materiali, supporti e moving load avanzati; trasferire fasi e workflow eseguibile;
   portare l'exporter completo, inclusi load patch, string group, Morph/Rotate, output stations e rilettura dei risultati;
   collaudare i casi statici generati dai percorsi e la corrispondenza con le catture F1.
2. **F6:** sostituire i grafi di produzione di BridGHe e Arachne, preservare porte/GUID e persistenza delle identità;
   rimuovere l'exporter locale dopo l'equivalenza; eliminare BinaryFormatter in Arachne.
3. **F7:** set condiviso, coesistenza delle DLL nello stesso processo, ANTHEA/ReBear, archivi, autosave, memoria e scala.
   ReBear è stato individuato nel checkout locale `Repository/ConcreteSectionChecker` (master pulito al controllo), ancora non modificato né incluso in un worktree F7.
4. **F8:** set coerente con commit/versioni/hash, documentazione ed esempi definitivi, installazione pulita e rollback.

Tutto resta sul branch `codex/gpc-unification-20261102`; nessun merge, push o aggiornamento delle installazioni Rhino.
