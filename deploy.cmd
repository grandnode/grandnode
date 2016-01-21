@if "%SCM_TRACE_LEVEL%" NEQ "4" @echo off

:: ----------------------
:: KUDU Deployment Script
:: Version: 1.0.6
:: ----------------------

:: Prerequisites
:: -------------

:: Verify node.js installed
where node 2>nul >nul
IF %ERRORLEVEL% NEQ 0 (
  echo Missing node.js executable, please install node.js, if already installed make sure it can be reached from current environment.
  goto error
)

:: Setup
:: -----

setlocal enabledelayedexpansion

SET ARTIFACTS=%~dp0%..\artifacts

IF NOT DEFINED DEPLOYMENT_SOURCE (
  SET DEPLOYMENT_SOURCE=%~dp0%.
)

IF NOT DEFINED DEPLOYMENT_TARGET (
  SET DEPLOYMENT_TARGET=%ARTIFACTS%\wwwroot
)

IF NOT DEFINED NEXT_MANIFEST_PATH (
  SET NEXT_MANIFEST_PATH=%ARTIFACTS%\manifest

  IF NOT DEFINED PREVIOUS_MANIFEST_PATH (
    SET PREVIOUS_MANIFEST_PATH=%ARTIFACTS%\manifest
  )
)

IF NOT DEFINED KUDU_SYNC_CMD (
  :: Install kudu sync
  echo Installing Kudu Sync
  call npm install kudusync -g --silent
  IF !ERRORLEVEL! NEQ 0 goto error

  :: Locally just running "kuduSync" would also work
  SET KUDU_SYNC_CMD=%appdata%\npm\kuduSync.cmd
)
IF NOT DEFINED DEPLOYMENT_TEMP (
  SET DEPLOYMENT_TEMP=%temp%\___deployTemp%random%
  SET CLEAN_LOCAL_DEPLOYMENT_TEMP=true
)

IF DEFINED CLEAN_LOCAL_DEPLOYMENT_TEMP (
  IF EXIST "%DEPLOYMENT_TEMP%" rd /s /q "%DEPLOYMENT_TEMP%"
  mkdir "%DEPLOYMENT_TEMP%"
)

IF DEFINED MSBUILD_PATH goto MsbuildPathDefined
SET MSBUILD_PATH=%ProgramFiles(x86)%\MSBuild\14.0\Bin\MSBuild.exe
:MsbuildPathDefined

::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
:: Deployment
:: ----------

echo Handling .NET Web Application deployment.

