# F7 — prestazioni

Stato al 4 novembre 2026, sera. Tabelle prima e dopo, master contro F6. Le fonti sono le note `correzioni-L5.md`, `correzioni-L9.md` (§11 e §12), `correzioni-F7a.md`, `correzioni-L10.md` e `correzioni-P2B.md`, nella cartella `C:\Users\g.pacini\Documents\Repository\gpc-unification-20261102\revisione-claude-20261103`. Lo stato generale è in [STATO.md](STATO.md).

## Come leggere le misure

**Host di ogni misura:**

| Misure | Strumento | Host |
|---|---|---|
| Arachne, documento GH | `Arachne.Compare --io` (rete k×k con 3 aste per nodo: Beam → Force Density → Build Model, poi Form Find lineare con mesh) | .NET Framework 4.8, processo senza interfaccia con Rhino e Grasshopper caricati. I tempi dei componenti sono il `ProcessorTime` di Grasshopper |
| Arachne, dettaglio del Form Find e copie | `Arachne.ExampleWriter --migration-scale` | .NET Framework 4.8, senza interfaccia |
| BridGHe, esempi 50 e 99 | `BridGHe.Smoke` (net8.0-windows), runner nativi | Rhino 8 su .NET 8, avviato senza finestra |
| D3 sull'esempio 50 (L10) | programma di banco sui binari di `BridGHe.Smoke` | .NET 8 fuori da Rhino, e Rhino 8 su .NET 8 (Rhino.Inside, finestra nascosta) |
| Archivi di Model (L5) | `tools/CompactArchiveProbe` | .NET Framework 4.8 |
| `GPC.Archive` e sidecar (L3) | `tools/ExternalArchiveProbe bench` | .NET 8 |
| ANTHEA 1.19.0 (F7a) | `ModelWorkspace.Checks --archive-timing` | .NET 8, Release, senza Rhino |

**Attenzione:**
- La macchina era condivisa con le altre corsie: build e prove in parallelo. Le misure prese in momenti diversi non si confrontano bene; si confrontano le colonne misurate in sequenza alternata.
- La licenza di Rhino (Cloud Zoo) è caduta più volte. Nessun numero viene da un run fallito.
- **Nessuna misura di Arachne in Rhino 8 su .NET 8.** Rhino 8 carica i plugin su .NET 8 per impostazione predefinita: lì la compattazione dei risultati costa circa 1 s a 10.000 nodi (misura di L5 fuori da Rhino), contro 2,0 s su .NET Framework.

## Arachne: master contro F6

### Tabella principale (L9 §11.2)

Master = Arachne `39c54a2`. F6 = Model `b2bf3d54` + Arachne `b50cddb`, che contengono L5 e L9. Run alternati fra le 16:12 e le 16:30 del 4/11, macchina quasi scarica. A 60.025 nodi la riapertura del modello di master è spenta (`ARACHNE_IO_REOPEN=0`): master non rilegge il proprio modello a quella scala.

| Misura | 1.024 nodi: master | 1.024: F6 | 10.000: master | 10.000: F6 | 60.025: master | 60.025: F6 |
|---|---:|---:|---:|---:|---:|---:|
| Build Model, soluzione intera | 0,66–0,67 s | 0,34–0,54 s | 6,05–7,01 s | 3,52–4,53 s | 44,9 s | 26,2–30,8 s |
| — Beam | 0,23–0,26 s | 0,05 s | 2,36–2,67 s | 0,29–0,37 s | 15,8 s | 2,1–3,5 s |
| — Force Density | 0,003 s | 0,030–0,035 s | 0,10–0,12 s | 0,37–0,55 s | 1,2 s | 2,3–2,6 s |
| — Build Model | 0,40–0,43 s | 0,25–0,45 s | 3,55–4,17 s | 2,84–3,59 s | 27,9 s | 21,6–24,5 s |
| Duplicate di `GH_Model` | 0,004 s | 0,06–0,10 s | 0,10 s | 1,00–1,71 s | 1,4 s | 6,3–7,7 s |
| Modello internalizzato nel `.gh` | 340.068 B | 313.200–313.286 B | 3.246.842 B | 2.986.406–2.987.304 B | 19.535.628 B | 17.843.377–17.849.996 B |
| — salvataggio | 0,76–0,87 s | 0,19–0,31 s | 8,19–8,63 s | 1,09–2,06 s | 63,8 s | 7,9–10,1 s |
| — apertura | 0,59–0,77 s | 0,15–0,20 s | 30,4–33,4 s | 0,78–0,98 s | non riletto | 5,2 s |
| Form Find | 0,63–0,72 s | 0,90–1,35 s | 6,82–7,28 s | 10,7–17,3 s | 54,0 s | 59,8–78,3 s |
| — sidecar del Form Find | — | 1,68 MB | — | 16,73–16,75 MB | — | 102,1 MB |
| — salvataggio* / apertura del documento | 0,22–0,26 / 0,14 s | 0,62–1,03 / 0,12–0,14 s | 2,06–2,11 / 0,73–0,78 s | 6,03–10,90 / 0,84–0,97 s | 17,1 / 5,5 s | 37,2–45,4* / 4,2–10,9 s |

