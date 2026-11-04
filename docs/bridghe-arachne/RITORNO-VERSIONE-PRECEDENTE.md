# Ritorno alla versione precedente

Guida per utenti e amministratori. Scritta il 4 novembre 2026, prima del rilascio di F8. Il rilascio è descritto in [STATO.md](STATO.md), «F8».

Dopo F8 escono insieme:
- il set ufficiale delle librerie GPC (GPCModel almeno 5.50.0.0);
- ANTHEA 1.19.0;
- BridGHe 0.4.0, Arachne e ReBear sullo stesso set.

Questa guida dice come tornare alle versioni di prima e che cosa succede ai file.

## Le versioni precedenti

| Prodotto | Versione | Commit | Librerie GPC |
|---|---|---|---|
| ANTHEA | 1.18.0 | tag `v1.18.0` = `2efb5ab0` | set AU in `lib/Checker` |
| BridGHe | 0.3.0 | master `95ab7c3` (su origin) | set del 2/11 in `lib/GPC` (GPCModel 5.37.0.2) |
| Arachne | 0.3.0-rc.8 (DLL 0.3.0.8) | master `39c54a2` (su origin) | solo GPCGeometry 2.3.0.0 e GPCUtilities 3.0.0.0, più TensileLib 2.5.0.1 |
| ReBear | 0.3.3 | master `f2df907` | set del 2/11 in `lib/GPC` (GPCModel 5.37.0.2) |

Rhino2FEM, Pawper, Ant e Meerkat non cambiano con F8: per loro non c'è nulla da riportare indietro.

## Prima di tutto: copie dei file

Alcuni file salvati con le versioni nuove non si aprono con quelle vecchie. **Prima di aprire con la versione nuova un file che serve ancora con quella vecchia, farne una copia.** I dettagli sono più sotto, per ogni prodotto.

## ANTHEA: tornare a 1.18.0

### Dove sta l'installer

- Oggi: `\\STORAGESARZANA\Varie\20 - AGGIORNAMENTI_SOFTWARE\Anthea\ANTHEA-1.18.0-Setup-x64.exe`.
- Dopo il rilascio della 1.19.0: `\\STORAGESARZANA\Varie\20 - AGGIORNAMENTI_SOFTWARE\Anthea\_SUPERATI\ANTHEA-1.18.0-Setup-x64.exe`. La regola dell'utente sposta lì il precedente quando esce il nuovo.
- Copia locale: `ANTHEA\supporto\artefatti\rilascio-1.18.0\installer\ANTHEA-1.18.0-Setup-x64.exe`.
  - 74.038.987 byte;
  - SHA-256 `330D52D858079E870749314A480F8128782270C9A498F61469161722E0A3B613`;
  - secondo RIPRESA.md la copia sul server è identica per SHA-256 all'ultima costruita. Qui non è ricontrollata.

### Come

1. Chiudere ANTHEA.
2. Disinstallare ANTHEA 1.19.0 da Impostazioni di Windows, App.
3. Eseguire `ANTHEA-1.18.0-Setup-x64.exe`.
4. Aprire ANTHEA e controllare il numero di versione.

Non è verificato se l'installer della 1.18.0 si installa sopra una 1.19.0 senza disinstallarla. Per questo la guida dice di disinstallare prima.

### File di ANTHEA

- **Un documento della 1.19 con almeno una sezione c.a. non si apre con la 1.18.**
  - La 1.19 salva le sezioni c.a. nel formato compatto e mette il documento alla versione 5.
  - La 1.18.0 dice «Operazione non completata» — «Formato o versione del file non supportati.». È lo stesso messaggio di ogni file di una versione più recente. Provato con la vera 1.18.0 (F7a).
- **Un progetto della 1.18 aperto e salvato con la 1.19 non si apre più con la 1.18**, anche senza modifiche. Al primo salvataggio la 1.19 converte le sezioni nel formato compatto. È la scelta dell'utente («tenila così»). Quindi: copia del file prima di salvarlo con la 1.19.
- Un documento della 1.19 **senza** sezioni c.a. compatte tiene le versioni 1–4 e i suoi byte. Lo dice il codice (`CompactSectionDocumentPersistence`); con la vera 1.18.0 non è stato provato.
- L'archivio dei risultati (blocchi di `GPC.Archive`) è identico byte per byte a quello della 1.18.0 (L3).
- Non esiste un convertitore dalla 1.19 alla 1.18.

### Per ricostruire la 1.18.0 dai sorgenti

- Il tag `v1.18.0` (`2efb5ab0`) contiene in `lib/Checker` il set AU: stessa data e stessi commit del suo `release-set.json`.
- Per riportare il set AU in un altro checkout di ANTHEA: `tools/libs/Update-Snapshot.ps1 -ReleaseSet <cartella del set> -Install`. Lo script ricontrolla gli SHA-256.

## Plugin: tornare a BridGHe 0.3.0, Arachne master e ReBear master

### Riportarli tutti e tre insieme

