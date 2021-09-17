namespace MapIt.Types

open System.Collections.Generic
open System.Text.Json

type MapitConfig =
    { name: string
      importMapPath: string option
      dependencies: IDictionary<string, JsonElement> option }

type ImportMap =
    { imports: IDictionary<string, string>
      scopes: IDictionary<string, string> }

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
      alias: string option
      source: Source option }

type UninstallPackageOptions = { package: string option }

type SetEnvOptions = { env: Env option }


exception CommandNotParsedException of string
exception HelpRequestedException
exception MissingPackageNameException
exception MissingImportMapPathException
exception PackageNotFoundException