:: 1. Restore NuGet packages
call :ExecuteCmd "%MSBUILD_PATH%" "%DEPLOYMENT_SOURCE%\Plugins\Nop.Plugin.DiscountRules.CustomerRoles\Nop.Plugin.DiscountRules.CustomerRoles.csproj" /nologo /verbosity:m /t:Build /t:pipelinePreDeployCopyAllFilesToOneFolder /p:_PackageTempDir="%DEPLOYMENT_TEMP%"; /p:AutoParameterizationWebConfigConnectionStrings=false;Configuration=Release /p:SolutionDir="%DEPLOYMENT_SOURCE%\.\\" %SCM_BUILD_ARGS%
call :ExecuteCmd "%MSBUILD_PATH%" "%DEPLOYMENT_SOURCE%\Plugins\Nop.Plugin.DiscountRules.HadSpentAmount\Nop.Plugin.DiscountRules.HadSpentAmount.csproj" /nologo /verbosity:m /t:Build /t:pipelinePreDeployCopyAllFilesToOneFolder /p:_PackageTempDir="%DEPLOYMENT_TEMP%";/p:AutoParameterizationWebConfigConnectionStrings=false;Configuration=Release /p:SolutionDir="%DEPLOYMENT_SOURCE%\.\\" %SCM_BUILD_ARGS%
call :ExecuteCmd "%MSBUILD_PATH%" "%DEPLOYMENT_SOURCE%\Plugins\Nop.Plugin.DiscountRules.HasAllProducts\Nop.Plugin.DiscountRules.HasAllProducts.csproj" /nologo /verbosity:m /t:Build /t:pipelinePreDeployCopyAllFilesToOneFolder /p:_PackageTempDir="%DEPLOYMENT_TEMP%";/p:AutoParameterizationWebConfigConnectionStrings=false;Configuration=Release /p:SolutionDir="%DEPLOYMENT_SOURCE%\.\\" %SCM_BUILD_ARGS%
call :ExecuteCmd "%MSBUILD_PATH%" "%DEPLOYMENT_SOURCE%\Plugins\Nop.Plugin.DiscountRules.HasOneProduct\Nop.Plugin.DiscountRules.HasOneProduct.csproj" /nologo /verbosity:m /t:Build /t:pipelinePreDeployCopyAllFilesToOneFolder /p:_PackageTempDir="%DEPLOYMENT_TEMP%";/p:AutoParameterizationWebConfigConnectionStrings=false;Configuration=Release /p:SolutionDir="%DEPLOYMENT_SOURCE%\.\\" %SCM_BUILD_ARGS%
call :ExecuteCmd "%MSBUILD_PATH%" "%DEPLOYMENT_SOURCE%\Plugins\Nop.Plugin.ExchangeRate.McExchange\Nop.Plugin.ExchangeRate.McExchange.csproj" /nologo /verbosity:m /t:Build /t:pipelinePreDeployCopyAllFilesToOneFolder /p:_PackageTempDir="%DEPLOYMENT_TEMP%";/p:AutoParameterizationWebConfigConnectionStrings=false;Configuration=Release /p:SolutionDir="%DEPLOYMENT_SOURCE%\.\\" %SCM_BUILD_ARGS%
call :ExecuteCmd "%MSBUILD_PATH%" "%DEPLOYMENT_SOURCE%\Plugins\Nop.Plugin.ExternalAuth.Facebook\Nop.Plugin.ExternalAuth.Facebook.csproj" /nologo /verbosity:m /t:Build /t:pipelinePreDeployCopyAllFilesToOneFolder /p:_PackageTempDir="%DEPLOYMENT_TEMP%";/p:AutoParameterizationWebConfigConnectionStrings=false;Configuration=Release /p:SolutionDir="%DEPLOYMENT_SOURCE%\.\\" %SCM_BUILD_ARGS%
call :ExecuteCmd "%MSBUILD_PATH%" "%DEPLOYMENT_SOURCE%\Plugins\Nop.Plugin.Feed.Froogle\Nop.Plugin.Feed.Froogle.csproj" /nologo /verbosity:m /t:Build /t:pipelinePreDeployCopyAllFilesToOneFolder /p:_PackageTempDir="%DEPLOYMENT_TEMP%";/p:AutoParameterizationWebConfigConnectionStrings=false;Configuration=Release /p:SolutionDir="%DEPLOYMENT_SOURCE%\.\\" %SCM_BUILD_ARGS%
call :ExecuteCmd "%MSBUILD_PATH%" "%DEPLOYMENT_SOURCE%\Plugins\Nop.Plugin.Misc.FacebookShop\Nop.Plugin.Misc.FacebookShop.csproj" /nologo /verbosity:m /t:Build /t:pipelinePreDeployCopyAllFilesToOneFolder /p:_PackageTempDir="%DEPLOYMENT_TEMP%";/p:AutoParameterizationWebConfigConnectionStrings=false;Configuration=Release /p:SolutionDir="%DEPLOYMENT_SOURCE%\.\\" %SCM_BUILD_ARGS%
call :ExecuteCmd "%MSBUILD_PATH%" "%DEPLOYMENT_SOURCE%\Plugins\Nop.Plugin.Payments.AuthorizeNet\Nop.Plugin.Payments.AuthorizeNet.csproj" /nologo /verbosity:m /t:Build /t:pipelinePreDeployCopyAllFilesToOneFolder /p:_PackageTempDir="%DEPLOYMENT_TEMP%";/p:AutoParameterizationWebConfigConnectionStrings=false;Configuration=Release /p:SolutionDir="%DEPLOYMENT_SOURCE%\.\\" %SCM_BUILD_ARGS%
call :ExecuteCmd "%MSBUILD_PATH%" "%DEPLOYMENT_SOURCE%\Plugins\Nop.Plugin.Payments.CheckMoneyOrder\Nop.Plugin.Payments.CheckMoneyOrder.csproj" /nologo /verbosity:m /t:Build /t:pipelinePreDeployCopyAllFilesToOneFolder /p:_PackageTempDir="%DEPLOYMENT_TEMP%";/p:AutoParameterizationWebConfigConnectionStrings=false;Configuration=Release /p:SolutionDir="%DEPLOYMENT_SOURCE%\.\\" %SCM_BUILD_ARGS%
call :ExecuteCmd "%MSBUILD_PATH%" "%DEPLOYMENT_SOURCE%\Plugins\Nop.Plugin.Payments.Manual\Nop.Plugin.Payments.Manual.csproj" /nologo /verbosity:m /t:Build /t:pipelinePreDeployCopyAllFilesToOneFolder /p:_PackageTempDir="%DEPLOYMENT_TEMP%";/p:AutoParameterizationWebConfigConnectionStrings=false;Configuration=Release /p:SolutionDir="%DEPLOYMENT_SOURCE%\.\\" %SCM_BUILD_ARGS%
call :ExecuteCmd "%MSBUILD_PATH%" "%DEPLOYMENT_SOURCE%\Plugins\Nop.Plugin.Payments.PayPalDirect\Nop.Plugin.Payments.PayPalDirect.csproj" /nologo /verbosity:m /t:Build /t:pipelinePreDeployCopyAllFilesToOneFolder /p:_PackageTempDir="%DEPLOYMENT_TEMP%";/p:AutoParameterizationWebConfigConnectionStrings=false;Configuration=Release /p:SolutionDir="%DEPLOYMENT_SOURCE%\.\\" %SCM_BUILD_ARGS%
call :ExecuteCmd "%MSBUILD_PATH%" "%DEPLOYMENT_SOURCE%\Plugins\Nop.Plugin.Payments.PayPalStandard\Nop.Plugin.Payments.PayPalStandard.csproj" /nologo /verbosity:m /t:Build /t:pipelinePreDeployCopyAllFilesToOneFolder /p:_PackageTempDir="%DEPLOYMENT_TEMP%";/p:AutoParameterizationWebConfigConnectionStrings=false;Configuration=Release /p:SolutionDir="%DEPLOYMENT_SOURCE%\.\\" %SCM_BUILD_ARGS%
call :ExecuteCmd "%MSBUILD_PATH%" "%DEPLOYMENT_SOURCE%\Plugins\Nop.Plugin.Payments.PurchaseOrder\Nop.Plugin.Payments.PurchaseOrder.csproj" /nologo /verbosity:m /t:Build /t:pipelinePreDeployCopyAllFilesToOneFolder /p:_PackageTempDir="%DEPLOYMENT_TEMP%";/p:AutoParameterizationWebConfigConnectionStrings=false;Configuration=Release /p:SolutionDir="%DEPLOYMENT_SOURCE%\.\\" %SCM_BUILD_ARGS%
call :ExecuteCmd "%MSBUILD_PATH%" "%DEPLOYMENT_SOURCE%\Plugins\Nop.Plugin.SMS.Verizon\Nop.Plugin.SMS.Verizon.csproj" /nologo /verbosity:m /t:Build /t:pipelinePreDeployCopyAllFilesToOneFolder /p:_PackageTempDir="%DEPLOYMENT_TEMP%";/p:AutoParameterizationWebConfigConnectionStrings=false;Configuration=Release /p:SolutionDir="%DEPLOYMENT_SOURCE%\.\\" %SCM_BUILD_ARGS%
call :ExecuteCmd "%MSBUILD_PATH%" "%DEPLOYMENT_SOURCE%\Plugins\Nop.Plugin.Shipping.AustraliaPost\Nop.Plugin.Shipping.AustraliaPost.csproj" /nologo /verbosity:m /t:Build /t:pipelinePreDeployCopyAllFilesToOneFolder /p:_PackageTempDir="%DEPLOYMENT_TEMP%";/p:AutoParameterizationWebConfigConnectionStrings=false;Configuration=Release /p:SolutionDir="%DEPLOYMENT_SOURCE%\.\\" %SCM_BUILD_ARGS%
call :ExecuteCmd "%MSBUILD_PATH%" "%DEPLOYMENT_SOURCE%\Plugins\Nop.Plugin.Shipping.ByWeight\Nop.Plugin.Shipping.ByWeight.csproj" /nologo /verbosity:m /t:Build /t:pipelinePreDeployCopyAllFilesToOneFolder /p:_PackageTempDir="%DEPLOYMENT_TEMP%";/p:AutoParameterizationWebConfigConnectionStrings=false;Configuration=Release /p:SolutionDir="%DEPLOYMENT_SOURCE%\.\\" %SCM_BUILD_ARGS%
call :ExecuteCmd "%MSBUILD_PATH%" "%DEPLOYMENT_SOURCE%\Plugins\Nop.Plugin.Shipping.CanadaPost\Nop.Plugin.Shipping.CanadaPost.csproj" /nologo /verbosity:m /t:Build /t:pipelinePreDeployCopyAllFilesToOneFolder /p:_PackageTempDir="%DEPLOYMENT_TEMP%";/p:AutoParameterizationWebConfigConnectionStrings=false;Configuration=Release /p:SolutionDir="%DEPLOYMENT_SOURCE%\.\\" %SCM_BUILD_ARGS%
call :ExecuteCmd "%MSBUILD_PATH%" "%DEPLOYMENT_SOURCE%\Plugins\Nop.Plugin.Shipping.Fedex\Nop.Plugin.Shipping.Fedex.csproj" /nologo /verbosity:m /t:Build /t:pipelinePreDeployCopyAllFilesToOneFolder /p:_PackageTempDir="%DEPLOYMENT_TEMP%";/p:AutoParameterizationWebConfigConnectionStrings=false;Configuration=Release /p:SolutionDir="%DEPLOYMENT_SOURCE%\.\\" %SCM_BUILD_ARGS%
call :ExecuteCmd "%MSBUILD_PATH%" "%DEPLOYMENT_SOURCE%\Plugins\Nop.Plugin.Shipping.FixedRateShipping\Nop.Plugin.Shipping.FixedRateShipping.csproj" /nologo /verbosity:m /t:Build /t:pipelinePreDeployCopyAllFilesToOneFolder /p:_PackageTempDir="%DEPLOYMENT_TEMP%";/p:AutoParameterizationWebConfigConnectionStrings=false;Configuration=Release /p:SolutionDir="%DEPLOYMENT_SOURCE%\.\\" %SCM_BUILD_ARGS%
call :ExecuteCmd "%MSBUILD_PATH%" "%DEPLOYMENT_SOURCE%\Plugins\Nop.Plugin.Shipping.UPS\Nop.Plugin.Shipping.UPS.csproj" /nologo /verbosity:m /t:Build /t:pipelinePreDeployCopyAllFilesToOneFolder /p:_PackageTempDir="%DEPLOYMENT_TEMP%";/p:AutoParameterizationWebConfigConnectionStrings=false;Configuration=Release /p:SolutionDir="%DEPLOYMENT_SOURCE%\.\\" %SCM_BUILD_ARGS%
call :ExecuteCmd "%MSBUILD_PATH%" "%DEPLOYMENT_SOURCE%\Plugins\Nop.Plugin.Shipping.USPS\Nop.Plugin.Shipping.USPS.csproj" /nologo /verbosity:m /t:Build /t:pipelinePreDeployCopyAllFilesToOneFolder /p:_PackageTempDir="%DEPLOYMENT_TEMP%";/p:AutoParameterizationWebConfigConnectionStrings=false;Configuration=Release /p:SolutionDir="%DEPLOYMENT_SOURCE%\.\\" %SCM_BUILD_ARGS%
call :ExecuteCmd "%MSBUILD_PATH%" "%DEPLOYMENT_SOURCE%\Plugins\Nop.Plugin.Tax.CountryStateZip\Nop.Plugin.Tax.CountryStateZip.csproj" /nologo /verbosity:m /t:Build /t:pipelinePreDeployCopyAllFilesToOneFolder /p:_PackageTempDir="%DEPLOYMENT_TEMP%";/p:AutoParameterizationWebConfigConnectionStrings=false;Configuration=Release /p:SolutionDir="%DEPLOYMENT_SOURCE%\.\\" %SCM_BUILD_ARGS%
call :ExecuteCmd "%MSBUILD_PATH%" "%DEPLOYMENT_SOURCE%\Plugins\Nop.Plugin.Tax.FixedRate\Nop.Plugin.Tax.FixedRate.csproj" /nologo /verbosity:m /t:Build /t:pipelinePreDeployCopyAllFilesToOneFolder /p:_PackageTempDir="%DEPLOYMENT_TEMP%";/p:AutoParameterizationWebConfigConnectionStrings=false;Configuration=Release /p:SolutionDir="%DEPLOYMENT_SOURCE%\.\\" %SCM_BUILD_ARGS%
call :ExecuteCmd "%MSBUILD_PATH%" "%DEPLOYMENT_SOURCE%\Plugins\Nop.Plugin.Widgets.GoogleAnalytics\Nop.Plugin.Widgets.GoogleAnalytics.csproj" /nologo /verbosity:m /t:Build /t:pipelinePreDeployCopyAllFilesToOneFolder /p:_PackageTempDir="%DEPLOYMENT_TEMP%";/p:AutoParameterizationWebConfigConnectionStrings=false;Configuration=Release /p:SolutionDir="%DEPLOYMENT_SOURCE%\.\\" %SCM_BUILD_ARGS%
call :ExecuteCmd "%MSBUILD_PATH%" "%DEPLOYMENT_SOURCE%\Plugins\Nop.Plugin.Widgets.NivoSlider\Nop.Plugin.Widgets.NivoSlider.csproj" /nologo /verbosity:m /t:Build /t:pipelinePreDeployCopyAllFilesToOneFolder /p:_PackageTempDir="%DEPLOYMENT_TEMP%";/p:AutoParameterizationWebConfigConnectionStrings=false;Configuration=Release /p:SolutionDir="%DEPLOYMENT_SOURCE%\.\\" %SCM_BUILD_ARGS%

