# RustRun

A [PowerToys Run](https://learn.microsoft.com/en-us/windows/powertoys/run) plugin written in C# using the [Template](https://github.com/hlaueriksson/Community.PowerToys.Run.Plugin.Templates) that will import and run functions from a Rust `cdylib`-style DLL placed alongside it called `rustinteroprust.dll`. Heavily supported by research done in [csbindgen](https://github.com/Cysharp/csbindgen).

### Important Notes
`ID` in `plugin.json` MUST match the ID provided by the DLL when the plugin is run. It also helps to change `ActionKeyword`.