# Typetree Generator

A simple **cross-platform** CLI port of [Perfare's MonoBehaviourConverter](https://github.com/Perfare/AssetStudio/blob/master/AssetStudioUtility/MonoBehaviourConverter.cs) into a CLI application.
Unity.CecilTools and Unity.SerializationLogic are from the official [UnityCSReference](https://github.com/Unity-Technologies/UnityCsReference).

### Argumens
  * -p  ~ assembly folder path (required)
  * -a  ~ assembly name (required)
  * -v  ~ unity version (required)
  * -c  ~ class name (if not set, all classes will be dumped)
  * -n  ~ namespace (optional for set class name)
  * -d  ~ dump style (simple, json, json_min, bin)
  * -z  ~ if used, the data will be compressed via gzip
  * -o  ~ output path (instead of printing the data will be stored in the given path) //TODO

All binary formats (bin, compressed) are displayed as base64 string.

### Example

```bash
./TypeTreeGeneratorCLI.exe -p "./DummyDll" -a "Assembly-CSharp.dll" -v "2018.4.36f1" -d json -o "./assembly-csharp.json"
```

Linux and Mac users have to use the .exe as well.

In case it boots up wine for them,
they can try ``dotnet ./TypeTreeGeneratorCLI.dll`` instead of ``./TypeTreeGeneratorCLI.exe``.

### Download

Builds can be downloaded from [Releases](https://github.com/K0lb3/TypeTreeGenerator/releases/).
