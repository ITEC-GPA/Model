BUILD RIPRODUCIBILE E BASELINE API

Eseguire: ./build/Verify.ps1
Il runner prepara .dependencies da build/dependencies.props, ripristina NuGet
con --locked-mode, esegue Model.Core.Tests, UnitTest e Model.NetFramework.Tests e
confronta le API di GPCModel e GPC.Model.Persistence.

.NET Framework (2/11/2026). GPCModel e' netstandard2.0 e archivi e impronte passano
tra .NET e .NET Framework. Model.NetFramework.Tests (net472) esegue su .NET Framework
i test condivisi di tests/ che UnitTest esegue su .NET 6 (RuntimeIndependentFingerprintTest):
gli stessi attesi su entrambi i runtime. Fino a GPCModel 5.37.0.0, su .NET Framework
ModelArchive.Load rifiutava ogni archivio con snapshot d'analisi scritto su .NET
("Analysis snapshot inputs disagree with their provenance"): Type.FullName qualifica
gli argomenti generici con il loro assembly, che per i tipi del framework cambia
(mscorlib invece di System.Private.CoreLib; System.Core per HashSet<T>, il tipo di
StageCombinationsMap, che entra in ogni impronta d'analisi). HistoricalTypeNames li
nomina ora come .NET. Riproduzione: dotnet test Model.NetFramework.Tests -c Release
(con il sorgente precedente falliscono entrambi i test).
Limite residuo, di .NET Framework e non di Model: il suo parser dei double sbaglia di
1 ulp circa 1,5 valori calcolati su 10.000 scritti da .NET, e il formato "R" con cui
scrive non sempre si rilegge uguale. Un modello grande scritto su .NET puo' quindi
aprire su .NET Framework uno snapshot con impronta diversa anche dopo questa correzione.

Dal 10/10/2026 GPCModelChecker e ModelChecker.Tests stanno nel repository Checker,
a valle di Model (revisione di architettura, punto R3): Model fissa solo Geometry e
Utilities e non dipende piu' da nessuna DLL di Checker. Le baseline API di
GPCModelChecker e di GPCChecker.Concrete sono in Checker/build/api.

Rilascio: treno unico di Utilities, Geometry, Model e Checker in Checker/build/RILASCIO.txt.
Le versioni le assegna l'utente; il set coerente lo produce Checker/build/Release-Set.ps1,
che esegue anche questo Verify (-Results <cartella> per un risultato separato per suite).

Build deterministiche: in Release (Directory.Build.props) ContinuousIntegrationBuild
mappa la radice del checkout su /_/, quindi lo SHA-256 di una DLL dipende dai
sorgenti, dal commit e dalle DLL referenziate, non dalla cartella di compilazione.
Per una macchina senza repository fratelli: -SourceBundle <cartella DLL>.
Per build dirette e' possibile -p:GpcDependencyDir=<cartella>; gli hash restano
obbligatori. Non e' previsto un fallback silenzioso a bin/Release.

Il manifest fissa versioni e SHA256 dei tre file (DelaunayMesh, GPCGeometry, GPCUtilities). ObservedSourceCommit e'
provenienza del checkout osservato: non attesta una ricompilazione di DLL gia'
esistenti. Un aggiornamento delle dipendenze richiede revisione del manifest e
nuova validazione, non una rigenerazione automatica durante CI.

Gmsh (GMsh.Net, UnsafeEx, gmsh-*.dll nativa) non fa parte del bundle e non si
distribuisce con le librerie: va solo negli unit test. UnitTest lo referenzia
dal repository fratello Gmsh.Net (src/GMsh.Net/bin/Release/netstandard2.0).

Baseline API acquisita dalle DLL del checkpoint precedente al refactoring:
GPCModel 1.6.1.9, GPCModelChecker 1.0.4, Concrete 0.0.18.0.
Il tool verifica tipi pubblici, basi, interfacce, metodi/accessor, visibilita',
parametri/default, vincoli generici e costanti. Le aggiunte sono ammesse;
rimozioni o modifiche non approvate falliscono. Non sostituisce le prove
numeriche/archivi e non certifica semantica o compatibilita' di codice esterno
non disponibile. Le eccezioni per tipi rimossi sono file *.removed-types.txt.
Non usare capture per aggiornare gli attesi dopo un errore di compatibilita'.

Validazione dei controlli: una DLL temporanea alterata viene respinta per SHA256;
una DLL assente viene respinta senza usare il repository fratello.

Model 4: type-renames.tsv applica rinomine pubbliche puntuali alle firme della
baseline prima del confronto. Non autorizza rimozioni di metodi o modifiche
a parametri/default. La baseline storica non e' stata sovrascritta.
Namespaces-v3 conserva i contratti XML e i fingerprint prima della migrazione;
le stringhe dei vecchi namespace nelle mappe sono identita' wire intenzionali.

Uguaglianza degli oggetti del modello (revisione di architettura R4, 11/10/2026):
tools/EqualityAudit (Roslyn) cerca nei sorgenti di GPCModel, Converter, Persistence e
GPCModelData gli usi dell'uguaglianza predefinita (Equals, ==, HashSet/Dictionary e
operatori LINQ senza comparatore) sui tipi derivati da ModelObject, fuori dai membri di
uguaglianza. Ogni uso deve stare in equality-allowlist.json con il motivo e il numero di
occorrenze; un uso nuovo dichiara invece l'intento: ModelObjectIdentityComparer per
l'identita', ContentEquals per il contenuto. 'list' stampa gli usi nel formato
dell'elenco. Verify.ps1 esegue 'check' dopo il controllo dell'API.
