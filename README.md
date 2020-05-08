# Troubleshooting

### Error: `The reference assemblies for framework ".NETFramework,Version=v4.7.1" were not found.`
#### **TL;DR:** 
- Go to https://aka.ms/msbuild/developerpacks and download `.NET Framework v4.7.1`.
#### Details:
- Unity includes its own, custom implementation of the `.NET Standard 2.0` specifications that are included within the Unity distribution - i.e. C# code executed by Unity does **NOT** use the `.NET Framework` installed on your computer (I'm pretty sure), but rather, _Unity itself_ contains everything that you need.
- "Unity .NET" _(not a real term)_ corresponds to a particular version of `.NET Framework`.
    - You can think of `.NET Framework` as the "parent" of "Unity .NET"
- Other tools, like Rider, don't understand "Unity .NET" - they understand `.NET Framework`.
- Unity tells tools like Rider what version of `.NET Framework` to treat "Unity .NET" as by defining `<TargetFrameworkVersion>` in the Unity-generated `.csproj` files, e.g. [Assembly-CSharp.csproj](../Assembly-CSharp.csproj)
- Rider (and probably other tools - it looks like VSCode in particular has this issue a lot) requires that you have the ***exact `.NET Framework` version*** installed on your computer that the `.csproj` files ask for.
- You might have the most up-to-date version of `.NET Framework` on your computer - for example:
    - On `5/8/2020`, using Unity `2019.3.13f1`;
    - The most recent `.NET Framework` version was `4.8` (released `2019-04-18`)
    - Unity set `<TargetFrameworkVersion>` to `4.7.1` (released `2017-10-17`)