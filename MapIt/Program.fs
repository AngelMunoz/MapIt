// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp
open Argu

open FsToolkit.ErrorHandling

open MapIt.Types
open MapIt.Cli
open MapIt.Runner


let processExit result =
    task {
        match! result with
        | Ok exitCode -> return exitCode
        | Error ex ->
            match ex with
            | CommandNotParsedException message -> eprintfn "%s" message
            | others -> eprintfn $"%s{others.Message}, at %s{others.Source}"

            return 1
    }

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
            | [ Uninstall subcmd ] -> return Uninstall subcmd
            | [ Version ] -> return Version
            | args -> return! CommandNotParsedException $"%A{args}" |> Error
        }

    taskResult {
        let! command = getCommand ()

        match command with
        | Init subcmd -> return! subcmd |> InitArgs.ToOptions |> runInit
        | Search subcmd -> return! subcmd |> SearchArgs.ToOptions |> runSearch
        | Show subcmd -> return! subcmd |> ShowArgs.ToOptions |> runShow
        | Install subcmd -> return! subcmd |> InstallArgs.ToOptions |> runInstall
        | Uninstall subcmd -> return! subcmd |> UninstallArgs.ToOptions |> runUninstall
        | Version ->
            printfn
                "%A"
                (System
                    .Reflection
                    .Assembly
                    .GetEntryAssembly()
                    .GetName()
                    .Version)

            return! Ok 0
    }
    |> processExit
    |> Async.AwaitTask
    |> Async.RunSynchronously
