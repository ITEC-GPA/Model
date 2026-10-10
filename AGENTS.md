# Regole per gli agenti (librerie GPC: Model)

- **Versioni**: le assegna l'utente. Non cambiare `AssemblyVersion`, `Version` o `FileVersion` senza averglielo chiesto; sui branch di lavoro le versioni restano quelle di master.
- **Rilascio**: un solo treno per Utilities, Geometry, Model e Checker, descritto in `../Checker/build/RILASCIO.txt`. Il set coerente lo produce `../Checker/build/Release-Set.ps1`; nessun rilascio senza tutte le suite di Model (`build/Verify.ps1`) e di Checker.
- **Dipendenze**: Model fissa in `build/dependencies.props` solo Geometry e Utilities (SHA-256 dei binari committati). Il dominio (GPCModel) non referenzia Checker, Converter o ANTHEA; `GPCModelChecker` sta nel repository Checker.
- **Gmsh** (GMsh.Net, UnsafeEx, `gmsh-*.dll`, GMesh) solo negli unit test, mai nel bundle fissato o nelle DLL delle librerie.
- **Attesi e fixture**: non rigenerare mai un atteso, una fixture d'archivio o una baseline API per far passare un test senza l'ok dell'utente. Segnalare i bug invece di correggerli in silenzio.
- **Git**: ogni lavoro parte da master aggiornato su un branch dedicato; merge su master e push solo con l'ok dell'utente.
