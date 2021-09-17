namespace MapIt.Types

type LockDependency =
    { lookUp: string
      pin: string
      import: string }

type MapitConfig =
    { name: string
      importMapPath: string option
      dependencies: Map<string, string> option }

type MapItLock = Map<string, LockDependency>

type ImportMap =
    { imports: Map<string, string>
      scopes: Map<string, string> }

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
exception HeaderNotFoundException of string
