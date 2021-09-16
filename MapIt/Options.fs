namespace MapIt.Cli

open Argu
open MapIt.Types

type InitArgs =
    | [<AltCommandLine("-p")>] Path of string

    static member ToOptions(args: ParseResults<InitArgs>) : InitOptions = { path = args.TryGetResult(Path) }

    interface IArgParserTemplate with
        member this.Usage: string =
            match this with
            | Path _ -> "Where to write the config file"

type SearchArgs =
    | [<AltCommandLine("-p")>] Package of string

    static member ToOptions(args: ParseResults<SearchArgs>) : SearchOptions =
        { package = args.TryGetResult(SearchArgs.Package) }

    interface IArgParserTemplate with
        member this.Usage: string =
            match this with
            | Package _ -> "The name of the package to search for."

type ShowArgs =
    | [<AltCommandLine("-p")>] Package of string

    static member ToOptions(args: ParseResults<ShowArgs>) : ShowPackageOptions =
        { package = args.TryGetResult(ShowArgs.Package) }

    interface IArgParserTemplate with
        member this.Usage: string =
            match this with
            | Package _ -> "The name of the package to show information about."

type InstallArgs =
    | [<AltCommandLine("-p")>] Package of string
    | [<AltCommandLine("-s")>] Source of Source option

    static member ToOptions(args: ParseResults<InstallArgs>) : InstallPackageOptions =
        { package = args.TryGetResult(InstallArgs.Package)
          source =
              args.TryGetResult(InstallArgs.Source)
              |> Option.flatten }

    interface IArgParserTemplate with
        member this.Usage: string =
            match this with
            | Package _ -> "The name of the package to show information about."
            | Source _ -> "The name of the source you want to install a package from. e.g. unpkg or skypack."

type SetEnvArgs =
    | [<AltCommandLine("-p")>] Env of Env

    static member ToOptions(args: ParseResults<SetEnvArgs>) : SetEnvOptions = { env = args.TryGetResult(Env) }

    interface IArgParserTemplate with
        member this.Usage: string =
            match this with
            | Env _ -> "Sets the export map for development/production."


type MapItArgs =
    | [<CliPrefix(CliPrefix.None)>] Init of ParseResults<InitArgs>
    | [<CliPrefix(CliPrefix.None); AltCommandLine("s")>] Search of ParseResults<SearchArgs>
    | [<CliPrefix(CliPrefix.None)>] Show of ParseResults<ShowArgs>
    | [<CliPrefix(CliPrefix.None)>] Install of ParseResults<InstallArgs>
    | [<AltCommandLine("-v")>] Version

    interface IArgParserTemplate with
        member this.Usage: string =
            match this with
            | Init _ -> "Creates basic files and directories to start using mapit."
            | Search _ -> "Searches a package in the skypack API."
            | Show _ -> "Gets the skypack information about a package."
            | Install _ -> "Generates an entry in the import map and saves it to the dependencies."
            | Version _ -> "Prints out the cli version to the console."
