module Pather.SystemSettings

open Microsoft.Win32

open Pather

let private readSet key name =
    let value = Registry.GetValue(key, name, "").ToString()
    value |> PathSet.fromEnvVar

let getUserPath () =
    readSet  @"HKEY_CURRENT_USER\Environment" "PATH"

let getSystemPath () =
    readSet @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Environment" "PATH"
