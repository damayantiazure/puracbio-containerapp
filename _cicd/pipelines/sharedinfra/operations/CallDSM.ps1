param(
  [parameter(Mandatory=$true, HelpMessage="Path of url for AddOn. Starts with '/api/'.")]
  [ValidateNotNullOrEmpty()]
  [string] $urlPath,
  [parameter(Mandatory=$false, HelpMessage="Resource for url. Default 'https://dsmprdfrontdoor.azurefd.net'.")]
  [string] $urlResource = "https://dsmprdfrontdoor.azurefd.net",
  [parameter(Mandatory=$false, HelpMessage="Method of URL: 'GET', 'POST', 'PUT', 'PATCH', 'DELETE'. Default 'PUT'.")]
  [string] $urlMethod = 'POST',
  [parameter(Mandatory=$false, HelpMessage="Groupname of Azure DevOps variables. Default ''.")]
  [string] $variableStartname='',
  [parameter(Mandatory=$false, HelpMessage="Body for url. Start/End with triple quote, escape each double quote.")]
  [string] $jsonBody,
  [parameter(Mandatory=$false, HelpMessage="Azure DevOps variables, comma-separated.")]
  [string] $azdoVariables,
  [parameter(Mandatory=$false, HelpMessage="Azure DevOps variables as secret, comma-separated.")]
  [string] $azdoVariablesSecret,
  [parameter(Mandatory=$false, HelpMessage="Allowed http statuscode. Default ''.")]
  [string] $allowedHttpStatusCode='',
  [parameter(Mandatory=$false, HelpMessage="Expect a correlation id returned. Default 'true'.")]
  [switch] $expectCorrelationId = $true,
  [parameter(Mandatory=$false, HelpMessage="Wait till AddOn job is executed. Default 'true'.")]
  [switch] $waitForFinished = $true,
  [parameter(Mandatory=$false, HelpMessage="Number of seconds to wait before next job status is requested. Default '15'.")]
  [int] $statusInterval= 15,
  [parameter(Mandatory=$false, HelpMessage="Number of retries. Default '9'.")]
  [int] $numberOfRetries= 9
)

# Version: 1.0.0

##
# Functions

Function GetMsToken {
    <#
      .SYNOPSIS
          Get an authentication token required for interacting with Azure 
    #>
    param(
      [parameter(Mandatory=$true, HelpMessage="A tenant id for Azure AD application.")]
      [ValidateNotNullOrEmpty()]
      [string]$TenantId,
  
      [parameter(Mandatory=$true, HelpMessage="A resource to get token for.")]
      [ValidateNotNullOrEmpty()]
      [string]$Resource,
      [parameter(Mandatory=$true, HelpMessage="A client id for Azure AD application.")]
      [ValidateNotNullOrEmpty()]
      [string]$ClientId,
  
      [parameter(Mandatory=$true, HelpMessage="A secret of the client for Azure AD application.")]
      [ValidateNotNullOrEmpty()]
      [string]$ClientSecret
    )
    $adTokenUrl = "https://login.microsoftonline.com/$TenantId/oauth2/token"
    $adBody = @{
        grant_type    = "client_credentials"
        client_id     = $ClientId
        client_secret = $ClientSecret
        resource      = $Resource
    }
    $VerbosePreference = "SilentlyContinue"
    $response = Invoke-RestMethod -Method 'Post' -Uri $adTokenUrl -ContentType "application/x-www-form-urlencoded" -Body $adBody
    $VerbosePreference = "Continue"
    return $response.access_token
}

Function GetAzDoVariables {
    param(
      [hashtable]$VarHash,
      [string]$Resource,
      [string]$CorrelationId,
      [string]$AzDoVariables,
      [bool]$IsSecret,
      [string]$VariableStartname
    )

    if("$AzDoVariables" -match "\S"){
       $urlStatus = '{0}/api/jobs/{1}/getvalues/{2}' -f $Resource,$CorrelationId,$AzDoVariables
       $getValues = Invoke-RestMethod -Method GET -Headers $headers -Uri $urlStatus

       $AzDoVariables.Split(",") | ForEach {
          if ("$($getValues.output.$_)" -match "\S") {
             $outputVar = "{0}$_" -f $variableStartname
             if ($IsSecret) {
                Write-Host "##vso[task.setvariable variable=$outputVar;issecret=true]$($getValues.output.$_)"
                $VarHash.Add("$outputVar", " ****")
             } else {
                Write-Host "##vso[task.setvariable variable=$outputVar]$($getValues.output.$_)"
                $VarHash.Add("$outputVar", $($getValues.output.$_))
             }
          } else {
             Write-Host "...because variable '$_' is empty its value is not put in Azure DevOps variable '$VariableStartname$_'."
		  }
       } 
    }

    return $VarHash
}

# End of Functions
###

$tenant_id = $env:tenantId
$client_id = $env:servicePrincipalId
$client_secret = $env:servicePrincipalKey

Write-Host "Tenant_id  is:" $tenant_id 

if ($variableStartname) {
    $variableStartname += "."
}

$accesstoken = GetMsToken -TenantId $tenant_id -Resource $urlResource -ClientId $client_id -ClientSecret $client_secret
$url= '{0}{1}' -f $urlResource,$urlPath
$headers = @{
    'Authorization' = 'Bearer ' + $accesstoken
    'Content-Type' = 'application/json'
}

Write-Host "`nnumberOfRetries: $numberOfRetries"
write-Host "azdoVariables to read: $azdoVariables"
write-Host "azdoVariablesSecret to read: $azdoVariablesSecret"

