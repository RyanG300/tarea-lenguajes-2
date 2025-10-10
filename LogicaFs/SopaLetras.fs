namespace LogicaFs

module public SopaLetras =

    open System

    let randomInt (rnd: Random) (max:int) : int * Random =
        rnd.Next(max), rnd


    let randomDouble (rnd: Random) =
        rnd.NextDouble(), rnd

    /// Crear matriz vacía
    let crearMatriz filas columnas =
        Array2D.create filas columnas ' '

    /// Coloca palabra en matriz de forma
    let ponerPalabraDir (matriz: char[,]) (palabra: string) (fila:int) (columna:int) (dr:int) (dc:int) =
        let largo = palabra.Length
        let filas = Array2D.length1 matriz
        let columnas = Array2D.length2 matriz

        let endRow = fila + dr * (largo - 1)
        let endCol = columna + dc * (largo - 1)

        if endRow < 0 || endRow >= filas || endCol < 0 || endCol >= columnas then
            matriz, false
        else
            let puede = 
                [|0 .. largo-1|] |> Array.forall (fun i ->
                    let r = fila + dr*i
                    let c = columna + dc*i
                    matriz.[r,c] = ' ' || matriz.[r,c] = palabra.[i]
                )
            if puede then
                // Crear un mapa inmutable de posiciones a caracteres para la palabra
                let posList : ((int*int) * char) list = [ for k in 0..largo-1 -> ((fila + dr*k, columna + dc*k), palabra.[k]) ]
                let posMap : Map<int*int,char> = Map.ofList posList
                let nueva = Array2D.init filas columnas (fun (i:int) (j:int) ->
                    match Map.tryFind (i,j) posMap with
                    | Some ch -> ch
                    | None -> matriz.[i,j]
                )
                nueva, true
            else
                matriz, false

    /// Rellenar matriz con letras aleatorias
    let rellenarAleatorio (matriz: char[,]) (rnd: Random) =
        let filas = Array2D.length1 matriz
        let columnas = Array2D.length2 matriz

        let rec loop (i:int) (j:int) (mat: char[,]) (rnd: Random) : char[,] * Random =

            if i >= filas then mat, rnd
            elif j >= columnas then loop (i+1) 0 mat rnd
            else
                let ch = mat.[i,j]
                if ch = ' ' then
                    let letra, rnd1 = randomInt rnd 26
                    let nuevaMat: char[,] = Array2D.mapi (fun r c v -> if r = i && c = j then char (letra + int 65) else v) mat
                    loop i (j+1) nuevaMat rnd1
                else
                    loop i (j+1) mat rnd
        loop 0 0 matriz rnd

    /// Obtener línea de coordenadas alineadas
    let obtenerLineaAlineada (sr:int) (sc:int) (tr:int) (tc:int) : (int*int)[] =
        let dr = tr - sr
        let dc = tc - sc
        if dr = 0 && dc = 0 then [| (sr, sc) |]
        elif not (dr = 0 || dc = 0 || abs dr = abs dc) then [||]
        else
            let stepR = if dr = 0 then 0 elif dr > 0 then 1 else -1
            let stepC = if dc = 0 then 0 elif dc > 0 then 1 else -1
            let steps = max (abs dr) (abs dc)
            [| for k in 0 .. steps -> (sr + k*stepR, sc + k*stepC) |]

    let mezclar (arr: 'a[]) (rnd: Random) =
        let rec shuffle lst rnd =
            match lst with
            | [] -> [], rnd
            | _ ->
                let n = List.length lst
                let idx, rnd1 = randomInt rnd n
                let elem = List.item idx lst
                let rest = lst |> List.mapi (fun i x -> i, x) |> List.filter (fun (i,_) -> i<>idx) |> List.map snd
                let shuffledRest, rnd2 = shuffle rest rnd1
                elem :: shuffledRest, rnd2
        let l, r = shuffle (Array.toList arr) rnd
        Array.ofList l, r

    /// Seleccionar palabras que quepan en el tablero
    let seleccionarPalabras (palabras: string[]) maxPalabras filas columnas rnd =
        let maxLen = max filas columnas
        let filtradas = palabras |> Array.filter (fun p -> p <> null && p.Trim() <> "" && p.Length <= maxLen)
        mezclar filtradas rnd |> fun (arr, rnd1) -> arr |> Array.truncate maxPalabras, rnd1

    /// Intentar colocar palabras en la matriz
    let rec intentarColocar (palabra:string) (matriz:char[,]) (directions:(int*int)[]) (rnd:Random) (intentos:int) (maxIntentos:int) =
        if intentos >= maxIntentos then matriz, false, rnd
        else
            let filas = Array2D.length1 matriz
            let columnas = Array2D.length2 matriz
            let fila, rnd1 = randomInt rnd filas
            let col, rnd2 = randomInt rnd1 columnas
            let dirIndex, rnd3 = randomInt rnd2 directions.Length
            let dr, dc = directions.[dirIndex]
            let palabraFinal, rnd4 =
                let d, rnd5 = randomDouble rnd3
                if d < 0.5 then palabra, rnd5 else (new string (palabra.ToCharArray() |> Array.rev)), rnd5
            let nuevaMatriz, exito = ponerPalabraDir matriz palabraFinal fila col dr dc
            if exito then nuevaMatriz, true, rnd4
            else intentarColocar palabra matriz directions rnd4 (intentos+1) maxIntentos

    /// Generar sopa de letras
    let generarSopa (palabras: string[]) (filas:int) (columnas:int) (rnd: Random) : char[,] * string[] =
        let directions = [| (-1,0); (-1,1); (0,1); (1,1); (1,0); (1,-1); (0,-1); (-1,-1) |]
        let maxPalabras = 10

        let palabrasNorm = palabras |> Array.map (fun p -> if p = null then "" else p.Trim().ToUpper())
        let candidatos, rnd1 = mezclar (palabrasNorm |> Array.filter (fun p -> p <> "" && p.Length <= max filas columnas)) rnd

        let rec loop idx matriz placed rnd =
            if idx >= candidatos.Length || List.length placed >= maxPalabras then matriz, List.rev placed, rnd
            else
                let palabra = candidatos.[idx]
                let nuevaMatriz, exito, rnd1 = intentarColocar palabra matriz directions rnd 0 200
                let nuevaPlaced = if exito then palabra::placed else placed
                loop (idx+1) nuevaMatriz nuevaPlaced rnd1

        let matrizInicial = crearMatriz filas columnas
        let matrizFinal, palabrasColocadas, rndFinal = loop 0 matrizInicial [] rnd1
        let matrizRellenada, _ = rellenarAleatorio matrizFinal rndFinal
        matrizRellenada, palabrasColocadas |> List.toArray

    /// Comprueba si una palabra está en el arreglo de palabras.
    let comprobarPalabra (pal:string) (placed:string[]) : string =
        if String.IsNullOrWhiteSpace pal then null
        else
            let p = pal.Trim().ToUpper()
            let rev = new string (p.ToCharArray() |> Array.rev)
            if Array.exists (fun x -> x = p) placed then p
            elif Array.exists (fun x -> x = rev) placed then rev
            else null

    /// Comprueba una secuencia de coordenadas
    let ComprobarPorCoordenadasValueTuples (mat: char[,]) (coords: (int*int)[]) (placed:string[]) : string =
        if mat = null || coords = null || coords.Length = 0 then null
        else
            try
                let chars = coords |> Array.map (fun (r,c) -> mat.[r,c])
                let s = System.String(chars)
                comprobarPalabra s placed
            with
            | _ -> null

    /// Intenta encontrar palabra por coordenadas
    let TryFindByCoords (mat: char[,]) (coords: (int*int)[]) (placedWords: string[]) (foundWords: Set<string>) =
        let matched = ComprobarPorCoordenadasValueTuples mat coords placedWords
        if String.IsNullOrEmpty(matched) then matched, foundWords
        else matched, Set.add matched foundWords

    /// Obtener palabras ya encontradas
    let GetFoundWords (foundWords: Set<string>) : string[] =
        foundWords |> Set.toArray


  
    // Buscador automático


    /// Devuelve las posiciones iniciales donde comienza la palabra
    let posicionesIniciales (sopa: char[,]) (palabra: string) =
        if sopa = null || String.IsNullOrEmpty(palabra) then []
        else
            let filas = Array2D.length1 sopa
            let columnas = Array2D.length2 sopa
            [ for i in 0 .. filas - 1 do
                for j in 0 .. columnas - 1 do
                    if sopa.[i,j] = palabra.[0] then
                        yield (i,j) ]


    let vecinos (fila,col) ruta sopa =
        let filas = Array2D.length1 sopa
        let columnas = Array2D.length2 sopa

        let dentro (i,j) = i >= 0 && i < filas && j >= 0 && j < columnas
        let direcciones = [ (-1,0); (1,0); (0,-1); (0,1); (-1,-1); (-1,1); (1,-1); (1,1) ]

        if List.length ruta = 1 then
            // Primera letra: todas las direcciones posibles
            direcciones
            |> List.map (fun (di,dj) -> (fila+di, col+dj))
            |> List.filter dentro
            |> List.filter (fun coord -> not (List.contains coord ruta))
        else
            // Ya hay dirección: continuar en línea recta
            let (prevFila, prevCol) = List.item 1 ruta
            let di = fila - prevFila
            let dj = col - prevCol
            let siguiente = (fila + di, col + dj)
            if dentro siguiente && not (List.contains siguiente ruta) then [siguiente]
            else []
    let extender ruta sopa =
        vecinos (List.head ruta) ruta sopa
        |> List.map (fun coord -> coord :: ruta)


    let rec profAux fin rutas sopa =
        match rutas with
        | [] -> []
        | ruta :: resto ->
            let nodoActual = List.head ruta
            let nuevasRutas = extender ruta sopa
            if nodoActual = fin then
                (List.rev ruta) :: profAux fin (resto @ nuevasRutas) sopa
            else
                profAux fin (resto @ nuevasRutas) sopa

    let prof ini fin sopa =
        profAux fin [[ini]] sopa

    /// Buscar rutas de longitud exacta
    let buscarRutasPorLongitud (ini: int*int) (longitud: int) (sopa: char[,]) : (int*int) list[] =
        let rec aux pendientes resultados =
            match pendientes with
            | [] -> Array.ofList (List.rev resultados)
            | ruta :: resto ->
                if List.length ruta = longitud then
                    // ruta ya tiene la longitud deseada: añadir a resultados (invertida a orden inicio->fin)
                    aux resto ((List.rev ruta) :: resultados)
                else
                    let ext = extender ruta sopa
                    aux (resto @ ext) resultados
        aux [[ini]] []
        
    let buscarRutaParaPalabra (sopa: char[,]) (palabra: string) : (int*int)[] =
        if sopa = null || String.IsNullOrEmpty(palabra) then [||]
        else
            let p = palabra.Trim().ToUpper()
            if p = "" then [||] else
            let largo = p.Length
            let filas = Array2D.length1 sopa
            let columnas = Array2D.length2 sopa
    
            let inis = posicionesIniciales sopa p
            let finales = [ for i in 0 .. filas - 1 do for j in 0 .. columnas - 1 do if sopa.[i,j] = p.[largo-1] then yield (i,j) ]

            let tryRutaFromIni ini =
                finales
                |> List.tryPick (fun finCoord ->
                    let rutas = prof ini finCoord sopa // devuelve (int*int) list list
                    rutas
                    |> List.tryFind (fun rutaList ->
                        try
                            if List.length rutaList <> largo then false
                            else
                                let chars = rutaList |> List.map (fun (r,c) -> sopa.[r,c]) |> Array.ofList
                                let s = System.String(chars)
                                s = p
                        with _ -> false)
                    |> Option.map (fun rl -> rl |> List.toArray)
                )
            let rec buscar = function
                | [] -> [||]
                | ini :: resto ->
                    match tryRutaFromIni ini with
                    | Some rutaArr -> rutaArr
                    | None -> buscar resto

            buscar inis

    let SolveAll (sopa: char[,]) (placed: string[]) : (string * (int*int)[])[] =
        if sopa = null || placed = null then [||]
        else
            placed |> Array.map (fun w ->
                let palabra = if w = null then "" else w.Trim().ToUpper()
                let ruta = buscarRutaParaPalabra sopa palabra
                (palabra, ruta)
            )

