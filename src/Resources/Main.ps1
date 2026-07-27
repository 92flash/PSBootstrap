using module .\PSBootstrap.dll

begin {
    # Enable Config in 'Bootstrap.xml' when using $Config. Loads and validates all the script resources. See Bootstrap.xml to change what the script validates and with what values it starts with.
    $Config = Invoke-Bootstrap
}
process {

}
end {

}