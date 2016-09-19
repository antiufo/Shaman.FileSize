# Shaman.FileSize
Library for working with formatted file sizes (MG, GB…)

```csharp
using Shaman.Types;

FileSize f = FileSize.Parse("50 MB");
// f.Bytes == 52428800;
f.ToString(); // "50 MB"
```