\* Il salvataggio subito dopo il run aspetta la preparazione del sidecar in background.

Nota: il Form Find a 60k di F6 alle 16:28 non ha scritto il sidecar, perché il disco C: si stava riempiendo. Rifatto alle 17:22 con il disco libero: sidecar di 102.056.553 B, salvataggio 37,2 s, apertura 10,9 s.

### Dopo l'ultimo giro di L9 (§12.3)

Master = `39c54a2`. F6 prima = la colonna F6 sopra. F6 dopo = Model `9a91a9b9` (writer riusati) + Arachne `611a3d8` (sidecar preparato durante la proiezione). Run alternati fra le 18:10 e le 18:45, macchina quasi scarica, licenza stabile.

| Misura | master | F6 prima | F6 dopo |
|---|---:|---:|---:|
| 10k: Form Find | 6,61–8,12 s | 9,02–11,59 s | 8,86–9,06 s |
| 10k: salvataggio del documento subito dopo il run* | 2,01–2,11 s | 5,12–5,42 s | 3,05–3,26 s |
| 10k: Force Density | 0,080–0,094 s | 0,30–0,32 s | 0,24–0,32 s |
| 60k: Form Find | 54,0 s | 59,8–78,3 s | 55,2 s |
| 60k: salvataggio del documento subito dopo il run* | 17,1 s | 37,2–45,4 s | 21,9 s (sidecar 102,1 MB) |
| 60k: Force Density | 1,2 s | 2,3–2,6 s | 1,9 s |

\* Aspetta la parte della preparazione del sidecar non ancora finita.

Dettaglio del Form Find a 10k, F6 dopo (`--migration-scale`, .NET Framework):

| Passo | Tempo | Memoria allocata |
|---|---:|---:|
| grafo d'analisi | 1,1 s | 256 MB |
| impronta d'analisi | 1,4 s | 756 MB |
| export | 1,7 s | — |
| solve | 1,4 s | — |
| attacco dei risultati | 0,3 s | — |
| proiezione delle uscite | 3,4 s (prima 5,1–5,9 s) | — |
| — di cui compattazione dei risultati | 2,0 s (prima 2,7 s) | 1.180 MB (prima 2.340 MB) |
| preparazione del sidecar, in parallelo alla proiezione | 4,7 s | 3.875 MB (prima 4.840 MB) |

### Prima e dopo, corsia per corsia, a 10.000 nodi

Da CORREZIONI.md. Per master: valori di L5 e, dopo il punto e virgola, quelli di L9. P2A = Arachne `3992879`.

| Misura | master `7b1108d` | dopo P2A | dopo L5 | dopo L9 (primo giro) |
|---|---|---|---|---|
| Build Model, soluzione intera | 8,23 s; 5,44–8,06 s | 11,33 s | 10,46 s | 3,83–3,96 s |
| Componente Beam | 2,95 s; 1,81–2,77 s | — | 2,32 s | 0,23–0,29 s |
| Componente Force Density | 0,11 s; 0,09–0,16 s | — | 2,76 s | 0,48–0,64 s |
| Duplicate di `GH_Model` | 0,10 s; 0,09–0,22 s | 1,72 s | 1,16 s | 1,06–1,12 s |
| Modello internalizzato nel `.gh` | 3.246.842 B | 30.596.631 B | 2.985.807 B | 2,99 MB |
| Salvataggio / apertura del `.gh` | 9,33 / 39,08 s | 15,69 / 7,50 s | 1,33 / 0,66 s | 1,22–1,71 / 0,60–0,84 s |
| Form Find | 7,18 s; 6,58–8,14 s | 12,17 s | 12,04 s | 10,38–12,32 s |
| Sidecar del Form Find | — | non scritto (limite di 256 MB) | 16.745.226 B | 16,74–16,76 MB |

### Copie con proprietà e sistemi di riferimento condivisi (L9 §10.4)

Copia del modello (`--migration-scale` con `MIGRATION_SCALE_COPIES_ONLY=1`, .NET Framework):

