namespace MapIt.Types

open System.Collections.Generic

type MapitConfig =
    { name: string
      author: string
      importMapPath: string option
      contributors: string seq option
      dependencies: IDictionary<string, obj> option }

type Source =
    | Skypack = 0

type Env =
    | Dev = 0
    | Prod = 1

type InitOptions = { path: string option }

// https://api.skypack.dev/v1/search?q=package-name&p=1
type SearchOptions = { package: string option }

// https://api.skypack.dev/v1/package/package-name
type ShowPackageOptions = { package: string option }

// https://cdn.skypack.dev/package-name
type InstallPackageOptions =
    { package: string option
      source: Source option }

type SetEnvOptions = { env: Env option }


exception CommandNotParsedException of string
exception HelpRequestedException
