module Common 

open System

type Position = {
    X: int
    Y: int
}

type Board<'t> = {
    Cells: 't
    Size: int
    Score: int
    RNG: Random
    RNGSeed: int option
}

type Direction =
    | Up = 0
    | Down = 1
    | Left = 2
    | Right = 3

type BoardContext<'t> = {
    TrySwipe: 't -> Direction -> 't
    Clone: 't -> 't
    Create: int -> 't
    CreateWithSeed: int -> int -> 't
    CanSwipe: 't -> bool
    ToString: 't -> string
    Score: 't -> int
    RNG: 't -> Random
}
