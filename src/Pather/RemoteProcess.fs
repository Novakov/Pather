module Pather.RemoteProcess

open System
open System.Diagnostics
open System.IO.Pipes
open System.IO
open System.Text
open Pather

let private injectionPath =
    let basePath = System.AppDomain.CurrentDomain.BaseDirectory
    {
        Native.LibraryPath.Path86 = Path.Combine(basePath, "Injections", "Injection.x86.dll")
        Native.LibraryPath.Path64 = Path.Combine(basePath, "Injections", "Injection.x64.dll")
    }

let openChannel (processId: int) =
    let proc = Process.GetProcessById(processId)
    
    let pipeName = sprintf "pather\%d" processId
    
    let pipe = new NamedPipeServerStream(pipeName)    

    Native.injectLibrary proc.Handle injectionPath

    pipe.WaitForConnection()

    pipe

let pingPong (channel: NamedPipeServerStream) =
    channel.WriteByte(45uy)

    channel.ReadByte() = 54

let setEnvVar (channel: NamedPipeServerStream) (variable: string) (value: string) =
    use writer = new BinaryWriter(channel, Encoding.Unicode)

    writer.Write(01uy)
    writer.Write(variable)
    writer.Write(value)

    channel

let readEnvVar (channel: NamedPipeServerStream) (variable: string) =
    use writer = new BinaryWriter(channel, Encoding.Unicode)
    writer.Write(02uy)
    writer.Write(variable)
    
    use reader = new BinaryReader(channel, Encoding.Unicode)
    reader.ReadString()    

let readPathSet (processId : int) = 
    use channel = openChannel processId
    readEnvVar channel "PATH" |> PathSet.fromEnvVar

let setPath (processId : int) (path : PathSet.PathSet) = 
    use channel = openChannel processId
    setEnvVar channel "PATH" (PathSet.toEnvVar path) |> ignore
