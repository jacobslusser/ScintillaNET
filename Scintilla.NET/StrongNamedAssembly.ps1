<#
MIT License

Copyright (c) 2021 Petteri Kautonen

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
#>

Write-Output "Init strong-named assembly signing..."

$output_file = "ScintillaNET\CryptEnvVar.exe"
$download_url = "https://www.vpksoft.net/toolset/CryptEnvVar.exe"

Write-Output "Download file:  $download_url ..."
(New-Object System.Net.WebClient).DownloadFile($download_url, $output_file)
Write-Output "Download done."

$output_file = "ScintillaNET\SnInstallPfx.exe"
$download_url = "https://www.vpksoft.net/toolset/SnInstallPfx.exe"

Write-Output "Download file:  $download_url ..."
(New-Object System.Net.WebClient).DownloadFile($download_url, $output_file)
Write-Output "Download done."

# application parameters..
$application = "ScintillaNET"
$environment_cryptor = "CryptEnvVar.exe"

$arguments = @("-s", $Env:SK_KEY, "-e", "SK_1;SK_2", "-f", "ScintillaNET\scintilla.net.pfx", "-w", "80", "-i", "-v")
& (-join($application, "\", $environment_cryptor)) $arguments

Write-Output "Import strong-named signing certificate..."

$certificate_import_tool = "SnInstallPfx.exe"

# register the certificate to the CI image.. (C::https://github.com/honzajscz/SnInstallPfx)
$certpw=$Env:QQ
$arguments = @("ScintillaNET\scintilla.net.pfx", $certpw)
& (-join($application, "\", $certificate_import_tool)) $arguments
Write-Output "Import done."
