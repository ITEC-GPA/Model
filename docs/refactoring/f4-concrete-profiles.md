# F4.14 — catalogo tipizzato dei profili CLS

Model 5.4.0.0: ConcreteStandardKind e ConcreteStandardCatalog raccolgono i nove profili già usati da ANTHEA. Parse/Create rifiutano valori mancanti o sconosciuti; ogni chiamata crea uno standard indipendente. Gli override DIN/NS prima applicati dalla UI appartengono ora al catalogo del dominio. Nessuna modifica ai costruttori storici o alla serializzazione delle norme; αcc UNI riusa il valore già presente nel costruttore.

Prima del port: ANTHEA residual-before.json SHA ca5700c5e25399b0f69b693e9f48de7b32d380678fd113114999b90a26fc2921. I soli nove record normativi sono estratti, senza ricalcolo, in tests/Fixtures/concrete-profiles-before.json, SHA ec9dbc055172ae78936c6905d51915da0ea817ada13629204de439413ea862cd. Quattro test mirati PASS, incluse le due prove del modulo elastico condiviso. Adattatore ANTHEA e suite complete ancora da integrare. Nuova API: incremento del secondo componente della versione.


## Registrazione del vocabolario, 5.4.0.1

Il primo set completo O ha rilevato il nuovo enum non dichiarato in ModelDataContracts (303 tipi pubblici contro 302 registrati). Aggiunto il tipo al vocabolario esplicito; nessun test o atteso cambiato. Suite ExplicitPersistenceTest superata dopo il fix; set completo da ripetere. Incremento finale di versione per la correzione.
