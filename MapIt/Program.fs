// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp
open Argu

open FsToolkit.ErrorHandling

open MapIt.Types
open MapIt.Cli
open MapIt.Runner


let processExit result =
    match result with
    | Ok exitCode -> exitCode
    | Error ex ->
        match ex with
        | CommandNotParsedException message -> eprintfn "%s" message
        | others -> eprintfn $"%s{others.Message}, at %s{others.Source}"

        1

[<EntryPoint>]
let main argv =
    let getCommand () : Result<MapItArgs, exn> =
        result {
            let! parser =
                try
                    ArgumentParser.Create<MapItArgs>() |> Ok
                with
                | ex -> CommandNotParsedException(ex.Message) |> Error

            let! parsed =
                result {
                    try
                        if argv.Length = 0 then
                            printfn "%s" (parser.PrintUsage())
                            return! Error(HelpRequestedException)

                        return parser.Parse argv
                    with
                    | ex -> return! Error ex
                }


            let cliArgs = parsed.GetAllResults()

            match cliArgs with
            | [ Init subcmd ] -> return Init subcmd
            | [ Search subcmd ] -> return Search subcmd
            | [ Show subcmd ] -> return Show subcmd
            | [ Install subcmd ] -> return Install subcmd
            | [ Version ] -> return Version
            | args -> return! CommandNotParsedException $"%A{args}" |> Error
        }

    result {
        let! command = getCommand ()

        return!
            match command with
            | Init subcmd -> subcmd |> InitArgs.ToOptions |> runInit
            | Search subcmd -> subcmd |> SearchArgs.ToOptions |> runSearch
            | Show subcmd -> subcmd |> ShowArgs.ToOptions |> runShow
            | Install subcmd -> subcmd |> InstallArgs.ToOptions |> runInstall
            | Version ->
                printfn
                    "%A"
                    (System
                        .Reflection
                        .Assembly
                        .GetEntryAssembly()
                        .GetName()
                        .Version)

                Ok 0
    }
    |> processExit
