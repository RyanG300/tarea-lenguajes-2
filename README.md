# 🎮 Tarea Lenguajes de Programación 2

**Juegos de Sopa de Letras y Ahorcado**

Proyecto que integra F# (lógica de juegos) con C# (interfaz gráfica Avalonia UI).

## 🚀 Cómo empezar a usar este programa

### 📋 Prerrequisitos

- **.NET 9.0 SDK** o superior
- **Visual Studio Code** (recomendado) con extensiones:
  - C# for Visual Studio Code
  - Ionide-fsharp (para F#)

### 🛠️ Instalación y ejecución

1. **Clonar el repositorio:**

   ```bash
   git clone https://github.com/RyanG300/tarea-lenguajes-2.git
   cd tarea-lenguajes-2
   ```

2. **Restaurar dependencias:**

   ```bash
   dotnet restore
   ```

3. **Compilar el proyecto:**

   ```bash
   dotnet build
   ```

4. **Ejecutar la aplicación:**
   ```bash
   dotnet run --project MiGuiCs
   ```

### 🎯 Estructura del proyecto

- **`LogicaFs/`** - Lógica de juegos en F#
  - `AhorcadoGame.fs` - Funciones del juego del ahorcado
  - `SopaLetras.fs` - Generador y solver de sopa de letras
- **`MiGuiCs/`** - Interfaz gráfica en C# con Avalonia
  - `MainWindow.axaml/.cs` - Ventana principal
  - `Games/` - Ventanas de juegos individuales

### 🎮 Juegos incluidos

- **🎯 Ahorcado**: Adivina palabras letra por letra
- **🔍 Sopa de Letras**: Encuentra palabras escondidas en una matriz

### 📝 Personalización

- **Palabras del ahorcado**: Edita `MiGuiCs/Games/Lectura_games/palabrasAhorcado.txt`
- **Palabras sopa de letras**: Edita `MiGuiCs/Games/Lectura_games/palabrasSopaLetras.txt`

### 🔧 Comandos útiles

```bash
# Limpiar compilación
dotnet clean

# Reconstruir todo
dotnet rebuild

# Ejecutar en modo release
dotnet run --project MiGuiCs --configuration Release
```

---

_Proyecto desarrollado como tarea para la materia de Lenguajes de Programación_
