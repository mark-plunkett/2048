open System

type Position = {
    X: int
    Y: int
}

type CellValue = int

type Board = Map<Position, CellValue>

type Direction =
| Up
| Down
| Left
| Right

let dumpBoard board =
    board
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
let randomPos max =
    { X = r.Next(1, max); Y = r.Next(1, max) }

let randomValue () =
    pown 2 (r.Next(1, 3))

let newBoard size =
    let randoms = set [(randomPos size); (randomPos size)]
    [ for x in [1..size] do
        for y in [1..size] do
            yield { X = x; Y = y} ]
    |> Seq.fold (fun board position -> 
        let cell = 
            if Set.contains position randoms 
            then randomValue() 
            else 0
        Map.add position cell board
    ) Map.empty

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
        board
        |> Map.toList
        |> List.where (fun (_, v) -> v > 0)
        |> List.where (fst >> cellFilter currentRowCol)
        |> List.map snd
        |> flatten
        |> cellSorter
        |> padList size 0
        |> List.indexed
        |> List.fold (cellFolder currentRowCol) board

    [1..size]
    |> List.fold rowFolder board

let test () =
    newBoardFromList [
        [ 0; 0; 2; 4 ]
        [ 2; 0; 2; 4 ]
        [ 2; 0; 2; 0 ]
        [ 0; 2; 8; 16 ]
    ] 

[<EntryPoint>]
let main argv =
    //let board = newBoard 4
    //dumpBoard board

    //printfn ""
    ////flatten [2;2;4;4]
    ////|> Seq.iter (printfn "%i")

    //swipe Up board
    //|> dumpBoard
    let swipe = swipe 4

    test ()
    |> dumpBoard
    |> swipe Up
    |> dumpBoard
    |> swipe Down
    |> dumpBoard
    |> swipe Left
    |> dumpBoard
    |> swipe Up
    |> dumpBoard
    |> swipe Left
    |> dumpBoard

    0