# GPC Model: post-processing NODE / BEAM / SHELL

Il dominio FEM rimane `GPC.Model.Models.Model`. Nodi, beam, shell e solidi condividono identità e registri; gli assi sono sempre i tipi reali `Point3d`, `Vector3d` e **`GPC.Geometry.CoordinateSystem`**.

I convertitori sono nel progetto separato **`GPC Converter/GPC Converter.csproj`**, incluso in `GPCModel.sln`, con assembly/namespace `GPC.Converter`. `Model` non dipende dai convertitori né dal verificatore. L'adattatore al verificatore esistente è opzionale: `ModelChecker/GPCModelChecker.csproj`.

```powershell
dotnet build GPCModel.sln --no-restore
dotnet test UnitTest/UnitTest.csproj --no-restore
dotnet test ModelChecker.Tests/ModelChecker.Tests.csproj
dotnet run --project Examples/MixedModel/MixedModel.csproj --no-restore
dotnet run --project Examples/MixedModel/MixedModel.csproj --no-restore -- --without-checker
dotnet run --project Examples/MixedModel/MixedModel.csproj --no-restore -- --model-only
dotnet run --project Examples/MixedModel/MixedModel.csproj -- --civil-file Examples/CivilTextSynthetic.mct synthetic-revision
dotnet run --project Examples/MixedModel/MixedModel.csproj -- --straus-api-file 'C:\modelli\modello.st7' rev-1
```

Servono le dipendenze GPC locali indicate nei progetti. Su un checkout senza `obj/project.assets.json`, eseguire prima `dotnet restore` con le sorgenti NuGet aziendali configurate. Target e versioni dei pacchetti esistenti sono invariati: librerie `netstandard2.0`, test/esempio `net6.0`.

L'esempio è **sintetico**: non esegue un'analisi FEM. Esercita topologia condivisa, risultati e armature, preparazione, verificatore reale quando presente, archivio e riapertura. Le risultanti shell assegnate non costituiscono una soluzione in equilibrio del modello misto.

Il flusso comune comprende gruppi gerarchici e importazione atomica di **reazioni/azioni nodali, spostamenti/rotazioni e risultanti beam/plate**, con provenienza e unità. Le azioni nodali dei singoli elementi mantengono famiglia, elemento proprietario, estremo I/J e corpo dell'azione. `ResultPreparation.Prepare` seleziona nodi/elementi per gruppi e stati, valida i dati e prepara copie negli assi locali tramite `CoordinateSystem`; le copie diventano non correnti dopo modifiche agli input o ai risultati. `ModelPreparation` controlla invece l'idoneità strutturale prima di `ModelChecker.Verify(model, request, cancellationToken)`. Ogni lavoro può scegliere gruppi/elementi, dataset/casi/fasi e impostazioni diverse. Il servizio crea uno o più Checker reali, riutilizzando sezioni e configurazioni compatibili, e mantiene nel report anche verifiche mancanti o non supportate. Ai nodi non viene attribuito un esito generico di verifica del calcestruzzo.

Il primo connettore è **MIDAS Civil NX API**: trasporto in sola lettura, schema comune unità/nodi/connettività, richiesta tabelle beam/plate e lettura delle colonne per nome. È predisposto per profili di schema di più versioni, senza whitelist di release. Campi aggiuntivi conservati; schemi incompatibili rifiutati con diagnostica. Il collaudo attuale usa risposte sintetiche: **non è una dichiarazione di compatibilità verificata con tutte le versioni**.

Il secondo lettore è **MIDAS Civil MCT/MGT** (`MidasCivilTextReader`): importa nodi, connettività beam/plate, gruppi, casi statici, carichi nodali e vincoli globali permanenti semplici. Normalizza N/mm e ricostruisce il `CoordinateSystem` delle travi con angolo beta. Conserva i comandi originali con righe sorgente e rifiuta atomicamente input incoerenti. Il formato ufficiale Civil è MCT: un file chiamato MGT è accettabile se contiene lo stesso schema Civil, senza dedurre la compatibilità dall'estensione. Test ed esempio sono sintetici, senza collaudo su export reali.

Il terzo connettore è **Straus7 API R3** (`Straus7ApiConverter`): apre i file ST7 in sola lettura tramite la DLL installata, importa nodi, beam/plate, assi nativi `CoordinateSystem`, gruppi gerarchici e casi. Normalizza risultati statici lineari beam/plate, reazioni nodali, spostamenti in mm e rotazioni in radianti. Con `IncludeElementNodeForces=true` acquisisce anche le azioni nodali dei singoli beam/plate, se memorizzate dal solver. Collaudato con **Straus7 R31 / API 3.1.5**, un modello plate del produttore e un benchmark risolto dal solver reale. Richiede Windows x64 e licenza API; altre release R3 con lo stesso ABI sono predisposte ma non dichiarate collaudate. Proprietà conservate come dati nativi, ulteriori assegnazioni da mappare. [Uso e limiti](POST_PROCESSING.md#straus7-api-r3).

Ordine dei connettori richiesto: Civil NX API → Civil tramite MGT/MCT → Straus7 → SAP2000 → GEN NX → GEN. Il prossimo convertitore è SAP2000.

Il nucleo Model comprende ora anche riferimenti fisici beam con offset e tratti rigidi, trasporto al baricentro, inversioni I/J e normali su copie del modello, sezioni tabulate/rettangolari variabili, profili di rigidezza separati dalla resistenza, combinazioni nodali/beam/plate, inviluppi di stati concomitanti, accumulo esplicito di incrementi lineari e bilanci di equilibrio con copertura e identità dei contributi. Le inversioni conservano i dati sorgente e rendono non correnti le analisi della revisione precedente. `--model-only` esercita combinazioni, preparazione locale, equilibrio e archivio senza istanziare Checker.

La [documentazione tecnica](POST_PROCESSING.md) contiene contratti, esempi, limiti e il catalogo delle sezioni presenti e da estendere. Restano separati dal nucleo comune i mapping specifici dei converter, SAP2000/GEN, i metodi di verifica da sviluppare in Checker, l'interfaccia e il collaudo su archivi reali estesi. Geometrie/materiali specialistici sono esplicitati nel catalogo: il supporto a una sagoma non implica un algoritmo di verifica resistente per quella famiglia.