:: 2. Build to the temporary path
call :ExecuteCmd "%MSBUILD_PATH%" "%DEPLOYMENT_SOURCE%\Presentation\Nop.Web\Nop.Web.csproj" /nologo /verbosity:m /t:Build /t:pipelinePreDeployCopyAllFilesToOneFolder /p:_PackageTempDir="%DEPLOYMENT_TEMP%";AutoParameterizationWebConfigConnectionStrings=false;Configuration=Release /p:SolutionDir="%DEPLOYMENT_SOURCE%\.\\" %SCM_BUILD_ARGS%

IF !ERRORLEVEL! NEQ 0 goto error

:: 3. Build admin website
call :ExecuteCmd "%MSBUILD_PATH%" "%DEPLOYMENT_SOURCE%\Presentation\Nop.Web\Administration\Nop.Admin.csproj" /nologo /verbosity:m /t:Build /t:pipelinePreDeployCopyAllFilesToOneFolder /p:_PackageTempDir="%DEPLOYMENT_TEMP%";/p:AutoParameterizationWebConfigConnectionStrings=false;Configuration=Release /p:SolutionDir="%DEPLOYMENT_SOURCE%\.\\" %SCM_BUILD_ARGS%

::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
goto end

:: Execute command routine that will echo out when error
:ExecuteCmd
setlocal
set _CMD_=%*
call %_CMD_%
if "%ERRORLEVEL%" NEQ "0" echo Failed exitCode=%ERRORLEVEL%, command=%_CMD_%
exit /b %ERRORLEVEL%

:error
endlocal
echo An error has occurred during web site deployment.
call :exitSetErrorLevel
call :exitFromFunction 2>nul

:exitSetErrorLevel
exit /b 1

:exitFromFunction
()

:end
endlocal
echo Finished successfully.
