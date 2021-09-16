namespace LinkIt.Types


type Source =
    | Skypack = 0

type Env =
    | Dev = 0
    | Prod = 1

type Configuration =
    { path: string option
      output: string option }

type InitOptions = { path: string }

// https://api.skypack.dev/v1/search?q=package-name&p=1
type SearchOptions = { package: string }

// https://api.skypack.dev/v1/package/package-name
type ShowPackage = { pacakge: string }

// https://cdn.skypack.dev/package-name
type InstallPackage =
    { pacakge: string
      source: Source option }

type SetEnvOptions = { env: Env }
