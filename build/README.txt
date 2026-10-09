BUILD RIPRODUCIBILE E BASELINE API

Eseguire: ./build/Verify.ps1
Il runner prepara .dependencies da build/dependencies.props, ripristina NuGet
con --locked-mode, esegue Model.Core.Tests, UnitTest e ModelChecker.Tests e confronta le API.
Per una macchina senza repository fratelli: -SourceBundle <cartella DLL>.
Per build dirette e' possibile -p:GpcDependencyDir=<cartella>; gli hash restano
obbligatori. Non e' previsto un fallback silenzioso a bin/Release.

Il manifest fissa versioni e SHA256 dei sei file. ObservedSourceCommit e'
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
