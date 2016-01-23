module Pather.ToolHelp

open System
open System.Runtime.InteropServices
open Microsoft.Win32.SafeHandles

[<DllImport("kernel32.dll", SetLastError = true)>]
extern bool CloseHandle(IntPtr hObject)

type SnapshotFlags = 
    | HeapList = 0x00000001
    | Process  = 0x00000002
    | Thread   = 0x00000004
    | Module   = 0x00000008
    | Module32 = 0x00000010
    | Inherit  = 0x80000000
    | NoHeaps = 0x40000000

type SnapshotHandle() = 
    class
        inherit SafeHandleZeroOrMinusOneIsInvalid(true)
        override this.ReleaseHandle() = CloseHandle(this.handle)
    end

[<StructLayout(LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Unicode)>]
type ModuleEntry = 
    struct
        val mutable EntrySize : int
        val mutable ModuleId : int
        val mutable ProcessId : int
        val mutable Reserved0 : int
        val mutable Reserved1 : int
        val mutable BaseAddress : IntPtr
        val mutable ModuleSize : int
        val mutable ModuleHandle : IntPtr
        [<MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)>]
        val mutable ModuleName : string
        [<MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)>]
        val mutable ModulePath : string
    end

[<DllImport("kernel32.dll", SetLastError = true)>]
extern SnapshotHandle CreateToolhelp32Snapshot(SnapshotFlags dwFlags, int th32ProcessID)

[<DllImport("kernel32.dll", EntryPoint = "Module32FirstW")>]
extern bool Module32First(SnapshotHandle hSnapshot, ModuleEntry& lpme)

[<DllImport("kernel32.dll", EntryPoint = "Module32NextW")>]
extern bool Module32Next(SnapshotHandle hSnapshot, ModuleEntry& lpme)

let createSnapshot (flags : SnapshotFlags) (processId : int) = CreateToolhelp32Snapshot(flags, processId)

let modules (snapshot : SnapshotHandle) = 
    seq {         
        let mutable entry = new ModuleEntry()
        entry.EntrySize <- Marshal.SizeOf<ModuleEntry>()        

        Module32First(snapshot, &entry) |> ignore
        
        yield entry

        while Module32Next(snapshot, &entry) do
            yield entry
    }
