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

let dumpBoard origin board =
    Console.SetCursorPosition(origin.X, origin.Y)
    board.Cells
    |> Map.toSeq
    |> Seq.sortBy (fun (pos, _) -> pos.Y, pos.X)
    |> Seq.iter (fun (pos, cell) ->
        printf "%s" (if pos.X = 1 then Environment.NewLine else String.Empty)
        match cell with
        | 0 -> "    -"
        | v -> sprintf "%5i" v
        |> printf "%s"
    )
    printfn ""
    board

let r = Random()
let randomPos board =
    { X = r.Next(1, board.Size); Y = r.Next(1, board.Size) }

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

    let cellMapper currentRowCol i =
        match direction with
        | Up -> {X = currentRowCol; Y = i + 1}
        | Down -> {X = currentRowCol; Y = size - i}
        | Left -> {X = i + 1; Y = currentRowCol}
        | Right -> {X = size - i; Y = currentRowCol}

    let cellSorter cells =
        match direction with
        | Up | Left -> cells
        | Down | Right -> List.rev cells

    let cellFolder currentRowCol cells (i, newVal) =
        Map.add (cellMapper currentRowCol i) newVal cells

    let rowFolder board currentRowCol =
        let newCells = 
            board.Cells
            |> Map.toList
            |> List.where (snd >> (<) 0)
            |> List.where (fst >> cellFilter currentRowCol)
            |> List.map snd
            |> (flatten >> cellSorter >> padList size 0)
            |> List.indexed
            |> List.fold (cellFolder currentRowCol) board.Cells
        { board with Cells = newCells }

    List.fold rowFolder board [1..size]
