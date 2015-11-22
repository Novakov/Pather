module Pather.SystemSettings

open Microsoft.Win32

open Pather

let private readSet key name =
    let value = Registry.GetValue(key, name, "").ToString()
    value |> PathSet.fromEnvVar

let private writeSet key name set =
    let value = set |> PathSet.toEnvVar

    Registry.SetValue(key, name, value)

let private userKey = @"HKEY_CURRENT_USER\Environment"
let private systemKey = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Environment"

let getUserPath () =
    readSet userKey "PATH"

let getSystemPath () =
    readSet systemKey "PATH"

let setUserPath set =
    writeSet userKey "PATH" set

let setSystemPath set =
    writeSet systemKey "PATH" set