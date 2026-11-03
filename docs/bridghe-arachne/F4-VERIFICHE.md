# F4 — Esito delle verifiche

3 novembre 2026. Piloti eseguiti sul branch separato `codex/gpc-unification-20261102`. [Perimetro, contratti e stima residua](F4-PILOTI.md); [manifest delle evidenze](f4-evidenze.json).

| Verifica | Esito |
|---|---|
| Ponte GH → GPC → Straus7 → archivio → GHX | 2 varianti, 511 controlli PASS |
| Membrana GH → GPC → TensileLib → archivio → GHX | 4 casi, 863 controlli PASS |
| Riapertura in nuovi processi Rhino | 2 GHX ponte e 4 GHX membrana; risultati binari, input e identità invariati |
| Model completo | 63 Core + 1.670 UnitTest + 46 Framework = **1.779 PASS**, 12 ignorati preesistenti |
| API e audit Model | PASS, nessuna rimozione API inattesa |
| Checker completo | **10.361 PASS**, 11 suite, zero fallimenti; API PASS |
| BridGHe predefinito | Build zero errori/avvisi; smoke **269 PASS** |
| Arachne predefinito | Build zero errori/avvisi; **153 PASS** (66 inventario, 38 form-finding, 43 patterning, 6 reti) |

I conteggi dei piloti sono asserzioni del banco prova, non verifiche di progetto strutturale. I 12 test Model ignorati sono gli stessi del gate F3; non sono conteggiati tra i superati. Checker mantiene i suoi avvisi preesistenti di compilazione.

## Evidenze native

- Ponte finale: `BridGHe/artifacts/f4/bridge-20261103-055300`. Contiene GHX, sidecar, hash separati di input/mappa/risultati, file Straus7/LSA dei percorsi baseline, GPC e riaperto, misure dei tempi.
- Membrana finale: `Arachne/artifacts/f4/membrane-20261103-055003`. Quattro GHX/sidecar e relativi hash; esito di convergenza, fingerprint, tempi e diagnostica in `case-*.txt`.
- Gate e log finali: `artifacts/f4` nella radice del workspace; TRX Model in `model-tests-final`, Checker in `checker-tests`.
- DLL effettivamente usate dai piloti: copie e SHA-256 in `artifacts/f4/binaries` e `build-libraries.json`. La DLL TensileLib resta quella congelata in F1; non è una ricompilazione del solutore.

Il ponte verifica anche l’invarianza degli spostamenti e delle reazioni globali dopo inversione I/J coerente con gli offset. La membrana confronta tutti gli spostamenti, le forze delle 40 aste e le reazioni; tutti i quattro casi convergono. I file GH conservano gli input dei componenti e un riferimento XML ai risultati esterni: nessun payload numerico dei risultati GPC viene inserito nel pannello.

## Tempi osservati

Sul ponte: conversione in GPC 118,7 ms alla prima chiamata e 2,7 ms nella seconda variante; copia/controlli/esportazione 38,0 e 2,9 ms. Straus7 richiede 2,37/1,84 s sul percorso precedente e 1,91/1,81 s sul percorso GPC. Preparazione/salvataggio 73,5/20,3 ms; riapertura e verifica 87,2/33,2 ms.

Sulla membrana il tratto misurato, dalla conversione a GPC fino al secondo calcolo dopo riapertura, richiede 2,089 s alla prima chiamata e 0,306/0,338/0,247 s nei casi successivi. Include due soluzioni confrontate, persistenza, riapertura e nuova soluzione; esclude la costruzione iniziale GH. I dati sono misure di questi piccoli piloti, non mediane di un benchmark né garanzie di latenza per modelli grandi. Il protocollo di scala resta quello documentato in F2.

## Correzioni emerse

1. Mancava un ingresso pubblico per le identità degli elementi/casi creati dai componenti. Aggiunti costruttori con GUID, senza mutare identità di entità assemblate.
2. I nuovi tipi Analysis dovevano usare la convenzione XML storica e dichiarare anche `value__` degli enum nel fingerprint. Le prove di riapertura e l’audit dei contratti hanno individuato entrambe le omissioni; corretti i contratti e ripetuto il gate completo. Nessun atteso o fixture precedente è stato rigenerato.
3. Il modello GH BridGHe può avere casi collegati ai carichi degli elementi senza registro globale compilato. L’adattatore li raccoglie da entrambe le sorgenti.
4. Il banco prova deve cancellare i valori persistenti predefiniti prima di impostare l’unità GH. Deve inoltre registrare gli errori prima della chiusura Rhino: la chiusura può restituire codice 0 anche dopo un’eccezione. Il successo richiede quindi il marcatore finale `F4 ... PASS`, oltre al codice di uscita. Le prime interruzioni non erano guasti del solutore Straus7.

Le prove preliminari fallite sono conservate nei log e nelle cartelle dei tentativi; non sono incluse nei conteggi finali.

## Riproduzione

Prerequisiti: stesso workspace isolato, SDK e dipendenze fissate, Rhino 8 e Straus7 R3 con licenza, DLL TensileLib congelata. Eseguire gli host Rhino in sequenza.

```powershell
$ws = 'C:\Users\g.pacini\Documents\Repository\gpc-unification-20261102'
Set-Location "$ws\BridGHe"
dotnet build tests/BridGHe.Smoke/BridGHe.Smoke.csproj -c Release -p:GpcPilot=true
& .\obj\bin\BridGHe.Smoke\Release\net8.0-windows\BridGHe.Smoke.exe --gpc-pilot

Set-Location "$ws\Arachne"
$tensile = "$ws\baseline-dependencies\TensileLib\TensileLib.dll"
dotnet build tools/Arachne.ExampleWriter/Arachne.ExampleWriter.csproj -c Release -p:GpcPilot=true "-p:TensileLibPath=$tensile"
& .\bin\Arachne.ExampleWriter\Release\net48\Arachne.ExampleWriter.exe $PWD.Path 'C:\Program Files\Rhino 8\System' 'C:\Program Files\Rhino 8\Plug-ins\Grasshopper' --gpc-pilot
```

Ogni esecuzione crea una nuova cartella e ne stampa il percorso dopo `F4 BRIDGE PASS` o `F4 MEMBRANE PASS`. Per la riapertura in un altro processo impostare `$env:GPC_PILOT_REOPEN` al percorso appena prodotto e ripetere il comando dell’eseguibile corrispondente; attendere `F4 REOPEN PASS 2` o `F4 REOPEN PASS 4`. Rimuovere poi la variabile con `Remove-Item Env:GPC_PILOT_REOPEN`.

Per ripristinare le build predefinite, ripetere i due comandi `dotnet build` senza `-p:GpcPilot=true`. È già stato fatto al termine delle prove riportate qui. La feature del pilota non modifica il set `BridGHe/lib/GPC`, i GUID/port delle componenti distribuite o TensileLib.

F4 chiude il gate dei piloti nel perimetro indicato. F5/F6 restano necessari per dominio, exporter e migrazione completa dei componenti; F7/F8 per adozione coordinata e rilascio.