| Modello | copia completa | solo proprietà condivise | proprietà e sistemi condivisi |
|---|---:|---:|---:|
| 10.000 nodi, 59.203 elementi | 1,17–1,48 s, 309–312 MB | 1,20–1,25 s, 309 MB | 0,80–1,17 s, 204 MB |
| 60.025 nodi, 358.193 elementi | 8,9–10,1 s, 1,90 GB | 7,2–7,9 s, 1,90–1,96 GB | 5,3–5,8 s, 1,33 GB |

Duplicate di `GH_Model` (`Arachne.Compare --io`): a 10k da 1,05–1,12 s a 0,93–1,00 s; a 60k da 7,97 s a 6,04 s. Build a 60k da 24,2 s a 21,4 s.

## Archivi

### Archivi di Model (L5, `tools/CompactArchiveProbe`, .NET Framework 4.8)

Rete di 10.000 nodi e 59.203 elementi, uno spostamento per nodo.

| Archivio | XML: salva / apre / dimensione | Compatto: salva / apre / dimensione |
|---|---|---|
| `ModelArchive` senza risultati | 17,7 / 7,9 s / 44,1 MB (pacchetto) | 0,84 / 0,74 s / 3,94 MB |
| `ModelArchive` con risultati | 14,4 / 8,6 s / 55,6 MB | 1,85 / 1,08 s / 5,32 MB |
| `ModelValueArchive` (proprietà di trave) | 23,2 KB | 1,5 KB |

### `GPC.Archive` e sidecar (L3, `tools/ExternalArchiveProbe bench`, .NET 8)

30.000 nodi × 10 casi, 5 run, sidecar di 482 MB.

| Operazione | Prima (`72274c83`) | Dopo (`62508fef`) |
|---|---:|---:|
| Primo salvataggio | 1,71 s | 0,97 s |
| Salvataggio invariato (autosave) | 0,72–1,35 s | 0,18–0,20 s |
| Salvataggio invariato in una sessione riaperta | 0,75–0,91 s | 0,24–0,40 s |
| Salvataggio con una run nuova | 2,96 s | 2,10 s |
| Apertura di una run | 23,6 s | 4,4 s |

In ANTHEA (L3) `GPC.Archive` e il codice di 1.18.0 salvano nello stesso tempo, entro il rumore.

## ANTHEA 1.19.0 (F7a)

Banco dell'archivio dei risultati, mediana in ms (.NET 8, Release, 24 processori). Prima = ANTHEA `6fb72d56` (set AU con `GPC.Archive` di L4). Dopo = `18d96c67` con il set F7a. I banchi non hanno sezioni c.a.: misurano `GPC.Archive` e Persistence nuove.

| Fase | 1000x100 prima | 1000x100 dopo | 2000x50 prima | 2000x50 dopo |
|---|---:|---:|---:|---:|
| Salva (archivio + documento) | 265 | 218 | 198 | 230 |
| Salva senza modifiche | 181 | 190 | 1711 (minimo 158) | 198 |
| Secondo Salva (una combinazione in più) | 232 | 216 | 310 | 311 |
| Apertura del progetto | 101 | 93 | 88 | 80 |
| Primo accesso ai risultati | 2439 | 1344 | 1749 | 2337 |
| Salva dopo la riapertura | 193 | 178 | 159 | 143 |
| File (documento + archivio) | 14,85 + 40,2 MB | uguale | 8,81 + 51,1 MB | uguale |

Le differenze stanno nel rumore.

Sezioni c.a. nel formato compatto (una corsa, indicativa):

| File | Sezioni | XML (1.18) | Compatto (1.19) | Apertura XML | Salvataggio | Apertura compatto |
|---|---:|---:|---:|---:|---:|---:|
| 15 esempi distribuiti | 1 | 70–427 kB | 9–23 kB | 4–15 ms (133 ms il primo) | 11–22 ms | 1,4–4,9 ms |
| `progetto-1.18.0.programma` | 4 | 531 kB | 65 kB | 29,7 ms | 38,3 ms | 15,9 ms |

## BridGHe e D3

### D3 preparato una volta (L10, esempio 50)

66.960 righe, 498 travi, 40 casi, 38 azioni, metodo Compatible. Prima = chiamate singole (Checker `7948cc5f`); dopo = `Prepare` una volta e poi il piano (Checker `d85a9fda`).

