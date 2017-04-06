@ECHO OFF
:start
cls
.\packages\Machine.Specifications.Runner.Console.0.9.2\tools\mspec-clr4.exe .\GestionePrenotazioni.Tests\bin\Debug\GestionePrenotazioni.Tests.dll
pause
goto start