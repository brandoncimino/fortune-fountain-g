using System.Runtime.CompilerServices;

// This lets everything inside of another assembly access `internal` members.
// This file should live in the same directory as an `.asmdef` file.
// The `assemblyName` should refer to the `name:` field in another `.asmdef` file.
[assembly: InternalsVisibleTo("com.fowlfever.fortunefountaing.runtime.tests")]