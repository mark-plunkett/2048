module GameFunctional

open System

open Common

module Board =

    let empty size =
        {
            Cells = Map.empty
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

    let addRandomCell (board:Board<Map<Position, int>>) =
        let rec addRec pos value board =
            match Map.find pos board.Cells with
            | v when v = 0 -> { board with Cells = Map.add pos value board.Cells }
            | _ -> addRec (randomPos board) value board
        let value = randomValue board
        let pos = randomPos board
        addRec pos value board

    let init board =
        let cells =
            [ for x in [1..board.Size] do
                for y in [1..board.Size] do
                    yield { X = x; Y = y }, 0 ]
            |> Map.ofList
        { board with Cells = cells }
        |> addRandomCell
        |> addRandomCell

    let create = empty >> init

    let clone board =
        match board.RNGSeed with
        | Some s -> { board with RNG = Random(s) }
        | None -> { board with RNG = Random() }

    let toString board =
        let newLine = Environment.NewLine
        board.Cells
        |> Map.toSeq
        |> Seq.sortBy (fun (pos, _) -> pos.Y, pos.X)
        |> Seq.fold (fun acc (pos, cell) ->
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

    let cellFolder rowOrCol cells (i, newVal) =
        Map.add (cellMapper rowOrCol i) newVal cells

    let cellsFolder board rowOrCol =
        let flattened, score =
            board.Cells
            |> Map.toList
            |> List.where (snd >> (<) 0)
            |> List.where (fst >> cellFilter rowOrCol)
            |> List.map snd
            |> flatten

        let flattened' =
            flattened
            |> cellSorter 
            |> padList board.Size 0
            |> List.indexed
            |> List.fold (cellFolder rowOrCol) board.Cells
        
        { board with 
            Cells = flattened'
            Score = board.Score + score }

    List.fold cellsFolder board [1..board.Size] 

let trySwipe board direction =
    let newBoard = swipe board direction 
    if board = newBoard then board
    else Board.addRandomCell newBoard

let canAnyCellsMove cells =
    List.exists ((=) 0) cells 
    || cells
        |> List.pairwise
        |> List.exists (fun (a, b) -> a = b)

let canSwipeOrientation grouping board =
    board.Cells
    |> Map.toList
    |> List.groupBy grouping
    |> List.map (snd >> List.map snd)
    |> List.exists canAnyCellsMove
    
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
    CreateWithSeed = fun size seed -> Board.emptyWithSeed size seed |> Board.init
    CanSwipe = canSwipe
    ToString = Board.toString
}
