# Mission Planner Generator

Παράγει γρήγορα ένα Mission Briefing με:
- Προτεινόμενο mode λειτουργίας (VLOS/EVLOS/BVLOS/other)
- Στελέχωση πληρώματος (ρόλοι/πλήθος)
- ERP (triggers/actions/muster/ELS)
- Ενδεικτικές εγκρίσεις/γνωστοποιήσεις με βάση ζώνη/airspace

Δεν απαιτεί σύνδεση, βασίζεται σε κανόνες (deterministic).

## Χρήση

```pwsh
python .\Tools\TrainingCenter\mission_planner.py `
  --operation facade `
  --time 06:00 `
  --zone urban `
  --airspace G `
  --sail-target II `
  --entries 3 `
  --length-m 120 `
  --format both `
  --out .\Docs\Compliance\Missions\Facade_0600.md `
  --json-out .\Docs\Compliance\Missions\Facade_0600.json
```

Ή μέσα από το VS Code Chat:

- `@skyworks /plan-mission operation=facade time=06:00 zone=urban airspace=G sail=II entries=3 length=120 format=both`
- Προαιρετικά: `updateMatrix=true` για αυτόματη ενημέρωση του `Docs/Compliance/OSO_to_Evidence_Matrix.md` στο αντίστοιχο Scenario Pack.

- operation: facade|roof|pv|wind_turbine|stadium|custom
- zone: urban|rural|industrial|stadium|other
- airspace: A–G (π.χ. G για μη ελεγχόμενο)
- sail-target: προαιρετικό (π.χ. II)
- entries: αριθμός εισόδων/σημείων φύλαξης CGA
- length-m: ενδεικτικό μήκος πρόσοψης/περιμέτρου για παραγώγιση παρατηρητών
- format: markdown|json|both
- out/json-out: προαιρετικά μονοπάτια αποθήκευσης

## Παρατηρήσεις
- Οι συστάσεις είναι ασφαλείς default πρακτικές. Αν έχεις ειδικές απαιτήσεις αρχής ή venue, προσαρμόζεις.
- Για controlled airspace (C/D/E/B) θα χρειαστείς συντονισμό με ANSP/ATC.
- Για stadium/urban, πρόσθεσε liaison και αυξημένους marshals.
- Συνδύασε με το `Docs/Compliance/OSO_to_Evidence_Matrix.md` για τα OSO & τεκμήρια.
