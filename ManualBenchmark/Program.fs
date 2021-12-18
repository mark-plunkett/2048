open System

[<EntryPoint>]
let main argv =

    let duration = Array.tryHead argv |> Option.map (Int32.Parse) |> Option.defaultValue 5
    
    0