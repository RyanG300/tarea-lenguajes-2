namespace LogicaFs


module Greeting =
    let GetGreeting (name: string) : string =
        sprintf "¡Hola %s desde F#, sos muy negro!" name