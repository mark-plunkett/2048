module Game

open System

type Position = {
    X: int
    Y: int
}

type Board = {
    Cells: Map<Position, int>
    Size: int
}

type Direction = | Up | Down | Left | Right

let boardToString board =
    board.Cells
    |> Map.toSeq
    |> Seq.sortBy (fun (pos, _) -> pos.Y, pos.X)
    |> Seq.fold (fun acc (pos, cell) ->
        acc
            + sprintf "%s" (if pos.X = 1 then Environment.NewLine else String.Empty)
            + match cell with
                | 0 -> "    -"
                | v -> sprintf "%5i" v
    ) String.Empty

let r = Random()
let randomPos board =
    { X = r.Next(1, board.Size + 1); Y = r.Next(1, board.Size + 1) }

let randomValue () =
    pown 2 (r.Next(1, 3))

let addRandomCell board =
    let rec addRec pos value board =
        match board.Cells.[pos] with
        | v when v = 0 -> { board with Cells = Map.add pos value board.Cells }
        | _ -> addRec (randomPos board) value board
    let value = randomValue()
    let pos = randomPos board
    addRec pos value board

let newBoard size =
    let cells =
        [ for x in [1..size] do
            for y in [1..size] do
                yield { X = x; Y = y}, 0 ]
        |> Map.ofList
    { Cells = cells; Size = size }
    |> addRandomCell
    |> addRandomCell

let newBoardFromList board =
    [ for y, row in List.indexed board do
        for x, cell in List.indexed row do
            yield ({ X = x + 1; Y = y + 1}, cell) ]
    |> List.fold (fun board (pos, cell) ->
        Map.add pos cell board
    ) Map.empty

let flatten cells =
    let rec flattenRec acc cells =
        match cells with
        | [] -> acc
        | [x] -> x::acc
        | first::second::rest ->
            if first = second then flattenRec ((first + second)::acc) rest
            else flattenRec (first::acc) (second::rest)

    flattenRec [] cells
    |> List.rev

let padList n value list =
    list@(List.replicate (n - List.length list) value)

let swipe size direction board =
    let cellFilter rowCol cellPos = 
        match direction with
        | Up | Down -> cellPos.X = rowCol
        | Left | Right -> cellPos.Y = rowCol

    let cellMapper rowOrCol i =
        match direction with
        | Up -> {X = rowOrCol; Y = i + 1}
        | Down -> {X = rowOrCol; Y = size - i}
        | Left -> {X = i + 1; Y = rowOrCol}
        | Right -> {X = size - i; Y = rowOrCol}

    let cellSorter cells =
        match direction with
        | Up | Left -> cells
        | Down | Right -> List.rev cells

    let cellFolder rowOrCol cells (i, newVal) =
        Map.add (cellMapper rowOrCol i) newVal cells

    let cellsFolder board rowOrCol =
        board
        |> Map.toList
        |> List.where (snd >> (<) 0)
        |> List.where (fst >> cellFilter rowOrCol)
        |> List.map snd
        |> (flatten >> cellSorter >> padList size 0)
        |> List.indexed
        |> List.fold (cellFolder rowOrCol) board

    { board with Cells = List.fold cellsFolder board.Cells [1..size]}

let cellsCanMove cells =
    List.exists (fun value -> value = 0) cells
    || cells
        |> List.pairwise
        |> List.exists (fun (a, b) -> a = b)

let canSwipeOrientation grouping board =
    board.Cells
    |> Map.toList
    |> List.groupBy grouping
    |> List.map (snd >> List.map snd)
    |> List.exists cellsCanMove
    
let canSwipeHorizontal board =
    canSwipeOrientation (fun ({X = _; Y = y}, _) -> y) board

let canSwipeVertical board =
    canSwipeOrientation (fun ({X = x; Y = _}, _) -> x) board

let canSwipe board =
    canSwipeVertical board || canSwipeHorizontal board