# F4 — proprietà elastiche e densità condivise

GPCModel 5.3.0.0, nuove API (secondo componente incrementato). EuropeanConcreteElasticity concentra una sola formula per Ecm dalla resistenza media, valutabile direttamente in MPa o GPa. Le proprietà del materiale continuano a valutare in GPa e convertire in MPa, mantenendo l'ordine aritmetico storico. BridgeConcept può chiedere direttamente MPa, come nel proprio percorso storico. Nessuna correzione numerica, arrotondamento o tolleranza allargata.

Densità dell'acciaio: valore SI canonico 7850 kg/m³, costanti derivate t/mm³ e t/m³. Il valore della costante esistente rimane identico bit per bit. Non si deriva la densità SI da un valore già arrotondato in un'unità molto piccola.

Cattura PRIMA dei sorgenti modificati: tests/Fixtures/material-modulus-before.json, 141 resistenze fck da 20 a 90 MPa con passo 0.5, DLL Model del set distribuito (7fd1fe4c) e ordine aritmetico Bridge ec284cf2. Il test verifica SHA e tutti i bit di entrambi i percorsi; fixture immutabile. Attesi semplici indipendenti fcm=10: 22 GPa / 22000 MPa e densità 7.85 t/m³. Due nuove prove, suite complete Model e controllo API superati. Nessun cambiamento a serializzazione, mesh, reologia o diagrammi. Evidenze: ANTHEA/supporto/artefatti/refactoring/f4/model-materials-tests.
