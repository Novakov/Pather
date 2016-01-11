module Pather.Native

open System
open System.Text
open System.Runtime.InteropServices
open Microsoft.FSharp.NativeInterop

[<StructLayout(LayoutKind.Sequential, Pack = 1)>]
type ProcessBasicInformation = struct
      val mutable ExitStatus: IntPtr;
      val mutable PebBaseAddress: nativeptr<byte>;
      val mutable AffinityMask: IntPtr;
      val mutable BasePriority: IntPtr;
      val mutable UniqueProcessId: UIntPtr;
      val mutable InheritedFromUniqueProcessId: IntPtr;    
end

[<StructLayout(LayoutKind.Sequential)>]
type PEB = struct    
    val mutable Reserved1_0: Byte;
    val mutable Reserved1_1: Byte;
    
    val mutable BeingDebugged: Byte;
       
    val mutable Reserved2_0: Byte; 
    
    val mutable Reserved3_0: IntPtr;
    val mutable Reserved3_1: IntPtr;

    val mutable Ldr: IntPtr;

    val mutable ProcessParameters: nativeptr<byte>;
end

[<StructLayout(LayoutKind.Sequential)>]
type UnicodeString = struct
    val mutable Length: UInt16;
    val mutable MaximumLength: UInt16;   
    val mutable Buffer: nativeptr<char>;
end

[<StructLayout(LayoutKind.Sequential)>]
type RtlString = struct
    val mutable Length: UInt16;
    val mutable MaximumLength: UInt16;   
    val mutable Buffer: nativeptr<Byte>;
end

[<StructLayout(LayoutKind.Sequential, Pack = 1)>]
[<StructAttribute>]
type CurDir = struct
    val mutable DosPath: UnicodeString;
    val mutable Handle: nativeint;
end

[<StructLayout(LayoutKind.Sequential, Pack = 1)>]
[<StructAttribute>]
type RtlDriveLetterCurDir = struct
    val mutable Flags: UInt16
    val mutable Length: UInt16
    val mutable TimeStamp: UInt32
    val mutable DosPath: RtlString
end

[<StructLayout(LayoutKind.Sequential)>]
[<StructAttribute>]
type RTLUserProcessParameters = struct        
    val mutable MaximumLength    : UInt32
    val mutable Length           : UInt32
    val mutable Flags            : UInt32
    val mutable DebugFlags       : UInt32
    val mutable ConsoleHandle    : nativeint
    val mutable ConsoleFlags     : UInt32
    val mutable StandardInput    : nativeint
    val mutable StandardOutput   : nativeint
    val mutable StandardError    : nativeint
    val mutable CurrentDirectory : CurDir
    val mutable DllPath          : UnicodeString
    val mutable ImagePathName    : UnicodeString
    val mutable CommandLine      : UnicodeString
    val mutable Environment      : nativeint
    val mutable StartingX        : UInt32
    val mutable StartingY        : UInt32
    val mutable CountX           : UInt32
    val mutable CountY           : UInt32
    val mutable CountCharsX      : UInt32
    val mutable CountCharsY      : UInt32
    val mutable FillAttribute    : UInt32
    val mutable WindowFlags      : UInt32
    val mutable ShowWindowFlags  : UInt32
    val mutable WindowTitle      : UnicodeString
    val mutable DesktopInfo      : UnicodeString
    val mutable ShellInfo        : UnicodeString
    val mutable RuntimeData      : UnicodeString
    [<MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)>]
    val mutable CurrentDirectores : RtlDriveLetterCurDir array
    val mutable EnvironmentSize  : UInt64
    val mutable EnvironmentVersion : Byte
    val mutable PackageDependencyData : nativeint
    val mutable ProcessGroupId   : UInt32
    val mutable LoaderThreads    : UInt32
end

[<StructLayout(LayoutKind.Sequential)>]
[<StructAttribute>]
type MemoryBasicInformation = struct
    val mutable BaseAddress:IntPtr;
    val mutable AllocationBase:IntPtr;
    val mutable AllocationProtect:int;
    val mutable RegionSize:IntPtr;
    val mutable State:int;
    val mutable Protect:int;
    val mutable Type:int;
end    

[<Flags>]
type AllocationType = 
     | Commit = 0x1000
     | Reserve = 0x2000
     | Decommit = 0x4000
     | Release = 0x8000
     | Reset = 0x80000
     | Physical = 0x400000
     | TopDown = 0x100000
     | WriteWatch = 0x200000
     | LargePages = 0x20000000

type MemoryProtection =
     | Execute = 0x10
     | ExecuteRead = 0x20
     | ExecuteReadWrite = 0x40
     | ExecuteWriteCopy = 0x80
     | NoAccess = 0x01
     | ReadOnly = 0x02
     | ReadWrite = 0x04
     | WriteCopy = 0x08
     | GuardModifierflag = 0x100
     | NoCacheModifierflag = 0x200
     | WriteCombineModifierflag = 0x400

