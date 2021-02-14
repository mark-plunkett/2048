module GameArray

open System
open System.Collections.Generic

open Common

module Board =

    (*
        This board uses a 1 dimensional array 16 of uint16s e.g.
        
        [ 0; 2; 256; 4; 16; 2; 4; 0; 2; 8; 4; 2; 0; 128; 16; 4 ]
        
        which represents the following board

          0   2 256   4 
         16   2   4   0 
          2   8   4   2 
          0 128  16   4
    *)

    let size = 4 // Fixed size for performance reasons
    let boardSize = size * size
    let empty (_:int) =
        {
            Cells = Array.zeroCreate<uint16> boardSize
            Size = size
            Score = 0
            RNG = Random()
            RNGSeed = None
        } 
    
    let emptyWithSeed _ seed =
        { empty size with
            RNG = Random(seed)
            RNGSeed = Some seed }

    let xyToIndex size x y =
        size * y + x

    let posToIndex size pos =
        xyToIndex size pos.X pos.Y

    let randomPos board =
        { X = board.RNG.Next(0, board.Size); Y = board.RNG.Next(0, board.Size) }

    let randomValue board =
        uint16 <| if board.RNG.NextDouble() > 0.9 then 4 else 2

    let addRandomCell (board:Board<uint16[]>) =
        let rec addRec pos value (board:Board<uint16[]>) =
            let i = posToIndex size pos
            match board.Cells.[i] with
            | v when v = 0us -> 
                board.Cells.[i] <- value
                board
            | _ -> 
                addRec (randomPos board) value board
        let value = randomValue board
        let pos = randomPos board
        addRec pos value board

    let init board =
        board
        |> addRandomCell
        |> addRandomCell

    let create = empty >> init

    let clone (board:Board<uint16[]>) =
        let random =
            match board.RNGSeed with
            | Some s -> Random(s)
            | None -> Random()
        let cells' = Array.zeroCreate boardSize
        Array.blit board.Cells 0 cells' 0 boardSize
        { board with
            Cells = cells'
            RNG = random }

    let toString (board:Board<uint16[]>) =
        let newLine = Environment.NewLine
        board.Cells
        |> Array.indexed
        |> Seq.fold (fun acc (i, v) ->
            acc
                + if i % size = 0 then newLine + newLine else String.Empty
                + match v with
                    | 0us -> "    -"
                    | v -> sprintf "%5i" v
        ) newLine

    let fromList board =
        board
        |> List.collect id
        |> List.toArray
        |> Array.map uint16

let flatten cells =
    let rec flattenRec acc cells score =
        match cells with
        | [] -> acc, score
        | [x] -> x::acc, score
        | first::second::rest ->
            if first = second then 
                let total = first + second
                flattenRec (total::acc) rest (score + total)
            else flattenRec (first::acc) (second::rest) score

    let cells', score = flattenRec [] cells 0
    List.rev cells', score

let swipe board direction =
    board

let trySwipe (board:Board<uint16[]>) direction =
    board

let canAnyCellsMove cells =
    Array.exists ((=) 0) cells 
    || cells
        |> Array.pairwise
        |> Array.exists (fun (a, b) -> a = b)

let canSwipeHorizontal board =
    false

let canSwipe board =
    true