| Misura | Prima | Dopo |
|---|---|---|
| `Prepare` | — | 0,099 s fuori da Rhino; 0,065 s in Rhino |
| `BuildInput`, un thread | 23,4 ms per riga (35,4 ms in Rhino) | 0,084 ms (0,077 ms in Rhino) |
| `BuildInput`, 24 thread, 66.960 righe | 752 s (461 s in Rhino) | 0,82 s |
| `BuildInput` + `Calculate`, 24 thread, 66.960 righe (il ciclo di C5) | 1.366 s fuori da Rhino; 1.202 s in Rhino | 214 s fuori da Rhino; 223 s in Rhino |
| Fasi e risultati di ogni riga | — | identici bit per bit, nello stesso host |

Fra fuori e dentro Rhino cambiano i risultati di 57.994 righe, nell'ultima cifra. È un effetto dell'host, già visto da P2B. Per questo prima e dopo si confrontano nello stesso host.

**BridGHe non usa ancora il piano.** `Super Element Check` passa al piano con C5.

### Esempio 50 in BridGHe (P2B, passo 5)

Rhino 8 su .NET 8, runner nativo, stesso set di librerie, macchina carica da altre corsie.

| Misura | Motore precedente | Motore GPC (Converter) |
|---|---:|---:|
| Analisi (5 modelli + mobile) | 61,7 s | 79,9 s |
| Verifiche | 107,9 s | 108,7 s |

Salvataggio del `.gh` del 50 con il formato attuale (payload FeMM `BGHA1`): 23,06 MB, serializzazione 12,3 s, scrittura 7,2 s, riapertura 16,5 s. **È solo la misura «prima»:** il formato finale dei tipi GH non c'è ancora.

## Altre misure

- **Aree a taglio (L6):** 13 sezioni da 4,10 a 0,44 s; cerchio Ø100 da 1,5 a 0,08 s; 50 travi da ponte una per una da 1,85 a 1,52 s; prova nativa delle BXS da 64–71 a 39 s.
- **Validazione del workflow D3 (L3):** 20.000 stati di sezione da 6,7 s a meno di 1,5 s.
- **Form Find a 10.000 nodi (P2A):** il run da 125,8 a 12,171 s; l'attacco dei risultati da 65 a 0,51 s, senza lo snapshot d'analisi.

## Che cosa resta più lento, e perché

### Arachne (.NET Framework)

| Che cosa | Quanto | Perché | Che cosa servirebbe |
|---|---|---|---|
| Duplicate di `GH_Model` | 10k: 1,0–1,7 s contro 0,1 s; 60k: 6,3–7,7 s contro 1,4 s | Il modello è una copia vera: si copia il grafo degli elementi (collezioni, assegnazioni, indici dei nomi). Master aveva un costruttore di copia con pochi campi | Condividere il modello. L'utente ha deciso di no: resta così |
| Force Density | 10k: 0,24–0,32 s contro 0,08–0,09 s; 60k: 1,9 s contro 1,2 s | Ogni trave si copia (3,9 KB). Fuori da Grasshopper costa 3,4–4,2 µs per trave, come master; dentro costa 8,6 µs per la raccolta dei rifiuti con lo heap grande di Rhino | Non copiare l'elemento (contro la decisione dell'utente), oppure un grafo d'elemento più leggero (cambia il contratto dei dati di GPCModel) |
| Form Find a 10k | 8,9–9,1 s contro 6,6–8,1 s (+1–2 s) | Impronta d'analisi (1,4 s, 756 MB); compattazione dei risultati (2,0 s), dominata dal `DeflateStream` per segmento di .NET Framework e dalle `SerializationInfo` dei risultati; due copie del grafo (circa 1 s) | Riusare nelle uscite i segmenti già codificati del grafo d'analisi: è una decisione sull'identità dei risultati. Oppure un'impronta incrementale, che cambia il contratto delle impronte |
| Salvataggio subito dopo il run | 10k: 3,1–3,3 s contro 2,0–2,1 s; 60k: 21,9 s contro 17,1 s | La preparazione del sidecar (4,7 s a 10k) dura più della proiezione (3,4 s): il salvataggio aspetta il resto | Non preparare la codifica dei risultati del grafo d'analisi nel sidecar: cambio di formato, oppure il riuso dei segmenti (stessa decisione di sopra) |

Il Form Find a 60k è alla pari con master (55,2 s contro 54,0 s). La proposta di Claude è misurare prima in Rhino 8 su .NET 8, poi decidere.

### BridGHe

- **Analisi dell'esempio 50:** +18 s (79,9 contro 61,7 s). Il motore GPC apre una sessione Straus7 per analisi, fa una copia contrattuale del modello e verifica l'import dei risultati.
- **Non ancora misurato:** salvataggio e apertura del `.gh` con i tipi GH su GPC e il file `BGHR2`; `Super Element Check` con il piano D3.
