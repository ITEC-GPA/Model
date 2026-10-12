# Reologia del calcestruzzo — supporto di progetto ANTHEA 5.2

Nuova API immutabile `ConcreteTimeEffects`, per calcestruzzo ordinario EN 1992-1-1:2004,
viscosità lineare a umidità costante e temperatura di riferimento 20 °C. Cementi S/N/R;
fck 12–90 MPa, RH 40–100 %, h0 positivo in mm, età di carico almeno 0,5 giorni.
`NotionalSize(Ac,u)` usa mm² e mm. Nessuna dipendenza da WPF o Checker.

`AtAge(t)` restituisce φ(t,t0), Ecm(t) in MPa e il rapporto Ecm(t)/Ecm(28).
L'età è quella del calcestruzzo, non la durata del carico: fino a t0 la viscosità è
zero. Età zero e negative sono rifiutate. `CreepInfinity` è il limite analitico;
`AtAge(+∞)` restituisce anche il limite analitico della maturazione di E.

Per la viscosità si applicano B.1–B.9. L'età corretta per il cemento entra soltanto
in β(t0); la durata t−t0 resta cronologica. φ si riferisce al modulo tangente
Ec=1,05 Ecm(28). Ecm(t), ricavato dall'API di maturazione già presente in Model,
è il modulo secante: non è un modulo efficace ridotto per viscosità. Non sono
inclusi ritiro, viscosità non lineare, storie termiche o variazioni di carico.

## Riuso e compatibilità

Il nucleo B.2–B.5 è estratto dal metodo storico `GetEpsilonCCInfiniteTime` in
`ConcreteMaterialEuropeanCommon.NotionalCreepCoefficient`: una sola implementazione,
con lo stesso ordine delle operazioni. L'API storica e quella nuova lo richiamano.
La maturazione continua a richiamare `GetEcm(Fcm,t)` con il segno negativo di Fcm.
La nuova API non modifica materiali o archivi e non aggiunge opzioni implicite ai verificatori.

La vecchia API restituisce una deformazione nel ramo lineare e un coefficiente nel
ramo non lineare: quel limite già documentato non viene corretto durante l'estrazione.
La finestra nuova userà esclusivamente l'API tipizzata di viscosità lineare.

## Verifiche

- Attesi indipendenti in `tests/Fixtures/concrete-time-effects.json`, calcolati da
  `tools/references/concrete-time-effects.py` prima del confronto. Il generatore
  rifiuta di sovrascrivere un file esistente. I valori non derivano dalle DLL.
- `creep-historical.csv` è una cattura di compatibilità, non un atteso fisico:
  648 risposte della vecchia API, includendo il ramo non lineare e l'opzione termica.
  Catture prima/dopo identiche bit per bit, SHA-256
  `7A7B2B71981D954373C8C11B5F7A38C9A7741E141871824A0B902D9E52BEDAA0`.
- Origine del carico, valore esatto a 28 giorni, limite infinito, monotonia, cementi,
  soglia fcm=35 MPa, rifiuti e uso concorrente coperti da `ConcreteTimeEffectsTests`.

Riferimento dei metodi: EN 1992-1-1:2004 §3.1.2, §3.1.3(3), §3.1.4 e Allegato B.
Le guide per l'utente e i PDF sono mantenuti nelle due guide globali di ANTHEA.

## Versioni e collaudo

Model 5.1.0.0: API nuova, quindi incremento del secondo componente secondo la delega dell’utente. ModelData 0.0.2.9, Persistence 3.0.0.2 e Converter 2.0.3.2: revisioni tecniche per riallineare il riferimento al dominio; nessuna modifica dei loro contratti. Il rilascio resta subordinato al treno coerente.

`build/Verify.ps1`: Core 12 PASS, UnitTest 1186 PASS e due prove native già escluse; nessuna rimozione API inattesa, controllo delle uguaglianze superato. Evidenze ANTHEA `supporto/artefatti/refactoring/reologia/model-all`.
