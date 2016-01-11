module EnvInjectionHelper.Marker

type Marker = class
    static member Assembly 
        with get() = typeof<Marker>.Assembly
end