In uno stesso Rhino la prima copia caricata di una DLL GPC vale per tutti i plugin. BridGHe 0.3.0 e ReBear master usano lo stesso set del 2/11. Arachne master non carica GPCModel. Se un plugin nuovo resta accanto a quelli vecchi, i controlli al caricamento segnalano le DLL diverse. Per questo i tre plugin vanno riportati indietro insieme.

### Dal server

- Cartelle dei plugin: `X:\ITEC-MODELLI\5-RHINO\SGBIM-GR_PLUGIN\<plugin>`. I colleghi prendono i plugin da lì.
- Alla pubblicazione di F8 il contenuto vecchio si sposta, per rinomina, in `X:\ITEC-MODELLI\5-RHINO\SGBIM-GR_PLUGIN_da-eliminare_<data>`. Sta fuori dall'albero dei plugin, perché Grasshopper può leggere le sottocartelle. La cancella l'utente.
- Finché quella cartella esiste, il ritorno più rapido è questo, con Rhino chiuso:
  1. spostare il contenuto nuovo di `<plugin>` in un'altra cartella da eliminare;
  2. rimettere in `<plugin>` il contenuto vecchio;
  3. controllare gli SHA-256 dei file copiati;
  4. tenere una sola `.gha` per cartella.
- **Attenzione, BridGHe.** Oggi (4/11 sera) sul server c'è `BridGHe.Grasshopper.gha` con versione del file 0.2.1.0, del 4/11 alle 11:58: non la 0.3.0. Arachne è 0.3.0.8 e ReBear 0.3.3.0. Se il server non cambia prima di F8, la cartella da eliminare conterrà BridGHe 0.2.1. Per tornare alla 0.3.0 serve ricostruirla da `95ab7c3`.

### Dai sorgenti

1. Creare un worktree sul commit della tabella in alto, per esempio `git worktree add <cartella> 95ab7c3`.
2. Costruire in Release, con Rhino chiuso, come per ogni rilascio del plugin.
   - BridGHe e ReBear: il set di librerie è già nel commit (`lib/GPC`).
   - Arachne master legge GPCGeometry e GPCUtilities da `..\Geometry` e `..\Utilities` (`Directory.Build.props`). Servono i bin di Geometry `6734eee` e Utilities `e57b440`, che F8 non cambia.
   - Arachne: costruire `Arachne.sln` con `-p:ShouldUnsetParentConfigurationAndPlatform=false`, altrimenti i progetti fuori dalla soluzione si compilano in Debug (nota P2A).
3. Il controllo al caricamento avvisa se il bin non viene dal commit del checkout. Ricostruire il bin dopo ogni cambio di commit.
4. Copiare bin ed esempi sul server presi da git (`git archive`), non dal working tree, e controllare gli SHA-256.

### File di BridGHe

- **File dei risultati `<output>.bghr` accanto al `.st7`.**
  - La 0.4.0 scrive il formato `BGHR2` nello stesso percorso della 0.3.0, con gli stessi elementi nel documento (`ResultsFile`, `ResultsHash`).
  - La 0.3.0 legge solo `BGHR1`. Con un file `BGHR2` dà «Formato del file dei risultati non riconosciuto: <percorso>» e servono nuove analisi (Run).
  - Al contrario, se la 0.3.0 rilancia l'analisi, riscrive il file in `BGHR1`. La 0.4.0 lo riconosce e dice «Risultati di una versione precedente di BridGHe: rilanciare Run.».
  - Se si usano le due versioni sullo stesso progetto, conviene una cartella di output diversa per ognuna.
  - Questo comportamento viene dal codice (BridGHe master `95ab7c3` e branch F6 `f142705`); non è provato con un documento vero.
- **Dati internalizzati nei `.gh`.**
  - Al commit F6 di oggi (`f142705`) i tipi GH scrivono ancora i payload FeMM `BGHA1`, che la 0.3.0 legge.
  - Con le fasi (a) e (b) di P2B i dati passeranno ai contratti GPC (`BGHGPC1`, `GpcGooArchive`). La 0.3.0 rifiuta un payload che non è `BGHA1` («Formato archivio non BridGHe…») e il dato si perde all'apertura.
  - **Da verificare a fine BridGHe**, sul formato finale della 0.4.0.

### File di Arachne

Le note delle corsie non provano l'apertura in master di un `.gh` scritto da F6. Dal codice:
- F6 scrive i dati internalizzati nei contratti GPC (`ARCGPC3`): elementi nella voce `Data`, `GH_Model` nella voce `Model`.
- Master `39c54a2` legge quei dati con BinaryFormatter. La lettura fallisce, `Read` restituisce false e il dato si perde. Il codice di master scrive solo una riga di debug.
- Il `.gh` si apre: componenti e fili restano, perché GUID e porte sono invariati (inventario 66/66). I dati internalizzati vanno rifatti dagli ingressi.
- I risultati del Form Find di F6 stanno nel file `<documento>.<InstanceGuid>.gpc-results`. Master non lo legge: il Form Find va rilanciato.

