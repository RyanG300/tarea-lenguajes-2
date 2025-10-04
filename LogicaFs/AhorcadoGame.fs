namespace LogicaFs
open System.IO

module AhorcadoGame =
    let separarPalabras (texto: string) : string[] =
        texto.Split([| '\n'; '\r' |], System.StringSplitOptions.RemoveEmptyEntries)

    let palabraAleatoria (palabras: string[]) (rnd: System.Random) : string =
        let index = rnd.Next(palabras.Length)
        palabras.[index].Trim().ToLower()

    let comprobarLetra (palabra: string) (letra: char) : bool =
        palabra.Contains(letra)

    let actualizarEstado (palabra: string) (estadoActual: char[]) (letra: char) : char[] =
        estadoActual |> Array.mapi (fun i c -> if palabra.[i] = letra then letra else c)
    
    let colocarEspacios (estado: char[]) : string =
        estado |> Array.map (fun c -> string c) |> String.concat " "

    let revelarLetras (palabra: string) (estadoActual: string) (letra: char) : string =
        let estadoArray = estadoActual.ToCharArray()
        let nuevoEstado = actualizarEstado palabra estadoArray letra
        colocarEspacios nuevoEstado

    let stringToCharArray (s: string) : char[] =
        s.Replace(" ", "").ToCharArray()

    let esGanador (estadoActual: char[]) : bool =
        not (Array.exists (fun c -> c = '_') estadoActual)

    let tiempo (segundos: int) : string =
        let minutos = segundos / 60
        let segundosRestantes = segundos % 60
        sprintf "%d:%02d" minutos segundosRestantes

