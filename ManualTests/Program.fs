open System
open System.Numerics

[<EntryPoint>]
let main argv =

    let t = GameSIMD.Board.fromList [
        [ 2; 4; 8; 16 ]
        [ 4; 8; 16; 32 ]
        [ 16; 4; 32; 64 ]
        [ 16; 32; 64; 128 ] ]   
    let b = { GameSIMD.Board.empty 4 with Cells = t }
    printfn "%A" <| GameSIMD.canSwipe b

    0