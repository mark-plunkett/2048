module GameDict

open System
open System.Collections.Generic

open Common

module Board =

    let empty size =
        {
            Cells = Dictionary()
            Size = size
            Score = 0
            RNG = Random()
            RNGSeed = None
        } 
    
    let emptyWithSeed size seed =
        { empty size with
            RNG = Random(seed)
            RNGSeed = Some seed }

    let randomPos board =
        { X = board.RNG.Next(1, board.Size + 1); Y = board.RNG.Next(1, board.Size + 1) }

    let randomValue board =
        if board.RNG.NextDouble() > 0.9 then 4 else 2

    let addRandomCell (board:Board<Dictionary<Position, int>>) =
        let rec addRec pos value (board:Board<Dictionary<Position, int>>) =
            match board.Cells.[pos] with
            | v when v = 0 -> 
                board.Cells.[pos] <- value
                board
            | _ -> 
                addRec (randomPos board) value board
        let value = randomValue board
        let pos = randomPos board
        addRec pos value board

    let init board =
        let cells =
            [ for x in [1..board.Size] do
                for y in [1..board.Size] do
                    yield { X = x; Y = y } ]
            |> List.map (fun p -> KeyValuePair(p, 0))
            |> fun ps -> Dictionary<Position, int>(ps)
        { board with Cells = cells }
        |> addRandomCell
        |> addRandomCell

    let create = empty >> init

    let clone (board:Board<Dictionary<Position, int>>) =
        let random =
            match board.RNGSeed with
            | Some s -> Random(s)
            | None -> Random()
        { board with
            Cells = Dictionary(board.Cells)
            RNG = random }

    let toString (board:Board<Dictionary<Position, int>>) =
        let newLine = Environment.NewLine
        board.Cells
        |> Seq.sortBy (fun kvp -> kvp.Key.Y, kvp.Key.X)
        |> Seq.fold (fun acc kvp ->
            let pos =  kvp.Key
            let cell = kvp.Value
            acc
                + if pos.X = 1 && pos.Y > 1 then newLine + newLine else String.Empty
                + match cell with
                    | 0 -> "    -"
                    | v -> sprintf "%5i" v
        ) newLine

    let fromList board =
        [ for y, row in List.indexed board do
            for x, cell in List.indexed row do
                yield ({ X = x + 1; Y = y + 1}, cell) ]
        |> List.fold (fun board (pos, cell) ->
            Map.add pos cell board
        ) Map.empty

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

let padList n value list =
    list@(List.replicate (n - List.length list) value)

let swipe board direction =
    let cellFilter rowCol cellPos = 
        match direction with
        | Up | Down -> cellPos.X = rowCol
        | Left | Right -> cellPos.Y = rowCol

    let cellMapper rowOrCol i =
        match direction with
        | Up -> {X = rowOrCol; Y = i + 1}
        | Down -> {X = rowOrCol; Y = board.Size - i}
        | Left -> {X = i + 1; Y = rowOrCol}
        | Right -> {X = board.Size - i; Y = rowOrCol}

    let cellSorter cells =
        match direction with
        | Up | Left -> cells
        | Down | Right -> List.rev cells

    let cellFolder rowOrCol (cells:Dictionary<Position, int>) (i, newVal) =
        let pos = cellMapper rowOrCol i
        cells.[pos] <- newVal
        cells

    let cellsFolder (board:Board<Dictionary<Position, int>>) rowOrCol =
        let flattened, score =
            board.Cells
            |> Seq.toList
            |> List.filter (fun kvp -> kvp.Value > 0 && (cellFilter rowOrCol kvp.Key))
            |> List.map (fun kvp -> kvp.Value)
            |> flatten

        flattened
        |> cellSorter 
        |> padList board.Size 0
        |> List.indexed
        |> List.fold (cellFolder rowOrCol) board.Cells
        |> ignore
    
        { board with Score = board.Score + score }

    List.fold cellsFolder board [1..board.Size] 

let trySwipe (board:Board<Dictionary<Position, int>>) direction =
    let before = Array.zeroCreate board.Cells.Values.Count
    board.Cells.Values.CopyTo(before, 0)
    let newBoard = swipe board direction
    let noChanges =
        Seq.zip before newBoard.Cells.Values 
        |> Seq.forall (fun (a, b) -> a = b)
    if noChanges then board
    else Board.addRandomCell newBoard

let canAnyCellsMove cells =
    Array.exists ((=) 0) cells 
    || cells
        |> Array.pairwise
        |> Array.exists (fun (a, b) -> a = b)

let canSwipeOrientation grouping (board:Board<Dictionary<Position, int>>) =
    board.Cells
    |> Seq.toArray
    |> Array.map (fun kvp -> kvp.Key, kvp.Value)
    |> Array.groupBy grouping
    |> Array.map (snd >> Array.map snd)
    |> Array.exists canAnyCellsMove
    
let canSwipeHorizontal board =
    canSwipeOrientation (fun ({X = _; Y = y}, _) -> y) board

let canSwipeVertical board =
    canSwipeOrientation (fun ({X = x; Y = _}, _) -> x) board

let canSwipe board =
    canSwipeVertical board || canSwipeHorizontal board

let boardContext = {
    TrySwipe = trySwipe
    Clone = Board.clone
    Create = Board.create
    CreateWithSeed = fun i j -> Board.emptyWithSeed i j |> Board.init
    CanSwipe = canSwipe
    ToString = Board.toString
}