Write-Host "`n`nURL: $url"
Write-Host "METHOD: $urlMethod"

if ($jsonBody) {
    $body = $jsonBody  | ConvertFrom-Json
    Write-Host "BODY: $body`n"
} else {
    Write-Host "BODY: <no body>`n"
}

if ($expectCorrelationId) {
      
    $statusCode = 200

    $retryCount = 0
    $retryAction = $true
    $addSleep = 0
    $sleep = $statusInterval
    while ($retryCount -lt 5 -AND $retryAction) {
        $retryCount++
        $retryAction = $false

        try{
            if ($jsonBody) {
                $output = Invoke-RestMethod -Method $urlMethod -Headers $headers -Body $body -Uri $url
            } else {
                $output = Invoke-RestMethod -Method $urlMethod -Headers $headers -Uri $url
		    }
        }catch [System.Net.WebException]{
           $errRespString = $_.ErrorDetails.Message
   
           $statusCode = $_.Exception.Response.StatusCode.value__
           if ($null -ne (@('429', '503') | ? { $statusCode -match $_ })) {
                $retryAction = $true
                Write-Host "API is temporarily unavailable...(retry count: $retryCount/5)...retrying after $sleep seconds..."
                sleep $sleep 
                $addSleep += $statusInterval
                $sleep += $addSleep
           } elseif ($allowedHttpStatusCode -eq "") {
                Write-Error "API call failed with status code $statusCode. Extra information: $errRespString"
           } elseif ($allowedHttpStatusCode -ne $statusCode) {
                Write-Error "API call failed with status code $statusCode (expected status code: $allowedHttpStatusCode). Extra information: $errRespString"
           } else {
                Write-Host "API call ended with expected deviated status code $statusCode."
           }
        }
    }

    if ($allowedHttpStatusCode -ne "") {
        if ($allowedHttpStatusCode -ne $statusCode) {
            Write-Error "API call ended with unexpected status code $statusCode (expected status code: $allowedHttpStatusCode)!"
        }
    }

    if ($retryAction) {
        Write-Error "Maximum number of retries has reached! Contact the CCC Support Board if problem reoccurs."
    }

    if ($output.correlation_id -and !$output.value) {
        $outputVar = '{0}CorrelationId' -f $variableStartname
        Write-Host "##vso[task.setvariable variable=$outputVar;]$($output.correlation_id)"
        Write-Host "Correlation id '$($output.correlation_id)' has been put in Azure DevOps variable '$outputVar'."
   
        if ($waitForFinished) {
            $jobStatus = $output
            $urlStatus = '{0}/api/jobs/{1}' -f $urlResource,$output.correlation_id

            $initialWait = 10
            Write-Host "`n`nGet Job status: $urlStatus...wait $initialWait seconds..."
            sleep $initialWait

            $sleep = $statusInterval
            $retryCount = 0
            $addSleep = 0
            $totalWait = $initialWait
            while ($retryCount -lt $numberOfRetries -AND !$jobStatus.finished) {
                $retryCount++
                try{
                    $jobStatus = Invoke-RestMethod -Method GET -Headers $headers -Uri $urlStatus
                    if (!$jobStatus.finished) {
                       Write-Host "Job is not finished, yet (retry count: $retryCount/$numberOfRetries)...retrying after $sleep seconds..."

                       sleep $sleep 
                       $totalWait += $sleep
                       $addSleep += $statusInterval
                       $sleep += $addSleep
                    } elseif (!$jobStatus.successful) {
                       Write-Error "Job is finished, but execution has failed! Contact the CCC Support Board if problem reoccurs."

                    }                       
                      else {
                       Write-Host "Job is finished and successfully executed."

                       $varHash = @{}
                       $varHash = GetAzDoVariables -VarHash $varHash -AzDoVariables "$azdoVariables"       -IsSecret $false -Resource $urlResource -CorrelationId $output.correlation_id -VariableStartname $variableStartname 
                       $varHash = GetAzDoVariables -VarHash $varHash -AzDoVariables "$azdoVariablesSecret" -IsSecret $true  -Resource $urlResource -CorrelationId $output.correlation_id -VariableStartname $variableStartname 
                       $varHash | Format-Table
                    }
				} catch [System.Net.WebException]{
                    $statusCode = $_.Exception.Response.StatusCode.value__
                    if ($null -ne (@('404', '429', '503') | ? { $statusCode -match $_ })) {
                       Write-Host "Job status is not received! Returncode: $statusCode (retry count: $retryCount/$numberOfRetries)...retrying after $sleep seconds..."
                       sleep $sleep 
					}  
                    else 
                    {
                       Write-Host "(statuscode: $statusCode)..."  
                       sleep 5
					}

                    $totalWait += $sleep
                    $addSleep += $statusInterval
                    $sleep += $addSleep
				 }
                 
            }
   
            $jobStatus 
            if (!$jobStatus.finished) {
               Write-Host "Job has not finished within $totalWait seconds!"
               if ($totalWait -ge 1500) {
                  Write-Error "Contact the CCC Support Board if problem reoccurs."
               } else {
                  Write-Error "Increase the statusInterval time and/or numberOfRetries  and retry."
               }
            }
        }
   
    } else {
        $output
    }
   
} else {
    Write-Host "BODY: <no body>`n"
    Invoke-RestMethod -Method $urlMethod -Headers $headers -Uri $url
}
 
