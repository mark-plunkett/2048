module Args

open Argu

type Args =
| Human
| MonteCarlo of int * int
| Size of int 

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Human -> "specify human solver (interactive)."
            | MonteCarlo _ -> "specify monte-carlo 'AI' solver with number of branches and branch depth."
            | Size _ -> "specify board size."