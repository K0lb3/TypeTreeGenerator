# Typetree Generator

A simple CLI port of [Perfare's MonoBehaviourConverter](https://github.com/Perfare/AssetStudio/blob/master/AssetStudioUtility/MonoBehaviourConverter.cs) into a CLI application.
Unity.CecilTools and Unity.SerializationLogic are from the official [UnityCSReference](https://github.com/Unity-Technologies/UnityCsReference).

__Argumens__
  * -p  ~ assembly folder path (required)
  * -a  ~ assembly name (required)
  * -c  ~ class name (if not set, all classes will be dumped)
  * -n  ~ namespace (optional for set class name)
  * -d  ~ dump style (simple, json, json_min, bin)
  * -z  ~ if used, the data will be compressed via gzip
  * -o  ~ output path (instead of printing the data will be stored in the given path) //TODO
  * -v  ~ version seperated by , e.g. 2018,1,2,0

All binary formats (bin, compressed) are displayed as base64 string.