[<DllImport("ntdll.dll", SetLastError = true)>]
extern int NtQueryInformationProcess(IntPtr processHandle, int processInformationClass, IntPtr processInformation, int processInformationLength, IntPtr returnLength);

[<DllImport("kernel32.dll", SetLastError = true)>]
extern Int32 ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr buffer, int size, int * lpNumberOfBytesRead);

[<DllImport("kernel32.dll", SetLastError = true)>]
extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr lpBuffer, int nSize, int * lpNumberOfBytesWritten);

[<DllImport("kernel32.dll")>]
extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, MemoryBasicInformation * lpBuffer, int dwLength);

[<DllImport("kernel32.dll", EntryPoint = "RtlFillMemory", SetLastError = false)>]
extern void FillMemory(IntPtr destination, int length, byte fill);

[<DllImport("kernel32.dll", SetLastError = true)>]
extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, IntPtr dwSize, AllocationType flAllocationType, MemoryProtection flProtect);

let getProcessBasicInfo (handle: IntPtr) =
    let basicInfoPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typedefof<ProcessBasicInformation>));

    NtQueryInformationProcess(handle, 0, basicInfoPtr, Marshal.SizeOf(typedefof<ProcessBasicInformation>), IntPtr.Zero) |> ignore

    let basicInfo = Marshal.PtrToStructure(basicInfoPtr, typedefof<ProcessBasicInformation>) :?> ProcessBasicInformation

    Marshal.FreeHGlobal(basicInfoPtr)

    basicInfo

let read<'t when 't:unmanaged> (processHandle: IntPtr) (address: nativeptr<byte>) =
    let size = Marshal.SizeOf(typedefof<'t>)
    let buffer = Marshal.AllocHGlobal(size)

    let mutable rr = 0

    let r = ReadProcessMemory(processHandle, NativePtr.toNativeInt address, buffer, size, &&rr)

    let result = NativePtr.ofNativeInt<'t> buffer |> NativePtr.read

    Marshal.FreeHGlobal(buffer)

    result

let write<'t when 't: unmanaged> (processHandle: IntPtr) (address: nativeptr<byte>) (value: 't) =
    let mutable x = value

    let ptr = &&x |> NativePtr.toNativeInt

    let mutable written = 0

    WriteProcessMemory(processHandle, NativePtr.toNativeInt address, ptr, sizeof<'t>, &&written)

let readField<'s, 't when 't: unmanaged> (fieldName:string) (processHandle: IntPtr) (baseAddress: nativeptr<byte>) =
    let offset = Marshal.OffsetOf(typedefof<'s>, fieldName).ToInt32()
    let effectiveAddress = NativePtr.add baseAddress offset

    read<'t> processHandle effectiveAddress

let readRange<'t when 't: unmanaged> (processHandle: IntPtr) (baseAddress: nativeptr<byte>) (size: int) =
    let mutable readBytes = 0

    let buffer = Marshal.AllocHGlobal(size)

    ReadProcessMemory(processHandle, NativePtr.toNativeInt baseAddress, buffer, size, &&readBytes) |> ignore

    NativePtr.ofNativeInt<'t> buffer

let readPeb (processHandle: IntPtr) =
    let basicInfo = getProcessBasicInfo processHandle
    read<PEB> processHandle basicInfo.PebBaseAddress

let splitStrings (buffer:nativeptr<byte>) (size:int) =   
    let rec strings (start: nativeptr<char>) (stop:nativeptr<char>) =
        seq {
            if start = stop then
                ()
            else
                let str = String(start)
                if str <> "" then
                    yield str
                    let next = NativePtr.add start (str.Length + 1)
                    yield! strings next stop
        }

    let charsStart = buffer |> NativePtr.toNativeInt |> NativePtr.ofNativeInt<char>
    let charsStop = NativePtr.add buffer size |> NativePtr.toNativeInt |> NativePtr.ofNativeInt<char>

    strings charsStart charsStop      

let asEnvVariable (s:string) =
    let parts = s.Split([| '=' |], 2)
    (parts.[0], parts.[1])

let readEnvironmentBlock (processHandle: IntPtr) =
    let peb = readPeb processHandle
    
    let environmentBlockStart = readField<RTLUserProcessParameters, nativeptr<byte>> "Environment" processHandle peb.ProcessParameters
    let environmentBlockSize = int (readField<RTLUserProcessParameters, UInt64> "EnvironmentSize" processHandle peb.ProcessParameters)

    let environmentBlock = readRange<byte> processHandle environmentBlockStart environmentBlockSize

    let result = splitStrings environmentBlock environmentBlockSize
                    |> Seq.map asEnvVariable
                    |> Map.ofSeq

    Marshal.FreeHGlobal(NativePtr.toNativeInt environmentBlock)

    result