Nell'altro verso, F6 rifiuta con un messaggio i vecchi payload BinaryFormatter, NRBF e `ARCGPC2`; gli ingressi collegati si ricalcolano.

### File di ReBear

Per F8 ReBear cambia solo le librerie in `lib/GPC` (F7a, commit `63a78f4`). Le note non segnalano cambi di formato dei file di ReBear. Non è verificato.

## Il set di librerie precedente

### Set AU (ANTHEA 1.18.0)

- Cartella: `C:\Users\g.pacini\Documents\Repository\ANTHEA\supporto\artefatti\librerie-20261102-au`.
- `release-set.json`: SHA-256 `35B4D0ADD47DA75F84CCFD79E6A27264E4AF1D95B1E3A5E4F31E37E35EA47A5F`, creato il 2/11/2026 alle 20:32, non candidato.
- Commit: Utilities `e57b440b`, Geometry `6734eee1`, Model `2f934f9f`, Checker `6156e4ce`, tutti su origin/master.
- È il `-Previous` da dare a `Release-Set.ps1` per il set ufficiale di F8 (F7a, §7).

| DLL | Versione | SHA-256 |
|---|---|---|
| GPCUtilities.dll | 3.0.0.0 | `92031B381F9AA426A5F88B48762C72DB89724F784670FFDA8802203FC52A05E5` |
| GPCGeometry.dll | 2.3.0.0 | `7EF76EDFB4C3736AAE7B6F66B2F3CE30A8BB8DE031C492B03BD52289D6E9D29E` |
| DelaunayMesh.dll | 2.0.0.14 | `D18BB08255D23239271C197130515758767291F75CE1A9996F1A92DB55F114BB` |
| GPCModel.dll | 5.38.0.0 | `670526497D59048E53E4EC8778C394A5C101FB840D881718036A0242101E16ED` |
| GPCModelData.dll | 0.3.0.26 | `9E702EA456E0E33224C6067B9A68230D2FCA1F173050C28F86C55BCA200EBF2E` |
| GPC.Model.Persistence.dll | 3.4.0.16 | `B68E3F373751C4EAF039B10E9423E090E3FF5B36C0DE1EE26FDF48F4B119EC50` |
| GPC.Converter.dll | 2.12.0.6 | `1B2BDD263B4FFE1FFA2AE184A214F1C57DB03C9C32E4113E3A8FCF750E2735D0` |
| GPCChecker.Concrete.dll | 0.35.0.6 | `2939F5977FB5673E6BD3FFB326353630477EBB21480C1F7A2813E7B299056BE7` |
| GPCChecker.Steel.dll | 2.129.0.5 | `D86F952C37E272AC29A321DC4E778DA886D120B7E992325E25B39112B62F7CDA` |
| GPCChecker.CompositeBridge.dll | 1.23.0.7 | `76166B28CCC3E8B1A7FCB33222C039BB19CD005AAE4848F2A6E2A75CF386D8ED` |
| GPCChecker.Geotechnics.dll | 0.27.0.16 | `8926D0177D43364B41C0E20E941CCC8ADDBF1692BC01E4D31C4B3F2493B58AC1` |
| GPCModelChecker.dll | 2.38.0.4 | `761BDEA3D0301466A291C8E84FCA16FB46ED8608796084A5FD99F54ED9327806` |
| GPCChecker.Masonry.dll | 0.10.0.31 | `0C28D861852DDADFBE03CB2E782DE95FD8C822FB69C1FA2961605425FCA2719F` |
| GPC.Design.dll | 0.45.0.5 | `706C6BA0E7D42533C1555AEF65AB82985D6F6337F808E07183278FC38EF8187D` |

`GPC.Archive` non c'è nel set AU: entra con F8.

### Set del 2/11 mattina (BridGHe 0.3.0 e ReBear master)

**I plugin non sono sul set AU.** BridGHe `95ab7c3` e ReBear `f2df907` hanno in `lib/GPC` il set ufficiale del 2/11 alle 10:36:
- commit Model `15587299`, Checker `742f45f3`, Utilities `e57b440b`, Geometry `6734eee1`;
- GPCModel 5.37.0.2 (SHA-256 che inizia con `1E845324`), GPCModelData 0.3.0.25, GPC.Model.Persistence 3.4.0.15, GPC.Converter 2.12.0.5, GPCChecker.Concrete 0.35.0.5, GPCChecker.Steel 2.129.0.4, GPCChecker.CompositeBridge 1.23.0.6;
- il `release-set.json` dei due plugin è lo stesso file (blob git `63eda307`).

Una copia del set è in `%TEMP%\gpc-release-20261102-742f45f3`, con lo stesso `release-set.json`. Una cartella temporanea può sparire: la copia che conta è quella nei commit dei plugin.

## Che cosa non è verificato

- L'installazione della 1.18.0 sopra la 1.19.0 senza disinstallare.
- L'apertura in master di un `.gh` di Arachne scritto da F6: la guida descrive il codice, non una prova.
- I `.gh` di BridGHe 0.4.0 con il formato finale dei dati internalizzati: P2B non è finita.
- I file di ReBear.
