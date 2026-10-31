# Configure dictionary storage

1. In the editor choose **Dictionary storages**, then **Add registration**.
2. Choose the registered adapter type (currently FileJson) and provide a unique ID.
3. Configure the dictionary directory, read-only option, and source language.
4. **Use dictionaryless key = value** enables the default convention where the source key itself is the original text. Keep this enabled unless your storage workflow intentionally uses different keys.
5. Validate the draft, review the changes and confirm Save.

The storage configuration is edited in memory. Opening Help does not change it. If the directory is unavailable, use validation and any later runtime diagnostics separately; a valid JSON schema alone does not prove filesystem access.
