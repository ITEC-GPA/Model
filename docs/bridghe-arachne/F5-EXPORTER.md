# F5 — esportazione del modello e workflow di verifica

Avanzamento del 3 novembre 2026 nel branch `codex/gpc-unification-20261102`.
Questo documento descrive il nuovo percorso verificato; non chiude ancora il gate della matrice F5 e non dichiara migrati i componenti di produzione F6.

## Contratti e scrittura

`ElasticBeamProperty` e `ElasticPlateProperty` descrivono le proprietà elastiche necessarie al calcolo, comprese le piastre ortotrope e i load patch senza materiale o rigidezza fittizi. `RestrainAssignment.FreedomCase` separa il caso dei vincoli dal caso di carico storico. String group e distribuzione dei load patch hanno contratti espliciti. Copia, archivio, uguaglianza e impronte comprendono questi dati; gli input precedenti senza le nuove assegnazioni conservano il proprio schema.

Versioni: Model **5.43.0.0**, Persistence **3.9.0.0**, Converter **2.15.0.0**. Gli input con i nuovi contratti richiedono schema Model 8 / ModelDocument 7; il reader rifiuta schemi futuri.

`Straus7ModelWritePlan.Prepare` verifica l'input prima delle chiamate native e prepara operazioni separate da eseguire tramite `IStraus7ModelWriteApi`, `IStraus7WriteApi` e le interfacce specialistiche. Scrive nodi, elementi, casi, gruppi, proprietà, assi, eccentricità, vincoli, rilasci e azioni, poi assegnazioni fisiche, carichi mobili e fasi. Gli ID nativi sono indipendenti dagli ID del modello; `SolverIdentityMap` conserva la corrispondenza per GUID e famiglia. La scrittura rifiuta piani superati e destinazioni esistenti.

Le azioni distribuite o concentrate `NodeToNode` sono trasportate alla linea nativa della trave con il momento dell'eccentricità. Le pressioni uniformi richiedono copertura dell'intero elemento: un poligono parziale non viene esteso implicitamente. I nodi Straus7 non appartengono a gruppi; i gruppi nodali GPC restano metadati organizzativi. Il gruppo radice nativo non è un falso gruppo di dominio e viene disattivato nelle fasi esplicite.

## Prove numeriche native

La suite dedicata comprende:

- mensola con forza concentrata e carico uniforme, confrontata con Euler–Bernoulli senza deformabilità a taglio;
- piastra membranale ortotropa con assi del materiale a 0° e 90°, confrontata con `u = PL/(Etb)`;
- incremento 0,5 → 1 → 0 con scarico completo;
- nascita di una seconda asta: Original e Reset ripartiscono il carico, Morph conserva scarica l'asta aggiunta;
- trasferimento della pressione da un load patch alle travi e rilettura dei relativi string group;
- confronto con entrambi i ponti congelati F4, comprese eccentricità e connettività invertita;
- momento della forza eccentrica riferita alla linea nodale e peso proprio con massa in tonnellate;
- carico mobile concentrato: reazioni e frecce confrontate con la soluzione della mensola a ogni posizione generata;
- esportazione, calcolo, importazione dei risultati normalizzati, archivio e riapertura, conservando gli identificativi sorgente.

La conversione statica R31 colloca il veicolo al centro delle suddivisioni. Su un percorso di 1.000 mm con quattro divisioni le posizioni osservate e verificate sono 125, 375, 625, 875 mm. Il comando non aggiunge automaticamente gli estremi e non costituisce una verifica dell'inviluppo continuo. Gli attesi iniziali del nuovo test, che supponevano cinque posizioni ai bordi, sono stati corretti dopo la diagnosi nativa; nessuna baseline preesistente è stata rigenerata.

`Straus7ModelRun` lega i file effettivamente prodotti alle rispettive SHA-256 e alla revisione GPC. L'importazione usa la mappa del solutore, mantiene `SourceIdentity` e `AnalysisSource` originali e registra le evidenze native nel modello. Il risultato riaperto dall'archivio conserva impronte, unità, casi e campioni. Il percorso automatico copre i casi primari lineari senza fasi o casi mobili generati; questi ultimi sono già collaudati tramite lettura nativa esplicita e richiedono ancora il collegamento al percorso automatico.

## Workflow Checker

Checker.CompositeBridge **1.25.0.0** aggiunge `BridgeWorkflowExecutor`: la ricetta persistente viene collegata a selettori esatti dei risultati GPC e trasformata negli input dei motori compatibile e storico lineare già esistenti. Coefficienti per GUID, revisione delle sorgenti, proprietà fisica della sezione, unità N/mm, terna, punto di riduzione, stazione e lato sono controllati esplicitamente. Sono conservati attivazione a carico nullo, rapporto modulare locale e ritiro primario.

Il collegamento rifiuta totali non lineari, stati ambigui, componenti mancanti, stazioni non disponibili e azioni fuori piano non trattate dal motore. Non interpola risultati né deduce una legge di viscosità dai giorni della fase. I 16 test di definizione/esecuzione comprendono il confronto numerico con gli stessi motori alimentati mediante fasi manuali.

## Lavoro residuo di F5

Restano da chiudere la matrice semantica campo per campo, i descrittori di generazione del ponte, i dettagli avanzati dei carichi mobili e le assegnazioni sorgente non ancora coperte. Il rifiuto esplicito degli input non supportati evita omissioni, ma non equivale alla loro implementazione. La migrazione dei componenti e la rimozione degli exporter locali restano F6.

Gate Model: 1.868 PASS e 12 esclusioni preesistenti; Checker: 10.377 PASS e un'esclusione preesistente. Tutti i controlli API e l'audit delle uguaglianze passano. La suite dedicata dell'esportatore comprende 16 test superati. Le SHA-256 di sorgenti, prove e binari congelati sono in `f5-exporter-evidenze.json`.
