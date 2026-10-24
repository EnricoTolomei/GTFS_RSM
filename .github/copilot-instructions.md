# Copilot Instructions

## General Guidelines
- Preferisce mantenere l'interfaccia utente disaccoppiata dalla logica applicativa, in modo da poter sostituire o modernizzare la UI senza riscrivere il core.

## Project-Specific Rules
- Preferenza: ottimizzare GetScheduledTripIdsInTimeWindow considerando solo stop_sequence di partenza (0 o 1) e pre-caricare 'stop_times.txt' in background per calcoli pianificati vs rilevati.
- Nella code review del progetto, FeedVehicleManager.ElaboraUltimoFeedValido_BCK è una copia di backup della vecchia logica di refactoring e può essere ignorata perché verrà rimossa.
- Nel progetto, AcquisizioneNEW in FormGTFS_RSM potrebbe essere codice legacy del refactoring e non più invocato; prima di attribuirgli responsabilità operative verificare i riferimenti effettivi nel codice.
- Nel progetto, LeggiValidFeedNEW in FormGTFS_RSM è codice storico e verrà eliminato; non ha riferimenti attivi e non va considerato parte del flusso operativo.