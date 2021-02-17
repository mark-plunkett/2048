open System
open System.Numerics

[<EntryPoint>]
let main argv =

    let i = [[2;4;8;16;16;0;0;0;0;0;0;0;0;0;0;0;]] |> GameSIMD.Board.fromList
    let expected = [[2;4;8;16;16;0;0;0;0;0;0;0;0;0;0;0;]] |> GameSIMD.Board.fromList
    
    GameSIMD.swipeSIMD i |> ignore

    0